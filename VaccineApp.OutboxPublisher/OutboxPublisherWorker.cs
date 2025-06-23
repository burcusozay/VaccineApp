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

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly IBus _bus;
    private readonly ServiceAccountOptions _serviceAccount;

    // YAPILAN DE����KL�K: Constructor temizlendi.
    // Art�k sadece ger�ekten kullan�lan servisler enjekte ediliyor.
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
                // Her d�ng�de g�ncel ve ge�erli bir token al
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
                            _logger.LogInformation($"Mesaj {msg.Id} i�leniyor.");
                            var markResponse = await client.PostAsync($"{_serviceAccount.ApiCallEndpoint}/MarkProcessed/{msg.Id}", null, stoppingToken);
                            markResponse.EnsureSuccessStatusCode();
                            _logger.LogInformation($"Mesaj {msg.Id} ba�ar�yla i�lendi.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Outbox mesaj� i�lenirken hata: {msg.Id}");
                            await _bus.Publish(new NotificationDto
                            {
                                Message = $"Mesaj ID {msg.Id} i�lenemedi. Hata: {ex.Message}",
                                NotificationType = EnumNotificationType.Error,
                                CreatedAt = DateTime.UtcNow
                            }, stoppingToken);
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError(httpEx, "Yetkilendirme hatas� (401 Unauthorized) al�nd�. Token ge�ersiz veya s�resi dolmu� olabilir. Cache temizleniyor...");
                // Token ge�ersizse cache'i temizle ki bir sonraki denemede yeni token al�ns�n.
                await _distributedCache.RemoveAsync("outbox_bearer_token", stoppingToken);
                await Task.Delay(1000, stoppingToken); // K�sa bir s�re bekle
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox mod�l�nde genel bir hata olu�tu.");
                await _bus.Publish(new NotificationDto
                {
                    Message = $"Outbox mod�l�nde genel bir hata olu�tu. Hata: {ex.Message}",
                    NotificationType = EnumNotificationType.Error,
                    CreatedAt = DateTime.UtcNow
                }, stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Sadece bu s�n�f i�inde token ve son kullanma tarihini bir arada tutmak i�in kullan�l�r.
    /// </summary>
    private class CachedTokenInfo
    {
        public string AccessToken { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

    /// <summary>
    /// Bearer token'� al�r. Token'� �ncelikle Redis cache'den okumaya �al���r.
    /// Cache'de yoksa veya ge�ersizse, login endpoint'inden yenisini al�r ve cache'ler.
    /// </summary>
    private async Task<string> GetBearerTokenAsync(CancellationToken stoppingToken)
    {
        const string tokenKey = "outbox_bearer_token";

        // 1. Token'� ve son kullanma tarihini Redis'ten almay� dene.
        var cachedTokenJson = await _distributedCache.GetStringAsync(tokenKey, stoppingToken);
        if (!string.IsNullOrEmpty(cachedTokenJson))
        {
            try
            {
                var cachedToken = JsonSerializer.Deserialize<LoginResponseDto>(cachedTokenJson);

                // YEN� EKLENEN KONTROL: Token'�n s�resinin dolup dolmad���n� kontrol et.
                // Token'�n s�resinin dolmas�na 15 saniyeden fazla varsa ge�erli say.
                if (cachedToken != null && cachedToken.ExpirationTime > DateTime.UtcNow.AddSeconds(15))
                {
                    _logger.LogInformation("Bearer token Redis cache'den ba�ar�yla al�nd� ve ge�erli.");
                    return cachedToken.AccessToken;
                }
                _logger.LogWarning("Redis cache'de bulunan token'�n s�resi dolmu� veya dolmak �zere.");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Redis'teki token verisi JSON format�nda de�il. Cache temizleniyor.");
                await _distributedCache.RemoveAsync(tokenKey, stoppingToken); // Bozuk veriyi temizle
            }
        }

        // 2. Cache'de yoksa veya s�resi dolmu�sa, API'den yeni token al.
        _logger.LogInformation("API'den yeni token isteniyor...");
        using var httpClient = _httpClientFactory.CreateClient();
        var loginRequest = new { Username = _serviceAccount.Username, Password = _serviceAccount.Password };

        var tokenResponse = await httpClient.PostAsJsonAsync(_serviceAccount.TokenEndpoint, loginRequest, stoppingToken);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenContent = await tokenResponse.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: stoppingToken);
        var newToken = tokenContent?.AccessToken;
        var newExpirationTime = tokenContent.ExpirationTime;

        if (string.IsNullOrEmpty(newToken))
            throw new InvalidOperationException("Kimlik do�rulama servisinden bo� token d�nd�.");

        // 3. Yeni token'� ve son kullanma tarihini bir nesne olarak Redis'e kaydet.
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

        _logger.LogInformation("Yeni token al�nd� ve {Minutes:F1} dakika i�in Redis'e kaydedildi.", cacheExpiration.TotalMinutes);

        return newToken;
    }
}
