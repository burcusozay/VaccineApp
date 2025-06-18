using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class FreezerController : ControllerBase
    {
        private readonly ILogger<FreezerController> _logger;
        public FreezerController(ILogger<FreezerController> logger)
        {
            _logger = logger;
        }
    }
}
