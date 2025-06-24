using Microsoft.AspNetCore.Mvc;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Services;
using VaccineApp.ViewModel.Dtos;
using VaccineApp.ViewModel.RequestDto;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FreezerTemperatureController : ControllerBase
    {
        private readonly IFreezerTemperatureService _tempratureService; 
        private readonly IExcelService _excelService; // Excel servisini inject et
        public FreezerTemperatureController(IFreezerTemperatureService tempratureService, IExcelService excelService)
        {
            _tempratureService = tempratureService;
            _excelService = excelService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreezerTemperature(int id)
        {
            var stock = await _tempratureService.GetTemperatureByIdAsync(id);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpGet("FreezerTemperatureList")]
        public async Task<IActionResult> GetFreezerTemperatureList([FromQuery] FreezerTemperatureRequestDto model)
        {
            var stock = await _tempratureService.GetFreezerTemperatureListAsync(model);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddFreezerTemperature([FromBody] FreezerTemperatureDto model)
        {
            var created = await _tempratureService.AddTemperatureAsync(model);
            return CreatedAtAction(nameof(GetFreezerTemperature), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFreezerTemperature(long id, [FromBody] FreezerTemperatureDto model)
        {
            // İsteğe bağlı ama önerilen: URL'deki id ile body'deki id'nin aynı olduğunu kontrol et.
            if (id != model.Id)
            {
                return BadRequest("URL ID ile gövde (body) ID'si uyuşmuyor.");
            }

            var updatedDto = await _tempratureService.UpdateTemperatureAsync(id, model);

            if (updatedDto == null)
            {
                // Güncellenmek istenen kaynak bulunamadıysa.
                return NotFound();
            }

            // DÜZELTME 2: Başarılı bir PUT isteği için 204 No Content veya 200 OK dönmek daha doğrudur.
            return NoContent(); // Başarılı, yanıt gövdesinde içerik yok.
            // Alternatif olarak güncellenmiş nesneyi de dönebilirsiniz: return Ok(updatedDto);
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteFreezerTemperature(int id)
        {
            await _tempratureService.DeleteTemperatureAsync(id);
            return Ok();
        }

        [HttpPost("SoftDelete/{id}")]
        public async Task<IActionResult> SoftDeleteFreezerTemperature(int id)
        {
            await _tempratureService.SoftDeleteTemperatureAsync(id);
            return Ok();
        }

        [HttpPost("Excel")] // Filtreleri body'de almak için POST
        public async Task<IActionResult> ExportExcel([FromBody] FreezerTemperatureRequestDto model)
        {
            // 1. Sayfalama olmadan filtrelenmiş tüm veriyi al
            var dataToExport = await _tempratureService.GetFreezerTemperatureListAsync(model);

            // 2. Excel servisi ile dosyayı byte dizisine çevir
            var fileBytes = await _excelService.ExportToExcelAsync(dataToExport.Items, "Sıcaklık Değerleri");

            // 3. Dosyayı kullanıcıya gönder
            string fileName = $"Excel_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}