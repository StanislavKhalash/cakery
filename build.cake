#tool nuget:?package=NUnit.ConsoleRunner&version=3.7.0
#tool nuget:?package=JetBrains.ReSharper.CommandLineTools

#addin "Cake.Issues"
#addin "Cake.Issues.InspectCode"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./Source/Client/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./Client.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./Client.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./Client.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./Tests/**/bin/" + configuration + "/*.Test.dll", new NUnit3Settings { NoResults = true, Labels = NUnit3Labels.On });
});

var resharperReportsDirectory = buildDir + Directory("_ReSharperReports");
var inspectCodeReportsFile = resharperReportsDirectory + File("inspectcode-output.xml");
var dupFinderReportsFile = resharperReportsDirectory + File("dupfinder-output.xmlie");

Task("Run-Inspect-Code")
    .WithCriteria(IsRunningOnWindows())
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    InspectCode("./Client.sln", new InspectCodeSettings {
      SolutionWideAnalysis = true,
      ThrowExceptionOnFindingViolations = true,
      OutputFile = inspectCodeReportsFile
    });
});

Task("Lint")
    .WithCriteria(IsRunningOnWindows())
    .IsDependentOn("Run-Inspect-Code")
    .Does(() =>
{
    var settings =
        new ReadIssuesSettings(buildDir)
        {
            Format = IssueCommentFormat.Markdown
        };

    var issues = ReadIssues(
        new List<IIssueProvider>
        {
            InspectCodeIssuesFromFilePath(inspectCodeReportsFile)
        },
        settings);

    foreach(var issue in issues)
    {
        Information("{0}({1}) {2}", issue.AffectedFileRelativePath, issue.Line, issue.Message);
    }

    if (issues.Count() > 0)
    {
        throw new Exception(string.Format("{0} issues are found.", issues.Count()));
    }
});

Task("FindDuplicates")
	.WithCriteria(IsRunningOnWindows())
	.IsDependentOn("Build")
	.Does(() =>
{
	DupFinder("./Client.sln", new DupFinderSettings {
		ShowStats = true,
		ShowText = true,
		OutputFile = dupFinderReportsFile, 
		ThrowExceptionOnFindingDuplicates = true
    });
});
		
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests")
	.IsDependentOn("Lint")
	.IsDependentOn("FindDuplicates");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
