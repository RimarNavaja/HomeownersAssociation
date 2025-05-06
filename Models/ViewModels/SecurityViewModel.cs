using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeownersAssociation.Models.ViewModels
{
    public class VisitorPassViewModel
    {
        public int Id { get; set; }

        [Required]
        public string RequestedById { get; set; } = string.Empty;

        public string? RequestedByName { get; set; }

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
        public DateTime VisitDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Expected Time In")]
        [DataType(DataType.Time)]
        public DateTime ExpectedTimeIn { get; set; } = DateTime.Today.AddHours(9);

        [Required]
        [Display(Name = "Expected Time Out")]
        [DataType(DataType.Time)]
        public DateTime ExpectedTimeOut { get; set; } = DateTime.Today.AddHours(17);

        [Display(Name = "Vehicle Details")]
        [StringLength(200)]
        public string? VehicleDetails { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Actual Time In")]
        [DataType(DataType.DateTime)]
        public DateTime? ActualTimeIn { get; set; }

        [Display(Name = "Actual Time Out")]
        [DataType(DataType.DateTime)]
        public DateTime? ActualTimeOut { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public class VehicleViewModel
    {
        public int Id { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        public string? OwnerName { get; set; }

        [Required]
        [Display(Name = "License Plate")]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Vehicle Type")]
        [StringLength(50)]
        public string VehicleType { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? VehicleTypes { get; set; }

        [Required]
        [Display(Name = "Make")]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Model")]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "Year")]
        [Range(1900, 2100)]
        public int? Year { get; set; }

        [Display(Name = "Color")]
        [StringLength(30)]
        public string? Color { get; set; }

        [Display(Name = "RFID Tag")]
        [StringLength(50)]
        public string? RfidTag { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public class EmergencyContactViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Organization")]
        [StringLength(100)]
        public string Organization { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contact Type")]
        [StringLength(50)]
        public string ContactType { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? ContactTypes { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [StringLength(20)]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Alternative Phone")]
        [StringLength(20)]
        [Phone]
        public string? AlternativePhone { get; set; }

        [Display(Name = "Email")]
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Address")]
        [StringLength(200)]
        public string? Address { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Available 24/7")]
        public bool IsAvailable24x7 { get; set; } = false;

        [Display(Name = "Operating Hours")]
        [StringLength(100)]
        public string? OperatingHours { get; set; }

        [Display(Name = "Priority Order")]
        [Range(1, 999)]
        public int PriorityOrder { get; set; } = 999;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    // Dashboard ViewModel for Security Module
    public class SecurityDashboardViewModel
    {
        public List<VisitorPass>? PendingVisitorPasses { get; set; }
        public List<VisitorPass>? TodayVisitorPasses { get; set; }
        public List<Vehicle>? RecentVehicles { get; set; }
        public int TotalVisitorPasses { get; set; }
        public int TotalApprovedPasses { get; set; }
        public int TotalRejectedPasses { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalEmergencyContacts { get; set; }
    }
} 