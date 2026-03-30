using Microsoft.AspNetCore.Mvc;

namespace FreelancePlatformApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        // GET: api/ping
        [HttpGet]
        public IActionResult WakeUp()
        {
            // Цей метод взагалі не чіпає базу даних, просто каже Railway: "Я не сплю!"
            return Ok(new { message = "Сервер прокинувся і готовий до роботи!" });
        }
    }
}