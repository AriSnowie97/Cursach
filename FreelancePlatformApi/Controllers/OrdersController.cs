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

        // PUT: api/orders/{id}/accept-proposal/{proposalId}
        [HttpPut("{id}/accept-proposal/{proposalId}")]
        public async Task<IActionResult> AcceptProposal(int id, int proposalId)
        {
            var order = await _context.Orders
                .Include(o => o.Proposals)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null) return NotFound("Замовлення не знайдено");

            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null) return NotFound("Пропозицію не знайдено");

            if (proposal.OrderId != id) return BadRequest("Ця пропозиція не відноситься до даного замовлення");

            order.FreelancerId = proposal.FreelancerId;
            order.Status = "InProgress";

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
                .Include(o => o.Freelancer)
                .ToListAsync();
            return Ok(orders);
        }

        // GET: api/orders/{id} (Отримання конкретного замовлення)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)   // Підвантажуємо автора замовлення
                .Include(o => o.Freelancer) // Підвантажуємо виконавця
                .Include(o => o.Proposals)  // Підвантажуємо пропозиції
                    .ThenInclude(p => p.Freelancer) // Підвантажуємо авторів пропозицій
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // АВТО-ВИПРАВЛЕННЯ ДЛЯ СТАРИХ ЗАМОВЛЕНЬ:
            // Якщо статус InProgress, але виконавець не був збережений в БД (через стару версію коду),
            // ми автоматично призначаємо фрілансера, який залишив пропозицію.
            if (order.Status == "InProgress" && order.FreelancerId == null && order.Proposals.Any())
            {
                var firstProposal = order.Proposals.First();
                order.FreelancerId = firstProposal.FreelancerId;
                await _context.SaveChangesAsync();
                
                // Перезавантажуємо замовлення з оновленою властивістю Freelancer
                order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Freelancer)
                    .Include(o => o.Proposals)
                        .ThenInclude(p => p.Freelancer)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }

            return Ok(order);
        }
    }
}