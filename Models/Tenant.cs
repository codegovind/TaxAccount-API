using TaxAccount.Models.Settings;
namespace TaxAccount.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? State { get; set; } // For CGST/SGST vs IGST logic
        public string? Gstin { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public TenantSetting? Settings { get; set; }
    }
}