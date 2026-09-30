using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using AlMuhandisShop.Services;
using AlMuhandisShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Controllers;

[Authorize]
public class PurchasesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly PurchaseService _purchaseService;
    private readonly ReturnService _returnService;

    public PurchasesController(ApplicationDbContext db, PurchaseService purchaseService, ReturnService returnService)
    {
        _db = db;
        _purchaseService = purchaseService;
        _returnService = returnService;
    }

    public async Task<IActionResult> Index(string? search, DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.Purchases
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Supplier.Name.Contains(search) || (p.Supplier.CompanyName != null && p.Supplier.CompanyName.Contains(search)));

        if (fromDate.HasValue)
            query = query.Where(p => p.Date >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.Date < toDate.Value.AddDays(1));

        var purchases = await query.OrderByDescending(p => p.Date).ToListAsync();
        ViewBag.Search = search;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(purchases);
    }

    public async Task<IActionResult> Details(int id)
    {
        var purchase = await _db.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseItems).ThenInclude(pi => pi.Product)
            .Include(p => p.PurchaseReturns).ThenInclude(pr => pr.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null) return NotFound();
        return View(purchase);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new CreatePurchaseViewModel
        {
            Date = DateTime.Now,
            Suppliers = new SelectList(
                await _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(),
                "Id", "Name"),
            Products = new SelectList(
                await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(),
                "Id", "Name")
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseViewModel vm)
    {
        ModelState.Remove("Suppliers");
        ModelState.Remove("Products");

        if (!ModelState.IsValid)
        {
            vm.Suppliers = new SelectList(await _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(), "Id", "Name", vm.SupplierId);
            vm.Products = new SelectList(await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(vm);
        }

        var (success, error, purchaseId) = await _purchaseService.CreatePurchaseAsync(vm);
        if (!success)
        {
            TempData["Error"] = error;
            vm.Suppliers = new SelectList(await _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(), "Id", "Name", vm.SupplierId);
            vm.Products = new SelectList(await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(vm);
        }

        TempData["Success"] = "تم حفظ فاتورة الشراء بنجاح";
        return RedirectToAction(nameof(Details), new { id = purchaseId });
    }

    public async Task<IActionResult> Return(int purchaseId)
    {
        var purchase = await _db.Purchases
            .Include(p => p.PurchaseItems).ThenInclude(pi => pi.Product)
            .Include(p => p.PurchaseReturns).ThenInclude(pr => pr.Items)
            .FirstOrDefaultAsync(p => p.Id == purchaseId);

        if (purchase == null) return NotFound();

        var vm = new CreatePurchaseReturnViewModel
        {
            PurchaseId = purchaseId,
            Date = DateTime.Now,
            Items = purchase.PurchaseItems.Select(pi => new ReturnItemInput
            {
                ProductId = pi.ProductId,
                ProductName = pi.Product.Name,
                UnitPrice = pi.UnitPrice,
                MaxQuantity = pi.Quantity - purchase.PurchaseReturns.SelectMany(pr => pr.Items).Where(i => i.ProductId == pi.ProductId).Sum(i => i.Quantity)
            }).Where(i => i.MaxQuantity > 0).ToList()
        };

        ViewBag.Purchase = purchase;
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Return(CreatePurchaseReturnViewModel vm)
    {
        ModelState.Remove("Items");
        var (success, error, returnId) = await _returnService.CreatePurchaseReturnAsync(vm);
        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Return), new { purchaseId = vm.PurchaseId });
        }
        TempData["Success"] = "تم تسجيل مرتجع المشتريات بنجاح";
        return RedirectToAction(nameof(Details), new { id = vm.PurchaseId });
    }
}
