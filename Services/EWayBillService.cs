using Microsoft.EntityFrameworkCore;
using TaxAccount.Data;
using TaxAccount.DTOs.Compliance;
using TaxAccount.Models.Compliance;

namespace TaxAccount.Services;

public interface IEWayBillService
{
    Task<EWayBillResponseDto> GenerateEWayBillAsync(EWayBillRequestDto request);
    Task<EWayBillResponseDto?> GetByInvoiceIdAsync(int invoiceId);
}

public class EWayBillService : IEWayBillService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly HttpClient? _httpClient;

    public EWayBillService(
        AppDbContext context, 
        ITenantService tenantService,
        HttpClient? httpClient = null)
    {
        _context = context;
        _tenantService = tenantService;
        _httpClient = httpClient;
    }

    public async Task<EWayBillResponseDto> GenerateEWayBillAsync(EWayBillRequestDto request)
    {
        var tenantId = _tenantService.GetTenantId();
        
        // Check if E-Way Bill feature is enabled
        var settings = await _context.TenantSettings
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        
        if (settings == null || !settings.IsEWayBillEnabled)
        {
            throw new InvalidOperationException("E-Way Bill feature is not enabled for this tenant.");
        }

        // Fetch invoice details
        var invoice = await _context.Invoices
            .Include(i => i.Contact)
            .Include(i => i.TransportDetail)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.TenantId == tenantId);
        
        if (invoice == null)
        {
            throw new KeyNotFoundException("Invoice not found.");
        }

        // Check if E-Way Bill already exists
        var existingEWayBill = await _context.EWayBills
            .FirstOrDefaultAsync(e => e.InvoiceId == request.InvoiceId);
        
        if (existingEWayBill != null)
        {
            return new EWayBillResponseDto
            {
                Id = existingEWayBill.Id,
                EWayBillNumber = existingEWayBill.EWayBillNumber,
                GeneratedDate = existingEWayBill.GeneratedDate,
                ValidUntil = existingEWayBill.ValidUntil,
                Irn = existingEWayBill.Irn,
                IsActive = true
            };
        }

        // TODO: Integrate with GSTN API for actual E-Way Bill generation
        // For now, generate a mock E-Way Bill number
        var ewayBillNumber = $"EWB{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        var generatedDate = DateTime.Now;
        var validUntil = generatedDate.AddDays(15); // Standard validity

        // Create JSON payload for GSTN API (placeholder)
        var jsonData = $@"{{
            ""InvoiceId"": {request.InvoiceId},
            ""TransporterId"": ""{request.TransporterId}"",
            ""VehicleNumber"": ""{request.VehicleNumber}"",
            ""DispatchDate"": ""{request.DispatchDate:yyyy-MM-dd}""
        }}";

        var ewayBill = new EWayBill
        {
            InvoiceId = request.InvoiceId,
            EWayBillNumber = ewayBillNumber,
            GeneratedDate = generatedDate,
            ValidUntil = validUntil,
            Irn = string.Empty, // Populate when e-Invoice integration is done
            JsonData = jsonData,
            TenantId = tenantId
        };

        _context.EWayBills.Add(ewayBill);
        await _context.SaveChangesAsync();

        return new EWayBillResponseDto
        {
            Id = ewayBill.Id,
            EWayBillNumber = ewayBill.EWayBillNumber,
            GeneratedDate = ewayBill.GeneratedDate,
            ValidUntil = ewayBill.ValidUntil,
            Irn = ewayBill.Irn,
            IsActive = true
        };
    }

    public async Task<EWayBillResponseDto?> GetByInvoiceIdAsync(int invoiceId)
    {
        var tenantId = _tenantService.GetTenantId();
        
        var ewayBill = await _context.EWayBills
            .FirstOrDefaultAsync(e => e.InvoiceId == invoiceId && e.TenantId == tenantId);
        
        if (ewayBill == null)
        {
            return null;
        }

        return new EWayBillResponseDto
        {
            Id = ewayBill.Id,
            EWayBillNumber = ewayBill.EWayBillNumber,
            GeneratedDate = ewayBill.GeneratedDate,
            ValidUntil = ewayBill.ValidUntil,
            Irn = ewayBill.Irn,
            IsActive = true
        };
    }
}
