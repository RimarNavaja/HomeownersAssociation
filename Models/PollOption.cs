using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeownersAssociation.Models
{
    public class PollOption
    {
        public int Id { get; set; }

        [Required]
        public int PollId { get; set; }
        [ForeignKey("PollId")]
        public virtual Poll Poll { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Option Text")]
        public string OptionText { get; set; }
    }
} 