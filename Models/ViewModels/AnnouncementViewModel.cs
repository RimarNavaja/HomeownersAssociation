using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HomeownersAssociation.Models.ViewModels
{
    public class AnnouncementViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Attachment")]
        public IFormFile? Attachment { get; set; }

        // For editing, to keep track of existing attachment
        public string? ExistingAttachmentUrl { get; set; }
    }
}