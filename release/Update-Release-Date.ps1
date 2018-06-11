param([String]$nugetFile)

# loading XML content of nuspec file
[xml]$XmlDocument = Get-Content "$nugetFile"

$currentDate = [DateTime]::Now.ToString("o");

# getting current version
$XmlDocument.package.metadata.releaseNotes = "Published: $currentDate";

# updating date in the nuspec file
$XmlDocument.Save((Resolve-Path $nugetFile))
