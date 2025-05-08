using System.ComponentModel.DataAnnotations;

namespace HomeownersAssociation.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Category / Department")]
        public string Category { get; set; } // e.g., "Security", "Administration", "Maintenance", "Emergency"

        [StringLength(50)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string? EmailAddress { get; set; }

        [StringLength(100)]
        [Display(Name = "Office Hours / Availability")]
        public string? OfficeHours { get; set; }

        [StringLength(100)]
        public string? Location { get; set; } // e.g., "Admin Building, Room 101"

        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Visible to Public?")]
        public bool IsPublic { get; set; } = true;

        [Display(Name = "Display Order")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Display Order must be a non-negative number.")]
        public int DisplayOrder { get; set; } = 0; // For sorting
    }
} 