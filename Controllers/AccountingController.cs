using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxAccount.DTOs.Accounting;
using TaxAccount.Services;

namespace TaxAccount.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountingController : ControllerBase
{
    private readonly IAccountingService _accountingService;

    public AccountingController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    /// <summary>
    /// Get Chart of Accounts as a hierarchical tree
    /// </summary>
    [HttpGet("chart-of-accounts")]
    public async Task<ActionResult<List<AccountHeadDto>>> GetChartOfAccounts()
    {
        var accounts = await _accountingService.GetChartOfAccountsAsync();
        return Ok(accounts);
    }

    /// <summary>
    /// Create a new Account Head
    /// </summary>
    [HttpPost("accounts")]
    public async Task<ActionResult<AccountHeadDto>> CreateAccount([FromBody] CreateAccountHeadDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var account = await _accountingService.CreateAccountHeadAsync(dto);
        return CreatedAtAction(nameof(GetChartOfAccounts), new { }, account);
    }

    /// <summary>
    /// Post a manual journal entry to the General Ledger
    /// </summary>
    [HttpPost("ledger")]
    public async Task<ActionResult<LedgerEntryDto>> PostTransaction([FromBody] LedgerEntryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entry = await _accountingService.PostTransactionAsync(dto);
        return Ok(entry);
    }

    /// <summary>
    /// Get Trial Balance for a date range
    /// </summary>
    [HttpGet("trial-balance")]
    public async Task<ActionResult<List<TrialBalanceDto>>> GetTrialBalance(
        [FromQuery] DateTime fromDate, 
        [FromQuery] DateTime toDate)
    {
        var trialBalance = await _accountingService.GetTrialBalanceAsync(fromDate, toDate);
        return Ok(trialBalance);
    }

    /// <summary>
    /// Get Profit & Loss Statement
    /// </summary>
    [HttpGet("profit-loss")]
    public async Task<ActionResult<List<FinancialStatementDto>>> GetProfitLoss(
        [FromQuery] DateTime fromDate, 
        [FromQuery] DateTime toDate)
    {
        var result = await _accountingService.GetProfitLossAsync(fromDate, toDate);
        return Ok(result);
    }

    /// <summary>
    /// Get Balance Sheet as of a specific date
    /// </summary>
    [HttpGet("balance-sheet")]
    public async Task<ActionResult<List<FinancialStatementDto>>> GetBalanceSheet(
        [FromQuery] DateTime asOfDate)
    {
        var result = await _accountingService.GetBalanceSheetAsync(asOfDate);
        return Ok(result);
    }
}
