using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class EmergencyContact
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Organization")]
        [StringLength(100)]
        public string Organization { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contact Type")]
        [StringLength(50)]
        public string ContactType { get; set; } = string.Empty; // Fire, Police, Medical, HOA, Maintenance, etc.

        [Required]
        [Display(Name = "Phone Number")]
        [StringLength(20)]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Alternative Phone")]
        [StringLength(20)]
        [Phone]
        public string? AlternativePhone { get; set; }

        [Display(Name = "Email")]
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Address")]
        [StringLength(200)]
        public string? Address { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Available 24/7")]
        public bool IsAvailable24x7 { get; set; } = false;

        [Display(Name = "Operating Hours")]
        [StringLength(100)]
        public string? OperatingHours { get; set; }

        [Display(Name = "Priority Order")]
        public int PriorityOrder { get; set; } = 999;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created By")]
        public string? CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
    }
} 