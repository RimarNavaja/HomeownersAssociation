using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ServiceCategory? Category { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Priority { get; set; } = ServiceRequestPriority.Medium; // Default priority

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = ServiceRequestStatus.New; // Default status

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Completed At")]
        public DateTime? CompletedAt { get; set; }
    }

    // Static class for status constants
    public static class ServiceRequestStatus
    {
        public const string New = "New";
        public const string InProgress = "In Progress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
    
    // Static class for priority constants (optional, could use int directly)
    public static class ServiceRequestPriority
    {
        public const int Low = 1;
        public const int Medium = 2;
        public const int High = 3;
    }
} 