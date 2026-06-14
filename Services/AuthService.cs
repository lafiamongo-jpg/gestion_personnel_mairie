using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

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
            var utilisateur = _context.Utilisateurs
                .Include(u => u.Role)
                .Include(u => u.Agent)
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (utilisateur == null) return null;

            // Vérifier BCrypt ou plain-text (migration progressive)
            bool valide = false;
            if (utilisateur.MotPasse.StartsWith("$2"))
            {
                valide = BCrypt.Net.BCrypt.Verify(motPasse, utilisateur.MotPasse);
            }
            else
            {
                // Plain-text legacy — on hash maintenant
                valide = utilisateur.MotPasse == motPasse;
                if (valide)
                {
                    utilisateur.MotPasse = BCrypt.Net.BCrypt.HashPassword(motPasse);
                    _context.SaveChanges();
                }
            }

            return valide ? utilisateur : null;
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

            // Rôle "Agent" (corrigé)
            var roleDefaut = _context.Roles
                .FirstOrDefault(r => r.NomRole == "Agent");

            if (roleDefaut == null)
                throw new Exception("Rôle introuvable. Contactez l'administrateur.");

            var nouvelUtilisateur = new Utilisateur
            {
                Nom = nom,
                Email = email,
                MotPasse = BCrypt.Net.BCrypt.HashPassword(motPasse),
                IdRole = roleDefaut.IdRole,
                IdAgent = agent.Id
            };

            _context.Utilisateurs.Add(nouvelUtilisateur);
            _context.SaveChanges();
        }

        // ─── Vérifie si un email existe ───────────────────────────────────────────
        public bool EmailExiste(string email)
        {
            return _context.Utilisateurs
                .Any(u => u.Email.ToLower() == email.ToLower());
        }

        // ─── Génère un token OTP (valable 30 min) ─────────────────────────────────
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

        // ─── Valide le token OTP ──────────────────────────────────────────────────
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

            utilisateur.MotPasse = BCrypt.Net.BCrypt.HashPassword(nouveauMotPasse);
            utilisateur.ResetToken = null;
            utilisateur.ResetTokenExpiration = null;
            _context.SaveChanges();
        }

        // ─── Changer mot de passe (profil) ────────────────────────────────────────
        public void ChangerMotDePasse(int idUtilisateur, string ancienMdp, string nouveauMdp)
        {
            var utilisateur = _context.Utilisateurs.Find(idUtilisateur);
            if (utilisateur == null)
                throw new Exception("Utilisateur introuvable.");

            bool valide = utilisateur.MotPasse.StartsWith("$2")
                ? BCrypt.Net.BCrypt.Verify(ancienMdp, utilisateur.MotPasse)
                : utilisateur.MotPasse == ancienMdp;

            if (!valide)
                throw new Exception("Ancien mot de passe incorrect.");

            utilisateur.MotPasse = BCrypt.Net.BCrypt.HashPassword(nouveauMdp);
            _context.SaveChanges();
        }
    }
}
