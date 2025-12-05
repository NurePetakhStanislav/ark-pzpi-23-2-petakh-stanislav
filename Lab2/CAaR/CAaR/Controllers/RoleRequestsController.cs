using CAaR.Models;
using CAaR.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

[Route("api/[controller]")]
[ApiController]
public class RoleRequestsController : ControllerBase
{
    private readonly GenericService<RoleRequest> _service;
    private readonly GenericService<User> _userService;
    public class RoleRequestUpload
    {
        public string RoleRequestJson { get; set; }
        public IFormFile Document { get; set; }
    }
    public RoleRequestsController(GenericService<RoleRequest> service, GenericService<User> userService)
    {
        _service = service;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleRequest>>> GetAll()
    {
        var roleRequests = await _service.ReadAllAsync();
        return Ok(roleRequests);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleRequest>> Get(int id)
    {
        var roleRequest = await _service.ReadAsyncByID(id);
        if (roleRequest == null)
            return NotFound($"Даного запиту з ID {id} не існує");

        return Ok(roleRequest);
    }

    [HttpPost("upload")]
    public async Task<ActionResult> Create([FromForm] RoleRequestUpload requestUpload)
    {
        if (requestUpload.RoleRequestJson.Contains("RoleID", StringComparison.OrdinalIgnoreCase))
            return Forbid("Заборонено задавати ID запиту під час створення");

        if (!requestUpload.RoleRequestJson.Contains("UserID", StringComparison.OrdinalIgnoreCase))
            return NotFound("ID користувача було відсутнє");

        var roleRequest = JsonSerializer.Deserialize<RoleRequest>(requestUpload.RoleRequestJson);

        var user = await _userService.ReadAsyncByID(roleRequest.UserID);
        if (user == null)
            return NotFound($"Користувача з ID {roleRequest.UserID} не існує");

        var folder = Path.Combine("wwwroot", "RoleDocs");
        Directory.CreateDirectory(folder);

        var fileName = Guid.NewGuid() + Path.GetExtension(requestUpload.Document.FileName);
        var filePath = Path.Combine(folder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await requestUpload.Document.CopyToAsync(stream);

        roleRequest.File = fileName;
        roleRequest.User = user;

        await _service.AddAsync(roleRequest);
        return CreatedAtAction(nameof(Get), new { id = roleRequest.RoleID }, roleRequest);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var roleRequest = await _service.ReadAsyncByID(id);
        if (roleRequest == null)
            return NotFound("Даного запиту за ID не існує");

        if (!string.IsNullOrEmpty(roleRequest.File))
        {
            var filePath = Path.Combine("wwwroot", "RoleDocs", roleRequest.File);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        await _service.DeleteAsync(roleRequest);
        return NoContent();
    }
}