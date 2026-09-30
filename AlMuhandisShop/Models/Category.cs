using System.ComponentModel.DataAnnotations;

namespace AlMuhandisShop.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم الفئة مطلوب")]
    [StringLength(100, ErrorMessage = "اسم الفئة لا يمكن أن يتجاوز 100 حرف")]
    [Display(Name = "اسم الفئة")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
