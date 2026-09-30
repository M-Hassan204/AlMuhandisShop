using System.ComponentModel.DataAnnotations;

namespace AlMuhandisShop.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم العميل مطلوب")]
    [StringLength(200, ErrorMessage = "اسم العميل لا يمكن أن يتجاوز 200 حرف")]
    [Display(Name = "اسم العميل")]
    public string Name { get; set; } = string.Empty;

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
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<CustomerPayment> Payments { get; set; } = new List<CustomerPayment>();
}
