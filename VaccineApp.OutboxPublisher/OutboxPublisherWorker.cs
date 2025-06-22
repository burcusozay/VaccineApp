using Medallion.Threading;
using Medallion.Threading.Redis;
using StackExchange.Redis;
using System.Net.Http.Json;
using VaccineApp.ViewModel.Dtos;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redis;

    public OutboxPublisherWorker(IHttpClientFactory httpClientFactory, IConnectionMultiplexer redis)
    {
        _httpClientFactory = httpClientFactory;
        _redis = redis;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var locker = new RedisDistributedSynchronizationProvider(_redis.GetDatabase());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // WebAPI'den iþlenmemiþ mesajlarý çek
                var messages = await client.GetFromJsonAsync<List<OutboxMessageDto>>(
                    "https://localhost:44395/api/OutboxMessage/UnprocessedList",
                    cancellationToken: stoppingToken
                );

                if (messages == null) { await Task.Delay(2000, stoppingToken); continue; }

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
                            var markResponse = await client.PostAsync(
                                $"https://localhost:44395/api/OutboxMessage/MarkProcessed/{msg.Id}",
                                null,
                                stoppingToken
                            );
                            markResponse.EnsureSuccessStatusCode();
                        }
                        catch (Exception ex)
                        {
                            // Hatalý durumda logla, DB’ye iþlenmemiþ olarak býrak
                        }
                    }
                }

                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                // Network veya global hata varsa bir süre bekle
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
