using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services
{
    public class UtilisateurService
    {
        private readonly AppDbContext _context;

        public UtilisateurService(AppDbContext context)
        {
            _context = context;
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

        public List<Role> GetRoles()
        {
            return _context.Roles.AsNoTracking().ToList();
        }

        public void Update(int idUtilisateur, string nom, string email, int idRole, string? nouveauMdp)
        {
            var u = _context.Utilisateurs.Find(idUtilisateur);
            if (u == null) return;

            u.Nom = nom;
            u.Email = email;
            u.IdRole = idRole;

            if (!string.IsNullOrWhiteSpace(nouveauMdp))
                u.MotPasse = BCrypt.Net.BCrypt.HashPassword(nouveauMdp);

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
    }
}
