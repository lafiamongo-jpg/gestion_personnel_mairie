using GestionPersonnelMairie.Models;

namespace GestionPersonnelMairie.Core.DTOs;

public class LoginResult
{
    public required Utilisateur Utilisateur { get; init; }
    public required string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
}
