using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template_Identity.Data;
using System.Security.Claims;

namespace Template_Identity.Controllers;

[Authorize(Roles = "Admin,Manager,Volunteer")]
public class AssignmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    public AssignmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin, Manager, Volunteer")]
    public async Task<IActionResult> TaskList(string sortOrder, string searchString)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userData = _context.Employees.FirstOrDefault(p => p.Id == userId);

        var assignments = _context.Assignments.AsQueryable();

        if (User.IsInRole("Volunteer"))
        {
            if (userData == null)
            {
                return NotFound("Nie znaleziono przypisanego pracownika.");
            }
            assignments = assignments.Where(z => z.EmployeeId == userData.EmployeeId);
        }
        else if (User.IsInRole("Manager"))
        {
            if (userData == null)
            {
                return NotFound("Nie znaleziono przypisanego pracownika.");
            }
            assignments = assignments.Where(z => z.EmployeeRecord.EventId == userData.EventId);
        }
        if (!string.IsNullOrEmpty(searchString))
        {
            assignments = assignments.Where(s => s.EmployeeId.ToString().Contains(searchString)
                                   || s.EmployeeRecord.EventRecord.EventName.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                assignments = assignments.OrderByDescending(s => s.EmployeeId);
                break;
            case "Date":
                assignments = assignments.OrderBy(s => s.DueDate);
                break;
            case "date_desc":
                assignments = assignments.OrderByDescending(s => s.DueDate);
                break;
            default:
                assignments = assignments.OrderBy(s => s.EmployeeId);
                break;
        }
        return View(await assignments.AsNoTracking().ToListAsync());
    }


    public IActionResult AddTask() => View();

    [HttpPost]
    public IActionResult AddTask(string newTaskName, string newDueDate, int assignedEmployeeId)
    {
        if (string.IsNullOrEmpty(newTaskName) || string.IsNullOrEmpty(newDueDate))
        {
            ViewBag.Error = "Nazwa i termin zadania są wymagane.";
            return View();
        }

        var employees = from s in _context.Employees
                        select s;
        employees = employees.Where(s => s.EmployeeId == assignedEmployeeId);
        if (!employees.Any())
        {
            ViewBag.Error = "Pracownik o takim id nie istnieje.";
            return View();
        }

        _context.Assignments.Add(new Assignment
        {
            EmployeeId = assignedEmployeeId,
            TaskName = newTaskName,
            DueDate = newDueDate
            // IsDone = NotStarted
        });
        _context.SaveChanges();
        return RedirectToAction("TaskList");
    }

    [HttpPost]
    public IActionResult EditTask(string newTaskName, string newDueDate, int assignedEmployeeId)
    {
        _context.Assignments.Add(new Assignment
        {
            TaskName = newTaskName,
            DueDate = newDueDate,
            EmployeeId = assignedEmployeeId
        });
        _context.SaveChanges();
        return RedirectToAction("TaskList");
    }

    public IActionResult DeleteTask(int id)
    {
        var retrievedAssignment = _context.Assignments.SingleOrDefault(task => task.AssignmentId == id);
        _context.Assignments.Remove(retrievedAssignment);
        _context.SaveChanges();
        return RedirectToAction("TaskList");
    }

    // public IActionResult ChangeTaskState(int id, State newState)
    //     {
    //     var retrievedAssignment = _context.Assignments.SingleOrDefault(task => task.AssignmentId == id);
    //     retrievedAssignment.IsDone = newState;
    //     _context.SaveChanges();
    //     return RedirectToAction("TaskList");
    // }

}
