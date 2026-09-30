using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMuhandisShop.Models;

public enum PaymentMethod
{
    [Display(Name = "نقدي")]
    Cash = 1,
    [Display(Name = "تحويل بنكي")]
    BankTransfer = 2,
    [Display(Name = "شيك")]
    Check = 3,
    [Display(Name = "آجل")]
    Credit = 4
}

public class Sale
{
    public int Id { get; set; }

    [Display(Name = "العميل")]
    public int? CustomerId { get; set; }

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
    public Customer? Customer { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<SaleReturn> SaleReturns { get; set; } = new List<SaleReturn>();
}

public class SaleItem
{
    public int Id { get; set; }

    [Required]
    public int SaleId { get; set; }

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
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
