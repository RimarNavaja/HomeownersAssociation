using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // e.g., Forms, Guidelines, Minutes, Financials

        [Required]
        public string FileUrl { get; set; } = string.Empty; // Store relative path to the file
        
        [NotMapped] // Store the actual filename separately for display/download header
        public string? FileName => Path.GetFileName(FileUrl);

        [Required]
        [Display(Name = "Uploaded By")]
        public string UploadedById { get; set; } = string.Empty;

        [ForeignKey("UploadedById")]
        public virtual ApplicationUser? UploadedBy { get; set; }

        [Required]
        [Display(Name = "Uploaded At")]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Is Publicly Accessible")]
        public bool IsPublic { get; set; } = false;
    }
} 