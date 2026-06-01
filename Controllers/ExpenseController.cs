using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxAccount.Services;
using TaxAccount.DTOs;

namespace TaxAccount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(
            IExpenseService expenseService,
            ILogger<ExpenseController> logger)
        {
            _expenseService = expenseService;
            _logger = logger;
        }

        /// <summary>Get all expenses for the tenant</summary>
        [HttpGet]
        public async Task<IActionResult> GetExpenses(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var expenses = await _expenseService.GetExpensesAsync(fromDate, toDate);
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expenses");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get expense by ID</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpense(int id)
        {
            try
            {
                var expense = await _expenseService.GetExpenseAsync(id);
                return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expense");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Create new expense</summary>
        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
        {
            try
            {
                var expense = await _expenseService.CreateExpenseAsync(dto);
                return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Update expense</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseDto dto)
        {
            try
            {
                var expense = await _expenseService.UpdateExpenseAsync(id, dto);
                return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expense");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Delete expense</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                var result = await _expenseService.DeleteExpenseAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get total expenses for a date range</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetExpenseSummary(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var total = await _expenseService.GetTotalExpensesAsync(fromDate, toDate);
                return Ok(new { totalExpenses = total, fromDate, toDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating expense summary");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
