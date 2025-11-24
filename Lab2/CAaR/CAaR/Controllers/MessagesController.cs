using CAaR.Models;
using CAaR.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly GenericService<Message> _service;
    private readonly GenericService<User> _userService;

    public MessagesController(GenericService<Message> service, GenericService<User> userService)
    {
        _service = service;
        _userService = userService;
    }

    private bool AnyFromPatchDoc(JsonPatchDocument<Message> patchDoc, string id)
    {
        return patchDoc.Operations.Any(op => op.path.ToLower() == id.ToLower());
    }

    [HttpGet]
    public async Task<ActionResult<List<Message>>> GetAll()
    {
        var message = await _service.ReadAllAsync();
        return Ok(message);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> Get(int id)
    {
        var message = await _service.ReadAsyncByID(id);
        if (message == null)
            return NotFound();

        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Message message)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.ReadAsyncByID(message.UserID);
        if (user == null)
            return BadRequest("Даний користувач відсутній");

        message.User = user;
        await _service.AddAsync(message);
        return CreatedAtAction(nameof(Get), new { id = message.MessageID }, message);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Message>> Update(int id, Message message)
    {
        var realMessage = await _service.ReadAsyncByID(id);
        if (realMessage == null)
            return NotFound($"Повідомлення за адресою {id} не існує");

        message.MessageID = realMessage.MessageID;
        message.UserID = realMessage.UserID;
        message.User = realMessage.User;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateAsync(message);
        return Ok(message);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Message>> Patch(int id, [FromBody] JsonPatchDocument<Message> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest("Патч документ не може бути порожнім.");

        var existingMessage = await _service.ReadAsyncByID(id);
        if (existingMessage == null)
            return NotFound($"Повідомлення з ID {id} не знайдено.");

        if (AnyFromPatchDoc(patchDoc, "/messageid"))
            return Forbid("Змінювати MessageID заборонено.");

        if (AnyFromPatchDoc(patchDoc, "/userid"))
            return Forbid("Змінювати UserID заборонено.");

        if (AnyFromPatchDoc(patchDoc, "/messagedate"))
            return Forbid("Змінювати дату повідомлення заборонено");

        patchDoc.ApplyTo(existingMessage);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateAsync(existingMessage);
        return Ok(existingMessage);
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