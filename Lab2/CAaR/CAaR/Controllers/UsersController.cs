using System.Security.Cryptography;
using System.Text;
using CAaR.Models;
using CAaR.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly GenericService<User> _service;

    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    public UsersController(GenericService<User> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll()
    {
        return await _service.ReadAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(int id)
    {
        var user = await _service.ReadAsyncByID(id);
        if (user == null)
            return NotFound($"Даного користувача з ID {id} не існує");

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create(User user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        user.Password = HashPassword(user.Password);
        await _service.AddAsync(user);
        return CreatedAtAction(nameof(Get), new { id = user.UserID }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> Update(int id, User user)
    {
        if (id != user.UserID)
            return Forbid("Заборонено змінювати ID користувача");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateAsync(user);
        return Ok(user);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<User>> Patch(int id, [FromBody] JsonPatchDocument<User> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest("Патч документ не може бути порожнім.");

        var existingUser = await _service.ReadAsyncByID(id);
        if (existingUser == null)
            return NotFound($"Користувач з ID {id} не знайдений.");

        if (patchDoc.Operations.Any(op => op.path.ToLower() == "/userid"))
            return Forbid("Змінювати UserID заборонено.");

        patchDoc.ApplyTo(existingUser);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (patchDoc.Operations.Any(op => op.path.ToLower() == "/password"))
        {
            existingUser.Password = HashPassword(existingUser.Password);
        }

        await _service.UpdateAsync(existingUser);
        return Ok(existingUser);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var user = await _service.ReadAsyncByID(id);
        if (user == null)
            return NotFound("Даного користувача за ID не існує");

        await _service.DeleteAsync(user);
        return NoContent();
    }
}