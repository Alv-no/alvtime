using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace AlvTime.Common.Configuration;

public static class ConfigurationHelper
{
    private static string? GetEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrEmpty(environment))
        {
            environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        }

        return environment;
    }

    public static IConfigurationBuilder CommonConfigure<T>(this IConfigurationBuilder configurationBuilder) where T : class
    {
        return CommonConfigure(configurationBuilder, typeof(T));
    }
    
    private static IConfigurationBuilder CommonConfigure(IConfigurationBuilder configurationBuilder, Type? userSecretsType)
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

        return configurationBuilder;
    }

    private static DefaultAzureCredential GetAzureCredential()
    {
        return new DefaultAzureCredential( new DefaultAzureCredentialOptions { ExcludeSharedTokenCacheCredential = true } );
    }
}