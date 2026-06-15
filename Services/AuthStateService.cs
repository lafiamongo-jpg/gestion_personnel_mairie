using GestionPersonnelMairie.Core.Constants;

namespace GestionPersonnelMairie.Services;

public class AuthStateService
{
    public string? Email { get; set; }
    public string? NomComplet { get; set; }
    public string? Role { get; set; }
    public int? AgentId { get; set; }
    public int? UtilisateurId { get; set; }
    public string? JwtToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    public bool EstConnecte => !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(JwtToken);
    public bool EstAgent => Role == Roles.Agent;
    public bool EstAdmin => Role == Roles.Admin;
    public bool EstSuperAdmin => Role == Roles.SuperAdmin;
    public bool PeutGererConges => EstAdmin || EstSuperAdmin;
    public bool PeutGererServices => EstAdmin || EstSuperAdmin;
    public bool PeutGererAgents => EstSuperAdmin;
    public bool PeutGererUtilisateurs => EstSuperAdmin;
    public bool PeutVoirDashboardComplet => EstSuperAdmin;
    public bool PeutVoirAudit => EstSuperAdmin;

    public void Clear()
    {
        Email = null;
        NomComplet = null;
        Role = null;
        AgentId = null;
        UtilisateurId = null;
        JwtToken = null;
        TokenExpiresAt = null;
    }
}
