using System.Text.Json.Serialization;
using VaccineApp.Core;

namespace VaccineApp.ViewModel.RequestDto
{
    public class LoginRequestDto 
    {
        public string Username { get; set; }

        public string Password { get; set; } 
    }
}
