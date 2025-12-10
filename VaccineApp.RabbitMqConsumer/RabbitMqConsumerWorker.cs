using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using VaccineApp.RabbitMqConsumer.Options;
using VaccineApp.ViewModel.Dtos;
using VaccineApp.ViewModel.Worker;

namespace VaccineApp.RabbitMqConsumer
{
    // RabbitMQ kuyruðundan mesajlarý dinler ve iþleme alýr
    public class RabbitMqConsumerWorker : BackgroundService
    {
        private readonly ILogger<RabbitMqConsumerWorker> _logger;
        private readonly ServiceAccountOptions _serviceAccount;
        private Dictionary<long, decimal> _consecutiveOverFive;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _distributedCache;
        private string _bearerToken;
        private DateTime _tokenExpire;
        private readonly object _tokenLock = new object();


        public RabbitMqConsumerWorker(ILogger<RabbitMqConsumerWorker> logger, IOptions<ServiceAccountOptions> serviceAccount, IHttpClientFactory httpClientFactory, IDistributedCache distributedCache)
        {
            _logger = logger;
            _consecutiveOverFive = new Dictionary<long, decimal>();
            _serviceAccount = serviceAccount.Value;
            _httpClientFactory = httpClientFactory;
            _distributedCache = distributedCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new RabbitMQ.Client.ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "123456" };
            using var conn = await factory.CreateConnectionAsync();
            using var channel = await conn.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "soket_data_queue", durable: false, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var queeData = JsonSerializer.Deserialize<QueeData>(message);

                // Alarm logic
                if (queeData != null && queeData.Value > 5)
                {
                    if (!_consecutiveOverFive.ContainsKey(queeData.Id))
                    {
                        _consecutiveOverFive.Add(queeData.Id, 1);
                    }
                    else
                    {
                        _consecutiveOverFive[queeData.Id]++;
                        if (_consecutiveOverFive[queeData.Id] >= 5)
                        {
                            // ALARM DURUMUNDA WEB API'YE HTTP POST AT  
                            try
                            {
                                var data = new
                                {
                                    Id = 0,
                                    FreezerId = queeData.Id,
                                    Temperature = queeData.Value,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                // API adresini kendine göre deðiþtir!
                                await SendRequestToApiAsync(data);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Could not communicate to WebAPI ex:{ex}");
                            }

                            Console.WriteLine($"alarm id: {queeData.Id}, count:{_consecutiveOverFive[queeData.Id]}");
                            _consecutiveOverFive[queeData.Id] = 0; // alarm sonrasý sayaç sýfýrlanýr
                        }
                    }
                }
                else
                {
                    //consecutiveOverFive[queeData.Id] = 0;
                }
            };

            await channel.BasicConsumeAsync(queue: "soket_data_queue", autoAck: true, consumer: consumer);

            // Sonsuz bekleme
            //return Task.Delay(-1, stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken); // Hata veren satýrýn düzeltilmiþ hali 
        }

        private async Task SendRequestToApiAsync(object data)
        {
            try
            {

                using var httpClient = _httpClientFactory.CreateClient();
                // Token’ý thread-safe þekilde al
                var token = await GetBearerTokenAsync();

                // 2. Token'ý header'a ekle
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 3. Asýl POST isteði
                var response = await httpClient.PostAsJsonAsync(_serviceAccount.ApiCallEndpoint, data);
                response.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            { 
                throw ex;
            }
        }

        private async Task<string> GetBearerTokenAsync()
        {
            const string tokenKey = "rabbit_bearer_token";
            string token = await _distributedCache.GetStringAsync(tokenKey);
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            // Token cache kontrolü
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_bearerToken) && DateTime.UtcNow < _tokenExpire)
                {
                    return _bearerToken;
                }
            }

            // Token alýnmamýþ veya süresi geçmiþse tekrar iste
            using var httpClient = _httpClientFactory.CreateClient();
            var loginRequest = new
            {
                Username = _serviceAccount.Username,
                Password = _serviceAccount.Password
            };
            var tokenResponse = await httpClient.PostAsJsonAsync(_serviceAccount.TokenEndpoint, loginRequest);
            tokenResponse.EnsureSuccessStatusCode();

            var tokenContent = await tokenResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
            token = tokenContent?.AccessToken;

            if (token == null)
                throw new Exception("Token alýnamadý.");

            var expires = tokenContent.ExpirationTime; // Token süresini payload'dan alabilirsin

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _distributedCache.SetStringAsync(tokenKey, token, options);

            return token;
        }
    }
}