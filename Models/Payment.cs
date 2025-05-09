using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Payment Number")]
        public string PaymentNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Amount Paid")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [ForeignKey("Bill")]
        public int BillId { get; set; }

        public virtual Bill? Bill { get; set; }

        [ForeignKey("Homeowner")]
        public string HomeownerId { get; set; } = string.Empty;

        public virtual ApplicationUser? Homeowner { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Receipt Number")]
        public string? ReceiptNumber { get; set; }

        [Display(Name = "Processed By")]
        public string? ProcessedById { get; set; }

        [ForeignKey("ProcessedById")]
        public virtual ApplicationUser? ProcessedBy { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}