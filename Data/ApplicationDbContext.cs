using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Template_Identity.Data;

public class ApplicationDbContext : IdentityDbContext
{

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
}
