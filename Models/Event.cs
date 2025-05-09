using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Date & Time")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "End Date & Time")]
        public DateTime EndDateTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Location { get; set; } = string.Empty;

        [StringLength(20)]
        public string EventType { get; set; } = string.Empty; // Community, Maintenance, Meeting, etc.

        public bool IsActive { get; set; } = true;

        public bool IsAllDay { get; set; }

        [StringLength(7)]
        public string Color { get; set; } = "#007bff"; // Hex color code for calendar display

        [ForeignKey("CreatedBy")]
        public string CreatedById { get; set; } = string.Empty;
        
        public virtual ApplicationUser CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 