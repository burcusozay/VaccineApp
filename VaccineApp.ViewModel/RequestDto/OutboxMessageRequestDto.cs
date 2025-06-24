using System.Text.Json.Serialization;
using VaccineApp.Core.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class OutboxMessageRequestDto : BaseRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
