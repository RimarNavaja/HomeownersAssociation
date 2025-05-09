using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HomeownersAssociation.Models;
using HomeownersAssociation.Models.ViewModels;
using HomeownersAssociation.Data;

namespace HomeownersAssociation.Controllers
{
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BillingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Billing - Admin Dashboard
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var bills = await _context.Bills
                .Include(b => b.Homeowner)
                .OrderByDescending(b => b.IssueDate)
                .ToListAsync();

            // Get statistics
            var stats = new
            {
                TotalBills = bills.Count,
                PaidBills = bills.Count(b => b.Status == BillStatus.Paid),
                UnpaidBills = bills.Count(b => b.Status == BillStatus.Unpaid),
                OverdueBills = bills.Count(b => b.Status == BillStatus.Overdue),
                TotalAmount = bills.Sum(b => b.Amount),
                PaidAmount = bills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.Amount),
                UnpaidAmount = bills.Where(b => b.Status == BillStatus.Unpaid || b.Status == BillStatus.Overdue).Sum(b => b.Amount),
                OverdueAmount = bills.Where(b => b.Status == BillStatus.Overdue).Sum(b => b.Amount)
            };

            ViewBag.Stats = stats;

            return View(bills);
        }

        // GET: Billing/MyBills - Homeowner View
        [Authorize]
        public async Task<IActionResult> MyBills()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Get both personal bills (HomeownerId matches user) AND global bills (HomeownerId is null)
            var bills = await _context.Bills
                .Where(b => b.HomeownerId == user.Id || b.HomeownerId == null)
                .Include(b => b.Homeowner) // Include homeowner info (will be null for global bills)
                .OrderByDescending(b => b.IssueDate)
                .ToListAsync();

            // Get statistics
            var stats = new
            {
                TotalBills = bills.Count,
                PaidBills = bills.Count(b => b.Status == BillStatus.Paid),
                UnpaidBills = bills.Count(b => b.Status == BillStatus.Unpaid),
                OverdueBills = bills.Count(b => b.Status == BillStatus.Overdue),
                TotalAmount = bills.Sum(b => b.Amount),
                PaidAmount = bills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.Amount),
                UnpaidAmount = bills.Where(b => b.Status == BillStatus.Unpaid || b.Status == BillStatus.Overdue).Sum(b => b.Amount),
                OverdueAmount = bills.Where(b => b.Status == BillStatus.Overdue).Sum(b => b.Amount)
            };

            ViewBag.Stats = stats;

            return View(bills);
        }

        // GET: Billing/Create
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create()
        {
            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");

            var model = new BillViewModel
            {
                Homeowners = homeowners,
                BillNumber = GenerateBillNumber(),
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                IsGlobal = false
            };

            return View(model);
        }

        // POST: Billing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create(BillViewModel model)
        {
            // Manual validation for HomeownerId if IsGlobal is false
            if (!model.IsGlobal && string.IsNullOrEmpty(model.HomeownerId))
            {
                ModelState.AddModelError("HomeownerId", "The Homeowner field is required for non-global bills.");
            }
            else if (model.IsGlobal)
            {
                // Ensure HomeownerId errors are removed if it's global
                ModelState.Remove("HomeownerId"); 
            }

            if (ModelState.IsValid)
            {
                var bill = new Bill
                {
                    BillNumber = model.BillNumber,
                    Description = model.Description,
                    Amount = model.Amount,
                    DueDate = model.DueDate,
                    IssueDate = model.IssueDate,
                    Status = model.Status,
                    Type = model.Type,
                    Notes = model.Notes
                };

                if (model.IsGlobal)
                {
                    bill.HomeownerId = null; // Set to null for global bills
                    bill.Notes = (bill.Notes ?? "") + " (Global Bill)"; 
                }
                else
                {
                    bill.HomeownerId = model.HomeownerId;
                }

                _context.Add(bill);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = model.IsGlobal ? 
                    "Global bill created successfully!" : 
                    "Bill created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");
            model.Homeowners = homeowners;

            return View(model);
        }

        // GET: Billing/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");
            
            // Determine if the bill is currently global (HomeownerId is null)
            bool isCurrentlyGlobal = bill.HomeownerId == null;

            var model = new BillViewModel
            {
                Id = bill.Id,
                BillNumber = bill.BillNumber,
                Description = bill.Description,
                Amount = bill.Amount,
                DueDate = bill.DueDate,
                IssueDate = bill.IssueDate,
                Status = bill.Status,
                HomeownerId = bill.HomeownerId, // Pre-select if not global
                Type = bill.Type,
                Notes = bill.Notes?.Replace(" (Global Bill)", ""), // Clean notes for editing
                Homeowners = homeowners,
                IsGlobal = isCurrentlyGlobal
            };

            return View(model);
        }

        // POST: Billing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, BillViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Manual validation for HomeownerId if IsGlobal is false
            if (!model.IsGlobal && string.IsNullOrEmpty(model.HomeownerId))
            {
                ModelState.AddModelError("HomeownerId", "The Homeowner field is required for non-global bills.");
            }
            else if (model.IsGlobal)
            {
                // Ensure HomeownerId errors are removed if it's global
                ModelState.Remove("HomeownerId");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var bill = await _context.Bills.FindAsync(id);
                    if (bill == null)
                    {
                        return NotFound();
                    }

                    bill.BillNumber = model.BillNumber;
                    bill.Description = model.Description;
                    bill.Amount = model.Amount;
                    bill.DueDate = model.DueDate;
                    bill.IssueDate = model.IssueDate;
                    bill.Status = model.Status;
                    bill.Type = model.Type;
                    bill.Notes = model.Notes;

                    // Handle HomeownerId and Notes based on IsGlobal
                    if (model.IsGlobal)
                    {
                        bill.HomeownerId = null; // Set to null for global bills
                        bill.Notes = (bill.Notes ?? "") + " (Global Bill)"; 
                    }
                    else
                    {
                        // Assign the selected homeowner for non-global bills
                        bill.HomeownerId = model.HomeownerId;
                        // Ensure global bill note is removed if switching back
                        bill.Notes = bill.Notes?.Replace(" (Global Bill)", "");
                    }

                    _context.Update(bill);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Bill updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillExists(model.Id))
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

            // If we got this far, something failed, redisplay form
            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");
            model.Homeowners = homeowners;

            return View(model);
        }

        // GET: Billing/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Homeowner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // POST: Billing/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            // Check if there are any payments for this bill
            var hasPayments = await _context.Payments.AnyAsync(p => p.BillId == id);
            if (hasPayments)
            {
                TempData["ErrorMessage"] = "Cannot delete bill with existing payments.";
                return RedirectToAction(nameof(Index));
            }

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bill deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Billing/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Homeowner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bill == null)
            {
                return NotFound();
            }

            // Get payments for this bill
            var payments = await _context.Payments
                .Where(p => p.BillId == id)
                .Include(p => p.ProcessedBy)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.Payments = payments;

            // Calculate the remaining amount due
            decimal totalPaid = payments.Sum(p => p.AmountPaid);
            ViewBag.AmountPaid = totalPaid;
            ViewBag.AmountDue = bill.Amount - totalPaid;

            return View(bill);
        }

        // GET: Billing/PayBill/5
        [Authorize]
        public async Task<IActionResult> PayBill(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Homeowner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (bill == null)
            {
                return NotFound();
            }

            // Check if user is the homeowner or admin/staff
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (bill.HomeownerId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid();
            }

            // Get payments for this bill
            var payments = await _context.Payments
                .Where(p => p.BillId == id)
                .ToListAsync();

            // Calculate the remaining amount due
            decimal totalPaid = payments.Sum(p => p.AmountPaid);
            decimal amountDue = bill.Amount - totalPaid;

            if (amountDue <= 0)
            {
                TempData["ErrorMessage"] = "This bill has already been fully paid.";
                return RedirectToAction(User.IsInRole("Admin") || User.IsInRole("Staff") ? "Index" : "MyBills");
            }

            var model = new BillPaymentViewModel
            {
                BillId = bill.Id,
                BillNumber = bill.BillNumber,
                Description = bill.Description,
                TotalAmount = bill.Amount,
                AmountDue = amountDue,
                AmountToPay = amountDue // Default to full payment
            };

            return View(model);
        }

        // POST: Billing/PayBill
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> PayBill(BillPaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var bill = await _context.Bills.FindAsync(model.BillId);
                if (bill == null)
                {
                    return NotFound();
                }

                // Check if user is the homeowner or admin/staff
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                if (bill.HomeownerId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
                {
                    return Forbid();
                }

                // Calculate the remaining amount due
                var payments = await _context.Payments
                    .Where(p => p.BillId == model.BillId)
                    .ToListAsync();
                decimal totalPaid = payments.Sum(p => p.AmountPaid);
                decimal amountDue = bill.Amount - totalPaid;

                if (amountDue <= 0)
                {
                    TempData["ErrorMessage"] = "This bill has already been fully paid.";
                    return RedirectToAction(User.IsInRole("Admin") || User.IsInRole("Staff") ? "Details" : "MyBills");
                }

                if (model.AmountToPay > amountDue)
                {
                    ModelState.AddModelError("AmountToPay", "Payment amount cannot exceed the amount due.");
                    model.AmountDue = amountDue;
                    return View(model);
                }

                // Create payment record
                var payment = new Payment
                {
                    PaymentNumber = GeneratePaymentNumber(),
                    AmountPaid = model.AmountToPay,
                    PaymentDate = DateTime.Now,
                    BillId = model.BillId,
                    HomeownerId = bill.HomeownerId,
                    PaymentMethod = model.PaymentMethod,
                    ReferenceNumber = model.ReferenceNumber,
                    Notes = model.Notes,
                    ProcessedById = User.IsInRole("Admin") || User.IsInRole("Staff") ? user.Id : null
                };

                _context.Add(payment);

                // Update bill status
                if (model.AmountToPay >= amountDue)
                {
                    bill.Status = BillStatus.Paid;
                    bill.PaymentDate = DateTime.Now;
                    bill.PaymentMethod = model.PaymentMethod;
                    bill.PaymentReference = model.ReferenceNumber;
                }
                else
                {
                    bill.Status = BillStatus.PartiallyPaid;
                }

                _context.Update(bill);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Payment recorded successfully!";
                string actionName = User.IsInRole("Admin") || User.IsInRole("Staff") ? "Details" : "MyBills";
                return RedirectToAction(actionName, new { id = model.BillId });
            }

            return View(model);
        }

        // Helper method to generate bill number
        private string GenerateBillNumber()
        {
            string prefix = "BILL";
            string dateCode = DateTime.Now.ToString("yyMMdd");

            // Get the last bill number with this prefix and date
            var lastBill = _context.Bills
                .Where(b => b.BillNumber.StartsWith(prefix + dateCode))
                .OrderByDescending(b => b.BillNumber)
                .FirstOrDefault();

            int sequence = 1;

            if (lastBill != null)
            {
                string sequenceStr = lastBill.BillNumber.Substring((prefix + dateCode).Length);
                if (int.TryParse(sequenceStr, out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }

            return $"{prefix}{dateCode}{sequence:D4}";
        }

        // Helper method to generate payment number
        private string GeneratePaymentNumber()
        {
            string prefix = "PAY";
            string dateCode = DateTime.Now.ToString("yyMMdd");

            // Get the last payment number with this prefix and date
            var lastPayment = _context.Payments
                .Where(p => p.PaymentNumber.StartsWith(prefix + dateCode))
                .OrderByDescending(p => p.PaymentNumber)
                .FirstOrDefault();

            int sequence = 1;

            if (lastPayment != null)
            {
                string sequenceStr = lastPayment.PaymentNumber.Substring((prefix + dateCode).Length);
                if (int.TryParse(sequenceStr, out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }

            return $"{prefix}{dateCode}{sequence:D4}";
        }

        // Helper method to check if bill exists
        private bool BillExists(int id)
        {
            return _context.Bills.Any(e => e.Id == id);
        }

        // Batch bill generation for monthly dues
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateMonthlyBills()
        {
            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");
            ViewBag.HomeownerCount = homeowners.Count;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateMonthlyBills(decimal amount, string description, DateTime dueDate)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than 0");
                var homeownersList = await _userManager.GetUsersInRoleAsync("Homeowner");
                ViewBag.HomeownerCount = homeownersList.Count;
                return View();
            }

            var homeowners = await _userManager.GetUsersInRoleAsync("Homeowner");

            int billsCreated = 0;
            foreach (var homeowner in homeowners)
            {
                // Check if homeowner already has a monthly bill for this month
                bool hasMonthlyBill = await _context.Bills
                    .AnyAsync(b => b.HomeownerId == homeowner.Id &&
                                b.Type == BillType.MonthlyDues &&
                                b.IssueDate.Month == DateTime.Now.Month &&
                                b.IssueDate.Year == DateTime.Now.Year);

                if (!hasMonthlyBill)
                {
                    var bill = new Bill
                    {
                        BillNumber = GenerateBillNumber(),
                        Description = string.IsNullOrEmpty(description) ? $"Monthly Dues - {DateTime.Now:MMMM yyyy}" : description,
                        Amount = amount,
                        DueDate = dueDate,
                        IssueDate = DateTime.Now,
                        Status = BillStatus.Unpaid,
                        HomeownerId = homeowner.Id,
                        Type = BillType.MonthlyDues
                    };

                    _context.Add(bill);
                    billsCreated++;
                }
            }

            if (billsCreated > 0)
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully generated {billsCreated} monthly dues bills.";
            }
            else
            {
                TempData["InfoMessage"] = "No new bills were generated. All homeowners already have monthly bills for this month.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Billing/Payments
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Payments()
        {
            var payments = await _context.Payments
                .Include(p => p.Bill)
                .Include(p => p.Homeowner)
                .Include(p => p.ProcessedBy)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        // GET: Billing/MyPayments
        [Authorize]
        public async Task<IActionResult> MyPayments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var payments = await _context.Payments
                .Include(p => p.Bill)
                .Where(p => p.HomeownerId == user.Id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }
    }
}