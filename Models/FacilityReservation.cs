using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class FacilityReservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FacilityId { get; set; }

        [ForeignKey("FacilityId")]
        public virtual Facility? Facility { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [StringLength(200)]
        public string? Purpose { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = ReservationStatus.Pending; // Default status

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // Static class for status constants to avoid magic strings
    public static class ReservationStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";
    }
} 