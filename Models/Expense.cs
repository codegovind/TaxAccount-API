namespace TaxAccount.Models
{
    public enum ExpenseCategory
    {
        Rent = 1,
        Utilities = 2,
        Salaries = 3,
        Marketing = 4,
        Travel = 5,
        Office = 6,
        Supplies = 7,
        Other = 8
    }

    public class Expense
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string ExpenseNumber { get; set; } = string.Empty;
        public ExpenseCategory Category { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public int? ContactId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
        public Contact? Contact { get; set; }
        public User CreatedBy { get; set; } = null!;
    }
}
