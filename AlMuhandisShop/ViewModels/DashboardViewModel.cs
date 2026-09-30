using AlMuhandisShop.Models;

namespace AlMuhandisShop.ViewModels;

public class DashboardViewModel
{
    public decimal TodaySales { get; set; }
    public decimal TodayPurchases { get; set; }
    public decimal CashBalance { get; set; }
    public decimal TotalCustomerDebt { get; set; }
    public decimal TotalSupplierDebt { get; set; }
    public int ProductCount { get; set; }
    public List<Product> LowStockProducts { get; set; } = new();
    public List<Sale> RecentSales { get; set; } = new();
    public List<Purchase> RecentPurchases { get; set; } = new();
    public List<ChartDataPoint> SalesChart { get; set; } = new();
    public List<ChartDataPoint> PurchasesChart { get; set; } = new();
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class SaleDetailViewModel
{
    public Sale Sale { get; set; } = null!;
    public decimal TotalReturned { get; set; }
    public decimal NetAmount => Sale.TotalAmount - TotalReturned;
}
