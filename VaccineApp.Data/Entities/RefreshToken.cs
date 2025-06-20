using VaccineApp.Core;

namespace VaccineApp.Data.Entities
{
    public class RefreshToken  :BaseEntity<long>
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public new bool IsActive => Revoked == null && !IsExpired;

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
