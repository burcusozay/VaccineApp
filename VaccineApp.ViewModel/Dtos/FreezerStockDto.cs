namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerStockDto
    {
        public long Id { get; set; }

        public long VaccineFreezerId { get; set; }

        public int StockCount { get; set; }
    }
}
