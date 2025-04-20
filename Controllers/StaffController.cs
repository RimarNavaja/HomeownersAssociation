using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Models;
using HomeownersAssociation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IO;
using HomeownersAssociation.Models.ViewModels;

namespace HomeownersAssociation.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public StaffController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        // Dashboard for staff
        public IActionResult Index()
        {
            return View();
        }

        // Announcements Management
        public async Task<IActionResult> Announcements()
        {
            var announcements = await _context.Announcements
                .Include(a => a.Author)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.DatePosted)
                .ToListAsync();

            return View(announcements);
        }

        public IActionResult CreateAnnouncement()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(AnnouncementViewModel viewModel)
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
                return RedirectToAction(nameof(Announcements));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> EditAnnouncement(int? id)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnnouncement(int id, AnnouncementViewModel viewModel)
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
                    return RedirectToAction(nameof(Announcements));
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
            }
            return View(viewModel);
        }

        private bool AnnouncementExists(int id)
        {
            return _context.Announcements.Any(e => e.Id == id);
        }

        // Billing Management - use the existing actions from BillingController
        public async Task<IActionResult> Billing()
        {
            var bills = await _context.Bills
                .Include(b => b.Homeowner)
                .OrderByDescending(b => b.IssueDate)
                .ToListAsync();

            // Get statistics
            var stats = new
            {
                TotalBills = bills.Count,
                PaidBills = bills.Count(b => b.Status == BillStatus.Paid),
                UnpaidBills = bills.Count(b => b.Status == BillStatus.Unpaid),
                OverdueBills = bills.Count(b => b.Status == BillStatus.Overdue),
                TotalAmount = bills.Sum(b => b.Amount),
                PaidAmount = bills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.Amount),
                UnpaidAmount = bills.Where(b => b.Status == BillStatus.Unpaid || b.Status == BillStatus.Overdue).Sum(b => b.Amount)
            };

            ViewBag.Stats = stats;

            return View(bills);
        }

        public IActionResult CreateGlobalBill()
        {
            var model = new BillViewModel
            {
                BillNumber = GenerateBillNumber(),
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGlobalBill(BillViewModel model)
        {
            ModelState.Remove("HomeownerId");
            
            if (ModelState.IsValid)
            {
                var bill = new Bill
                {
                    BillNumber = model.BillNumber,
                    Description = model.Description,
                    Amount = model.Amount,
                    DueDate = model.DueDate,
                    IssueDate = model.IssueDate,
                    Status = model.Status,
                    Type = model.Type,
                    Notes = model.Notes + " (Global bill - not assigned to specific homeowner)"
                };

                _context.Add(bill);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Global bill created successfully!";
                return RedirectToAction(nameof(Billing));
            }

            return View(model);
        }

        public async Task<IActionResult> CreateBill()
        {
            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");

            var model = new BillViewModel
            {
                Homeowners = homeowners,
                BillNumber = GenerateBillNumber(),
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBill(BillViewModel model)
        {
            if (ModelState.IsValid)
            {
                var bill = new Bill
                {
                    BillNumber = model.BillNumber,
                    Description = model.Description,
                    Amount = model.Amount,
                    DueDate = model.DueDate,
                    IssueDate = model.IssueDate,
                    Status = model.Status,
                    HomeownerId = model.HomeownerId,
                    Type = model.Type,
                    Notes = model.Notes
                };

                _context.Add(bill);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bill created successfully!";
                return RedirectToAction(nameof(Billing));
            }

            // If we got this far, something failed, redisplay form
            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");
            model.Homeowners = homeowners;

            return View(model);
        }

        public async Task<IActionResult> EditBill(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");

            var model = new BillViewModel
            {
                Id = bill.Id,
                BillNumber = bill.BillNumber,
                Description = bill.Description,
                Amount = bill.Amount,
                DueDate = bill.DueDate,
                IssueDate = bill.IssueDate,
                Status = bill.Status,
                HomeownerId = bill.HomeownerId,
                Type = bill.Type,
                Notes = bill.Notes,
                Homeowners = homeowners
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBill(int id, BillViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var bill = await _context.Bills.FindAsync(id);
                    if (bill == null)
                    {
                        return NotFound();
                    }

                    bill.BillNumber = model.BillNumber;
                    bill.Description = model.Description;
                    bill.Amount = model.Amount;
                    bill.DueDate = model.DueDate;
                    bill.IssueDate = model.IssueDate;
                    bill.Status = model.Status;
                    bill.HomeownerId = model.HomeownerId;
                    bill.Type = model.Type;
                    bill.Notes = model.Notes;

                    _context.Update(bill);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Bill updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Billing));
            }

            // If we got this far, something failed, redisplay form
            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");
            model.Homeowners = homeowners;

            return View(model);
        }

        public async Task<IActionResult> BillDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Homeowner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Payments
                .Include(p => p.Bill)
                .Include(p => p.Bill.Homeowner)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        private string GenerateBillNumber()
        {
            var lastBill = _context.Bills.OrderByDescending(b => b.Id).FirstOrDefault();
            int billNumber = 1000;
            
            if (lastBill != null)
            {
                if (int.TryParse(lastBill.BillNumber.Replace("BILL-", ""), out int lastNumber))
                {
                    billNumber = lastNumber + 1;
                }
            }
            
            return $"BILL-{billNumber}";
        }

        private bool BillExists(int id)
        {
            return _context.Bills.Any(e => e.Id == id);
        }
    }
} 