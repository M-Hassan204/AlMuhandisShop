using AlMuhandisShop.Data;
using AlMuhandisShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly CashService _cashService;

    public DashboardService(ApplicationDbContext db, CashService cashService)
    {
        _db = db;
        _cashService = cashService;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var todayEnd = today.AddDays(1);

        var todaySales = await _db.Sales
            .Where(s => s.Date >= today && s.Date < todayEnd)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

        var todayPurchases = await _db.Purchases
            .Where(p => p.Date >= today && p.Date < todayEnd)
            .SumAsync(p => (decimal?)p.TotalAmount) ?? 0m;

        var cashBalance = await _cashService.GetCurrentBalanceAsync();

        // Total customer debts (remaining amounts on sales)
        var totalCustomerDebt = await _db.Sales
            .SumAsync(s => (decimal?)s.RemainingAmount) ?? 0m;
        
        // Subtract sale returns that reduced the balance
        var totalSaleReturns = await _db.SaleReturns
            .SumAsync(sr => (decimal?)sr.TotalAmount) ?? 0m;
        totalCustomerDebt = Math.Max(0, totalCustomerDebt - totalSaleReturns);
        
        // Add customer payments already captured in sale.PaidAmount — the real approach:
        // Total customer balance = sum of remaining amounts in Sales
        // But we need to subtract subsequent CustomerPayments too
        var totalSubsequentPayments = await _db.CustomerPayments
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        totalCustomerDebt = Math.Max(0, totalCustomerDebt - totalSubsequentPayments);

        // Total supplier debts
        var totalSupplierDebt = await _db.Purchases
            .SumAsync(p => (decimal?)p.RemainingAmount) ?? 0m;
        
        var totalPurchaseReturns = await _db.PurchaseReturns
            .SumAsync(pr => (decimal?)pr.TotalAmount) ?? 0m;
        totalSupplierDebt = Math.Max(0, totalSupplierDebt - totalPurchaseReturns);
        
        var totalSupplierPayments = await _db.SupplierPayments
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        totalSupplierDebt = Math.Max(0, totalSupplierDebt - totalSupplierPayments);

        var productCount = await _db.Products.CountAsync(p => p.IsActive);
        var lowStockProducts = await _db.Products
            .Where(p => p.IsActive && p.StockQuantity <= 10)
            .Include(p => p.Category)
            .OrderBy(p => p.StockQuantity)
            .Take(10)
            .ToListAsync();

        var recentSales = await _db.Sales
            .Include(s => s.Customer)
            .OrderByDescending(s => s.Date)
            .Take(5)
            .ToListAsync();

        var recentPurchases = await _db.Purchases
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.Date)
            .Take(5)
            .ToListAsync();

        // Last 7 days chart data
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .Reverse()
            .ToList();

        var salesChart = new List<ChartDataPoint>();
        var purchasesChart = new List<ChartDataPoint>();

        foreach (var day in last7Days)
        {
            var dayEnd = day.AddDays(1);
            var daySales = await _db.Sales
                .Where(s => s.Date >= day && s.Date < dayEnd)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
            var dayPurchases = await _db.Purchases
                .Where(p => p.Date >= day && p.Date < dayEnd)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0m;

            salesChart.Add(new ChartDataPoint { Label = day.ToString("MM/dd"), Value = daySales });
            purchasesChart.Add(new ChartDataPoint { Label = day.ToString("MM/dd"), Value = dayPurchases });
        }

        return new DashboardViewModel
        {
            TodaySales = todaySales,
            TodayPurchases = todayPurchases,
            CashBalance = cashBalance,
            TotalCustomerDebt = totalCustomerDebt,
            TotalSupplierDebt = totalSupplierDebt,
            ProductCount = productCount,
            LowStockProducts = lowStockProducts,
            RecentSales = recentSales,
            RecentPurchases = recentPurchases,
            SalesChart = salesChart,
            PurchasesChart = purchasesChart
        };
    }
}
