using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _db;

    public PaymentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        var totalSales = await _db.Sales
            .Where(s => s.CustomerId == customerId)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

        var totalPaid = await _db.CustomerPayments
            .Where(p => p.CustomerId == customerId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var totalSalePaidDirectly = await _db.Sales
            .Where(s => s.CustomerId == customerId)
            .SumAsync(s => (decimal?)s.PaidAmount) ?? 0m;

        var totalReturns = await _db.SaleReturns
            .Where(sr => sr.Sale.CustomerId == customerId)
            .SumAsync(sr => (decimal?)sr.TotalAmount) ?? 0m;

        return totalSales - totalSalePaidDirectly - totalPaid - totalReturns;
    }

    public async Task<decimal> GetSupplierBalanceAsync(int supplierId)
    {
        var totalPurchases = await _db.Purchases
            .Where(p => p.SupplierId == supplierId)
            .SumAsync(p => (decimal?)p.TotalAmount) ?? 0m;

        var totalPaidDirectly = await _db.Purchases
            .Where(p => p.SupplierId == supplierId)
            .SumAsync(p => (decimal?)p.PaidAmount) ?? 0m;

        var totalPayments = await _db.SupplierPayments
            .Where(p => p.SupplierId == supplierId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var totalReturns = await _db.PurchaseReturns
            .Where(pr => pr.SupplierId == supplierId)
            .SumAsync(pr => (decimal?)pr.TotalAmount) ?? 0m;

        return totalPurchases - totalPaidDirectly - totalPayments - totalReturns;
    }

    public async Task<(bool Success, string? Error)> RecordCustomerPaymentAsync(
        int customerId, decimal amount, DateTime date, PaymentMethod method, string? notes)
    {
        if (amount <= 0) return (false, "المبلغ يجب أن يكون أكبر من صفر");

        var customer = await _db.Customers.FindAsync(customerId);
        if (customer == null) return (false, "العميل غير موجود");

        var balance = await GetCustomerBalanceAsync(customerId);
        if (amount > balance + 0.01m)
            return (false, $"المبلغ يتجاوز الرصيد المستحق ({balance:N2})");

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var payment = new CustomerPayment
            {
                CustomerId = customerId,
                Amount = amount,
                Date = date,
                PaymentMethod = method,
                Notes = notes
            };
            _db.CustomerPayments.Add(payment);
            await _db.SaveChangesAsync();

            if (method == PaymentMethod.Cash)
            {
                _db.CashTransactions.Add(new CashTransaction
                {
                    Date = date,
                    Type = CashTransactionType.CustomerPayment,
                    Direction = CashFlowDirection.In,
                    Amount = amount,
                    ReferenceId = payment.Id,
                    Notes = $"دفعة من عميل: {customer.Name}"
                });
                await _db.SaveChangesAsync();
            }

            await tx.CommitAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, (string?)$"خطأ: {ex.Message}");
        }
        });
    }

    public async Task<(bool Success, string? Error)> RecordSupplierPaymentAsync(
        int supplierId, decimal amount, DateTime date, PaymentMethod method, string? notes)
    {
        if (amount <= 0) return (false, "المبلغ يجب أن يكون أكبر من صفر");

        var supplier = await _db.Suppliers.FindAsync(supplierId);
        if (supplier == null) return (false, "المورد غير موجود");

        var balance = await GetSupplierBalanceAsync(supplierId);
        if (amount > balance + 0.01m)
            return (false, $"المبلغ يتجاوز الرصيد المستحق ({balance:N2})");

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var payment = new SupplierPayment
            {
                SupplierId = supplierId,
                Amount = amount,
                Date = date,
                PaymentMethod = method,
                Notes = notes
            };
            _db.SupplierPayments.Add(payment);
            await _db.SaveChangesAsync();

            if (method == PaymentMethod.Cash)
            {
                _db.CashTransactions.Add(new CashTransaction
                {
                    Date = date,
                    Type = CashTransactionType.SupplierPayment,
                    Direction = CashFlowDirection.Out,
                    Amount = amount,
                    ReferenceId = payment.Id,
                    Notes = $"دفعة لمورد: {supplier.Name}"
                });
                await _db.SaveChangesAsync();
            }

            await tx.CommitAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, (string?)$"خطأ: {ex.Message}");
        }
        });
    }
}
