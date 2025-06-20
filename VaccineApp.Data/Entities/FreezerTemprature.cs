using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class FreezerTemprature : BaseEntity<long>
{
    public long FreezerId { get; set; }

    public decimal Temprature { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Freezer Freezer { get; set; } = null!;
}
