using MassTransit;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using VaccineApp.NotificationWorker.Consumers;
using VaccineApp.NotificationWorker.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // --- AppSettings Yapýlandýrmasý ---
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // --- Servislerin Eklenmesi ---

        // Options
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
        builder.Services.Configure<SignalROptions>(builder.Configuration.GetSection("SignalR"));

        // SignalR Hub Connection'ý Singleton olarak ekliyoruz.
        builder.Services.AddSingleton(sp =>
        {
            var signalROptions = sp.GetRequiredService<IOptions<SignalROptions>>().Value;
            return new HubConnectionBuilder()
                .WithUrl(signalROptions.HubUrl)
                .WithAutomaticReconnect()
                .Build();
        });

        // MassTransit ve RabbitMQ Tüketici Yapýlandýrmasý
        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<RabbitNotificationNotificationConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                cfg.Host(rabbitMqOptions.HostName, (ushort)rabbitMqOptions.Port, "/", h => {
                    h.Username(rabbitMqOptions.UserName);
                    h.Password(rabbitMqOptions.Password);
                });

                cfg.ReceiveEndpoint("error-notification-queue", e =>
                {
                    e.ConfigureConsumer<RabbitNotificationNotificationConsumer>(context);
                });
            });
        }); 
   
        var host = builder.Build();
        host.Run();
    }
}