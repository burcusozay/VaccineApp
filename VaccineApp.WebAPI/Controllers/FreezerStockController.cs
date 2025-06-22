using Microsoft.AspNetCore.Mvc;
using VaccineApp.Business.Interfaces;
using VaccineApp.ViewModel.Dtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 

    public class FreezerStockController : ControllerBase
    {
        private readonly IFreezerStockService _stockService;

        public FreezerStockController(IFreezerStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreezerStock(int id)
        {
            var stock = await _stockService.GetStockByIdAsync(id);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpPost("AddFreezerStock")]
        public async Task<IActionResult> AddFreezerStock([FromBody] FreezerStockDto model)
        {
            var created = await _stockService.AddStockAsync(model);
            return CreatedAtAction(nameof(GetFreezerStock), new { id = created.Id }, created);
        }
    }
}