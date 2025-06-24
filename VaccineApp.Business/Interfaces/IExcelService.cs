using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineApp.Business.Interfaces
{
    public interface IExcelService
    {
        /// <summary>
        /// Herhangi bir türdeki veri listesini bir Excel dosyasına dönüştürür.
        /// </summary>
        /// <typeparam name="T">Veri listesinin tipi.</typeparam>
        /// <param name="data">Excel'e aktarılacak veri listesi.</param>
        /// <param name="sheetName">Excel sayfasının adı.</param>
        /// <returns>Excel dosyasının byte dizisi.</returns>
        Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1");
    }
}
