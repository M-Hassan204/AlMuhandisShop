using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(categories);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Category model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _db.Categories.AnyAsync(c => c.Name == model.Name))
        {
            ModelState.AddModelError("Name", "الفئة موجودة بالفعل");
            return View(model);
        }

        _db.Categories.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة الفئة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Category model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        if (await _db.Categories.AnyAsync(c => c.Name == model.Name && c.Id != id))
        {
            ModelState.AddModelError("Name", "الفئة موجودة بالفعل");
            return View(model);
        }

        _db.Categories.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تحديث الفئة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();

        if (category.Products.Any())
        {
            TempData["Error"] = "لا يمكن حذف الفئة لوجود منتجات مرتبطة بها";
            return RedirectToAction(nameof(Index));
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم حذف الفئة";
        return RedirectToAction(nameof(Index));
    }
}
