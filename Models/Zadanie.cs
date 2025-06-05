using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
public class Zadanie
{
    [Key]
    public int IdZadania { get; set; }
    public int IdPracownika { get; set; }
    public string Nazwa { get; set; }
    public DateTime Termin { get; set; }
}