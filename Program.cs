using GestionPersonnelMairie.Components;
using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using GestionPersonnelMairie.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=GestionPersonnel.db"));

builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<CongeService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UtilisateurService>();
builder.Services.AddSingleton<AuthStateService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ─── Seed des données initiales ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (!context.Services.Any())
    {
        context.Services.AddRange(
            new Service { NomService = "Ressources Humaines",  Description = "Gestion du personnel" },
            new Service { NomService = "Comptabilité",         Description = "Gestion financière" },
            new Service { NomService = "Informatique",         Description = "Gestion des systèmes" },
            new Service { NomService = "Administration",       Description = "Gestion administrative" },
            new Service { NomService = "Technique",            Description = "Services techniques" }
        );
        context.SaveChanges();
    }

    if (!context.Postes.Any())
    {
        context.Postes.AddRange(
            new Poste { NomPoste = "Directeur",           Description = "Direction générale" },
            new Poste { NomPoste = "Chef de service",     Description = "Responsable de service" },
            new Poste { NomPoste = "Agent administratif", Description = "Tâches administratives" },
            new Poste { NomPoste = "Technicien",          Description = "Support technique" },
            new Poste { NomPoste = "Comptable",           Description = "Gestion comptable" },
            new Poste { NomPoste = "Secrétaire",          Description = "Secrétariat" }
        );
        context.SaveChanges();
    }

    if (!context.Roles.Any())
    {
        context.Roles.AddRange(
            new Role { NomRole = "Administrateur", Description = "Accès total au système" },
            new Role { NomRole = "Agent",          Description = "Accès limité à ses propres données" }
        );
        context.SaveChanges();
    }

    if (!context.Utilisateurs.Any())
    {
        var roleAdmin = context.Roles.First(r => r.NomRole == "Administrateur");

        Agent agentAdmin;
        if (!context.Agents.Any())
        {
            agentAdmin = new Agent
            {
                Nom        = "Admin",
                Prenom     = "Super",
                Email      = "admin@mairie.bj",
                Telephone  = "00000000",
                Departement = "Littoral",
                DateEmbauche = DateTime.Today,
                Salaire    = 0,
                Statut     = "Actif",
                IdService  = context.Services.First().IdService,
                IdPoste    = context.Postes.First().IdPoste
            };
            context.Agents.Add(agentAdmin);
            context.SaveChanges();
        }
        else
        {
            agentAdmin = context.Agents.First();
        }

        context.Utilisateurs.Add(new Utilisateur
        {
            Nom      = "Super Admin",
            Email    = "admin@mairie.bj",
            MotPasse = BCrypt.Net.BCrypt.HashPassword("admin123"),
            IdRole   = roleAdmin.IdRole,
            IdAgent  = agentAdmin.Id
        });
        context.SaveChanges();
    }
    else
    {
        // Migration des mots de passe plain-text existants vers BCrypt
        var utilisateursLegacy = context.Utilisateurs
            .Where(u => !u.MotPasse.StartsWith("$2"))
            .ToList();

        foreach (var u in utilisateursLegacy)
        {
            u.MotPasse = BCrypt.Net.BCrypt.HashPassword(u.MotPasse);
        }
        if (utilisateursLegacy.Any())
            context.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
