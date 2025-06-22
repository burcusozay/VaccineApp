using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class UserDto : BaseEntityDto<Guid>
    {  
        public string Name { get; set; } = null!;

        public string Surname { get; set; } = null!;

        public string Username { get; set; } = null!; 

        public string Address { get; set; } = null!;

        public string Phone { get; set; } = null!;
        public string Role { get; set; }
        public List<string> Roles { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
