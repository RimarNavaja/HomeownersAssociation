using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date Posted")]
        public DateTime DatePosted { get; set; } = DateTime.Now;

        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

        [Required]
        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Is Public")]
        public bool IsPublic { get; set; } = false;

        [Display(Name = "Posted By")]
        public string? AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public ApplicationUser? Author { get; set; }

        [Display(Name = "Attachment")]
        public string? AttachmentUrl { get; set; }
    }

    public enum AnnouncementPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }
}