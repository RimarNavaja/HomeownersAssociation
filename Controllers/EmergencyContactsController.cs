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
    [Authorize(Roles = "Admin,Staff")]
    public class EmergencyContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmergencyContactsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: EmergencyContacts
        public async Task<IActionResult> Index()
        {
            var contacts = await _context.EmergencyContacts
                .Include(ec => ec.CreatedBy)
                .OrderBy(ec => ec.PriorityOrder)
                .ThenBy(ec => ec.Name)
                .ToListAsync();
            
            return View(contacts);
        }

        // GET: EmergencyContacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.EmergencyContacts
                .Include(ec => ec.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: EmergencyContacts/Create
        public IActionResult Create()
        {
            var viewModel = new EmergencyContactViewModel
            {
                IsActive = true,
                ContactTypes = GetContactTypesList()
            };
            
            return View(viewModel);
        }

        // POST: EmergencyContacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmergencyContactViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var contact = new EmergencyContact
                {
                    Name = viewModel.Name,
                    Organization = viewModel.Organization,
                    ContactType = viewModel.ContactType,
                    PhoneNumber = viewModel.PhoneNumber,
                    AlternativePhone = viewModel.AlternativePhone,
                    Email = viewModel.Email,
                    Address = viewModel.Address,
                    Description = viewModel.Description,
                    IsAvailable24x7 = viewModel.IsAvailable24x7,
                    OperatingHours = viewModel.OperatingHours,
                    PriorityOrder = viewModel.PriorityOrder,
                    IsActive = viewModel.IsActive,
                    CreatedById = _userManager.GetUserId(User),
                    CreatedAt = DateTime.Now
                };
                
                _context.Add(contact);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Emergency contact has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            viewModel.ContactTypes = GetContactTypesList();
            return View(viewModel);
        }

        // GET: EmergencyContacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.EmergencyContacts
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (contact == null)
            {
                return NotFound();
            }

            var viewModel = new EmergencyContactViewModel
            {
                Id = contact.Id,
                Name = contact.Name,
                Organization = contact.Organization,
                ContactType = contact.ContactType,
                PhoneNumber = contact.PhoneNumber,
                AlternativePhone = contact.AlternativePhone,
                Email = contact.Email,
                Address = contact.Address,
                Description = contact.Description,
                IsAvailable24x7 = contact.IsAvailable24x7,
                OperatingHours = contact.OperatingHours,
                PriorityOrder = contact.PriorityOrder,
                IsActive = contact.IsActive,
                ContactTypes = GetContactTypesList()
            };
            
            return View(viewModel);
        }

        // POST: EmergencyContacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmergencyContactViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var contact = await _context.EmergencyContacts.FindAsync(id);
                    
                    if (contact == null)
                    {
                        return NotFound();
                    }
                    
                    contact.Name = viewModel.Name;
                    contact.Organization = viewModel.Organization;
                    contact.ContactType = viewModel.ContactType;
                    contact.PhoneNumber = viewModel.PhoneNumber;
                    contact.AlternativePhone = viewModel.AlternativePhone;
                    contact.Email = viewModel.Email;
                    contact.Address = viewModel.Address;
                    contact.Description = viewModel.Description;
                    contact.IsAvailable24x7 = viewModel.IsAvailable24x7;
                    contact.OperatingHours = viewModel.OperatingHours;
                    contact.PriorityOrder = viewModel.PriorityOrder;
                    contact.IsActive = viewModel.IsActive;
                    contact.UpdatedAt = DateTime.Now;
                    
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Emergency contact has been updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmergencyContactExists(viewModel.Id))
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
            
            viewModel.ContactTypes = GetContactTypesList();
            return View(viewModel);
        }

        // POST: EmergencyContacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.EmergencyContacts.FindAsync(id);
            
            if (contact == null)
            {
                return NotFound();
            }
            
            _context.EmergencyContacts.Remove(contact);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Emergency contact has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: EmergencyContacts/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var contact = await _context.EmergencyContacts.FindAsync(id);
            
            if (contact == null)
            {
                return NotFound();
            }
            
            contact.IsActive = !contact.IsActive;
            contact.UpdatedAt = DateTime.Now;
            _context.Update(contact);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Emergency contact status has been {(contact.IsActive ? "activated" : "deactivated")} successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool EmergencyContactExists(int id)
        {
            return _context.EmergencyContacts.Any(e => e.Id == id);
        }
        
        private IEnumerable<SelectListItem> GetContactTypesList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Fire", Text = "Fire Department" },
                new SelectListItem { Value = "Police", Text = "Police" },
                new SelectListItem { Value = "Medical", Text = "Medical/Hospital" },
                new SelectListItem { Value = "Association", Text = "Homeowners Association" },
                new SelectListItem { Value = "Maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "Security", Text = "Security" },
                new SelectListItem { Value = "Utility", Text = "Utility Services" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };
        }
    }
} 