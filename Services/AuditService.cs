using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services;

public class AuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public void Log(int? idUtilisateur, string action, string entite, string? entiteId = null, string? details = null)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            IdUtilisateur = idUtilisateur,
            Action = action,
            Entite = entite,
            EntiteId = entiteId,
            Details = details
        });
        _context.SaveChanges();
    }

    public List<JournalConnexion> GetJournauxConnexion(int limit = 100) =>
        _context.JournauxConnexion
            .Include(j => j.Utilisateur)
            .AsNoTracking()
            .OrderByDescending(j => j.DateConnexion)
            .Take(limit)
            .ToList();

    public List<AuditLog> GetAuditLogs(int limit = 100) =>
        _context.AuditLogs
            .Include(a => a.Utilisateur)
            .AsNoTracking()
            .OrderByDescending(a => a.DateAction)
            .Take(limit)
            .ToList();
}
