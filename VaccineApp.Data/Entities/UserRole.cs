using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class UserRole : BaseEntity<Guid>
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
