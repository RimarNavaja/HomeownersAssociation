using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HomeownersAssociation.Data
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roleNames = { "Admin", "Homeowner", "Staff" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@homeowners.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail, Email = adminEmail, EmailConfirmed = true,
                    FirstName = "System", LastName = "Administrator", Address = "Admin Office",
                    LotNumber = "N/A", BlockNumber = "N/A",
                    RegistrationDate = DateTime.UtcNow.AddMonths(-13),
                    IsApproved = true, UserType = UserType.Admin, IsActive = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
            }
            await SeedDashboardDataAsync(userManager, context);
        }

        public static async Task SeedDashboardDataAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            // --- Seed Users (Homeowners & Staff) ---
            var adminUser = await userManager.FindByEmailAsync("admin@homeowners.com");
            var staffUser = await userManager.FindByEmailAsync("staff@homeowners.com");

            if (staffUser == null) // Ensure staff user is created if previous logic missed it
            {
                staffUser = new ApplicationUser
                {
                    UserName = "staff@homeowners.com", Email = "staff@homeowners.com", EmailConfirmed = true,
                    FirstName = "Sarah", LastName = "Stafford", Address = "Staff Quarters", LotNumber = "S1", BlockNumber = "A",
                    RegistrationDate = DateTime.UtcNow.AddMonths(-6), IsApproved = true, UserType = UserType.Staff, IsActive = true
                };
                var staffResult = await userManager.CreateAsync(staffUser, "Staff@123");
                if (staffResult.Succeeded) await userManager.AddToRoleAsync(staffUser, "Staff");
            }

            var approvedHomeowners = await userManager.Users.Where(u => u.UserType == UserType.Homeowner && u.IsApproved).ToListAsync();
            if (!approvedHomeowners.Any() || approvedHomeowners.Count < 10) // Ensure enough homeowners for subsequent seeding
            {
                 if (!await userManager.Users.AnyAsync(u => u.Email != null && u.Email.StartsWith("user")))
                {
                    for (int i = 1; i <= 15; i++)
                    {
                        var approvedUser = new ApplicationUser
                        {
                            UserName = $"user{i}@homeowners.com", Email = $"user{i}@homeowners.com", EmailConfirmed = true,
                            FirstName = $"UserF{i}", LastName = $"UserL{i}", Address = $"Unit {i}", LotNumber = $"L{i}", BlockNumber = i % 2 == 0 ? "A" : "C",
                            RegistrationDate = DateTime.UtcNow.AddMonths(- (i % 12 == 0 ? 1 : i % 12) ).AddDays(-i),
                            IsApproved = true, UserType = UserType.Homeowner, IsActive = (i % 5 != 0)
                        };
                        var approvedResult = await userManager.CreateAsync(approvedUser, "User@123");
                        if (approvedResult.Succeeded) await userManager.AddToRoleAsync(approvedUser, "Homeowner");
                    }
                    approvedHomeowners = await userManager.Users.Where(u => u.UserType == UserType.Homeowner && u.IsApproved).ToListAsync();
                }
            }
            // Ensure we have at least one homeowner for FK constraints if the list is somehow still empty
            if (!approvedHomeowners.Any() && adminUser != null) approvedHomeowners.Add(adminUser); 


            // --- Seed Announcements ---
            if (!await context.Announcements.AnyAsync() && adminUser != null)
            {
                var announcements = new List<Announcement>();
                for(int i = 1; i <= 5; i++)
                {
                    announcements.Add(new Announcement
                    {
                        Title = $"Sample Announcement {i}",
                        Content = $"This is the detailed content for sample announcement number {i}. It contains important information for all residents.",
                        DatePosted = DateTime.UtcNow.AddDays(-i*3),
                        ExpiryDate = DateTime.UtcNow.AddDays(30 - i*3),
                        Priority = (AnnouncementPriority)(i % Enum.GetValues(typeof(AnnouncementPriority)).Length),
                        IsPublic = (i % 2 == 0),
                        IsActive = true,
                        AuthorId = adminUser.Id
                    });
                }
                await context.Announcements.AddRangeAsync(announcements);
            }

            // --- Seed Bills & Payments ---
            if (await context.Bills.CountAsync() < 60) // Modified condition to seed if bill count is low
            {
                var billList = new List<Bill>();
                var paymentList = new List<Payment>();
                
                var billTypeValues = Enum.GetValues(typeof(Models.BillType)).Cast<Models.BillType>().ToList();

                int billCounter = 0;
                if (approvedHomeowners.Any() && billTypeValues.Any()) // Ensure we have homeowners and bill types to proceed
                {
                    foreach (var owner in approvedHomeowners)
                    {
                        for (int i = 0; i < 6; i++) // Increased from 4 to 6 bills per owner
                        {
                            var dueDate = DateTime.UtcNow.AddMonths(i - 3).AddDays(billCounter % 15); // Adjusted month offset for more spread
                            var currentBillType = billTypeValues[billCounter % billTypeValues.Count];
                            var billAmount = (currentBillType == Models.BillType.MonthlyDues ? 1000m : 500m) + (billCounter * 10m) + (i * 50m);

                            var bill = new Bill
                            {
                                BillNumber = $"BILL-{DateTime.UtcNow.Year}{(DateTime.UtcNow.Month):D2}-{owner.Id.ToString().Substring(0, Math.Min(5, owner.Id.Length))}-{billCounter + 1000}",
                                HomeownerId = owner.Id,
                                Type = currentBillType,
                                Amount = billAmount,
                                DueDate = dueDate,
                                IssueDate = dueDate.AddDays(-15),
                                Description = $"{currentBillType.ToString().Replace("_"," ")} for {dueDate:MMMM yyyy}",
                                Status = Models.BillStatus.Unpaid // Default to Unpaid
                            };
                            
                            // Modify conditions for bill statuses and payments
                            if (i == 0 && billCounter % 2 == 0) { // Some overdue
                                bill.Status = Models.BillStatus.Overdue; 
                                bill.DueDate = DateTime.UtcNow.AddDays(-10 - (billCounter % 5)); // Vary overdue dates
                            } else if (i == 1 || i == 3) { // Mark 2nd and 4th bill as Paid for each user
                                bill.Status = Models.BillStatus.Paid;
                                bill.PaymentDate = bill.DueDate.AddDays(-(billCounter % 3 + 1)); // Vary payment date slightly before due date
                                bill.PaymentMethod = (Models.PaymentMethod)(billCounter % Enum.GetValues(typeof(Models.PaymentMethod)).Length); // Cycle through payment methods
                                bill.PaymentReference = $"PAYREF-{Guid.NewGuid().ToString().Substring(0,10).ToUpper()}";
                                
                                paymentList.Add(new Payment { 
                                    PaymentNumber = $"PAYNUM-{DateTime.UtcNow.Year}{(DateTime.UtcNow.Month):D2}-{owner.Id.ToString().Substring(0, Math.Min(5, owner.Id.Length))}-{billCounter + 2000}",
                                    // BillId will be set after bills are saved
                                    AmountPaid = bill.Amount,
                                    PaymentDate = bill.PaymentDate.Value,
                                    PaymentMethod = bill.PaymentMethod.Value,
                                    ReferenceNumber = bill.PaymentReference,
                                    HomeownerId = owner.Id, 
                                    UserId = owner.Id, 
                                    ProcessedById = staffUser?.Id,
                                    TempBillNumberForMatching = bill.BillNumber // Store BillNumber for matching after save
                                });
                            } else if (i == 5 && billCounter % 4 == 0) { // Some cancelled
                                bill.Status = Models.BillStatus.Cancelled;
                            }
                            // Others will remain Unpaid by default

                            billList.Add(bill);
                            billCounter++;
                        }
                    }
                    await context.Bills.AddRangeAsync(billList);
                    await context.SaveChangesAsync(); // Save bills first to get their IDs

                    // Update BillId for payments using the TempBillNumberForMatching and save payments
                    if (paymentList.Any())
                    {
                        foreach(var payment in paymentList)
                        {
                            var relatedBill = await context.Bills.FirstOrDefaultAsync(b => b.BillNumber == payment.TempBillNumberForMatching);
                            if (relatedBill != null) 
                            {
                                payment.BillId = relatedBill.Id;
                            }
                        }
                        await context.Payments.AddRangeAsync(paymentList.Where(p => p.BillId != 0));
                        await context.SaveChangesAsync(); // Save payments
                    }
                }
            }
            
            // --- Seed Service Categories ---
            if (await context.ServiceCategories.CountAsync() < 3) // MODIFIED condition
            { 
                context.ServiceCategories.AddRange(
                    new ServiceCategory { Name = "Plumbing", Description = "Fix leaks, pipes, etc." },
                    new ServiceCategory { Name = "Electrical", Description = "Wiring, lights, power issues, etc." },
                    new ServiceCategory { Name = "Landscaping", Description = "Garden, lawn, tree maintenance." },
                    new ServiceCategory { Name = "Pest Control", Description = "Insect and rodent control services." },
                    new ServiceCategory { Name = "General Maintenance", Description = "Other general repairs and upkeep." }
                );
                await context.SaveChangesAsync(); 
            }
            var serviceCategories = await context.ServiceCategories.ToListAsync();

            // --- Seed Service Requests ---
            if (!await context.ServiceRequests.AnyAsync() && approvedHomeowners.Any() && serviceCategories.Any())
            {
                var requests = new List<ServiceRequest>();
                int srCounter = 0;
                var statusOptions = new List<string> { Models.ServiceRequestStatus.New, Models.ServiceRequestStatus.InProgress };
                var priorityOptions = new List<int> { Models.ServiceRequestPriority.Low, Models.ServiceRequestPriority.Medium, Models.ServiceRequestPriority.High };

                foreach(var owner in approvedHomeowners.Take(5))
                {
                    requests.Add(new ServiceRequest {
                        UserId = owner.Id,
                        CategoryId = serviceCategories[srCounter % serviceCategories.Count].Id,
                        Title = $"Request from {owner.LotNumber}: {serviceCategories[srCounter % serviceCategories.Count].Name} Issue",
                        Description = $"Sample service request {srCounter + 1} for {serviceCategories[srCounter % serviceCategories.Count].Name.ToLower()}. Please attend to an issue in unit {owner.LotNumber}. Further details to be provided upon contact.",
                        Status = statusOptions[srCounter % statusOptions.Count],
                        CreatedAt = DateTime.UtcNow.AddDays(-(srCounter * 2)),
                        Priority = priorityOptions[srCounter % priorityOptions.Count]
                    });
                    srCounter++;
                }
                if(requests.Any()) await context.ServiceRequests.AddRangeAsync(requests);
            }

            // --- Seed Visitor Passes ---
            if (!await context.VisitorPasses.AnyAsync())
            {
                var visitorPasses = new List<VisitorPass>();
                int passCounter = 0;
                if (approvedHomeowners.Any())
                {
                    foreach (var owner in approvedHomeowners)
                    {
                        for (int j = 0; j < 2; j++) // 2 visitor passes per owner
                        {
                            var visitDate = DateTime.UtcNow.Date.AddDays(passCounter % 10); // Spread visit dates
                            visitorPasses.Add(new VisitorPass
                            {
                                RequestedById = owner.Id,
                                VisitorName = $"Visitor {owner.FirstName[0]}{passCounter + 1}",
                                Purpose = $"Social Visit {j + 1}",
                                VisitDate = visitDate,
                                ExpectedTimeIn = visitDate.AddHours(10 + j),
                                ExpectedTimeOut = visitDate.AddHours(14 + j * 2),
                                VehicleDetails = (passCounter % 2 == 0) ? $"Car ABC {100 + passCounter}" : null,
                                Status = (passCounter % 3 == 0) ? "Pending" : "Approved",
                                CreatedAt = DateTime.UtcNow.AddDays(-(passCounter + 1))
                            });
                            passCounter++;
                        }
                    }
                    await context.VisitorPasses.AddRangeAsync(visitorPasses);
                }
            }

            // --- Seed Vehicles ---
            if (!await context.Vehicles.AnyAsync())
            {
                var vehicles = new List<Vehicle>();
                var vehicleTypes = new List<string> { "Sedan", "SUV", "Motorcycle", "Hatchback", "Van" };
                var vehicleMakes = new List<string> { "Toyota", "Honda", "Ford", "Mitsubishi", "Nissan" };
                var vehicleColors = new List<string> { "Red", "Blue", "Black", "White", "Silver", "Gray" };
                int vehicleCounter = 0;
                if (approvedHomeowners.Any())
                {
                    foreach (var owner in approvedHomeowners)
                    {
                        vehicles.Add(new Vehicle
                        {
                            OwnerId = owner.Id,
                            LicensePlate = $"XYZ{700 + vehicleCounter}",
                            VehicleType = vehicleTypes[vehicleCounter % vehicleTypes.Count],
                            Make = vehicleMakes[vehicleCounter % vehicleMakes.Count],
                            Model = $"Model {(char)('A' + vehicleCounter % 5)}{vehicleCounter % 10}",
                            Year = DateTime.UtcNow.Year - (vehicleCounter % 10),
                            Color = vehicleColors[vehicleCounter % vehicleColors.Count],
                            RfidTag = (vehicleCounter % 2 == 0) ? $"RFID{1000 + vehicleCounter}" : null,
                            IsActive = true,
                            RegistrationDate = DateTime.UtcNow.AddMonths(-(vehicleCounter % 6) - 1)
                        });
                        vehicleCounter++;
                        if (vehicleCounter % 3 == 0 && approvedHomeowners.Count > vehicleCounter) // Some owners have two vehicles
                        {
                             vehicles.Add(new Vehicle
                            {
                                OwnerId = owner.Id, // Same owner
                                LicensePlate = $"ABC{300 + vehicleCounter}",
                                VehicleType = vehicleTypes[(vehicleCounter+1) % vehicleTypes.Count],
                                Make = vehicleMakes[(vehicleCounter+1) % vehicleMakes.Count],
                                Model = $"Sport {(char)('S' + vehicleCounter % 3)}{vehicleCounter % 7}",
                                Year = DateTime.UtcNow.Year - ((vehicleCounter+1) % 8),
                                Color = vehicleColors[(vehicleCounter+1) % vehicleColors.Count],
                                IsActive = true,
                                RegistrationDate = DateTime.UtcNow.AddMonths(-(vehicleCounter % 5) - 2)
                            });
                            vehicleCounter++;
                        }
                    }
                    await context.Vehicles.AddRangeAsync(vehicles);
                }
            }

            // --- Seed General Emergency Contacts (HOA Level) ---
            if (!await context.EmergencyContacts.AnyAsync())
            {
                var emergencyContacts = new List<EmergencyContact>
                {
                    new EmergencyContact 
                    {
                        Name = "Local Police Department", Organization = "City Police", ContactType = "Police", 
                        PhoneNumber = "(555) 123-4567", IsAvailable24x7 = true, PriorityOrder = 1, IsActive = true, CreatedById = adminUser?.Id
                    },
                    new EmergencyContact 
                    {
                        Name = "City Fire Station", Organization = "City Fire Dept.", ContactType = "Fire", 
                        PhoneNumber = "(555) 765-4321", IsAvailable24x7 = true, PriorityOrder = 2, IsActive = true, CreatedById = adminUser?.Id
                    },
                    new EmergencyContact 
                    {
                        Name = "General Hospital ER", Organization = "City Hospital", ContactType = "Medical", 
                        PhoneNumber = "(555) 111-2222", IsAvailable24x7 = true, PriorityOrder = 3, IsActive = true, CreatedById = adminUser?.Id
                    },
                    new EmergencyContact 
                    {
                        Name = "HOA Security Office", Organization = "747HOA Security", ContactType = "Security", 
                        PhoneNumber = "(555) 999-0000", OperatingHours = "Mon-Sun, 24hrs (Security Patrol)", IsAvailable24x7 = true, PriorityOrder = 4, IsActive = true, CreatedById = adminUser?.Id
                    },
                     new EmergencyContact 
                    {
                        Name = "HOA Maintenance Desk", Organization = "747HOA Maintenance", ContactType = "Maintenance", 
                        PhoneNumber = "(555) 888-1111", OperatingHours = "Mon-Fri, 9am-5pm", IsAvailable24x7 = false, PriorityOrder = 5, IsActive = true, CreatedById = adminUser?.Id
                    }
                };
                await context.EmergencyContacts.AddRangeAsync(emergencyContacts);
            }

            // --- Seed Events ---
            if (await context.Events.CountAsync() < 3 && adminUser != null) // MODIFIED condition
            {
                var eventTypes = new List<string> { "Community", "Maintenance", "Meeting", "Social", "Holiday" };
                var eventColors = new List<string> { "#007bff", "#ffc107", "#28a745", "#dc3545", "#17a2b8" };
                var events = new List<Event>();
                for (int i = 0; i < 5; i++)
                {
                    var startDate = DateTime.UtcNow.Date.AddDays(i * 10).AddHours(10 + i);
                    events.Add(new Event
                    {
                        Title = $"Sample Event {i + 1}: {eventTypes[i % eventTypes.Count]} Activity",
                        Description = $"This is a description for {eventTypes[i % eventTypes.Count]} event {i + 1}. All residents are welcome!",
                        StartDateTime = startDate,
                        EndDateTime = startDate.AddHours(2 + (i % 3)),
                        Location = (i % 2 == 0) ? "Clubhouse" : "Community Park",
                        EventType = eventTypes[i % eventTypes.Count],
                        IsActive = true,
                        IsAllDay = (i % 4 == 0),
                        Color = eventColors[i % eventColors.Count],
                        CreatedById = adminUser.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(- (i*2) - 1)
                    });
                }
                await context.Events.AddRangeAsync(events);
            }

            // --- Seed Facilities ---
            if (await context.Facilities.CountAsync() < 2) // MODIFIED condition
            {
                context.Facilities.AddRange(
                    new Facility { Name = "Clubhouse Hall", Description = "Multi-purpose hall for events.", Capacity = 100, RatePerHour = 50.00m, IsActive = true, MaintenanceSchedule = "Cleaned daily. Deep clean Mondays." },
                    new Facility { Name = "Swimming Pool", Description = "Outdoor swimming pool.", Capacity = 50, RatePerHour = 0m, IsActive = true, MaintenanceSchedule = "Closed for cleaning Tuesdays 8am-12pm." },
                    new Facility { Name = "Tennis Court 1", Description = "Outdoor tennis court.", Capacity = 4, RatePerHour = 10.00m, IsActive = true },
                    new Facility { Name = "Basketball Court", Description = "Full-size basketball court.", Capacity = 20, RatePerHour = 15.00m, IsActive = true }
                );
                await context.SaveChangesAsync(); // Save facilities to get their IDs
            }
            var facilities = await context.Facilities.ToListAsync();

            // --- Seed Facility Reservations ---
            if (await context.FacilityReservations.CountAsync() < 3 && facilities.Any() && approvedHomeowners.Any()) // MODIFIED condition
            {
                var reservations = new List<FacilityReservation>();
                int resCounter = 0;
                foreach (var facility in facilities.Where(f => f.RatePerHour > 0)) // Only reservable for those with rates for simplicity
                {
                    foreach (var user in approvedHomeowners.Take(2)) // 2 users reserve each facility
                    {
                        var resDate = DateTime.UtcNow.Date.AddDays(resCounter * 3);
                        reservations.Add(new FacilityReservation
                        {
                            FacilityId = facility.Id,
                            UserId = user.Id,
                            StartTime = resDate.AddHours(14 + (resCounter % 4)),
                            EndTime = resDate.AddHours(16 + (resCounter % 4)),
                            Purpose = $"Birthday Party for {user.FirstName}",
                            Status = (resCounter % 2 == 0) ? ReservationStatus.Approved : ReservationStatus.Pending,
                            CreatedAt = DateTime.UtcNow.AddDays(- (resCounter * 2))
                        });
                        resCounter++;
                    }
                }
                if(reservations.Any()) await context.FacilityReservations.AddRangeAsync(reservations);
            }

            // --- Seed Documents ---
            if (await context.Documents.CountAsync() < 3 && adminUser != null) // MODIFIED condition
            {
                var docCategories = new List<string> { "Forms", "Guidelines", "Meeting Minutes", "Financials", "Newsletters" };
                var documents = new List<Document>();
                for (int i = 0; i < 5; i++)
                {
                    documents.Add(new Document
                    {
                        Title = $"Sample Document {i + 1} - {docCategories[i % docCategories.Count]}",
                        Description = $"Important {docCategories[i % docCategories.Count]} document uploaded on {DateTime.UtcNow.AddDays(-i*5):yyyy-MM-dd}.",
                        Category = docCategories[i % docCategories.Count],
                        FileUrl = $"/uploads/documents/sample_doc_{i+1}.pdf", // Dummy URL
                        UploadedById = adminUser.Id,
                        UploadedAt = DateTime.UtcNow.AddDays(-i*5),
                        IsPublic = (i % 2 == 0)
                    });
                }
                await context.Documents.AddRangeAsync(documents);
            }

            // --- Seed Feedback ---
            if (await context.Feedbacks.CountAsync() < 50 && approvedHomeowners.Any() && staffUser != null) // MODIFIED condition threshold
            {
                var feedbackTypes = new List<string> { "Feedback", "Complaint", "Suggestion", "Appreciation" };
                var feedbackStatuses = new List<string> { "New", "InProgress", "Resolved", "Closed" };
                var feedbacks = new List<Feedback>();
                int fbCounter = 0;
                foreach (var user in approvedHomeowners.Take(4)) // 4 users submit feedback
                {
                    var feedbackType = feedbackTypes[fbCounter % feedbackTypes.Count];
                    var feedback = new Feedback
                    {
                        Title = $"{feedbackType} from {user.FirstName}",
                        Description = $"This is a sample {feedbackType.ToLower()} regarding community services. Details are lorem ipsum dolor sit amet.",
                        Type = feedbackType,
                        Status = feedbackStatuses[fbCounter % feedbackStatuses.Count],
                        Priority = (fbCounter % 3) + 1, // 1, 2, or 3
                        SubmittedById = user.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-(fbCounter * 7)),
                        IsPublic = (fbCounter % 3 == 0)
                    };
                    if (feedback.Status == "Resolved" || feedback.Status == "Closed")
                    {
                        feedback.Response = "Thank you for your feedback. This issue has been addressed.";
                        feedback.RespondedAt = feedback.CreatedAt.AddDays(2);
                        feedback.RespondedById = staffUser.Id;
                    }
                    feedbacks.Add(feedback);
                    fbCounter++;
                }
                await context.Feedbacks.AddRangeAsync(feedbacks);
            }

            // --- Seed Contacts (HOA Directory) ---
            if (await context.Contacts.CountAsync() < 50) // MODIFIED condition threshold
            {
                var contacts = new List<Contact>
                {
                    new Contact { Name = "Administration Office", Category = "Administration", PhoneNumber = "(555) 010-0001", EmailAddress = "adminoffice@747hoa.com", OfficeHours = "Mon-Fri, 9am-5pm", Location = "Main Admin Building", DisplayOrder = 1 },
                    new Contact { Name = "Security Main Gate", Category = "Security", PhoneNumber = "(555) 010-0002", OfficeHours = "24/7", DisplayOrder = 2, IsPublic=false },
                    new Contact { Name = "Maintenance Department", Category = "Maintenance", PhoneNumber = "(555) 010-0003", EmailAddress = "maintenance@747hoa.com", OfficeHours = "Mon-Sat, 8am-6pm", DisplayOrder = 3 },
                    new Contact { Name = "Billing Department", Category = "Billing", PhoneNumber = "(555) 010-0004", EmailAddress = "billing@747hoa.com", OfficeHours = "Mon-Fri, 9am-4pm", DisplayOrder = 4 }
                };
                await context.Contacts.AddRangeAsync(contacts);
            }

            // --- Seed Forum Categories ---
            if (await context.ForumCategories.CountAsync() < 50) // MODIFIED condition threshold
            {
                context.ForumCategories.AddRange(
                    new ForumCategory { Name = "General Discussion", Description = "Talk about anything related to the community.", IsActive = true },
                    new ForumCategory { Name = "Events & Activities", Description = "Discuss upcoming events and activities.", IsActive = true },
                    new ForumCategory { Name = "Suggestions Box", Description = "Share your suggestions for improving our HOA.", IsActive = true },
                    new ForumCategory { Name = "Marketplace", Description = "Buy, sell, or trade items with neighbors.", IsActive = false } // Example of an inactive category
                );
                await context.SaveChangesAsync(); // Save categories to get IDs
            }
            var forumCategories = await context.ForumCategories.Where(fc => fc.IsActive).ToListAsync();

            // --- Seed Forum Threads & Replies ---
            if (await context.ForumThreads.CountAsync() < 3 && forumCategories.Any() && approvedHomeowners.Any())
            {
                var threads = new List<ForumThread>();
                var replies = new List<ForumReply>();
                int threadCounter = 0;
                foreach (var category in forumCategories)
                {
                    for (int i = 0; i < 2; i++) // 2 threads per active category
                    {
                        var threadUser = approvedHomeowners[threadCounter % approvedHomeowners.Count];
                        var thread = new ForumThread
                        {
                            CategoryId = category.Id,
                            UserId = threadUser.Id,
                            Title = $"Thread {threadCounter + 1} in {category.Name}",
                            Content = $"This is the initial post for thread {threadCounter + 1}. What are your thoughts on this topic related to {category.Name.ToLower()}?",
                            CreatedAt = DateTime.UtcNow.AddDays(-(threadCounter * 2)),
                            IsLocked = (threadCounter % 5 == 0) // Some threads locked
                        };
                        threads.Add(thread);

                        // Add replies to this thread
                        for (int j = 0; j < 3; j++) // 3 replies per thread
                        {
                            var replyUser = approvedHomeowners[(threadCounter + j + 1) % approvedHomeowners.Count];
                            replies.Add(new ForumReply
                            {
                                // ThreadId will be set after threads are saved
                                UserId = replyUser.Id,
                                Content = $"This is reply number {j + 1} to thread '{thread.Title}'. I think this is an interesting point!",
                                CreatedAt = thread.CreatedAt.AddHours(j + 1),
                                TempThreadTitleForMatching = thread.Title // Helper for matching
                            });
                        }
                        threadCounter++;
                    }
                }
                await context.ForumThreads.AddRangeAsync(threads);
                await context.SaveChangesAsync(); // Save threads to get IDs

                // Update ThreadId for replies and save replies
                if (replies.Any())
                {
                    foreach(var reply in replies)
                    {
                        var relatedThread = await context.ForumThreads.FirstOrDefaultAsync(t => t.Title == reply.TempThreadTitleForMatching);
                        if (relatedThread != null) reply.ThreadId = relatedThread.Id;
                    }
                    await context.ForumReplies.AddRangeAsync(replies.Where(r => r.ThreadId != 0));
                }
            }

            // --- Seed Polls & Surveys ---
            if (await context.Polls.CountAsync() < 50 && adminUser != null && approvedHomeowners.Any()) // MODIFIED condition threshold
            {
                var polls = new List<Poll>();
                var pollOptions = new List<PollOption>();
                var pollVotes = new List<PollVote>();

                // Poll 1: Favorite Community Amenity
                var poll1 = new Poll 
                {
                    Title = "What's your favorite community amenity?", Description = "Help us understand which amenities are most valued.",
                    StartDate = DateTime.UtcNow.AddDays(-7), EndDate = DateTime.UtcNow.AddDays(7), CreatedById = adminUser.Id, IsActive = true
                };
                polls.Add(poll1);
                // Options for Poll 1 - will be added after Polls are saved
                var poll1Options = new List<PollOption>
                {
                    new PollOption { OptionText = "Swimming Pool" },
                    new PollOption { OptionText = "Clubhouse Hall" },
                    new PollOption { OptionText = "Tennis Court" },
                    new PollOption { OptionText = "Basketball Court" },
                    new PollOption { OptionText = "Community Park / Playground" }
                };

                // Poll 2: Best Day for Weekly Farmers Market
                var poll2 = new Poll
                {
                    Title = "Best Day for Weekly Farmers Market?", Description = "We are considering a weekly farmers market. What day works best for you?",
                    StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(12), CreatedById = adminUser.Id, IsActive = true
                };
                polls.Add(poll2);
                var poll2Options = new List<PollOption>
                {
                    new PollOption { OptionText = "Friday Afternoon" },
                    new PollOption { OptionText = "Saturday Morning" },
                    new PollOption { OptionText = "Sunday Morning" }
                };

                await context.Polls.AddRangeAsync(polls);
                await context.SaveChangesAsync(); // Save polls to get IDs

                // Add options to saved polls
                foreach(var opt in poll1Options) { opt.PollId = poll1.Id; pollOptions.Add(opt); }
                foreach(var opt in poll2Options) { opt.PollId = poll2.Id; pollOptions.Add(opt); }
                await context.PollOptions.AddRangeAsync(pollOptions);
                await context.SaveChangesAsync(); // Save options to get IDs

                // Add votes
                var savedPoll1Options = await context.PollOptions.Where(po => po.PollId == poll1.Id).ToListAsync();
                var savedPoll2Options = await context.PollOptions.Where(po => po.PollId == poll2.Id).ToListAsync();
                int voteCounter = 0;
                if (savedPoll1Options.Any())
                {
                    foreach (var user in approvedHomeowners.Take(approvedHomeowners.Count / 2)) // Half the users vote on poll 1
                    {
                        pollVotes.Add(new PollVote { PollId = poll1.Id, PollOptionId = savedPoll1Options[voteCounter % savedPoll1Options.Count].Id, UserId = user.Id, VotedAt = DateTime.UtcNow.AddDays(-voteCounter % 5) });
                        voteCounter++;
                    }
                }
                if (savedPoll2Options.Any())
                {
                    foreach (var user in approvedHomeowners.Skip(approvedHomeowners.Count / 2)) // Other half vote on poll 2
                    {
                        pollVotes.Add(new PollVote { PollId = poll2.Id, PollOptionId = savedPoll2Options[voteCounter % savedPoll2Options.Count].Id, UserId = user.Id, VotedAt = DateTime.UtcNow.AddDays(-voteCounter % 3) });
                        voteCounter++;
                    }
                }
                if(pollVotes.Any()) await context.PollVotes.AddRangeAsync(pollVotes);
            }

            await context.SaveChangesAsync(); // Final save for any pending changes
        }

        // Helper property for ForumReply model (add this to your ForumReply.cs)
        // [NotMapped]
        // public string TempThreadTitleForMatching { get; set; }

    }
}