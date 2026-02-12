using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Template_Identity.Data;

public class Program
{
    public static async Task Main(string[] args)
    {
        var cultureInfo = new System.Globalization.CultureInfo("pl-PL");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthorization();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!dbContext.Events.Any(w => w.EventId == 1))
            {
                dbContext.Events.Add(new Event
                {
                    EventId = 1,
                    EventName = "Wydarzenie przykładowe",
                    Address = "ul. Przykładowa 1",
                    EventDate = DateTime.Now.AddDays(7)
                });
                await dbContext.SaveChangesAsync();
            }
        }

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[] { "Admin", "Manager", "Volunteer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                } 
            }
        }

        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var users = new[]
            {
                new { Email = "admin@admin.com", Password = "Admin@123", Role = "Admin", FirstName = "Adam", Surname = "Admin", EmployeeRole = AccessLevel.Admin },
                new { Email = "manager@manager.com", Password = "Manager@123", Role = "Manager", FirstName = "Jan", Surname = "Kowalski", EmployeeRole = AccessLevel.Manager },
                new { Email = "user@user.com", Password = "User@123", Role = "Volunteer", FirstName = "Anna", Surname = "Nowak", EmployeeRole = AccessLevel.Volunteer }
            };
            foreach (var u in users)
            {
                if (await userManager.FindByEmailAsync(u.Email) == null)
                {
                    var baseUser = new IdentityUser { UserName = u.Email, Email = u.Email };
                    var result = await userManager.CreateAsync(baseUser, u.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(baseUser, u.Role);
                        if (!dbContext.Employees.Any(p => p.Id == baseUser.Id))
                        {
                            dbContext.Employees.Add(new Employee
                            {
                                Id = baseUser.Id,
                                FirstName = u.FirstName,
                                Surname = u.Surname,
                                EmployeeRole = u.EmployeeRole,
                                EventId = 1,
                                EventRecord = dbContext.Events.First(w => w.EventId == 1)
                            });
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
            }
        }
        await app.RunAsync();
    }
}
