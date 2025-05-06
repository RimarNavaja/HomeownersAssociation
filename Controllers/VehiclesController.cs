using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace HomeownersAssociation.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehiclesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var vehicles = await _context.Vehicles
                .Where(v => v.OwnerId == userId)
                .OrderBy(v => v.RegistrationDate)
                .ToListAsync();
            
            return View(vehicles);
        }
        
        // GET: Vehicles/Manage
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Manage()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Owner)
                .OrderBy(v => v.Owner.LastName)
                .ThenBy(v => v.Owner.FirstName)
                .ToListAsync();
            
            return View(vehicles);
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (vehicle == null)
            {
                return NotFound();
            }

            // Security check: Only admin, staff, or the vehicle owner can view details
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && vehicle.OwnerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            var viewModel = new VehicleViewModel
            {
                OwnerId = _userManager.GetUserId(User),
                VehicleTypes = GetVehicleTypesList()
            };
            
            return View(viewModel);
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var vehicle = new Vehicle
                {
                    OwnerId = viewModel.OwnerId,
                    LicensePlate = viewModel.LicensePlate,
                    VehicleType = viewModel.VehicleType,
                    Make = viewModel.Make,
                    Model = viewModel.Model,
                    Year = viewModel.Year,
                    Color = viewModel.Color,
                    RfidTag = viewModel.RfidTag,
                    IsActive = viewModel.IsActive,
                    Notes = viewModel.Notes,
                    RegistrationDate = DateTime.Now
                };
                
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Vehicle has been registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            viewModel.VehicleTypes = GetVehicleTypesList();
            return View(viewModel);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (vehicle == null)
            {
                return NotFound();
            }

            // Security check: Only admin, staff, or the vehicle owner can edit
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && vehicle.OwnerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            var viewModel = new VehicleViewModel
            {
                Id = vehicle.Id,
                OwnerId = vehicle.OwnerId,
                OwnerName = vehicle.Owner?.UserName,
                LicensePlate = vehicle.LicensePlate,
                VehicleType = vehicle.VehicleType,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                RfidTag = vehicle.RfidTag,
                IsActive = vehicle.IsActive,
                Notes = vehicle.Notes,
                VehicleTypes = GetVehicleTypesList()
            };
            
            return View(viewModel);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            // Get the vehicle to check ownership
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            // Security check: Only admin, staff, or the vehicle owner can edit
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && vehicle.OwnerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vehicle.LicensePlate = viewModel.LicensePlate;
                    vehicle.VehicleType = viewModel.VehicleType;
                    vehicle.Make = viewModel.Make;
                    vehicle.Model = viewModel.Model;
                    vehicle.Year = viewModel.Year;
                    vehicle.Color = viewModel.Color;
                    vehicle.RfidTag = viewModel.RfidTag;
                    vehicle.IsActive = viewModel.IsActive;
                    vehicle.Notes = viewModel.Notes;
                    
                    // Only admin/staff can change vehicle owner
                    if ((User.IsInRole("Admin") || User.IsInRole("Staff")) && viewModel.OwnerId != vehicle.OwnerId)
                    {
                        vehicle.OwnerId = viewModel.OwnerId;
                    }
                    
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Vehicle information has been updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                if (User.IsInRole("Admin") || User.IsInRole("Staff"))
                {
                    return RedirectToAction(nameof(Manage));
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            viewModel.VehicleTypes = GetVehicleTypesList();
            return View(viewModel);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            
            if (vehicle == null)
            {
                return NotFound();
            }

            // Security check: Only admin, staff, or the vehicle owner can delete
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff") && vehicle.OwnerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }
            
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Vehicle has been deleted successfully.";
            
            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                return RedirectToAction(nameof(Manage));
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Vehicles/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            
            if (vehicle == null)
            {
                return NotFound();
            }
            
            vehicle.IsActive = !vehicle.IsActive;
            _context.Update(vehicle);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Vehicle status has been {(vehicle.IsActive ? "activated" : "deactivated")} successfully.";
            return RedirectToAction(nameof(Manage));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
        
        private IEnumerable<SelectListItem> GetVehicleTypesList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Car", Text = "Car" },
                new SelectListItem { Value = "SUV", Text = "SUV" },
                new SelectListItem { Value = "Pickup", Text = "Pickup Truck" },
                new SelectListItem { Value = "Motorcycle", Text = "Motorcycle" },
                new SelectListItem { Value = "Van", Text = "Van" },
                new SelectListItem { Value = "Truck", Text = "Truck" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };
        }
    }
} 