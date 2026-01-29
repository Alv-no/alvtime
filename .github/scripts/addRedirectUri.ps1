param (
    [string]$uriToAdd,
    [string]$appRegistrationClientId
)
# Fetch existing redirect URIs from App Registration
$existingRedirectUris = az ad app show --id $appRegistrationClientId --query spa.redirectUris 

# Remove whitespace, quotes, and brackets, then split into an array
$cleanedRedirectUriList = $existingRedirectUris | Out-String | ForEach-Object { $_ -replace '\s', '' } | ForEach-Object { $_ -replace '"', '' } | ForEach-Object { $_ -replace '\[|\]', '' } | ForEach-Object { $_.Split(',') }

if ($cleanedRedirectUriList -contains $uriToAdd) {
    Write-Host "The URI '$uriToAdd' already exists in the redirect URIs. No action taken."
    exit 0
}

$urisToSet = $cleanedRedirectUriList + $uriToAdd

# Set up the argument correctly with proper formatting
$uriListString = $urisToSet -join "','"
$setArgument = "spa={'redirectUris': ['" + $uriListString + "']}"

# Set the new list of RedirectUris in the App Registration
az ad app update --id $appRegistrationClientId --set $setArgument