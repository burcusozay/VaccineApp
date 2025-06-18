using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class User : BaseEntity<long>
{
    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Username { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public bool IsActive { get; set; }
     
    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!; 

    public string PasswordHash { get; set; } // Hashlenmiş şifre

    public string Role { get; set; }

    public virtual ICollection<VaccineOrder> VaccineOrders { get; set; } = new List<VaccineOrder>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
