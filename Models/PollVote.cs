using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class PollVote
    {
        public int Id { get; set; }

        [Required]
        public int PollId { get; set; }
        [ForeignKey("PollId")]
        public virtual Poll Poll { get; set; } = null!;

        [Required]
        public int PollOptionId { get; set; }
        [ForeignKey("PollOptionId")]
        public virtual PollOption PollOption { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public DateTime VotedAt { get; set; }
    }
} 