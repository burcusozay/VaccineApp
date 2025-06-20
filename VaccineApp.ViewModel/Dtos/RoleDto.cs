namespace VaccineApp.ViewModel.Dtos
{
    public class RoleDto
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
          
        public string Normalizedname { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public string Description { get; set; } = null!; 
    }
}
