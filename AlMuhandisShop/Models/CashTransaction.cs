using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMuhandisShop.Models;

public enum CashTransactionType
{
    [Display(Name = "بيع")]
    Sale = 1,
    [Display(Name = "دفعة من عميل")]
    CustomerPayment = 2,
    [Display(Name = "دفعة لمورد")]
    SupplierPayment = 3,
    [Display(Name = "مصروف")]
    Expense = 4,
    [Display(Name = "شراء")]
    Purchase = 5,
    [Display(Name = "إيداع")]
    AdjustmentIn = 6,
    [Display(Name = "سحب")]
    AdjustmentOut = 7,
    [Display(Name = "مرتجع مبيعات")]
    SaleReturn = 8,
    [Display(Name = "مرتجع مشتريات")]
    PurchaseReturn = 9
}

public enum CashFlowDirection
{
    In = 1,
    Out = 2
}

public class CashTransaction
{
    public int Id { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Display(Name = "النوع")]
    public CashTransactionType Type { get; set; }

    [Display(Name = "الاتجاه")]
    public CashFlowDirection Direction { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "المبلغ")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Reference ID - points to the source record. NOT a DB FK because the source table varies.
    /// Validated in service logic.
    /// </summary>
    [Display(Name = "رقم المرجع")]
    public int? ReferenceId { get; set; }

    [StringLength(1000)]
    [Display(Name = "البيان")]
    public string? Notes { get; set; }
}
