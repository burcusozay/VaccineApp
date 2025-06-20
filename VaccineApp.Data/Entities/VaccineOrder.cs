using VaccineApp.Core;

namespace VaccineApp.Data.Entities;

public class VaccineOrder : BaseEntity<long>
{ 
    public Guid UserId { get; set; }

    public long FreezerStockId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime OrderDate { get; set; }

    public int VaccineOrderCount { get; set; }

    public virtual FreezerStock FreezerStock { get; set; } = null!;
    
    public virtual User User { get; set; } = null!;
}
