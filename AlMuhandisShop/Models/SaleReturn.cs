using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMuhandisShop.Models;

public class SaleReturn
{
    public int Id { get; set; }

    [Required(ErrorMessage = "رقم فاتورة البيع مطلوب")]
    [Display(Name = "فاتورة البيع")]
    public int SaleId { get; set; }

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
    public Sale Sale { get; set; } = null!;
    public ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();
}

public class SaleReturnItem
{
    public int Id { get; set; }

    [Required]
    public int SaleReturnId { get; set; }

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
    public SaleReturn SaleReturn { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
