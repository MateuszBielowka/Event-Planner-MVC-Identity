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

        // Add services to the container.
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

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!dbContext.Wydarzenia.Any(w => w.IdWydarzenia == 1))
            {
                dbContext.Wydarzenia.Add(new Wydarzenie
                {
                    IdWydarzenia = 1,
                    Nazwa = "Wydarzenie przykładowe",
                    Adres = "ul. Przykładowa 1",
                    Data = DateTime.Now.AddDays(7)
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
            var adminEmail = "admin@admin.com";
            var adminPassword = "Admin@123";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userEmail = "manager@manager.com";
            var adminPassword = "Manager@123";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var baseUser = new IdentityUser { UserName = userEmail, Email = userEmail };
                var result = await userManager.CreateAsync(baseUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(baseUser, "Manager");
                    dbContext.Pracownicy.Add(new Pracownik
                    {
                        Id = baseUser.Id,
                        Imie = "Jan",
                        Nazwisko = "Kowalski",
                        Funkcja = Funkcja.Menadżer,
                        IdWydarzenia = 1
                    });
                    await dbContext.SaveChangesAsync();
                }
            }
        }
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userEmail = "user@user.com";
            var adminPassword = "User@123";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var baseUser = new IdentityUser { UserName = userEmail, Email = userEmail };
                var result = await userManager.CreateAsync(baseUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(baseUser, "Volunteer");
                    dbContext.Pracownicy.Add(new Pracownik
                    {
                        Id = baseUser.Id,
                        Imie = "Anna",
                        Nazwisko = "Nowak",
                        Funkcja = Funkcja.Wolontariusz,
                        IdWydarzenia = 1
                    });
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        await app.RunAsync();
    }
}
