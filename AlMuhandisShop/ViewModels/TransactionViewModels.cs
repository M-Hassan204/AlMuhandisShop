using System.ComponentModel.DataAnnotations;
using AlMuhandisShop.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlMuhandisShop.ViewModels;

public class CreateSaleViewModel
{
    [Display(Name = "العميل")]
    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.Now;

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [Range(0, 9999999.99)]
    [Display(Name = "المبلغ المدفوع")]
    public decimal PaidAmount { get; set; }

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    public List<SaleItemInput> Items { get; set; } = new();

    // For dropdowns
    public SelectList? Customers { get; set; }
    public SelectList? Products { get; set; }
}

public class SaleItemInput
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(0.001, 999999)]
    public decimal Quantity { get; set; }

    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreatePurchaseViewModel
{
    [Required(ErrorMessage = "المورد مطلوب")]
    [Display(Name = "المورد")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.Now;

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [Range(0, 9999999.99)]
    [Display(Name = "المبلغ المدفوع")]
    public decimal PaidAmount { get; set; }

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    public List<PurchaseItemInput> Items { get; set; } = new();

    // For dropdowns
    public SelectList? Suppliers { get; set; }
    public SelectList? Products { get; set; }
}

public class PurchaseItemInput
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(0.001, 999999)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, 9999999)]
    public decimal UnitPrice { get; set; }

    public string? ProductName { get; set; }
}

public class CreateSaleReturnViewModel
{
    [Required]
    public int SaleId { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.Now;

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    public List<ReturnItemInput> Items { get; set; } = new();
}

public class CreatePurchaseReturnViewModel
{
    [Required]
    public int PurchaseId { get; set; }

    [Required(ErrorMessage = "التاريخ مطلوب")]
    [Display(Name = "التاريخ")]
    public DateTime Date { get; set; } = DateTime.Now;

    [StringLength(1000)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    public List<ReturnItemInput> Items { get; set; } = new();
}

public class ReturnItemInput
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(0.001, 999999)]
    public decimal Quantity { get; set; }

    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal MaxQuantity { get; set; }
}

public class CustomerAccountViewModel
{
    public AlMuhandisShop.Models.Customer Customer { get; set; } = null!;
    public decimal TotalSales { get; set; }
    public decimal TotalPaidOnSales { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal OutstandingBalance { get; set; }
    public List<Sale> Sales { get; set; } = new();
    public List<AlMuhandisShop.Models.CustomerPayment> Payments { get; set; } = new();
}

public class SupplierAccountViewModel
{
    public AlMuhandisShop.Models.Supplier Supplier { get; set; } = null!;
    public decimal TotalPurchases { get; set; }
    public decimal TotalPaidOnPurchases { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal OutstandingBalance { get; set; }
    public List<Purchase> Purchases { get; set; } = new();
    public List<AlMuhandisShop.Models.SupplierPayment> Payments { get; set; } = new();
}

public class ReportFilterViewModel
{
    [Display(Name = "من تاريخ")]
    public DateTime? FromDate { get; set; }

    [Display(Name = "إلى تاريخ")]
    public DateTime? ToDate { get; set; }
}

public class SalesReportViewModel : ReportFilterViewModel
{
    public List<Sale> Sales { get; set; } = new();
    public decimal TotalSales { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRemaining { get; set; }
}

public class PurchasesReportViewModel : ReportFilterViewModel
{
    public List<Purchase> Purchases { get; set; } = new();
    public decimal TotalPurchases { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRemaining { get; set; }
}

public class ExpensesReportViewModel : ReportFilterViewModel
{
    public List<AlMuhandisShop.Models.Expense> Expenses { get; set; } = new();
    public decimal TotalExpenses { get; set; }
}

public class CashReportViewModel : ReportFilterViewModel
{
    public List<CashTransaction> Transactions { get; set; } = new();
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal Balance { get; set; }
}
