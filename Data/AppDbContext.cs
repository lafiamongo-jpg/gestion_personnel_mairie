using Microsoft.EntityFrameworkCore;
using GestionPersonnelMairie.Models;


namespace GestionPersonnelMairie.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Poste> Postes { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<DemandeConge> DemandesConge { get; set; }
        public DbSet<Conge> Conges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agent>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Agents)
                .HasForeignKey(a => a.IdService);

            modelBuilder.Entity<Agent>()
                .HasOne(a => a.Poste)
                .WithMany(p => p.Agents)
                .HasForeignKey(a => a.IdPoste);

            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Utilisateurs)
                .HasForeignKey(u => u.IdRole);

            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.Agent)
                .WithOne(a => a.Utilisateur)
                .HasForeignKey<Utilisateur>(u => u.IdAgent);

            modelBuilder.Entity<DemandeConge>()
                .HasOne(d => d.Agent)
                .WithMany(a => a.DemandesConge)
                .HasForeignKey(d => d.IdAgent);

            modelBuilder.Entity<Conge>()
                .HasOne(c => c.DemandeConge)
                .WithOne(d => d.Conge)
                .HasForeignKey<Conge>(c => c.IdDemande);
        }
    }
}

