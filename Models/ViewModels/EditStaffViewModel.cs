using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HomeownersAssociation.Models.ViewModels
{
    public class EditStaffViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password (leave blank to keep current)")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmNewPassword { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        [Display(Name = "Current Profile Picture")]
        public string? ProfilePictureUrl { get; set; }
    }
}