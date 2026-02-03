using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Alvtime.Adminpanel.Client;
using Alvtime.Adminpanel.Client.Authorization;
using Alvtime.Adminpanel.Client.ErrorHandling;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();

builder.Services.AddHttpClient("Alvtime.API", (sp, client) =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
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
