using VaccineApp.Core;

namespace VaccineApp.Data.Entities
{
    public class AuditLog : BaseEntity<long>
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Username { get; set; }
        public string? IpAddress { get; set; }
        public string? Action { get; set; } // Created / Updated / Deleted
        public string? Controller { get; set; }
        public string? ActionName { get; set; }
        public string? Route { get; set; }
        public string? TableName { get; set; }
        public string? PrimaryKey { get; set; }
        public string? Changes { get; set; } // JSON format    }
    }
}