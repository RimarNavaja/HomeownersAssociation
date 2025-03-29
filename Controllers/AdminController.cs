using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace HomeownersAssociation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> PendingApprovals()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => !u.IsApproved && u.UserType == UserType.Homeowner)
                .ToListAsync();

            return View(pendingUsers);
        }

        public async Task<IActionResult> AllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.IsApproved = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Add to correct role based on user type
                if (user.UserType == UserType.Homeowner)
                {
                    await _userManager.AddToRoleAsync(user, "Homeowner");
                }

                TempData["SuccessMessage"] = $"User {user.Email} has been approved.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error approving user.";
            }

            return RedirectToAction(nameof(PendingApprovals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.Email} has been rejected and removed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error rejecting user.";
            }

            return RedirectToAction(nameof(PendingApprovals));
        }

        // Staff Management
        public async Task<IActionResult> StaffMembers()
        {
            var staffMembers = await _userManager.Users
                .Where(u => u.UserType == UserType.Staff)
                .ToListAsync();

            return View(staffMembers);
        }

        public IActionResult CreateStaff()
        {
            return View(new CreateStaffViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    LotNumber = "N/A",
                    BlockNumber = "N/A",
                    RegistrationDate = DateTime.Now,
                    IsApproved = true, // Staff is auto-approved
                    UserType = UserType.Staff
                };

                // Handle profile picture upload
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePicture.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }

                    user.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Add staff to Staff role
                    await _userManager.AddToRoleAsync(user, "Staff");

                    TempData["SuccessMessage"] = "Staff member created successfully!";
                    return RedirectToAction(nameof(StaffMembers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> EditStaff(string id)
        {
            var staff = await _userManager.FindByIdAsync(id);
            if (staff == null || staff.UserType != UserType.Staff)
            {
                return NotFound();
            }

            var model = new EditStaffViewModel
            {
                Id = staff.Id,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                Email = staff.Email,
                Address = staff.Address,
                ProfilePictureUrl = staff.ProfilePictureUrl,
                IsActive = staff.IsApproved
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(EditStaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var staff = await _userManager.FindByIdAsync(model.Id);
            if (staff == null || staff.UserType != UserType.Staff)
            {
                return NotFound();
            }

            staff.FirstName = model.FirstName;
            staff.LastName = model.LastName;
            staff.Address = model.Address;
            staff.IsApproved = model.IsActive;

            // Handle profile picture upload
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                // Delete old profile picture if exists
                if (!string.IsNullOrEmpty(staff.ProfilePictureUrl))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, staff.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePicture.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(fileStream);
                }

                staff.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(staff);
            if (result.Succeeded)
            {
                // Handle password change if provided
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(staff);
                    var passwordResult = await _userManager.ResetPasswordAsync(staff, token, model.NewPassword);

                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = "Staff member updated successfully!";
                return RedirectToAction(nameof(StaffMembers));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var staff = await _userManager.FindByIdAsync(id);

            if (staff == null || staff.UserType != UserType.Staff)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(staff);

            if (result.Succeeded)
            {
                // Delete profile picture if exists
                if (!string.IsNullOrEmpty(staff.ProfilePictureUrl))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, staff.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                TempData["SuccessMessage"] = $"Staff member {staff.Email} has been deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting staff member.";
            }

            return RedirectToAction(nameof(StaffMembers));
        }
    }
}