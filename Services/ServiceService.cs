using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services;

public class ServiceService
{
    private readonly AppDbContext _context;

    public ServiceService(AppDbContext context)
    {
        _context = context;
    }

    public List<Service> GetAll() =>
        _context.Services
            .Include(s => s.Agents)
            .Include(s => s.Postes)
            .AsNoTracking()
            .OrderBy(s => s.NomService)
            .ToList();

    public Service? GetById(int id) =>
        _context.Services
            .Include(s => s.Agents)
            .Include(s => s.Postes)
            .AsNoTracking()
            .FirstOrDefault(s => s.IdService == id);

    public void Add(Service service)
    {
        _context.Services.Add(service);
        _context.SaveChanges();
    }

    public void Update(Service service)
    {
        _context.Services.Update(service);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var service = _context.Services
            .Include(s => s.Agents)
            .FirstOrDefault(s => s.IdService == id)
            ?? throw new InvalidOperationException("Service introuvable.");

        if (service.Agents.Any())
            throw new InvalidOperationException("Impossible de supprimer un service avec des agents affectés.");

        _context.Services.Remove(service);
        _context.SaveChanges();
    }
}
