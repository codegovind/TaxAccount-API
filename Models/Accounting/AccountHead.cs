namespace TaxAccount.Models.Accounting;

public enum AccountType 
{ 
    Asset = 1, 
    Liability = 2, 
    Equity = 3, 
    Income = 4, 
    Expense = 5 
}

public class AccountHead
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    
    public int? ParentId { get; set; }
    public AccountHead? Parent { get; set; }
    public ICollection<AccountHead> Children { get; set; } = new List<AccountHead>();
    
    public decimal OpeningBalance { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AccountGroup
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PrimaryGroup { get; set; } = string.Empty;
    
    // Navigation
    public ICollection<AccountHead> AccountHeads { get; set; } = new List<AccountHead>();
}