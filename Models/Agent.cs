namespace GestionPersonnelMairie.Models
{
    public class Agent
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Departement { get; set; } = string.Empty;
        public DateTime DateEmbauche { get; set; }
        public decimal Salaire { get; set; }
        public string Statut { get; set; } = "Actif";

        // Clés étrangères
        public int IdService { get; set; }
        public int IdPoste { get; set; }

        // Navigation
        public Service Service { get; set; } = null!;
        public Poste Poste { get; set; } = null!;
        public Utilisateur? Utilisateur { get; set; }
        public ICollection<DemandeConge> DemandesConge { get; set; } = new List<DemandeConge>();

        // Propriétés calculées
        public string Initiales => (Prenom.Length > 0 && Nom.Length > 0)
            ? $"{Prenom[0]}{Nom[0]}".ToUpper()
            : "??";
        public string NomComplet => $"{Prenom} {Nom}";
    }
}