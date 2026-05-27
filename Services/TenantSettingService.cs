using Microsoft.EntityFrameworkCore;
using TaxAccount.Data;
using TaxAccount.DTOs.Compliance;
using TaxAccount.Models;
using TaxAccount.Models.Settings;

namespace TaxAccount.Services;

public interface ITenantSettingService
{
    Task<TenantSettingDto> GetSettingsAsync();
    Task<TenantSettingDto> UpdateSettingsAsync(UpdateTenantSettingDto dto);
}

public class TenantSettingService : ITenantSettingService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;

    public TenantSettingService(AppDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<TenantSettingDto> GetSettingsAsync()
    {
        var tenantId = _tenantService.GetTenantId();
        
        var settings = await _context.TenantSettings
            .Include(t => t.Tenant)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        
        if (settings == null)
        {
            // Create default settings if not exist
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                throw new KeyNotFoundException("Tenant not found");
            }

            settings = new TenantSetting
            {
                TenantId = tenantId,
                CompanyName = tenant.CompanyName,
                Gstn = string.Empty,
                StateCode = string.Empty,
                Address = string.Empty,
                Phone = string.Empty,
                Email = string.Empty,
                IsEWayBillEnabled = false,
                IsInventoryEnabled = true,
                IsGstEnabled = true
            };

            _context.TenantSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return new TenantSettingDto
        {
            Id = settings.Id,
            CompanyName = settings.CompanyName,
            Gstn = settings.Gstn,
            StateCode = settings.StateCode,
            Address = settings.Address,
            Phone = settings.Phone,
            Email = settings.Email,
            IsEWayBillEnabled = settings.IsEWayBillEnabled,
            IsInventoryEnabled = settings.IsInventoryEnabled,
            IsGstEnabled = settings.IsGstEnabled,
            EWayBillUsername = settings.EWayBillUsername,
            EWayBillPassword = settings.EWayBillPassword
        };
    }

    public async Task<TenantSettingDto> UpdateSettingsAsync(UpdateTenantSettingDto dto)
    {
        var tenantId = _tenantService.GetTenantId();
        
        var settings = await _context.TenantSettings
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        
        if (settings == null)
        {
            // Create new settings
            settings = new TenantSetting
            {
                TenantId = tenantId,
                CompanyName = dto.CompanyName,
                Gstn = dto.Gstn,
                StateCode = dto.StateCode,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                IsEWayBillEnabled = dto.IsEWayBillEnabled,
                IsInventoryEnabled = dto.IsInventoryEnabled,
                IsGstEnabled = dto.IsGstEnabled,
                EWayBillUsername = dto.EWayBillUsername,
                EWayBillPassword = dto.EWayBillPassword
            };

            _context.TenantSettings.Add(settings);
        }
        else
        {
            // Update existing settings
            settings.CompanyName = dto.CompanyName;
            settings.Gstn = dto.Gstn;
            settings.StateCode = dto.StateCode;
            settings.Address = dto.Address;
            settings.Phone = dto.Phone;
            settings.Email = dto.Email;
            settings.IsEWayBillEnabled = dto.IsEWayBillEnabled;
            settings.IsInventoryEnabled = dto.IsInventoryEnabled;
            settings.IsGstEnabled = dto.IsGstEnabled;
            
            // Only update credentials if provided
            if (!string.IsNullOrEmpty(dto.EWayBillUsername))
                settings.EWayBillUsername = dto.EWayBillUsername;
            if (!string.IsNullOrEmpty(dto.EWayBillPassword))
                settings.EWayBillPassword = dto.EWayBillPassword; // TODO: Encrypt in production
        }

        await _context.SaveChangesAsync();

        return new TenantSettingDto
        {
            Id = settings.Id,
            CompanyName = settings.CompanyName,
            Gstn = settings.Gstn,
            StateCode = settings.StateCode,
            Address = settings.Address,
            Phone = settings.Phone,
            Email = settings.Email,
            IsEWayBillEnabled = settings.IsEWayBillEnabled,
            IsInventoryEnabled = settings.IsInventoryEnabled,
            IsGstEnabled = settings.IsGstEnabled,
            EWayBillUsername = settings.EWayBillUsername,
            EWayBillPassword = settings.EWayBillPassword
        };
    }
}
