param([String]$branch)
# to make all errors terminating -> catchable in Try/Catch
$ErrorActionPreference = "Stop"

if (-not $branch.StartsWith("rel-")) {
	Write-Host "Build version can be incremented only in release branch (current branch is $branch)."
	exit 1
}

# path to NuGet specification file (nuspec) 
$nuspecFilename = "../publish/Package.nuspec";

# path to AssemblyInfo.cs file (DLL version)
$assemblyFilename = "../GenericRepository/Properties/AssemblyInfo.cs"

# loading current version from .nuspec file
$version = ../publish/Load-Version -file $nuspecFilename
$oldVersion = "$($version.Major).$($version.Minor).$($version.Build).$($version.Revision)";

# defining nuget package and dll version -
$newVersion = "$($version.Major).$($version.Minor).$($version.Build+1).0";
$dllVersion = "$($version.Major).$($version.Minor).$($version.Build+1).0";          

Write-host "Upgrading version $oldVersion to $newVersion.";

# updating version in the nuspec and assembly file
../publish/Save-Version -assemblyFile $assemblyFilename -assemblyVersion $dllVersion -nugetFile $nuspecFilename -nugetVersion $newVersion
