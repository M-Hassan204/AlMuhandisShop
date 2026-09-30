using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using AlMuhandisShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Services;

public class ReturnService
{
    private readonly ApplicationDbContext _db;

    public ReturnService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, string? Error, int ReturnId)> CreateSaleReturnAsync(CreateSaleReturnViewModel vm)
    {
        if (vm.Items == null || vm.Items.Count == 0)
            return (false, "يجب إضافة منتج واحد على الأقل", 0);

        var sale = await _db.Sales
            .Include(s => s.SaleItems)
            .Include(s => s.SaleReturns).ThenInclude(sr => sr.Items)
            .FirstOrDefaultAsync(s => s.Id == vm.SaleId);

        if (sale == null) return (false, "فاتورة البيع غير موجودة", 0);

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var saleReturn = new SaleReturn
            {
                SaleId = vm.SaleId,
                Date = vm.Date,
                Notes = vm.Notes
            };
            _db.SaleReturns.Add(saleReturn);
            await _db.SaveChangesAsync();

            decimal total = 0;
            foreach (var item in vm.Items)
            {
                if (item.Quantity <= 0)
                    return (false, "الكمية يجب أن تكون أكبر من صفر", 0);

                // Validate against original sale quantity
                var originalItem = sale.SaleItems.FirstOrDefault(si => si.ProductId == item.ProductId);
                if (originalItem == null)
                    return (false, "المنتج غير موجود في فاتورة البيع الأصلية", 0);

                var alreadyReturned = sale.SaleReturns
                    .SelectMany(sr => sr.Items)
                    .Where(i => i.ProductId == item.ProductId)
                    .Sum(i => i.Quantity);

                if (item.Quantity > originalItem.Quantity - alreadyReturned)
                    return (false, $"الكمية المرتجعة تتجاوز الكمية المتاحة للإرجاع", 0);

                var product = await _db.Products.FindAsync(item.ProductId);
                if (product == null) return (false, "المنتج غير موجود", 0);

                var lineTotal = originalItem.UnitPrice * item.Quantity;
                _db.SaleReturnItems.Add(new SaleReturnItem
                {
                    SaleReturnId = saleReturn.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = originalItem.UnitPrice,
                    Total = lineTotal
                });

                // Return to stock
                product.StockQuantity += item.Quantity;
                total += lineTotal;
            }

            saleReturn.TotalAmount = total;

            // Cash refund if applicable
            if (total > 0)
            {
                _db.CashTransactions.Add(new CashTransaction
                {
                    Date = vm.Date,
                    Type = CashTransactionType.SaleReturn,
                    Direction = CashFlowDirection.Out,
                    Amount = total,
                    ReferenceId = saleReturn.Id,
                    Notes = $"مرتجع مبيعات رقم {saleReturn.Id}"
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return (true, null, saleReturn.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, (string?)$"خطأ: {ex.Message}", 0);
        }
        });
    }

    public async Task<(bool Success, string? Error, int ReturnId)> CreatePurchaseReturnAsync(CreatePurchaseReturnViewModel vm)
    {
        if (vm.Items == null || vm.Items.Count == 0)
            return (false, "يجب إضافة منتج واحد على الأقل", 0);

        var purchase = await _db.Purchases
            .Include(p => p.PurchaseItems)
            .Include(p => p.PurchaseReturns).ThenInclude(pr => pr.Items)
            .FirstOrDefaultAsync(p => p.Id == vm.PurchaseId);

        if (purchase == null) return (false, "فاتورة الشراء غير موجودة", 0);

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var purchaseReturn = new PurchaseReturn
            {
                PurchaseId = vm.PurchaseId,
                SupplierId = purchase.SupplierId,
                Date = vm.Date,
                Notes = vm.Notes
            };
            _db.PurchaseReturns.Add(purchaseReturn);
            await _db.SaveChangesAsync();

            decimal total = 0;
            foreach (var item in vm.Items)
            {
                if (item.Quantity <= 0)
                    return (false, "الكمية يجب أن تكون أكبر من صفر", 0);

                var originalItem = purchase.PurchaseItems.FirstOrDefault(pi => pi.ProductId == item.ProductId);
                if (originalItem == null)
                    return (false, "المنتج غير موجود في فاتورة الشراء الأصلية", 0);

                var alreadyReturned = purchase.PurchaseReturns
                    .SelectMany(pr => pr.Items)
                    .Where(i => i.ProductId == item.ProductId)
                    .Sum(i => i.Quantity);

                if (item.Quantity > originalItem.Quantity - alreadyReturned)
                    return (false, $"الكمية المرتجعة تتجاوز الكمية المتاحة للإرجاع", 0);

                var product = await _db.Products.FindAsync(item.ProductId);
                if (product == null) return (false, "المنتج غير موجود", 0);
                if (product.StockQuantity < item.Quantity)
                    return (false, $"الكمية المتاحة من '{product.Name}' في المخزن غير كافية للإرجاع", 0);

                var lineTotal = originalItem.UnitPrice * item.Quantity;
                _db.PurchaseReturnItems.Add(new PurchaseReturnItem
                {
                    PurchaseReturnId = purchaseReturn.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = originalItem.UnitPrice,
                    Total = lineTotal
                });

                // Remove from stock
                product.StockQuantity -= item.Quantity;
                total += lineTotal;
            }

            purchaseReturn.TotalAmount = total;

            // Cash inflow from purchase return
            if (total > 0)
            {
                _db.CashTransactions.Add(new CashTransaction
                {
                    Date = vm.Date,
                    Type = CashTransactionType.PurchaseReturn,
                    Direction = CashFlowDirection.In,
                    Amount = total,
                    ReferenceId = purchaseReturn.Id,
                    Notes = $"مرتجع مشتريات رقم {purchaseReturn.Id}"
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return (true, null, purchaseReturn.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, (string?)$"خطأ: {ex.Message}", 0);
        }
        });
    }
}
