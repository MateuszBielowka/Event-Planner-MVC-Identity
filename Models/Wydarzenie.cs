using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
public class Wydarzenie
{
    [Key]
    public int IdWydarzenia { get; set; }
    public string Nazwa { get; set; }
    public string Adres { get; set; }
    public DateTime Data { get; set; }
}