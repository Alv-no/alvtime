param (
    [string]$numberPR,
    [string]$appRegistrationClientId
)

# Fetch existing redirect URIs from App Registration
$existingRedirectUrisList = az ad app show --id $appRegistrationClientId --query spa.redirectUris

# Remove whitespace, quotes, and brackets, then split into an array
$cleanedRedirectUriList = $existingRedirectUrisList | Out-String | ForEach-Object { $_ -replace '\s', '' } | ForEach-Object { $_ -replace '"', '' } | ForEach-Object { $_ -replace '\[|\]', '' } | ForEach-Object { $_.Split(',') }

# Filter out the URI that contains the specified PR number
$filter = "-" + $numberPR + "."
$filteredList = $cleanedRedirectUriList | Where-Object { $_ -notmatch $filter }

# Set up the argument correctly with proper formatting
$uriListString = $filteredList -join "','"
$setArgument = "spa={'redirectUris': ['" + $uriListString + "']}"

# Set the new list of RedirectUris in the App Registration
az ad app update --id $appRegistrationClientId --set $setArgument