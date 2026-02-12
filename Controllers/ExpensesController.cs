using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template_Identity.Data;
using System.Globalization;
using System.Security.Claims;


namespace Template_Identity.Controllers;

[Authorize(Roles = "Admin,Manager,Volunteer")]
public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _context;
    public ExpensesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> ExpensesOverview(string sortOrder, string searchString)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userData = _context.Employees.FirstOrDefault(p => p.Id == userId);

        var expenses = _context.Expenses.AsQueryable();

        if (User.IsInRole("Manager"))
        {
            if (userData == null)
            {
                return NotFound("Nie znaleziono przypisanego pracownika.");
            }
            expenses = expenses.Where(w => w.EmployeeRecord.EventId == userData.EventId);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            expenses = expenses.Where(s => s.EmployeeId.ToString().Contains(searchString)
                                   || s.ExpensePurpose.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                expenses = expenses.OrderByDescending(s => s.EmployeeId);
                break;
            case "Date":
                expenses = expenses.OrderBy(s => s.ExpenseCost);
                break;
            case "date_desc":
                expenses = expenses.OrderByDescending(s => s.ExpenseCost);
                break;
            default:
                expenses = expenses.OrderBy(s => s.EmployeeId);
                break;
        }

        var expensesSorted = expenses.ToList();
        var expensesSum = expensesSorted.Sum(x => x.ExpenseCost);
        ViewBag.Expenses = expensesSum;

        return View(await expenses.AsNoTracking().ToListAsync());
    }


    [Authorize(Roles = "Admin, Manager")]
    public IActionResult AddExpense() => View();
    [HttpPost]
    public IActionResult AddExpense(int newExpenseCost, string newExpenseName, int assignedEmployeeId)
    {
        var cultureInfo = new System.Globalization.CultureInfo("pl-PL");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        if (string.IsNullOrEmpty(newExpenseCost.ToString()) || string.IsNullOrEmpty(newExpenseName))
        {
            ViewBag.Error = "Kwota i cel wydatku są wymagane.";
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

        _context.Expenses.Add(new Expense
        {
            EmployeeId = assignedEmployeeId,
            ExpensePurpose = newExpenseName,
            ExpenseCost = newExpenseCost
        });
        Console.WriteLine($"Dodano wydatek: {newExpenseName} o kwocie {newExpenseCost} dla pracownika o ID {assignedEmployeeId}");
        _context.SaveChanges();
        return RedirectToAction("ExpensesOverview");
    }

    public IActionResult DeleteExpense(int id)
    {
        var retrievedExpense = _context.Expenses.SingleOrDefault(e => e.ExpenseId == id);
        _context.Expenses.Remove(retrievedExpense);
        _context.SaveChanges();
        return RedirectToAction("ExpensesOverview");
    }


}