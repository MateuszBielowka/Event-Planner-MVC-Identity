using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
public class Wydatek
{
    [Key]
    public int IdWydatku { get; set; }
    public int IdPracownika { get; set; }
    public string Cel { get; set; }
    public int Kwota { get; set; }
}