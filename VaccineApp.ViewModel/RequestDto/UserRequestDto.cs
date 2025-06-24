using System.Text.Json.Serialization;
using VaccineApp.Core.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class UserRequestDto : BaseRequestDto
    {
        public string Username { get; set; }
        public string Name { get; set; } 
        public string Surname { get; set; } 
        public string Email { get; set; } 
    }
}
