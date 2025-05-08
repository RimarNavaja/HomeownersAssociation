using System.Collections.Generic;

namespace HomeownersAssociation.Models.ViewModels
{
    public class PollResultsViewModel
    {
        public int PollId { get; set; }
        public string PollTitle { get; set; }
        public string? PollDescription { get; set; }
        public int TotalVotes { get; set; }
        public List<PollOptionResultViewModel> OptionsWithVotes { get; set; }
        public bool UserHasVoted { get; set; } // For public view, to disable voting if already voted
        public int? UserVoteOptionId { get; set; } // For public view, to highlight user's vote
        public bool IsPollOpen { get; set; }
        public bool HasPollEnded { get; set; }
        public DateTime EndDate { get; set; }

        public PollResultsViewModel()
        {
            OptionsWithVotes = new List<PollOptionResultViewModel>();
        }
    }

    public class PollOptionResultViewModel
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; }
        public int VoteCount { get; set; }
        public double VotePercentage { get; set; }
    }
} 