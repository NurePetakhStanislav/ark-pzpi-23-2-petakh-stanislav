using CAaR.Models;
using CAaR.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class GenericController<T> : ControllerBase where T : class, IHasUser, IHasFile
{
    private readonly GenericService<T> _service;
    private readonly GenericService<User>? _userService;
    public List<string> forbids;

    public GenericController(GenericService<T> service, GenericService<User> userService)
    {
        _service = service;
        _userService = userService;
    }

    public class DTO
    {
        public string Json { get; set; }
        public IFormFile File { get; set; }
    }

    public object GetItemID(T item)
    {
        var id = typeof(T).GetProperties().FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));

        if (id == null)
            return BadRequest("В класі відсутня властивість з [Key]");

        var idValue = id.GetValue(item);
        return idValue!;
    }

    private bool AnyFromPatchDoc(JsonPatchDocument<T> patchDoc, string id)
    {
        return patchDoc.Operations.Any(op => op.path.ToLower() == ('/' + id.ToLower()));
    }

    [HttpGet]
    public async Task<ActionResult<List<T>>> GetAll()
    {
        var items = await _service.ReadAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<T>> Get(int id)
    {
        var item = await _service.ReadAsyncByID(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult> Create(T item)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.ReadAsyncByID(item.UserID);
        if (user == null)
            return BadRequest("Даного користувача не існує");
        
        item.User = user;

        var idResult = GetItemID(item);
        if (idResult is BadRequestObjectResult bad)
            return bad;

        await _service.AddAsync(item);
        return CreatedAtAction(nameof(Get), new { id = idResult }, item);
    }

    [HttpPost("upload")]
    public async Task<ActionResult> Create([FromForm] DTO itemUpload, string itemFolder)
    {
        if (itemUpload.File == null)
            return BadRequest("Ви не надали файл");

        if (!itemUpload.Json.Contains("UserID", StringComparison.OrdinalIgnoreCase))
            return NotFound("ID користувача було відсутнє");

        var item = JsonSerializer.Deserialize<T>(itemUpload.Json);

        var user = await _userService.ReadAsyncByID(item.UserID);
        if (user == null)
            return NotFound($"Користувача з ID {item.UserID} не існує");

        var folder = Path.Combine("wwwroot", itemFolder);
        Directory.CreateDirectory(folder);

        var fileName = Guid.NewGuid() + Path.GetExtension(itemUpload.File.FileName);
        var filePath = Path.Combine(folder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await itemUpload.File.CopyToAsync(stream);

        if (item is IHasFile fileItem)
            fileItem.File = fileName;
        item.User = user;

        var idResult = GetItemID(item);
        if (idResult is BadRequestObjectResult bad)
            return bad;

        await _service.AddAsync(item);
        return CreatedAtAction(nameof(Get), new { id = idResult }, item);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<T>> Update(int id, T item)
    {
        var realItem = await _service.ReadAsyncByID(id);
        if (realItem == null)
            return NotFound($"Повідомлення за адресою {id} не існує");

        var prop = typeof(T).GetProperties().FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));
        if (prop != null && prop.CanWrite)
            prop.SetValue(item, id);

        item.UserID = realItem.UserID;
        item.User = realItem.User;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateAsync(item);
        return Ok(item);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<T>> Patch(int id, [FromBody] JsonPatchDocument<T> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest("Патч документ не може бути порожнім.");

        var existingItem = await _service.ReadAsyncByID(id);
        if (existingItem == null)
            return NotFound($"Повідомлення з ID {id} не знайдено.");

        foreach (var forbid in forbids)
        {
            if (AnyFromPatchDoc(patchDoc, forbid))
                return Forbid($"Змінювати {forbid} заборонено.");
        }

        patchDoc.ApplyTo(existingItem);

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateAsync(existingItem);
        return Ok(existingItem);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, bool isNeed, string itemFolder = null)
    {
        var item = await _service.ReadAsyncByID(id);
        if (item == null)
            return NotFound("Даного запиту за ID не існує");

        if (item is IHasFile && !isNeed)
        {
            if (itemFolder == null)
                return BadRequest("Потрібно вказати папку для файлів.");

            var filePath = Path.Combine("wwwroot", itemFolder, item.File);

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не вдалося видалити файл: {ex.Message}");
                }
            }
        }

        await _service.DeleteAsync(item);
        return NoContent();
    }
}