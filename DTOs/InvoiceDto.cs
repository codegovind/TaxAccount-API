using System.ComponentModel.DataAnnotations;
using TaxAccount.Models;

namespace TaxAccount.DTOs
{    
    public class CreateInvoiceDto
    {
        public int? ContactId { get; set; } // null = cash sale

        [Required]
        public DateTime DueDate { get; set; }

        public InvoiceType InvoiceType { get; set; } = InvoiceType.Sale;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public EntrySource EntrySource { get; set; } = EntrySource.FullAccounting;

        public string Notes { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one item required")]
        public List<CreateInvoiceItemDto> Items { get; set; } = new();
    }

    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string EntrySource { get; set; } = string.Empty;
        public int? ContactId { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new();
    }
}