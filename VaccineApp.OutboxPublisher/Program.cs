using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;
using VaccineApp.OutboxPublisher.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        // --- AppSettings Yapýlandýrmasý ---
        var builder = Host.CreateApplicationBuilder(args);
        var enviroment = builder.Environment.EnvironmentName ?? "Development";
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{enviroment}.json", optional: true)
            .Build();

        // --- Servislerin Eklenmesi ---

        // Options Pattern
        builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
        builder.Services.Configure<ServiceAccountOptions>(builder.Configuration.GetSection("ServiceAccount"));
        
        // Redis Cache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["Redis:ConnectionString"];
            options.InstanceName = builder.Configuration["Redis:InstanceName"];
        });

        //builder.Services.AddSingleton<ConnectionFactory>(sp =>
        //{
        //    var rabbitMqOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>().Value;
        //    return new ConnectionFactory()
        //    {
        //        HostName = rabbitMqOptions.HostName,
        //        UserName = rabbitMqOptions.UserName,
        //        Password = rabbitMqOptions.Password,
        //        Port = rabbitMqOptions.Port
        //    };
        //});

        //// RabbitMQ IConnection'ý Singleton olarak kaydet.
        //// Baðlantýyý oluþtururken hata oluþursa, yeniden deneme mekanizmasý ekleyebilirsiniz.
        //builder.Services.AddSingleton<IConnection>(sp =>
        //{
        //    var factory = sp.GetRequiredService<ConnectionFactory>();
        //    try
        //    {
        //        // CreateConnectionAsync asenkron olduðu için burada .GetAwaiter().GetResult() kullanýyoruz.
        //        // Bu, Program.cs'in sync context'inde DI container'ý oluþtururken blocking bir çaðrý yapar.
        //        // Modern yaklaþýmlarda IHostedService'in ExecuteAsync metodunda baðlantýyý kurmak da yaygýndýr
        //        // ancak baðlantýnýn her zaman açýk olmasý bekleniyorsa Singleton olarak kaydedilebilir.
        //        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        var logger = sp.GetRequiredService<ILogger<Program>>(); // Program.cs için logger alabiliriz
        //        logger.LogError(ex, "RabbitMQ baðlantýsý kurulurken hata oluþtu!");
        //        // Hata durumunda uygulama baþlamamalý veya retry mekanizmasý olmalý.
        //        // Basit bir örnek için doðrudan fýrlatýyoruz.
        //        throw;
        //    }
        //});

        // Redis ConnectionMultiplexer (Distributed Lock için)
        builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer => ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));

        // MassTransit ve RabbitMQ Yapýlandýrmasý
        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                cfg.Host(rabbitMqOptions.HostName, (ushort)rabbitMqOptions.Port, "/", h => {
                    h.Username(rabbitMqOptions.UserName);
                    h.Password(rabbitMqOptions.Password);
                });
            });
        });

        // Worker Servisi
        builder.Services.AddHostedService<OutboxPublisherWorker>();
        builder.Services.AddHttpClient();

        var host = builder.Build();
        host.Run();
    }
}