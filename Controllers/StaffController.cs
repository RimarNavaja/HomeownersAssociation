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

        // Removed duplicated Announcement and Billing actions
        // Staff will now use AnnouncementsController and BillingController directly
    }
} 