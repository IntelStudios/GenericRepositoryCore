param([String]$branch)
# to make all errors terminating -> catchable in Try/Catch
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrEmpty($branch)) {
	Write-Host "Release script cannot be executed directly, run batch script release_stable_version.bat."
	exit 1
}

if ($branch -ne "master") {
	Write-Host "Stable version can be created only from master branch (current branch is $branch)."
	exit 1
}

# path to NuGet specification file (nuspec) 
$nuspecFilename = "../publish/Package.nuspec";

# path to AssemblyInfo.cs file (DLL version)
$assemblyFilename = "../GenericRepository/Properties/AssemblyInfo.cs"

# loading current version from .nuspec file
$version = ../publish/Load-Version -file $nuspecFilename
$oldVersion = "$($version.Major).$($version.Minor).$($version.Build).$($version.Revision)"; 

# name of the brach which will be created
$releaseBranch = "rel-$($version.Major)-$($version.Minor)"

# commont git executable path
$cmdPath = "git.exe"

# creating release branch
$cmdArgList = @(
	"checkout",
	"-b","$releaseBranch"
)
& $cmdPath $cmdArgList

# creating fake commit to force Jenkins build the branch
../release/Update-Release-Date -nugetFile $nuspecFilename

# adding modified nuspec file
$cmdArgList = @(
	"add",
	"$nuspecFilename"
)
& $cmdPath $cmdArgList

# commiting modified files
$cmdArgList = @(
	"commit",
	"-m", "Initial Build"
)
& $cmdPath $cmdArgList

# setting up stream of release branch
$cmdArgList = @(
	"push",
	"--set-upstream", "origin", "$releaseBranch"
)
& $cmdPath $cmdArgList

# pushing code into release branch
$cmdArgList = @(
	"push"
)
& $cmdPath $cmdArgList

# return back to master
$cmdArgList = @(
	"checkout",
	"master"
)
& $cmdPath $cmdArgList

# defining nuget package and dll version -
$newVersion = "$($version.Major).$($version.Minor+1).0.0";
$dllVersion = "$($version.Major).$($version.Minor+1).0.0";      

Write-host "Upgrading version $oldVersion to $newVersion.";

# incrementing minor versions
$nugetPackageVersion = "$($version.Major).$($version.Minor+1).0.0";
$dllVersion = "$($version.Major).$($version.Minor+1).0.0";          

# updating version in the nuspec file
../publish/Save-Version -assemblyFile $assemblyFilename -assemblyVersion $dllVersion -nugetFile $nuspecFilename -nugetVersion $nugetPackageVersion

# adding modified files
$cmdArgList = @(
	"add",
	"$nuspecFilename"
)
& $cmdPath $cmdArgList

$cmdArgList = @(
	"add",
	"$assemblyFilename"
)
& $cmdPath $cmdArgList

# commiting modified files
$cmdArgList = @(
	"commit",
	"-m", "New version $newVersion"
)
& $cmdPath $cmdArgList

# pushing modified files
$cmdArgList = @(
	"push"
)
& $cmdPath $cmdArgList

Write-host
Write-host "New stable version $oldVersion was successfully created into $releaseBranch.";
Write-host "Since now, master version is $nugetPackageVersion.";