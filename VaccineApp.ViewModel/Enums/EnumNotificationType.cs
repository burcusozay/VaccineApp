using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineApp.ViewModel.Enums
{
    public enum EnumNotificationType
    {
        [Description("Bilgi")]
        Information = 1,
        [Description("Uyarı")]
        Warning = 2,
        [Description("Hata")]
        Error = 3,
        [Description("Başarılı")]
        Success = 4,
        [Description("Kritik")]
        Critical = 5
    }
}
