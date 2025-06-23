using Microsoft.AspNetCore.SignalR;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.WebAPI.Hubs
{
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Bu metod NotificationWorker tarafından çağrılır.
        /// Gelen bildirimi alır ve tüm bağlı istemcilere (React) gönderir.
        /// </summary>
        /// <param name="notification">Gönderilecek bildirim nesnesi</param>
        public async Task BroadcastErrorNotification(NotificationDto notification)
        {
            // React istemcilerinin dinlediği "ReceiveErrorNotification" metodunu tetikle.
            await Clients.All.SendAsync("ReceiveErrorNotification", notification);
        }

    }
}
