#tool nuget:?package=NUnit.ConsoleRunner&version=3.7.0
#tool nuget:?package=JetBrains.ReSharper.CommandLineTools
#tool nuget:?package=Cake.Prca.Issues.InspectCode

#addin "Cake.Prca"
#addin "Cake.Prca.Issues.InspectCode"

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
var resharperReportsFile = resharperReportsDirectory + File("inspectcode-output.xml");

Task("Run-Inspect-Code")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    InspectCode("./Client.sln", new InspectCodeSettings {
      SolutionWideAnalysis = true,
      ThrowExceptionOnFindingViolations = true,
      OutputFile = resharperReportsFile
    });
});

Task("Inspect-Code-Read-Issues")
    .IsDependentOn("Run-Inspect-Code")
    .Does(() =>
{
    var settings =
        new ReadIssuesSettings(buildDir)
        {
            Format = PrcaCommentFormat.Markdown
        };

    var issues = ReadIssues(
        new List<ICodeAnalysisProvider>
        {
            InspectCodeIssuesFromFilePath(resharperReportsFile)
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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);