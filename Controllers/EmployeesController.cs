using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template_Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Security.Claims;
using System;

namespace Template_Identity.Controllers;

[Authorize(Roles = "Admin,Manager,Volunteer")]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _context;
    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }
    public bool IsValid(string emailaddress)
    {
        try
        {
            MailAddress m = new MailAddress(emailaddress);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> EmployeesOverview(string sortOrder, string searchString)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userData = _context.Employees.FirstOrDefault(e => e.Id == userId);
        var employees = from s in _context.Employees
                         select s;


        if (User.IsInRole("Manager"))
        {
            if (userData == null)
            {
                return NotFound("Nie znaleziono przypisanego pracownika.");
            }
            employees = employees.Where(e => e.EventId == userData.EventId);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            employees = employees.Where(s => s.FirstName.Contains(searchString)
                                   || s.Surname.Contains(searchString)
                                   || s.EmployeeId.ToString().Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                employees = employees.OrderByDescending(s => s.FirstName);
                break;
            case "Date":
                employees = employees.OrderBy(s => s.EmployeeRole);
                break;
            case "date_desc":
                employees = employees.OrderByDescending(s => s.EmployeeRole);
                break;
            default:
                employees = employees.OrderBy(s => s.FirstName);
                break;
        }
        return View(await employees.AsNoTracking().ToListAsync());
    }


    public IActionResult AddEmployee() => View();
    [HttpPost]
    public async Task<IActionResult> AddEmployee(string registerEmail, string registerPassword, int eventId, string accessLevelInput, string registerFirstName, string registerSurname)
    {
        var userManager = HttpContext.RequestServices.GetService(typeof(UserManager<IdentityUser>)) as UserManager<IdentityUser>;
        if (string.IsNullOrEmpty(registerEmail) || string.IsNullOrEmpty(registerPassword) || string.IsNullOrEmpty(eventId.ToString())
        || string.IsNullOrEmpty(accessLevelInput) || string.IsNullOrEmpty(registerFirstName) || string.IsNullOrEmpty(registerSurname))
        {
            ViewBag.Error = "Wszystkie pola są wymagane.";
            return View();
        }

        if (!Enum.TryParse<AccessLevel>(accessLevelInput, out var userRole))
        {
            ViewBag.Error = "Niepoprawna funkcja.";
            return View();
        }

        if (!IsValid(registerEmail))
        {
            ViewBag.Error = "Adres email nie spełnia wymagań";
            return View();
        }



        var existingUser = await userManager.FindByEmailAsync(registerEmail);
        if (existingUser != null)
        {
            ViewBag.Error = "Użytkownik o podanym adresie e-mail już istnieje.";
            return View();
        }
        String userName = registerEmail;
        var newUser = new IdentityUser{ UserName = userName, Email = registerEmail};
        var result = await userManager.CreateAsync(newUser, registerPassword);
        if (!result.Succeeded)
        {
            ViewBag.Error = "Błąd podczas tworzenia użytkownika: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }
        

        _context.Employees.Add(new Employee
        {
            Id = newUser.Id,
            EventId = eventId,
            EmployeeRole = userRole,
            FirstName = registerFirstName,
            Surname = registerSurname,
            EventRecord = _context.Events.SingleOrDefault(e => e.EventId == eventId)
        });
        string role = "";
        if (userRole == AccessLevel.Volunteer)
        {
            role = "Volunteer";
        }
        else if (userRole == AccessLevel.Manager)
        {
            role = "Manager";
        }
        else if (userRole == AccessLevel.Admin)
        {
            role = "Admin";
        }
        else
        {
            ViewBag.Error = "Niepoprawna funkcja.";
            return View();
        }
        var roleResult = await userManager.AddToRoleAsync(newUser, role);
        
        await _context.SaveChangesAsync();
        return RedirectToAction("EmployeesOverview");
    }

    public IActionResult DeleteEmployee(int id)
    {
        var RetrievedEmployee = _context.Employees.SingleOrDefault(e => e.EmployeeId == id);
        _context.Employees.Remove(RetrievedEmployee);
        _context.SaveChanges();
        return RedirectToAction("EmployeesOverview");
    }


}