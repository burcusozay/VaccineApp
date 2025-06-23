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

        // RabbitMQ IConnection'ý Singleton olarak kaydet.
        // Baðlantýyý oluþtururken hata oluþursa, yeniden deneme mekanizmasý ekleyebilirsiniz.
        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<ConnectionFactory>();
            try
            {
                // CreateConnectionAsync asenkron olduðu için burada .GetAwaiter().GetResult() kullanýyoruz.
                // Bu, Program.cs'in sync context'inde DI container'ý oluþtururken blocking bir çaðrý yapar.
                // Modern yaklaþýmlarda IHostedService'in ExecuteAsync metodunda baðlantýyý kurmak da yaygýndýr
                // ancak baðlantýnýn her zaman açýk olmasý bekleniyorsa Singleton olarak kaydedilebilir.
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                var logger = sp.GetRequiredService<ILogger<Program>>(); // Program.cs için logger alabiliriz
                logger.LogError(ex, "RabbitMQ baðlantýsý kurulurken hata oluþtu!");
                // Hata durumunda uygulama baþlamamalý veya retry mekanizmasý olmalý.
                // Basit bir örnek için doðrudan fýrlatýyoruz.
                throw;
            }
        });

        builder.Services.AddHostedService<SoketListenerWorker>(); 
        //builder.Configuration.GetConnectionString("PostgresConnection");

        var host = builder.Build();
        host.Run();
    }
}