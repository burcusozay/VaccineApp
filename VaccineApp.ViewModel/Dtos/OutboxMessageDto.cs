using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class OutboxMessageDto : BaseEntityDto<long>
    { 
        public string Type { get; set; }
        public string Payload { get; set; }
        public DateTime OccuredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string? Error { get; set; } 
    }
}
