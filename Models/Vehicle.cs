using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser? Owner { get; set; }

        [Required]
        [Display(Name = "License Plate")]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Vehicle Type")]
        [StringLength(50)]
        public string VehicleType { get; set; } = string.Empty; // Car, Motorcycle, Truck, etc.

        [Required]
        [Display(Name = "Make")]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Model")]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "Year")]
        public int? Year { get; set; }

        [Display(Name = "Color")]
        [StringLength(30)]
        public string? Color { get; set; }

        [Display(Name = "RFID Tag")]
        [StringLength(50)]
        public string? RfidTag { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
} 