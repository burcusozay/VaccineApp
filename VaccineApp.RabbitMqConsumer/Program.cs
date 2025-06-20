using VaccineApp.RabbitMqConsumer;
using VaccineApp.RabbitMqConsumer.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // appsettings.json'dan "ConnectionStrings" bölümünü PostgreSqlOptions'a baðla
                // hostContext.Configuration, uygulamanýn yapýlandýrma kaynaðýný temsil eder. 
                services.Configure<PostgreSqlOptions>(hostContext.Configuration.GetSection("ConnectionStrings"));
                // RabbitMqConsumerWorker sýnýfýnýzý bir Hosted Service olarak kaydet
                // Böylece DI, constructor'ýndaki baðýmlýlýklarý otomatik olarak enjekte edecektir. 
                services.AddHostedService<RabbitMqConsumerWorker>();
            }).Build();
        builder.Run();
    }
}