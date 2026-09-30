using AlMuhandisShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply pending migrations
        await db.Database.MigrateAsync();

        // Seed roles
        string[] roles = ["Admin", "Employee"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed admin user
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@almuhandis.com",
                FullName = "مدير النظام",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123456");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed employee user
        if (await userManager.FindByNameAsync("employee") == null)
        {
            var emp = new ApplicationUser
            {
                UserName = "employee",
                Email = "employee@almuhandis.com",
                FullName = "موظف المبيعات",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(emp, "Emp@123456");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(emp, "Employee");
        }

        // Seed categories
        if (!await db.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "مبيدات حشرية" },
                new() { Name = "مبيدات فطرية" },
                new() { Name = "مبيدات حشائش" },
                new() { Name = "أسمدة" },
                new() { Name = "بذور" },
                new() { Name = "أخرى" }
            };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();
        }

        // Seed suppliers
        if (!await db.Suppliers.AnyAsync())
        {
            var suppliers = new List<Supplier>
            {
                new() { Name = "أحمد الصالح", CompanyName = "شركة الصالح للمبيدات", Phone = "0501234567", Address = "الرياض - حي العليا", IsActive = true },
                new() { Name = "محمد العتيبي", CompanyName = "مؤسسة العتيبي للأسمدة", Phone = "0559876543", Address = "جدة - حي الروضة", IsActive = true },
                new() { Name = "خالد الدوسري", CompanyName = "شركة الأخضر الزراعية", Phone = "0531112233", Address = "الدمام - حي النزهة", IsActive = true }
            };
            db.Suppliers.AddRange(suppliers);
            await db.SaveChangesAsync();
        }

        // Seed customers
        if (!await db.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new() { Name = "عبدالله المطيري", Phone = "0501122334", Address = "الرياض", IsActive = true },
                new() { Name = "سعد الشمري", Phone = "0559988776", Address = "الخرج", IsActive = true },
                new() { Name = "فهد الحربي", Phone = "0533445566", Address = "المزاحمية", IsActive = true },
                new() { Name = "ناصر العنزي", Phone = "0507766554", Address = "الدوادمي", IsActive = true },
                new() { Name = "يوسف القحطاني", Phone = "0562233445", Address = "الرياض", IsActive = true }
            };
            db.Customers.AddRange(customers);
            await db.SaveChangesAsync();
        }

        // Seed products
        if (!await db.Products.AnyAsync())
        {
            var cats = await db.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

            var products = new List<Product>
            {
                new() { Name = "كلوربيريفوس 48% EC", CategoryId = cats["مبيدات حشرية"], PurchasePrice = 45m, SellingPrice = 65m, StockQuantity = 50, Unit = "لتر", IsActive = true },
                new() { Name = "ايميداكلوبريد 70% WG", CategoryId = cats["مبيدات حشرية"], PurchasePrice = 120m, SellingPrice = 165m, StockQuantity = 30, Unit = "كجم", IsActive = true },
                new() { Name = "مانكوزيب 80% WP", CategoryId = cats["مبيدات فطرية"], PurchasePrice = 35m, SellingPrice = 55m, StockQuantity = 40, Unit = "كجم", IsActive = true },
                new() { Name = "ثايوفانيت ميثيل 70% WP", CategoryId = cats["مبيدات فطرية"], PurchasePrice = 85m, SellingPrice = 120m, StockQuantity = 25, Unit = "كجم", IsActive = true },
                new() { Name = "جلايفوسيت 41% SL", CategoryId = cats["مبيدات حشائش"], PurchasePrice = 28m, SellingPrice = 45m, StockQuantity = 60, Unit = "لتر", IsActive = true },
                new() { Name = "2,4-D 72% SL", CategoryId = cats["مبيدات حشائش"], PurchasePrice = 20m, SellingPrice = 35m, StockQuantity = 45, Unit = "لتر", IsActive = true },
                new() { Name = "نترات الأمونيوم 33%", CategoryId = cats["أسمدة"], PurchasePrice = 15m, SellingPrice = 25m, StockQuantity = 200, Unit = "كجم", IsActive = true },
                new() { Name = "سوبر فوسفات الكالسيوم", CategoryId = cats["أسمدة"], PurchasePrice = 12m, SellingPrice = 20m, StockQuantity = 150, Unit = "كجم", IsActive = true },
                new() { Name = "سماد NPK 20-20-20", CategoryId = cats["أسمدة"], PurchasePrice = 55m, SellingPrice = 80m, StockQuantity = 80, Unit = "كجم", IsActive = true },
                new() { Name = "بذور طماطم هجين", CategoryId = cats["بذور"], PurchasePrice = 200m, SellingPrice = 280m, StockQuantity = 20, Unit = "علبة", IsActive = true },
                new() { Name = "بذور خيار هجين", CategoryId = cats["بذور"], PurchasePrice = 180m, SellingPrice = 250m, StockQuantity = 15, Unit = "علبة", IsActive = true },
                new() { Name = "رش ظهري 16 لتر", CategoryId = cats["أخرى"], PurchasePrice = 95m, SellingPrice = 140m, StockQuantity = 8, Unit = "قطعة", IsActive = true }
            };
            db.Products.AddRange(products);
            await db.SaveChangesAsync();
        }

        // Seed some purchases and sales only if DB is fresh
        if (!await db.Purchases.AnyAsync())
        {
            var supplier1 = await db.Suppliers.FirstAsync();
            var products = await db.Products.Take(4).ToListAsync();

            var purchase = new Purchase
            {
                SupplierId = supplier1.Id,
                Date = DateTime.UtcNow.AddDays(-15),
                PaymentMethod = PaymentMethod.Cash,
                PaidAmount = 2000m,
                TotalAmount = 2500m,
                RemainingAmount = 500m,
                Notes = "فاتورة شراء أولى"
            };
            db.Purchases.Add(purchase);
            await db.SaveChangesAsync();

            db.PurchaseItems.AddRange(
                new PurchaseItem { PurchaseId = purchase.Id, ProductId = products[0].Id, Quantity = 20, UnitPrice = products[0].PurchasePrice, Total = 20 * products[0].PurchasePrice },
                new PurchaseItem { PurchaseId = purchase.Id, ProductId = products[1].Id, Quantity = 10, UnitPrice = products[1].PurchasePrice, Total = 10 * products[1].PurchasePrice }
            );
            db.CashTransactions.Add(new CashTransaction
            {
                Date = purchase.Date,
                Type = CashTransactionType.Purchase,
                Direction = CashFlowDirection.Out,
                Amount = 2000m,
                ReferenceId = purchase.Id,
                Notes = $"شراء رقم {purchase.Id}"
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Sales.AnyAsync())
        {
            var customer1 = await db.Customers.FirstAsync();
            var products = await db.Products.Take(3).ToListAsync();

            var sale = new Sale
            {
                CustomerId = customer1.Id,
                Date = DateTime.UtcNow.AddDays(-7),
                PaymentMethod = PaymentMethod.Cash,
                PaidAmount = 800m,
                TotalAmount = 1000m,
                RemainingAmount = 200m,
                Notes = "أول فاتورة بيع"
            };
            db.Sales.Add(sale);
            await db.SaveChangesAsync();

            db.SaleItems.AddRange(
                new SaleItem { SaleId = sale.Id, ProductId = products[0].Id, Quantity = 5, UnitPrice = products[0].SellingPrice, Total = 5 * products[0].SellingPrice },
                new SaleItem { SaleId = sale.Id, ProductId = products[2].Id, Quantity = 8, UnitPrice = products[2].SellingPrice, Total = 8 * products[2].SellingPrice }
            );
            db.CashTransactions.Add(new CashTransaction
            {
                Date = sale.Date,
                Type = CashTransactionType.Sale,
                Direction = CashFlowDirection.In,
                Amount = 800m,
                ReferenceId = sale.Id,
                Notes = $"بيع رقم {sale.Id}"
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Expenses.AnyAsync())
        {
            var expenses = new List<Expense>
            {
                new() { Name = "إيجار المحل", Amount = 3000m, Date = DateTime.UtcNow.AddDays(-30), Notes = "إيجار شهر أغسطس" },
                new() { Name = "فاتورة الكهرباء", Amount = 450m, Date = DateTime.UtcNow.AddDays(-20) },
                new() { Name = "مصاريف نقل", Amount = 200m, Date = DateTime.UtcNow.AddDays(-10) }
            };
            db.Expenses.AddRange(expenses);
            await db.SaveChangesAsync();

            // Add cash transactions for expenses
            foreach (var exp in await db.Expenses.ToListAsync())
            {
                db.CashTransactions.Add(new CashTransaction
                {
                    Date = exp.Date,
                    Type = CashTransactionType.Expense,
                    Direction = CashFlowDirection.Out,
                    Amount = exp.Amount,
                    ReferenceId = exp.Id,
                    Notes = exp.Name
                });
            }
            await db.SaveChangesAsync();
        }
    }
}
