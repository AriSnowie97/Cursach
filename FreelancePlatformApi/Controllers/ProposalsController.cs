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

        // POST: api/proposals (одразу додаємо метод для створення нових відгуків на майбутнє!)
        [HttpPost]
        public async Task<ActionResult<Proposal>> CreateProposal(Proposal proposal)
        {
            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync();
            return Ok(proposal);
        }
    }
}