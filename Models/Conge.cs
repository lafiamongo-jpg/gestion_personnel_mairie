using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models
{
    public class Conge
    {
        [Key]
        public int IdConge { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int Duree { get; set; }
        public string? TypeConge { get; set; }
        public string Statut { get; set; } = "en_cours";

        // Retour anticipé
        public bool RetourAnticipe { get; set; } = false;
        public DateTime? DateRetourEffectif { get; set; }

        // Clé étrangère
        public int IdDemande { get; set; }

        // Navigation
        public DemandeConge DemandeConge { get; set; } = null!;
    }
}