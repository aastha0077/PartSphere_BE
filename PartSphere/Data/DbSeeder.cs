using Microsoft.EntityFrameworkCore;
using PartSphere.Models;

namespace PartSphere.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
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

            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Staff))
            {
                var staff = new User
                {
                    Name = "John Staff",
                    Email = "staff@partsphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                    Role = UserRole.Staff,
                    IsActive = true
                };
                context.Users.Add(staff);
                await context.SaveChangesAsync();
            }

            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Customer))
            {
                var customerUser = new User
                {
                    Name = "Alice Johnson",
                    Email = "alice@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                    Role = UserRole.Customer,
                    IsActive = true
                };
                context.Users.Add(customerUser);
                await context.SaveChangesAsync();

                // Link to Alice's Customer profile if it already exists
                var aliceCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == "alice@example.com");
                if (aliceCustomer != null)
                {
                    aliceCustomer.UserId = customerUser.Id;
                    context.Customers.Update(aliceCustomer);
                    await context.SaveChangesAsync();
                }
            }

            if (!await context.Vendors.AnyAsync())
            {
                var vendors = new List<Vendor>
                {
                    new Vendor { Name = "Sagarmatha Auto Components", ContactPerson = "Ram Bahadur", Email = "ram@sagarmatha.com", Phone = "9841122334", Address = "Teku, Kathmandu", Category = "Engine Components" },
                    new Vendor { Name = "Lumbini Rubber Industries", ContactPerson = "Sita Devi", Email = "sita@lumbini.com", Phone = "9851023456", Address = "Butwal, Rupandehi", Category = "Wheels & Tires" },
                    new Vendor { Name = "Annapurna Braking Systems", ContactPerson = "Krishna Prasad", Email = "krishna@annapurna.com", Phone = "9803344556", Address = "Pokhara, Kaski", Category = "Braking Systems" },
                    new Vendor { Name = "Himalayan Filter House", ContactPerson = "Maya Shrestha", Email = "maya@himalayan.com", Phone = "9867788990", Address = "Biratnagar, Morang", Category = "Filters & Lubricants" }
                };
                context.Vendors.AddRange(vendors);
                await context.SaveChangesAsync();
            }

            if (!await context.Customers.AnyAsync())
            {
                var aliceUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "alice@example.com");
                var customers = new List<Customer>
                {
                    new Customer { Name = "Alice Johnson", Email = "alice@example.com", Phone = "555-0101", Address = "123 Maple St", UserId = aliceUser?.Id },
                    new Customer { Name = "Bob Wilson", Email = "bob@example.com", Phone = "555-0102", Address = "456 Oak Ave" },
                    new Customer { Name = "Charlie Brown", Email = "charlie@example.com", Phone = "555-0103", Address = "789 Pine Rd" }
                };
                context.Customers.AddRange(customers);
                await context.SaveChangesAsync();
            }

            if (!await context.VehicleParts.AnyAsync())
            {
                var vendors = await context.Vendors.ToListAsync();
                var parts = new List<VehiclePart>
                {
                    new VehiclePart { Name = "V8 Engine Block", Brand = "Sagarmatha", Price = 250000.00m, StockQuantity = 5, Description = "High-performance engine block", VendorId = vendors[0].Id },
                    new VehiclePart { Name = "Brake Pads (Set of 4)", Brand = "Annapurna", Price = 8500.50m, StockQuantity = 25, Description = "Ceramic brake pads for durability", VendorId = vendors[2].Id },
                    new VehiclePart { Name = "All-Season Tire 18\"", Brand = "Lumbini", Price = 12000.00m, StockQuantity = 40, Description = "Reliable all-season performance", VendorId = vendors[1].Id },
                    new VehiclePart { Name = "Oil Filter", Brand = "Himalayan", Price = 1500.00m, StockQuantity = 100, Description = "High-efficiency oil filter", VendorId = vendors[3].Id },
                    new VehiclePart { Name = "Spark Plug", Brand = "FireUp", Price = 899.00m, StockQuantity = 8, Description = "Platinum spark plug", VendorId = vendors[0].Id }
                };
                context.VehicleParts.AddRange(parts);
                await context.SaveChangesAsync();
            }

            if (!await context.Vehicles.AnyAsync())
            {
                var customers = await context.Customers.ToListAsync();
                var vehicles = new List<Vehicle>
                {
                    new Vehicle { CustomerId = customers[0].Id, Brand = "Toyota", Model = "Camry", VehicleNumber = "BA-1-PA-1234", Mileage = 15000 },
                    new Vehicle { CustomerId = customers[0].Id, Brand = "Honda", Model = "Civic", VehicleNumber = "BA-2-PA-5678", Mileage = 12000 },
                    new Vehicle { CustomerId = customers[1].Id, Brand = "Ford", Model = "F-150", VehicleNumber = "BA-3-PA-9012", Mileage = 45000 }
                };
                context.Vehicles.AddRange(vehicles);
                await context.SaveChangesAsync();
            }

            if (!await context.SalesInvoices.AnyAsync())
            {
                var customers = await context.Customers.ToListAsync();
                var parts = await context.VehicleParts.ToListAsync();
                var admin = await context.Users.FirstAsync(u => u.Role == UserRole.Admin);

                var sales = new List<SalesInvoice>
                {
                    new SalesInvoice
                    {
                        CustomerId = customers[0].Id,
                        StaffId = admin.Id,
                        Date = DateTime.UtcNow.AddDays(-2),
                        TotalAmount = 25000.00m,
                        DiscountAmount = 500m,
                        PaymentMethod = "Cash",
                        PaymentStatus = "PAID",
                        Items = new List<SalesItem> { new SalesItem { VehiclePartId = parts[1].Id, Quantity = 3, UnitPrice = 8500.50m, TotalPrice = 25501.50m } }
                    }
                };
                context.SalesInvoices.AddRange(sales);
                await context.SaveChangesAsync();
            }

            if (!await context.Appointments.AnyAsync())
            {
                var customers = await context.Customers.ToListAsync();
                var vehicles = await context.Vehicles.ToListAsync();

                var appointments = new List<Appointment>
                {
                    new Appointment
                    {
                        CustomerId = customers[0].Id,
                        VehicleId = vehicles[0].Id,
                        ServiceType = "Oil & Filter Change",
                        Date = DateTime.UtcNow.AddDays(1),
                        Status = AppointmentStatus.Confirmed,
                        Description = "Regular maintenance checkup."
                    },
                    new Appointment
                    {
                        CustomerId = customers[1].Id,
                        VehicleId = vehicles[2].Id,
                        ServiceType = "Brake Service",
                        Date = DateTime.UtcNow.AddDays(3),
                        Status = AppointmentStatus.Pending,
                        Description = "Squeaky brakes on the front wheels."
                    }
                };
                context.Appointments.AddRange(appointments);
                await context.SaveChangesAsync();
            }
        }
    }
}
