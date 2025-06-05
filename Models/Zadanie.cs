using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Zadanie
{
    [Key]
    public int IdZadania { get; set; }

    [ForeignKey("Pracownik")]
    public int IdPracownika { get; set; }
    public Pracownik Pracownik { get; set; }
    public string Nazwa { get; set; }
    public string Termin { get; set; }
}

