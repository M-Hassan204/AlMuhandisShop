using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, bool? lowStock)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (lowStock == true)
            query = query.Where(p => p.StockQuantity <= 10);

        var products = await query.OrderBy(p => p.Name).ToListAsync();

        ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.LowStock = lowStock;

        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.SaleItems).ThenInclude(si => si.Sale)
            .Include(p => p.PurchaseItems).ThenInclude(pi => pi.Purchase)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Product model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        if (model.PurchasePrice < 0 || model.SellingPrice < 0)
        {
            ModelState.AddModelError("", "الأسعار لا يمكن أن تكون سالبة");
            ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        model.CreatedAt = DateTime.UtcNow;
        _db.Products.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Product model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        if (model.PurchasePrice < 0 || model.SellingPrice < 0)
        {
            ModelState.AddModelError("", "الأسعار لا يمكن أن تكون سالبة");
            ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        _db.Products.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تحديث المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        product.IsActive = !product.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = product.IsActive ? "تم تفعيل المنتج" : "تم إيقاف المنتج";
        return RedirectToAction(nameof(Index));
    }

    // AJAX endpoint for getting product price
    [HttpGet]
    public async Task<IActionResult> GetPrice(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        return Json(new { price = product.SellingPrice, purchasePrice = product.PurchasePrice, stock = product.StockQuantity, unit = product.Unit });
    }
}
