using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using System.Collections.Generic;
using System.Globalization;
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
            try
            {
                using (var stream = System.IO.File.Create(path))
                {
                    await file.CopyToAsync(stream);
                }

                var expenses = ImportExpenseData(path);
                foreach (var expense in expenses)
                {
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


        private List<ExpenseDto> ImportExpenseData(string file)
        {
            return ImportCSVData<ExpenseDto>(file).ToList();
        }
        private IEnumerable<T> ImportCSVData<T>(string filePath) where T : new()
        {
            var lines = System.IO.File.ReadAllLines(filePath).ToList();
            if (lines.Count == 0)
            {
                throw new InvalidOperationException("The CSV file is empty.");
            }

            var headerLine = lines[0];
            var columnNames = headerLine.Split(',').Select(c => c.Trim().ToLower()).ToList();
            var columns = columnNames.Select((v, i) => new { colIndex = i, colName = v });

            var dataLines = lines.Skip(1);
            var type = typeof(T);
            var list = new List<T>();

            foreach (var row in dataLines)
            {
                var rowValues = row.Split(',').Select(v => v.Trim()).ToList();
                var obj = new T();

                foreach (var prop in type.GetProperties())
                {
                    if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var col = columns.FirstOrDefault(c => c.colName == prop.Name.ToLower());
                    if (col != null)
                    {
                        var colIndex = col.colIndex;
                        var value = rowValues[colIndex];
                        if (!string.IsNullOrEmpty(value))
                        {
                            try
                            {
                                if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                                {
                                    if (DateTime.TryParseExact(value, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
                                    {
                                        prop.SetValue(obj, dateValue);
                                    }
                                }
                                else if (prop.PropertyType.IsEnum)
                                {
                                    if (Enum.TryParse(prop.PropertyType, value, out var enumValue))
                                    {
                                        prop.SetValue(obj, enumValue);
                                    }
                                }
                                else
                                {
                                    prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                                }
                            }
                            catch (Exception ex){}
                        }
                    }
                }

                list.Add(obj);
            }

            return list;
        }

    }
}
