using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using AlMuhandisShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Services;

public class PurchaseService
{
    private readonly ApplicationDbContext _db;

    public PurchaseService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, string? Error, int PurchaseId)> CreatePurchaseAsync(CreatePurchaseViewModel vm)
    {
        if (vm.Items == null || vm.Items.Count == 0)
            return (false, "يجب إضافة منتج واحد على الأقل", 0);

        if (vm.PaidAmount < 0)
            return (false, "المبلغ المدفوع لا يمكن أن يكون سالبًا", 0);

        var supplier = await _db.Suppliers.FindAsync(vm.SupplierId);
        if (supplier == null) return (false, "المورد غير موجود", 0);
        if (!supplier.IsActive) return (false, "المورد غير نشط", 0);

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var purchase = new Purchase
            {
                SupplierId = vm.SupplierId,
                Date = vm.Date,
                PaymentMethod = vm.PaymentMethod,
                Notes = vm.Notes
            };
            _db.Purchases.Add(purchase);
            await _db.SaveChangesAsync();

            decimal total = 0;
            foreach (var item in vm.Items)
            {
                if (item.Quantity <= 0)
                    return (false, "الكمية يجب أن تكون أكبر من صفر", 0);
                if (item.UnitPrice < 0)
                    return (false, "سعر الوحدة لا يمكن أن يكون سالبًا", 0);

                var product = await _db.Products.FindAsync(item.ProductId);
                if (product == null) return (false, $"المنتج غير موجود", 0);

                var lineTotal = item.UnitPrice * item.Quantity;
                _db.PurchaseItems.Add(new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = lineTotal
                });

                // Increase stock
                product.StockQuantity += item.Quantity;
                // Update purchase price
                product.PurchasePrice = item.UnitPrice;
                total += lineTotal;
            }

            if (vm.PaidAmount > total)
                return (false, "المبلغ المدفوع لا يمكن أن يتجاوز إجمالي الفاتورة", 0);

            purchase.TotalAmount = total;
            purchase.PaidAmount = vm.PaidAmount;
            purchase.RemainingAmount = total - vm.PaidAmount;

            if (vm.PaidAmount > 0 && vm.PaymentMethod == PaymentMethod.Cash)
            {
                _db.CashTransactions.Add(new CashTransaction
                {
                    Date = vm.Date,
                    Type = CashTransactionType.Purchase,
                    Direction = CashFlowDirection.Out,
                    Amount = vm.PaidAmount,
                    ReferenceId = purchase.Id,
                    Notes = $"شراء رقم {purchase.Id}"
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return (true, null, purchase.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, (string?)$"خطأ أثناء حفظ الفاتورة: {ex.Message}", 0);
        }
        });
    }

    public async Task<List<Purchase>> GetRecentPurchasesAsync(int count = 10)
    {
        return await _db.Purchases
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.Date)
            .Take(count)
            .ToListAsync();
    }
}
