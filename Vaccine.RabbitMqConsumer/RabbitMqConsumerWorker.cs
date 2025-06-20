using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using VaccineApp.Core.Core;
using VaccineApp.RabbitMqConsumer.Options;

namespace VaccineApp.RabbitMqConsumer
{
    public class RabbitMqConsumerWorker : BackgroundService
    {
        private readonly ILogger<RabbitMqConsumerWorker> _logger;
        private readonly string _pgConnStr;
        private Dictionary<int, int> consecutiveOverFive;

        public RabbitMqConsumerWorker(ILogger<RabbitMqConsumerWorker> logger, IOptions<PostgreSqlOptions> pgOptions)
        {
            _logger = logger;
            _pgConnStr = pgOptions.Value.PostgreSqlConnection;
            consecutiveOverFive = new Dictionary<int, int>();
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

                // PostgreSQL insert
                //using var pgConn = new NpgsqlConnection(_pgConnStr);
                //await pgConn.OpenAsync();
                //using var cmd = new NpgsqlCommand("INSERT INTO sensor_data(sensor_id, value) VALUES(@sensor_id, @value)", pgConn);
                //cmd.Parameters.AddWithValue("sensor_id", sensorData.id);
                //cmd.Parameters.AddWithValue("value", sensorData.value);
                //await cmd.ExecuteNonQueryAsync();

                // Alarm logic
                if (queeData.Value > 5)
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
