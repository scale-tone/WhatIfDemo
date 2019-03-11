# Sets the Function assembly version according to the current build number

$buildVersion = $Env:BUILD_BUILDNUMBER
$newFileVersion = $buildVersion.Substring(0, 4) + "." + $buildVersion.Substring(4, 2) + "." + $buildVersion.Substring(6, 2) + "." + $buildVersion.Substring(9)

$csprojFile = $Env:BUILD_SOURCESDIRECTORY + "\WhatIfDemo-Functions\WhatIfDemo-Functions.csproj"

Write-Host "Setting assembly version $newFileVersion to $csprojFile"

$fileVersionRegex = "\d+\.\d+\.\d+\.\d+"
$fileText = Get-Content($csprojFile)
$fileText = $fileText -replace ("<FileVersion>" + $fileVersionRegex + "</FileVersion>"), ("<FileVersion>" + $newFileVersion + "</FileVersion>")
$fileText = $fileText -replace ("<AssemblyVersion>" + $fileVersionRegex + "</AssemblyVersion>"), ("<AssemblyVersion>" + $newFileVersion + "</AssemblyVersion>")

$fileText | Out-File $csprojFile -Encoding UTF8
