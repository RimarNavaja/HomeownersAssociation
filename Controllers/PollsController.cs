using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeownersAssociation.Controllers
{
    public class PollsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PollsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Polls/Manage (Admin View)
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Manage()
        {
            ViewData["ActiveLink"] = "ManagePolls";
            var polls = await _context.Polls
                .Include(p => p.CreatedBy)
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
            return View(polls);
        }

        // GET: Polls/Create
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Create()
        {
            ViewData["ActiveLink"] = "ManagePolls";
            var viewModel = new PollFormViewModel();
            return View(viewModel);
        }

        // POST: Polls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create(PollFormViewModel viewModel)
        {
            ViewData["ActiveLink"] = "ManagePolls";
            if (viewModel.Options == null || viewModel.Options.Count(o => !string.IsNullOrWhiteSpace(o)) < 2)
            {
                ModelState.AddModelError("Options", "A poll must have at least two non-empty options.");
            }

            if (viewModel.EndDate <= viewModel.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date must be after Start Date.");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var poll = new Poll
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    CreatedById = currentUser.Id,
                    IsActive = viewModel.IsActive,
                    Options = viewModel.Options
                                    .Where(o => !string.IsNullOrWhiteSpace(o))
                                    .Select(optionText => new PollOption { OptionText = optionText })
                                    .ToList()
                };

                _context.Polls.Add(poll);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Poll created successfully.";
                return RedirectToAction(nameof(Manage));
            }
            return View(viewModel);
        }

        // GET: Polls/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["ActiveLink"] = "ManagePolls";
            if (id == null)
            {
                return NotFound();
            }

            var poll = await _context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == id);
            if (poll == null)
            {
                return NotFound();
            }

            var viewModel = new PollFormViewModel
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                StartDate = poll.StartDate,
                EndDate = poll.EndDate,
                IsActive = poll.IsActive,
                ExistingOptions = poll.Options.Select(o => new PollOptionViewModel { Id = o.Id, OptionText = o.OptionText }).ToList(),
                Options = new List<string>() // For adding new options during edit
            };
            return View(viewModel);
        }

        // POST: Polls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, PollFormViewModel viewModel)
        {
            ViewData["ActiveLink"] = "ManagePolls";
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var existingOptionsCount = viewModel.ExistingOptions?.Count(eo => !string.IsNullOrWhiteSpace(eo.OptionText)) ?? 0;
            var newOptionsCount = viewModel.Options?.Count(no => !string.IsNullOrWhiteSpace(no)) ?? 0;

            if (existingOptionsCount + newOptionsCount < 2)
            {
                ModelState.AddModelError("Options", "A poll must have at least two non-empty options.");
            }
            
            if (viewModel.EndDate <= viewModel.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date must be after Start Date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var pollToUpdate = await _context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == id);
                    if (pollToUpdate == null) { return NotFound(); }

                    pollToUpdate.Title = viewModel.Title;
                    pollToUpdate.Description = viewModel.Description;
                    pollToUpdate.StartDate = viewModel.StartDate;
                    pollToUpdate.EndDate = viewModel.EndDate;
                    pollToUpdate.IsActive = viewModel.IsActive;

                    // Update existing options and remove those not present or empty
                    var optionsToRemove = new List<PollOption>();
                    foreach (var existingOptionInDb in pollToUpdate.Options.ToList()) // ToList to allow modification
                    {
                        var submittedOption = viewModel.ExistingOptions.FirstOrDefault(eo => eo.Id == existingOptionInDb.Id);
                        if (submittedOption == null || string.IsNullOrWhiteSpace(submittedOption.OptionText))
                        {
                            optionsToRemove.Add(existingOptionInDb);
                        }
                        else
                        {
                            existingOptionInDb.OptionText = submittedOption.OptionText;
                        }
                    }
                    _context.PollOptions.RemoveRange(optionsToRemove);

                    // Add new options
                    if (viewModel.Options != null)
                    {
                        foreach (var newOptionText in viewModel.Options.Where(o => !string.IsNullOrWhiteSpace(o)))
                        {
                            pollToUpdate.Options.Add(new PollOption { OptionText = newOptionText });
                        }
                    }
                    
                    _context.Update(pollToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Poll updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PollExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }
            // If model state is invalid, repopulate ExistingOptions if they were lost during postback
            if (viewModel.ExistingOptions == null || !viewModel.ExistingOptions.Any()){
                 var poll = await _context.Polls.Include(p => p.Options).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                 if(poll != null) {
                    viewModel.ExistingOptions = poll.Options.Select(o => new PollOptionViewModel { Id = o.Id, OptionText = o.OptionText }).ToList();
                 }
            }
            return View(viewModel);
        }

        // GET: Polls/Delete/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["ActiveLink"] = "ManagePolls";
            if (id == null) { return NotFound(); }
            var poll = await _context.Polls.Include(p => p.CreatedBy).FirstOrDefaultAsync(m => m.Id == id);
            if (poll == null) { return NotFound(); }
            return View(poll);
        }

        // POST: Polls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var poll = await _context.Polls.FindAsync(id);
            if (poll != null)
            {
                //Cascade delete for options and votes is configured in DbContext
                _context.Polls.Remove(poll);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Poll deleted successfully.";
            }
            return RedirectToAction(nameof(Manage));
        }
        
        // GET: Polls/Results/5 (Admin view for results)
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Results(int? id)
        {
            ViewData["ActiveLink"] = "ManagePolls";
            if (id == null) { return NotFound(); }

            var poll = await _context.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poll == null) { return NotFound(); }

            var resultsViewModel = new PollResultsViewModel
            {
                PollId = poll.Id,
                PollTitle = poll.Title,
                PollDescription = poll.Description,
                TotalVotes = poll.Votes.Count,
                OptionsWithVotes = poll.Options.Select(o => new PollOptionResultViewModel
                {
                    OptionId = o.Id,
                    OptionText = o.OptionText,
                    VoteCount = _context.PollVotes.Count(v => v.PollOptionId == o.Id && v.PollId == poll.Id) 
                }).ToList()
            };

            return View(resultsViewModel);
        }

        // USER-FACING ACTIONS START HERE

        // GET: Polls or Polls/Index (User view for active polls)
        [Authorize] // Ensure user is logged in to see polls
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var pollsData = await _context.Polls
                .Where(p => p.IsActive && p.StartDate <= DateTime.Now) // Show active polls that have started
                .Include(p => p.Options) 
                .Include(p => p.Votes) // Eager load votes for processing
                .OrderByDescending(p => p.EndDate) 
                .ToListAsync(); // Fetch data from DB

            // Process data in memory to build ViewModels
            var viewModels = new List<PollResultsViewModel>();
            foreach (var p in pollsData)
            {
                var userVote = p.Votes.FirstOrDefault(v => v.UserId == userId);
                var viewModel = new PollResultsViewModel
                {
                    PollId = p.Id,
                    PollTitle = p.Title,
                    PollDescription = p.Description,
                    TotalVotes = p.Votes.Count, // Count votes from the loaded collection
                    OptionsWithVotes = p.Options.Select(o => new PollOptionResultViewModel
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText,
                        // Count votes for this specific option from the loaded collection
                        VoteCount = p.Votes.Count(v => v.PollOptionId == o.Id) 
                    }).ToList(),
                    UserHasVoted = userVote != null,
                    UserVoteOptionId = userVote?.PollOptionId,
                    IsPollOpen = p.IsOpen, 
                    HasPollEnded = p.HasEnded, 
                    EndDate = p.EndDate 
                };
                viewModels.Add(viewModel);
            }

            return View("UserIndex", viewModels); // Pass the processed list of ViewModels
        }

        // POST: Polls/Vote
        [HttpPost]
        [Authorize] // Ensure user is logged in to vote
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int pollId, int selectedOptionId)
        {
            var userId = _userManager.GetUserId(User);
            var poll = await _context.Polls.Include(p => p.Votes).FirstOrDefaultAsync(p => p.Id == pollId);

            if (poll == null)
            {
                TempData["ErrorMessage"] = "Poll not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!poll.IsOpen)
            {
                TempData["ErrorMessage"] = "This poll is not currently open for voting.";
                return RedirectToAction(nameof(Index));
            }

            bool alreadyVoted = await _context.PollVotes.AnyAsync(pv => pv.PollId == pollId && pv.UserId == userId);
            if (alreadyVoted)
            {
                TempData["ErrorMessage"] = "You have already voted in this poll.";
                return RedirectToAction(nameof(Index)); // Or redirect to results view for this poll
            }

            var vote = new PollVote
            {
                PollId = pollId,
                PollOptionId = selectedOptionId,
                UserId = userId,
                VotedAt = DateTime.UtcNow
            };

            _context.PollVotes.Add(vote);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your vote has been recorded!";
            return RedirectToAction(nameof(Index)); // Or redirect to results view: RedirectToAction("ViewPollResults", new { id = pollId });
        }

        // GET: Polls/ViewPollResults/5 (User view for results, could be same as admin or a simplified version)
        [Authorize]
        public async Task<IActionResult> ViewPollResults(int? id)
        {
            if (id == null) { return NotFound(); }
            var userId = _userManager.GetUserId(User);

            var poll = await _context.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poll == null || !poll.IsActive) // Users should only see results for active polls they could interact with
            {
                return NotFound(); 
            }
            
            var resultsViewModel = new PollResultsViewModel
            {
                PollId = poll.Id,
                PollTitle = poll.Title,
                PollDescription = poll.Description,
                TotalVotes = poll.Votes.Count(),
                OptionsWithVotes = poll.Options.Select(o => new PollOptionResultViewModel
                {
                    OptionId = o.Id,
                    OptionText = o.OptionText,
                    VoteCount = poll.Votes.Count(v => v.PollOptionId == o.Id)
                }).ToList(),
                UserHasVoted = poll.Votes.Any(v => v.UserId == userId),
                UserVoteOptionId = poll.Votes.FirstOrDefault(v => v.UserId == userId)?.PollOptionId,
                IsPollOpen = poll.IsOpen,
                HasPollEnded = poll.HasEnded,
                EndDate = poll.EndDate // Populate EndDate
            };
            // Decide if public users always see results or only after voting/poll ends.
            // For now, they see it if they navigate here.
            return View("Results", resultsViewModel); // Reuse admin Results.cshtml, or create a UserResults.cshtml
        }

        private bool PollExists(int id)
        {
            return _context.Polls.Any(e => e.Id == id);
        }
    }
} 