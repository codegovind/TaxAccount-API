namespace TaxAccount.Models.Settings;

public class TenantSetting
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Company Details
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Gstn { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string Pancard { get; set; } = string.Empty;
    
    // Feature Toggles
    public bool IsEWayBillEnabled { get; set; } = false;
    public bool IsInventoryEnabled { get; set; } = true;
    public bool IsGstEnabled { get; set; } = true;
    public bool IsMultiCurrencyEnabled { get; set; } = false;
    
    // E-Way Bill Credentials (Encrypted in production)
    public string? EWayBillUsername { get; set; }
    public string? EWayBillPassword { get; set; }
    public string? EWayBillApiUrl { get; set; }
    
    // Financial Year
    public int FinancialYearStart { get; set; } = 4; // April
    public int FinancialYearStartDay { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
