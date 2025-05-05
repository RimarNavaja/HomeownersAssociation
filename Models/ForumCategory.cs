using System.ComponentModel.DataAnnotations;

namespace HomeownersAssociation.Models
{
    public class ForumCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation property for threads
        public virtual ICollection<ForumThread>? ForumThreads { get; set; }
    }
} 