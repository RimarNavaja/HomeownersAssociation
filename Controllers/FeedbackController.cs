using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;

namespace HomeownersAssociation.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public FeedbackController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Feedback
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            // For regular users, show only their own feedback
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var userFeedback = await _context.Feedbacks
                    .Include(f => f.SubmittedBy)
                    .Include(f => f.RespondedBy)
                    .Where(f => f.SubmittedById == currentUser.Id)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();
                
                return View(userFeedback);
            }
            
            // For admin/staff, show all feedback
            var allFeedback = await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.RespondedBy)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
            
            return View("Manage", allFeedback);
        }

        // GET: Feedback/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.RespondedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (feedback == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            
            // Check if the current user is authorized to view this feedback
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && feedback.SubmittedById != currentUser.Id && !feedback.IsPublic)
            {
                return Forbid();
            }

            return View(feedback);
        }

        // GET: Feedback/Create
        public IActionResult Create()
        {
            var model = new FeedbackViewModel
            {
                Status = "New",
                Priority = 2, // Default to Medium priority
                CreatedAt = DateTime.Now
            };
            
            ViewData["FeedbackTypes"] = new List<string> { "Complaint", "Feedback", "Suggestion", "Appreciation" };
            ViewData["Priorities"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Low" },
                new SelectListItem { Value = "2", Text = "Medium" },
                new SelectListItem { Value = "3", Text = "High" }
            };
            
            return View(model);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeedbackViewModel model)
        {
            // Explicitly remove validation errors for certain fields
            ModelState.Remove("SubmittedById");
            ModelState.Remove("SubmittedByName");
            ModelState.Remove("RespondedById");
            ModelState.Remove("RespondedByName");
            ModelState.Remove("AttachmentUrl");
            ModelState.Remove("Attachment");
            ModelState.Remove("Status");
            
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        TempData["ErrorMessage"] = "Current user not found";
                        return PrepareCreateView(model);
                    }

                    var feedback = new Feedback
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Type = model.Type,
                        Status = "New",
                        Priority = model.Priority,
                        IsPublic = model.IsPublic,
                        SubmittedById = currentUser.Id,
                        CreatedAt = DateTime.Now
                    };

                    // Handle file upload if there is an attachment
                    if (model.Attachment != null && model.Attachment.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "feedback");
                        
                        // Create the uploads directory if it doesn't exist
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        // Generate a unique filename
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Attachment.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Attachment.CopyToAsync(fileStream);
                        }
                        
                        // Save the relative URL for database storage
                        feedback.AttachmentUrl = "/uploads/feedback/" + uniqueFileName;
                    }

                    _context.Add(feedback);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Your feedback has been submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error submitting feedback: {ex.Message}";
                    return PrepareCreateView(model);
                }
            }

            var errorMessages = string.Join("; ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
            TempData["ErrorMessage"] = $"Validation errors: {errorMessages}";
            
            return PrepareCreateView(model);
        }

        private IActionResult PrepareCreateView(FeedbackViewModel model)
        {
            ViewData["FeedbackTypes"] = new List<string> { "Complaint", "Feedback", "Suggestion", "Appreciation" };
            ViewData["Priorities"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Low" },
                new SelectListItem { Value = "2", Text = "Medium" },
                new SelectListItem { Value = "3", Text = "High" }
            };
            return View(model);
        }

        // GET: Feedback/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.RespondedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (feedback == null)
            {
                return NotFound();
            }

            var model = new FeedbackViewModel
            {
                Id = feedback.Id,
                Title = feedback.Title,
                Description = feedback.Description,
                Type = feedback.Type,
                Status = feedback.Status,
                Priority = feedback.Priority,
                Response = feedback.Response,
                RespondedAt = feedback.RespondedAt,
                RespondedById = feedback.RespondedById,
                RespondedByName = feedback.RespondedBy?.UserName,
                IsPublic = feedback.IsPublic,
                SubmittedById = feedback.SubmittedById,
                SubmittedByName = feedback.SubmittedBy?.UserName,
                CreatedAt = feedback.CreatedAt,
                UpdatedAt = feedback.UpdatedAt,
                AttachmentUrl = feedback.AttachmentUrl
            };

            ViewData["FeedbackTypes"] = new List<string> { "Complaint", "Feedback", "Suggestion", "Appreciation" };
            ViewData["Statuses"] = new List<string> { "New", "InProgress", "Resolved", "Closed" };
            ViewData["Priorities"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Low" },
                new SelectListItem { Value = "2", Text = "Medium" },
                new SelectListItem { Value = "3", Text = "High" }
            };

            return View(model);
        }

        // POST: Feedback/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, FeedbackViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Explicitly remove validation errors for certain fields
            ModelState.Remove("SubmittedById");
            ModelState.Remove("SubmittedByName");
            ModelState.Remove("RespondedById");
            ModelState.Remove("RespondedByName");
            ModelState.Remove("AttachmentUrl");
            ModelState.Remove("Attachment");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                try
                {
                    var feedback = await _context.Feedbacks.FindAsync(id);
                    if (feedback == null)
                    {
                        return NotFound();
                    }

                    feedback.Title = model.Title;
                    feedback.Description = model.Description;
                    feedback.Type = model.Type;
                    feedback.Status = model.Status;
                    feedback.Priority = model.Priority;
                    feedback.IsPublic = model.IsPublic;
                    feedback.UpdatedAt = DateTime.Now;

                    // If there's a response, update the response fields
                    if (!string.IsNullOrEmpty(model.Response))
                    {
                        var currentUser = await _userManager.GetUserAsync(User);
                        
                        feedback.Response = model.Response;
                        feedback.RespondedById = currentUser.Id;
                        feedback.RespondedAt = DateTime.Now;
                    }

                    // Handle file upload if there is a new attachment
                    if (model.Attachment != null && model.Attachment.Length > 0)
                    {
                        // Delete the old file if it exists
                        if (!string.IsNullOrEmpty(feedback.AttachmentUrl))
                        {
                            string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, feedback.AttachmentUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "feedback");
                        
                        // Create the uploads directory if it doesn't exist
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        // Generate a unique filename
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Attachment.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Attachment.CopyToAsync(fileStream);
                        }
                        
                        // Save the relative URL for database storage
                        feedback.AttachmentUrl = "/uploads/feedback/" + uniqueFileName;
                    }

                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Feedback updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }

            ViewData["FeedbackTypes"] = new List<string> { "Complaint", "Feedback", "Suggestion", "Appreciation" };
            ViewData["Statuses"] = new List<string> { "New", "InProgress", "Resolved", "Closed" };
            ViewData["Priorities"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Low" },
                new SelectListItem { Value = "2", Text = "Medium" },
                new SelectListItem { Value = "3", Text = "High" }
            };
            
            return View(model);
        }

        // GET: Feedback/Public
        [AllowAnonymous]
        public async Task<IActionResult> Public()
        {
            var publicFeedback = await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.RespondedBy)
                .Where(f => f.IsPublic && (f.Status == "Resolved" || f.Status == "Closed"))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
            
            return View(publicFeedback);
        }

        // POST: Feedback/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                // Delete the attachment file if it exists
                if (!string.IsNullOrEmpty(feedback.AttachmentUrl))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, feedback.AttachmentUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Feedback deleted successfully!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.Id == id);
        }
    }
} 