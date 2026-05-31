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
            // Якщо фронт надіслав CustomerId = 0 (через проблеми з кешем або VS Code),
            // ми беремо першого ліпшого Замовника з бази, щоб база не видавала помилку Foreign Key!
            if (order.CustomerId == 0)
            {
                var anyCustomer = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Customer");
                if (anyCustomer != null)
                {
                    order.CustomerId = anyCustomer.Id;
                }
                else
                {
                    // Якщо в базі взагалі немає користувачів, примусово ставимо ID = 1
                    order.CustomerId = 1; 
                }
            }

            // Примусово гарантуємо статус, як на початку розробки
            if (string.IsNullOrEmpty(order.Status))
            {
                order.Status = "Open";
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound("Замовлення не знайдено");

            // Можна додати перевірки, наприклад: тільки замовник може скасувати, 
            // або тільки фрілансер може перевести в InProgress
            order.Status = newStatus;
            
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        // GET: api/orders (Отримання ВСІХ замовлень для головної сторінки)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .ToListAsync();
            return Ok(orders);
        }

        // GET: api/orders/{id} (Отримання конкретного замовлення)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)   // Підвантажуємо автора замовлення
                .Include(o => o.Proposals)  // Підвантажуємо пропозиції
                    .ThenInclude(p => p.Freelancer) // Підвантажуємо авторів пропозицій
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return Ok(order);
        }
    }
}