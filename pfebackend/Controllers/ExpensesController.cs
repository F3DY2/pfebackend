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
        [Authorize]
        [HttpPost("import-csv")]
        public async Task<IActionResult> ImportCSVFile(IFormFile file)
        {
            string path = "uploads/tempcsv.csv";
            using (var stream = System.IO.File.Create(path))
            {
                await file.CopyToAsync(stream);
            }
            var expenses = ImportExpenseData(path);
            foreach(var expense in expenses)
            {
                PostExpense(expense);
            }
                return Ok(new {ImportResult=true});
        }

        private List<ExpenseDto> ImportExpenseData(string file)
        {
            return ImportCSVData<ExpenseDto>(file).ToList();
        }
        private IEnumerable<T> ImportCSVData<T>(string filePath)
        {
            List<T> list = new List<T>();

            List<string> lines = System.IO.File.ReadAllLines(filePath).ToList();
            string headerLine = lines[0];
            var columnNames = headerLine.Split(',');
            var columns = columnNames.Select((v, i) => new { colIndex = i, colName = v });

            var dataLines = lines.Skip(1);
            Type type = typeof(T);

            foreach(var row in dataLines)
            {
                var rowValues = row.Split(',').ToList();
                var obj = (T?)Activator.CreateInstance(type);
                foreach(var prop in type.GetProperties())
                {
                    var col = columns.Single(c => c.colName.ToLower() == prop.Name.ToLower());
                    var colIndex = col.colIndex;
                    var value = rowValues[colIndex];
                    prop.SetValue(obj, Convert.ChangeType(value,prop.PropertyType));
                }
                if (obj != null)
                {
                    list.Add(obj);
                }
                
            }
            return list;
        }
    }
}
