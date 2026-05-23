using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancePlatformApi.Data;
using FreelancePlatformApi.Models;

namespace FreelancePlatformApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/orders (Створення нового замовлення)
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            // Тимчасовий лог для перевірки: виведе в консоль Railway реальний CustomerId
            Console.WriteLine($"=== СТВОРЕННЯ ЗАМОВЛЕННЯ: Прийшов CustomerId = {order.CustomerId} ===");

            if (order.CustomerId == 0)
            {
                return BadRequest("Помилка: CustomerId не може бути 0. Користувач не авторизований належним чином.");
            }

            if (string.IsNullOrEmpty(order.Status))
            {
                order.Status = "Open";
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // GET: api/orders (Отримання ВСІХ замовлень для головної сторінки)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }
    }
}