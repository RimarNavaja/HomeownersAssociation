using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; } // Feedback, Complaint, Suggestion, Appreciation

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // New, InProgress, Resolved, Closed

        [Required]
        public int Priority { get; set; } // 1-Low, 2-Medium, 3-High

        public string? Response { get; set; }

        public DateTime? RespondedAt { get; set; }

        [ForeignKey("RespondedBy")]
        public string RespondedById { get; set; }
        
        public virtual ApplicationUser RespondedBy { get; set; }

        public bool IsPublic { get; set; } = false;

        [ForeignKey("SubmittedBy")]
        public string SubmittedById { get; set; }
        
        [Required]
        public virtual ApplicationUser SubmittedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public string? AttachmentUrl { get; set; }
    }
} 