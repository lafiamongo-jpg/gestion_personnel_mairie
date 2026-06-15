using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPersonnelMairie.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRole = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomRole = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Roles", x => x.IdRole));

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    IdService = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomService = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Services", x => x.IdService));

            migrationBuilder.CreateTable(
                name: "Postes",
                columns: table => new
                {
                    IdPoste = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomPoste = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IdService = table.Column<int>(type: "INTEGER", nullable: false),
                    NbPostes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postes", x => x.IdPoste);
                    table.ForeignKey(
                        name: "FK_Postes_Services_IdService",
                        column: x => x.IdService,
                        principalTable: "Services",
                        principalColumn: "IdService",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", nullable: false),
                    Adresse = table.Column<string>(type: "TEXT", nullable: false),
                    Sexe = table.Column<string>(type: "TEXT", nullable: true),
                    DateNaissance = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Departement = table.Column<string>(type: "TEXT", nullable: false),
                    DateEmbauche = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Salaire = table.Column<decimal>(type: "TEXT", nullable: false),
                    Statut = table.Column<string>(type: "TEXT", nullable: false),
                    PhotoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SoldeCongeAnnuel = table.Column<int>(type: "INTEGER", nullable: false),
                    SoldeCongeRestant = table.Column<int>(type: "INTEGER", nullable: false),
                    IdService = table.Column<int>(type: "INTEGER", nullable: false),
                    IdPoste = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_Postes_IdPoste",
                        column: x => x.IdPoste,
                        principalTable: "Postes",
                        principalColumn: "IdPoste",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agents_Services_IdService",
                        column: x => x.IdService,
                        principalTable: "Services",
                        principalColumn: "IdService",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    IdUtilisateur = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    MotPasse = table.Column<string>(type: "TEXT", nullable: false),
                    EstActif = table.Column<bool>(type: "INTEGER", nullable: false),
                    TentativesEchouees = table.Column<int>(type: "INTEGER", nullable: false),
                    DateBlocage = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResetToken = table.Column<string>(type: "TEXT", nullable: true),
                    ResetTokenExpiration = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdRole = table.Column<int>(type: "INTEGER", nullable: false),
                    IdAgent = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.IdUtilisateur);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Agents_IdAgent",
                        column: x => x.IdAgent,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Roles_IdRole",
                        column: x => x.IdRole,
                        principalTable: "Roles",
                        principalColumn: "IdRole",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdUtilisateur = table.Column<int>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Entite = table.Column<string>(type: "TEXT", nullable: false),
                    EntiteId = table.Column<string>(type: "TEXT", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    DateAction = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DemandesConge",
                columns: table => new
                {
                    IdDemande = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateDemande = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TypeConge = table.Column<string>(type: "TEXT", nullable: true),
                    Statut = table.Column<string>(type: "TEXT", nullable: false),
                    Commentaire = table.Column<string>(type: "TEXT", nullable: true),
                    IdAgent = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesConge", x => x.IdDemande);
                    table.ForeignKey(
                        name: "FK_DemandesConge_Agents_IdAgent",
                        column: x => x.IdAgent,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournauxConnexion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdUtilisateur = table.Column<int>(type: "INTEGER", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Reussi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AdresseIp = table.Column<string>(type: "TEXT", nullable: true),
                    DateConnexion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournauxConnexion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournauxConnexion_Utilisateurs_IdUtilisateur",
                        column: x => x.IdUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Conges",
                columns: table => new
                {
                    IdConge = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duree = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeConge = table.Column<string>(type: "TEXT", nullable: true),
                    Statut = table.Column<string>(type: "TEXT", nullable: false),
                    IdDemande = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conges", x => x.IdConge);
                    table.ForeignKey(
                        name: "FK_Conges_DemandesConge_IdDemande",
                        column: x => x.IdDemande,
                        principalTable: "DemandesConge",
                        principalColumn: "IdDemande",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_Agents_IdPoste", table: "Agents", column: "IdPoste");
            migrationBuilder.CreateIndex(name: "IX_Agents_IdService", table: "Agents", column: "IdService");
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_IdUtilisateur", table: "AuditLogs", column: "IdUtilisateur");
            migrationBuilder.CreateIndex(name: "IX_Conges_IdDemande", table: "Conges", column: "IdDemande", unique: true);
            migrationBuilder.CreateIndex(name: "IX_DemandesConge_IdAgent", table: "DemandesConge", column: "IdAgent");
            migrationBuilder.CreateIndex(name: "IX_JournauxConnexion_IdUtilisateur", table: "JournauxConnexion", column: "IdUtilisateur");
            migrationBuilder.CreateIndex(name: "IX_Postes_IdService", table: "Postes", column: "IdService");
            migrationBuilder.CreateIndex(name: "IX_Utilisateurs_IdAgent", table: "Utilisateurs", column: "IdAgent", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Utilisateurs_IdRole", table: "Utilisateurs", column: "IdRole");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "Conges");
            migrationBuilder.DropTable(name: "JournauxConnexion");
            migrationBuilder.DropTable(name: "DemandesConge");
            migrationBuilder.DropTable(name: "Utilisateurs");
            migrationBuilder.DropTable(name: "Agents");
            migrationBuilder.DropTable(name: "Postes");
            migrationBuilder.DropTable(name: "Roles");
            migrationBuilder.DropTable(name: "Services");
        }
    }
}
