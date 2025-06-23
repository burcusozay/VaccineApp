using RabbitMQ.Client;
using VaccineApp.SoketListener;
using VaccineApp.SoketListener.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
        builder.Services.AddSingleton<ConnectionFactory>(sp =>
        {
            var rabbitMqOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>().Value;
            return new ConnectionFactory()
            {
                HostName = rabbitMqOptions.HostName,
                UserName = rabbitMqOptions.UserName,
                Password = rabbitMqOptions.Password,
                Port = rabbitMqOptions.Port
            };
        });

        // RabbitMQ IConnection'� Singleton olarak kaydet.
        // Ba�lant�y� olu�tururken hata olu�ursa, yeniden deneme mekanizmas� ekleyebilirsiniz.
        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<ConnectionFactory>();
            try
            {
                // CreateConnectionAsync asenkron oldu�u i�in burada .GetAwaiter().GetResult() kullan�yoruz.
                // Bu, Program.cs'in sync context'inde DI container'� olu�tururken blocking bir �a�r� yapar.
                // Modern yakla��mlarda IHostedService'in ExecuteAsync metodunda ba�lant�y� kurmak da yayg�nd�r
                // ancak ba�lant�n�n her zaman a��k olmas� bekleniyorsa Singleton olarak kaydedilebilir.
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                var logger = sp.GetRequiredService<ILogger<Program>>(); // Program.cs i�in logger alabiliriz
                logger.LogError(ex, "RabbitMQ ba�lant�s� kurulurken hata olu�tu!");
                // Hata durumunda uygulama ba�lamamal� veya retry mekanizmas� olmal�.
                // Basit bir �rnek i�in do�rudan f�rlat�yoruz.
                throw;
            }
        });

        builder.Services.AddHostedService<SoketListenerWorker>(); 
        //builder.Configuration.GetConnectionString("PostgresConnection");

        var host = builder.Build();
        host.Run();
    }
}