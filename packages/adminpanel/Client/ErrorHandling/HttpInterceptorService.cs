using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Toolbelt.Blazor;

namespace Alvtime.Adminpanel.Client.ErrorHandling;

public class HttpInterceptorService
{
    private readonly HttpClientInterceptor _interceptor;
    private readonly NavigationManager _navManager;
    private readonly ISnackbar _snackbar;

    public HttpInterceptorService(HttpClientInterceptor interceptor, NavigationManager navManager, ISnackbar snackbar)
    {
        _interceptor = interceptor;
        _navManager = navManager;
        _snackbar = snackbar;
    }
    
    public void RegisterEvent() => _interceptor.AfterSendAsync += InterceptResponse;
    
    private async Task InterceptResponse(object sender, HttpClientInterceptorEventArgs e)
    {
        var message = string.Empty;
        if (e.Response.IsSuccessStatusCode) return;
        var statusCode = e.Response.StatusCode;
        switch (statusCode)
        {
            case HttpStatusCode.BadRequest:
                var capturedContent = await e.GetCapturedContentAsync();
                var content = await capturedContent.ReadAsStringAsync();
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (problemDetails != null)
                {
                    foreach (var problemDetail in problemDetails.Errors.SelectMany(problemKey => problemKey.Value))
                    {
                        _snackbar.Add(problemDetail, Severity.Error);
                    }
                }
                break;
            case HttpStatusCode.NotFound:
                // Prevent default on 404
                break;
            default:
                _navManager.NavigateTo("/500");
                message = "Noe gikk galt. Kontakt høyere makter.";
                break;
        }
        throw new HttpResponseException(message);
    }
    
    public void DisposeEvent() => _interceptor.AfterSendAsync -= InterceptResponse;
}