using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMuhandisShop.Models;

public class CustomerPayment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "العميل مطلوب")]
    [Display(Name = "العميل")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "المبلغ مطلوب")]
    [Range(0.01, 9999999.99, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "المبلغ")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
}

public class SupplierPayment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "المورد مطلوب")]
    [Display(Name = "المورد")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "المبلغ مطلوب")]
    [Range(0.01, 9999999.99, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "المبلغ")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    // Navigation
    public Supplier Supplier { get; set; } = null!;
}
