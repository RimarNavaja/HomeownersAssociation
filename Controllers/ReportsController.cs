using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HomeownersAssociation.Controllers
{
    [Authorize(Roles = "Admin,Staff")] // Ensure only Admins and Staff can access reports
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public IActionResult Index()
        {
            ViewData["ActiveLink"] = "Reports"; // For highlighting active nav link
            return View();
        }

        // GET: Reports/ServiceRequestsReport
        public async Task<IActionResult> ServiceRequestsReport()
        {
            ViewData["ActiveLink"] = "Reports";
            var viewModel = new ServiceRequestsReportViewModel();

            var serviceRequests = await _context.ServiceRequests
                                              .Include(sr => sr.Category) // Include category for its name
                                              .ToListAsync();

            viewModel.TotalRequests = serviceRequests.Count;

            viewModel.RequestsByStatus = serviceRequests
                .GroupBy(sr => sr.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            viewModel.RequestsByCategory = serviceRequests
                .Where(sr => sr.Category != null) // Ensure category is not null
                .GroupBy(sr => sr.Category.Name) // Group by category name
                .ToDictionary(g => g.Key, g => g.Count());

            // Assuming Priority is an int where 1=Low, 2=Medium, 3=High, etc.
            // You might need a helper function to convert these to text if not already handled.
            // For simplicity, this example groups by the integer value directly.
            // Consider mapping these to user-friendly names in the view or ViewModel.
            var priorityMapping = new Dictionary<int, string>
            {
                { 1, "Low" },
                { 2, "Medium" },
                { 3, "High" },
                { 4, "Urgent" }
                // Add other mappings as defined in your application
            };

            viewModel.RequestsByPriority = serviceRequests
                .GroupBy(sr => sr.Priority) // Priority is int
                .ToDictionary(g => priorityMapping.TryGetValue(g.Key, out var name) ? name : $"Priority {g.Key}", g => g.Count());

            return View(viewModel);
        }
    }
} 