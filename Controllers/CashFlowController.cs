using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxAccount.Models.Dtos;
using TaxAccount.Services;

namespace TaxAccount.Controllers;

[ApiController]
[Route("api/accounting")]
[Authorize] // Ensures only authenticated users can access
public class CashFlowController : ControllerBase
{
    private readonly CashFlowService _cashFlowService;
    private readonly ILogger<CashFlowController> _logger;

    public CashFlowController(CashFlowService cashFlowService, ILogger<CashFlowController> logger)
    {
        _cashFlowService = cashFlowService;
        _logger = logger;
    }

    /// <summary>
    /// Get Cash Flow Statement (Direct or Indirect Method)
    /// </summary>
    [HttpGet("cashflow")]
    public async Task<ActionResult<CashFlowStatementDto>> GetCashFlow(
        [FromQuery] string method = "direct",
        [FromQuery] string period = "monthly",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            // Restore Tenant ID from Auth Claim
            var tenantClaim = User.FindFirst("tenant_id");
            if (tenantClaim == null || !int.TryParse(tenantClaim.Value, out int tenantId))
            {
                return Unauthorized(new { error = "Invalid or missing tenant_id claim" });
            }

            // Calculate date range if not provided
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                var today = DateTime.Today;
                switch (period.ToLower())
                {
                    case "monthly":
                        fromDate = new DateTime(today.Year, today.Month, 1);
                        toDate = today;
                        break;
                    case "quarterly":
                        var quarter = (today.Month - 1) / 3;
                        fromDate = new DateTime(today.Year, quarter * 3 + 1, 1);
                        toDate = fromDate.Value.AddMonths(3).AddDays(-1);
                        break;
                    case "yearly":
                        fromDate = new DateTime(today.Year, 1, 1);
                        toDate = new DateTime(today.Year, 12, 31);
                        break;
                    default:
                        fromDate = new DateTime(today.Year, today.Month, 1);
                        toDate = today;
                        break;
                }
            }

            CashFlowStatementDto result;
            if (method.ToLower() == "indirect")
            {
                result = await _cashFlowService.CalculateIndirectMethodAsync(fromDate.Value, toDate.Value, tenantId);
            }
            else
            {
                result = await _cashFlowService.CalculateDirectMethodAsync(fromDate.Value, toDate.Value, tenantId);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cash flow statement for Tenant");
            return StatusCode(500, new { error = "Failed to generate cash flow statement", details = ex.Message });
        }
    }

    /// <summary>
    /// Get drill-down details for a specific account head
    /// </summary>
    [HttpGet("cashflow/drilldown")]
    public async Task<ActionResult<List<TransactionDetailDto>>> GetCashFlowDrillDown(
        [FromQuery] int accountHeadId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var tenantClaim = User.FindFirst("tenant_id");
            if (tenantClaim == null || !int.TryParse(tenantClaim.Value, out int tenantId))
            {
                return Unauthorized(new { error = "Invalid or missing tenant_id claim" });
            }

            var result = await _cashFlowService.GetTransactionDetailsAsync(accountHeadId, fromDate, toDate, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drill-down details");
            return StatusCode(500, new { error = "Failed to get transaction details", details = ex.Message });
        }
    }
    
    // Note: Export endpoints (Excel/PDF) removed temporarily until 
    // corresponding methods are added to CashFlowService to avoid build errors.
}