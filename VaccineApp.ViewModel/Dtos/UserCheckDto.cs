namespace VaccineApp.ViewModel.Dtos
{
    public class UserCheckDto
    {  
        public string Username { get; set; } = null!; 
        public string PasswordHash { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
