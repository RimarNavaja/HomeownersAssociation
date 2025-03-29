using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HomeownersAssociation.Models;

namespace HomeownersAssociation.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Redirect admin users to Admin Dashboard
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Admin");
        }

        // Redirect staff users to Admin Dashboard as well
        if (User.IsInRole("Staff"))
        {
            return RedirectToAction("Index", "Admin");
        }

        // Regular users see the normal homepage
        return View();
    }

    [Authorize]
    public IActionResult MyBills()
    {
        return RedirectToAction("MyBills", "Billing");
    }

    [Authorize]
    public IActionResult MyPayments()
    {
        return RedirectToAction("MyPayments", "Billing");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult TermsAndConditions()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
