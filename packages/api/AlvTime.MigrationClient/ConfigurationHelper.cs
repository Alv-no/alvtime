using System.Reflection;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace AlvTime.MigrationClient;

public static class ConfigurationHelper
{
    private static string? GetEnvironment() => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    
    public static void CommonConfigure<T>(this IConfigurationBuilder configurationBuilder) where T : class
    {
        CommonConfigure(configurationBuilder, typeof(T));
    }
    
    private static void CommonConfigure(IConfigurationBuilder configurationBuilder, Type? userSecretsType)
    {
        var basePath = Directory.GetCurrentDirectory();
        var environmentName = GetEnvironment();
        configurationBuilder
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        if (!string.IsNullOrEmpty(environmentName))
        {
            configurationBuilder
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
        }

        if (userSecretsType != null)
        {
            configurationBuilder.AddUserSecrets(userSecretsType.Assembly, optional: true);
        }

        configurationBuilder.AddEnvironmentVariables();

        var configuration = configurationBuilder.Build();

        var keyVaultUrl = configuration["AzureKeyVault:Uri"];

        if (!string.IsNullOrEmpty(keyVaultUrl))
        {
            configurationBuilder.AddAzureKeyVault(new Uri(keyVaultUrl), GetAzureCredential(), new DottableKeyVaultSecretManager());
        }
    }
    
    public static DefaultAzureCredential GetAzureCredential()
    {
        return new DefaultAzureCredential( new DefaultAzureCredentialOptions { ExcludeSharedTokenCacheCredential = true } );
    }
}