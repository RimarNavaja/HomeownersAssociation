using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList

namespace HomeownersAssociation.Models.ViewModels
{
    public class ServiceRequestViewModel
    {
        public int Id { get; set; } // For editing/details

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } // For display

        // UserId will be set from controller
        public string? UserId { get; set; }
        public string? UserName { get; set; } // For display

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(ServiceRequestPriority.Low, ServiceRequestPriority.High, ErrorMessage = "Invalid Priority Level")]
        public int Priority { get; set; } = ServiceRequestPriority.Medium;

        // Status will be set by the system/admin
        public string Status { get; set; } = ServiceRequestStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        // For populating dropdowns in the Create/Edit view
        public IEnumerable<SelectListItem>? AvailableCategories { get; set; }
        public IEnumerable<SelectListItem>? AvailablePriorities { get; set; }
    }
} 