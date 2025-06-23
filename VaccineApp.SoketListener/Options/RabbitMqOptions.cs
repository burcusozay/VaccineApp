namespace VaccineApp.SoketListener.Options
{
    public class RabbitMqOptions
    {
        public const string RabbitMQ = "RabbitMQ"; // appsettings.json'daki bölüm adı

        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public int Port { get; set; } = 5672;
    }
}
