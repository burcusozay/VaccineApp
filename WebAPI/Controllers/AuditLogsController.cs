using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaccineApp.Data.Context;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _context.AuditLogs.OrderByDescending(x => x.Timestamp).Take(100).ToListAsync();
            return Ok(logs);
        }
    }
}
