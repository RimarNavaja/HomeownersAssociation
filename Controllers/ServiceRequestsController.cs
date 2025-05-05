using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList

namespace HomeownersAssociation.Controllers
{
    [Authorize] // All actions require login
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ServiceRequests/Index - Show requests for the current user
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var requests = await _context.ServiceRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.Category)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(requests);
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.ServiceRequests
                .Include(sr => sr.User)
                .Include(sr => sr.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (request == null)
            {
                return NotFound();
            }

            // Security check: Ensure user is viewing their own request OR is admin/staff
            var currentUserId = _userManager.GetUserId(User);
            if (request.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid(); // Or RedirectToAction("AccessDenied", "Account")
            }

            return View(request);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ServiceRequestViewModel
            {
                AvailableCategories = await GetCategorySelectList(),
                AvailablePriorities = GetPrioritySelectList()
            };
            return View(viewModel);
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var serviceRequest = new ServiceRequest
                {
                    UserId = _userManager.GetUserId(User),
                    CategoryId = viewModel.CategoryId,
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Priority = viewModel.Priority,
                    Status = ServiceRequestStatus.New, // Initial status
                    CreatedAt = DateTime.Now
                };
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service request submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            // If failed, reload dropdowns
            viewModel.AvailableCategories = await GetCategorySelectList();
            viewModel.AvailablePriorities = GetPrioritySelectList();
            return View(viewModel);
        }
        
        // GET: ServiceRequests/Manage - Admin/Staff view
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Manage(string filterStatus = "New")
        {
            ViewData["CurrentFilter"] = filterStatus;

            var requestsQuery = _context.ServiceRequests
                .Include(r => r.Category)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "All")
            {
                requestsQuery = requestsQuery.Where(r => r.Status == filterStatus);
            }
            
            var orderedRequests = await requestsQuery
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(orderedRequests);
        }

        // POST: ServiceRequests/UpdateStatus/5 - Admin/Staff update status
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            // Validate the status
            if (status != ServiceRequestStatus.InProgress && 
                status != ServiceRequestStatus.Completed && 
                status != ServiceRequestStatus.Cancelled &&
                status != ServiceRequestStatus.New) // Allow reverting to New?
            {
                TempData["ErrorMessage"] = "Invalid status provided.";
                return RedirectToAction(nameof(Manage));
            }

            request.Status = status;
            if (status == ServiceRequestStatus.Completed || status == ServiceRequestStatus.Cancelled)
            {
                request.CompletedAt = DateTime.Now;
            }
            else
            {
                 request.CompletedAt = null; // Clear completed date if reopening
            }

            _context.Update(request);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Request status updated to {status}.";
            return RedirectToAction(nameof(Manage), new { filterStatus = status });
        }

        // --- Helper Methods ---
        private async Task<IEnumerable<SelectListItem>> GetCategorySelectList()
        {
             return await _context.ServiceCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync();
        }

        private IEnumerable<SelectListItem> GetPrioritySelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = ServiceRequestPriority.Low.ToString(), Text = "Low" },
                new SelectListItem { Value = ServiceRequestPriority.Medium.ToString(), Text = "Medium" },
                new SelectListItem { Value = ServiceRequestPriority.High.ToString(), Text = "High" },
            };
        }
    }
} 