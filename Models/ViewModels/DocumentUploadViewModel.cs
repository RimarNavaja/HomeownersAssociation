using System.ComponentModel.DataAnnotations;

namespace HomeownersAssociation.Models.ViewModels
{
    public class DocumentUploadViewModel
    {
        public int Id { get; set; } // Used for potential edit later

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a file to upload.")]
        [Display(Name = "Document File")]
        [DataType(DataType.Upload)]
        public IFormFile DocumentFile { get; set; } = null!; // The actual uploaded file

        [Display(Name = "Is Publicly Accessible?")]
        public bool IsPublic { get; set; } = false;

        // Maybe add a list of predefined categories for a dropdown?
        // public IEnumerable<SelectListItem>? AvailableCategories { get; set; }
    }
} 