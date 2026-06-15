using System.Net;
using System.Net.Mail;
using GestionPersonnelMairie.Infrastructure.Email;
using Microsoft.Extensions.Options;

namespace GestionPersonnelMairie.Services;

public class EmailService
{
    private readonly SmtpSettings _smtp;
    private readonly ILogger<EmailService> _logger;
    private readonly string _baseUrl;

    public EmailService(IOptions<SmtpSettings> smtp, IConfiguration config, ILogger<EmailService> logger)
    {
        _smtp = smtp.Value;
        _logger = logger;
        _baseUrl = config["App:BaseUrl"] ?? "http://localhost:5282";
    }

    public string BuildVerificationLink(string token) =>
        $"{_baseUrl.TrimEnd('/')}/verifier-email?token={token}";

    public async Task<string> EnvoyerVerificationCompteAsync(
        string destinataire, string nom, string role, string token)
    {
        var lien = BuildVerificationLink(token);
        var sujet = "Activation de votre compte — Mairie de Banikoara";
        var corps = $"""
<!DOCTYPE html>
<html lang="fr">
<head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
<body style="margin:0;padding:0;background:#e4eaf2;font-family:Arial,sans-serif;">
  <table width="100%" cellpadding="0" cellspacing="0" style="background:#e4eaf2;padding:40px 0;">
    <tr><td align="center">
      <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.08);">
        <tr>
          <td style="background:#0f2137;padding:28px 32px;">
            <p style="margin:0;color:#c9a227;font-size:11px;letter-spacing:2px;text-transform:uppercase;">République du Bénin</p>
            <h1 style="margin:6px 0 0;color:#ffffff;font-size:20px;font-weight:700;">Mairie de Banikoara</h1>
          </td>
        </tr>
        <tr>
          <td style="padding:32px;">
            <p style="font-size:15px;color:#334155;margin-top:0;">Bonjour <strong>{nom}</strong>,</p>
            <p style="font-size:14px;color:#475569;line-height:1.6;">
              Un compte <strong>{role}</strong> a été créé pour vous sur le système de gestion du personnel de la Mairie de Banikoara.
            </p>
            <table cellpadding="0" cellspacing="0" style="background:#f0f4f8;border-left:4px solid #c9a227;border-radius:0 8px 8px 0;margin:20px 0;width:100%;">
              <tr><td style="padding:14px 16px;">
                <p style="margin:0;font-size:13px;color:#475569;">Mot de passe temporaire : <strong style="font-size:18px;color:#0f2137;letter-spacing:3px;">{Core.Constants.AppDefaults.MotDePasseInitial}</strong></p>
                <p style="margin:6px 0 0;font-size:12px;color:#64748b;">À modifier dès votre première connexion via <em>Mon profil</em>.</p>
              </td></tr>
            </table>
            <p style="font-size:14px;color:#475569;">Pour activer votre compte, cliquez sur le bouton ci-dessous :</p>
            <p style="text-align:center;margin:24px 0;">
              <a href="{lien}" style="background:#0f2137;color:#ffffff;padding:14px 32px;border-radius:8px;font-size:15px;font-weight:700;text-decoration:none;display:inline-block;">
                Activer mon compte
              </a>
            </p>
            <p style="font-size:12px;color:#94a3b8;text-align:center;">Ce lien est valable pendant <strong>48 heures</strong>.</p>
            <hr style="border:none;border-top:1px solid #e2e8f0;margin:24px 0;">
            <p style="font-size:11px;color:#94a3b8;text-align:center;margin:0;">
              Mairie de Banikoara — Service des Ressources Humaines<br>
              Commune de Banikoara, Département de l'Alibori, République du Bénin
            </p>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>
""";

        await EnvoyerAsync(destinataire, sujet, corps, isHtml: true);
        return lien;
    }

