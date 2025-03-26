using Microsoft.AspNetCore.Mvc;
using pfebackend.DTOs;
using pfebackend.Interfaces;

namespace pfebackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetsController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        // GET: api/Budgets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetDto>>> GetBudgets()
        {
            IEnumerable<BudgetDto> budgets = await _budgetService.GetAllBudget();
            return Ok(budgets);
        }



        // GET: api/Budgets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetDto>> GetBudget(int id)
        {
            BudgetDto budget = await _budgetService.GetBudgetById(id);
            if (budget == null)
            {
                return NotFound(new { message = "No budgets found for the specified id." });
            }
            return Ok(budget);
        }


        [HttpGet("getUserBudgetsById/{userId}")]
        public async Task<ActionResult<IEnumerable<BudgetDto>>> GetUserBudgets(string userId)
        {
            IEnumerable<BudgetDto> budget = await _budgetService.GetUserBudgetsByUserId(userId);
            if (budget == null)
            {
                return NotFound(new { message = "No budgets found for the specified user." });
            }
            return Ok(budget);
        }


        // PUT: api/Budgets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBudget(int id, BudgetDto budgetDto)
        {
            bool isOverlap = await _budgetService.CheckBudgetOverlap(budgetDto);

            if (isOverlap)
            {
                return BadRequest(new { message = "Cannot add budget for similar category at the same date range." });
            }

            var (isUpdated, message) = await _budgetService.UpdateBudget(id, budgetDto);
            if (!isUpdated)
            {
                return NotFound(new { message });
            }
            return Ok(new { message });
        }



        // POST: api/Budgets
        [HttpPost]
        public async Task<ActionResult<BudgetDto>> PostBudget(BudgetDto budgetDto)
        {
            BudgetDto budget = await _budgetService.CreateBudget(budgetDto);
            bool isOverlap = await _budgetService.CheckBudgetOverlap(budgetDto);

            if (budget == null)
            {
                return NotFound(new { message = "Budget creation failed. The provided data might be invalid or incomplete." });
            }
            if (isOverlap)
            {
                return BadRequest(new { message = "Cannot add budget for similar category at the same date range." });
            }
            return Ok(budget);
        }

        // DELETE: api/Budgets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            bool isDeleted = await _budgetService.RemoveBudget(id);
            if (!isDeleted)
            {
                return NotFound(new { message = $"Budget with ID {id} not found. Deletion failed." });
            }
            return NoContent();
        }

    }
}
