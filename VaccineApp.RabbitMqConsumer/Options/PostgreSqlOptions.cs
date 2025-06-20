using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineApp.RabbitMqConsumer.Options
{
    public class PostgreSqlOptions
    {
        public const string PostgreSql = "PostgreSql"; // appsettings.json'daki bölüm adı
        public string PostgreSqlConnection { get; set; }
    }
}
