using MassTransit;
using Microsoft.AspNetCore.SignalR.Client;
using VaccineApp.ViewModel.Dtos; // NotificationDto i�in namespace

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
            _logger.LogInformation($"Hata bildirimi kuyruktan al�nd�: Hata={notification.Message}");

            try
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("SignalR Hub ba�lant�s� kuruldu.");
                }

                // D�ZELTME: Sunucudaki Hub'�n "BroadcastErrorNotification" metodunu �a��r�yoruz.
                // Bu metod da istemcilere "ReceiveErrorNotification" olay�n� g�nderecek.
                await _hubConnection.SendAsync("BroadcastErrorNotification", notification);

                _logger.LogInformation($"SignalR Hub'�na bildirim g�nderildi: Hata={notification.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignalR Hub'�na bildirim g�nderilirken hata olu�tu.");
                // Hata olu�ursa MassTransit mesaj� yeniden deneyecektir.
                throw;
            }
        }
    }
}