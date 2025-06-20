using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using VaccineApp.Core.Core;
using VaccineApp.RabbitMqConsumer.Options;

namespace VaccineApp.RabbitMqConsumer
{
    public class RabbitMqConsumerWorker : BackgroundService
    {
        private readonly ILogger<RabbitMqConsumerWorker> _logger; 
        private Dictionary<long, decimal> consecutiveOverFive;

        public RabbitMqConsumerWorker(ILogger<RabbitMqConsumerWorker> logger)
        {
            _logger = logger; 
            consecutiveOverFive = new Dictionary<long, decimal>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new RabbitMQ.Client.ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "123456" };
            using var conn = await factory.CreateConnectionAsync();
            using var channel = await conn.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "soket_data_queue", durable: false, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var queeData = JsonSerializer.Deserialize<QueeData>(message);
                               
                // Alarm logic
                if (queeData != null && queeData.Value > 5)
                {
                    if (!consecutiveOverFive.ContainsKey(queeData.Id))
                    {
                        consecutiveOverFive.Add(queeData.Id, 1);
                    }
                    else
                    {
                        consecutiveOverFive[queeData.Id]++;

                        if (consecutiveOverFive[queeData.Id] >= 5)
                        {

                            // ALARM DURUMUNDA WEB API'YE HTTP POST AT
                            using var httpClient = new HttpClient();
                            var freezerTemp = new
                            {
                                FreezerId = queeData.Id,
                                Temperature = queeData.Value,
                                CreatedDate = DateTime.UtcNow
                            };

                            // API adresini kendine göre deðiþtir!
                            var response = await httpClient.PostAsJsonAsync("https://localhost:44395/api/FreezerTemperature", freezerTemp);
                            response.EnsureSuccessStatusCode();
 
                            Console.WriteLine($"alarm id: {queeData.Id}, count:{consecutiveOverFive[queeData.Id]}");
                        }
                    }
                }
            };

            await channel.BasicConsumeAsync(queue: "soket_data_queue", autoAck: true, consumer: consumer);

            // Sonsuz bekleme
            //return Task.Delay(-1, stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken); // Hata veren satýrýn düzeltilmiþ hali 
        }
    }
}
