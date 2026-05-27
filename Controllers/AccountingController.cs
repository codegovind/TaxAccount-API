using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxAccount.Services;

namespace TaxAccount.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountingController : ControllerBase
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<AccountingController> _logger;

    public AccountingController(IAccountingService accountingService, ILogger<AccountingController> logger)
    {
        _accountingService = accountingService;
        _logger = logger;
    }

    [HttpGet("chart-of-accounts")]
    public async Task<IActionResult> GetChartOfAccounts()
    {
        var accounts = await _accountingService.GetChartOfAccountsAsync();
        return Ok(accounts);
    }

    [HttpPost("chart-of-accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] Models.Accounting.AccountHead account)
    {
        var created = await _accountingService.CreateAccountHeadAsync(account);
        return CreatedAtAction(nameof(GetChartOfAccounts), new { id = created.Id }, created);
    }

    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedger(
        [FromQuery] int? accountHeadId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var entries = await _accountingService.GetGeneralLedgerAsync(accountHeadId, fromDate, toDate);
        return Ok(entries);
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        var result = await _accountingService.GetTrialBalanceAsync(fromDate, toDate);
        return Ok(result);
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate)
    {
        var result = await _accountingService.GetBalanceSheetAsync(asOfDate);
        return Ok(result);
    }

    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitLoss(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        var result = await _accountingService.GetProfitLossAsync(fromDate, toDate);
        return Ok(result);
    }
}
