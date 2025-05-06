using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class VisitorPass
    {
        public int Id { get; set; }

        [Required]
        public string RequestedById { get; set; } = string.Empty;

        [ForeignKey("RequestedById")]
        public virtual ApplicationUser? RequestedBy { get; set; }

        [Required]
        [Display(Name = "Visitor Name")]
        [StringLength(100)]
        public string VisitorName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Purpose of Visit")]
        [StringLength(200)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Visit Date")]
        [DataType(DataType.Date)]
        public DateTime VisitDate { get; set; }

        [Required]
        [Display(Name = "Expected Time In")]
        [DataType(DataType.Time)]
        public DateTime ExpectedTimeIn { get; set; }

        [Required]
        [Display(Name = "Expected Time Out")]
        [DataType(DataType.Time)]
        public DateTime ExpectedTimeOut { get; set; }

        [Display(Name = "Vehicle Details")]
        [StringLength(200)]
        public string? VehicleDetails { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [Display(Name = "Actual Time In")]
        [DataType(DataType.DateTime)]
        public DateTime? ActualTimeIn { get; set; }

        [Display(Name = "Actual Time Out")]
        [DataType(DataType.DateTime)]
        public DateTime? ActualTimeOut { get; set; }
        
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 