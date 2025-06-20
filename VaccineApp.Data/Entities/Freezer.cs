using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class Freezer : BaseEntity<long>
{
    public string Name { get; set; } = null!;

    public int OrderNo { get; set; }

    public virtual ICollection<FreezerTemperature> FreezerTemperatures { get; set; } = new List<FreezerTemperature>();

    public virtual ICollection<VaccineFreezer> VaccineFreezers { get; set; } = new List<VaccineFreezer>();
}
