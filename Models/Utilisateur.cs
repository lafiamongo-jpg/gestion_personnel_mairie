using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models
{
    public class Utilisateur
    {
        [Key]
        public int IdUtilisateur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotPasse { get; set; } = string.Empty;

        // Réinitialisation mot de passe
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiration { get; set; }

        // Clés étrangères
        public int IdRole { get; set; }
        public int IdAgent { get; set; }

        // Navigation
        public Role Role { get; set; } = null!;
        public Agent Agent { get; set; } = null!;
    }
}