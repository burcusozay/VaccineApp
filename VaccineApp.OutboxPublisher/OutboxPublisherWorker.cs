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
                // WebAPI'den i�lenmemi� mesajlar� �ek
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
                            var markResponse = await client.PostAsync(
                                $"https://localhost:44395/api/OutboxMessage/MarkProcessed/{msg.Id}",
                                null,
                                stoppingToken
                            );
                            markResponse.EnsureSuccessStatusCode();
                        }
                        catch (Exception ex)
                        {
                            // Hatal� durumda logla, DB�ye i�lenmemi� olarak b�rak
                        }
                    }
                }

                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                // Network veya global hata varsa bir s�re bekle
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
