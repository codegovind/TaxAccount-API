using Microsoft.AspNetCore.Mvc;
using TaxAccount.Models;
using TaxAccount.Services;
using TaxAccount.Models.Dtos;

namespace TaxAccount.Controllers;

[ApiController]
[Route("api/accounting")]
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
            var tenantId = User.FindFirst("tenant_id")?.Value ?? "default";
            
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
                        toDate = today;
                        break;
                    case "yearly":
                        fromDate = new DateTime(today.Year, 1, 1);
                        toDate = today;
                        break;
                    default:
                        fromDate = new DateTime(today.Year, today.Month, 1);
                        toDate = today;
                        break;
                }
            }

            var result = await _cashFlowService.CalculateCashFlowAsync(
                tenantId,
                method.ToLower() == "indirect" ? CashFlowMethod.Indirect : CashFlowMethod.Direct,
                fromDate.Value,
                toDate.Value);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cash flow statement");
            return StatusCode(500, new { error = "Failed to generate cash flow statement", details = ex.Message });
        }
    }

    /// <summary>
    /// Get drill-down details for a specific cash flow line item
    /// </summary>
    [HttpGet("cashflow/drilldown")]
    public async Task<ActionResult<List<TransactionDetailDto>>> GetCashFlowDrillDown(
        [FromQuery] string activityType,
        [FromQuery] string lineItem,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var tenantId = User.FindFirst("tenant_id")?.Value ?? "default";
            
            var result = await _cashFlowService.GetTransactionDetailsAsync(
                tenantId,
                activityType,
                lineItem,
                fromDate,
                toDate);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drill-down details");
            return StatusCode(500, new { error = "Failed to get transaction details", details = ex.Message });
        }
    }

    /// <summary>
    /// Export Cash Flow to Excel
    /// </summary>
    [HttpGet("cashflow/export/excel")]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] string method = "direct",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var tenantId = User.FindFirst("tenant_id")?.Value ?? "default";
            var bytes = await _cashFlowService.ExportToExcelAsync(tenantId, method, fromDate, toDate);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CashFlow.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to Excel");
            return StatusCode(500, new { error = "Failed to export to Excel", details = ex.Message });
        }
    }

    /// <summary>
    /// Export Cash Flow to PDF
    /// </summary>
    [HttpGet("cashflow/export/pdf")]
    public async Task<IActionResult> ExportToPdf(
        [FromQuery] string method = "direct",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var tenantId = User.FindFirst("tenant_id")?.Value ?? "default";
            var bytes = await _cashFlowService.ExportToPdfAsync(tenantId, method, fromDate, toDate);
            return File(bytes, "application/pdf", "CashFlow.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to PDF");
            return StatusCode(500, new { error = "Failed to export to PDF", details = ex.Message });
        }
    }
}
