namespace VaccineApp.ViewModel.Dtos
{
    public class VaccineOrderDto
    {
        public long Id { get; set; }

        public Guid UserId { get; set; }

        public long FreezerStockId { get; set; }

    }
}
