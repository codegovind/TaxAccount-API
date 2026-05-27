using TaxAccount.DTOs;

namespace TaxAccount.Services
{
    public interface IInvoiceService
    {
        Task<List<InvoiceResponseDto>> GetAllAsync();
        Task<InvoiceResponseDto> GetByIdAsync(int id);
        Task<InvoiceResponseDto> CreateAsync(CreateInvoiceDto dto, int createdByUserId);
        Task<bool> DeleteAsync(int id);
    }
}