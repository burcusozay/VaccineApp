using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerStockDto : BaseEntityDto<long>
    {
        public long VaccineFreezerId { get; set; }

        public int StockCount { get; set; }
    }
}
