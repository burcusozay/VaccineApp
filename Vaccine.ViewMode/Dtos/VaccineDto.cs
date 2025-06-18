namespace VaccineApp.ViewModel.Dtos
{
    public class VaccineDto
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public string CompanyName { get; set; } = null!;
    }
}
