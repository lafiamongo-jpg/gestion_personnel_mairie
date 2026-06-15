using GestionPersonnelMairie.Core.Constants;
using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services;

public class UtilisateurService
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public UtilisateurService(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public List<Utilisateur> GetAll()
    {
        return _context.Utilisateurs
            .Include(u => u.Role)
            .Include(u => u.Agent)
            .AsNoTracking()
            .OrderBy(u => u.Nom)
            .ToList();
    }

    public List<Role> GetRoles() =>
        _context.Roles.AsNoTracking().OrderBy(r => r.NomRole).ToList();

    public List<Agent> GetAgentsSansCompte() =>
        _context.Agents
            .Where(a => !_context.Utilisateurs.Any(u => u.IdAgent == a.Id))
            .AsNoTracking()
            .OrderBy(a => a.Nom)
            .ThenBy(a => a.Prenom)
            .ToList();

    public async Task<string> CreerAsync(string nom, string email, int idRole, int? idAgent = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("L'email est obligatoire pour créer un compte.");

        if (_context.Utilisateurs.Any(u => u.Email.ToLower() == email.ToLower()))
            throw new InvalidOperationException("Un compte existe déjà avec cet email.");

        var role = await _context.Roles.FindAsync(idRole)
            ?? throw new InvalidOperationException("Rôle introuvable.");

        if (idAgent.HasValue && idAgent.Value > 0)
        {
            var agent = await _context.Agents.FindAsync(idAgent.Value)
                ?? throw new InvalidOperationException("Agent introuvable.");
            if (_context.Utilisateurs.Any(u => u.IdAgent == idAgent.Value))
                throw new InvalidOperationException("Cet agent a déjà un compte.");
        }
        else if (role.NomRole == Roles.Agent)
            throw new InvalidOperationException("Un compte Agent doit être lié à une fiche agent.");

        var token = Guid.NewGuid().ToString("N");

        var utilisateur = new Utilisateur
        {
            Nom = nom,
            Email = email,
            MotPasse = BCrypt.Net.BCrypt.HashPassword(AppDefaults.MotDePasseInitial),
            IdRole = idRole,
            IdAgent = idAgent,
            EstActif = true,
            EmailVerifie = false,
            TokenVerification = token,
            TokenVerificationExpiration = DateTime.UtcNow.AddHours(48)
        };

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        return await _emailService.EnvoyerVerificationCompteAsync(email, nom, role.NomRole, token);
    }

    public bool VerifierEmail(string token)
    {
        var u = _context.Utilisateurs
            .FirstOrDefault(x => x.TokenVerification == token);

        if (u == null || u.TokenVerificationExpiration == null
            || u.TokenVerificationExpiration < DateTime.UtcNow)
            return false;

        u.EmailVerifie = true;
        u.TokenVerification = null;
        u.TokenVerificationExpiration = null;
        _context.SaveChanges();
        return true;
    }

    public void Update(int idUtilisateur, string nom, string email, int idRole, string? nouveauMdp)
    {
        var u = _context.Utilisateurs.Find(idUtilisateur)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");

        u.Nom = nom;
        u.Email = email;
        u.IdRole = idRole;

        if (!string.IsNullOrWhiteSpace(nouveauMdp))
            u.MotPasse = BCrypt.Net.BCrypt.HashPassword(nouveauMdp);

        _context.SaveChanges();
    }

    public void Desactiver(int idUtilisateur)
    {
        var u = _context.Utilisateurs.Find(idUtilisateur)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");
        u.EstActif = false;
        _context.SaveChanges();
    }

    public void Activer(int idUtilisateur)
    {
        var u = _context.Utilisateurs.Find(idUtilisateur)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");
        u.EstActif = true;
        u.TentativesEchouees = 0;
        u.DateBlocage = null;
        _context.SaveChanges();
    }

    public void Delete(int idUtilisateur)
    {
        var u = _context.Utilisateurs.Find(idUtilisateur);
        if (u != null)
        {
            _context.Utilisateurs.Remove(u);
            _context.SaveChanges();
        }
    }

    public async Task<string> RenvoyerVerificationAsync(int idUtilisateur)
    {
        var u = await _context.Utilisateurs
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.IdUtilisateur == idUtilisateur)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");

        if (u.EmailVerifie)
            throw new InvalidOperationException("Ce compte est déjà vérifié.");

        var token = Guid.NewGuid().ToString("N");
        u.TokenVerification = token;
        u.TokenVerificationExpiration = DateTime.UtcNow.AddHours(48);
        await _context.SaveChangesAsync();

        return await _emailService.EnvoyerVerificationCompteAsync(
            u.Email, u.Nom, u.Role?.NomRole ?? "Agent", token);
    }
}
