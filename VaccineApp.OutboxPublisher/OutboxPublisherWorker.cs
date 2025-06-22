using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VaccineApp.OutboxPublisher.Options;
using VaccineApp.ViewModel.Dtos;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly ServiceAccountOptions _serviceAccount;
    private string _bearerToken;
    private DateTime _tokenExpire;
    private readonly object _tokenLock = new object();


    public OutboxPublisherWorker(IHttpClientFactory httpClientFactory, IConnectionMultiplexer redis, ILogger<OutboxPublisherWorker> logger, IDistributedCache distributedCache, IOptions<ServiceAccountOptions> serviceAccount)
    {
        _httpClientFactory = httpClientFactory;
        _redis = redis;
        _logger = logger;
        _distributedCache = distributedCache;
        _serviceAccount = serviceAccount.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var locker = new RedisDistributedSynchronizationProvider(_redis.GetDatabase());

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                var token = await GetBearerTokenAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // WebAPI'den iþlenmemiþ mesajlarý çek
                var messages = await client.GetFromJsonAsync<List<OutboxMessageDto>>($"{_serviceAccount.ApiCallEndpoint}/UnprocessedList", cancellationToken: stoppingToken);
                if (messages == null)
                {
                    await Task.Delay(2000, stoppingToken); continue;
                }

                foreach (var msg in messages)
                {
                    var lockKey = $"outbox-msg-lock:{msg.Id}";
                    await using (var handle = await locker.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(5)))
                    {
                        if (handle == null)
                            continue; // Baþka worker iþliyor

                        try
                        {
                            // Burada iþleme kodu (örn. event publish, baþka servise POST)
                            // --- Örneðin alarmý RabbitMQ'ya publish et, vs. ---
                            // Mesajý ilgili API'ye gönder (ör: WebAPI endpointine)
                            // React Client Notification Gönder
                            //var apiResponse = await client.PostAsJsonAsync(
                            //    "https://localhost:44395/api/OutboxMessage/SendWebNotification",
                            //    new
                            //    {
                            //        Id = msg.Id,
                            //        Type = msg.Type,
                            //        Payload = msg.Payload,                                    
                            //    },
                            //    stoppingToken
                            //);
                            //apiResponse.EnsureSuccessStatusCode();

                            // Ýþlenince WebAPI'ye bildir
                            var markResponse = await client.PostAsync($"{_serviceAccount.ApiCallEndpoint}/MarkProcessed/{msg.Id}", null, stoppingToken);
                            markResponse.EnsureSuccessStatusCode();
                        }
                        catch (Exception ex)
                        {
                            // Hatalý durumda logla, DB’ye iþlenmemiþ olarak býrak
                            _logger.LogError(ex, "Outbox mesajý iþlenirken hata: {MessageId}", msg.Id);
                        }
                    }
                }

                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                // Network veya global hata varsa bir süre bekle
                _logger.LogError(ex, "Outbox isteði atýlýrken hata oluþtu");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }


    private async Task<string> GetBearerTokenAsync()
    {
        const string tokenKey = "outbox_bearer_token";
        string token = await _distributedCache.GetStringAsync(tokenKey);
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }

        // Token cache kontrolü
        lock (_tokenLock)
        {
            if (!string.IsNullOrEmpty(_bearerToken) && DateTime.UtcNow < _tokenExpire)
            {
                return _bearerToken;
            }
        }

        // Token alýnmamýþ veya süresi geçmiþse tekrar iste
        using var httpClient = _httpClientFactory.CreateClient();
        var loginRequest = new
        {
            Username = _serviceAccount.Username,
            Password = _serviceAccount.Password
        };
        var tokenResponse = await httpClient.PostAsJsonAsync(_serviceAccount.TokenEndpoint, loginRequest);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenContent = await tokenResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        token = tokenContent?.AccessToken;

        if (token == null)
            throw new Exception("Token alýnamadý.");

        var expires = tokenContent.ExpirationTime; // Token süresini payload'dan alabilirsin

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        await _distributedCache.SetStringAsync(tokenKey, token, options);

        return token;
    }
}
