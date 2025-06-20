namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerTempratureDto
    {
        public long Id { get; set; }

        public long FreezerId { get; set; }

        public decimal Temprature { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
