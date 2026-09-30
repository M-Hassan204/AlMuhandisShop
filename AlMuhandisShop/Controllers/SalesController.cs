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
public class SalesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly SaleService _saleService;
    private readonly ReturnService _returnService;

    public SalesController(ApplicationDbContext db, SaleService saleService, ReturnService returnService)
    {
        _db = db;
        _saleService = saleService;
        _returnService = returnService;
    }

    public async Task<IActionResult> Index(string? search, DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.Sales
            .Include(s => s.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Customer != null && s.Customer.Name.Contains(search));

        if (fromDate.HasValue)
            query = query.Where(s => s.Date >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.Date < toDate.Value.AddDays(1));

        var sales = await query.OrderByDescending(s => s.Date).ToListAsync();
        ViewBag.Search = search;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(sales);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await _saleService.GetSaleDetailAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new CreateSaleViewModel
        {
            Date = DateTime.Now,
            Customers = new SelectList(
                await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(),
                "Id", "Name"),
            Products = new SelectList(
                await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(),
                "Id", "Name")
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSaleViewModel vm)
    {
        // Remove SelectList validation issues
        ModelState.Remove("Customers");
        ModelState.Remove("Products");

        if (!ModelState.IsValid)
        {
            vm.Customers = new SelectList(await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name", vm.CustomerId);
            vm.Products = new SelectList(await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(vm);
        }

        var (success, error, saleId) = await _saleService.CreateSaleAsync(vm);
        if (!success)
        {
            TempData["Error"] = error;
            vm.Customers = new SelectList(await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name", vm.CustomerId);
            vm.Products = new SelectList(await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(vm);
        }

        TempData["Success"] = "تم حفظ فاتورة البيع بنجاح";
        return RedirectToAction(nameof(Details), new { id = saleId });
    }

    // Sale Return
    public async Task<IActionResult> Return(int saleId)
    {
        var sale = await _db.Sales
            .Include(s => s.SaleItems).ThenInclude(si => si.Product)
            .Include(s => s.SaleReturns).ThenInclude(sr => sr.Items)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null) return NotFound();

        var vm = new CreateSaleReturnViewModel
        {
            SaleId = saleId,
            Date = DateTime.Now,
            Items = sale.SaleItems.Select(si => new ReturnItemInput
            {
                ProductId = si.ProductId,
                ProductName = si.Product.Name,
                UnitPrice = si.UnitPrice,
                MaxQuantity = si.Quantity - sale.SaleReturns.SelectMany(sr => sr.Items).Where(i => i.ProductId == si.ProductId).Sum(i => i.Quantity)
            }).Where(i => i.MaxQuantity > 0).ToList()
        };

        ViewBag.Sale = sale;
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Return(CreateSaleReturnViewModel vm)
    {
        ModelState.Remove("Items");
        var (success, error, returnId) = await _returnService.CreateSaleReturnAsync(vm);
        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Return), new { saleId = vm.SaleId });
        }
        TempData["Success"] = "تم تسجيل مرتجع المبيعات بنجاح";
        return RedirectToAction(nameof(Details), new { id = vm.SaleId });
    }
}
