using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMuhandisShop.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(200, ErrorMessage = "اسم المنتج لا يمكن أن يتجاوز 200 حرف")]
    [Display(Name = "اسم المنتج")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "الفئة مطلوبة")]
    [Display(Name = "الفئة")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "سعر الشراء مطلوب")]
    [Range(0, 9999999.99, ErrorMessage = "سعر الشراء يجب أن يكون أكبر من أو يساوي صفر")]
    [Display(Name = "سعر الشراء")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PurchasePrice { get; set; }

    [Required(ErrorMessage = "سعر البيع مطلوب")]
    [Range(0, 9999999.99, ErrorMessage = "سعر البيع يجب أن يكون أكبر من أو يساوي صفر")]
    [Display(Name = "سعر البيع")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SellingPrice { get; set; }

    [Display(Name = "الكمية المتاحة")]
    [Column(TypeName = "decimal(18,3)")]
    public decimal StockQuantity { get; set; }

    [Display(Name = "وحدة القياس")]
    [StringLength(50)]
    public string? Unit { get; set; }

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Description { get; set; }

    [Display(Name = "تاريخ الإضافة")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public ICollection<SaleReturnItem> SaleReturnItems { get; set; } = new List<SaleReturnItem>();
    public ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; } = new List<PurchaseReturnItem>();
}
