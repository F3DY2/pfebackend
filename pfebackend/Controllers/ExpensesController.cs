using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Services;

namespace pfebackend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ICsvImportService _csvImportService;
        private readonly IUserService _userService;

        public ExpensesController(IExpenseService expenseService, ICsvImportService csvImportService, IUserService userService)
        {
            _expenseService = expenseService;
            _csvImportService = csvImportService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses()
        {
            IEnumerable<ExpenseDto> expenses = await _expenseService.GetExpensesAsync();
            return Ok(expenses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(int id)
        {
            ExpenseDto? expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null)
            {
                return NotFound(new { Message = $"Expense with ID {id} not found." });
            }

            return Ok(expense);
        }
        [HttpGet("getUserExpensesById/{userId}")]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetUserExpenses(string userId)
        {
            IEnumerable<ExpenseDto>? expenses = await _expenseService.GetExpensesByUserIdAsync(userId);
            return Ok(expenses);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, ExpenseDto expenseDto)
        {
            (bool updated, string message) = await _expenseService.UpdateExpenseAsync(id, expenseDto);
            if (!updated)
            {
                return NotFound(new { Message = message });
            }

            return Ok(new { Message = message });
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> PostExpense(ExpenseDto expenseDto)
        {
            (bool success, string message, ExpenseDto createdExpenseDto) = await _expenseService.CreateExpenseAsync(expenseDto);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(createdExpenseDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            bool deleted = await _expenseService.DeleteExpenseAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Expense with ID {id} not found. Deletion failed." });
            }

            return NoContent();
        }

        [HttpPost("import-csv")]
        public async Task<IActionResult> ImportCSVFile(IFormFile file)
        {
            string path = "uploads/tempcsv.csv";
            try
            {
                using (var stream = System.IO.File.Create(path))
                {
                    await file.CopyToAsync(stream);
                }

                var userId = _userService.GetCurrentUserId();

                var expenses = await _csvImportService.ImportExpensesFromCsvAsync(path);

                foreach (var expense in expenses)
                {
                    expense.UserId = userId;
                    await _expenseService.CreateExpenseAsync(expense);
                }

                return Ok(new { ImportResult = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ImportResult = false, Error = ex.Message });
            }
            finally
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }
    }
}
