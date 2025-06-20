using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class Freezer : BaseEntity<long>
{
    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public int OrderNo { get; set; }

    public virtual ICollection<FreezerTemprature> FreezerTempratures { get; set; } = new List<FreezerTemprature>();

    public virtual ICollection<VaccineFreezer> VaccineFreezers { get; set; } = new List<VaccineFreezer>();
}
