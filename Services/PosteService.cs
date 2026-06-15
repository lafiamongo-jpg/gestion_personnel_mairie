using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services;

public class PosteService
{
    private readonly AppDbContext _context;

    public PosteService(AppDbContext context)
    {
        _context = context;
    }

    public List<Poste> GetAll() =>
        _context.Postes
            .Include(p => p.Service)
            .Include(p => p.Agents)
            .AsNoTracking()
            .OrderBy(p => p.NomPoste)
            .ToList();

    public void Add(Poste poste)
    {
        poste.Service = null!;
        _context.Postes.Add(poste);
        _context.SaveChanges();
    }

    public void Update(Poste poste)
    {
        poste.Service = null!;
        _context.Postes.Update(poste);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var poste = _context.Postes
            .Include(p => p.Agents)
            .FirstOrDefault(p => p.IdPoste == id)
            ?? throw new InvalidOperationException("Poste introuvable.");

        if (poste.Agents.Any())
            throw new InvalidOperationException("Impossible de supprimer un poste occupé.");

        _context.Postes.Remove(poste);
        _context.SaveChanges();
    }
}
