param([String]$branch="master")
# to make all errors terminating -> catchable in Try/Catch
$ErrorActionPreference = "Stop"

# -------------------
# - FILES TO UPDATE -
# -------------------

# path to NuGet specification file (nuspec) 
$nuspecFilename = "$env:WORKSPACE/publish/Package.nuspec";

# path to AssemblyInfo.cs file (DLL version)
$assemblyFilename = "$env:WORKSPACE/GenericRepository/Properties/AssemblyInfo.cs"

# ---------------------------------------------
# - LOADING CURRENT VERSION FROM .NUSPEC FILE -
# ---------------------------------------------

$version = ./Load-Version -file $nuspecFilename

Write-Host "Found version $($version.Major).$($version.Minor).$($version.Build).$($version.Revision).";

# ------------------------------------------
# - DEFINING NUGET PACKAGE AND DLL VERSION -
# ------------------------------------------

if ($branch -eq "master") {

    # prerelease nuget version of master
    $nugetPackageVersion = "$($version.Major).$($version.Minor).0-master";
    $dllVersion = "$($version.Major).$($version.Minor).0.0";          

    Write-host "Creating prerelease NuGet package $nugetPackageId (version $nugetPackageVersion, DLL version $dllVersion) of current master.";

} elseif ($branch.ToLower().StartsWith("rel-")) {

	# stable nuget version of defined branch version
    $nugetPackageVersion = "$($version.Major).$($version.Minor).$($version.Build).0";
    $dllVersion = "$($version.Major).$($version.Minor).$($version.Build).0";

    Write-host "Creating stable NuGet package $nugetPackageId ($nugetPackageVersion) from branch $branch.";

} else {

    # development nuget version of particular branch
    $nugetPackageVersion = "$($version.Major).$($version.Minor).$($version.Build)-$branch";
    $dllVersion = "$($version.Major).$($version.Minor).$($version.Build).0";    

    Write-Host "Creating development NuGet package $nugetPackageId (version $nugetPackageVersion, DLL version $dllVersion).";
}

# updating version in the nuspec file
.\Save-Version -assemblyFile $assemblyFilename -assemblyVersion $dllVersion -nugetFile $nuspecFilename -nugetVersion $nugetPackageVersion