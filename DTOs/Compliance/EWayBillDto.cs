using System.ComponentModel.DataAnnotations;

namespace TaxAccount.DTOs.Compliance
{
    public class EWayBillRequestDto
    {
        [Required]
        public int InvoiceId { get; set; }

        [StringLength(50)]
        public string? TransporterId { get; set; }

        [StringLength(20)]
        public string? VehicleNumber { get; set; }

        public DateTime? DispatchDate { get; set; }
    }

    public class EWayBillResponseDto
    {
        public int Id { get; set; }
        public string EWayBillNumber { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Irn { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
