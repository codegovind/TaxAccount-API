using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxAccount.DTOs.Compliance;
using TaxAccount.Services;

namespace TaxAccount.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ComplianceController : ControllerBase
{
    private readonly IEWayBillService _eWayBillService;
    private readonly ITenantSettingService _tenantSettingService;

    public ComplianceController(
        IEWayBillService eWayBillService,
        ITenantSettingService tenantSettingService)
    {
        _eWayBillService = eWayBillService;
        _tenantSettingService = tenantSettingService;
    }

    #region E-Way Bill

    /// <summary>
    /// Generate E-Way Bill for an invoice (only if feature is enabled)
    /// </summary>
    [HttpPost("ewaybill/generate")]
    public async Task<ActionResult<EWayBillResponseDto>> GenerateEWayBill([FromBody] EWayBillRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _eWayBillService.GenerateEWayBillAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get E-Way Bill details for an invoice
    /// </summary>
    [HttpGet("ewaybill/invoice/{invoiceId}")]
    public async Task<ActionResult<EWayBillResponseDto?>> GetEWayBillByInvoice(int invoiceId)
    {
        var result = await _eWayBillService.GetByInvoiceIdAsync(invoiceId);
        
        if (result == null)
            return NotFound(new { message = "E-Way Bill not found for this invoice" });
        
        return Ok(result);
    }

    #endregion

    #region Tenant Settings

    /// <summary>
    /// Get tenant settings including feature toggles
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<TenantSettingDto>> GetSettings()
    {
        var settings = await _tenantSettingService.GetSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// Update tenant settings and feature toggles
    /// </summary>
    [HttpPut("settings")]
    public async Task<ActionResult<TenantSettingDto>> UpdateSettings([FromBody] UpdateTenantSettingDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var settings = await _tenantSettingService.UpdateSettingsAsync(dto);
        return Ok(settings);
    }

    #endregion
}
