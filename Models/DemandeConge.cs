using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models
{
    public class DemandeConge
    {
        [Key]
        public int IdDemande { get; set; }
        public DateTime DateDemande { get; set; } = DateTime.Now;
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public string? TypeConge { get; set; }
        public string Statut { get; set; } = "attente";
        public string? Commentaire { get; set; }

        public int IdAgent { get; set; }

        public Agent Agent { get; set; } = null!;
        public Conge? Conge { get; set; }
    }
}
