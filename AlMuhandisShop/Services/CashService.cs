using AlMuhandisShop.Data;
using AlMuhandisShop.Models;
using Microsoft.EntityFrameworkCore;

namespace AlMuhandisShop.Services;

public class CashService
{
    private readonly ApplicationDbContext _db;

    public CashService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> GetCurrentBalanceAsync()
    {
        var inflow = await _db.CashTransactions
            .Where(t => t.Direction == CashFlowDirection.In)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var outflow = await _db.CashTransactions
            .Where(t => t.Direction == CashFlowDirection.Out)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        return inflow - outflow;
    }

    public async Task RecordAsync(CashTransaction transaction)
    {
        _db.CashTransactions.Add(transaction);
        await _db.SaveChangesAsync();
    }
}
