using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Client.Services;

namespace Client.Providers;

public class OpaqueTokenAuthStateProvider(LocalStorageService localStorage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorage.GetItemAsync<string>("accessToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            // No token = Not Logged In
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Token exists. Create a generic authenticated state.
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "Authenticated User") }, 
            "OpaqueBearer" // The authentication type must have a value to register as logged in
        );
            
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserLoggedIn()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "Authenticated User") }, 
            "OpaqueBearer"
        );
            
        var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        NotifyAuthenticationStateChanged(authState);
    }
}