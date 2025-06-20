using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class VaccineFreezer : BaseEntity<long>
{ 

    public long VaccineId { get; set; }

    public long FreezerId { get; set; }

    public virtual Freezer Freezer { get; set; } = null!;

    public virtual ICollection<FreezerStock> FreezerStocks { get; set; } = new List<FreezerStock>();

    public virtual Vaccine Vaccine { get; set; } = null!;
}
