using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HomeownersAssociation.Models.ViewModels
{
    public class FeedbackViewModel
    {
        public FeedbackViewModel()
        {
            // No need to initialize nullable string properties with empty strings
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description/Details")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [Display(Name = "Type")]
        public string Type { get; set; } // Feedback, Complaint, Suggestion, Appreciation

        [Display(Name = "Status")]
        public string? Status { get; set; } // New, InProgress, Resolved, Closed

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        [Range(1, 3, ErrorMessage = "Priority must be between 1 (Low) and 3 (High)")]
        public int Priority { get; set; } // 1-Low, 2-Medium, 3-High

        [Display(Name = "Response")]
        public string? Response { get; set; }

        [Display(Name = "Responded At")]
        public DateTime? RespondedAt { get; set; }

        public string? RespondedById { get; set; }
        
        [Display(Name = "Responded By")]
        public string? RespondedByName { get; set; }

        [Display(Name = "Make Public")]
        public bool IsPublic { get; set; }

        public string? SubmittedById { get; set; }
        
        [Display(Name = "Submitted By")]
        public string? SubmittedByName { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Current Attachment")]
        public string? AttachmentUrl { get; set; }
        
        [Display(Name = "Attachment (Optional)")]
        public IFormFile? Attachment { get; set; }
    }
} 