namespace VaccineApp.ViewModel.Dtos
{
    public class UserDto
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string Surname { get; set; } = null!;

        public string Username { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public string Address { get; set; } = null!;

        public string Phone { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
