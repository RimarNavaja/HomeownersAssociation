using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Facility
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        public int? Capacity { get; set; }

        [Display(Name = "Rate Per Hour")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? RatePerHour { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Maintenance Schedule")]
        [DataType(DataType.MultilineText)]
        public string? MaintenanceSchedule { get; set; }

        // Navigation property for reservations
        public virtual ICollection<FacilityReservation>? Reservations { get; set; }
    }
} 