using MassTransit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;
using VaccineApp.OutboxPublisher.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        // --- AppSettings Yap�land�rmas� ---
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

        //// RabbitMQ IConnection'� Singleton olarak kaydet.
        //// Ba�lant�y� olu�tururken hata olu�ursa, yeniden deneme mekanizmas� ekleyebilirsiniz.
        //builder.Services.AddSingleton<IConnection>(sp =>
        //{
        //    var factory = sp.GetRequiredService<ConnectionFactory>();
        //    try
        //    {
        //        // CreateConnectionAsync asenkron oldu�u i�in burada .GetAwaiter().GetResult() kullan�yoruz.
        //        // Bu, Program.cs'in sync context'inde DI container'� olu�tururken blocking bir �a�r� yapar.
        //        // Modern yakla��mlarda IHostedService'in ExecuteAsync metodunda ba�lant�y� kurmak da yayg�nd�r
        //        // ancak ba�lant�n�n her zaman a��k olmas� bekleniyorsa Singleton olarak kaydedilebilir.
        //        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        var logger = sp.GetRequiredService<ILogger<Program>>(); // Program.cs i�in logger alabiliriz
        //        logger.LogError(ex, "RabbitMQ ba�lant�s� kurulurken hata olu�tu!");
        //        // Hata durumunda uygulama ba�lamamal� veya retry mekanizmas� olmal�.
        //        // Basit bir �rnek i�in do�rudan f�rlat�yoruz.
        //        throw;
        //    }
        //});

        // Redis ConnectionMultiplexer (Distributed Lock i�in)
        builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer => ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));

        // MassTransit ve RabbitMQ Yap�land�rmas�
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