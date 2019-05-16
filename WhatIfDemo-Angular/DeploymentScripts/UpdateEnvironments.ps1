# Update the environment.prod.ts file with parameter values taken from Environment Variables.
# You need to specify these environment variables (and values for them) for the build task.

$filePath = $Env:BUILD_SOURCESDIRECTORY + "\WhatIfDemo-Angular\src\environments\environment.prod.ts"
$fileText = Get-Content($filePath)

$fileText = $fileText -replace ("facebookAppId: ''"), ("facebookAppId: '$Env:facebookAppId'")
$fileText = $fileText -replace ("backendBaseUri: ''"), ("backendBaseUri: '$Env:backendBaseUri'")
$fileText = $fileText -replace ("appInsightsInstrumentationKey: ''"), ("appInsightsInstrumentationKey: '$Env:appInsightsInstrumentationKey'")

$fileText | Out-File $filePath -Encoding UTF8