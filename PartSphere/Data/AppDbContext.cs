using Microsoft.EntityFrameworkCore;
using PartSphere.Models;

namespace PartSphere.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<VehiclePart> VehicleParts => Set<VehiclePart>();
        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
        public DbSet<SalesItem> SalesItems => Set<SalesItem>();
        public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
        public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<CreditPayment> CreditPayments => Set<CreditPayment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<PartRequest> PartRequests => Set<PartRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>();
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithOne(u => u.Customer)
                    .HasForeignKey<Customer>(c => c.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(c => c.Vehicles)
                    .WithOne(v => v.Customer)
                    .HasForeignKey(v => v.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.SalesInvoices)
                    .WithOne(s => s.Customer)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Appointments)
                    .WithOne(a => a.Customer)
                    .HasForeignKey(a => a.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Reviews)
                    .WithOne(r => r.Customer)
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.CreditPayments)
                    .WithOne(cp => cp.Customer)
                    .HasForeignKey(cp => cp.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasIndex(v => v.VehicleNumber).IsUnique();
            });

            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.HasMany(v => v.Parts)
                    .WithOne(p => p.Vendor)
                    .HasForeignKey(p => p.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(v => v.PurchaseInvoices)
                    .WithOne(pi => pi.Vendor)
                    .HasForeignKey(pi => pi.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VehiclePart>(entity =>
            {
                entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<SalesInvoice>(entity =>
            {
                entity.HasOne(s => s.Staff)
                    .WithMany()
                    .HasForeignKey(s => s.StaffId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(s => s.Items)
                    .WithOne(i => i.SalesInvoice)
                    .HasForeignKey(i => i.SalesInvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Invoice)
                    .WithOne(i => i.SalesInvoice)
                    .HasForeignKey<Invoice>(i => i.SalesInvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SalesItem>(entity =>
            {
                entity.HasOne(i => i.VehiclePart)
                    .WithMany(p => p.SalesItems)
                    .HasForeignKey(i => i.VehiclePartId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PurchaseInvoice>(entity =>
            {
                entity.HasMany(p => p.Items)
                    .WithOne(i => i.PurchaseInvoice)
                    .HasForeignKey(i => i.PurchaseInvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PurchaseItem>(entity =>
            {
                entity.HasOne(i => i.VehiclePart)
                    .WithMany(p => p.PurchaseItems)
                    .HasForeignKey(i => i.VehiclePartId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CreditPayment>(entity =>
            {
                entity.Property(c => c.Status).HasConversion<string>();

                entity.HasOne(c => c.SalesInvoice)
                    .WithMany()
                    .HasForeignKey(c => c.SalesInvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Type).HasConversion<string>();
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.Property(a => a.Status).HasConversion<string>();
            });

            modelBuilder.Entity<PartRequest>(entity =>
            {
                entity.HasOne(pr => pr.Customer)
                    .WithMany()
                    .HasForeignKey(pr => pr.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
