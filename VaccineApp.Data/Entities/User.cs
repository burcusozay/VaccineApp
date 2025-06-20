using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class User : BaseEntity<Guid>
{
    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string NormalizedUserName { get; set; } = null!;
     
    public string? Address { get; set; }

    public string PhoneNumber { get; set; } = null!; 

    public string Email { get; set; } = null!; 

    public string PasswordHash { get; set; } // Hashlenmiş şifre 
     
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<VaccineOrder> VaccineOrders { get; set; } = new List<VaccineOrder>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
