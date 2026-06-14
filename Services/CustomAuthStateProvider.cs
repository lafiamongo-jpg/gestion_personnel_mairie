using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace GestionPersonnelMairie.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthStateService _authStateService;
        private readonly IJSRuntime _js;

        public CustomAuthStateProvider(AuthStateService authStateService, IJSRuntime js)
        {
            _authStateService = authStateService;
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Si pas de session en mémoire, tenter de restaurer depuis localStorage
            if (string.IsNullOrEmpty(_authStateService.Email))
            {
                try
                {
                    var email = await _js.InvokeAsync<string?>("authStorage.load", "auth_email");
                    var nom = await _js.InvokeAsync<string?>("authStorage.load", "auth_nom");
                    var role = await _js.InvokeAsync<string?>("authStorage.load", "auth_role");
                    var agentIdStr = await _js.InvokeAsync<string?>("authStorage.load", "auth_agentId");
                    var userIdStr = await _js.InvokeAsync<string?>("authStorage.load", "auth_userId");

                    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(role))
                    {
                        _authStateService.Email = email;
                        _authStateService.NomComplet = nom ?? email;
                        _authStateService.Role = role;
                        _authStateService.AgentId = int.TryParse(agentIdStr, out var aid) ? aid : null;
                        _authStateService.UtilisateurId = int.TryParse(userIdStr, out var uid) ? uid : null;
                    }
                }
                catch { /* JS pas encore disponible au premier rendu statique */ }
            }

            if (string.IsNullOrEmpty(_authStateService.Email))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            return BuildAuthState();
        }

        public async Task LoginAsync(string email, string nomComplet, string role, int agentId, int utilisateurId)
        {
            _authStateService.Email = email;
            _authStateService.NomComplet = nomComplet;
            _authStateService.Role = role;
            _authStateService.AgentId = agentId;
            _authStateService.UtilisateurId = utilisateurId;

            // Persister dans localStorage
            try
            {
                await _js.InvokeVoidAsync("authStorage.save", "auth_email", email);
                await _js.InvokeVoidAsync("authStorage.save", "auth_nom", nomComplet);
                await _js.InvokeVoidAsync("authStorage.save", "auth_role", role);
                await _js.InvokeVoidAsync("authStorage.save", "auth_agentId", agentId.ToString());
                await _js.InvokeVoidAsync("authStorage.save", "auth_userId", utilisateurId.ToString());
            }
            catch { }

            NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthState()));
        }

        public async Task LogoutAsync()
        {
            _authStateService.Clear();

            try { await _js.InvokeVoidAsync("authStorage.clear"); }
            catch { }

            NotifyAuthenticationStateChanged(Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        private AuthenticationState BuildAuthState()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _authStateService.Email!),
                new Claim(ClaimTypes.Email, _authStateService.Email!),
            };

            if (!string.IsNullOrEmpty(_authStateService.Role))
                claims.Add(new Claim(ClaimTypes.Role, _authStateService.Role));

            if (!string.IsNullOrEmpty(_authStateService.NomComplet))
                claims.Add(new Claim("NomComplet", _authStateService.NomComplet));

            if (_authStateService.AgentId.HasValue)
                claims.Add(new Claim("AgentId", _authStateService.AgentId.Value.ToString()));

            if (_authStateService.UtilisateurId.HasValue)
                claims.Add(new Claim("UtilisateurId", _authStateService.UtilisateurId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, "session");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
