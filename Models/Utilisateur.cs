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
        public bool EstActif { get; set; } = true;
        public int TentativesEchouees { get; set; }
        public DateTime? DateBlocage { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiration { get; set; }

        public bool EmailVerifie { get; set; }
        public string? TokenVerification { get; set; }
        public DateTime? TokenVerificationExpiration { get; set; }

        public int IdRole { get; set; }
        public int? IdAgent { get; set; }

        public Role Role { get; set; } = null!;
        public Agent? Agent { get; set; }
    }
}
