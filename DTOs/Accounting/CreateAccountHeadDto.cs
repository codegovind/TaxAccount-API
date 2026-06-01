using System.ComponentModel.DataAnnotations;
using TaxAccount.Models.Accounting;

namespace TaxAccount.DTOs.Accounting
{
    public class CreateAccountHeadDto
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public AccountType Type { get; set; }

        public int? ParentId { get; set; }

        public decimal OpeningBalance { get; set; } = 0;
    }

    public class UpdateAccountHeadDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        public decimal? OpeningBalance { get; set; }

        public bool? IsActive { get; set; }
    }

    public class AccountHeadDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public decimal OpeningBalance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
