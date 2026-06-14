using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        // ─── Connexion ────────────────────────────────────────────────────────────
        public Utilisateur? Login(string email, string motPasse)
        {
            return _context.Utilisateurs
                .Include(u => u.Role)
                .Include(u => u.Agent)
                .FirstOrDefault(u => u.Email == email && u.MotPasse == motPasse);
        }

        // ─── Inscription ──────────────────────────────────────────────────────────
        public void Inscrire(string nom, string email, string motPasse)
        {
            var compteExiste = _context.Utilisateurs
                .Any(u => u.Email.ToLower() == email.ToLower());

            if (compteExiste)
                throw new Exception("Un compte existe déjà avec cet email.");

            var agent = _context.Agents
                .FirstOrDefault(a => a.Email.ToLower() == email.ToLower());

            if (agent == null)
                throw new Exception("Aucun agent trouvé avec cet email. Contactez l'administrateur.");

            var roleDefaut = _context.Roles
                .FirstOrDefault(r => r.NomRole.ToLower() == "employé" || r.NomRole.ToLower() == "employe");

            if (roleDefaut == null)
                throw new Exception("Le rôle par défaut 'Employé' est introuvable. Contactez l'administrateur.");

            var nouvelUtilisateur = new Utilisateur
            {
                Nom = nom,
                Email = email,
                MotPasse = motPasse,
                IdRole = roleDefaut.IdRole,
                IdAgent = agent.Id  // ✅ Corrigé : agent.Id et non agent.IdAgent
            };

            _context.Utilisateurs.Add(nouvelUtilisateur);
            _context.SaveChanges();
        }

        // ─── Vérifie si un email existe (compte utilisateur) ─────────────────────
        public bool EmailExiste(string email)
        {
            return _context.Utilisateurs
                .Any(u => u.Email.ToLower() == email.ToLower());
        }

        // ─── Génère un token de réinitialisation (valable 30 min) ────────────────
        public Task<string> GenererTokenReinitialisationAsync(string email)
        {
            var utilisateur = _context.Utilisateurs
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (utilisateur == null)
                throw new Exception("Utilisateur introuvable.");

            var random = new Random();
            var token = random.Next(100000, 999999).ToString();

            utilisateur.ResetToken = token;
            utilisateur.ResetTokenExpiration = DateTime.UtcNow.AddMinutes(30);
            _context.SaveChanges();

            return Task.FromResult(token);
        }

        // ─── Vérifie que le token saisi est valide ────────────────────────────────
        public bool ValiderToken(string email, string token)
        {
            var utilisateur = _context.Utilisateurs
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (utilisateur == null) return false;
            if (utilisateur.ResetToken == null) return false;
            if (utilisateur.ResetTokenExpiration == null) return false;
            if (DateTime.UtcNow > utilisateur.ResetTokenExpiration) return false;
            if (utilisateur.ResetToken != token) return false;

            return true;
        }

        // ─── Réinitialise le mot de passe ─────────────────────────────────────────
        public void ReinitialisMotDePasse(string email, string nouveauMotPasse)
        {
            var utilisateur = _context.Utilisateurs
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (utilisateur == null)
                throw new Exception("Utilisateur introuvable.");

            utilisateur.MotPasse = nouveauMotPasse;
            utilisateur.ResetToken = null;
            utilisateur.ResetTokenExpiration = null;
            _context.SaveChanges();
        }
    }
}