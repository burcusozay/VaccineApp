namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerTemperatureDto
    {
        public long Id { get; set; }

        public long FreezerId { get; set; }

        public decimal Temperature { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
