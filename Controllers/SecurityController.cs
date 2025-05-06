using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;

namespace HomeownersAssociation.Controllers
{
    [Authorize]
    public class SecurityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SecurityController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Security - Dashboard
        public async Task<IActionResult> Index()
        {
            var viewModel = new SecurityDashboardViewModel
            {
                // Get pending visitor passes
                PendingVisitorPasses = await _context.VisitorPasses
                    .Where(vp => vp.Status == "Pending")
                    .Include(vp => vp.RequestedBy)
                    .OrderBy(vp => vp.VisitDate)
                    .Take(5)
                    .ToListAsync(),

                // Get today's visitor passes
                TodayVisitorPasses = await _context.VisitorPasses
                    .Where(vp => vp.VisitDate.Date == DateTime.Today.Date)
                    .Include(vp => vp.RequestedBy)
                    .OrderBy(vp => vp.ExpectedTimeIn)
                    .ToListAsync(),

                // Get recent vehicle registrations
                RecentVehicles = await _context.Vehicles
                    .Include(v => v.Owner)
                    .OrderByDescending(v => v.RegistrationDate)
                    .Take(5)
                    .ToListAsync(),

                // Get stats
                TotalVisitorPasses = await _context.VisitorPasses.CountAsync(),
                TotalApprovedPasses = await _context.VisitorPasses.CountAsync(vp => vp.Status == "Approved"),
                TotalRejectedPasses = await _context.VisitorPasses.CountAsync(vp => vp.Status == "Rejected"),
                TotalVehicles = await _context.Vehicles.CountAsync(v => v.IsActive),
                TotalEmergencyContacts = await _context.EmergencyContacts.CountAsync(ec => ec.IsActive)
            };

            return View(viewModel);
        }

        // GET: Security/EmergencyContacts - Public view of emergency contacts
        [AllowAnonymous]
        public async Task<IActionResult> EmergencyContacts()
        {
            var contacts = await _context.EmergencyContacts
                .Where(ec => ec.IsActive)
                .OrderBy(ec => ec.PriorityOrder)
                .ThenBy(ec => ec.Name)
                .ToListAsync();

            return View(contacts);
        }
    }
} 