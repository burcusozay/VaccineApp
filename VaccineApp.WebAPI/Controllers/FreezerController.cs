using Microsoft.AspNetCore.Mvc;
using VaccineApp.Business.Interfaces;
using VaccineApp.ViewModel.Dtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class FreezerController : ControllerBase
    {
        private readonly ILogger<FreezerController> _logger;
        private readonly IFreezerService _freezerService;
        private readonly IExcelService _excelService; // Excel servisini inject et
        public FreezerController(ILogger<FreezerController> logger, IFreezerService freezerService, IExcelService excelService)
        {
            _logger = logger;
            _freezerService = freezerService;
            _excelService = excelService;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreezer(int id)
        {
            var stock = await _freezerService.GetFreezerByIdAsync(id);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpGet("FreezerList")]
        public async Task<IActionResult> GetFreezerList([FromQuery] FreezerRequestDto model)
        {
            var stock = await _freezerService.GetFreezerListAsync(model);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddFreezer([FromBody] FreezerDto model)
        {
            var created = await _freezerService.AddFreezerAsync(model);
            return CreatedAtAction(nameof(GetFreezer), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFreezer(long id, [FromBody] FreezerDto model)
        {
            // İsteğe bağlı ama önerilen: URL'deki id ile body'deki id'nin aynı olduğunu kontrol et.
            if (id != model.Id)
            {
                return BadRequest("URL ID ile gövde (body) ID'si uyuşmuyor.");
            }

            var updatedDto = await _freezerService.UpdateFreezerAsync(id, model);

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
        public async Task<IActionResult> DeleteFreezer(int id)
        {
            await _freezerService.DeleteFreezerAsync(id);
            return Ok();
        }

        [HttpPost("SoftDelete/{id}")]
        public async Task<IActionResult> SoftDeleteFreezer(int id)
        {
            await _freezerService.SoftDeleteFreezerAsync(id);
            return Ok();
        }


        [HttpPost("Excel")] // Filtreleri body'de almak için POST
        public async Task<IActionResult> ExportExcel([FromBody] FreezerRequestDto model)
        {
            // 1. Sayfalama olmadan filtrelenmiş tüm veriyi al
            var dataToExport = await _freezerService.GetFreezerListAsync(model);

            // 2. Excel servisi ile dosyayı byte dizisine çevir
            var fileBytes = await _excelService.ExportToExcelAsync(dataToExport.Items);

            // 3. Dosyayı kullanıcıya gönder
            string fileName = $"Excel_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
