using TaxAccount.Data;
using TaxAccount.Models;
using TaxAccount.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace TaxAccount.Services;

public class CashFlowService
{
    private readonly AppDbContext _context;

    public CashFlowService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CashFlowStatementDto> CalculateDirectMethodAsync(DateTime fromDate, DateTime toDate, int tenantId)
    {
        var result = new CashFlowStatementDto
        {
            Period = $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}",
            OpeningBalance = await GetOpeningBalanceAsync(fromDate, tenantId),
            ClosingBalance = await GetClosingBalanceAsync(toDate, tenantId)
        };

        // Fetch all cash/bank transactions for the period
        var entries = await _context.VoucherEntries
            .Include(e => e.AccountHead)
            .Where(e => e.TenantId == tenantId && e.Date >= fromDate && e.Date <= toDate)
            .ToListAsync();

        // Operating Activities (Simplified: Journal entries affecting P&L)
        result.OperatingActivities.Name = "Cash Flow from Operating Activities";
        result.OperatingActivities.Items = MapOperatingActivities(entries);
        result.OperatingActivities.NetAmount = result.OperatingActivities.Items.Sum(i => i.IsIncome ? i.Amount : -i.Amount);

        // Investing Activities (Simplified: Asset purchases/sales)
        result.InvestingActivities.Name = "Cash Flow from Investing Activities";
        result.InvestingActivities.Items = MapInvestingActivities(entries);
        result.InvestingActivities.NetAmount = result.InvestingActivities.Items.Sum(i => i.IsIncome ? i.Amount : -i.Amount);

        // Financing Activities (Simplified: Capital, Loans, Contra)
        result.FinancingActivities.Name = "Cash Flow from Financing Activities";
        result.FinancingActivities.Items = MapFinancingActivities(entries);
        result.FinancingActivities.NetAmount = result.FinancingActivities.Items.Sum(i => i.IsIncome ? i.Amount : -i.Amount);

        result.NetChange = result.OperatingActivities.NetAmount + 
                           result.InvestingActivities.NetAmount + 
                           result.FinancingActivities.NetAmount;

        return result;
    }

    public async Task<CashFlowStatementDto> CalculateIndirectMethodAsync(DateTime fromDate, DateTime toDate, int tenantId)
    {
        // For simplicity, returning Direct Method structure
        // In a real app, this would start with Net Profit and adjust for non-cash items
        return await CalculateDirectMethodAsync(fromDate, toDate, tenantId);
    }

    public async Task<List<TransactionDetailDto>> GetTransactionDetailsAsync(int accountHeadId, DateTime fromDate, DateTime toDate, int tenantId)
    {
        var entries = await _context.VoucherEntries
            .Include(e => e.AccountHead)
            .Where(e => e.TenantId == tenantId && 
                       e.Date >= fromDate && 
                       e.Date <= toDate && 
                       (e.AccountHeadId == accountHeadId || e.DrCr == 0)) // Simplified filter
            .OrderBy(e => e.Date)
            .ToListAsync();

        return entries.Select(e => new TransactionDetailDto
        {
            Date = e.Date,
            VoucherNumber = e.VoucherNumber ?? string.Empty,
            PartyName = e.AccountHead?.Name ?? string.Empty,
            Narration = e.Narration ?? string.Empty,
            Debit = e.DebitAmount,
            Credit = e.CreditAmount
        }).ToList();
    }

    private async Task<decimal> GetOpeningBalanceAsync(DateTime fromDate, int tenantId)
    {
        // Sum of all Cash/Bank accounts before fromDate
        var cashGroup = await _context.AccountHeads.FirstOrDefaultAsync(a => a.Name == "Cash-in-hand" && a.TenantId == tenantId);
        var bankGroup = await _context.AccountHeads.FirstOrDefaultAsync(a => a.Name.Contains("Bank") && a.TenantId == tenantId);
        
        if (cashGroup == null && bankGroup == null) return 0;

        var opening = await _context.VoucherEntries
            .Where(e => e.TenantId == tenantId && 
                       e.Date < fromDate && 
                       (e.AccountHeadId == (cashGroup?.Id ?? 0) || e.AccountHeadId == (bankGroup?.Id ?? 0)))
            .SumAsync(e => e.DebitAmount - e.CreditAmount);

        return opening;
    }

    private async Task<decimal> GetClosingBalanceAsync(DateTime toDate, int tenantId)
    {
        // Sum of all Cash/Bank accounts up to toDate
        var cashGroup = await _context.AccountHeads.FirstOrDefaultAsync(a => a.Name == "Cash-in-hand" && a.TenantId == tenantId);
        var bankGroup = await _context.AccountHeads.FirstOrDefaultAsync(a => a.Name.Contains("Bank") && a.TenantId == tenantId);
        
        if (cashGroup == null && bankGroup == null) return 0;

        var closing = await _context.VoucherEntries
            .Where(e => e.TenantId == tenantId && 
                       e.Date <= toDate && 
                       (e.AccountHeadId == (cashGroup?.Id ?? 0) || e.AccountHeadId == (bankGroup?.Id ?? 0)))
            .SumAsync(e => e.DebitAmount - e.CreditAmount);

        return closing;
    }

    private List<LineItemDto> MapOperatingActivities(List<VoucherEntry> entries)
    {
        // Simplified mapping logic
        var items = new List<LineItemDto>();
        // Add logic to group entries by nature (Sales, Purchase, Expenses)
        return items;
    }

    private List<LineItemDto> MapInvestingActivities(List<VoucherEntry> entries)
    {
        var items = new List<LineItemDto>();
        // Add logic for Asset purchase/sale
        return items;
    }

    private List<LineItemDto> MapFinancingActivities(List<VoucherEntry> entries)
    {
        var items = new List<LineItemDto>();
        // Add logic for Capital, Drawings, Loans
        return items;
    }
}
