using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancePlatformApi.Data;
using FreelancePlatformApi.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Шукаємо користувача в базі PostgreSQL за поштою та паролем
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Невірний логін або пароль" });
        }

        // Повертаємо ім'я конкретного користувача (Аріна, Артем тощо)
        return Ok(new { name = user.Name, email = user.Email, role = user.Role });
    }
}

// Допоміжний клас для запиту
public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}