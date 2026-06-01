using TaxAccount.DTOs;

namespace TaxAccount.Services
{
    public interface IExpenseService
    {
        Task<List<ExpenseDto>> GetExpensesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ExpenseDto> GetExpenseAsync(int id);
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto);
        Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseDto dto);
        Task<bool> DeleteExpenseAsync(int id);
        Task<List<ExpenseDto>> GetExpensesByContactAsync(int contactId);
        Task<decimal> GetTotalExpensesAsync(DateTime fromDate, DateTime toDate);
    }
}
