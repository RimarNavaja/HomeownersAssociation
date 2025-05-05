using System.ComponentModel.DataAnnotations;

namespace HomeownersAssociation.Models.ViewModels
{
    public class FacilityReservationViewModel
    {
        public int Id { get; set; } // Needed for editing existing reservations

        [Required]
        [Display(Name = "Facility")]
        public int FacilityId { get; set; }
        
        // Added for display purposes
        public string? FacilityName { get; set; }

        // UserId will be set from the logged-in user in the controller
        public string? UserId { get; set; }
        public string? UserName { get; set; } // For display on admin view

        [Required]
        [Display(Name = "Reservation Date")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [StringLength(200)]
        public string? Purpose { get; set; }

        // Status might be set by Admin/Staff, not directly by user during creation
        public string Status { get; set; } = ReservationStatus.Pending;

        // Used to pass available facilities to the view
        public IEnumerable<Facility>? AvailableFacilities { get; set; }

        // Used to pass existing reservations for conflict checking/display
        public IEnumerable<FacilityReservation>? ExistingReservations { get; set; }
    }
} 