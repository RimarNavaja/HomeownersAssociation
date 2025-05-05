using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models.ViewModels
{
    public class FacilityViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
        public int? Capacity { get; set; }

        [Display(Name = "Rate Per Hour (Optional)")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.00, double.MaxValue, ErrorMessage = "Rate cannot be negative.")]
        public decimal? RatePerHour { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Maintenance Schedule (Optional)")]
        [DataType(DataType.MultilineText)]
        public string? MaintenanceSchedule { get; set; }
    }
} 