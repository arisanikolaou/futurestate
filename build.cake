#addin "nuget:?package=Cake.ArgumentHelpers"
#addin "nuget:?package=Cake.SemVer"
#addin "nuget:?package=semver&version=2.0.4"
#addin "nuget:?package=Cake.Git"
#addin "nuget:?package=Cake.AppVeyor"
#addin "nuget:?package=Refit&version=3.0.0"
#addin "nuget:?package=Newtonsoft.Json&version=9.0.1"
#addin "Cake.XdtTransform"
#addin "Cake.FileHelpers"
#addin "System.Net.Http"

#tool nuget:?package=vswhere
#tool "xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configurationName = "Release";
var configuration = Argument<string>("configuration", configurationName);
var solutionFile = GetFiles("./*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));
var distDir = Directory("./dist");
var nugetDirname = "./nuget";
var nugetDir = Directory(nugetDirname);
var buildDir = Directory("./build");
var testDir = Directory("./testOutput");
var defaultVersion = "0.3.0";
var solutionVersion = Argument<string>("BUILD_VERSION",defaultVersion);

// nuget get
var nugetServer = "https://www.nuget.org";
var apiKey = EnvironmentVariable("NUGET_APIKEY");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Information("Starting build: " + solutionVersion);
Information("Nuget Api Key: " + apiKey);

Task("Clean")
	.Does(() => 
	{
		CleanDirectory(buildDir);
		CleanDirectory(distDir);
		CleanDirectory(nugetDir);
	});

Task("UpdateAssemblyInfo")
	.Does(() =>
{
	var versionInfo = GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = true,
		OutputType = GitVersionOutput.BuildServer
	});

	Information(versionInfo);
});

Task("SetVersion")
	.IsDependentOn("UpdateAssemblyInfo")
	.Does(() => {
		Information("Updating assembly details.");
	   
		ReplaceRegexInFiles("./src/**/**/AssemblyInfo*.cs", 
						   "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))", 
						   solutionVersion);

		ReplaceRegexInFiles("./src/**/**/AssemblyInfo*.cs", 
						   "(?<=AssemblyFileVersion\\(\")(.+?)(?=\"\\))", 
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

	Information(@"Building from commit {0}
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

		// find latest ms build/tools
		DirectoryPath vsLatest  = VSWhereLatest();

		FilePath msBuildPathX64 = (vsLatest==null)
									? null
									: vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/amd64/MSBuild.exe");
									
		var settings = new MSBuildSettings() 
		{
			MaxCpuCount = 0, // use all processors
			ToolPath = msBuildPathX64,
			Verbosity = Verbosity.Minimal,
			Configuration = configuration
		};

		// rebuild
		settings.WithTarget("Rebuild");

		// Use MSBuild
		MSBuild(solutionFile, settings);
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

			var assemblyInfo = ParseAssemblyInfo(project.GetDirectory().CombineWithFilePath("./Properties/AssemblyInfo.cs"));
			var assemblyVersion = ParseSemVer(assemblyInfo.AssemblyVersion); 
			var packageVersion = assemblyVersion;

			Information("Package version: " + packageVersion.ToString());

			NuGetPack(project, new NuGetPackSettings
			{
				OutputDirectory = nugetDir,
				IncludeReferencedProjects = true,
				Version  = packageVersion.ToString(),
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
	XUnit2(string.Format("./tests/**/bin/{0}/*.Tests.dll", configuration), new XUnit2Settings {
		XmlReport = true,
		UseX86 = false,
		HtmlReport = true,
		OutputDirectory = testDir
	});
});

//////////////////////////////////////////////////////////////////////

// publish packages to nuget server
Task("Publish-Packages")
	.IsDependentOn("Packages")
	.DoesForEach(GetFiles(nugetDirname + "/*.nupkg"), (package)=> {

		Information("Publishing Packages Api Key: " + apiKey);

		// Push the package.
		NuGetPush(package, new NuGetPushSettings {
			Source = nugetServer,
			ApiKey = apiKey
		});

	});

  // publish artefacts on appveyor	
  Task("Artefacts")
	.IsDependentOn("Packages")
	.Does(() =>
	{
		if (AppVeyor.IsRunningOnAppVeyor)
		{
			Information("Deploying artefacts on AppVeyor.");

			foreach (var file in GetFiles(distDir))
				AppVeyor.UploadArtifact(file.FullPath);

			foreach (var file in GetFiles(nugetDirname + "/*.nupkg"))
				AppVeyor.UploadArtifact(file.FullPath);
		}

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
