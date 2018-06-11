param([String]$assemblyFile, [String]$assemblyVersion, [String]$nugetFile, [String]$nugetVersion)

# loading XML content of nuspec file
Try {
    [xml]$XmlDocument = Get-Content "$nugetFile"
} 
Catch {
    Write-Error "NuGet nuspec cannot be read."
    exit 1;
}

# updating version in the nuspec file
Write-Host "Updating NuGet version in $nugetFile."
$XmlDocument.package.metadata.version = $nugetVersion
$XmlDocument.Save((Resolve-Path $nugetFile))

# updating version in assembly file
Write-Host "Updating DLL version in $assemblyFile."
$assemblyVersionRegex = '\("(\d)+\.(\d)+\.(\d)+\.(\d)+"\)'
(Get-Content "$assemblyFile") -replace $assemblyVersionRegex, "(""$assemblyVersion"")" | Set-Content $assemblyFile
