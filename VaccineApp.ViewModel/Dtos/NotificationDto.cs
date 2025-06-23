using VaccineApp.ViewModel.Enums;

namespace VaccineApp.ViewModel.Dtos
{
    public class NotificationDto
    {
        public string Message { get; set; }
        public EnumNotificationType NotificationType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
