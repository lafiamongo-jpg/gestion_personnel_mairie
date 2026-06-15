using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models;

public class JournalConnexion
{
    [Key]
    public int Id { get; set; }
    public int? IdUtilisateur { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool Reussi { get; set; }
    public string? AdresseIp { get; set; }
    public DateTime DateConnexion { get; set; } = DateTime.UtcNow;
    public string? Message { get; set; }

    public Utilisateur? Utilisateur { get; set; }
}
