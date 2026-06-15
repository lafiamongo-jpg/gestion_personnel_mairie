using GestionPersonnelMairie.Core.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using GestionPersonnelMairie.Infrastructure.Auth;

namespace GestionPersonnelMairie.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthStateService _authStateService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IJSRuntime _js;

    public CustomAuthStateProvider(
        AuthStateService authStateService,
        JwtTokenService jwtTokenService,
        IJSRuntime js)
    {
        _authStateService = authStateService;
        _jwtTokenService = jwtTokenService;
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrEmpty(_authStateService.JwtToken))
        {
            try
            {
                var token = await _js.InvokeAsync<string?>("authStorage.load", "auth_token");
                if (!string.IsNullOrEmpty(token))
                {
                    var principal = _jwtTokenService.ValidateToken(token);
                    if (principal != null)
                        HydraterDepuisClaims(principal, token);
                    else
                        await _js.InvokeVoidAsync("authStorage.clear");
                }
            }
            catch { /* JS pas encore disponible */ }
        }
        else if (_authStateService.TokenExpiresAt.HasValue
                 && _authStateService.TokenExpiresAt <= DateTime.UtcNow)
        {
            _authStateService.Clear();
            try { await _js.InvokeVoidAsync("authStorage.clear"); } catch { }
        }

        if (string.IsNullOrEmpty(_authStateService.JwtToken))
            return Anonyme();

        return BuildAuthState();
    }

    public async Task LoginWithResultAsync(LoginResult result)
    {
        var principal = _jwtTokenService.ValidateToken(result.Token);
        if (principal == null)
            throw new UnauthorizedAccessException("Token invalide.");

        await LoginAsync(result.Token, result.ExpiresAt, principal);
    }

    public async Task LoginAsync(string token, DateTime expiresAt, ClaimsPrincipal principal)
    {
        HydraterDepuisClaims(principal, token);
        _authStateService.TokenExpiresAt = expiresAt;

        try
        {
            await _js.InvokeVoidAsync("authStorage.save", "auth_token", token);
            await _js.InvokeVoidAsync("authStorage.save", "auth_expires", expiresAt.ToString("O"));
        }
        catch { }

        NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthState()));
    }

    public async Task LogoutAsync()
    {
        _authStateService.Clear();
        try { await _js.InvokeVoidAsync("authStorage.clear"); } catch { }
        NotifyAuthenticationStateChanged(Task.FromResult(Anonyme()));
    }

    private void HydraterDepuisClaims(ClaimsPrincipal principal, string token)
    {
        _authStateService.JwtToken = token;
        _authStateService.Email = principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst(ClaimTypes.Name)?.Value;
        _authStateService.NomComplet = principal.FindFirst("NomComplet")?.Value ?? _authStateService.Email;
        _authStateService.Role = principal.FindFirst(ClaimTypes.Role)?.Value;

        if (int.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            _authStateService.UtilisateurId = userId;

        if (int.TryParse(principal.FindFirst("AgentId")?.Value, out var agentId))
            _authStateService.AgentId = agentId;
    }

    private AuthenticationState BuildAuthState()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, _authStateService.Email!),
            new(ClaimTypes.Email, _authStateService.Email!)
        };

        if (!string.IsNullOrEmpty(_authStateService.Role))
            claims.Add(new Claim(ClaimTypes.Role, _authStateService.Role));

        if (!string.IsNullOrEmpty(_authStateService.NomComplet))
            claims.Add(new Claim("NomComplet", _authStateService.NomComplet));

        if (_authStateService.AgentId.HasValue)
            claims.Add(new Claim("AgentId", _authStateService.AgentId.Value.ToString()));

        if (_authStateService.UtilisateurId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, _authStateService.UtilisateurId.Value.ToString()));

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private static AuthenticationState Anonyme() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}
