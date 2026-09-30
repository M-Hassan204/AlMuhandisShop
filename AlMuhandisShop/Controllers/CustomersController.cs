using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using AlMuhandisShop.Services;
using AlMuhandisShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly PaymentService _paymentService;

    public CustomersController(ApplicationDbContext db, PaymentService paymentService)
    {
        _db = db;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.Phone != null && c.Phone.Contains(search)));

        var customers = await query.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Search = search;
        return View(customers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        var sales = await _db.Sales
            .Where(s => s.CustomerId == id)
            .Include(s => s.SaleItems)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        var payments = await _db.CustomerPayments
            .Where(p => p.CustomerId == id)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        var totalSales = sales.Sum(s => s.TotalAmount);
        var totalPaidOnSales = sales.Sum(s => s.PaidAmount);
        var totalPayments = payments.Sum(p => p.Amount);
        var totalReturns = await _db.SaleReturns
            .Where(sr => sr.Sale.CustomerId == id)
            .SumAsync(sr => (decimal?)sr.TotalAmount) ?? 0m;

        var vm = new CustomerAccountViewModel
        {
            Customer = customer,
            TotalSales = totalSales,
            TotalPaidOnSales = totalPaidOnSales,
            TotalPayments = totalPayments,
            TotalReturns = totalReturns,
            OutstandingBalance = totalSales - totalPaidOnSales - totalPayments - totalReturns,
            Sales = sales,
            Payments = payments
        };

        return View(vm);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer model)
    {
        if (!ModelState.IsValid) return View(model);
        model.CreatedAt = DateTime.UtcNow;
        _db.Customers.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة العميل بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Customer model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Customers.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تحديث بيانات العميل";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        customer.IsActive = !customer.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = customer.IsActive ? "تم تفعيل العميل" : "تم إيقاف العميل";
        return RedirectToAction(nameof(Index));
    }

    // Record a payment
    public async Task<IActionResult> Pay(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        var balance = await _paymentService.GetCustomerBalanceAsync(id);
        ViewBag.Balance = balance;
        ViewBag.CustomerId = id;
        ViewBag.CustomerName = customer.Name;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Pay(int id, decimal amount, DateTime date, PaymentMethod method, string? notes)
    {
        var (success, error) = await _paymentService.RecordCustomerPaymentAsync(id, amount, date, method, notes);
        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Pay), new { id });
        }
        TempData["Success"] = "تم تسجيل الدفعة بنجاح";
        return RedirectToAction(nameof(Details), new { id });
    }
}
