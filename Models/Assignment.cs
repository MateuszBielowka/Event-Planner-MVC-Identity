using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Assignment
{
    [Key]
    public int AssignmentId { get; set; }

    [ForeignKey("Employee")]
    public int EmployeeId { get; set; }
    public Employee EmployeeRecord { get; set; }
    public string TaskName { get; set; }
    public string DueDate { get; set; }

    // public State IsDone {get;  set; }
}

public enum State
{
    Done,
    Pending,
    NotStarted
}