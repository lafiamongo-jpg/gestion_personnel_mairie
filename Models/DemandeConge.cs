using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models
{
    public class DemandeConge
    {
        [Key]
        public int IdDemande { get; set; }
        public DateTime DateDemande { get; set; } = DateTime.Now;
        public string Statut { get; set; } = "attente";
        public string? Commentaire { get; set; }

        // Clé étrangère
        public int IdAgent { get; set; }

        // Navigation
        public Agent Agent { get; set; } = null!;
        public Conge? Conge { get; set; }
    }
}