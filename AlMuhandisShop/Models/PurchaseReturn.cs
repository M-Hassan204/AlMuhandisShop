using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMuhandisShop.Models;

public class PurchaseReturn
{
    public int Id { get; set; }

    [Required(ErrorMessage = "رقم فاتورة الشراء مطلوب")]
    [Display(Name = "فاتورة الشراء")]
    public int PurchaseId { get; set; }

    [Display(Name = "المورد")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "إجمالي المرتجع")]
    public decimal TotalAmount { get; set; }

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    // Navigation
    public Purchase Purchase { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}

public class PurchaseReturnItem
{
    public int Id { get; set; }

    [Required]
    public int PurchaseReturnId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "الكمية مطلوبة")]
    [Range(0.001, 999999, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    [Column(TypeName = "decimal(18,3)")]
    [Display(Name = "الكمية")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "سعر الوحدة")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "الإجمالي")]
    public decimal Total { get; set; }

    // Navigation
    public PurchaseReturn PurchaseReturn { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
