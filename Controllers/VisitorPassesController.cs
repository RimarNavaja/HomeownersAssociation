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
    public class VisitorPassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VisitorPassesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: VisitorPasses
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var visitorPasses = await _context.VisitorPasses
                .Include(v => v.RequestedBy)
                .Where(v => v.RequestedById == userId)
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
            
            return View(visitorPasses);
        }
        
        // GET: VisitorPasses/Manage
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Manage()
        {
            var visitorPasses = await _context.VisitorPasses
                .Include(v => v.RequestedBy)
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
            
            return View(visitorPasses);
        }

        // GET: VisitorPasses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitorPass = await _context.VisitorPasses
                .Include(v => v.RequestedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (visitorPass == null)
            {
                return NotFound();
            }

            // Security check: Only admin, staff, or the pass requestor can view details
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && visitorPass.RequestedById != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(visitorPass);
        }

        // GET: VisitorPasses/Create
        public IActionResult Create()
        {
            var viewModel = new VisitorPassViewModel
            {
                RequestedById = _userManager.GetUserId(User) ?? string.Empty,
                VisitDate = DateTime.Today,
                ExpectedTimeIn = DateTime.Today.AddHours(9),
                ExpectedTimeOut = DateTime.Today.AddHours(17)
            };
            
            return View(viewModel);
        }

        // POST: VisitorPasses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VisitorPassViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var visitorPass = new VisitorPass
                {
                    RequestedById = viewModel.RequestedById,
                    VisitorName = viewModel.VisitorName,
                    Purpose = viewModel.Purpose,
                    VisitDate = viewModel.VisitDate,
                    ExpectedTimeIn = viewModel.ExpectedTimeIn,
                    ExpectedTimeOut = viewModel.ExpectedTimeOut,
                    VehicleDetails = viewModel.VehicleDetails,
                    Status = "Pending",
                    Notes = viewModel.Notes,
                    CreatedAt = DateTime.Now
                };
                
                _context.Add(visitorPass);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Visitor pass request has been submitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            return View(viewModel);
        }

        // GET: VisitorPasses/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitorPass = await _context.VisitorPasses
                .Include(v => v.RequestedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (visitorPass == null)
            {
                return NotFound();
            }

            var viewModel = new VisitorPassViewModel
            {
                Id = visitorPass.Id,
                RequestedById = visitorPass.RequestedById,
                RequestedByName = visitorPass.RequestedBy?.UserName,
                VisitorName = visitorPass.VisitorName,
                Purpose = visitorPass.Purpose,
                VisitDate = visitorPass.VisitDate,
                ExpectedTimeIn = visitorPass.ExpectedTimeIn,
                ExpectedTimeOut = visitorPass.ExpectedTimeOut,
                VehicleDetails = visitorPass.VehicleDetails,
                Status = visitorPass.Status,
                ActualTimeIn = visitorPass.ActualTimeIn,
                ActualTimeOut = visitorPass.ActualTimeOut,
                Notes = visitorPass.Notes
            };
            
            return View(viewModel);
        }

        // POST: VisitorPasses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, VisitorPassViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var visitorPass = await _context.VisitorPasses.FindAsync(id);
                    
                    if (visitorPass == null)
                    {
                        return NotFound();
                    }
                    
                    visitorPass.VisitorName = viewModel.VisitorName;
                    visitorPass.Purpose = viewModel.Purpose;
                    visitorPass.VisitDate = viewModel.VisitDate;
                    visitorPass.ExpectedTimeIn = viewModel.ExpectedTimeIn;
                    visitorPass.ExpectedTimeOut = viewModel.ExpectedTimeOut;
                    visitorPass.VehicleDetails = viewModel.VehicleDetails;
                    visitorPass.Status = viewModel.Status;
                    visitorPass.ActualTimeIn = viewModel.ActualTimeIn;
                    visitorPass.ActualTimeOut = viewModel.ActualTimeOut;
                    visitorPass.Notes = viewModel.Notes;
                    
                    _context.Update(visitorPass);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Visitor pass has been updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitorPassExists(viewModel.Id))
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
            
            return View(viewModel);
        }

        // POST: VisitorPasses/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var visitorPass = await _context.VisitorPasses.FindAsync(id);
            
            if (visitorPass == null)
            {
                return NotFound();
            }
            
            visitorPass.Status = status;
            _context.Update(visitorPass);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Visitor pass status has been updated to {status}.";
            return RedirectToAction(nameof(Manage));
        }

        // POST: VisitorPasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visitorPass = await _context.VisitorPasses.FindAsync(id);
            
            if (visitorPass == null)
            {
                return NotFound();
            }

            // Security check: Only admin, staff, or the pass requestor can delete
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && visitorPass.RequestedById != _userManager.GetUserId(User))
            {
                return Forbid();
            }
            
            _context.VisitorPasses.Remove(visitorPass);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Visitor pass has been deleted successfully.";
            
            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                return RedirectToAction(nameof(Manage));
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: VisitorPasses/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var visitorPass = await _context.VisitorPasses.FindAsync(id);
            
            if (visitorPass == null)
            {
                return NotFound();
            }
            
            visitorPass.ActualTimeIn = DateTime.Now;
            
            if (visitorPass.Status == "Pending")
            {
                visitorPass.Status = "Approved";
            }
            
            _context.Update(visitorPass);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Visitor has been checked in successfully.";
            return RedirectToAction(nameof(Manage));
        }

        // POST: VisitorPasses/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var visitorPass = await _context.VisitorPasses.FindAsync(id);
            
            if (visitorPass == null)
            {
                return NotFound();
            }
            
            visitorPass.ActualTimeOut = DateTime.Now;
            _context.Update(visitorPass);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Visitor has been checked out successfully.";
            return RedirectToAction(nameof(Manage));
        }

        private bool VisitorPassExists(int id)
        {
            return _context.VisitorPasses.Any(e => e.Id == id);
        }
    }
} 