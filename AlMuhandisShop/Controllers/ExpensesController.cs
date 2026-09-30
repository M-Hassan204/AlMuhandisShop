using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using AlMuhandisShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Controllers;

[Authorize]
public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _db;

    public ExpensesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.Expenses.AsQueryable();
        if (fromDate.HasValue) query = query.Where(e => e.Date >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(e => e.Date < toDate.Value.AddDays(1));

        var expenses = await query.OrderByDescending(e => e.Date).ToListAsync();
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Total = expenses.Sum(e => e.Amount);
        return View(expenses);
    }

    public IActionResult Create()
    {
        return View(new Expense { Date = DateTime.Now });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Expense model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.Amount <= 0)
        {
            ModelState.AddModelError("Amount", "المبلغ يجب أن يكون أكبر من صفر");
            return View(model);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Expenses.Add(model);
            await _db.SaveChangesAsync();

            _db.CashTransactions.Add(new CashTransaction
            {
                Date = model.Date,
                Type = CashTransactionType.Expense,
                Direction = CashFlowDirection.Out,
                Amount = model.Amount,
                ReferenceId = model.Id,
                Notes = model.Name
            });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = "تم تسجيل المصروف بنجاح";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await tx.RollbackAsync();
            TempData["Error"] = "حدث خطأ أثناء حفظ المصروف";
            return View(model);
        }
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense == null) return NotFound();
        return View(expense);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Expense model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Expenses.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تحديث المصروف";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense == null) return NotFound();
        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم حذف المصروف";
        return RedirectToAction(nameof(Index));
    }
}
