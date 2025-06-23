using MassTransit;
using Microsoft.AspNetCore.SignalR.Client;
using VaccineApp.ViewModel.Dtos; // NotificationDto için namespace

namespace VaccineApp.NotificationWorker.Consumers
{
    public class RabbitNotificationNotificationConsumer : IConsumer<NotificationDto>
    {
        private readonly ILogger<RabbitNotificationNotificationConsumer> _logger;
        private readonly HubConnection _hubConnection;

        public RabbitNotificationNotificationConsumer(ILogger<RabbitNotificationNotificationConsumer> logger, HubConnection hubConnection)
        {
            _logger = logger;
            _hubConnection = hubConnection;
        }

        public async Task Consume(ConsumeContext<NotificationDto> context)
        {
            var notification = context.Message;
            _logger.LogInformation($"Hata bildirimi kuyruktan alýndý: Hata={notification.Message}");

            try
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("SignalR Hub baðlantýsý kuruldu.");
                }

                // DÜZELTME: Sunucudaki Hub'ýn "BroadcastErrorNotification" metodunu çaðýrýyoruz.
                // Bu metod da istemcilere "ReceiveErrorNotification" olayýný gönderecek.
                await _hubConnection.SendAsync("BroadcastErrorNotification", notification);

                _logger.LogInformation($"SignalR Hub'ýna bildirim gönderildi: Hata={notification.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignalR Hub'ýna bildirim gönderilirken hata oluþtu.");
                // Hata oluþursa MassTransit mesajý yeniden deneyecektir.
                throw;
            }
        }
    }
}