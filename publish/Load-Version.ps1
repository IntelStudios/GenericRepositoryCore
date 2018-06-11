param([String]$file)

class Version
{
    [int]$Major
    [int]$Minor
    [int]$Build
    [int]$Revision
}

# loading XML content of nuspec file
Try {
    [xml]$XmlDocument = Get-Content "$file"
} 
Catch {
    Write-Error "NuGet nuspec cannot be read."
    exit 1;
}

# getting current version
$currentVersion = $XmlDocument.package.metadata.version

# splitting version into parts
$major = $currentVersion.split('.')[0];
$minor = $currentVersion.split('.')[1];
$build = $currentVersion.split('.')[2];
$revision = $currentVersion.split('.')[3];;

# defining return object
$version = New-Object Version
$version.Major = $major
$version.Minor = $minor
$version.Build = $build
$version.Revision = $revision

$version