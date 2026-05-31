using Microsoft.AspNetCore.Mvc;
using FreelancePlatformApi.Data;
using Microsoft.EntityFrameworkCore;
using FreelancePlatformApi.Models; // Зверни увагу, щоб простір імен збігався з твоїм
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FreelancePlatformApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProposalsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProposalsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/proposals/user/{name}
        [HttpGet("user/{name}")]
        public async Task<ActionResult<IEnumerable<Proposal>>> GetUserProposals(string name)
        {
            // 1. Спочатку знаходимо фрілансера за його ім'ям (яке прийшло з фронтенду)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
            
            if (user == null) 
            {
                return NotFound("Користувача не знайдено");
            }

            // 2. Дістаємо всі пропозиції цього фрілансера
            // УВАГА: Якщо в твоїй моделі Proposal поле називається FreelancerId замість UserId,
            // просто заміни p.UserId на p.FreelancerId у рядку нижче!
            var proposals = await _context.Proposals
                .Where(p => p.FreelancerId == user.Id) 
                .ToListAsync();

            return Ok(proposals);
        }

        // GET: api/proposals/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Proposal>> GetProposal(int id)
        {
            var proposal = await _context.Proposals
                .Include(p => p.Freelancer)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound("Пропозицію не знайдено");
            return Ok(proposal);
        }

        // POST: api/proposals (Створення нового відгуку)
        [HttpPost]
        public async Task<ActionResult<Proposal>> CreateProposal(Proposal proposal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProposal), new { id = proposal.Id }, proposal);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<Proposal>>> GetOrderProposals(int orderId)
        {
            // Отримуємо пропозиції конкретного замовлення + інформацію про фрілансера
            var proposals = await _context.Proposals
                .Include(p => p.Freelancer) 
                .Where(p => p.OrderId == orderId)
                .ToListAsync();

            return Ok(proposals);
        }
    }
}