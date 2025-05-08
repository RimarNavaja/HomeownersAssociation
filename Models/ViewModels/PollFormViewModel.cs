using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace HomeownersAssociation.Models.ViewModels
{
    public class PollFormViewModel
    {
        public int Id { get; set; } // Poll ID, used for editing

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1); // Default to tomorrow

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(8); // Default to one week from tomorrow

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;

        // For dynamically adding/editing poll options
        // Each string in the list represents the text for one PollOption
        public List<string> Options { get; set; } = new List<string> { "", "" }; // Start with 2 empty options

        // If editing, this will hold existing option IDs and their text
        public List<PollOptionViewModel> ExistingOptions { get; set; } = new List<PollOptionViewModel>();
    }

    public class PollOptionViewModel 
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
    }
} 