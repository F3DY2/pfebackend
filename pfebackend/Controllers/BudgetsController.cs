using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;

//using pfebackend.Interfaces;
using pfebackend.Models;
using pfebackend.Services;

namespace pfebackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetsController : ControllerBase
    {
        
        private readonly IBudgetService _budgetService;

        public BudgetsController(AppDbContext context, IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        // GET: api/Budgets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetDto>>> GetBudgets()
        {
            IEnumerable<BudgetDto> budgets = await _budgetService.GetBudgetsAsync();

            return Ok(budgets);
            
        }

        

        // GET: api/Budgets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetDto>> GetBudget(int id)
        {
            BudgetDto? budgetDto = await _budgetService.GetBudgetAsync(id);
            if (budgetDto == null)
            {
                return NotFound();

            }
            return Ok(budgetDto);
        }

       

        // PUT: api/Budgets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBudget(int id, BudgetDto budgetDto)
        {
            if (id != budgetDto.Id)
            {
                return BadRequest();
            }
            bool isUpdated= await _budgetService.PutBudgetAsync(id, budgetDto);
            if (!isUpdated) {
                return BadRequest();
            }
            return NoContent();
        }

        

        // POST: api/Budgets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BudgetDto>> PostBudget(BudgetDto budgetDto)
        {
            (bool isCreated, BudgetDto createdBudgetDto) = await _budgetService.PostBudgetAsync(budgetDto);
            if (isCreated == false)

            { return BadRequest(); }


            return Ok(budgetDto);
        }

        

        // DELETE: api/Budgets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            bool isDeleted = await _budgetService.DeleteBudgetAsync(id);

            if (isDeleted == false) { return BadRequest(); }

            return NoContent();
        }

        

    }
}
