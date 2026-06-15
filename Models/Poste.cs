using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models
{
    public class Poste
    {
        [Key]
        public int IdPoste { get; set; }
        public string NomPoste { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int IdService { get; set; }
        public int NbPostes { get; set; } = 1;

        public Service Service { get; set; } = null!;
        public ICollection<Agent> Agents { get; set; } = new List<Agent>();

        public int NbOccupes => Agents?.Count ?? 0;
        public int NbVacants => Math.Max(0, NbPostes - NbOccupes);
    }
}
