using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using GestionPersonnelMairie.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Infrastructure.Background;

/// <summary>
/// Service qui tourne en arrière-plan et envoie des rappels
/// automatiques aux agents dont le congé se termine dans 1 ou 2 jours.
/// </summary>
public class CongeReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CongeReminderService> _logger;

    // Vérification toutes les heures — se déclenche réellement une fois par jour grâce à _datesDejaSend
    private static readonly HashSet<string> _rappelsEnvoyesAujourdhui = new();
    private static DateTime _dernierNettoyage = DateTime.Today;

    public CongeReminderService(IServiceScopeFactory scopeFactory, ILogger<CongeReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CongeReminderService démarré.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnvoyerRappelsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans CongeReminderService");
            }

            // Attendre 1 heure avant la prochaine vérification
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task EnvoyerRappelsAsync()
    {
        var aujourd_hui = DateTime.Today;

        // Nettoyer les rappels de la veille
        if (_dernierNettoyage < aujourd_hui)
        {
            _rappelsEnvoyesAujourdhui.Clear();
            _dernierNettoyage = aujourd_hui;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        // Congés en cours se terminant dans 1 ou 2 jours
        var congesAVenir = await context.Conges
            .Include(c => c.DemandeConge)
                .ThenInclude(d => d.Agent)
            .Where(c => c.Statut == "en_cours" && !c.RetourAnticipe)
            .ToListAsync();

        foreach (var conge in congesAVenir)
        {
            var joursRestants = (conge.DateFin.Date - aujourd_hui).Days;

            if (joursRestants != 1 && joursRestants != 2)
                continue;

            var cleRappel = $"{conge.IdConge}-{joursRestants}-{aujourd_hui:yyyyMMdd}";
            if (_rappelsEnvoyesAujourdhui.Contains(cleRappel))
                continue;

            var agent = conge.DemandeConge?.Agent;
            if (agent == null) continue;

            // Trouver l'email de l'utilisateur lié à cet agent
            var utilisateur = await context.Utilisateurs
                .FirstOrDefaultAsync(u => u.IdAgent == agent.Id && u.EstActif);

            if (utilisateur == null) continue;

            _logger.LogInformation(
                "Envoi rappel fin congé à {Nom} ({Email}) — {Jours} jour(s) restant(s)",
                agent.NomComplet, utilisateur.Email, joursRestants);

            _ = emailService.EnvoyerRappelFinCongeAsync(
                utilisateur.Email, agent.NomComplet, conge.DateFin, joursRestants);

            _rappelsEnvoyesAujourdhui.Add(cleRappel);
        }
    }
}
