using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerTemperatureRequestDto : BaseRequestDto
    {
        [Range(-20, 100)]
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }  
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
