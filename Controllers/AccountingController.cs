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
        var tenantId = _tenantService.GetTenantId();
        var accounts = await _accountingService.GetChartOfAccountsAsync(tenantId);
        return Ok(accounts);
    }

    [HttpPost("chart-of-accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] AccountHead account)
    {
        // var tenantId = _tenantService.GetTenantId();
        // account.TenantId = tenantId;
        
        // // TODO: Implement CreateAccountHeadAsync in AccountingService
        // // For now, return not implemented
        // return StatusCode(501, "Account creation endpoint is under development. Please use the UI or seed data for now.");
        if (!ModelState.IsValid)
        return BadRequest(ModelState);

        try
        {
            var tenantId = _tenantService.GetTenantId();
            account.TenantId = tenantId;
            
            var created = await _accountingService.CreateAccountHeadAsync(account);
            
            return CreatedAtAction(nameof(GetChartOfAccounts), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating account", error = ex.Message });
        }
    }

    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedger(
        [FromQuery] int? accountHeadId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = _tenantService.GetTenantId();
        var entries = await _accountingService.GetGeneralLedgerAsync(tenantId, accountHeadId, fromDate, toDate);
        return Ok(entries);
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        var tenantId = _tenantService.GetTenantId();
        var result = await _accountingService.GetTrialBalanceAsync(tenantId, fromDate, toDate);
        return Ok(result);
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate)
    {
        var tenantId = _tenantService.GetTenantId();
        var result = await _accountingService.GetBalanceSheetAsync(tenantId, asOfDate);
        return Ok(result);
    }

    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitLoss(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        var tenantId = _tenantService.GetTenantId();
        var result = await _accountingService.GetProfitLossAsync(tenantId, fromDate, toDate);
        return Ok(result);
    }

    [HttpPost("voucher")]
    public async Task<IActionResult> CreateVoucher([FromBody] VoucherEntry[] entries)
    {
        if (!ModelState.IsValid || entries == null || entries.Length == 0)
            return BadRequest(new { message = "Invalid voucher entries" });

        try
        {
            var tenantId = _tenantService.GetTenantId();
            
            // Validate that debits equal credits
            var totalDebit = entries.Sum(e => e.Debit);
            var totalCredit = entries.Sum(e => e.Credit);
            
            if (Math.Abs(totalDebit - totalCredit) > 0.01m)
                return BadRequest(new { message = "Voucher is not balanced. Debits must equal credits." });

            // Set tenant ID and validate accounts
            foreach (var entry in entries)
            {
                entry.TenantId = tenantId;
                
                // Verify account exists
                var account = await _context.AccountHeads.FindAsync(entry.AccountHeadId);
                if (account == null || account.TenantId != tenantId)
                    return BadRequest(new { message = $"Account {entry.AccountHeadId} not found" });
            }

            // Generate voucher number based on type
            var voucherType = entries[0].VoucherType;
            var voucherNumberPrefix = voucherType switch
            {
                Models.Accounting.VoucherType.Contra => "CONTRA",
                Models.Accounting.VoucherType.Capital => "CAP",
                Models.Accounting.VoucherType.Journal => "JRNL",
                Models.Accounting.VoucherType.CreditNote => "CR",
                Models.Accounting.VoucherType.DebitNote => "DR",
                _ => "VOUCHER"
            };

            var count = await _context.VoucherEntries
                .CountAsync(e => e.VoucherType == voucherType && e.TenantId == tenantId);
            
            var voucherNumber = $"{voucherNumberPrefix}-{count + 1:D6}";

            // Assign voucher number to all entries
            foreach (var entry in entries)
            {
                entry.VoucherNumber = voucherNumber;
                entry.CreatedAt = DateTime.UtcNow;
            }

            _context.VoucherEntries.AddRange(entries);
            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                message = "Voucher created successfully",
                voucherNumber,
                entryIds = entries.Select(e => e.Id).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating voucher");
            return StatusCode(500, new { message = "Error creating voucher", error = ex.Message });
        }
    }

    [HttpGet("voucher/{voucherNumber}")]
    public async Task<IActionResult> GetVoucher(string voucherNumber)
    {
        var tenantId = _tenantService.GetTenantId();
        
        var entries = await _context.VoucherEntries
            .Include(e => e.AccountHead)
            .Where(e => e.VoucherNumber == voucherNumber && e.TenantId == tenantId)
            .OrderBy(e => e.Id)
            .ToListAsync();

        if (entries.Count == 0)
            return NotFound(new { message = "Voucher not found" });

        return Ok(new {
            voucherNumber = entries[0].VoucherNumber,
            voucherType = entries[0].VoucherType,
            date = entries[0].Date,
            entries = entries.Select(e => new {
                e.Id,
                e.AccountHeadId,
                accountName = e.AccountHead.Name,
                e.Debit,
                e.Credit,
                e.Narration
            }).ToList()
        });
    }

    [HttpGet("vouchers")]
    public async Task<IActionResult> GetVouchers(
        [FromQuery] VoucherType? voucherType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = _tenantService.GetTenantId();
        
        var query = _context.VoucherEntries
            .Include(e => e.AccountHead)
            .Where(e => e.TenantId == tenantId)
            .AsQueryable();

        if (voucherType.HasValue)
            query = query.Where(e => e.VoucherType == voucherType.Value);
        
        if (fromDate.HasValue)
            query = query.Where(e => e.Date >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(e => e.Date <= toDate.Value);

        var entries = await query.OrderByDescending(e => e.Date).ThenByDescending(e => e.CreatedAt).ToListAsync();

        // Group by voucher number
        var vouchers = entries.GroupBy(e => e.VoucherNumber).Select(g => new {
            voucherNumber = g.Key,
            voucherType = g.First().VoucherType,
            date = g.First().Date,
            totalAmount = g.Sum(e => e.Debit),
            entryCount = g.Count()
        }).ToList();

        return Ok(vouchers);
    }
}
