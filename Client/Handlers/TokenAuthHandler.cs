using System.Net;
using System.Net.Http.Headers;
using Shared.Dtos;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Client.Providers;

namespace Client.Handlers;

public class TokenAuthHandler(
    LocalStorageService localStorage,
    NavigationManager navigationManager,
    AuthenticationStateProvider authStateProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await localStorage.GetItemAsync<string>("accessToken");
        
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Execute the actual HTTP request to the API
        var response = await base.SendAsync(request, cancellationToken);

        // Intercept 401 Unauthorized
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshToken = await localStorage.GetItemAsync<string>("refreshToken");
            
            if (!string.IsNullOrWhiteSpace(refreshToken) && await TryRefreshTokenAsync(refreshToken))
            {
                // Refresh succeeded. Get the newly saved token.
                var newToken = await localStorage.GetItemAsync<string>("accessToken");
                
                // Update the original request with the new token and retry it
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                
                // Dispose the old 401 response to prevent memory leaks
                response.Dispose(); 
                
                return await base.SendAsync(request, cancellationToken);
            }

            // If we reach here, there is no refresh token OR the refresh attempt failed.
            await WipeSessionAndRedirect();
        }

        return response;
    }

    private async Task<bool> TryRefreshTokenAsync(string refreshToken)
    {
        // Create a temporary HttpClient that bypasses THIS handler to prevent an infinite 401 loop
        using var client = new HttpClient { BaseAddress = new Uri("http://localhost:5286/") };
        
        var request = new RefreshRequest { RefreshToken = refreshToken };
        var response = await client.PostAsJsonAsync("refresh", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null && !string.IsNullOrEmpty(result.AccessToken))
            {
                await localStorage.SetItemAsync("accessToken", result.AccessToken);
                await localStorage.SetItemAsync("refreshToken", result.RefreshToken);
                return true;
            }
        }

        return false;
    }

    private async Task WipeSessionAndRedirect()
    {
        await localStorage.RemoveItemAsync("accessToken");
        await localStorage.RemoveItemAsync("refreshToken");
        
        ((OpaqueTokenAuthStateProvider) authStateProvider).NotifyUserLogout();
        
        navigationManager.NavigateTo("/login");
    }
}