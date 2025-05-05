using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Data;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting; // Required for file path access
using System.IO; // Required for file operations

namespace HomeownersAssociation.Controllers
{
    [Authorize] // All logged-in users can potentially view documents
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment; // To get wwwroot path

        public DocumentsController(ApplicationDbContext context, 
                                 UserManager<ApplicationUser> userManager, 
                                 IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Documents - List accessible documents (public or all for admin/staff)
        public async Task<IActionResult> Index()
        {
            IQueryable<Document> documentsQuery = _context.Documents.Include(d => d.UploadedBy);

            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                // Homeowners only see public documents
                documentsQuery = documentsQuery.Where(d => d.IsPublic);
            }

            var documents = await documentsQuery.OrderByDescending(d => d.UploadedAt).ToListAsync();
            return View(documents);
        }

        // GET: Documents/Upload - Admin/Staff only
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Upload()
        {
            // Optionally populate categories here if using a dropdown
            var viewModel = new DocumentUploadViewModel();
            return View(viewModel);
        }

        // POST: Documents/Upload - Admin/Staff only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Upload(DocumentUploadViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = string.Empty;
                if (viewModel.DocumentFile != null)
                {
                    // 1. Define upload path (e.g., wwwroot/uploads/documents)
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "documents");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // 2. Generate unique file name (prevent overwrites)
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(viewModel.DocumentFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // 3. Save the file
                    try
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.DocumentFile.CopyToAsync(fileStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error 
                        ModelState.AddModelError("", "File upload failed. Please try again. Error: " + ex.Message);
                         // Optionally reload categories if using dropdown
                        return View(viewModel);
                    }
                }
                else
                {
                     ModelState.AddModelError("DocumentFile", "File is required.");
                      // Optionally reload categories if using dropdown
                     return View(viewModel);
                }

                // 4. Create Document entity
                var document = new Document
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Category = viewModel.Category,
                    FileUrl = Path.Combine("/uploads", "documents", uniqueFileName), // Store relative URL
                    UploadedById = _userManager.GetUserId(User),
                    UploadedAt = DateTime.Now,
                    IsPublic = viewModel.IsPublic
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Document uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }
             // Optionally reload categories if using dropdown
            return View(viewModel);
        }

        // GET: Documents/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            // Security check: Only allow download if public or user is admin/staff
            if (!document.IsPublic && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid(); // Or Access Denied
            }

            // Construct the physical path
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, document.FileUrl.TrimStart('/'));

            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["ErrorMessage"] = "File not found.";
                return RedirectToAction(nameof(Index));
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(physicalPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // Determine content type (simple example, could use a library for more accuracy)
            var contentType = "application/octet-stream"; // Default
            var fileExtension = Path.GetExtension(physicalPath).ToLowerInvariant();
            // Add more common types as needed
            if (fileExtension == ".pdf") contentType = "application/pdf";
            else if (fileExtension == ".doc") contentType = "application/msword";
            else if (fileExtension == ".docx") contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if (fileExtension == ".xls") contentType = "application/vnd.ms-excel";
            else if (fileExtension == ".xlsx") contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            else if (fileExtension == ".jpg" || fileExtension == ".jpeg") contentType = "image/jpeg";
            else if (fileExtension == ".png") contentType = "image/png";
            else if (fileExtension == ".txt") contentType = "text/plain";

            return File(memory, contentType, Path.GetFileName(physicalPath)); // Provide original filename for download
        }

        // GET: Documents/Delete/5 - Admin/Staff only
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.UploadedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5 - Admin/Staff only
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Delete the physical file first
            try
            {
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, document.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            catch (Exception ex)
            {
                 // Log error - File deletion failed but proceed to remove DB record
                 TempData["ErrorMessage"] = "Could not delete the physical file, but removing database record. Error: " + ex.Message;
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Document deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
} 