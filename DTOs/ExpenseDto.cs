using System.ComponentModel.DataAnnotations;
using TaxAccount.Models;

namespace TaxAccount.DTOs
{
    public class CreateExpenseDto
    {
        [Required]
        [StringLength(50)]
        public string ExpenseNumber { get; set; } = string.Empty;

        [Required]
        public ExpenseCategory Category { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        public int? ContactId { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }

    public class UpdateExpenseDto
    {
        [StringLength(500)]
        public string? Description { get; set; }

        public ExpenseCategory? Category { get; set; }

        public DateTime? ExpenseDate { get; set; }

        public int? ContactId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Amount { get; set; }

        public bool? IsApproved { get; set; }
    }

    public class ExpenseDto
    {
        public int Id { get; set; }
        public string ExpenseNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime ExpenseDate { get; set; }
        public string? ContactName { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
