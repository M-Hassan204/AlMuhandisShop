using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AlMuhandisShop.Models;

namespace AlMuhandisShop.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseItem> PurchaseItems { get; set; }
    public DbSet<CustomerPayment> CustomerPayments { get; set; }
    public DbSet<SupplierPayment> SupplierPayments { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<CashTransaction> CashTransactions { get; set; }
    public DbSet<SaleReturn> SaleReturns { get; set; }
    public DbSet<SaleReturnItem> SaleReturnItems { get; set; }
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
    public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Category
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100).IsUnicode(true);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Product
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200).IsUnicode(true);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.StockQuantity).HasColumnType("decimal(18,3)");
            entity.Property(e => e.Unit).HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.Description).HasMaxLength(1000).IsUnicode(true);

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Customer
        builder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200).IsUnicode(true);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500).IsUnicode(true);
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);
        });

        // Supplier
        builder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200).IsUnicode(true);
            entity.Property(e => e.CompanyName).HasMaxLength(200).IsUnicode(true);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500).IsUnicode(true);
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);
        });

        // Sale
        builder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RemainingAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Sales)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);
        });

        // SaleItem
        builder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Sale)
                  .WithMany(s => s.SaleItems)
                  .HasForeignKey(e => e.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.SaleItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Purchase
        builder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RemainingAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.Purchases)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseItem
        builder.Entity<PurchaseItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Purchase)
                  .WithMany(p => p.PurchaseItems)
                  .HasForeignKey(e => e.PurchaseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.PurchaseItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // CustomerPayment
        builder.Entity<CustomerPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Payments)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SupplierPayment
        builder.Entity<SupplierPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.Payments)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Expense
        builder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200).IsUnicode(true);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);
        });

        // CashTransaction
        builder.Entity<CashTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);
            entity.HasIndex(e => e.Date);
        });

        // SaleReturn
        builder.Entity<SaleReturn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);

            entity.HasOne(e => e.Sale)
                  .WithMany(s => s.SaleReturns)
                  .HasForeignKey(e => e.SaleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SaleReturnItem
        builder.Entity<SaleReturnItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.SaleReturn)
                  .WithMany(sr => sr.Items)
                  .HasForeignKey(e => e.SaleReturnId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.SaleReturnItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseReturn
        builder.Entity<PurchaseReturn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000).IsUnicode(true);

            entity.HasOne(e => e.Purchase)
                  .WithMany(p => p.PurchaseReturns)
                  .HasForeignKey(e => e.PurchaseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                  .WithMany()
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseReturnItem
        builder.Entity<PurchaseReturnItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.PurchaseReturn)
                  .WithMany(pr => pr.Items)
                  .HasForeignKey(e => e.PurchaseReturnId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.PurchaseReturnItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
