using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HomeownersAssociation.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Lot Number")]
        public string LotNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Block Number")]
        public string BlockNumber { get; set; } = string.Empty;

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        [Display(Name = "Current Profile Picture")]
        public string? ProfilePictureUrl { get; set; }
    }
}