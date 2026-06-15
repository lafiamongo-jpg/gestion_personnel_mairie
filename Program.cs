using System.Text;
using GestionPersonnelMairie.Components;
using GestionPersonnelMairie.Core.Constants;
using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Infrastructure.Auth;
using GestionPersonnelMairie.Core.Enums;
using GestionPersonnelMairie.Models;
using GestionPersonnelMairie.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ─── Base de données ─────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=GestionPersonnel.db";
var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException(
            "MySQL n'est pas encore disponible (Pomelo EF Core 10 en attente de sortie). " +
            "Utilisez DatabaseProvider=Sqlite dans appsettings.json.");
    options.UseSqlite(connectionString);
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

// ─── JWT ─────────────────────────────────────────────────────────────────────
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtTokenService>();

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? "GestionPersonnelMairie_SuperSecret_Key_2026_Min32Chars!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GestionPersonnelMairie",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GestionPersonnelMairie",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy(Policies.RequireAgent, p =>
        p.RequireRole(Roles.Agent, Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy(Policies.RequireAdmin, p =>
        p.RequireRole(Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy(Policies.RequireSuperAdmin, p =>
        p.RequireRole(Roles.SuperAdmin));
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireAgent, p =>
        p.RequireRole(Roles.Agent, Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy(Policies.RequireAdmin, p =>
        p.RequireRole(Roles.Admin, Roles.SuperAdmin));
    options.AddPolicy(Policies.RequireSuperAdmin, p =>
        p.RequireRole(Roles.SuperAdmin));
});

builder.Services.Configure<GestionPersonnelMairie.Infrastructure.Email.SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));

// ─── Services métier ─────────────────────────────────────────────────────────
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<CongeService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UtilisateurService>();
builder.Services.AddScoped<ServiceService>();
builder.Services.AddScoped<PosteService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddSingleton<AuthStateService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ─── Seed ────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    if (!context.Services.Any())
    {
        context.Services.AddRange(
            new Service { NomService = "Ressources Humaines", Description = "Gestion du personnel" },
            new Service { NomService = "Comptabilité", Description = "Gestion financière" },
            new Service { NomService = "Informatique", Description = "Gestion des systèmes" },
            new Service { NomService = "Administration", Description = "Gestion administrative" },
            new Service { NomService = "Technique", Description = "Services techniques" }
        );
        context.SaveChanges();
    }

    var premierService = context.Services.First();

    if (!context.Postes.Any())
    {
        context.Postes.AddRange(
            new Poste { NomPoste = "Directeur", Description = "Direction générale", IdService = premierService.IdService, NbPostes = 1 },
            new Poste { NomPoste = "Chef de service", Description = "Responsable de service", IdService = premierService.IdService, NbPostes = 2 },
            new Poste { NomPoste = "Agent administratif", Description = "Tâches administratives", IdService = premierService.IdService, NbPostes = 5 },
            new Poste { NomPoste = "Technicien", Description = "Support technique", IdService = premierService.IdService, NbPostes = 3 },
            new Poste { NomPoste = "Comptable", Description = "Gestion comptable", IdService = premierService.IdService, NbPostes = 2 },
            new Poste { NomPoste = "Secrétaire", Description = "Secrétariat", IdService = premierService.IdService, NbPostes = 2 }
        );
        context.SaveChanges();
    }

    if (!context.Roles.Any())
    {
        context.Roles.AddRange(
            new Role { NomRole = Roles.Agent, Description = "Accès à son dossier et ses congés" },
            new Role { NomRole = Roles.Admin, Description = "Gestion des services et validation des congés" },
            new Role { NomRole = Roles.SuperAdmin, Description = "Accès total au système" }
        );
        context.SaveChanges();
    }

    if (!context.Utilisateurs.Any())
    {
        var roleSuperAdmin = context.Roles.First(r => r.NomRole == Roles.SuperAdmin);

        Agent agentAdmin;
        if (!context.Agents.Any())
        {
            agentAdmin = new Agent
            {
                Nom = "Admin",
                Prenom = "Super",
                Email = "admin@mairie.bj",
                Telephone = "00000000",
                Adresse = "Banikoara, Alibori",
                Departement = "Alibori",
                DateEmbauche = DateTime.Today,
                Salaire = 0,
                Statut = StatutAgent.Actif,
                IdService = premierService.IdService,
                IdPoste = context.Postes.First().IdPoste
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
            Nom = "Super Admin",
            Email = "admin@mairie.bj",
            MotPasse = BCrypt.Net.BCrypt.HashPassword("admin123"),
            IdRole = roleSuperAdmin.IdRole,
            IdAgent = agentAdmin.Id,
            EstActif = true,
            EmailVerifie = true
        });
        context.SaveChanges();
    }
    else
    {
        // Migration des rôles legacy
        var adminLegacy = context.Roles.FirstOrDefault(r => r.NomRole == "Administrateur");
        if (adminLegacy != null)
        {
            adminLegacy.NomRole = Roles.SuperAdmin;
            adminLegacy.Description = "Accès total au système";
            context.SaveChanges();
        }

        if (!context.Roles.Any(r => r.NomRole == Roles.Admin))
        {
            context.Roles.Add(new Role { NomRole = Roles.Admin, Description = "Gestion des services et validation des congés" });
            context.SaveChanges();
        }

        // Migration mots de passe plain-text
        var utilisateursLegacy = context.Utilisateurs
            .Where(u => !u.MotPasse.StartsWith("$2"))
            .ToList();
        foreach (var u in utilisateursLegacy)
            u.MotPasse = BCrypt.Net.BCrypt.HashPassword(u.MotPasse);
        if (utilisateursLegacy.Any())
            context.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.ContentType = "text/html";
        context.HttpContext.Response.Redirect("/not-found");
    }
});
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

