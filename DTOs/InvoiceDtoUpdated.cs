using System.ComponentModel.DataAnnotations;

namespace TaxAccount.DTOs
{
    public class CreateInvoiceDto
    {
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime InvoiceDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int? ContactId { get; set; }

        public string PaymentMethod { get; set; } = "Credit";

        public string EntrySource { get; set; } = "FullAccounting";

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public List<InvoiceItemDto> Items { get; set; } = new();

        public decimal DiscountAmount { get; set; } = 0;

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class UpdateInvoiceDto
    {
        public int? ContactId { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Notes { get; set; }
    }

    public class InvoiceDtoResponse
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ContactName { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new();
    }
}
