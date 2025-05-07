using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;

namespace HomeownersAssociation.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Events/Calendar
        public IActionResult Calendar()
        {
            return View();
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.CreatedBy)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(events);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Create()
        {
            var viewModel = new EventViewModel
            {
                StartDateTime = DateTime.Now.Date.AddHours(9), // 9 AM today
                EndDateTime = DateTime.Now.Date.AddHours(10),  // 10 AM today
                IsActive = true,
                EventType = "Community", // Set a default event type
                Color = "#007bff" // Default bootstrap primary color
            };

            ViewData["EventTypes"] = new List<string> { "Community", "Maintenance", "Meeting", "Holiday", "Other" };
            return View(viewModel);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            // For debugging: check if the model is coming through correctly
            if (model == null)
            {
                TempData["ErrorMessage"] = "Model is null";
                ViewData["EventTypes"] = new List<string> { "Community", "Maintenance", "Meeting", "Holiday", "Other" };
                return View(new EventViewModel
                {
                    StartDateTime = DateTime.Now.Date.AddHours(9),
                    EndDateTime = DateTime.Now.Date.AddHours(10),
                    IsActive = true,
                    Color = "#007bff"
                });
            }

            // Explicitly remove validation errors for CreatedById and CreatedByName
            ModelState.Remove("CreatedById");
            ModelState.Remove("CreatedByName");

            // Check model state
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join("; ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage));
                TempData["ErrorMessage"] = $"Validation errors: {errorMessages}";
                
                ViewData["EventTypes"] = new List<string> { "Community", "Maintenance", "Meeting", "Holiday", "Other" };
                return View(model);
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Current user not found";
                    ViewData["EventTypes"] = new List<string> { "Community", "Maintenance", "Meeting", "Holiday", "Other" };
                    return View(model);
                }

                // Don't use the CreatedById and CreatedByName from the model
                // Instead, use the current user's information
                var @event = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDateTime = model.StartDateTime,
                    EndDateTime = model.EndDateTime,
                    Location = model.Location,
                    EventType = model.EventType,
                    IsActive = model.IsActive,
                    IsAllDay = model.IsAllDay,
                    Color = model.Color,
                    CreatedById = currentUser.Id, // Set from current user
                    CreatedAt = DateTime.Now
                };

                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating event: {ex.Message}";
                ViewData["EventTypes"] = new List<string> { "Community", "Maintenance", "Meeting", "Holiday", "Other" };
                return View(model);
            }
        }

        // GET: Events/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            var viewModel = new EventViewModel
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartDateTime = @event.StartDateTime,
                EndDateTime = @event.EndDateTime,
                Location = @event.Location,
                EventType = @event.EventType,
                IsActive = @event.IsActive,
                IsAllDay = @event.IsAllDay,
                Color = @event.Color,
                CreatedById = @event.CreatedById,
                CreatedByName = @event.CreatedBy?.UserName
            };

            ViewData["EventTypes"] = new List<string> { "Community", "Maintenance", "Meeting", "Holiday", "Other" };
            return View(viewModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, EventViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var @event = await _context.Events.FindAsync(id);
                    if (@event == null)
                    {
                        return NotFound();
                    }

                    @event.Title = model.Title;
                    @event.Description = model.Description;
                    @event.StartDateTime = model.StartDateTime;
                    @event.EndDateTime = model.EndDateTime;
                    @event.Location = model.Location;
                    @event.EventType = model.EventType;
                    @event.IsActive = model.IsActive;
                    @event.IsAllDay = model.IsAllDay;
                    @event.Color = model.Color;

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(id))
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

            ViewData["EventTypes"] = new List<string> { "Community", "Maintenance", "Meeting", "Holiday", "Other" };
            return View(model);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Events/GetEvents
        [HttpGet]
        public async Task<JsonResult> GetEvents()
        {
            var events = await _context.Events
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = e.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    description = e.Description,
                    location = e.Location,
                    allDay = e.IsAllDay,
                    color = e.Color ?? "#007bff",
                    eventType = e.EventType
                })
                .ToListAsync();

            return Json(events);
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }

        // POST: Events/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            @event.IsActive = !@event.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = @event.IsActive 
                ? "Event activated successfully!" 
                : "Event deactivated successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
} 