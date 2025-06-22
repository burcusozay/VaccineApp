using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccineApp.Core;

namespace VaccineApp.Data.Entities
{
    public class OutboxMessage : BaseEntity<long>
    { 
        public string Type { get; set; } // "FreezerAlarm" vb.
        public string Payload { get; set; } // JSON string
        public DateTime OccuredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string? Error { get; set; }
    }
}
