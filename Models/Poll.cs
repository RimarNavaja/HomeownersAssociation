using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class Poll
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        public string CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<PollOption> Options { get; set; } = new List<PollOption>();
        public virtual ICollection<PollVote> Votes { get; set; } = new List<PollVote>();

        [NotMapped]
        public bool IsOpen => IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;

        [NotMapped]
        public bool HasEnded => DateTime.Now > EndDate;
    }
} 