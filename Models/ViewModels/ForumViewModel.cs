using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeownersAssociation.Models.ViewModels
{
    // ViewModel for ForumCategory might be simple, similar to ServiceCategoryViewModel
    // Skipping if not strictly needed for initial implementation, can add later.

    public class ForumThreadViewModel
    {
        public int Id { get; set; } // For viewing/editing existing threads

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } // For display

        // User info will be set from controller or context
        public string? UserId { get; set; }
        public string? UserName { get; set; } // For display

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsLocked { get; set; } = false;

        // For dropdown in Create view
        public IEnumerable<SelectListItem>? AvailableCategories { get; set; }

        // For displaying replies in the Thread Details view
        public List<ForumReply>? Replies { get; set; }
        // For adding a new reply directly from the thread view
        public ForumReplyViewModel? NewReply { get; set; } 
    }

    public class ForumReplyViewModel
    {
         public int Id { get; set; } // For editing replies if needed later

        [Required]
        public int ThreadId { get; set; }

        // UserId will be set from controller
        public string? UserId { get; set; }
        public string? UserName { get; set; } // For display

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Your Reply")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 