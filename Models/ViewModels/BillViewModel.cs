using System.ComponentModel.DataAnnotations;

namespace HomeownersAssociation.Models.ViewModels
{
    public class BillViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Bill Number")]
        public string BillNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(30);

        [Display(Name = "Issue Date")]
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Display(Name = "Payment Status")]
        public BillStatus Status { get; set; } = BillStatus.Unpaid;

        [Required]
        [Display(Name = "Homeowner")]
        public string HomeownerId { get; set; } = string.Empty;

        [Display(Name = "Bill Type")]
        public BillType Type { get; set; } = BillType.MonthlyDues;

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // For dropdown lists in view
        public IEnumerable<ApplicationUser>? Homeowners { get; set; }
    }

    public class BillPaymentViewModel
    {
        public int BillId { get; set; }

        [Display(Name = "Bill Number")]
        public string BillNumber { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Amount Due")]
        public decimal AmountDue { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount to Pay")]
        public decimal AmountToPay { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}