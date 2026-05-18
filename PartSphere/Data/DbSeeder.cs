using Microsoft.EntityFrameworkCore;
using PartSphere.Models;

namespace PartSphere.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // 1. Admin User seeding / update
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "anup@partsphere.com")
                            ?? await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@partsphere.com")
                            ?? await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    Name = "Anup Sharma",
                    Email = "anup@partsphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = UserRole.Admin,
                    IsActive = true
                };
                context.Users.Add(adminUser);
            }
            else
            {
                adminUser.Name = "Anup Sharma";
                adminUser.Email = "anup@partsphere.com";
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
                context.Users.Update(adminUser);
            }
            await context.SaveChangesAsync();

            // 2. Staff User seeding / update
            var staffUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "jeewan@partsphere.com")
                            ?? await context.Users.FirstOrDefaultAsync(u => u.Email == "staff@partsphere.com")
                            ?? await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Staff);

            if (staffUser == null)
            {
                staffUser = new User
                {
                    Name = "Jeewan Adhikari",
                    Email = "jeewan@partsphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = UserRole.Staff,
                    IsActive = true
                };
                context.Users.Add(staffUser);
            }
            else
            {
                staffUser.Name = "Jeewan Adhikari";
                staffUser.Email = "jeewan@partsphere.com";
                staffUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
                context.Users.Update(staffUser);
            }
            await context.SaveChangesAsync();

            // 3. Customer User seeding / update (Aastha Aryal)
            var customerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "aastha.aryal@gmail.com")
                               ?? await context.Users.FirstOrDefaultAsync(u => u.Email == "alice@example.com")
                               ?? await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Customer);

            if (customerUser == null)
            {
                customerUser = new User
                {
                    Name = "Aastha Aryal",
                    Email = "aastha.aryal@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = UserRole.Customer,
                    IsActive = true
                };
                context.Users.Add(customerUser);
            }
            else
            {
                customerUser.Name = "Aastha Aryal";
                customerUser.Email = "aastha.aryal@gmail.com";
                customerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
                context.Users.Update(customerUser);
            }
            await context.SaveChangesAsync();

            // 4. Vendors
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

            // 5. Customers
            var aasthaCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == "aastha.aryal@gmail.com")
                                 ?? await context.Customers.FirstOrDefaultAsync(c => c.Email == "alice@example.com");

            if (aasthaCustomer == null)
            {
                aasthaCustomer = new Customer
                {
                    Name = "Aastha Aryal",
                    Email = "aastha.aryal@gmail.com",
                    Phone = "9841234567",
                    Address = "Baneshwor, Kathmandu",
                    UserId = customerUser.Id
                };
                context.Customers.Add(aasthaCustomer);
            }
            else
            {
                aasthaCustomer.Name = "Aastha Aryal";
                aasthaCustomer.Email = "aastha.aryal@gmail.com";
                aasthaCustomer.UserId = customerUser.Id;
                context.Customers.Update(aasthaCustomer);
            }

            var biratCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == "birat.bhatta@gmail.com")
                                ?? await context.Customers.FirstOrDefaultAsync(c => c.Email == "bob@example.com");

            if (biratCustomer == null)
            {
                biratCustomer = new Customer
                {
                    Name = "Birat Bhatta",
                    Email = "birat.bhatta@gmail.com",
                    Phone = "9851098765",
                    Address = "Patan, Lalitpur"
                };
                context.Customers.Add(biratCustomer);
            }
            else
            {
                biratCustomer.Name = "Birat Bhatta";
                biratCustomer.Email = "birat.bhatta@gmail.com";
                context.Customers.Update(biratCustomer);
            }

            var chandraCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == "chandra.shrestha@gmail.com")
                                  ?? await context.Customers.FirstOrDefaultAsync(c => c.Email == "charlie@example.com");

            if (chandraCustomer == null)
            {
                chandraCustomer = new Customer
                {
                    Name = "Chandra Shrestha",
                    Email = "chandra.shrestha@gmail.com",
                    Phone = "9801122334",
                    Address = "Lakhechaur, Pokhara"
                };
                context.Customers.Add(chandraCustomer);
            }
            else
            {
                chandraCustomer.Name = "Chandra Shrestha";
                chandraCustomer.Email = "chandra.shrestha@gmail.com";
                context.Customers.Update(chandraCustomer);
            }
            await context.SaveChangesAsync();

            // Link Customer User to profile (double check)
            if (customerUser != null && aasthaCustomer != null)
            {
                aasthaCustomer.UserId = customerUser.Id;
                context.Customers.Update(aasthaCustomer);
                await context.SaveChangesAsync();
            }

            // 6. Parts
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

            // 7. Vehicles
            var existingVehicles = await context.Vehicles.Where(v => v.CustomerId == aasthaCustomer.Id).ToListAsync();
            if (existingVehicles.Count == 0)
            {
                var vehicles = new List<Vehicle>
                {
                    new Vehicle { CustomerId = aasthaCustomer.Id, Brand = "Toyota", Model = "Camry", VehicleNumber = "BA-1-PA-1234", Mileage = 15000 },
                    new Vehicle { CustomerId = aasthaCustomer.Id, Brand = "Honda", Model = "Civic", VehicleNumber = "BA-2-PA-5678", Mileage = 12000 }
                };
                context.Vehicles.AddRange(vehicles);
                
                // Add one for Birat as well
                context.Vehicles.Add(new Vehicle { CustomerId = biratCustomer.Id, Brand = "Ford", Model = "F-150", VehicleNumber = "BA-3-PA-9012", Mileage = 45000 });
                await context.SaveChangesAsync();
            }

            // 8. Sales Invoices
            if (!await context.SalesInvoices.AnyAsync())
            {
                var parts = await context.VehicleParts.ToListAsync();
                var sales = new List<SalesInvoice>
                {
                    new SalesInvoice
                    {
                        CustomerId = aasthaCustomer.Id,
                        StaffId = adminUser.Id,
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

            // 9. Appointments
            if (!await context.Appointments.AnyAsync())
            {
                var vehicles = await context.Vehicles.ToListAsync();
                var appointments = new List<Appointment>
                {
                    new Appointment
                    {
                        CustomerId = aasthaCustomer.Id,
                        VehicleId = vehicles[0].Id,
                        ServiceType = "Oil & Filter Change",
                        Date = DateTime.UtcNow.AddDays(1),
                        Status = AppointmentStatus.Confirmed,
                        Description = "Regular maintenance checkup."
                    },
                    new Appointment
                    {
                        CustomerId = biratCustomer.Id,
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
