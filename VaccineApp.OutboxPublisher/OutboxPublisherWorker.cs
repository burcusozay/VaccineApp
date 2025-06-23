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

                // WebAPI'den i�lenmemi� mesajlar� �ek
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
                            continue; // Ba�ka worker i�liyor

                        try
                        {
                            // Burada i�leme kodu (�rn. event publish, ba�ka servise POST)
                            // --- �rne�in alarm� RabbitMQ'ya publish et, vs. ---
                            // Mesaj� ilgili API'ye g�nder (�r: WebAPI endpointine)
                            // React Client Notification G�nder
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

                            // ��lenince WebAPI'ye bildir
                            _logger.LogInformation($"Mesaj {msg.Id} i�leniyor.");
                            var markResponse = await client.PostAsync($"{_serviceAccount.ApiCallEndpoint}/MarkProcessed/{msg.Id}", null, stoppingToken);
                            markResponse.EnsureSuccessStatusCode();
                            _logger.LogInformation($"Mesaj {msg.Id} ba�ar�yla i�lendi.");
                        }
                        catch (Exception ex)
                        {
                            // Hatal� durumda logla, DB�ye i�lenmemi� olarak b�rak
                            _logger.LogError(ex, $"Outbox mesaj� i�lenirken hata: {msg.Id}");
                            //await PublishFailedNotificationToRabbitAsync(msg.Id, ex.ToString());
                            await _bus.Publish(new NotificationDto
                            { 
                                Message = $"{msg.Id} id kay�t edilemedi. Hata: {ex.Message}",
                                NotificationType = EnumNotificationType.Error,
                                CreatedAt = DateTime.UtcNow
                            }, stoppingToken);
                            _logger.LogInformation("Hata bildirimi RabbitMQ'ya g�nderildi: {MessageId}", msg.Id);

                        }
                    }
                }

                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                // Network veya global hata varsa bir s�re bekle
                _logger.LogError(ex, "Outbox mod�l�nde hata olu�tu");
                await _bus.Publish(new NotificationDto
                { 
                    Message = $"Outbox mod�l�nde hata olu�tu. Hata: {ex.Message}",
                    NotificationType = EnumNotificationType.Error,
                    CreatedAt = DateTime.UtcNow
                }, stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
    //private async Task PublishFailedNotificationToRabbitAsync(long messageId, string error)
    //{
    //    IChannel? channel = null; // Kanal� d��ar�da tan�ml�yoruz ki hata durumunda Dispose edebilelim

    //    try
    //    {
    //        // RabbitMQ Connection'� a��k m� kontrol et
    //        if (!_rabbitMqConnection.IsOpen)
    //        {
    //            _logger.LogError("RabbitMQ ba�lant�s� kapal�! Bildirim g�nderilemedi: {MessageId}", messageId);
    //            // Ba�lant� kapal�ysa daha fazla denemeden ��kabiliriz veya retry mekanizmas� uygulayabiliriz.
    //            return;
    //        }

    //        // Yeni bir kanal olu�tur
    //        channel = await _rabbitMqConnection.CreateChannelAsync();

    //        // Kuyru�u Declare et
    //        // Bu kuyruk ad�n� da RabbitMqOptions'tan alabiliriz, bu daha esnek olur.
    //        var notificationQueueName = _rabbitMqOptions.NotificationQueueName ?? "notification_queue"; // appsettings'ten gelmeli
    //        await channel.QueueDeclareAsync(
    //            queue: notificationQueueName,
    //            durable: false,
    //            exclusive: false,
    //            autoDelete: false
    //        );
    //        _logger.LogInformation("Kuyruk '{QueueName}' haz�rland�.", notificationQueueName);

    //        var notification = new
    //        {
    //            MessageId = messageId,
    //            Error = error,
    //            CreatedAt = DateTime.UtcNow
    //        };
    //        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(notification));

    //        await channel.BasicPublishAsync(
    //            exchange: "",
    //            routingKey: notificationQueueName, // Kuyruk ad�n� routing key olarak kullan�yoruz
    //            body: body
    //        );
    //        _logger.LogInformation("Ba�ar�s�z bildirim RabbitMQ kuyru�una g�nderildi: MessageId={MessageId}, Error={Error}", messageId, error);
    //    }
    //    catch (Exception ex)
    //    {
    //        // RabbitMQ'ya mesaj g�nderirken herhangi bir hata olu�ursa logla
    //        _logger.LogError(ex, "RabbitMQ'ya bildirim g�nderilirken hata olu�tu: MessageId={MessageId}, Error={Error}", messageId, error);
    //    }
    //    finally
    //    {
    //        // Kanal� her zaman dispose et
    //        // using blo�u yerine manuel dispose kullan�yoruz ��nk� try-catch-finally yap�s� var.
    //        if (channel != null && channel.IsOpen)
    //        {
    //            try
    //            {
    //                await channel.CloseAsync(); // Kanal� kapat
    //            }
    //            catch (Exception closeEx)
    //            {
    //                _logger.LogError(closeEx, "RabbitMQ kanal� kapat�l�rken hata olu�tu.");
    //            }
    //        }
    //        channel?.Dispose(); // Kanal� dispose et
    //    }
    //}

    /// <summary>
    /// Bearer token'� al�r ve cache'ler.
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

        // Token cache kontrol�
        lock (_tokenLock)
        {
            if (!string.IsNullOrEmpty(_bearerToken) && DateTime.UtcNow < _tokenExpire)
            {
                return _bearerToken;
            }
        }

        // Token al�nmam�� veya s�resi ge�mi�se tekrar iste
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
            throw new Exception("Token al�namad�.");

        var expiresIn = tokenContent.ExpirationTime - DateTime.UtcNow;// Token s�resini payload'dan alabilirsin
        var cacheExpiration = expiresIn > TimeSpan.FromMinutes(1) ? expiresIn.Subtract(TimeSpan.FromMinutes(1)) : expiresIn;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheExpiration
        };   
        await _distributedCache.SetStringAsync(tokenKey, token, options , stoppingToken);

        _tokenExpire = DateTime.UtcNow.Add(cacheExpiration); 
        _bearerToken = token; // Token'� cache'le

        return token;
    }
}
