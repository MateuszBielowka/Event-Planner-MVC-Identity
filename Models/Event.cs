using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;

public class Event
{
    [Key]
    public int EventId { get; set; }
    public string EventName { get; set; }
    public string Address { get; set; }
    public DateTime EventDate { get; set; }
}