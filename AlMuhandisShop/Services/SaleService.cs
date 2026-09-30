using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using AlMuhandisShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Services;

public class SaleService
{
    private readonly ApplicationDbContext _db;

    public SaleService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, string? Error, int SaleId)> CreateSaleAsync(CreateSaleViewModel vm)
    {
        if (vm.Items == null || vm.Items.Count == 0)
            return (false, "يجب إضافة منتج واحد على الأقل", 0);

        if (vm.PaidAmount < 0)
            return (false, "المبلغ المدفوع لا يمكن أن يكون سالبًا", 0);

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Validate customer if specified
            if (vm.CustomerId.HasValue)
            {
                var customer = await _db.Customers.FindAsync(vm.CustomerId.Value);
                if (customer == null) return (false, "العميل غير موجود", 0);
                if (!customer.IsActive) return (false, "العميل غير نشط", 0);
            }

            var sale = new Sale
            {
                CustomerId = vm.CustomerId,
                Date = vm.Date,
                PaymentMethod = vm.PaymentMethod,
                Notes = vm.Notes
            };
            _db.Sales.Add(sale);
            await _db.SaveChangesAsync(); // get sale.Id

            decimal total = 0;
            foreach (var item in vm.Items)
            {
                if (item.Quantity <= 0)
                    return (false, "الكمية يجب أن تكون أكبر من صفر", 0);

                var product = await _db.Products.FindAsync(item.ProductId);
                if (product == null) return (false, $"المنتج غير موجود", 0);
                if (!product.IsActive) return (false, $"المنتج '{product.Name}' غير نشط", 0);
                if (product.StockQuantity < item.Quantity)
                    return (false, $"الكمية المتاحة من '{product.Name}' هي {product.StockQuantity} فقط", 0);

                var unitPrice = product.SellingPrice;
                var lineTotal = unitPrice * item.Quantity;

                var saleItem = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    Total = lineTotal
                };
                _db.SaleItems.Add(saleItem);

                // Update stock
                product.StockQuantity -= item.Quantity;
                total += lineTotal;
            }

            if (vm.PaidAmount > total)
                return (false, "المبلغ المدفوع لا يمكن أن يتجاوز إجمالي الفاتورة", 0);

            sale.TotalAmount = total;
            sale.PaidAmount = vm.PaidAmount;
            sale.RemainingAmount = total - vm.PaidAmount;

            // Cash transaction only if money was received
            if (vm.PaidAmount > 0 && vm.PaymentMethod == PaymentMethod.Cash)
            {
                _db.CashTransactions.Add(new CashTransaction
                {
                    Date = vm.Date,
                    Type = CashTransactionType.Sale,
                    Direction = CashFlowDirection.In,
                    Amount = vm.PaidAmount,
                    ReferenceId = sale.Id,
                    Notes = $"بيع رقم {sale.Id}"
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return (true, null, sale.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, (string?)$"خطأ أثناء حفظ الفاتورة: {ex.Message}", 0);
        }
        });
    }

    public async Task<SaleDetailViewModel?> GetSaleDetailAsync(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems).ThenInclude(si => si.Product)
            .Include(s => s.SaleReturns).ThenInclude(sr => sr.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null) return null;

        return new SaleDetailViewModel
        {
            Sale = sale,
            TotalReturned = sale.SaleReturns.Sum(sr => sr.TotalAmount)
        };
    }

    public async Task<List<Sale>> GetRecentSalesAsync(int count = 10)
    {
        return await _db.Sales
            .Include(s => s.Customer)
            .OrderByDescending(s => s.Date)
            .Take(count)
            .ToListAsync();
    }
}
