using ClosedXML.Excel;
using System.Reflection;
using VaccineApp.Business.Interfaces;

namespace VaccineApp.Business.Services
{
    public class ExcelService : IExcelService
    {
        public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1")
        {
            // Reflection kullanarak T tipinin özelliklerini al
            PropertyInfo[] properties = typeof(T).GetProperties();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);

                // Başlıkları (kolon adlarını) oluştur
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = properties[i].Name;
                }

                // Başlık satırını biçimlendir
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Verileri satırlara yaz
                int row = 2;
                foreach (var item in data)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var value = properties[i].GetValue(item, null);
                        worksheet.Cell(row, i + 1).Value = value != null ? value.ToString() : "";
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents(); // Sütun genişliklerini ayarla

                // Workbook'u bir memory stream'e kaydet
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return await Task.FromResult(stream.ToArray());
                }
            }
        }
    }
}
