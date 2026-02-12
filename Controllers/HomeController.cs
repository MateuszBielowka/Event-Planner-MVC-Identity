using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template_Identity.Models;

namespace Template_Identity.Controllers;

[Authorize(Roles = "Admin,Manager,Volunteer")]
public class HomeController : Controller
{
    public IActionResult Index() =>  View();
    public IActionResult Privacy() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
