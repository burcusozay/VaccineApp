using Microsoft.AspNetCore.Mvc;
using VaccineApp.Business.Interfaces;
using VaccineApp.ViewModel.Dtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class FreezerTemperatureController : ControllerBase
    {
        private readonly IFreezerTemperatureService _tempratureService;  
        public FreezerTemperatureController(IFreezerTemperatureService tempratureService)
        {
            _tempratureService = tempratureService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreezerTemperature(int id)
        {
            var stock = await _tempratureService.GetTemperatureByIdAsync(id);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpGet("FreezerTemperatures")]
        public async Task<IActionResult> GetFreezerTemperatureList([FromQuery] FreezerTemperatureRequestDto model)
        {
            var stock = await _tempratureService.GetFreezerTemperaturesAsync(model);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpPost("AddFreezerTemperature")]
        public async Task<IActionResult> AddFreezerTemperature([FromBody] FreezerTemperatureDto model)
        {
            var created = await _tempratureService.AddTemperatureAsync(model);
            return CreatedAtAction(nameof(GetFreezerTemperature), new { id = created.Id }, created);
        }
    }
 }