using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HomeownersAssociation.Controllers
{
    [Authorize] // All actions require login unless otherwise specified
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations/Index - Show available facilities to book
        public async Task<IActionResult> Index()
        {
            var facilities = await _context.Facilities
                                        .Where(f => f.IsActive)
                                        .OrderBy(f => f.Name)
                                        .ToListAsync();
            return View(facilities);
        }
        
        // GET: Reservations/FacilityDetails/5 - Show details and existing reservations for a facility
        public async Task<IActionResult> FacilityDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .Include(f => f.Reservations!)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (facility == null)
            {
                return NotFound();
            }

             // Order reservations for display
            facility.Reservations = facility.Reservations?
                .Where(r => r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Rejected)
                .OrderBy(r => r.StartTime) // Order by DateTime StartTime
                .ToList();

            return View(facility);
        }

        // GET: Reservations/MyReservations - Show reservations for the current user
        public async Task<IActionResult> MyReservations()
        {
            var userId = _userManager.GetUserId(User);
            var reservations = await _context.FacilityReservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Facility)
                .OrderByDescending(r => r.StartTime) // Sort directly by DateTime StartTime
                .ToListAsync();
                
            return View(reservations);
        }

        // GET: Reservations/Create/5 - Show form to create reservation for a specific facility
        public async Task<IActionResult> Create(int facilityId)
        {
            var facility = await _context.Facilities.FindAsync(facilityId);
            if (facility == null || !facility.IsActive)
            {
                return NotFound();
            }

            // Get existing reservations for conflict checking
            var existingReservations = await _context.FacilityReservations
                .Where(r => r.FacilityId == facilityId && 
                             r.StartTime >= DateTime.Today && // Compare against DateTime StartTime
                             r.Status != ReservationStatus.Cancelled && 
                             r.Status != ReservationStatus.Rejected)
                .OrderBy(r => r.StartTime) // Sort directly by DateTime StartTime
                .ToListAsync();

            var viewModel = new FacilityReservationViewModel
            {
                FacilityId = facility.Id,
                FacilityName = facility.Name,
                ReservationDate = DateTime.Today,
                ExistingReservations = existingReservations // Use the existing reservations
            };

            return View(viewModel);
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacilityReservationViewModel viewModel)
        {
            var facility = await _context.Facilities.FindAsync(viewModel.FacilityId);
            if (facility == null || !facility.IsActive)
            {
                ModelState.AddModelError("", "Selected facility is not available.");
            }
            
            // Basic Time Validation
            if (viewModel.StartTime >= viewModel.EndTime)
            {
                 ModelState.AddModelError("EndTime", "End time must be after start time.");
            }
            if (viewModel.ReservationDate < DateTime.Today)
            {
                 ModelState.AddModelError("ReservationDate", "Reservation date cannot be in the past.");
            }

            // Conflict Check
            if (ModelState.IsValid) // Only check for conflicts if basic validation passes
            {
                var proposedStartDateTime = viewModel.ReservationDate.Date + viewModel.StartTime;
                var proposedEndDateTime = viewModel.ReservationDate.Date + viewModel.EndTime;

                // Check for conflicts using DateTime columns
                bool isConflict = await _context.FacilityReservations
                    .AnyAsync(r => r.FacilityId == viewModel.FacilityId &&
                                 r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Rejected &&
                                 // Check for time overlap using DateTime comparison
                                 proposedStartDateTime < r.EndTime && // Proposed start is before existing end
                                 proposedEndDateTime > r.StartTime);  // Proposed end is after existing start

                if (isConflict)
                {
                    ModelState.AddModelError("", $"The selected time slot conflicts with an existing reservation.");
                }
            }

            if (ModelState.IsValid)
            {
                // Combine Date and Time from ViewModel before creating the entity
                var startDateTime = viewModel.ReservationDate.Date + viewModel.StartTime;
                var endDateTime = viewModel.ReservationDate.Date + viewModel.EndTime;

                var reservation = new FacilityReservation
                {
                    FacilityId = viewModel.FacilityId,
                    UserId = _userManager.GetUserId(User),
                    // Assign combined DateTime values
                    StartTime = startDateTime,
                    EndTime = endDateTime, 
                    Purpose = viewModel.Purpose,
                    Status = ReservationStatus.Pending, 
                    CreatedAt = DateTime.Now
                };
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reservation request submitted successfully! It is pending approval.";
                return RedirectToAction(nameof(MyReservations));
            }

            // If failed, reload necessary data for the view
            viewModel.FacilityName = facility?.Name;
            // Correctly reload and sort existing reservations for the view
            viewModel.ExistingReservations = await _context.FacilityReservations
                 .Where(r => r.FacilityId == viewModel.FacilityId && 
                              r.StartTime >= DateTime.Today && 
                              r.Status != ReservationStatus.Cancelled && 
                              r.Status != ReservationStatus.Rejected)
                 .OrderBy(r => r.StartTime) // Sort directly by DateTime StartTime
                 .ToListAsync();
                 
            return View(viewModel);
        }
        
        // GET: Reservations/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
             if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.FacilityReservations
                .Include(r => r.Facility)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }
            
            // Ensure only the user who made the reservation or an admin/staff can view the cancel page
            var currentUserId = _userManager.GetUserId(User);
            if (reservation.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid();
            }

            // Check if it's already cancelled or too late to cancel (e.g., past)
            if (reservation.Status == ReservationStatus.Cancelled || reservation.StartTime < DateTime.Now)
            {
                 TempData["ErrorMessage"] = "This reservation cannot be cancelled.";
                 return RedirectToAction(nameof(MyReservations));
            }

            return View(reservation);
        }

        // POST: Reservations/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var reservation = await _context.FacilityReservations.FindAsync(id);
             if (reservation == null)
            {
                return NotFound();
            }

             // Ensure only the user who made the reservation or an admin/staff can cancel
            var currentUserId = _userManager.GetUserId(User);
            if (reservation.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid();
            }
            
            // Check if cancellable
             if (reservation.Status == ReservationStatus.Cancelled || reservation.StartTime < DateTime.Now)
            {
                 TempData["ErrorMessage"] = "This reservation cannot be cancelled.";
                 return RedirectToAction(nameof(MyReservations));
            }

            reservation.Status = ReservationStatus.Cancelled;
            _context.Update(reservation);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            
            // Redirect admin/staff back to management view if they cancelled
            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                return RedirectToAction(nameof(Manage));
            }
            return RedirectToAction(nameof(MyReservations));
        }

        // GET: Reservations/Manage - Admin/Staff view of all reservations
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Manage(string filterStatus = "Pending")
        {
            ViewData["CurrentFilter"] = filterStatus;

            var reservationsQuery = _context.FacilityReservations
                .Include(r => r.Facility)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "All")
            {
                reservationsQuery = reservationsQuery.Where(r => r.Status == filterStatus);
            }
            
            var orderedReservations = await reservationsQuery
                .OrderBy(r => r.StartTime) // Sort directly by DateTime StartTime
                .ToListAsync();

            return View(orderedReservations);
        }

        // POST: Reservations/UpdateStatus/5 - Admin/Staff update status
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var reservation = await _context.FacilityReservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Validate the status
            if (status != ReservationStatus.Approved && status != ReservationStatus.Rejected)
            {
                TempData["ErrorMessage"] = "Invalid status provided.";
                return RedirectToAction(nameof(Manage));
            }

            // Check for conflicts if approving
            if (status == ReservationStatus.Approved)
            {
                 bool isConflict = await _context.FacilityReservations
                    .AnyAsync(r => r.Id != id && // Exclude the current reservation
                                 r.FacilityId == reservation.FacilityId &&
                                 r.Status == ReservationStatus.Approved && // Only check against other Approved
                                 // Check for time overlap using DateTime comparison
                                 reservation.StartTime < r.EndTime && 
                                 reservation.EndTime > r.StartTime); 
                 if (isConflict)
                {
                    TempData["ErrorMessage"] = "Cannot approve: This reservation conflicts with another approved reservation.";
                    return RedirectToAction(nameof(Manage), new { filterStatus = ReservationStatus.Pending });
                }
            }

            reservation.Status = status;
            _context.Update(reservation);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Reservation status updated to {status}.";
            return RedirectToAction(nameof(Manage), new { filterStatus = ReservationStatus.Pending });
        }
    }
} 