using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxAccount.Services;
using TaxAccount.Models.Accounting;

namespace TaxAccount.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountingController : ControllerBase
{
    private readonly IAccountingService _accountingService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<AccountingController> _logger;

    public AccountingController(IAccountingService accountingService, ITenantService tenantService, ILogger<AccountingController> logger)
    {
        _accountingService = accountingService;
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet("chart-of-accounts")]
    public async Task<IActionResult> GetChartOfAccounts()
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var accounts = await _accountingService.GetChartOfAccountsAsync(tenantId);
        return Ok(accounts);
    }

    [HttpPost("chart-of-accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] AccountHead account)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        account.TenantId = tenantId;
        var created = await _accountingService.CreateAccountHeadAsync(account);
        return CreatedAtAction(nameof(GetChartOfAccounts), new { id = created.Id }, created);
    }

    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedger(
        [FromQuery] int? accountHeadId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var entries = await _accountingService.GetGeneralLedgerAsync(tenantId, accountHeadId, fromDate, toDate);
        return Ok(entries);
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var result = await _accountingService.GetTrialBalanceAsync(tenantId, fromDate, toDate);
        return Ok(result);
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var result = await _accountingService.GetBalanceSheetAsync(tenantId, asOfDate);
        return Ok(result);
    }

    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitLoss(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var result = await _accountingService.GetProfitLossAsync(tenantId, fromDate, toDate);
        return Ok(result);
    }
}
