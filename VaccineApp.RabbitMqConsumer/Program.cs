using Microsoft.Extensions.Hosting;
using VaccineApp.RabbitMqConsumer;
using VaccineApp.RabbitMqConsumer.Options;

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

        builder.Services.Configure<ServiceAccountOptions>(builder.Configuration.GetSection("ServiceAccount"));
        builder.Services.AddHostedService<RabbitMqConsumerWorker>();
        builder.Services.AddHttpClient();
        builder.Configuration.GetConnectionString("PostgresConnection");

        var host = builder.Build();
        host.Run();
    }
}