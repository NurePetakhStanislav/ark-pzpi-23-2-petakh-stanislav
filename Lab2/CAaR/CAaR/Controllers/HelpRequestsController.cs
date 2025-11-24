using CAaR.Models;
using CAaR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

[Route("api/[controller]")]
[ApiController]
public class HelpRequestsController : ControllerBase
{
    private readonly GenericService<HelpRequest> _service;
    private readonly GenericService<User> _userService;

    public HelpRequestsController(GenericService<HelpRequest> service, GenericService<User> userService)
    {
        _service = service;
        _userService = userService;
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
        var helpRequest = await _service.ReadAsyncByID(id);
        if (helpRequest == null)
            return NotFound($"Даного запиту з ID {id} не існує");

        return Ok(helpRequest);
    }

    [HttpPost]
    public async Task<ActionResult> Create(HelpRequest helpRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.ReadAsyncByID(helpRequest.UserID);
        if (user == null)
            return BadRequest("Даний користувач відсутній");

        helpRequest.User = user;
        await _service.AddAsync(helpRequest);
        return CreatedAtAction(nameof(Get), new { id = helpRequest.HelpID }, helpRequest);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var helpRequest = await _service.ReadAsyncByID(id);
        if (helpRequest == null)
            return NotFound();

        await _service.DeleteAsync(helpRequest);
        return NoContent();
    }
}