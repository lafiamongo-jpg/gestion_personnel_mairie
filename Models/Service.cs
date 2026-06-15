using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models
{
    public class Service
    {
        [Key]
        public int IdService { get; set; }
        public string NomService { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Agent> Agents { get; set; } = new List<Agent>();
        public ICollection<Poste> Postes { get; set; } = new List<Poste>();
    }
}