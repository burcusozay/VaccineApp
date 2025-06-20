using Microsoft.AspNetCore.Mvc;
using VaccineApp.Business.Interfaces;
using VaccineApp.ViewModel.Dtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class FreezerTempratureController : ControllerBase
    {
        private readonly IFreezerTempratureService _tempratureService;  
        public FreezerTempratureController(IFreezerTempratureService tempratureService)
        {
            _tempratureService = tempratureService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreezerTemprature(int id)
        {
            var stock = await _tempratureService.GetTempratureByIdAsync(id);
            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFreezerStock([FromBody] FreezerTempratureDto model)
        {
            var created = await _tempratureService.AddTempratureAsync(model);
            return CreatedAtAction(nameof(GetFreezerTemprature), new { id = created.Id }, created);
        }
    }
 }