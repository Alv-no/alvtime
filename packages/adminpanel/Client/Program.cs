using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Alvtime.Adminpanel.Client;
using Alvtime.Adminpanel.Client.Authorization;
using Alvtime.Adminpanel.Client.ErrorHandling;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();

builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

builder.Services.AddHttpClient("Alvtime.API", (sp, client) =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
        client.EnableIntercept(sp);
    })
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Alvtime.API"));
builder.Services.AddScoped<HttpInterceptorService>();


builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration["ApiSettings:Scope"]!);
    options.ProviderOptions.LoginMode = "redirect";
    options.ProviderOptions.Cache.CacheLocation = "localStorage";
});

builder.Services.AddHttpClientInterceptor();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
