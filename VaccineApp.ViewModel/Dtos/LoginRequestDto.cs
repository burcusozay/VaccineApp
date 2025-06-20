using System.Text.Json.Serialization;

namespace VaccineApp.ViewModel.Dtos
{
    public class LoginRequestDto
    {
        public string Username { get; set; }

        public string Password { get; set; }


    }
}
