using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class VaccineOrderDto : BaseEntityDto<long>
    { 
        public Guid UserId { get; set; }

        public long FreezerStockId { get; set; } 
    }
}
