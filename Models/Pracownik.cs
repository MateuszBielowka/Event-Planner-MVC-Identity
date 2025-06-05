using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Pracownik
{
    [Key]
    public int IdPracownika { get; set; }
    public int IdWydarzenia { get; set; }
    public Funkcja Funkcja { get; set; }
    public string Imie { get; set; }
    public string Nazwisko { get; set; }
    public string AdresEmail { get; set; }
}

public enum Funkcja
{
    Administrator,
    Menadżer,
    Wolontariusz
}