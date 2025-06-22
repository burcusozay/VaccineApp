using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerTemperatureDto : BaseEntityDto<long>
    {  
        public long FreezerId { get; set; }

        public decimal Temperature { get; set; } 
    }
}
