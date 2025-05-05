using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace HomeownersAssociation.Controllers
{
    [Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can manage facilities
    public class FacilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Facilities
        public async Task<IActionResult> Index()
        {
            return View(await _context.Facilities.ToListAsync());
        }

        // GET: Facilities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .Include(f => f.Reservations!) // Include reservations for this facility
                    .ThenInclude(r => r.User) // Include user info for reservations
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (facility == null)
            {
                return NotFound();
            }
            
            // Order reservations for display
            facility.Reservations = facility.Reservations?.OrderBy(r => r.StartTime).ToList();

            return View(facility);
        }

        // GET: Facilities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Facilities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacilityViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var facility = new Facility
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    Capacity = viewModel.Capacity,
                    RatePerHour = viewModel.RatePerHour,
                    IsActive = viewModel.IsActive,
                    MaintenanceSchedule = viewModel.MaintenanceSchedule
                };
                _context.Add(facility);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Facility created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Facilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities.FindAsync(id);
            if (facility == null)
            {
                return NotFound();
            }

            var viewModel = new FacilityViewModel
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                Capacity = facility.Capacity,
                RatePerHour = facility.RatePerHour,
                IsActive = facility.IsActive,
                MaintenanceSchedule = facility.MaintenanceSchedule
            };
            return View(viewModel);
        }

        // POST: Facilities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacilityViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var facility = await _context.Facilities.FindAsync(id);
                    if (facility == null) 
                    {
                        return NotFound();
                    }

                    facility.Name = viewModel.Name;
                    facility.Description = viewModel.Description;
                    facility.Capacity = viewModel.Capacity;
                    facility.RatePerHour = viewModel.RatePerHour;
                    facility.IsActive = viewModel.IsActive;
                    facility.MaintenanceSchedule = viewModel.MaintenanceSchedule;
                    
                    _context.Update(facility);
                    await _context.SaveChangesAsync();
                     TempData["SuccessMessage"] = "Facility updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacilityExists(viewModel.Id))
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

        // GET: Facilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (facility == null)
            {
                return NotFound();
            }

            // Check if there are any associated reservations
            bool hasReservations = await _context.FacilityReservations.AnyAsync(r => r.FacilityId == id);
            if (hasReservations)
            {
                TempData["ErrorMessage"] = "Cannot delete facility with existing reservations. Please cancel or reassign reservations first.";
                return RedirectToAction(nameof(Index));
            }

            return View(facility);
        }

        // POST: Facilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
             if (facility == null)
            {
                return NotFound();
            }
            
            // Double-check for reservations before deleting
             bool hasReservations = await _context.FacilityReservations.AnyAsync(r => r.FacilityId == id);
            if (hasReservations)
            {
                TempData["ErrorMessage"] = "Cannot delete facility with existing reservations.";
                return RedirectToAction(nameof(Index));
            }

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Facility deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool FacilityExists(int id)
        {
            return _context.Facilities.Any(e => e.Id == id);
        }
    }
} 