
# ${{ steps.builddeploy.outputs.static_web_app_url }}

$clientId = '148ac485-a123-479c-8ed3-bfaa56619e04'
$newUri = "https://ap.no"


$existingRedirectUrisList = az ad app show --id $clientId --query spa.redirectUris 

$cleanedRedirectUriList = $existingRedirectUrisList | Out-String | ForEach-Object { $_ -replace '\s', '' } | ForEach-Object { $_ -replace '"', '' } | ForEach-Object { $_ -replace '\[|\]', '' } | ForEach-Object { $_.Split(',') }


$urisToSet = $cleanedRedirectUriList + ${{ steps.builddeploy.outputs.static_web_app_url }}

$uriListString = $urisToSet -join "','"

$setArgument = "spa={'redirectUris': ['" + $uriListString + "']}"


az ad app update --id $clientId --set $setArgument