using Microsoft.EntityFrameworkCore;
using TaxAccount.Models;
using TaxAccount.Services;

namespace TaxAccount.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantService? _tenantService;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ITenantService? tenantService = null)
            : base(options)
        {
            _tenantService = tenantService;
        }

        // ── DbSets ──
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<TransportDetail> TransportDetails { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseBill> PurchaseBills { get; set; }
        public DbSet<PurchaseBillItem> PurchaseBillItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── Global Query Filters ──
            modelBuilder.Entity<Contact>()
                .HasQueryFilter(c => _tenantService == null ||
                    c.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => _tenantService == null ||
                    p.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<Invoice>()
                .HasQueryFilter(i => _tenantService == null ||
                    i.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<InvoiceItem>()
                .HasQueryFilter(ii => _tenantService == null ||
                    ii.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<TransportDetail>()
                .HasQueryFilter(t => _tenantService == null ||
                    t.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<StockAdjustment>()
                .HasQueryFilter(sa => _tenantService == null ||
                    sa.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<PurchaseOrder>()
                .HasQueryFilter(po => _tenantService == null ||
                    po.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasQueryFilter(poi => _tenantService == null ||
                    poi.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<PurchaseBill>()
                .HasQueryFilter(pb => _tenantService == null ||
                    pb.TenantId == _tenantService.GetTenantId());

            modelBuilder.Entity<PurchaseBillItem>()
                .HasQueryFilter(pbi => _tenantService == null ||
                    pbi.TenantId == _tenantService.GetTenantId());

            // ── Precision: Product ──
            modelBuilder.Entity<Product>()
                .Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Product>()
                .Property(p => p.PurchasePrice).HasPrecision(18, 2);
            modelBuilder.Entity<Product>()
                .Property(p => p.MarketValue).HasPrecision(18, 2);
            modelBuilder.Entity<Product>()
                .Property(p => p.Stock).HasPrecision(18, 2);
            modelBuilder.Entity<Product>()
                .Property(p => p.GSTPercent).HasPrecision(5, 2);

            // ── Precision: Contact ──
            modelBuilder.Entity<Contact>()
                .Property(c => c.OpeningBalance).HasPrecision(18, 2);

            // ── Precision: Invoice ──
            modelBuilder.Entity<Invoice>()
                .Property(i => i.SubTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount).HasPrecision(18, 2);

            // ── Precision: InvoiceItem ──
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.DiscountPercent).HasPrecision(5, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.TaxPercent).HasPrecision(5, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.TaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.CgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.CgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.SgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.SgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.IgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.IgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.TotalAmount).HasPrecision(18, 2);

            // ── Precision: StockAdjustment ──
            modelBuilder.Entity<StockAdjustment>()
                .Property(sa => sa.Quantity).HasPrecision(18, 2);

            // Precision
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.SubTotal).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.TaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.TotalAmount).HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.DiscountPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.TaxPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.TaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.CgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.CgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.SgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.SgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.IgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(i => i.IgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseOrderItem>()
            .Property(i => i.TotalAmount).HasPrecision(18, 2);

            // Precision: PurchaseBill
            modelBuilder.Entity<PurchaseBill>()
                .Property(pb => pb.SubTotal).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBill>()
                .Property(pb => pb.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBill>()
                .Property(pb => pb.TaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBill>()
                .Property(pb => pb.TotalAmount).HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.DiscountPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.TaxPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.TaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.CgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.CgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.SgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.SgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.IgstPercent).HasPrecision(5, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.IgstAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseBillItem>()
                .Property(i => i.TotalAmount).HasPrecision(18, 2);

            // ── RolePermission Composite Key ──
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // ── User: unique email per tenant ──
            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.TenantId, u.Email })
                .IsUnique();

            // ── User → Tenant ──
            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── User → Role ──
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Contact → Tenant ──
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Tenant)
                .WithMany(t => t.Contacts)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Invoice relationships ──
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Contact)
                .WithMany()
                .HasForeignKey(i => i.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Tenant)
                .WithMany()
                .HasForeignKey(i => i.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── TransportDetail 1-to-1 with Invoice ──
            modelBuilder.Entity<TransportDetail>()
                .HasOne(t => t.Invoice)
                .WithOne(i => i.TransportDetail)
                .HasForeignKey<TransportDetail>(t => t.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── StockAdjustment relationships ──
            modelBuilder.Entity<StockAdjustment>()
                .HasOne(sa => sa.Product)
                .WithMany(p => p.StockAdjustments)
                .HasForeignKey(sa => sa.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockAdjustment>()
                .HasOne(sa => sa.AdjustedBy)
                .WithMany()
                .HasForeignKey(sa => sa.AdjustedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockAdjustment>()
                .HasOne(sa => sa.Tenant)
                .WithMany()
                .HasForeignKey(sa => sa.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships Purchase
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Contact)
                .WithMany()
                .HasForeignKey(po => po.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.CreatedBy)
                .WithMany()
                .HasForeignKey(po => po.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(poi => poi.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.Product)
                .WithMany()
                .HasForeignKey(poi => poi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships PurchaseBill
            modelBuilder.Entity<PurchaseBill>()
                .HasOne(pb => pb.Contact)
                .WithMany()
                .HasForeignKey(pb => pb.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseBill>()
                .HasOne(pb => pb.CreatedBy)
                .WithMany()
                .HasForeignKey(pb => pb.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseBill>()
                .HasOne(pb => pb.Tenant)
                .WithMany()
                .HasForeignKey(pb => pb.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseBillItem>()
                .HasOne(pbi => pbi.PurchaseBill)
                .WithMany(pb => pb.Items)
                .HasForeignKey(pbi => pbi.PurchaseBillId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseBillItem>()
                .HasOne(pbi => pbi.Product)
                .WithMany()
                .HasForeignKey(pbi => pbi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Seed: Roles ──
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Owner",
                    Description = "Full access to everything" },
                new Role { Id = 2, Name = "Manager",
                    Description = "Manage operations" },
                new Role { Id = 3, Name = "Staff",
                    Description = "Day to day operations" },
                new Role { Id = 4, Name = "Auditor",
                    Description = "View only access" }
            );

            // ── Seed: Permissions ──
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "products.view",
                    Description = "View products" },
                new Permission { Id = 2, Name = "products.create",
                    Description = "Create products" },
                new Permission { Id = 3, Name = "products.edit",
                    Description = "Edit products" },
                new Permission { Id = 4, Name = "products.delete",
                    Description = "Delete products" },
                new Permission { Id = 5, Name = "invoices.view",
                    Description = "View invoices" },
                new Permission { Id = 6, Name = "invoices.create",
                    Description = "Create invoices" },
                new Permission { Id = 7, Name = "invoices.approve",
                    Description = "Approve invoices" },
                new Permission { Id = 8, Name = "reports.view",
                    Description = "View reports" },
                new Permission { Id = 9, Name = "users.manage",
                    Description = "Manage users" },
                new Permission { Id = 10, Name = "accounts.manage",
                    Description = "Manage accounts" },
                new Permission { Id = 11, Name = "contacts.manage",
                    Description = "Manage contacts" },
                new Permission { Id = 12, Name = "stock.manage",
                    Description = "Manage stock adjustments" }
            );

            // ── Seed: RolePermissions ──
            modelBuilder.Entity<RolePermission>().HasData(
                // Owner — all permissions
                new RolePermission { RoleId = 1, PermissionId = 1 },
                new RolePermission { RoleId = 1, PermissionId = 2 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 1, PermissionId = 4 },
                new RolePermission { RoleId = 1, PermissionId = 5 },
                new RolePermission { RoleId = 1, PermissionId = 6 },
                new RolePermission { RoleId = 1, PermissionId = 7 },
                new RolePermission { RoleId = 1, PermissionId = 8 },
                new RolePermission { RoleId = 1, PermissionId = 9 },
                new RolePermission { RoleId = 1, PermissionId = 10 },
                new RolePermission { RoleId = 1, PermissionId = 11 },
                new RolePermission { RoleId = 1, PermissionId = 12 },

                // Manager
                new RolePermission { RoleId = 2, PermissionId = 1 },
                new RolePermission { RoleId = 2, PermissionId = 2 },
                new RolePermission { RoleId = 2, PermissionId = 3 },
                new RolePermission { RoleId = 2, PermissionId = 5 },
                new RolePermission { RoleId = 2, PermissionId = 6 },
                new RolePermission { RoleId = 2, PermissionId = 7 },
                new RolePermission { RoleId = 2, PermissionId = 8 },
                new RolePermission { RoleId = 2, PermissionId = 11 },
                new RolePermission { RoleId = 2, PermissionId = 12 },

                // Staff
                new RolePermission { RoleId = 3, PermissionId = 1 },
                new RolePermission { RoleId = 3, PermissionId = 2 },
                new RolePermission { RoleId = 3, PermissionId = 5 },
                new RolePermission { RoleId = 3, PermissionId = 6 },
                new RolePermission { RoleId = 3, PermissionId = 11 },

                // Auditor — view only
                new RolePermission { RoleId = 4, PermissionId = 1 },
                new RolePermission { RoleId = 4, PermissionId = 5 },
                new RolePermission { RoleId = 4, PermissionId = 8 }
            );
        }
    }
}