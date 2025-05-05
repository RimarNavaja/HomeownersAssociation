using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Bill Number")]
        public string BillNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Issue Date")]
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Display(Name = "Payment Status")]
        public BillStatus Status { get; set; } = BillStatus.Unpaid;

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [ForeignKey("Homeowner")]
        public string? HomeownerId { get; set; }

        public virtual ApplicationUser? Homeowner { get; set; }

        [Display(Name = "Bill Type")]
        public BillType Type { get; set; } = BillType.MonthlyDues;

        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "Payment Reference")]
        public string? PaymentReference { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public enum BillStatus
    {
        [Display(Name = "Unpaid")]
        Unpaid,

        [Display(Name = "Paid")]
        Paid,

        [Display(Name = "Overdue")]
        Overdue,

        [Display(Name = "Partially Paid")]
        PartiallyPaid,

        [Display(Name = "Cancelled")]
        Cancelled
    }

    public enum BillType
    {
        [Display(Name = "Monthly Dues")]
        MonthlyDues,

        [Display(Name = "Special Assessment")]
        SpecialAssessment,

        [Display(Name = "Penalty Fee")]
        PenaltyFee,

        [Display(Name = "Maintenance Fee")]
        MaintenanceFee,

        [Display(Name = "Other")]
        Other
    }

    public enum PaymentMethod
    {
        [Display(Name = "Cash")]
        Cash,

        [Display(Name = "Bank Transfer")]
        BankTransfer,

        [Display(Name = "Credit Card")]
        CreditCard,

        [Display(Name = "GCash")]
        GCash,

        [Display(Name = "PayMaya")]
        PayMaya,

        [Display(Name = "Check")]
        Check,

        [Display(Name = "Other")]
        Other
    }
}