using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.Models;
using pfebackend.DTOs;
using pfebackend.Services; // Updated namespace for BudgetDto

namespace pfebackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BudgetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Budgets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetDto>>> GetBudgets()
        {
            return await _context.Budgets
                .Select(b => new BudgetDto
                {
                    Id = b.Id,
                    Category = (DTOs.Category)b.Category,
                    limitValue = b.limitValue,
                    alertValue = b.alertValue,
                    UserId = b.UserId
                })
                .ToListAsync();
        }

        // GET: api/Budgets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetDto>> GetBudget(int id)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id);

            if (budget == null)
            {
                return NotFound();
            }

            return new BudgetDto
            {
                Id = budget.Id,
                Category = (DTOs.Category)budget.Category,
                limitValue = budget.limitValue,
                alertValue = budget.alertValue,
                UserId = budget.UserId
            };
        }
        [HttpGet("getUserBudgetsById/{userId}")]
        public async Task<ActionResult<IEnumerable<BudgetDto>>> GetUserBudgets(string userId)
        {
            var budgets = await _context.Budgets
                            .Where(b => b.UserId == userId)
                            .ToListAsync();

            if (budgets == null || !budgets.Any())
            {
                return Ok(Enumerable.Empty<BudgetDto>());
            }

            return Ok(budgets.Select(b => new BudgetDto
            {
                Id = b.Id,
                Category = (DTOs.Category)b.Category,
                limitValue = b.limitValue,
                alertValue = b.alertValue,
                UserId = b.UserId
            }).ToList());
        }

        // PUT: api/Budgets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBudget(int id, BudgetDto budgetDto)
        {
            if (id != budgetDto.Id)
            {
                return BadRequest();
            }

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }

            budget.Category = (Models.Category)budgetDto.Category;
            budget.limitValue = (float)budgetDto.limitValue;
            budget.alertValue = (float)budgetDto.alertValue;
            budget.UserId = budgetDto.UserId;

            _context.Entry(budget).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BudgetExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Budgets
        [HttpPost]
        public async Task<ActionResult<BudgetDto>> PostBudget(BudgetDto budgetDto)
        {
            var budget = new Budget
            {
                Category = (Models.Category)budgetDto.Category,
                limitValue = budgetDto.limitValue,
                alertValue = budgetDto.alertValue,
                UserId = budgetDto.UserId
            };
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBudget", new { id = budget.Id }, new BudgetDto
            {
                Id = budget.Id,
                Category = (DTOs.Category)budget.Category,
                limitValue = budget.limitValue,
                alertValue = budget.alertValue,
                UserId = budget.UserId
            });
        }

        // DELETE: api/Budgets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.Id == id);
        }
    }
}
