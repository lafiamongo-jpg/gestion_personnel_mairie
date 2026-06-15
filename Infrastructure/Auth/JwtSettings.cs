namespace GestionPersonnelMairie.Infrastructure.Auth;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "GestionPersonnelMairie";
    public string Audience { get; set; } = "GestionPersonnelMairie";
    public int ExpirationMinutes { get; set; } = 480;
}
