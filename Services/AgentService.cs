using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services
{
    public class AgentService
    {
        private readonly AppDbContext _context;

        public AgentService(AppDbContext context)
        {
            _context = context;
        }

        public List<Agent> GetAll()
        {
            return _context.Agents
                .Include(a => a.Service)
                .Include(a => a.Poste)
                .AsNoTracking()
                .ToList();
        }

        public Agent? GetById(int id)
        {
            return _context.Agents
                .Include(a => a.Service)
                .Include(a => a.Poste)
                .AsNoTracking()
                .FirstOrDefault(a => a.Id == id);
        }

        public void Add(Agent agent)
        {
            agent.Service = null!;
            agent.Poste = null!;
            _context.Agents.Add(agent);
            _context.SaveChanges();

            // Créer automatiquement un utilisateur pour cet agent
            var role = _context.Roles.FirstOrDefault();
            if (role != null)
            {
                // Vérifier si un utilisateur avec cet email n'existe pas déjà
                var existant = _context.Utilisateurs.FirstOrDefault(u => u.Email == agent.Email);
                if (existant == null)
                {
                    var utilisateur = new Utilisateur
                    {
                        Nom = agent.Nom + " " + agent.Prenom,
                        Email = agent.Email,
                        MotPasse = "mairie123",
                        IdRole = role.IdRole,
                        IdAgent = agent.Id
                    };
                    _context.Utilisateurs.Add(utilisateur);
                    _context.SaveChanges();
                }
            }
        }

        public void Update(Agent agent)
        {
            agent.Service = null!;
            agent.Poste = null!;
            _context.Agents.Update(agent);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var agent = _context.Agents.Find(id);
            if (agent != null)
            {
                _context.Agents.Remove(agent);
                _context.SaveChanges();
            }
        }

        public List<Service> GetServices()
        {
            return _context.Services.AsNoTracking().ToList();
        }

        public List<Poste> GetPostes()
        {
            return _context.Postes.AsNoTracking().ToList();
        }
    }
}