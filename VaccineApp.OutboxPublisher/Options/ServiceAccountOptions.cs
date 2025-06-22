using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineApp.OutboxPublisher.Options
{
    public class ServiceAccountOptions
    {
        public string Username { get; set; } // appsettings.json'daki bölüm adı
        public string Password { get; set; }
        public string TokenEndpoint { get; set; }
        public string ApiCallEndpoint { get; set; }
    }
}
