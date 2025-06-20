using RabbitMQ.Client;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using VaccineApp.Core.Core;

namespace VaccineApp.SoketListener
{
    public class SoketListenerWorker : BackgroundService
    {
        private readonly ILogger<SoketListenerWorker> _logger;

        public SoketListenerWorker(ILogger<SoketListenerWorker> logger)
        {
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            //// RabbitMQ setup ... 
            var factory = new RabbitMQ.Client.ConnectionFactory() { HostName = "localhost", UserName = "admin" ,Password = "123456" };
            using var rabbitConn = await factory.CreateConnectionAsync();
            using var channel = await rabbitConn.CreateChannelAsync(); // connection.CreateModel();
            await channel.QueueDeclareAsync(queue: "soket_data_queue", durable: false, exclusive: false, autoDelete: false);
             
            while (!stoppingToken.IsCancellationRequested)
            {
                using var client = new TcpClient();
                try
                {
                    await client.ConnectAsync("localhost", 65432, stoppingToken);
                    Console.WriteLine("Baðlantý saðlandý, veri okunuyor...");
                    using var stream = client.GetStream();

                    var buffer = new byte[4096];
                    var strBuffer = new StringBuilder();

                    while (!stoppingToken.IsCancellationRequested && client.Connected)
                    {
                        var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                        if (byteCount == 0)
                        {
                            // Baðlantý koptuysa döngüden çýk ve tekrar dene
                            break;
                        }

                        strBuffer.Append(Encoding.UTF8.GetString(buffer, 0, byteCount));
                        while (strBuffer.ToString().Contains('\n'))
                        {
                            var index = strBuffer.ToString().IndexOf('\n');
                            var line = strBuffer.ToString().Substring(0, index);
                            strBuffer.Remove(0, index + 1);

                            try
                            {
                                var msg = JsonSerializer.Deserialize<QueeData>(line);
                                // ... RabbitMQ publish iþlemi ...
                                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
                                await channel.BasicPublishAsync(exchange: "", routingKey: "soket_data_queue", body: body);
                                Console.WriteLine($"Sent to RabbitMQ: {line}");
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Baðlantý saðlanamadý: " + ex.Message);
                    // Hata durumunda 5 saniye bekle, tekrar dene
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }
}
