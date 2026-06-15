using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }
    public int? IdUtilisateur { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entite { get; set; } = string.Empty;
    public string? EntiteId { get; set; }
    public string? Details { get; set; }
    public DateTime DateAction { get; set; } = DateTime.UtcNow;

    public Utilisateur? Utilisateur { get; set; }
}
