using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using Microsoft.AspNetCore.Authorization;

namespace HomeownersAssociation.Controllers
{
    [Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can manage forum categories
    public class ForumCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForumCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ForumCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.ForumCategories.ToListAsync());
        }

        // GET: ForumCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ForumCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsActive")] ForumCategory forumCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(forumCategory);
                await _context.SaveChangesAsync();
                 TempData["SuccessMessage"] = "Forum category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(forumCategory);
        }

        // GET: ForumCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories.FindAsync(id);
            if (forumCategory == null)
            {
                return NotFound();
            }
            return View(forumCategory);
        }

        // POST: ForumCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive")] ForumCategory forumCategory)
        {
            if (id != forumCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(forumCategory);
                    await _context.SaveChangesAsync();
                     TempData["SuccessMessage"] = "Forum category updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ForumCategoryExists(forumCategory.Id))
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
            return View(forumCategory);
        }

        // GET: ForumCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return NotFound();
            }

             // Check if category is in use before allowing delete
            bool isInUse = await _context.ForumThreads.AnyAsync(ft => ft.CategoryId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Cannot delete category: It is currently associated with existing forum threads.";
                return RedirectToAction(nameof(Index));
            }

            return View(forumCategory);
        }

        // POST: ForumCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var forumCategory = await _context.ForumCategories.FindAsync(id);
             if (forumCategory == null)
            {
                return NotFound();
            }

             // Re-check if category is in use just before deleting
            bool isInUse = await _context.ForumThreads.AnyAsync(ft => ft.CategoryId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Cannot delete category: It is currently associated with existing forum threads.";
                return RedirectToAction(nameof(Index));
            }

            _context.ForumCategories.Remove(forumCategory);
            await _context.SaveChangesAsync();
             TempData["SuccessMessage"] = "Forum category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool ForumCategoryExists(int id)
        {
            return _context.ForumCategories.Any(e => e.Id == id);
        }
    }
} 