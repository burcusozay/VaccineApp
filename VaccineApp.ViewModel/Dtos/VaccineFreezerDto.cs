using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class VaccineFreezerDto : BaseEntityDto<long>
    {  
        public long VaccineId { get; set; }

        public long FreezerId { get; set; } 
    }
}
