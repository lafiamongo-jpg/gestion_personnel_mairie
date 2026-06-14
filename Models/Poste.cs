using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models
{
    public class Poste
    {
        [Key]
        public int IdPoste { get; set; }
        public string NomPoste { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Agent> Agents { get; set; } = new List<Agent>();
    }
}