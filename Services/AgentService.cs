using GestionPersonnelMairie.Core.Constants;
using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services;

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
            .OrderBy(a => a.Nom)
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

    public List<Agent> Search(string? recherche, int? idService, int? idPoste, string? statut)
    {
        var query = _context.Agents
            .Include(a => a.Service)
            .Include(a => a.Poste)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            query = query.Where(a =>
                a.Nom.Contains(recherche) ||
                a.Prenom.Contains(recherche) ||
                a.Email.Contains(recherche));
        }

        if (idService.HasValue && idService > 0)
            query = query.Where(a => a.IdService == idService);

        if (idPoste.HasValue && idPoste > 0)
            query = query.Where(a => a.IdPoste == idPoste);

        if (!string.IsNullOrWhiteSpace(statut))
            query = query.Where(a => a.Statut == statut);

        return query.OrderBy(a => a.Nom).ToList();
    }

    public void Add(Agent agent)
    {
        if (string.IsNullOrWhiteSpace(agent.Statut))
            agent.Statut = Core.Enums.StatutAgent.Actif;

        agent.Service = null!;
        agent.Poste = null!;
        _context.Agents.Add(agent);
        _context.SaveChanges();
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
        var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.IdAgent == id);
        if (utilisateur != null)
            _context.Utilisateurs.Remove(utilisateur);

        var agent = _context.Agents.Find(id);
        if (agent != null)
        {
            _context.Agents.Remove(agent);
            _context.SaveChanges();
        }
    }

    public bool ACompteUtilisateur(int idAgent) =>
        _context.Utilisateurs.Any(u => u.IdAgent == idAgent);

    public void AffecterService(int idAgent, int idService)
    {
        var agent = _context.Agents.Find(idAgent)
            ?? throw new InvalidOperationException("Agent introuvable.");
        agent.IdService = idService;
        _context.SaveChanges();
    }

    public List<Service> GetServices() =>
        _context.Services.AsNoTracking().OrderBy(s => s.NomService).ToList();

    public List<Poste> GetPostes(int? idService = null)
    {
        var query = _context.Postes.Include(p => p.Service).AsNoTracking().AsQueryable();
        if (idService.HasValue && idService > 0)
            query = query.Where(p => p.IdService == idService);
        return query.OrderBy(p => p.NomPoste).ToList();
    }
}
