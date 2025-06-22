using StackExchange.Redis;
using VaccineApp.OutboxPublisher;
using VaccineApp.OutboxPublisher.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var enviroment = builder.Environment.EnvironmentName ?? "Development";
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{enviroment}.json", optional: true)
            .Build();

        var redisConfig = builder.Configuration.GetSection("Redis");
        builder.Services.Configure<RedisOptions>(redisConfig);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["Redis:ConnectionString"];
            options.InstanceName = builder.Configuration["Redis:InstanceName"];
        });

        builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer => ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));
        builder.Services.AddHostedService<OutboxPublisherWorker>();
        builder.Services.AddHttpClient();

        var host = builder.Build();
        host.Run();
    }
}