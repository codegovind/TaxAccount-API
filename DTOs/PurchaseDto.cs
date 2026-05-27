using System.ComponentModel.DataAnnotations;
using TaxAccount.Models;

namespace TaxAccount.DTOs
{
    public class CreatePurchaseItemDto
    {
        [Required]
        public int ProductId { get; set; }
        public string Description { get; set; } = string.Empty;
        [Required][Range(0.01, 99999)]
        public decimal Quantity { get; set; }
        [Range(0, 9999999)]
        public decimal UnitPrice { get; set; } = 0;
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;
        [Range(0, 100)]
        public decimal TaxPercent { get; set; } = 0;
    }

    public class CreatePurchaseBillDto
    {
        public int? ContactId { get; set; }
        [Required]
        public DateTime BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string VendorBillNumber { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; } 
            = PaymentMethod.Credit;
        public string Notes { get; set; } = string.Empty;
        [Required][MinLength(1)]
        public List<CreatePurchaseItemDto> Items { get; set; } = new();
    }

    public class CreatePurchaseOrderDto
    {
        public int? ContactId { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        [Required][MinLength(1)]
        public List<CreatePurchaseItemDto> Items { get; set; } = new();
    }

    public class PurchaseItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HsnCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PurchaseBillResponseDto
    {
        public int Id { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string VendorBillNumber { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public int? ContactId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PurchaseItemResponseDto> Items { get; set; } = new();
    }

    public class PurchaseOrderResponseDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? ContactId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PurchaseItemResponseDto> Items { get; set; } = new();
    }

    public class UpdatePurchaseOrderStatusDto
    {
        [Required]
        public PurchaseOrderStatus Status { get; set; }
    }
}