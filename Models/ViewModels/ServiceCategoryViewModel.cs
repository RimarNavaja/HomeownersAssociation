using System.ComponentModel.DataAnnotations;

namespace HomeownersAssociation.Models.ViewModels
{
    public class ServiceCategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
} 