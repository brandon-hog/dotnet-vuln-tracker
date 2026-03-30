using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Client.Services;
using Client.Handlers;
using Client.Providers;
using Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<LocalStorageService>();

builder.Services.AddTransient<TokenAuthHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, OpaqueTokenAuthStateProvider>();

builder.Services.AddScoped(sp => 
{
    // Resolve custom JWT handler from the DI container
    var tokenHandler = sp.GetRequiredService<TokenAuthHandler>();
    
    // Assign the base Blazor WebAssembly handler to actually execute the network request
    tokenHandler.InnerHandler = new HttpClientHandler();
    
    // Inject the pipeline into the new HttpClient
    return new HttpClient(tokenHandler) 
    { 
        BaseAddress = new Uri("http://localhost:5286/")
    };
});

await builder.Build().RunAsync();
