using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancePlatformApi.Data; // Підключаємо твій контекст БД
using FreelancePlatformApi.Models; // Розкоментував, щоб система бачила клас User (перевір чи він лежить в цій папці)

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

        // --- МЕТОД ДЛЯ ВХОДУ (ЛОГІН) ---
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
                    Id = user.Id,
                    Name = user.Name,       // УВАГА: перевір, як точно називається поле в твоїй моделі (Name чи FirstName)
                    LastName = user.LastName, 
                    Role = user.Role 
                });
            }

            // Якщо такого в базі немає або пароль не підійшов
            return Unauthorized("Невірний email або пароль");
        }

        // --- НОВИЙ МЕТОД ДЛЯ РЕЄСТРАЦІЇ ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email та пароль обов'язкові.");
            }

            // 1. Перевіряємо, чи немає вже такого email в базі
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest("Користувач з таким email вже існує!");
            }

            // 2. Створюємо нового користувача
            var newUser = new User // УВАГА: переконайся, що таблиця/клас в БД називається саме User
            {
                Name = request.Name,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password, // Для курсача підійде, в реальному житті паролі хешують
                Role = request.Role
            };

            // 3. Зберігаємо в базу PostgreSQL
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 4. Повертаємо дані назад на фронт, щоб відразу авторизувати користувача
            return Ok(new 
            { 
                Id = newUser.Id,
                Name = newUser.Name, 
                LastName = newUser.LastName, 
                Role = newUser.Role 
            });
        }
    }

    // Модель того, що прилітає з фронтенду при логіні
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    // Модель того, що прилітає з фронтенду при реєстрації
    public class RegisterRequest
    {
        public string Name { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Customer"; // За замовчуванням
    }
}