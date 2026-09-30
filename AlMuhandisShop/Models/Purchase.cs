using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMuhandisShop.Models;

public class Purchase
{
    public int Id { get; set; }

    [Required(ErrorMessage = "المورد مطلوب")]
    [Display(Name = "المورد")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "إجمالي الفاتورة")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "المبلغ المدفوع")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "المبلغ المتبقي")]
    public decimal RemainingAmount { get; set; }

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new List<PurchaseReturn>();
}

public class PurchaseItem
{
    public int Id { get; set; }

    [Required]
    public int PurchaseId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "الكمية مطلوبة")]
    [Range(0.001, 999999, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    [Display(Name = "الكمية")]
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "سعر الوحدة")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "الإجمالي")]
    public decimal Total { get; set; }

    // Navigation
    public Purchase Purchase { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
