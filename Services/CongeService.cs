using GestionPersonnelMairie.Core.Constants;
using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services;

public class CongeService
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public CongeService(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public List<DemandeConge> GetAllDemandes()
    {
        return _context.DemandesConge
            .Include(d => d.Agent)
            .Include(d => d.Conge)
            .AsNoTracking()
            .OrderByDescending(d => d.DateDemande)
            .ToList();
    }

    public List<DemandeConge> GetDemandesByAgent(int idAgent)
    {
        return _context.DemandesConge
            .Include(d => d.Agent)
            .Include(d => d.Conge)
            .AsNoTracking()
            .Where(d => d.IdAgent == idAgent)
            .OrderByDescending(d => d.DateDemande)
            .ToList();
    }

    public async Task SoumettreDemande(DemandeConge demande)
    {
        if (!demande.DateDebut.HasValue || !demande.DateFin.HasValue)
            throw new InvalidOperationException("Les dates de début et de fin sont obligatoires.");

        if (demande.DateFin < demande.DateDebut)
            throw new InvalidOperationException("La date de fin doit être postérieure à la date de début.");

        var agent = _context.Agents.Find(demande.IdAgent)
            ?? throw new InvalidOperationException("Agent introuvable.");

        var duree = CalculerJoursOuvres(demande.DateDebut.Value, demande.DateFin.Value);

        if (duree <= 0)
            throw new InvalidOperationException("La période doit contenir au moins un jour ouvré.");

        if (duree > agent.SoldeCongeRestant)
            throw new InvalidOperationException($"Solde insuffisant. Restant : {agent.SoldeCongeRestant} jour(s).");

        if (DetecterChevauchement(demande.IdAgent, demande.DateDebut.Value, demande.DateFin.Value))
            throw new InvalidOperationException("Cette période chevauche un congé déjà approuvé.");

        demande.Agent = null!;
        demande.DateDemande = DateTime.Now;
        demande.Statut = "attente";
        _context.DemandesConge.Add(demande);
        _context.SaveChanges();

        // Notifier le SuperAdmin par email
        var superAdmins = _context.Utilisateurs
            .Include(u => u.Role)
            .Where(u => u.Role.NomRole == Roles.SuperAdmin && u.EstActif)
            .ToList();
        foreach (var admin in superAdmins)
        {
            _ = _emailService.EnvoyerNotifNouvelleDemandeSuperAdminAsync(
                admin.Email, admin.Nom, agent.NomComplet,
                demande.DateDebut.Value, demande.DateFin.Value,
                demande.TypeConge ?? "Congé", demande.Commentaire);
        }
    }

    public async Task Approuver(int idDemande, DateTime dateDebut, DateTime dateFin, string typeConge)
    {
        var demande = _context.DemandesConge
            .Include(d => d.Agent)
            .FirstOrDefault(d => d.IdDemande == idDemande)
            ?? throw new InvalidOperationException("Demande introuvable.");

        if (demande.Statut != "attente")
            throw new InvalidOperationException("Cette demande a déjà été traitée.");

        var duree = CalculerJoursOuvres(dateDebut, dateFin);
        if (duree > demande.Agent.SoldeCongeRestant)
            throw new InvalidOperationException($"Solde insuffisant pour cet agent ({demande.Agent.SoldeCongeRestant} jour(s) restants).");

        if (DetecterChevauchement(demande.IdAgent, dateDebut, dateFin, idDemande))
            throw new InvalidOperationException("Cette période chevauche un congé déjà approuvé.");

        demande.Statut = "approuve";
        demande.DateDebut = dateDebut;
        demande.DateFin = dateFin;
        demande.TypeConge = typeConge;

        var conge = new Conge
        {
            DateDebut = dateDebut,
            DateFin = dateFin,
            Duree = duree,
            TypeConge = typeConge,
            Statut = "en_cours",
            IdDemande = idDemande
        };

        demande.Agent.SoldeCongeRestant -= duree;
        if (demande.Agent.Statut != Core.Enums.StatutAgent.EnConge)
            demande.Agent.Statut = Core.Enums.StatutAgent.EnConge;

        _context.Conges.Add(conge);
        _context.SaveChanges();

        // Notifier l'agent par email
        var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.IdAgent == demande.IdAgent);
        if (utilisateur != null)
        {
            _ = _emailService.EnvoyerNotifCongeAgentAsync(
                utilisateur.Email, demande.Agent.NomComplet, approuve: true,
                dateDebut, dateFin, duree, typeConge, null);
        }
    }

    public async Task Refuser(int idDemande, string commentaire)
    {
        var demande = _context.DemandesConge
            .Include(d => d.Agent)
            .FirstOrDefault(d => d.IdDemande == idDemande)
            ?? throw new InvalidOperationException("Demande introuvable.");

        demande.Statut = "refuse";
        demande.Commentaire = commentaire;
        _context.SaveChanges();

        // Notifier l'agent par email
        var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.IdAgent == demande.IdAgent);
        if (utilisateur != null && demande.DateDebut.HasValue && demande.DateFin.HasValue)
        {
            _ = _emailService.EnvoyerNotifCongeAgentAsync(
                utilisateur.Email, demande.Agent.NomComplet, approuve: false,
                demande.DateDebut.Value, demande.DateFin.Value, 0,
                demande.TypeConge ?? "Congé", commentaire);
        }
    }

    public void SupprimerDemande(int idDemande)
    {
        var demande = _context.DemandesConge.Find(idDemande);
        if (demande != null)
        {
            _context.DemandesConge.Remove(demande);
            _context.SaveChanges();
        }
    }

    public static int CalculerJoursOuvres(DateTime debut, DateTime fin)
    {
        if (fin < debut) return 0;

        var jours = 0;
        for (var date = debut.Date; date <= fin.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;
            jours++;
        }
        return jours;
    }

    private bool DetecterChevauchement(int idAgent, DateTime debut, DateTime fin, int? exclureDemandeId = null)
    {
        var query = _context.DemandesConge
            .Include(d => d.Conge)
            .Where(d => d.IdAgent == idAgent && d.Statut == "approuve");

        if (exclureDemandeId.HasValue)
            query = query.Where(d => d.IdDemande != exclureDemandeId);

        return query.Any(d =>
            d.Conge != null &&
            d.Conge.DateDebut <= fin &&
            d.Conge.DateFin >= debut);
    }
}
