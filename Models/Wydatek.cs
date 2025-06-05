using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
public class Wydatek
{
    [Key]
    public int IdWydatku { get; set; }
    public int IdPracownika { get; set; }
    public string Cel { get; set; }
    public decimal Kwota { get; set; }
}