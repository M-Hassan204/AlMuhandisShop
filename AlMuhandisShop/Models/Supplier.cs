using System.ComponentModel.DataAnnotations;

namespace AlMuhandisShop.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [StringLength(200, ErrorMessage = "اسم المورد لا يمكن أن يتجاوز 200 حرف")]
    [Display(Name = "اسم المورد")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "اسم الشركة")]
    public string? CompanyName { get; set; }

    [StringLength(50)]
    [Display(Name = "رقم الهاتف")]
    public string? Phone { get; set; }

    [StringLength(500)]
    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "تاريخ الإضافة")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    public ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>();
}
