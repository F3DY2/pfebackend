using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Models;

namespace pfebackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses()
        {
            List<ExpenseDto> expenses = await _context.Expenses
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Category = (DTOs.Category)e.Category,
                    Date = e.Date,
                    Amount = e.Amount,
                    UserId = e.UserId
                })
                .ToListAsync();

            return Ok(expenses);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(int id)
        {
            Expense? expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return NotFound();
            }

            ExpenseDto expenseDto = new ExpenseDto
            {
                Id = expense.Id,
                Name = expense.Name,
                Category = (DTOs.Category)expense.Category,
                Date = expense.Date,
                Amount = expense.Amount,
                UserId = expense.UserId
            };

            return Ok(expenseDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, ExpenseDto expenseDto)
        {
            if (id != expenseDto.Id)
            {
                return BadRequest();
            }

            Expense? expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            expense.Name = expenseDto.Name;
            expense.Category = (Models.Category)expenseDto.Category;
            expense.Date = expenseDto.Date;
            expense.Amount = expenseDto.Amount;
            expense.UserId = expenseDto.UserId;

            _context.Entry(expense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(id))
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> PostExpense(ExpenseDto expenseDto)
        {
            Expense expense = new Expense
            {
                Name = expenseDto.Name,
                Category = (Models.Category)expenseDto.Category,
                Date = expenseDto.Date,
                Amount = expenseDto.Amount,
                UserId = expenseDto.UserId
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            expenseDto.Id = expense.Id;

            return CreatedAtAction("GetExpense", new { id = expenseDto.Id }, expenseDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            Expense? expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}
