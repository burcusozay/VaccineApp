namespace VaccineApp.ViewModel.Dtos
{
    public class RefreshTokenDto
    {
        public long Id { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public bool IsActive { get; set; } 
        public Guid UserId { get; set; } 

    }
}
