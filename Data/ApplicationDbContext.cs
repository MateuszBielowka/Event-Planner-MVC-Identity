using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Template_Identity.Data;

public class ApplicationDbContext : IdentityDbContext
{

    public DbSet<Pracownik> Pracownicy { get; set; }
    public DbSet<Wydarzenie> Wydarzenia { get; set; }
    public DbSet<Wydatek> Wydatki { get; set; }
    public DbSet<Zadanie> Zadania { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
}
