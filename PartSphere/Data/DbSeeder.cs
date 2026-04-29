using Microsoft.EntityFrameworkCore;
using PartSphere.Models;

namespace PartSphere.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // 1. Seed Admin
            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
            {
                var admin = new User
                {
                    Name = "System Admin",
                    Email = "admin@partsphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRole.Admin,
                    IsActive = true
                };
                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }

            // 2. Seed Vendors
            if (!await context.Vendors.AnyAsync())
            {
                var vendors = new List<Vendor>
                {
                    new Vendor { Name = "Global Auto Parts", ContactPerson = "John Doe", Email = "john@globalparts.com", Phone = "1234567890", Address = "Detroit, MI", Category = "Engine Components" },
                    new Vendor { Name = "Apex Tires", ContactPerson = "Sarah Smith", Email = "sarah@apextires.com", Phone = "0987654321", Address = "Akron, OH", Category = "Wheels & Tires" },
                    new Vendor { Name = "BrakeMaster", ContactPerson = "Mike Ross", Email = "mike@brakemaster.com", Phone = "1122334455", Address = "Chicago, IL", Category = "Braking Systems" }
                };
                context.Vendors.AddRange(vendors);
                await context.SaveChangesAsync();
            }

            // 3. Seed Customers
            if (!await context.Customers.AnyAsync())
            {
                var customers = new List<Customer>
                {
                    new Customer { Name = "Alice Johnson", Email = "alice@example.com", Phone = "555-0101", Address = "123 Maple St", LoyaltyPoints = 150 },
                    new Customer { Name = "Bob Wilson", Email = "bob@example.com", Phone = "555-0102", Address = "456 Oak Ave", LoyaltyPoints = 50 }
                };
                context.Customers.AddRange(customers);
                await context.SaveChangesAsync();
            }

            // 4. Seed Parts
            if (!await context.VehicleParts.AnyAsync())
            {
                var vendors = await context.Vendors.ToListAsync();
                var parts = new List<VehiclePart>
                {
                    new VehiclePart { Name = "V8 Engine Block", Brand = "PowerCore", Price = 2500.00m, StockQuantity = 5, Description = "High-performance engine block", VendorId = vendors[0].Id },
                    new VehiclePart { Name = "Brake Pads (Set of 4)", Brand = "StopSafe", Price = 85.50m, StockQuantity = 25, Description = "Ceramic brake pads for durability", VendorId = vendors[2].Id },
                    new VehiclePart { Name = "All-Season Tire 18\"", Brand = "Apex", Price = 120.00m, StockQuantity = 40, Description = "Reliable all-season performance", VendorId = vendors[1].Id },
                    new VehiclePart { Name = "Oil Filter", Brand = "PureFlow", Price = 15.00m, StockQuantity = 100, Description = "High-efficiency oil filter", VendorId = vendors[0].Id },
                    new VehiclePart { Name = "Spark Plug", Brand = "FireUp", Price = 8.99m, StockQuantity = 8, Description = "Platinum spark plug", VendorId = vendors[0].Id } // Low stock
                };
                context.VehicleParts.AddRange(parts);
                await context.SaveChangesAsync();
            }

            // 5. Seed Recent Sales (for dashboard)
            if (!await context.SalesInvoices.AnyAsync())
            {
                var customers = await context.Customers.ToListAsync();
                var parts = await context.VehicleParts.ToListAsync();

                var sales = new List<SalesInvoice>
                {
                    new SalesInvoice 
                    { 
                        CustomerId = customers[0].Id, 
                        Date = DateTime.UtcNow.AddDays(-2), 
                        TotalAmount = 2500.00m, 
                        Status = "Paid",
                        Items = new List<SalesItem> { new SalesItem { PartId = parts[0].Id, Quantity = 1, UnitPrice = 2500.00m } }
                    },
                    new SalesInvoice 
                    { 
                        CustomerId = customers[1].Id, 
                        Date = DateTime.UtcNow.AddDays(-1), 
                        TotalAmount = 171.00m, 
                        Status = "Paid",
                        Items = new List<SalesItem> { new SalesItem { PartId = parts[1].Id, Quantity = 2, UnitPrice = 85.50m } }
                    }
                };
                context.SalesInvoices.AddRange(sales);
                await context.SaveChangesAsync();
            }
        }
    }
}
