using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using AlMuhandisShop.Services;
using AlMuhandisShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly PaymentService _paymentService;

    public SuppliersController(ApplicationDbContext db, PaymentService paymentService)
    {
        _db = db;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search) || (s.CompanyName != null && s.CompanyName.Contains(search)));

        var suppliers = await query.OrderBy(s => s.Name).ToListAsync();
        ViewBag.Search = search;
        return View(suppliers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        var purchases = await _db.Purchases
            .Where(p => p.SupplierId == id)
            .Include(p => p.PurchaseItems)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        var payments = await _db.SupplierPayments
            .Where(p => p.SupplierId == id)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        var totalPurchases = purchases.Sum(p => p.TotalAmount);
        var totalPaidOnPurchases = purchases.Sum(p => p.PaidAmount);
        var totalPayments = payments.Sum(p => p.Amount);
        var totalReturns = await _db.PurchaseReturns
            .Where(pr => pr.SupplierId == id)
            .SumAsync(pr => (decimal?)pr.TotalAmount) ?? 0m;

        var vm = new SupplierAccountViewModel
        {
            Supplier = supplier,
            TotalPurchases = totalPurchases,
            TotalPaidOnPurchases = totalPaidOnPurchases,
            TotalPayments = totalPayments,
            TotalReturns = totalReturns,
            OutstandingBalance = totalPurchases - totalPaidOnPurchases - totalPayments - totalReturns,
            Purchases = purchases,
            Payments = payments
        };

        return View(vm);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Supplier model)
    {
        if (!ModelState.IsValid) return View(model);
        model.CreatedAt = DateTime.UtcNow;
        _db.Suppliers.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة المورد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        return View(supplier);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Supplier model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Suppliers.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تحديث بيانات المورد";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        supplier.IsActive = !supplier.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = supplier.IsActive ? "تم تفعيل المورد" : "تم إيقاف المورد";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Pay(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        var balance = await _paymentService.GetSupplierBalanceAsync(id);
        ViewBag.Balance = balance;
        ViewBag.SupplierId = id;
        ViewBag.SupplierName = supplier.Name;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Pay(int id, decimal amount, DateTime date, PaymentMethod method, string? notes)
    {
        var (success, error) = await _paymentService.RecordSupplierPaymentAsync(id, amount, date, method, notes);
        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Pay), new { id });
        }
        TempData["Success"] = "تم تسجيل الدفعة بنجاح";
        return RedirectToAction(nameof(Details), new { id });
    }
}
