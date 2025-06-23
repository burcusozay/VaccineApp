using MassTransit;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
    private string _bearerToken;
    private DateTime _tokenExpire;
    private readonly object _tokenLock = new object();

    public OutboxPublisherWorker(IHttpClientFactory httpClientFactory,
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
                var token = await GetBearerTokenAsync(stoppingToken);
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
                    await using (var handle = await locker.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), stoppingToken))
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
                            _logger.LogInformation($"Mesaj {msg.Id} iþleniyor.");
                            var markResponse = await client.PostAsync($"{_serviceAccount.ApiCallEndpoint}/MarkProcessed/{msg.Id}", null, stoppingToken);
                            markResponse.EnsureSuccessStatusCode();
                            _logger.LogInformation($"Mesaj {msg.Id} baþarýyla iþlendi.");
                        }
                        catch (Exception ex)
                        {
                            // Hatalý durumda logla, DB’ye iþlenmemiþ olarak býrak
                            _logger.LogError(ex, $"Outbox mesajý iþlenirken hata: {msg.Id}");
                            //await PublishFailedNotificationToRabbitAsync(msg.Id, ex.ToString());
                            await _bus.Publish(new NotificationDto
                            { 
                                Message = $"{msg.Id} id kayýt edilemedi. Hata: {ex.Message}",
                                NotificationType = EnumNotificationType.Error,
                                CreatedAt = DateTime.UtcNow
                            }, stoppingToken);
                            _logger.LogInformation("Hata bildirimi RabbitMQ'ya gönderildi: {MessageId}", msg.Id);

                        }
                    }
                }

                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                // Network veya global hata varsa bir süre bekle
                _logger.LogError(ex, "Outbox modülünde hata oluþtu");
                await _bus.Publish(new NotificationDto
                { 
                    Message = $"Outbox modülünde hata oluþtu. Hata: {ex.Message}",
                    NotificationType = EnumNotificationType.Error,
                    CreatedAt = DateTime.UtcNow
                }, stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
    //private async Task PublishFailedNotificationToRabbitAsync(long messageId, string error)
    //{
    //    IChannel? channel = null; // Kanalý dýþarýda tanýmlýyoruz ki hata durumunda Dispose edebilelim

    //    try
    //    {
    //        // RabbitMQ Connection'ý açýk mý kontrol et
    //        if (!_rabbitMqConnection.IsOpen)
    //        {
    //            _logger.LogError("RabbitMQ baðlantýsý kapalý! Bildirim gönderilemedi: {MessageId}", messageId);
    //            // Baðlantý kapalýysa daha fazla denemeden çýkabiliriz veya retry mekanizmasý uygulayabiliriz.
    //            return;
    //        }

    //        // Yeni bir kanal oluþtur
    //        channel = await _rabbitMqConnection.CreateChannelAsync();

    //        // Kuyruðu Declare et
    //        // Bu kuyruk adýný da RabbitMqOptions'tan alabiliriz, bu daha esnek olur.
    //        var notificationQueueName = _rabbitMqOptions.NotificationQueueName ?? "notification_queue"; // appsettings'ten gelmeli
    //        await channel.QueueDeclareAsync(
    //            queue: notificationQueueName,
    //            durable: false,
    //            exclusive: false,
    //            autoDelete: false
    //        );
    //        _logger.LogInformation("Kuyruk '{QueueName}' hazýrlandý.", notificationQueueName);

    //        var notification = new
    //        {
    //            MessageId = messageId,
    //            Error = error,
    //            CreatedAt = DateTime.UtcNow
    //        };
    //        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(notification));

    //        await channel.BasicPublishAsync(
    //            exchange: "",
    //            routingKey: notificationQueueName, // Kuyruk adýný routing key olarak kullanýyoruz
    //            body: body
    //        );
    //        _logger.LogInformation("Baþarýsýz bildirim RabbitMQ kuyruðuna gönderildi: MessageId={MessageId}, Error={Error}", messageId, error);
    //    }
    //    catch (Exception ex)
    //    {
    //        // RabbitMQ'ya mesaj gönderirken herhangi bir hata oluþursa logla
    //        _logger.LogError(ex, "RabbitMQ'ya bildirim gönderilirken hata oluþtu: MessageId={MessageId}, Error={Error}", messageId, error);
    //    }
    //    finally
    //    {
    //        // Kanalý her zaman dispose et
    //        // using bloðu yerine manuel dispose kullanýyoruz çünkü try-catch-finally yapýsý var.
    //        if (channel != null && channel.IsOpen)
    //        {
    //            try
    //            {
    //                await channel.CloseAsync(); // Kanalý kapat
    //            }
    //            catch (Exception closeEx)
    //            {
    //                _logger.LogError(closeEx, "RabbitMQ kanalý kapatýlýrken hata oluþtu.");
    //            }
    //        }
    //        channel?.Dispose(); // Kanalý dispose et
    //    }
    //}

    /// <summary>
    /// Bearer token'ý alýr ve cache'ler.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<string> GetBearerTokenAsync(CancellationToken stoppingToken)
    {
        const string tokenKey = "outbox_bearer_token";
        string token = await _distributedCache.GetStringAsync(tokenKey, stoppingToken);
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
        var tokenResponse = await httpClient.PostAsJsonAsync(_serviceAccount.TokenEndpoint, loginRequest, stoppingToken);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenContent = await tokenResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        token = tokenContent?.AccessToken;

        if (token == null)
            throw new Exception("Token alýnamadý.");

        var expiresIn = tokenContent.ExpirationTime - DateTime.UtcNow;// Token süresini payload'dan alabilirsin
        var cacheExpiration = expiresIn > TimeSpan.FromMinutes(1) ? expiresIn.Subtract(TimeSpan.FromMinutes(1)) : expiresIn;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheExpiration
        };   
        await _distributedCache.SetStringAsync(tokenKey, token, options , stoppingToken);

        _tokenExpire = DateTime.UtcNow.Add(cacheExpiration); 
        _bearerToken = token; // Token'ý cache'le

        return token;
    }
}
