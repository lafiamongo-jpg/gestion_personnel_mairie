using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace GestionPersonnelMairie.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthStateService _authStateService;

        public CustomAuthStateProvider(AuthStateService authStateService)
        {
            _authStateService = authStateService;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (string.IsNullOrEmpty(_authStateService.Email))
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

            var claims = new[] { new Claim(ClaimTypes.Name, _authStateService.Email) };
            var identity = new ClaimsIdentity(claims, "session");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        public Task LoginAsync(string email)
        {
            _authStateService.Email = email;
            var claims = new[] { new Claim(ClaimTypes.Name, email) };
            var identity = new ClaimsIdentity(claims, "session");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            return Task.CompletedTask;
        }

        public Task LogoutAsync()
        {
            _authStateService.Email = null;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
            return Task.CompletedTask;
        }
    }
}