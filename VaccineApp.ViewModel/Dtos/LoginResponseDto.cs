namespace VaccineApp.ViewModel.Dtos
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

    }
}
