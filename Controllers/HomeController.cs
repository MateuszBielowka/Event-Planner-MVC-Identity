using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template_Identity.Data;
using Template_Identity.Models;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace Template_Identity.Controllers;
[Authorize(Roles = "Admin,Manager,Volunteer")]
public class HomeController : Controller
{


    private readonly ApplicationDbContext _context;
    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
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


    public IActionResult DodajPracownikow() => View();
    [HttpPost]
    public async Task<IActionResult> DodajPracownikow(string adresemail1, string haslo, int idwydarzenia1, string funkcja1, string imie1, string nazwisko1)
    {
        var userManager = HttpContext.RequestServices.GetService(typeof(UserManager<IdentityUser>)) as UserManager<IdentityUser>;
        if (string.IsNullOrEmpty(adresemail1) || string.IsNullOrEmpty(haslo) || string.IsNullOrEmpty(idwydarzenia1.ToString())
        || string.IsNullOrEmpty(funkcja1) || string.IsNullOrEmpty(imie1) || string.IsNullOrEmpty(nazwisko1))
        {
            ViewBag.Error = "Wszystkie pola są wymagane.";
            return View();
        }
        
        if (!Enum.TryParse<Funkcja>(funkcja1, out var funkcja))
        {
            ViewBag.Error = "Niepoprawna funkcja.";
            return View();
        }
        string passwordPattern = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$";

        if (!IsValid(adresemail1))
        {
            ViewBag.Error = "Adres email nie spełnia wymagań";
            return View();
        }



        var existingUser = await userManager.FindByEmailAsync(adresemail1);
        if (existingUser != null)
        {
            ViewBag.Error = "Użytkownik o podanym adresie e-mail już istnieje.";
            return View();
        }
        var newUser = new IdentityUser{ UserName = adresemail1, Email = adresemail1};
        var result = await userManager.CreateAsync(newUser, haslo);
        if (!result.Succeeded)
        {
            ViewBag.Error = "Błąd podczas tworzenia użytkownika: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }
        

        _context.Pracownicy.Add(new Pracownik
        {
            Id = newUser.Id,
            IdWydarzenia = idwydarzenia1,
            Funkcja = funkcja,
            Imie = imie1,
            Nazwisko = nazwisko1,
        });
        string role = "";
        if (funkcja == Funkcja.Wolontariusz)
        {
            role = "Volunteer";
        }
        else if (funkcja == Funkcja.Menadżer)
        {
            role = "Manager";
        }
        else if (funkcja == Funkcja.Administrator)
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
        return RedirectToAction("ListaPracownikow");
    }

    public IActionResult UsunPracownika(int id)
    {
        var PracownikInDb = _context.Pracownicy.SingleOrDefault(pracownik => pracownik.IdPracownika == id);
        _context.Pracownicy.Remove(PracownikInDb);
        _context.SaveChanges();
        return RedirectToAction("ListaPracownikow");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult DodajWydarzenia() => View();
    [HttpPost]
    public IActionResult DodajWydarzenia(string nazwa1, string adres1, DateTime data1)
    {
        if (string.IsNullOrEmpty(nazwa1) || string.IsNullOrEmpty(adres1))
        {
            ViewBag.Error = "Nazwa i adres wydarzenia są wymagane.";
            return View();
        }

        _context.Wydarzenia.Add(new Wydarzenie
        {
            Nazwa = nazwa1,
            Adres = adres1,
            Data = data1
        });

        _context.SaveChanges();
        return RedirectToAction("ListaWydarzen");
    }

    public IActionResult UsunWydarzenie(int id)
    {
        var WydarzenieInDb = _context.Wydarzenia.SingleOrDefault(wydarzenie => wydarzenie.IdWydarzenia == id);
        _context.Wydarzenia.Remove(WydarzenieInDb);
        _context.SaveChanges();
        return RedirectToAction("ListaWydarzen");
    }

    [Authorize(Roles = "Admin, Manager")]
    public IActionResult DodajWydatek() => View();
    [HttpPost]
    public IActionResult DodajWydatek(int liczba1, string nazwa1, int liczba2)
    {
        var cultureInfo = new System.Globalization.CultureInfo("pl-PL");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        if (string.IsNullOrEmpty(liczba1.ToString()) || string.IsNullOrEmpty(nazwa1))
        {
            ViewBag.Error = "Kwota i cel wydatku są wymagane.";
            return View();
        }

        var pracownicy = from s in _context.Pracownicy
                      select s;
        pracownicy = pracownicy.Where(s => s.IdPracownika == liczba2);
        if (!pracownicy.Any())
        {
            ViewBag.Error = "Pracownik o takim id nie istnieje.";
            return View();
        }

        _context.Wydatki.Add(new Wydatek
        {
            IdPracownika = liczba2,
            Cel = nazwa1,
            Kwota = liczba1
        });
        Console.WriteLine($"Dodano wydatek: {nazwa1} o kwocie {liczba1} dla pracownika o ID {liczba2}");
        _context.SaveChanges();
        return RedirectToAction("ListaWydatkow");
    }

    public IActionResult UsunWydatek(int id)
    {
        var WydatekInDb = _context.Wydatki.SingleOrDefault(wydatek => wydatek.IdWydatku == id);
        _context.Wydatki.Remove(WydatekInDb);
        _context.SaveChanges();
        return RedirectToAction("ListaWydatkow");
    }
    // public IActionResult EdytujWydatek(int id)
    // {
    //     var wydatekInDb = _context.Wydatki.SingleOrDefault(zadanie => zadanie.IdWydatku == id);
    //     _context.Wydatki.Remove(wydatekInDb);
    //     _context.SaveChanges();

    //     return View(wydatekInDb);
    // }

    // [HttpPost]
    // public IActionResult EdytujWydatek(string cel, decimal kwota, int id)
    // {
    //     _context.Wydatki.Add(new Wydatek
    //     {
    //         Cel = cel,
    //         Kwota = kwota,
    //         IdPracownika = id
    //     });
    //     _context.SaveChanges();
    //     return RedirectToAction("ListaWydatkow");
    // }

    public IActionResult DodajZadanie() => View();
    // [HttpPost]
    // public IActionResult DodajZadanie(Zadanie zadanie)
    // {
    //     if (string.IsNullOrEmpty(zadanie.Nazwa) || string.IsNullOrEmpty(zadanie.Termin.ToString()))
    //     {
    //         ViewBag.Error = "Nazwa i termin zadania są wymagane.";
    //         return View();
    //     }

    //     _context.Zadania.Add(new Zadanie
    //     {
    //         IdPracownika = zadanie.IdPracownika,
    //         Nazwa = zadanie.Nazwa,
    //         Termin = zadanie.Termin
    //     });
    //     _context.SaveChanges();
    //     return RedirectToAction("ListaZadan");
    // }
    [HttpPost]
    public IActionResult DodajZadanie(string nazwa1, string data1, int liczba1)
    {
        if (string.IsNullOrEmpty(nazwa1) || string.IsNullOrEmpty(data1))
        {
            ViewBag.Error = "Nazwa i termin zadania są wymagane.";
            return View();
        }

        var pracownicy = from s in _context.Pracownicy
                      select s;
        pracownicy = pracownicy.Where(s => s.IdPracownika == liczba1);
        if (!pracownicy.Any())
        {
            ViewBag.Error = "Pracownik o takim id nie istnieje.";
            return View();
        }

        _context.Zadania.Add(new Zadanie
        {
            IdPracownika = liczba1,
            Nazwa = nazwa1,
            Termin = data1
        });
        _context.SaveChanges();
        return RedirectToAction("ListaZadan");
    }

    // public async Task<IActionResult> EdytujZadanie(int? id)
    // {
    //     if (id == null)
    //     {
    //         return NotFound();
    //     }

    //     var course = await _context.Zadania
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(m => m.IdZadania == id);
    //     if (course == null)
    //     {
    //         return NotFound();
    //     }
    //     PopulateDepartmentsDropDownList(course.DepartmentID);
    //     return View(course);
    // }

    // public IActionResult EdytujZadanie(int id)
    // {
    //     var ZadanieInDb = _context.Zadania.SingleOrDefault(zadanie => zadanie.IdZadania == id);
    //     // _context.Zadania.Remove(ZadanieInDb);
    //     // _context.SaveChanges();

    //     return View(ZadanieInDb);
    // }

    // [HttpPost]
    // public IActionResult EdytujZadanie(Zadanie zadanie)
    // {
    //     _context.Zadania.Update(zadanie);
    //     _context.SaveChanges();
    //     return RedirectToAction("ListaZadan");
    // }

    [HttpPost]
    public IActionResult EdytujZadanie(string Nazwa, string Termin, int IdPracownika)
    {
        _context.Zadania.Add(new Zadanie
        {
            Nazwa = Nazwa,
            Termin = Termin,
            IdPracownika = IdPracownika
        });
        _context.SaveChanges();
        return RedirectToAction("ListaZadan");
    }

    public IActionResult UsunZadanie(int id)
    {
        var ZadanieInDb = _context.Zadania.SingleOrDefault(zadanie => zadanie.IdZadania == id);
        _context.Zadania.Remove(ZadanieInDb);
        _context.SaveChanges();
        return RedirectToAction("ListaZadan");
    }

    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> ListaPracownikow(string sortOrder, string searchString)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;

        var pracownicy = from s in _context.Pracownicy
                      select s;

        if (!string.IsNullOrEmpty(searchString))
        {
            pracownicy = pracownicy.Where(s => s.Imie.Contains(searchString)
                                   || s.Nazwisko.Contains(searchString)
                                   || s.IdPracownika.ToString().Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                pracownicy = pracownicy.OrderByDescending(s => s.Imie);
                break;
            case "Date":
                pracownicy = pracownicy.OrderBy(s => s.Funkcja);
                break;
            case "date_desc":
                pracownicy = pracownicy.OrderByDescending(s => s.Funkcja);
                break;
            default:
                pracownicy = pracownicy.OrderBy(s => s.Imie);
                break;
        }
        return View(await pracownicy.AsNoTracking().ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListaWydarzen(string sortOrder, string searchString)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;

        var wydarzenia = from s in _context.Wydarzenia
                         select s;

        if (!string.IsNullOrEmpty(searchString))
        {
            wydarzenia = wydarzenia.Where(s => s.Nazwa.Contains(searchString)
                                   || s.Adres.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                wydarzenia = wydarzenia.OrderByDescending(s => s.Nazwa);
                break;
            case "Date":
                wydarzenia = wydarzenia.OrderBy(s => s.Data);
                break;
            case "date_desc":
                wydarzenia = wydarzenia.OrderByDescending(s => s.Data);
                break;
            default:
                wydarzenia = wydarzenia.OrderBy(s => s.Nazwa);
                break;
        }
        return View(await wydarzenia.AsNoTracking().ToListAsync());
    }
    
    [Authorize(Roles = "Admin, Manager")]
    public async Task<IActionResult> ListaWydatkow(string sortOrder, string searchString)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var pracownik = _context.Pracownicy.FirstOrDefault(p => p.Id == userId);

        var wydatki = _context.Wydatki.AsQueryable();

        if (User.IsInRole("Manager"))
        {
            if (pracownik == null)
            {
                return NotFound("Nie znaleziono przypisanego pracownika.");
            }
            wydatki = wydatki.Where(w => w.Pracownik.IdWydarzenia == pracownik.IdWydarzenia);
        }
        
        if (!string.IsNullOrEmpty(searchString))
        {
            wydatki = wydatki.Where(s => s.IdPracownika.ToString().Contains(searchString)
                                   || s.Cel.Contains(searchString));
        }

        // SORTOWANIE KWOTĄ NIE DZIAŁA, MOŻE PRZEZ TYP DECIMAL
        switch (sortOrder)
        {
            case "name_desc":
                wydatki = wydatki.OrderByDescending(s => s.IdPracownika);
                break;
            case "Date":
                wydatki = wydatki.OrderBy(s => s.Kwota);
                break;
            case "date_desc":
                wydatki = wydatki.OrderByDescending(s => s.Kwota);
                break;
            default:
                wydatki = wydatki.OrderBy(s => s.IdPracownika);
                break;
        }

        var posortowaneWydatki = wydatki.ToList();
        var sumaWydatkow = posortowaneWydatki.Sum(x => x.Kwota);
        ViewBag.Wydatki = sumaWydatkow;

        return View(await wydatki.AsNoTracking().ToListAsync());
    }
    
      [Authorize(Roles = "Admin, Manager, Volunteer")]
    public async Task<IActionResult> ListaZadan(string sortOrder, string searchString)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var pracownik = _context.Pracownicy.FirstOrDefault(p => p.Id == userId);

        var zadania = _context.Zadania.AsQueryable();

        if (User.IsInRole("Volunteer"))
        {
            if (pracownik == null)
            {
                return NotFound("Nie znaleziono przypisanego pracownika.");
            }
            zadania = zadania.Where(z => z.IdPracownika == pracownik.IdPracownika);
        }
        else if (User.IsInRole("Manager"))
        {
            if (pracownik == null)
            {
                return NotFound("Nie znaleziono przypisanego pracownika.");
            }
            zadania = zadania.Where(z => z.Pracownik.IdWydarzenia == pracownik.IdWydarzenia);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            zadania = zadania.Where(s => s.IdPracownika.ToString().Contains(searchString)
                                   || s.Nazwa.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                zadania = zadania.OrderByDescending(s => s.IdPracownika);
                break;
            case "Date":
                zadania = zadania.OrderBy(s => s.Termin);
                break;
            case "date_desc":
                zadania = zadania.OrderByDescending(s => s.Termin);
                break;
            default:
                zadania = zadania.OrderBy(s => s.IdPracownika);
                break;
        }
        return View(await zadania.AsNoTracking().ToListAsync());
    }
    static String HashPassword(String napis)
    {
        Encoding enc = Encoding.UTF8;
        var hashBuilder = new StringBuilder();
        using var hash = MD5.Create();
        byte[] result = hash.ComputeHash(enc.GetBytes(napis));
        foreach (var b in result)
            hashBuilder.Append(b.ToString("x2"));
        return hashBuilder.ToString();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
