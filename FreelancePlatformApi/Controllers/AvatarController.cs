using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancePlatformApi.Data;

namespace FreelancePlatformApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvatarController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AvatarController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/avatar/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAvatar(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Користувача не знайдено");
            
            return Ok(new { AvatarUrl = user.AvatarUrl });
        }

        // POST: api/avatar/{userId}
        // Body: { "avatarBase64": "data:image/png;base64,..." }
        [HttpPost("{userId}")]
        public async Task<IActionResult> UploadAvatar(int userId, [FromBody] AvatarUploadRequest request)
        {
            if (string.IsNullOrEmpty(request.AvatarBase64))
                return BadRequest("Аватарка не може бути пустою");

            // Обмеження розміру — base64 рядок не більше 500KB (приблизно 375KB зображення)
            if (request.AvatarBase64.Length > 524288)
                return BadRequest("Зображення занадто велике. Максимальний розмір — 375KB.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Користувача не знайдено");

            user.AvatarUrl = request.AvatarBase64;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Аватарку успішно оновлено", AvatarUrl = user.AvatarUrl });
        }

        public class AvatarUploadRequest
        {
            public string AvatarBase64 { get; set; } = "";
        }
    }
}
