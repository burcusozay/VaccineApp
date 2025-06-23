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
        private readonly IConnection _rabbitMqConnection; // IConnection'ý enjekte ediyoruz 

        public SoketListenerWorker(ILogger<SoketListenerWorker> logger, IOptions<RabbitMqOptions> rabbitMqOptions, IConnection rabbitMqConnection)
        {
            _logger = logger;
            _rabbitMqConnection = rabbitMqConnection;
            _rabbitMqOptions = rabbitMqOptions.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Kanalý her döngüde veya hata durumunda yeniden oluþturacaðýz
            IChannel? channel = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                // Baðlantýnýn açýk olduðundan emin ol (re-connection logic)
                if (!_rabbitMqConnection.IsOpen)
                {
                    _logger.LogWarning("RabbitMQ baðlantýsý kapalý. Yeniden baðlanýlýyor...");
                    // Burada aslýnda IConnection'ýn yeniden oluþturulmasý gerekir.
                    // Singleton olarak kaydedildiði için bu, Program.cs'teki hata yönetiminde ele alýnýr.
                    // Eðer baðlantý kesilirse ve tekrar kurulmazsa, burasý sonsuz döngüye girer.
                    // Gerçek bir uygulamada, IConnection'ý doðrudan enjekte etmek yerine,
                    // baðlantýyý yöneten bir RabbitMqClientWrapper sýnýfý enjekte etmek daha saðlam olabilir.
                    await Task.Delay(5000, stoppingToken); // Baðlantýnýn kurulmasýný bekle
                    continue; // Bir sonraki döngüde tekrar dene
                }

                try
                {
                    // Kanalý burada oluþturmak veya zaten varsa kullanmak
                    // Her seferinde yeni bir kanal oluþturmak, kanal baðýmsýzlýðý saðlar.
                    // Ancak ayný kanalýn defalarca açýlýp kapatýlmasý overhead yaratabilir.
                    // Daha stabil bir yaklaþým: kanalýn ExecuteAsync'te bir kez oluþturulmasý ve
                    // hatalarda dispose edilip yeniden oluþturulmasý.
                    if (channel == null || channel.IsClosed)
                    {
                        channel = await _rabbitMqConnection.CreateChannelAsync();
                        await channel.QueueDeclareAsync(
                            queue: "soket_data_queue",
                            durable: false,
                            exclusive: false,
                            autoDelete: false
                        );
                        _logger.LogInformation("RabbitMQ kanalý ve kuyruðu hazýr: soket_data_queue");
                    }


                    using var client = new TcpClient();
                    await client.ConnectAsync("localhost", 65432, stoppingToken);
                    _logger.LogInformation("Soket baðlantýsý saðlandý, veri okunuyor...");

                    using var stream = client.GetStream();
                    var buffer = new byte[4096];
                    var strBuffer = new StringBuilder();

                    while (!stoppingToken.IsCancellationRequested && client.Connected)
                    {
                        var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                        if (byteCount == 0)
                        {
                            _logger.LogInformation("Soket baðlantýsý kapandý. Yeniden baðlanýlýyor...");
                            break; // Baðlantý koptuysa döngüden çýk ve tekrar dene
                        }

                        strBuffer.Append(Encoding.UTF8.GetString(buffer, 0, byteCount));
                        while (strBuffer.ToString().Contains('\n'))
                        {
                            var index = strBuffer.ToString().IndexOf('\n');
                            if (index == -1) break; // '\n' yoksa döngüden çýk

                            var line = strBuffer.ToString().Substring(0, index);
                            strBuffer.Remove(0, index + 1);

                            try
                            {
                                var msg = JsonSerializer.Deserialize<QueeData>(line);
                                if (msg == null)
                                {
                                    _logger.LogWarning("Soket mesajý JSON deserialization sonrasý boþ geldi: {Line}", line);
                                    continue;
                                }

                                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
                                await channel.BasicPublishAsync(exchange: "", routingKey: "soket_data_queue", body: body);
                                _logger.LogInformation("Mesaj RabbitMQ'ya gönderildi: {Line}", line);
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogError(ex, "Soket mesajý JSON deserialize edilirken hata: {Line}", line);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "RabbitMQ'ya mesaj gönderilirken hata: {Line}", line);
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    _logger.LogError(ex, "Soket baðlantý hatasý oluþtu: {ErrorMessage}. {Delay} saniye sonra tekrar denenecek.", ex.Message, 5);
                    // Hata durumunda kanalý kapatýp dispose etmek, bir sonraki döngüde yenisini oluþturmaya zorlar
                    channel?.Dispose();
                    channel = null;
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Genel hata oluþtu: {ErrorMessage}. {Delay} saniye sonra tekrar denenecek.", ex.Message, 5);
                    channel?.Dispose();
                    channel = null;
                    await Task.Delay(5000, stoppingToken);
                }
            }

            // Worker durdurulurken kanalý da dispose et
            if (channel != null && !channel.IsClosed)
            {
                await channel.CloseAsync();
                await channel.DisposeAsync();
            }
        }

        // QueeData sýnýfýnýn tanýmý eksikse, lütfen onu da ekle.
        // Örnek olarak ekledim:
        //public record QueeData(int id, string data, DateTime timestamp);
    } 
}
