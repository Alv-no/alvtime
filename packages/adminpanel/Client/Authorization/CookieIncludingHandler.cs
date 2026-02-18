using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Alvtime.Adminpanel.Client.Authorization;

public class CookieIncludingHandler() : DelegatingHandler(new HttpClientHandler())
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return await base.SendAsync(request, cancellationToken);
    }
}