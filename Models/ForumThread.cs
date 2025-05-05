using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class ForumThread
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ForumCategory? Category { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Locked")]
        public bool IsLocked { get; set; } = false;

        // Navigation property for replies
        public virtual ICollection<ForumReply>? ForumReplies { get; set; }
    }
} 