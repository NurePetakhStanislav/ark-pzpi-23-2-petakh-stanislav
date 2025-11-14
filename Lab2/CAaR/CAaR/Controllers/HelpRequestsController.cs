using CAaR.Models;
using CAaR.Services;
using Microsoft.AspNetCore.Mvc;

namespace CAaR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpRequestsController : ControllerBase
    {
        private readonly GenericService<HelpRequest> _service;

        public HelpRequestsController(GenericService<HelpRequest> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<HelpRequest>>> GetAll()
        {
            var helpRequests = await _service.ReadAllAsync();
            return Ok(helpRequests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HelpRequest>> Get(int id)
        {
            var helpRequest = (await _service.ReadAllAsync()).FirstOrDefault(h => h.HelpID == id);
            if (helpRequest == null)
                return NotFound();
            return Ok(helpRequest);
        }

        [HttpPost]
        public async Task<ActionResult> Create(HelpRequest helpRequest)
        {
            await _service.AddAsync(helpRequest);
            return CreatedAtAction(nameof(Get), new { id = helpRequest.HelpID }, helpRequest);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, HelpRequest helpRequest)
        {
            if (id != helpRequest.HelpID)
                return BadRequest();

            await _service.UpdateAsync(helpRequest);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var helpRequest = (await _service.ReadAllAsync()).FirstOrDefault(h => h.HelpID == id);
            if (helpRequest == null)
                return NotFound();

            await _service.DeleteAsync(helpRequest);
            return NoContent();
        }
    }
}