using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class Role : BaseEntity<Guid>
{
    public string Name { get; set; } = null!;
     
    public string NormalizedName { get; set; } = null!; 

    public string Description { get; set; } // Hashlenmiş şifre

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
