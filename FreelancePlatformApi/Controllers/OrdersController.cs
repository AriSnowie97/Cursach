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
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
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