using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;

namespace VaccineApp.SoketPublisher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Run().GetAwaiter().GetResult();
        }

        public static async Task Run()
        {
            Console.WriteLine("Hello, World!");

            var listener = new TcpListener(IPAddress.Loopback, 65432);
            listener.Start();
            Console.WriteLine("Socket publisher started, waiting for client...");
            // bu satır kendisine bağlanan bir uygulama olana kadar programı bekletir.
            // Ne zaman bu ip:porta bir uygulama bağlanırsa o zaman   using var stream = client.GetStream(); satırına ilerler. 
            using var client = listener.AcceptTcpClient();
            using var stream = client.GetStream();
            var rnd = new Random();

            while (true)
            {
                var data = new
                {
                    Id = rnd.Next(1, 3),
                    Value = rnd.Next(-20, 11)
                };
                var msg = JsonSerializer.Serialize(data) + "\n";
                var bytes = Encoding.UTF8.GetBytes(msg);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                await stream.FlushAsync();
                Console.WriteLine($"Sent: {msg.Trim()}");
                await Task.Delay(1000);
            }
        }
    }
}