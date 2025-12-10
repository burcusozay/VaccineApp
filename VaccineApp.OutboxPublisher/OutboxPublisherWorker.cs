using MassTransit;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using VaccineApp.OutboxPublisher.Options;
using VaccineApp.ViewModel.Dtos;
using VaccineApp.ViewModel.Enums;

// Bu servis veritabanýnda 5 kez baþarýsýz kayýt olmasý halinde api url e bilgilendirme gönderir. Notification iþlemi yapar
public class OutboxPublisherWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly IBus _bus;
    private readonly ServiceAccountOptions _serviceAccount;

    // YAPILAN DEÐÝÞÝKLÝK: Constructor temizlendi.
    // Artýk sadece gerçekten kullanýlan servisler enjekte ediliyor.
    public OutboxPublisherWorker(
        IHttpClientFactory httpClientFactory,
        IConnectionMultiplexer redis,
        ILogger<OutboxPublisherWorker> logger,
        IDistributedCache distributedCache,
        IOptions<ServiceAccountOptions> serviceAccount,
        IBus bus)
    {
        _httpClientFactory = httpClientFactory;
        _redis = redis;
        _logger = logger;
        _distributedCache = distributedCache;
        _serviceAccount = serviceAccount.Value;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var locker = new RedisDistributedSynchronizationProvider(_redis.GetDatabase());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Her döngüde güncel ve geçerli bir token al
                var token = await GetBearerTokenAsync(stoppingToken);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var messages = await client.GetFromJsonAsync<List<OutboxMessageDto>>($"{_serviceAccount.ApiCallEndpoint}/UnprocessedList", cancellationToken: stoppingToken);

                if (messages == null || !messages.Any())
                {
                    await Task.Delay(2000, stoppingToken);
                    continue;
                }

                foreach (var msg in messages)
                {
                    var lockKey = $"outbox-msg-lock:{msg.Id}";
                    await using (var handle = await locker.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), stoppingToken))
                    {
                        if (handle == null) continue;

                        try
                        {
                            _logger.LogInformation($"Mesaj {msg.Id} iþleniyor.");
                            var markResponse = await client.PostAsync($"{_serviceAccount.ApiCallEndpoint}/MarkProcessed/{msg.Id}", null, stoppingToken);
                            markResponse.EnsureSuccessStatusCode();
                            _logger.LogInformation($"Mesaj {msg.Id} baþarýyla iþlendi.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Outbox mesajý iþlenirken hata: {msg.Id}");
                            await _bus.Publish(new NotificationDto
                            {
                                Message = $"Mesaj ID {msg.Id} iþlenemedi. Hata: {ex.Message}",
                                NotificationType = EnumNotificationType.Error,
                                CreatedAt = DateTime.UtcNow
                            }, stoppingToken);
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError(httpEx, "Yetkilendirme hatasý (401 Unauthorized) alýndý. Token geçersiz veya süresi dolmuþ olabilir. Cache temizleniyor...");
                // Token geçersizse cache'i temizle ki bir sonraki denemede yeni token alýnsýn.
                await _distributedCache.RemoveAsync("outbox_bearer_token", stoppingToken);
                await Task.Delay(1000, stoppingToken); // Kýsa bir süre bekle
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox modülünde genel bir hata oluþtu.");
                await _bus.Publish(new NotificationDto
                {
                    Message = $"Outbox modülünde genel bir hata oluþtu. Hata: {ex.Message}",
                    NotificationType = EnumNotificationType.Error,
                    CreatedAt = DateTime.UtcNow
                }, stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Sadece bu sýnýf içinde token ve son kullanma tarihini bir arada tutmak için kullanýlýr.
    /// </summary>
    private class CachedTokenInfo
    {
        public string AccessToken { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

    /// <summary>
    /// Bearer token'ý alýr. Token'ý öncelikle Redis cache'den okumaya çalýþýr.
    /// Cache'de yoksa veya geçersizse, login endpoint'inden yenisini alýr ve cache'ler.
    /// </summary>
    private async Task<string> GetBearerTokenAsync(CancellationToken stoppingToken)
    {
        const string tokenKey = "outbox_bearer_token";

        // 1. Token'ý ve son kullanma tarihini Redis'ten almayý dene.
        var cachedTokenJson = await _distributedCache.GetStringAsync(tokenKey, stoppingToken);
        if (!string.IsNullOrEmpty(cachedTokenJson))
        {
            try
            {
                var cachedToken = JsonSerializer.Deserialize<LoginResponseDto>(cachedTokenJson);

                // YENÝ EKLENEN KONTROL: Token'ýn süresinin dolup dolmadýðýný kontrol et.
                // Token'ýn süresinin dolmasýna 15 saniyeden fazla varsa geçerli say.
                if (cachedToken != null && cachedToken.ExpirationTime > DateTime.UtcNow.AddSeconds(15))
                {
                    _logger.LogInformation("Bearer token Redis cache'den baþarýyla alýndý ve geçerli.");
                    return cachedToken.AccessToken;
                }
                _logger.LogWarning("Redis cache'de bulunan token'ýn süresi dolmuþ veya dolmak üzere.");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Redis'teki token verisi JSON formatýnda deðil. Cache temizleniyor.");
                await _distributedCache.RemoveAsync(tokenKey, stoppingToken); // Bozuk veriyi temizle
            }
        }

        // 2. Cache'de yoksa veya süresi dolmuþsa, API'den yeni token al.
        _logger.LogInformation("API'den yeni token isteniyor...");
        using var httpClient = _httpClientFactory.CreateClient();
        var loginRequest = new { Username = _serviceAccount.Username, Password = _serviceAccount.Password };

        var tokenResponse = await httpClient.PostAsJsonAsync(_serviceAccount.TokenEndpoint, loginRequest, stoppingToken);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenContent = await tokenResponse.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: stoppingToken);
        var newToken = tokenContent?.AccessToken;
        var newExpirationTime = tokenContent.ExpirationTime;

        if (string.IsNullOrEmpty(newToken))
            throw new InvalidOperationException("Kimlik doðrulama servisinden boþ token döndü.");

        // 3. Yeni token'ý ve son kullanma tarihini bir nesne olarak Redis'e kaydet.
        //var tokenToCache = new LoginResponseDto
        //{
        //    AccessToken = newToken,
        //    ExpirationTime = newExpirationTime,
        //    RefreshToken = tokenContent.RefreshToken
        //};

        var cacheExpiration = newExpirationTime - DateTime.UtcNow;

        await _distributedCache.SetStringAsync(
            tokenKey,
            JsonSerializer.Serialize(tokenContent),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheExpiration },
            stoppingToken);

        _logger.LogInformation("Yeni token alýndý ve {Minutes:F1} dakika için Redis'e kaydedildi.", cacheExpiration.TotalMinutes);

        return newToken;
    }
}
