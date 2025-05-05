using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class ForumReply
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ThreadId { get; set; }

        [ForeignKey("ThreadId")]
        public virtual ForumThread? Thread { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Replied At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 