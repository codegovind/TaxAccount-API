namespace TaxAccount.DTOs.Compliance;

public class EWayBillRequestDto
{
    public int InvoiceId { get; set; }
    public string TransporterId { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public DateTime DispatchDate { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingPinCode { get; set; }
    public string? ShippingState { get; set; }
}

public class EWayBillResponseDto
{
    public int Id { get; set; }
    public string EWayBillNumber { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Irn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class TenantSettingDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Gstn { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Feature Toggles
    public bool IsEWayBillEnabled { get; set; }
    public bool IsInventoryEnabled { get; set; } = true;
    public bool IsGstEnabled { get; set; } = true;
    
    // E-Way Bill Credentials (encrypted in production)
    public string? EWayBillUsername { get; set; }
    public string? EWayBillPassword { get; set; }
}

public class UpdateTenantSettingDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Gstn { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsEWayBillEnabled { get; set; }
    public bool IsInventoryEnabled { get; set; } = true;
    public bool IsGstEnabled { get; set; } = true;
    public string? EWayBillUsername { get; set; }
    public string? EWayBillPassword { get; set; }
}
