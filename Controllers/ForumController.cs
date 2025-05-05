using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering; // Correct namespace for SelectListItem

namespace HomeownersAssociation.Controllers
{
    [Authorize] // All logged-in users can access the forum
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ForumController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Forum - List categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.ForumCategories
                .Where(fc => fc.IsActive)
                .Include(fc => fc.ForumThreads) // Include threads to potentially show latest activity/count
                .OrderBy(fc => fc.Name)
                .ToListAsync();
            return View(categories);
        }

        // GET: Forum/Category/5 - List threads in a category
        public async Task<IActionResult> Category(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.ForumCategories
                .Include(fc => fc.ForumThreads!)
                    .ThenInclude(ft => ft.User) // Get user info for thread starter
                .Include(fc => fc.ForumThreads!)
                    .ThenInclude(ft => ft.ForumReplies) // Include replies to get count/latest
                .FirstOrDefaultAsync(fc => fc.Id == id && fc.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            // Order threads, e.g., by latest reply or creation date
            category.ForumThreads = category.ForumThreads?.OrderByDescending(ft => ft.ForumReplies!.Any() ? ft.ForumReplies!.Max(fr => fr.CreatedAt) : ft.CreatedAt).ToList();

            return View(category);
        }

        // GET: Forum/Thread/5 - View a specific thread and its replies
        public async Task<IActionResult> Thread(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thread = await _context.ForumThreads
                .Include(ft => ft.Category)
                .Include(ft => ft.User) // Thread starter info
                .Include(ft => ft.ForumReplies!) // Load replies
                    .ThenInclude(fr => fr.User) // Include user info for each reply
                .FirstOrDefaultAsync(m => m.Id == id);

            if (thread == null || !thread.Category!.IsActive) // Check if thread or category is valid
            {
                return NotFound();
            }
            
            // Prepare ViewModel
            var viewModel = new ForumThreadViewModel
            {
                Id = thread.Id,
                CategoryId = thread.CategoryId,
                CategoryName = thread.Category.Name,
                UserId = thread.UserId,
                UserName = thread.User?.UserName, // Or FirstName + LastName
                Title = thread.Title,
                Content = thread.Content,
                CreatedAt = thread.CreatedAt,
                IsLocked = thread.IsLocked,
                Replies = thread.ForumReplies?.OrderBy(r => r.CreatedAt).ToList() ?? new List<ForumReply>(),
                NewReply = new ForumReplyViewModel { ThreadId = thread.Id } // Prep for new reply form
            };

            return View(viewModel);
        }
        
        // GET: Forum/CreateThread/5 - Show form to create a thread in a category
        public async Task<IActionResult> CreateThread(int categoryId)
        {
             var category = await _context.ForumCategories.FindAsync(categoryId);
            if (category == null || !category.IsActive)
            {
                return NotFound(); // Or redirect with error
            }

             var viewModel = new ForumThreadViewModel
            {
                CategoryId = categoryId,
                CategoryName = category.Name
            };
            return View(viewModel);
        }

        // POST: Forum/CreateThread
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThread(ForumThreadViewModel viewModel)
        {
             var category = await _context.ForumCategories.FindAsync(viewModel.CategoryId);
            if (category == null || !category.IsActive)
            {
                ModelState.AddModelError("CategoryId", "Invalid category selected.");
            }

            if (ModelState.IsValid)
            {
                var thread = new ForumThread
                {
                    CategoryId = viewModel.CategoryId,
                    UserId = _userManager.GetUserId(User),
                    Title = viewModel.Title,
                    Content = viewModel.Content,
                    CreatedAt = DateTime.Now,
                    IsLocked = false // New threads aren't locked by default
                };
                _context.Add(thread);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thread created successfully!";
                return RedirectToAction(nameof(Thread), new { id = thread.Id });
            }
            
            // If failed, repopulate necessary fields
            viewModel.CategoryName = category?.Name;
            return View(viewModel);
        }
        
        // POST: Forum/CreateReply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReply(ForumReplyViewModel viewModel)
        {
             var thread = await _context.ForumThreads.FindAsync(viewModel.ThreadId);
            if (thread == null || thread.IsLocked)
            {
                // Handle error - thread not found or locked
                 TempData["ErrorMessage"] = "Cannot reply to this thread. It may be locked or deleted.";
                 // Attempt to redirect back, might need better error handling
                 return RedirectToAction(nameof(Index)); 
            }

            if (ModelState.IsValid)
            {
                var reply = new ForumReply
                {
                    ThreadId = viewModel.ThreadId,
                    UserId = _userManager.GetUserId(User),
                    Content = viewModel.Content,
                    CreatedAt = DateTime.Now
                };
                _context.Add(reply);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reply posted successfully!";
                return RedirectToAction(nameof(Thread), new { id = viewModel.ThreadId });
            }
            
             // If reply fails validation, ideally redirect back to Thread view with errors.
             // This requires loading the full ThreadViewModel again.
            TempData["ErrorMessage"] = "Reply could not be posted. Please check your input."; 
             // Simplified redirect for now:
            return RedirectToAction(nameof(Thread), new { id = viewModel.ThreadId });
        }
        
        // POST: Forum/ToggleLock/5 - Admin/Staff only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Admin,Staff")]
        public async Task<IActionResult> ToggleLock(int id)
        {
             var thread = await _context.ForumThreads.FindAsync(id);
            if (thread == null)
            {
                return NotFound();
            }
            thread.IsLocked = !thread.IsLocked;
            _context.Update(thread);
            await _context.SaveChangesAsync();
             TempData["SuccessMessage"] = thread.IsLocked ? "Thread locked." : "Thread unlocked.";
            return RedirectToAction(nameof(Thread), new { id = id });
        }

         // POST: Forum/DeleteThread/5 - Admin/Staff or Original Poster?
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles="Admin,Staff")] // Or add logic to allow OP delete
        public async Task<IActionResult> DeleteThread(int id)
        {
            var thread = await _context.ForumThreads.FindAsync(id);
            if (thread == null)
            {
                return NotFound();
            }

            // Security Check: Allow Admin/Staff or OP to delete?
             var currentUserId = _userManager.GetUserId(User);
             if (thread.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
             {
                 return Forbid();
             }

            // Note: Replies will cascade delete due to DB config
            _context.ForumThreads.Remove(thread);
            await _context.SaveChangesAsync();
             TempData["SuccessMessage"] = "Thread deleted successfully!";
            return RedirectToAction(nameof(Category), new { id = thread.CategoryId });
        }

        // POST: Forum/DeleteReply/5 - Admin/Staff or Original Poster?
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _context.ForumReplies.FindAsync(id);
            if (reply == null)
            {
                return NotFound();
            }

            // Security Check: Allow Admin/Staff or OP to delete?
             var currentUserId = _userManager.GetUserId(User);
             if (reply.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
             {
                 return Forbid();
             }

            var threadId = reply.ThreadId; // Store before deleting
            _context.ForumReplies.Remove(reply);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Reply deleted successfully!";
            return RedirectToAction(nameof(Thread), new { id = threadId });
        }
        
        // --- Helper --- 
         private async Task<IEnumerable<SelectListItem>> GetCategorySelectList()
        {
             return await _context.ForumCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync();
        }
    }
} 