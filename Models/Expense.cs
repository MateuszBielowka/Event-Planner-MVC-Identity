using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
public class Expense
{
    [Key]
    public int ExpenseId { get; set; }

    [ForeignKey("Employee")]
    public int EmployeeId { get; set; }
    public Employee EmployeeRecord { get; set; }
    public string ExpensePurpose { get; set; }
    public int ExpenseCost { get; set; }
}