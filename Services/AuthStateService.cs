namespace GestionPersonnelMairie.Services
{
    public class AuthStateService
    {
        public string? Email { get; set; }
        public string? NomComplet { get; set; }
        public string? Role { get; set; }
        public int? AgentId { get; set; }
        public int? UtilisateurId { get; set; }

        public bool EstAdmin => Role == "Administrateur";
        public bool EstConnecte => !string.IsNullOrEmpty(Email);

        public void Clear()
        {
            Email = null;
            NomComplet = null;
            Role = null;
            AgentId = null;
            UtilisateurId = null;
        }
    }
}