using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
public class Wydatek
{
    [Key]
    public int IdWydatku { get; set; }

    [ForeignKey("Pracownik")]
    public int IdPracownika { get; set; }
    public Pracownik Pracownik { get; set; }
    public string Cel { get; set; }
    public int Kwota { get; set; }
}