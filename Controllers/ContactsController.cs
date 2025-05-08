using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using Microsoft.AspNetCore.Authorization;

namespace HomeownersAssociation.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contacts (Public View)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var contacts = await _context.Contacts
                                        .Where(c => c.IsPublic)
                                        .OrderBy(c => c.DisplayOrder)
                                        .ThenBy(c => c.Category)
                                        .ThenBy(c => c.Name)
                                        .ToListAsync();
            return View(contacts);
        }

        // GET: Contacts/Manage (Admin View)
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Manage()
        {
            ViewData["ActiveLink"] = "ManageContacts"; // For highlighting active nav link
            var contacts = await _context.Contacts
                                        .OrderBy(c => c.DisplayOrder)
                                        .ThenBy(c => c.Category)
                                        .ThenBy(c => c.Name)
                                        .ToListAsync();
            return View(contacts);
        }


        // GET: Contacts/Details/5
        // Public users can see details of public contacts, Admins/Staff can see all.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts.FirstOrDefaultAsync(m => m.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            // If contact is not public and user is not Admin/Staff, deny access
            if (!contact.IsPublic && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid(); // Or RedirectToAction("AccessDenied", "Account");
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Create()
        {
             ViewData["ActiveLink"] = "ManageContacts";
            return View();
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([Bind("Id,Name,Category,PhoneNumber,EmailAddress,OfficeHours,Location,Notes,IsPublic,DisplayOrder")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Contact created successfully.";
                return RedirectToAction(nameof(Manage));
            }
             ViewData["ActiveLink"] = "ManageContacts";
            return View(contact);
        }

        // GET: Contacts/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
             ViewData["ActiveLink"] = "ManageContacts";
            return View(contact);
        }

        // POST: Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Category,PhoneNumber,EmailAddress,OfficeHours,Location,Notes,IsPublic,DisplayOrder")] Contact contact)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Contact updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
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
             ViewData["ActiveLink"] = "ManageContacts";
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }
            ViewData["ActiveLink"] = "ManageContacts";
            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Contact deleted successfully.";
            }
            return RedirectToAction(nameof(Manage));
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
} 