    public async Task EnvoyerNotifCongeAgentAsync(
        string email, string nom, bool approuve,
        DateTime debut, DateTime fin, int duree, string typeConge, string? commentaire)
    {
        var couleurStatut = approuve ? "#16a34a" : "#dc2626";
        var labelStatut  = approuve ? "APPROUVÉE" : "REFUSÉE";
        var iconStatut   = approuve ? "✅" : "❌";
        var messageCorps = approuve
            ? $"Votre demande de congé a été <strong>approuvée</strong>.<br>Vous bénéficierez de <strong>{duree} jour(s) ouvré(s)</strong> de congé."
            : $"Votre demande de congé a été <strong>refusée</strong>." +
              (string.IsNullOrWhiteSpace(commentaire) ? "" : $"<br><em>Motif : {commentaire}</em>");

        var corps = $"""
<!DOCTYPE html><html lang="fr"><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background:#e4eaf2;font-family:Arial,sans-serif;">
  <table width="100%" cellpadding="0" cellspacing="0" style="background:#e4eaf2;padding:40px 0;">
    <tr><td align="center">
      <table width="560" cellpadding="0" cellspacing="0" style="background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.08);">
        <tr><td style="background:#0f2137;padding:24px 32px;">
          <p style="margin:0;color:#c9a227;font-size:11px;letter-spacing:2px;text-transform:uppercase;">République du Bénin</p>
          <h1 style="margin:6px 0 0;color:#fff;font-size:20px;">Mairie de Banikoara</h1>
        </td></tr>
        <tr><td style="padding:32px;">
          <p style="font-size:15px;color:#334155;margin-top:0;">Bonjour <strong>{nom}</strong>,</p>
          <div style="background:{couleurStatut}15;border-left:4px solid {couleurStatut};border-radius:0 8px 8px 0;padding:16px 20px;margin:20px 0;">
            <p style="margin:0;font-size:16px;font-weight:700;color:{couleurStatut};">{iconStatut} Demande de congé {labelStatut}</p>
          </div>
          <p style="font-size:14px;color:#475569;">{messageCorps}</p>
          <table cellpadding="0" cellspacing="0" style="background:#f8fafc;border-radius:8px;padding:16px;width:100%;margin:16px 0;">
            <tr><td style="padding:4px 0;font-size:13px;color:#64748b;">Type</td><td style="font-size:13px;font-weight:600;color:#0f2137;">{typeConge}</td></tr>
            <tr><td style="padding:4px 0;font-size:13px;color:#64748b;">Début</td><td style="font-size:13px;font-weight:600;color:#0f2137;">{debut:dd/MM/yyyy}</td></tr>
            <tr><td style="padding:4px 0;font-size:13px;color:#64748b;">Fin</td><td style="font-size:13px;font-weight:600;color:#0f2137;">{fin:dd/MM/yyyy}</td></tr>
            {(approuve ? $"<tr><td style=\"padding:4px 0;font-size:13px;color:#64748b;\">Durée</td><td style=\"font-size:13px;font-weight:600;color:#0f2137;\">{duree} jour(s) ouvré(s)</td></tr>" : "")}
          </table>
          <hr style="border:none;border-top:1px solid #e2e8f0;margin:24px 0;">
          <p style="font-size:11px;color:#94a3b8;text-align:center;margin:0;">Mairie de Banikoara — Service des Ressources Humaines</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body></html>
""";
        await EnvoyerAsync(email, $"Demande de congé {labelStatut} — Mairie de Banikoara", corps, isHtml: true);
    }

