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

builder.Services.AddTransient<JwtAuthorizationHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

builder.Services.AddScoped(sp => 
{
    // Resolve custom JWT handler from the DI container
    var jwtHandler = sp.GetRequiredService<JwtAuthorizationHandler>();
    
    // Assign the base Blazor WebAssembly handler to actually execute the network request
    jwtHandler.InnerHandler = new HttpClientHandler();
    
    // Inject the pipeline into the new HttpClient
    return new HttpClient(jwtHandler) 
    { 
        BaseAddress = new Uri("http://localhost:5286/")
    };
});

await builder.Build().RunAsync();
