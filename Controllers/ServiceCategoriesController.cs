using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace HomeownersAssociation.Controllers
{
    [Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can manage categories
    public class ServiceCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceCategories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.ServiceCategories.ToListAsync();
            return View(categories);
        }

        // GET: ServiceCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ServiceCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var category = new ServiceCategory
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    IsActive = viewModel.IsActive
                };
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: ServiceCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.ServiceCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new ServiceCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };
            return View(viewModel);
        }

        // POST: ServiceCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceCategoryViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.ServiceCategories.FindAsync(id);
                    if (category == null) 
                    {
                        return NotFound();
                    }

                    category.Name = viewModel.Name;
                    category.Description = viewModel.Description;
                    category.IsActive = viewModel.IsActive;
                    
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Service category updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceCategoryExists(viewModel.Id))
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

        // GET: ServiceCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.ServiceCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if category is in use before allowing delete
            bool isInUse = await _context.ServiceRequests.AnyAsync(sr => sr.CategoryId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Cannot delete category: It is currently associated with existing service requests.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: ServiceCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.ServiceCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            
             // Re-check if category is in use just before deleting
            bool isInUse = await _context.ServiceRequests.AnyAsync(sr => sr.CategoryId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Cannot delete category: It is currently associated with existing service requests.";
                return RedirectToAction(nameof(Index));
            }

            _context.ServiceCategories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Service category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceCategoryExists(int id)
        {
            return _context.ServiceCategories.Any(e => e.Id == id);
        }
    }
} 