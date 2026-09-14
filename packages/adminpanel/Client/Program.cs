using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Alvtime.Adminpanel.Client;
using Alvtime.Adminpanel.Client.Authorization;
using Alvtime.Adminpanel.Client.ErrorHandling;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The host serves the API base url, because as of .NET 10 the WebAssembly runtime no longer
// picks up the server's environment from the Blazor-Environment header, so appsettings.{Environment}.json
// on the client always resolves to Production.
using (var hostClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
{
    var apiBaseUrl = await hostClient.GetStringAsync("api/config/apiURL");

    if (string.IsNullOrWhiteSpace(apiBaseUrl))
    {
        throw new InvalidOperationException("Host returned no value for ApiSettings:BaseUrl.");
    }

    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ApiSettings:BaseUrl"] = apiBaseUrl
    });
}

builder.Services.AddLocalization();

builder.Services.AddHttpClient("Alvtime.API", (sp, client) =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
        client.DefaultRequestHeaders.Add("X-CSRF", "1");
        client.EnableIntercept(sp);
    })
    .ConfigurePrimaryHttpMessageHandler<CookieIncludingHandler>();

builder.Services.AddTransient<CookieIncludingHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Alvtime.API"));
builder.Services.AddScoped<HttpInterceptorService>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();

builder.Services.AddHttpClientInterceptor();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
