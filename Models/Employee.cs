using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
    
    [ForeignKey("IdentityUser")]
    public string Id { get; set; }
    public IdentityUser IdentityUser { get; set; }
    
    [ForeignKey("Event")]
    public int EventId { get; set; }
    public Event EventRecord { get; set; }
    public AccessLevel EmployeeRole { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
}

public enum AccessLevel
{
    Admin,
    Manager,
    Volunteer
}