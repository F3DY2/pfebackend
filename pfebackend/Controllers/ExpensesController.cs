using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pfebackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses()
        {
            IEnumerable<ExpenseDto> expenses = await _expenseService.GetExpensesAsync();
            return Ok(expenses);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(int id)
        {
            ExpenseDto? expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            return Ok(expense);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, ExpenseDto expenseDto)
        {
            bool updated = await _expenseService.UpdateExpenseAsync(id, expenseDto);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> PostExpense(ExpenseDto expenseDto)
        {
            var result = await _expenseService.CreateExpenseAsync(expenseDto);

            if (result == null)
            {
                if (!await _expenseService.UserExists(expenseDto.UserId))
                {
                    return NotFound(new { message = "User not found." });
                }

                return BadRequest(new { message = "Amount must be greater than zero." });
            }

            return result;
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            bool deleted = await _expenseService.DeleteExpenseAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
