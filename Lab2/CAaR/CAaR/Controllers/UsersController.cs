using CAaR.Models;
using CAaR.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static UsersController;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly GenericService<User> _service;

    public class FileDTO
    {
        public IFormFile file { get; set; }
    }

    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    private bool AnyFromPatchDoc(JsonPatchDocument<User> patchDoc, string id)
    {
        return patchDoc.Operations.Any(op => op.path.ToLower() == ('/' + id.ToLower()));
    }

    private static readonly Dictionary<string, byte[]> _fileSignatures = new()
    {
        { "image/png",  new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { "image/gif",  new byte[] { 0x47, 0x49, 0x46, 0x38 } },
        { "image/webp", new byte[] { 0x52, 0x49, 0x46, 0x46 } }
    };

    private bool IsValidImage(IFormFile file)
    {
        if (!_fileSignatures.ContainsKey(file.ContentType))
            return false;

        using var reader = new BinaryReader(file.OpenReadStream());

        if (file.ContentType == "image/webp")
        {
            var header = reader.ReadBytes(12);
            return header.Take(4).SequenceEqual(_fileSignatures["image/webp"]) &&
                   header.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 });
        }
        else
        {
            var headerBytes = reader.ReadBytes(_fileSignatures[file.ContentType].Length);
            return headerBytes.SequenceEqual(_fileSignatures[file.ContentType]);
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
        user.ProfilePicture = null;

        await _service.AddAsync(user);
        return CreatedAtAction(nameof(Get), new { id = user.UserID }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> Update(int id, User user)
    {
        var existingUser = await _service.ReadAsyncByID(id);
        if (existingUser == null)
            return BadRequest("Даний користувач відсутній");

        existingUser.UserName = user.UserName;
        existingUser.Password = HashPassword(user.Password);
        existingUser.Email = user.Email;
        existingUser.Role = user.Role;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateAsync(existingUser);
        return Ok(existingUser);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<User>> Patch(int id, [FromBody] JsonPatchDocument<User> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest("Патч документ не може бути порожнім.");

        var existingUser = await _service.ReadAsyncByID(id);
        if (existingUser == null)
            return NotFound($"Користувач з ID {id} не знайдений.");

        if (AnyFromPatchDoc(patchDoc, "userid"))
            return Forbid("Змінювати UserID заборонено.");

        if (AnyFromPatchDoc(patchDoc, "ProfilePicture"))
            return Forbid("Змінювати файли заборонено для цього методу. Використайте окремий API.");

        patchDoc.ApplyTo(existingUser);

        if (AnyFromPatchDoc(patchDoc, "password"))
            existingUser.Password = HashPassword(existingUser.Password);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateAsync(existingUser);
        return Ok(existingUser);
    }

    [HttpPatch("{id}/image")]
    public async Task<ActionResult<User>> Patch(int id, [FromForm] FileDTO file)
    {
        if (file == null || file.file.Length == 0)
            return BadRequest("Файл не може бути порожнім.");

        var user = await _service.ReadAsyncByID(id);
        if (user == null)
            return NotFound("Такого користувача не існує.");

        if (!IsValidImage(file.file))
            return BadRequest("Невірний формат файлу. Дозволено тільки PNG, JPG, GIF та WEBP.");

        var folder = Path.Combine("wwwroot", "UserImages");
        Directory.CreateDirectory(folder);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.file.FileName);
        var filePath = Path.Combine(folder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.file.CopyToAsync(stream);

        if (user.ProfilePicture != null)
        {
            var oldfilePath = Path.Combine("wwwroot", "UserImages", user.ProfilePicture);
            if (System.IO.File.Exists(oldfilePath))
                System.IO.File.Delete(oldfilePath);
        }

        user.ProfilePicture = fileName;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateAsync(user);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var user = await _service.ReadAsyncByID(id);
        if (user == null)
            return NotFound("Даного користувача за ID не існує");

        if (user.ProfilePicture != null) {
            var filePath = Path.Combine("wwwroot", "UserImages", user.ProfilePicture);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
        
        await _service.DeleteAsync(user);
        return NoContent();
    }
}