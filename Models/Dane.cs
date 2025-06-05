using System.ComponentModel.DataAnnotations;
public class Dane
{
    [Key]
    public int Id { get; set; }
    public string Tresc { get; set; }
}