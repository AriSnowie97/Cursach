using FreelancePlatformApi.Data;
using FreelancePlatformApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancePlatformApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Подключаем нашу базу данных к контроллеру
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users (Получить всех пользователей)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // POST: api/users (Создать нового пользователя)
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Возвращаем созданного пользователя
            return Ok(user);
        }
    }
}