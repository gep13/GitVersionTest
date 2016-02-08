//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

GitVersion assertedVersions        = null;
var version = string.Empty;
var semVersion = string.Empty;

// Define directories.
var buildDir = Directory("./src/Gep13.GitVersionTest/bin") + Directory(configuration);
var buildResultDir = Directory("./build");
var nugetRoot = buildResultDir + Directory("nuget");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    Information("Building version {0} of Gep13.GitVersionTest.", semVersion);
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] {
        buildResultDir, nugetRoot});
});

Task("Run-GitVersion-Local")
    .Does(() =>
{
    assertedVersions = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json,
    });
    
    version = assertedVersions.MajorMinorPatch;
    semVersion = assertedVersions.LegacySemVerPadded;
    
    Information("Calculated Semantic Version: {0}", semVersion);
});

Task("Build")
    .IsDependentOn("Run-GitVersion-Local")
    .IsDependentOn("Clean")
    .Does(() =>
{
        MSBuild("./src/Gep13.GitVersionTest.sln", new MSBuildSettings()
            .SetConfiguration(configuration)
            .WithProperty("Windows", "True")
            .WithProperty("TreatWarningsAsErrors", "True")
            .UseToolVersion(MSBuildToolVersion.NET45)
            .SetVerbosity(Verbosity.Minimal)
            .SetNodeReuse(false));
});

Task("Create-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
{
    NuGetPack("./nuspec/Gep13.GitVersionTest.nuspec", new NuGetPackSettings {
        Version = semVersion,
        BasePath = buildDir,
        OutputDirectory = nugetRoot,
        Symbols = false,
        NoPackageAnalysis = true
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
  .IsDependentOn("Create-NuGet-Packages");
  
Task("Default")
  .IsDependentOn("Package");
  
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);  