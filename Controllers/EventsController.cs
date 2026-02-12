using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template_Identity.Data;

namespace Template_Identity.Controllers;

[Authorize(Roles = "Admin,Manager,Volunteer")]
public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }
      [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EventsOverview(string sortOrder, string searchString)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;

        var events = from s in _context.Events
                         select s;

        if (!string.IsNullOrEmpty(searchString))
        {
            events = events.Where(s => s.EventName.Contains(searchString)
                                   || s.Address.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                events = events.OrderByDescending(s => s.EventName);
                break;
            case "Date":
                events = events.OrderBy(s => s.EventDate);
                break;
            case "date_desc":
                events = events.OrderByDescending(s => s.EventDate);
                break;
            default:
                events = events.OrderBy(s => s.EventName);
                break;
        }
        return View(await events.AsNoTracking().ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AddEvent() => View();
    [HttpPost]
    public IActionResult AddEvent(string newEventName, string newAddress, DateTime newDate)
    {
        if (string.IsNullOrEmpty(newEventName) || string.IsNullOrEmpty(newAddress))
        {
            ViewBag.Error = "Nazwa i adres wydarzenia są wymagane.";
            return View();
        }

        _context.Events.Add(new Event
        {
            EventName = newEventName,
            Address = newAddress,
            EventDate = newDate
        });

        _context.SaveChanges();
        return RedirectToAction("EventsOverview");
    }

    public IActionResult DeleteEvent(int id)
    {
        var RetrievedEvent = _context.Events.SingleOrDefault(e => e.EventId == id);
        _context.Events.Remove(RetrievedEvent);
        _context.SaveChanges();
        return RedirectToAction("EventsOverview");
    }
}