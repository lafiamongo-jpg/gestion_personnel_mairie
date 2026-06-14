using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMairie.Models
{
    public class Role
    {
        [Key]
        public int IdRole { get; set; }
        public string NomRole { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
    }
}