using GestionPersonnelMairie.Core.Constants;
using GestionPersonnelMairie.Core.DTOs;
using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Infrastructure.Auth;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services;

public class AuthService
{
    private const int MaxTentatives = 5;
    private const int DureeBlocageMinutes = 30;

    private readonly AppDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(AppDbContext context, JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public LoginResult Login(string email, string motPasse, string? adresseIp = null)
    {
        var utilisateur = _context.Utilisateurs
            .Include(u => u.Role)
            .Include(u => u.Agent)
            .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

        if (utilisateur == null)
        {
            Journaliser(null, email, false, adresseIp, "Email inconnu");
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");
        }

        if (!utilisateur.EstActif)
        {
            Journaliser(utilisateur.IdUtilisateur, email, false, adresseIp, "Compte désactivé");
            throw new UnauthorizedAccessException("Ce compte est désactivé. Contactez l'administrateur.");
        }

        if (!utilisateur.EmailVerifie)
        {
            Journaliser(utilisateur.IdUtilisateur, email, false, adresseIp, "Email non vérifié");
            throw new UnauthorizedAccessException(
                "Votre compte n'est pas encore activé. Vérifiez votre email et cliquez sur le lien d'activation.");
        }

        if (utilisateur.DateBlocage.HasValue && utilisateur.DateBlocage > DateTime.UtcNow)
        {
            var minutes = (int)Math.Ceiling((utilisateur.DateBlocage.Value - DateTime.UtcNow).TotalMinutes);
            Journaliser(utilisateur.IdUtilisateur, email, false, adresseIp, "Compte bloqué");
            throw new UnauthorizedAccessException($"Compte bloqué. Réessayez dans {minutes} minute(s).");
        }

        if (utilisateur.DateBlocage.HasValue && utilisateur.DateBlocage <= DateTime.UtcNow)
        {
            utilisateur.DateBlocage = null;
            utilisateur.TentativesEchouees = 0;
        }

        if (!VerifierMotDePasse(utilisateur, motPasse))
        {
            utilisateur.TentativesEchouees++;
            if (utilisateur.TentativesEchouees >= MaxTentatives)
            {
                utilisateur.DateBlocage = DateTime.UtcNow.AddMinutes(DureeBlocageMinutes);
                utilisateur.TentativesEchouees = 0;
            }
            _context.SaveChanges();
            Journaliser(utilisateur.IdUtilisateur, email, false, adresseIp, "Mot de passe incorrect");
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");
        }

        utilisateur.TentativesEchouees = 0;
        utilisateur.DateBlocage = null;
        _context.SaveChanges();

        var (token, expiresAt) = _jwtTokenService.GenerateToken(utilisateur);
        Journaliser(utilisateur.IdUtilisateur, email, true, adresseIp, "Connexion réussie");

        return new LoginResult
        {
            Utilisateur = utilisateur,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public void Inscrire(string nom, string email, string motPasse)
    {
        if (_context.Utilisateurs.Any(u => u.Email.ToLower() == email.ToLower()))
            throw new InvalidOperationException("Un compte existe déjà avec cet email.");

        var agent = _context.Agents.FirstOrDefault(a => a.Email.ToLower() == email.ToLower())
            ?? throw new InvalidOperationException("Aucun agent trouvé avec cet email. Contactez l'administrateur.");

        var roleDefaut = _context.Roles.FirstOrDefault(r => r.NomRole == Roles.Agent)
            ?? throw new InvalidOperationException("Rôle introuvable. Contactez l'administrateur.");

        _context.Utilisateurs.Add(new Utilisateur
        {
            Nom = nom,
            Email = email,
            MotPasse = BCrypt.Net.BCrypt.HashPassword(motPasse),
            IdRole = roleDefaut.IdRole,
            IdAgent = agent.Id,
            EstActif = true,
            EmailVerifie = true
        });
        _context.SaveChanges();
    }

    public bool EmailExiste(string email) =>
        _context.Utilisateurs.Any(u => u.Email.ToLower() == email.ToLower());

    public Task<string> GenererTokenReinitialisationAsync(string email)
    {
        var utilisateur = _context.Utilisateurs
            .FirstOrDefault(u => u.Email.ToLower() == email.ToLower())
            ?? throw new InvalidOperationException("Utilisateur introuvable.");

        var token = Random.Shared.Next(100000, 999999).ToString();
        utilisateur.ResetToken = token;
        utilisateur.ResetTokenExpiration = DateTime.UtcNow.AddMinutes(30);
        _context.SaveChanges();
        return Task.FromResult(token);
    }

    public bool ValiderToken(string email, string token)
    {
        var utilisateur = _context.Utilisateurs
            .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

        if (utilisateur?.ResetToken == null || utilisateur.ResetTokenExpiration == null)
            return false;

        return DateTime.UtcNow <= utilisateur.ResetTokenExpiration
               && utilisateur.ResetToken == token;
    }

    public void ReinitialisMotDePasse(string email, string nouveauMotPasse)
    {
        var utilisateur = _context.Utilisateurs
            .FirstOrDefault(u => u.Email.ToLower() == email.ToLower())
            ?? throw new InvalidOperationException("Utilisateur introuvable.");

        utilisateur.MotPasse = BCrypt.Net.BCrypt.HashPassword(nouveauMotPasse);
        utilisateur.ResetToken = null;
        utilisateur.ResetTokenExpiration = null;
        utilisateur.TentativesEchouees = 0;
        utilisateur.DateBlocage = null;
        _context.SaveChanges();
    }

    public void ChangerMotDePasse(int idUtilisateur, string ancienMdp, string nouveauMdp)
    {
        var utilisateur = _context.Utilisateurs.Find(idUtilisateur)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");

        if (!VerifierMotDePasse(utilisateur, ancienMdp, hashOnSuccess: false))
            throw new InvalidOperationException("Ancien mot de passe incorrect.");

        utilisateur.MotPasse = BCrypt.Net.BCrypt.HashPassword(nouveauMdp);
        _context.SaveChanges();
    }

    private bool VerifierMotDePasse(Utilisateur utilisateur, string motPasse, bool hashOnSuccess = true)
    {
        if (utilisateur.MotPasse.StartsWith("$2"))
            return BCrypt.Net.BCrypt.Verify(motPasse, utilisateur.MotPasse);

        var valide = utilisateur.MotPasse == motPasse;
        if (valide && hashOnSuccess)
        {
            utilisateur.MotPasse = BCrypt.Net.BCrypt.HashPassword(motPasse);
            _context.SaveChanges();
        }
        return valide;
    }

    private void Journaliser(int? idUtilisateur, string email, bool reussi, string? ip, string message)
    {
        _context.JournauxConnexion.Add(new JournalConnexion
        {
            IdUtilisateur = idUtilisateur,
            Email = email,
            Reussi = reussi,
            AdresseIp = ip,
            Message = message
        });
        _context.SaveChanges();
    }
}
