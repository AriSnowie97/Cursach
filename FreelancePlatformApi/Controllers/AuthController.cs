using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancePlatformApi.Data; // Підключаємо твій контекст БД
// using FreelancePlatformApi.Models; // Розкоментуй, якщо юзер лежить в папці Models

namespace FreelancePlatformApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        // "Інжектимо" підключення до бази даних
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email та пароль обов'язкові.");
            }

            // Йдемо в БД і шукаємо реального користувача
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

            if (user != null)
            {
                // Якщо знайшли — повертаємо його РЕАЛЬНІ дані
                return Ok(new 
                { 
                    Name = user.Name,       // УВАГА: перевір, як точно називається поле в твоїй моделі (Name чи FirstName)
                    LastName = user.LastName, 
                    Role = user.Role 
                });
            }

            // Якщо такого в базі немає або пароль не підійшов
            return Unauthorized("Невірний email або пароль");
        }
    }

    // Модель того, що прилітає з фронтенду
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}