    public async Task EnvoyerNotifNouvelleDemandeSuperAdminAsync(
        string emailAdmin, string nomAdmin, string nomAgent,
        DateTime debut, DateTime fin, string typeConge, string? commentaire)
    {
        var corps = $"""
<!DOCTYPE html><html lang="fr"><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background:#e4eaf2;font-family:Arial,sans-serif;">
  <table width="100%" cellpadding="0" cellspacing="0" style="background:#e4eaf2;padding:40px 0;">
    <tr><td align="center">
      <table width="560" cellpadding="0" cellspacing="0" style="background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.08);">
        <tr><td style="background:#0f2137;padding:24px 32px;">
          <p style="margin:0;color:#c9a227;font-size:11px;letter-spacing:2px;text-transform:uppercase;">République du Bénin</p>
          <h1 style="margin:6px 0 0;color:#fff;font-size:20px;">Mairie de Banikoara</h1>
        </td></tr>
        <tr><td style="padding:32px;">
          <p style="font-size:15px;color:#334155;margin-top:0;">Bonjour <strong>{nomAdmin}</strong>,</p>
          <div style="background:#fef9ec;border-left:4px solid #f59e0b;border-radius:0 8px 8px 0;padding:16px 20px;margin:20px 0;">
            <p style="margin:0;font-size:15px;font-weight:700;color:#b45309;">⏳ Nouvelle demande de congé en attente</p>
            <p style="margin:6px 0 0;font-size:13px;color:#92400e;">L'agent <strong>{nomAgent}</strong> a soumis une demande qui nécessite votre traitement.</p>
          </div>
          <table cellpadding="0" cellspacing="0" style="background:#f8fafc;border-radius:8px;padding:16px;width:100%;margin:16px 0;">
            <tr><td style="padding:4px 0;font-size:13px;color:#64748b;">Agent</td><td style="font-size:13px;font-weight:600;color:#0f2137;">{nomAgent}</td></tr>
            <tr><td style="padding:4px 0;font-size:13px;color:#64748b;">Type</td><td style="font-size:13px;font-weight:600;color:#0f2137;">{typeConge}</td></tr>
            <tr><td style="padding:4px 0;font-size:13px;color:#64748b;">Début</td><td style="font-size:13px;font-weight:600;color:#0f2137;">{debut:dd/MM/yyyy}</td></tr>
            <tr><td style="padding:4px 0;font-size:13px;color:#64748b;">Fin</td><td style="font-size:13px;font-weight:600;color:#0f2137;">{fin:dd/MM/yyyy}</td></tr>
            {(string.IsNullOrWhiteSpace(commentaire) ? "" : $"<tr><td style=\"padding:4px 0;font-size:13px;color:#64748b;\">Motif</td><td style=\"font-size:13px;color:#0f2137;\">{commentaire}</td></tr>")}
          </table>
          <p style="text-align:center;margin:24px 0;">
            <a href="{_baseUrl}/conges" style="background:#0f2137;color:#fff;padding:12px 28px;border-radius:8px;font-size:14px;font-weight:700;text-decoration:none;display:inline-block;">Traiter la demande</a>
          </p>
          <hr style="border:none;border-top:1px solid #e2e8f0;margin:24px 0;">
          <p style="font-size:11px;color:#94a3b8;text-align:center;margin:0;">Mairie de Banikoara — Service des Ressources Humaines</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body></html>
""";
        await EnvoyerAsync(emailAdmin, "Nouvelle demande de congé en attente — Mairie de Banikoara", corps, isHtml: true);
    }

    private async Task EnvoyerAsync(string destinataire, string sujet, string corps, bool isHtml = false)
    {
        if (!_smtp.Enabled || string.IsNullOrWhiteSpace(_smtp.Host))
        {
            _logger.LogWarning(
                "SMTP désactivé — email non envoyé à {Email}. Sujet: {Sujet}",
                destinataire, sujet);
            _logger.LogInformation("Contenu email:\n{Corps}", corps);
            return;
        }

        using var client = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_smtp.User, _smtp.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_smtp.From, _smtp.FromName),
            Subject = sujet,
            Body = corps,
            IsBodyHtml = isHtml
        };
        message.To.Add(destinataire);
        message.Headers.Add("X-Mailer", "GestionPersonnelMairie");
        message.Headers.Add("X-Priority", "1");

        await client.SendMailAsync(message);
        _logger.LogInformation("Email envoyé à {Email}", destinataire);
    }
}
