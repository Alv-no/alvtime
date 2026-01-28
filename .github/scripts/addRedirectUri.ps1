param (
    [string]$uriToAdd,
    [string]$appRegistrationClientId
)

$existingRedirectUrisList = az ad app show --id $appRegistrationClientId --query spa.redirectUris 

$cleanedRedirectUriList = $existingRedirectUrisList | Out-String | ForEach-Object { $_ -replace '\s', '' } | ForEach-Object { $_ -replace '"', '' } | ForEach-Object { $_ -replace '\[|\]', '' } | ForEach-Object { $_.Split(',') }

if ($cleanedRedirectUriList -contains $uriToAdd) {
    Write-Host "The URI '$uriToAdd' already exists in the redirect URIs. No action taken."
    exit 0
}

$urisToSet = $cleanedRedirectUriList + $uriToAdd

$uriListString = $urisToSet -join "','"


$setArgument = "spa={'redirectUris': ['" + $uriListString + "']}"

az ad app update --id $appRegistrationClientId --set $setArgument
