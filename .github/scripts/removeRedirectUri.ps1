param (
    [string]$numberPR,
    [string]$appRegistrationClientId
)

$existingRedirectUrisList = az ad app show --id $appRegistrationClientId --query spa.redirectUris

$cleanedRedirectUriList = $existingRedirectUrisList | Out-String | ForEach-Object { $_ -replace '\s', '' } | ForEach-Object { $_ -replace '"', '' } | ForEach-Object { $_ -replace '\[|\]', '' } | ForEach-Object { $_.Split(',') }

$filter = "-" + $numberPR + "."
$filteredList = $cleanedRedirectUriList | Where-Object { $_ -notmatch $filter }

$uriListString = $filteredList -join "','"

$setArgument = "spa={'redirectUris': ['" + $uriListString + "']}"

az ad app update --id $appRegistrationClientId --set $setArgument