using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class Vaccine : BaseEntity<long>
{ 
    public string Name { get; set; } = null!;
     
    public string CompanyName { get; set; } = null!;

    public virtual ICollection<VaccineFreezer> VaccineFreezers { get; set; } = new List<VaccineFreezer>();
}
