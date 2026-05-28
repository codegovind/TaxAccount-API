using TaxAccount.Data;
using TaxAccount.Models;
using Microsoft.EntityFrameworkCore;

namespace TaxAccount.Services;

public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.AccountHeads.AnyAsync())
        {
            _logger.LogInformation("Database already seeded.");
            return;
        }

        _logger.LogInformation("Starting database seeding...");

        // 1. Seed Groups (Simplified)
        var groups = new List<AccountGroup>
        {
            new AccountGroup { Name = "Capital Account", PrimaryGroup = "Capital" },
            new AccountGroup { Name = "Current Assets", PrimaryGroup = "Current Assets" },
            new AccountGroup { Name = "Current Liabilities", PrimaryGroup = "Current Liabilities" },
            new AccountGroup { Name = "Sales Accounts", PrimaryGroup = "Sales Accounts" },
            new AccountGroup { Name = "Purchase Accounts", PrimaryGroup = "Purchase Accounts" },
            new AccountGroup { Name = "Direct Incomes", PrimaryGroup = "Direct Incomes" },
            new AccountGroup { Name = "Indirect Incomes", PrimaryGroup = "Indirect Incomes" },
            new AccountGroup { Name = "Direct Expenses", PrimaryGroup = "Direct Expenses" },
            new AccountGroup { Name = "Indirect Expenses", PrimaryGroup = "Indirect Expenses" },
            new AccountGroup { Name = "Assets", PrimaryGroup = "Fixed Assets" },
            new AccountGroup { Name = "Loans (Liability)", PrimaryGroup = "Secured Loans" },
            new AccountGroup { Name = "Cash-in-Hand", PrimaryGroup = "Cash-in-Hand" },
            new AccountGroup { Name = "Bank Accounts", PrimaryGroup = "Bank Accounts" },
            new AccountGroup { Name = "Duties & Taxes", PrimaryGroup = "Duties & Taxes" },
            new AccountGroup { Name = "Stock-in-Hand", PrimaryGroup = "Stock-in-Hand" },
            new AccountGroup { Name = "Sundry Debtors", PrimaryGroup = "Sundry Debtors" },
            new AccountGroup { Name = "Sundry Creditors", PrimaryGroup = "Sundry Creditors" }
        };
        await _context.AccountGroups.AddRangeAsync(groups);
        await _context.SaveChangesAsync();

        // 2. Seed Default Ledgers
        var cashGroup = groups.First(g => g.Name == "Cash-in-Hand");
        var bankGroup = groups.First(g => g.Name == "Bank Accounts");
        var salesGroup = groups.First(g => g.Name == "Sales Accounts");
        var purchaseGroup = groups.First(g => g.Name == "Purchase Accounts");
        var dutyGroup = groups.First(g => g.Name == "Duties & Taxes");
        var capitalGroup = groups.First(g => g.Name == "Capital Account");

        var ledgers = new List<AccountHead>
        {
            new AccountHead { Name = "Cash", GroupId = cashGroup.Id, OpeningBalance = 10000 },
            new AccountHead { Name = "HDFC Bank", GroupId = bankGroup.Id, OpeningBalance = 50000 },
            new AccountHead { Name = "Sales", GroupId = salesGroup.Id },
            new AccountHead { Name = "Purchases", GroupId = purchaseGroup.Id },
            new AccountHead { Name = "Output CGST", GroupId = dutyGroup.Id },
            new AccountHead { Name = "Output SGST", GroupId = dutyGroup.Id },
            new AccountHead { Name = "Input CGST", GroupId = dutyGroup.Id },
            new AccountHead { Name = "Input SGST", GroupId = dutyGroup.Id },
            new AccountHead { Name = "Capital Account", GroupId = capitalGroup.Id, OpeningBalance = 100000 }
        };
        await _context.AccountHeads.AddRangeAsync(ledgers);
        await _context.SaveChangesAsync();

        // 3. Seed Items & Godowns
        var godown = new Godown { Name = "Main Warehouse" };
        await _context.Godowns.AddAsync(godown);
        await _context.SaveChangesAsync();

        var items = new List<Item>
        {
            new Item { Name = "Laptop", HsnCode = "8471", Rate = 45000, GodownId = godown.Id },
            new Item { Name = "Mouse", HsnCode = "8471", Rate = 500, GodownId = godown.Id },
            new Item { Name = "Keyboard", HsnCode = "8471", Rate = 1200, GodownId = godown.Id },
            new Item { Name = "Monitor", HsnCode = "8528", Rate = 12000, GodownId = godown.Id },
            new Item { Name = "Printer", HsnCode = "8443", Rate = 15000, GodownId = godown.Id }
        };
        await _context.Items.AddRangeAsync(items);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Database seeding completed successfully.");
    }
}
