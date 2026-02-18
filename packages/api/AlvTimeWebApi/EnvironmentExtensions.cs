using Microsoft.Extensions.Hosting;

namespace AlvTimeWebApi;

public static class EnvironmentExtensions
{
    private const string TestEnvironment = "Test";
    
    public static bool IsTest(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsEnvironment(TestEnvironment);
    }
}