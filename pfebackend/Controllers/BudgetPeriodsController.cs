using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetPeriodsController : ControllerBase
    {
        private readonly IBudgetPeriodService _budgetPeriodService;


        public BudgetPeriodsController(IBudgetPeriodService budgetPeriodService)
        {
            _budgetPeriodService = budgetPeriodService;
        }

        // GET: api/BudgetPeriods
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetPeriodDto>>> GetBudgetPeriods()
        {

            List<BudgetPeriodDto> budgetPeriods = await _budgetPeriodService.GetBudgetPeriodsAsync();

            return Ok(budgetPeriods);
        }

        

        // GET: api/BudgetPeriods/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetPeriodDto>> GetBudgetPeriod(int id)
        {
            BudgetPeriodDto? budgetPeriodDto=await  _budgetPeriodService.GetBudgetPeriodAsync(id);
            if (budgetPeriodDto == null) { 
            return NotFound();
            
            }
            return Ok(budgetPeriodDto);
        }

        [HttpGet("getUserBudgetPeriodsById/{userId}")]
        public async Task<ActionResult<IEnumerable<BudgetPeriodDto>>> GetUserBudgetPeriods(string userId)
        {
            IEnumerable<BudgetPeriodDto>? budgetPeriods = await _budgetPeriodService.GetBudgetPeriodsByUserIdAsync(userId);
            return Ok(budgetPeriods);
        }

        // PUT: api/BudgetPeriods/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBudgetPeriod(int id, BudgetPeriodDto budgetPeriodDto)
        {
            if (id != budgetPeriodDto.Id)
            {
                return BadRequest();
            }

            var (success, updatedDto) = await _budgetPeriodService.PutBudgetPeriodAsync(id, budgetPeriodDto);

            if (!success)
            {
                return NotFound();
            }

            return Ok(updatedDto);
        }



        // POST: api/BudgetPeriods
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BudgetPeriodDto>> PostBudgetPeriod(BudgetPeriodDto budgetPeriodDto)
        {
            var (success, message, result) = await _budgetPeriodService.PostBudgetPeriodAsync(budgetPeriodDto);

            if (!success)
            {
                return BadRequest(message);
            }

            return Ok(budgetPeriodDto);
        }



        // DELETE: api/BudgetPeriods/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudgetPeriod(int id)
        {

            bool isDeleted= await _budgetPeriodService.DeleteBudgetPeriodAsync(id);

            if (isDeleted == false) { return BadRequest(); }

            return NoContent();
        }

        
    }
}
