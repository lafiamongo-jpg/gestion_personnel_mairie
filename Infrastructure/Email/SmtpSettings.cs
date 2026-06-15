namespace GestionPersonnelMairie.Infrastructure.Email;

public class SmtpSettings
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "noreply@mairie-banikoara.bj";
    public string FromName { get; set; } = "Mairie de Banikoara";
}
