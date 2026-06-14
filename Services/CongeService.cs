using GestionPersonnelMairie.Data;
using GestionPersonnelMairie.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPersonnelMairie.Services
{
    public class CongeService
    {
        private readonly AppDbContext _context;

        public CongeService(AppDbContext context)
        {
            _context = context;
        }

        // Récupérer toutes les demandes
        public List<DemandeConge> GetAllDemandes()
        {
            return _context.DemandesConge
                .Include(d => d.Agent)
                .Include(d => d.Conge)
                .AsNoTracking()
                .OrderByDescending(d => d.DateDemande)
                .ToList();
        }

        // Récupérer les demandes d'un agent
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

        // Soumettre une demande
        public void SoumettreDemande(DemandeConge demande)
        {
            demande.Agent = null!;
            demande.DateDemande = DateTime.Now;
            demande.Statut = "attente";
            _context.DemandesConge.Add(demande);
            _context.SaveChanges();
        }

        // Approuver une demande
        public void Approuver(int idDemande, DateTime dateDebut, DateTime dateFin, string typeConge)
        {
            var demande = _context.DemandesConge.Find(idDemande);
            if (demande == null) return;

            demande.Statut = "approuve";
            _context.DemandesConge.Update(demande);

            var duree = (int)(dateFin - dateDebut).TotalDays + 1;
            var conge = new Conge
            {
                DateDebut = dateDebut,
                DateFin = dateFin,
                Duree = duree,
                TypeConge = typeConge,
                Statut = "en_cours",
                IdDemande = idDemande
            };
            _context.Conges.Add(conge);
            _context.SaveChanges();
        }

        // Refuser une demande
        public void Refuser(int idDemande, string commentaire)
        {
            var demande = _context.DemandesConge.Find(idDemande);
            if (demande == null) return;

            demande.Statut = "refuse";
            demande.Commentaire = commentaire;
            _context.DemandesConge.Update(demande);
            _context.SaveChanges();
        }

        // Supprimer une demande
        public void SupprimerDemande(int idDemande)
        {
            var demande = _context.DemandesConge.Find(idDemande);
            if (demande != null)
            {
                _context.DemandesConge.Remove(demande);
                _context.SaveChanges();
            }
        }
    }
}