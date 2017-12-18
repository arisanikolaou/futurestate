#addin nuget:?package=Cake.Sonar
#addin "Cake.FileHelpers"
#addin nuget:?package=Cake.ArgumentHelpers
#addin nuget:?package=Cake.SemVer
#addin nuget:?package=semver&version=2.0.4
#addin "Cake.XdtTransform"
#addin nuget:?package=Cake.Codecov
#addin nuget:?package=Cake.Git

#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#tool "xunit.runner.console"
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool
#tool nuget:?package=Codecov


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configurationName = "Release";
var configuration = Argument("configuration", configurationName);
var solutionFile = GetFiles("./*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));
var distDir = Directory("./dist");
var nugetDirname = "./nuget";
var nugetDir = Directory(nugetDirname);
var buildDir = Directory("./build");
var defaultVersion = "0.1.2";
var solutionVersion = ArgumentOrEnvironmentVariable("BUILD_VERSION", "BUILD_VERSION", defaultVersion);

// nuget get
var nugetServer = "https://www.nuget.org";
var apiKey = ArgumentOrEnvironmentVariable("NUGET_APIKEY", "NUGET_APIKEY","");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Information("Starting build: " + solutionVersion);

Task("Clean-Outputs")
	.Does(() => 
	{
		CleanDirectory(buildDir);
		CleanDirectory(distDir);
	});

Task("Clean")
    .IsDependentOn("Clean-Outputs")
    .Does(() =>
{
        DotNetBuild(solutionFile, settings => settings
            .SetConfiguration(configuration)
            .WithTarget("Clean")
            .SetVerbosity(Verbosity.Minimal));
});

Task("SetVersion")
   .Does(() => {
	   	Information("Updating assembly details.");
	   
       	ReplaceRegexInFiles("./src/**/**/AssemblyInfo.cs", 
                           "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))", 
                           solutionVersion);
   });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
	Information("Restoring nuget packages.");
    NuGetRestore(solutionFile, new NuGetRestoreSettings { NoCache = true });
});

Task("Build")
	.IsDependentOn("SetVersion")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
	// assume git
	var lastCommit = GitLogTip("./");

	Information(@"Last commit {0}
		Short message: {1}
		Author:        {2}
		Authored:      {3:yyyy-MM-dd HH:mm:ss}
		Committer:     {4}
		Committed:     {5:yyyy-MM-dd HH:mm:ss}",
		lastCommit.Sha,
		lastCommit.MessageShort,
		lastCommit.Author.Name,
		lastCommit.Author.When,
		lastCommit.Committer.Name,
		lastCommit.Committer.When
    );

    if(IsRunningOnWindows())
    {
		Information("Building on Windows.");

		// Use MSBuild
		MSBuild(solutionFile, settings =>
			settings
			.SetConfiguration(configuration)
			.SetVerbosity(Verbosity.Minimal));
    }
    else
    {
      // Use XBuild
      XBuild(solutionFile, settings =>
        settings.SetConfiguration(configuration)
        .SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal));
    }
});


Task("Packages")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
	{
		var projectFilesToPack = solution.Value
			.Projects
			.Where(p => FileExists(p.Path.ChangeExtension(".nuspec")))
			.Select(p => p.Path);
		
		foreach(var project in projectFilesToPack)
		{
			Information("Packaging project: " + project);

			// var assemblyInfo = ParseAssemblyInfo(project.GetDirectory().CombineWithFilePath("./Properties/AssemblyInfo.cs"));
			// var assemblyVersion = ParseSemVer(assemblyInfo.AssemblyVersion); 
            // var packageVersion = assemblyVersion;
			// var packageVersion = assemblyVersion.Change(prerelease: "pre" + Jenkins.Environment.Build.BuildNumber);
            //var packageVersion = assemblyVersion.Change(prerelease: "1");

            // Information(packageVersion.ToString());

			NuGetPack(project, new NuGetPackSettings
			{
				OutputDirectory = nugetDir,
				IncludeReferencedProjects = true,
				Properties = new Dictionary<string, string> 
				{
					{ "Configuration", configuration }
				}
			});
		}
	});

Task("Websites")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
	{
		var webProjects = solution.Value
			.Projects
			.Where(p => p.Name.EndsWith(".Web"));

		foreach(var project in webProjects)
		{
			Information("Publishing {0}", project.Name);
			
			var publishDir = distDir + Directory(project.Name);

			DotNetBuild(project.Path, settings => settings
				.SetConfiguration(configuration)
				.WithProperty("DeployOnBuild", "true")
				.WithProperty("WebPublishMethod", "FileSystem")
				.WithProperty("DeployTarget", "WebPublish")
				.WithProperty("publishUrl", MakeAbsolute(publishDir).FullPath)
				.SetVerbosity(Verbosity.Minimal));

			Zip(publishDir, distDir + File(project.Name + ".zip"));
		}
	});

Task("Consoles")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
	{
		var consoleProjects = solution.Value
			.Projects
			.Where(p => p.Name.EndsWith(".Console"));

		foreach(var project in consoleProjects)
		{
			Information("Publishing {0}.", project.Name);

			var projectDir = project.Path.GetDirectory(); 
			var publishDir = distDir + Directory(project.Name);

			Information("Copying to output directory.");

			CopyDirectory(
				projectDir.Combine("bin").Combine(configuration),
				publishDir);

			var configFile = publishDir + File(project.Name + ".exe.config");
			var transformFile = projectDir.CombineWithFilePath("App." + configuration + ".config");

			Information("Transforming configuration file.");

			XdtTransformConfig(configFile, transformFile, configFile);

			Zip(publishDir, distDir + File(project.Name + ".zip"));
		}
	});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3(string.Format("./tests/**/bin/{0}/*NUnit.Tests.dll", configuration), new NUnit3Settings {
        NoResults = true
        });


    XUnit2(string.Format("./tests/**/bin/{0}/*Tests.dll", configuration), new XUnit2Settings {
        XmlReport = true,
        OutputDirectory = buildDir
    });
});

//////////////////////////////////////////////////////////////////////
Task("Publish-Packages")
	.IsDependentOn("Packages")
	.DoesForEach(GetFiles(nugetDirname + "/*.nupkg"), (package)=> {

		// Push the package.
		NuGetPush(package, new NuGetPushSettings {
			Source = nugetServer,
			ApiKey = apiKey
		});

	});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Packages")
	.IsDependentOn("Websites")
	.IsDependentOn("Consoles");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
