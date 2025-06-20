using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class FreezerStock : BaseEntity<long>
{
    public long VaccineFreezerId { get; set; }

    public int StockCount { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual VaccineFreezer VaccineFreezer { get; set; } = null!;

    public virtual ICollection<VaccineOrder> VaccineOrders { get; set; } = new List<VaccineOrder>();
}
