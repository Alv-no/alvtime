using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace AlvTime.Common.Configuration;

public class DottableKeyVaultSecretManager : KeyVaultSecretManager
{
    public override string GetKey(KeyVaultSecret secret)
    {
        var result = secret.Name.Replace("---", ".").Replace("--", ConfigurationPath.KeyDelimiter);

        return result;
    }
}