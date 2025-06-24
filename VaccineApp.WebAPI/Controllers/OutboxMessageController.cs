using Microsoft.AspNetCore.Mvc;
using VaccineApp.Business.Interfaces;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OutboxMessageController : ControllerBase
    {
        private readonly IOutboxMessageService _outboxMessageService;
        public OutboxMessageController(IOutboxMessageService outboxMessageService)
        {
            _outboxMessageService = outboxMessageService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOutboxMessage(int id)
        {
            var outboxMessage = await _outboxMessageService.GetOutboxMessageByIdAsync(id);
            if (outboxMessage == null)
                return NotFound();

            return Ok(outboxMessage);
        }
        [HttpGet("UnprocessedList")]
        public async Task<ActionResult<List<OutboxMessageDto>>> GetUnprocessedList()
        {
            var unprocessList = await _outboxMessageService.GetUnprocessedMessageListAsync();
             
            return Ok(unprocessList);
        }

        [HttpPost("MarkProcessed/{id}")]
        public async Task<IActionResult> MarkProcessed(long id)
        {
            var msg = await _outboxMessageService.MarkProcessedMessageAsync(id);
            if (msg == null) return NotFound();
              
            return Ok(msg);
        }

        [HttpPost("AddOutboxMessage")]
        public async Task<IActionResult> AddOutboxMessage([FromBody] OutboxMessageDto model)
        {
            var created = await _outboxMessageService.AddOutboxMessageAsync(model);
            return CreatedAtAction(nameof(GetOutboxMessage), new { id = created.Id }, created);
        }
    }
}