using System.Security.Cryptography;
using System.Text;
using CAaR.Models;
using CAaR.Services;
using Microsoft.AspNetCore.Mvc;

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
        var user = (await _service.ReadAllAsync()).FirstOrDefault(u => u.UserID == id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create(User user)
    {
        if (string.IsNullOrWhiteSpace(user.UserName)) return BadRequest("Ім'я має бути обов'язковим");
        if (string.IsNullOrWhiteSpace(user.Email)) return BadRequest("Email має бути обов'язковим");
        if (string.IsNullOrWhiteSpace(user.Role)) return BadRequest("Роль має бути обов'язковим");
        if (string.IsNullOrWhiteSpace(user.Password)) return BadRequest("Пароль має бути обов'язковим");

        user.Password = HashPassword(user.Password);
        await _service.AddAsync(user);
        return CreatedAtAction(nameof(Get), new { id = user.UserID }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> Update(int id, User user)
    {
        if (id != user.UserID)
            return BadRequest();

        await _service.UpdateAsync(user);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var user = (await _service.ReadAllAsync()).FirstOrDefault(u => u.UserID == id);
        if (user == null)
            return NotFound();

        await _service.DeleteAsync(user);
        return Ok(new { message = $"User з UserID {id} успішно видалено" });
    }
}