using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using AlMuhandisShop.Services;
using AlMuhandisShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly CashService _cashService;

    public ReportsController(ApplicationDbContext db, CashService cashService)
    {
        _db = db;
        _cashService = cashService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Sales(DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.Sales.Include(s => s.Customer).AsQueryable();
        if (fromDate.HasValue) query = query.Where(s => s.Date >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(s => s.Date < toDate.Value.AddDays(1));

        var sales = await query.OrderByDescending(s => s.Date).ToListAsync();
        var vm = new SalesReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            Sales = sales,
            TotalSales = sales.Sum(s => s.TotalAmount),
            TotalPaid = sales.Sum(s => s.PaidAmount),
            TotalRemaining = sales.Sum(s => s.RemainingAmount)
        };
        return View(vm);
    }

    public async Task<IActionResult> Purchases(DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.Purchases.Include(p => p.Supplier).AsQueryable();
        if (fromDate.HasValue) query = query.Where(p => p.Date >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.Date < toDate.Value.AddDays(1));

        var purchases = await query.OrderByDescending(p => p.Date).ToListAsync();
        var vm = new PurchasesReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            Purchases = purchases,
            TotalPurchases = purchases.Sum(p => p.TotalAmount),
            TotalPaid = purchases.Sum(p => p.PaidAmount),
            TotalRemaining = purchases.Sum(p => p.RemainingAmount)
        };
        return View(vm);
    }

    public async Task<IActionResult> CustomerDebts()
    {
        var customers = await _db.Customers
            .Where(c => c.IsActive)
            .Include(c => c.Sales)
            .Include(c => c.Payments)
            .ToListAsync();

        // Calculate balances
        var customerDebts = new List<(Customer Customer, decimal Balance)>();
        foreach (var c in customers)
        {
            var totalSales = c.Sales.Sum(s => s.TotalAmount);
            var totalPaidOnSales = c.Sales.Sum(s => s.PaidAmount);
            var totalPayments = c.Payments.Sum(p => p.Amount);
            var totalReturns = await _db.SaleReturns
                .Where(sr => sr.Sale.CustomerId == c.Id)
                .SumAsync(sr => (decimal?)sr.TotalAmount) ?? 0m;

            var balance = totalSales - totalPaidOnSales - totalPayments - totalReturns;
            if (balance > 0.01m)
                customerDebts.Add((c, balance));
        }

        ViewBag.CustomerDebts = customerDebts.OrderByDescending(x => x.Balance).ToList();
        ViewBag.TotalDebt = customerDebts.Sum(x => x.Balance);
        return View();
    }

    public async Task<IActionResult> SupplierDebts()
    {
        var suppliers = await _db.Suppliers
            .Where(s => s.IsActive)
            .Include(s => s.Purchases)
            .Include(s => s.Payments)
            .ToListAsync();

        var supplierDebts = new List<(Supplier Supplier, decimal Balance)>();
        foreach (var s in suppliers)
        {
            var totalPurchases = s.Purchases.Sum(p => p.TotalAmount);
            var totalPaidOnPurchases = s.Purchases.Sum(p => p.PaidAmount);
            var totalPayments = s.Payments.Sum(p => p.Amount);
            var totalReturns = await _db.PurchaseReturns
                .Where(pr => pr.SupplierId == s.Id)
                .SumAsync(pr => (decimal?)pr.TotalAmount) ?? 0m;

            var balance = totalPurchases - totalPaidOnPurchases - totalPayments - totalReturns;
            if (balance > 0.01m)
                supplierDebts.Add((s, balance));
        }

        ViewBag.SupplierDebts = supplierDebts.OrderByDescending(x => x.Balance).ToList();
        ViewBag.TotalDebt = supplierDebts.Sum(x => x.Balance);
        return View();
    }

    public async Task<IActionResult> Expenses(DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.Expenses.AsQueryable();
        if (fromDate.HasValue) query = query.Where(e => e.Date >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(e => e.Date < toDate.Value.AddDays(1));

        var expenses = await query.OrderByDescending(e => e.Date).ToListAsync();
        var vm = new ExpensesReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            Expenses = expenses,
            TotalExpenses = expenses.Sum(e => e.Amount)
        };
        return View(vm);
    }

    public async Task<IActionResult> Cash(DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.CashTransactions.AsQueryable();
        if (fromDate.HasValue) query = query.Where(t => t.Date >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(t => t.Date < toDate.Value.AddDays(1));

        var transactions = await query.OrderByDescending(t => t.Date).ToListAsync();
        var vm = new CashReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            Transactions = transactions,
            TotalIn = transactions.Where(t => t.Direction == CashFlowDirection.In).Sum(t => t.Amount),
            TotalOut = transactions.Where(t => t.Direction == CashFlowDirection.Out).Sum(t => t.Amount),
            Balance = await _cashService.GetCurrentBalanceAsync()
        };
        return View(vm);
    }

    public async Task<IActionResult> Stock(string? search, int? categoryId, bool? lowStock)
    {
        var query = _db.Products.Include(p => p.Category).Where(p => p.IsActive).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search));
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (lowStock == true) query = query.Where(p => p.StockQuantity <= 10);

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();

        ViewBag.Products = products;
        ViewBag.Categories = categories;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.LowStock = lowStock;
        ViewBag.TotalValue = products.Sum(p => p.StockQuantity * p.PurchasePrice);
        return View();
    }

    public async Task<IActionResult> Returns(DateTime? fromDate, DateTime? toDate)
    {
        var saleReturnsQuery = _db.SaleReturns.Include(sr => sr.Sale).ThenInclude(s => s.Customer).AsQueryable();
        var purchaseReturnsQuery = _db.PurchaseReturns.Include(pr => pr.Purchase).ThenInclude(p => p.Supplier).AsQueryable();

        if (fromDate.HasValue)
        {
            saleReturnsQuery = saleReturnsQuery.Where(sr => sr.Date >= fromDate.Value);
            purchaseReturnsQuery = purchaseReturnsQuery.Where(pr => pr.Date >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            saleReturnsQuery = saleReturnsQuery.Where(sr => sr.Date < toDate.Value.AddDays(1));
            purchaseReturnsQuery = purchaseReturnsQuery.Where(pr => pr.Date < toDate.Value.AddDays(1));
        }

        ViewBag.SaleReturns = await saleReturnsQuery.OrderByDescending(sr => sr.Date).ToListAsync();
        ViewBag.PurchaseReturns = await purchaseReturnsQuery.OrderByDescending(pr => pr.Date).ToListAsync();
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View();
    }
}
