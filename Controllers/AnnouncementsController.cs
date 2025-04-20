using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using HomeownersAssociation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace HomeownersAssociation.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AnnouncementsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Announcements (for logged-in users)
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var announcements = await _context.Announcements
                .Include(a => a.Author)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.DatePosted)
                .ToListAsync();

            // Filter out expired and inactive announcements for non-admin users
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                announcements = announcements
                    .Where(a => a.IsActive &&
                               (a.ExpiryDate == null || a.ExpiryDate >= DateTime.Now))
                    .ToList();
            }

            ViewData["Title"] = "Community Announcements";
            return View(announcements);
        }

        // GET: Announcements/Public (for public users)
        public async Task<IActionResult> Public()
        {
            var announcements = await _context.Announcements
                .Include(a => a.Author)
                .Where(a => a.IsPublic && a.IsActive &&
                          (a.ExpiryDate == null || a.ExpiryDate >= DateTime.Now))
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.DatePosted)
                .ToListAsync();

            ViewData["Title"] = "Public Announcements";
            return View("Index", announcements);
        }

        // GET: Announcements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            // Only allow public access if the announcement is public
            if (!User.Identity.IsAuthenticated && !announcement.IsPublic)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if announcement is active or user is admin/staff
            if (!announcement.IsActive && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return NotFound();
            }

            // Check if announcement is expired and user is not admin/staff
            if (announcement.ExpiryDate != null &&
                announcement.ExpiryDate < DateTime.Now &&
                !User.IsInRole("Admin") &&
                !User.IsInRole("Staff"))
            {
                return NotFound();
            }

            return View(announcement);
        }

        // GET: Announcements/Create
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create(AnnouncementViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var announcement = new Announcement
                {
                    Title = viewModel.Title,
                    Content = viewModel.Content,
                    DatePosted = DateTime.Now,
                    ExpiryDate = viewModel.ExpiryDate,
                    Priority = viewModel.Priority,
                    IsActive = viewModel.IsActive,
                    IsPublic = viewModel.IsPublic,
                    AuthorId = _userManager.GetUserId(User)
                };

                // Handle file upload
                if (viewModel.Attachment != null && viewModel.Attachment.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "announcements");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.Attachment.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.Attachment.CopyToAsync(fileStream);
                    }

                    announcement.AttachmentUrl = "/uploads/announcements/" + uniqueFileName;
                }

                _context.Add(announcement);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Announcement created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Announcements/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            var viewModel = new AnnouncementViewModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                ExpiryDate = announcement.ExpiryDate,
                Priority = announcement.Priority,
                IsActive = announcement.IsActive,
                IsPublic = announcement.IsPublic,
                ExistingAttachmentUrl = announcement.AttachmentUrl
            };

            return View(viewModel);
        }

        // POST: Announcements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, AnnouncementViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var announcement = await _context.Announcements.FindAsync(id);
                    if (announcement == null)
                    {
                        return NotFound();
                    }

                    announcement.Title = viewModel.Title;
                    announcement.Content = viewModel.Content;
                    announcement.ExpiryDate = viewModel.ExpiryDate;
                    announcement.Priority = viewModel.Priority;
                    announcement.IsActive = viewModel.IsActive;
                    announcement.IsPublic = viewModel.IsPublic;

                    // Handle file upload
                    if (viewModel.Attachment != null && viewModel.Attachment.Length > 0)
                    {
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(announcement.AttachmentUrl))
                        {
                            var oldFilePath = Path.Combine(_environment.WebRootPath, announcement.AttachmentUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "announcements");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.Attachment.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.Attachment.CopyToAsync(fileStream);
                        }

                        announcement.AttachmentUrl = "/uploads/announcements/" + uniqueFileName;
                    }

                    _context.Update(announcement);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Announcement updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnouncementExists(viewModel.Id))
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
            return View(viewModel);
        }

        // GET: Announcements/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // POST: Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            // Delete attachment if exists
            if (!string.IsNullOrEmpty(announcement.AttachmentUrl))
            {
                var filePath = Path.Combine(_environment.WebRootPath, announcement.AttachmentUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Announcement deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool AnnouncementExists(int id)
        {
            return _context.Announcements.Any(e => e.Id == id);
        }
    }
}