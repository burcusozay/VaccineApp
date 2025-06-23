using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using VaccineApp.SoketListener.Options;
using VaccineApp.ViewModel.Worker;

namespace VaccineApp.SoketListener
{
    public class SoketListenerWorker : BackgroundService
    {
        private readonly ILogger<SoketListenerWorker> _logger;
        private readonly RabbitMqOptions _rabbitMqOptions;
        private readonly IConnection _rabbitMqConnection; // IConnection'� enjekte ediyoruz 

        public SoketListenerWorker(ILogger<SoketListenerWorker> logger, IOptions<RabbitMqOptions> rabbitMqOptions, IConnection rabbitMqConnection)
        {
            _logger = logger;
            _rabbitMqConnection = rabbitMqConnection;
            _rabbitMqOptions = rabbitMqOptions.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Kanal� her d�ng�de veya hata durumunda yeniden olu�turaca��z
            IChannel? channel = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                // Ba�lant�n�n a��k oldu�undan emin ol (re-connection logic)
                if (!_rabbitMqConnection.IsOpen)
                {
                    _logger.LogWarning("RabbitMQ ba�lant�s� kapal�. Yeniden ba�lan�l�yor...");
                    // Burada asl�nda IConnection'�n yeniden olu�turulmas� gerekir.
                    // Singleton olarak kaydedildi�i i�in bu, Program.cs'teki hata y�netiminde ele al�n�r.
                    // E�er ba�lant� kesilirse ve tekrar kurulmazsa, buras� sonsuz d�ng�ye girer.
                    // Ger�ek bir uygulamada, IConnection'� do�rudan enjekte etmek yerine,
                    // ba�lant�y� y�neten bir RabbitMqClientWrapper s�n�f� enjekte etmek daha sa�lam olabilir.
                    await Task.Delay(5000, stoppingToken); // Ba�lant�n�n kurulmas�n� bekle
                    continue; // Bir sonraki d�ng�de tekrar dene
                }

                try
                {
                    // Kanal� burada olu�turmak veya zaten varsa kullanmak
                    // Her seferinde yeni bir kanal olu�turmak, kanal ba��ms�zl��� sa�lar.
                    // Ancak ayn� kanal�n defalarca a��l�p kapat�lmas� overhead yaratabilir.
                    // Daha stabil bir yakla��m: kanal�n ExecuteAsync'te bir kez olu�turulmas� ve
                    // hatalarda dispose edilip yeniden olu�turulmas�.
                    if (channel == null || channel.IsClosed)
                    {
                        channel = await _rabbitMqConnection.CreateChannelAsync();
                        await channel.QueueDeclareAsync(
                            queue: "soket_data_queue",
                            durable: false,
                            exclusive: false,
                            autoDelete: false
                        );
                        _logger.LogInformation("RabbitMQ kanal� ve kuyru�u haz�r: soket_data_queue");
                    }


                    using var client = new TcpClient();
                    await client.ConnectAsync("localhost", 65432, stoppingToken);
                    _logger.LogInformation("Soket ba�lant�s� sa�land�, veri okunuyor...");

                    using var stream = client.GetStream();
                    var buffer = new byte[4096];
                    var strBuffer = new StringBuilder();

                    while (!stoppingToken.IsCancellationRequested && client.Connected)
                    {
                        var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                        if (byteCount == 0)
                        {
                            _logger.LogInformation("Soket ba�lant�s� kapand�. Yeniden ba�lan�l�yor...");
                            break; // Ba�lant� koptuysa d�ng�den ��k ve tekrar dene
                        }

                        strBuffer.Append(Encoding.UTF8.GetString(buffer, 0, byteCount));
                        while (strBuffer.ToString().Contains('\n'))
                        {
                            var index = strBuffer.ToString().IndexOf('\n');
                            if (index == -1) break; // '\n' yoksa d�ng�den ��k

                            var line = strBuffer.ToString().Substring(0, index);
                            strBuffer.Remove(0, index + 1);

                            try
                            {
                                var msg = JsonSerializer.Deserialize<QueeData>(line);
                                if (msg == null)
                                {
                                    _logger.LogWarning("Soket mesaj� JSON deserialization sonras� bo� geldi: {Line}", line);
                                    continue;
                                }

                                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
                                await channel.BasicPublishAsync(exchange: "", routingKey: "soket_data_queue", body: body);
                                _logger.LogInformation("Mesaj RabbitMQ'ya g�nderildi: {Line}", line);
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogError(ex, "Soket mesaj� JSON deserialize edilirken hata: {Line}", line);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "RabbitMQ'ya mesaj g�nderilirken hata: {Line}", line);
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    _logger.LogError(ex, "Soket ba�lant� hatas� olu�tu: {ErrorMessage}. {Delay} saniye sonra tekrar denenecek.", ex.Message, 5);
                    // Hata durumunda kanal� kapat�p dispose etmek, bir sonraki d�ng�de yenisini olu�turmaya zorlar
                    channel?.Dispose();
                    channel = null;
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Genel hata olu�tu: {ErrorMessage}. {Delay} saniye sonra tekrar denenecek.", ex.Message, 5);
                    channel?.Dispose();
                    channel = null;
                    await Task.Delay(5000, stoppingToken);
                }
            }

            // Worker durdurulurken kanal� da dispose et
            if (channel != null && !channel.IsClosed)
            {
                await channel.CloseAsync();
                await channel.DisposeAsync();
            }
        }

        // QueeData s�n�f�n�n tan�m� eksikse, l�tfen onu da ekle.
        // �rnek olarak ekledim:
        //public record QueeData(int id, string data, DateTime timestamp);
    } 
}
