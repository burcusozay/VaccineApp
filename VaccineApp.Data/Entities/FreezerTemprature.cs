using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class FreezerTemperature : BaseEntity<long>
{
    public long FreezerId { get; set; }

    public decimal Temperature { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Freezer Freezer { get; set; } = null!;
}
