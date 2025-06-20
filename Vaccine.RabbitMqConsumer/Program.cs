using VaccineApp.RabbitMqConsumer;
using VaccineApp.RabbitMqConsumer.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // appsettings.json'dan "ConnectionStrings" b�l�m�n� PostgreSqlOptions'a ba�la
                // hostContext.Configuration, uygulaman�n yap�land�rma kayna��n� temsil eder. 
                services.Configure<PostgreSqlOptions>(hostContext.Configuration.GetSection("ConnectionStrings"));
                // RabbitMqConsumerWorker s�n�f�n�z� bir Hosted Service olarak kaydet
                // B�ylece DI, constructor'�ndaki ba��ml�l�klar� otomatik olarak enjekte edecektir. 
                services.AddHostedService<RabbitMqConsumerWorker>();
            }).Build();
        builder.Run();
    }
}