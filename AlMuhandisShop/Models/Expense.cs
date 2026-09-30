using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMuhandisShop.Models;

public class Expense
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم المصروف مطلوب")]
    [StringLength(200, ErrorMessage = "اسم المصروف لا يمكن أن يتجاوز 200 حرف")]
    [Display(Name = "البيان")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "المبلغ مطلوب")]
    [Range(0.01, 9999999.99, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "المبلغ")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
}
