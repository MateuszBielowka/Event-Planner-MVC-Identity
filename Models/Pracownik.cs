using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Pracownik
{
    [Key]
    public int IdPracownika { get; set; }
    
    [ForeignKey("IdentityUser")]
    public string Id { get; set; }
    public IdentityUser IdentityUser { get; set; }
    

    [ForeignKey("Wydarzenie")]
    public int IdWydarzenia { get; set; }
    public Wydarzenie Wydarzenie { get; set; }
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