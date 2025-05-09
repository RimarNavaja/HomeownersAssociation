using System;
using System.ComponentModel.DataAnnotations;

namespace HomeownersAssociation.Models.ViewModels
{
    public class EventViewModel
    {
        public EventViewModel()
        {
            // Initialize with empty strings to prevent null validation errors
            CreatedById = string.Empty;
            CreatedByName = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date and time is required")]
        [Display(Name = "Start Date & Time")]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "End date and time is required")]
        [Display(Name = "End Date & Time")]
        public DateTime EndDateTime { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(50, ErrorMessage = "Location cannot exceed 50 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event type is required")]
        [Display(Name = "Event Type")]
        public string EventType { get; set; } = string.Empty;

        [Display(Name = "All Day Event")]
        public bool IsAllDay { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Color")]
        [StringLength(7, ErrorMessage = "Color must be a valid hex code")]
        [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Must be a valid hex color code (e.g. #FF5733)")]
        public string Color { get; set; } = "#007bff";

        public string CreatedById { get; set; }
        
        public string CreatedByName { get; set; }
    }
} 