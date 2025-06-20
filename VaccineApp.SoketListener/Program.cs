using VaccineApp.SoketListener;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<SoketListenerWorker>();

        var host = builder.Build();
        host.Run();
    }
}