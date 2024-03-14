using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using BepInEx.AssemblyPublicizer;
using build.Utils;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Core;
using Cake.Frosting;
using dotenv.net;

namespace build;

public static class Build
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class BuildContext : FrostingContext
{
    #region Arguments

    public readonly string MsBuildConfiguration;
    public AbsolutePath? GameDir { get; }
    
    public readonly string? Version;

    #endregion

    
    #region Settings

    public string[] References { get; }
    public CSharpProject Project { get; }
    public AbsolutePath UnityProjectDir { get; }
    public string ManifestAuthor { get; }

    #endregion

    
    #region Env

    public AbsolutePath SolutionPath { get; }
    public bool UseStubbedLibs { get; }
    public AbsolutePath[] DeployTargets { get; }

    #endregion

    public readonly AbsolutePath GameReferencesDir = new AbsolutePath("../") / ".gameReferences";
    public AbsolutePath BuildDir { get; }
    public AbsolutePath UnityAssetBundlesDir { get; }
    
    public BuildContext(ICakeContext context) : base(context)
    {
        DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { "../.env" }));
        
        MsBuildConfiguration = context.Argument<string>("configuration", "Debug");
        Version = context.EnvironmentVariable("RELEASE_VERSION");
        
        SolutionPath = context.GetFiles("../*.sln")
            .First()
            .FullPath;
        
        var settings = ProjectBuildSettings.LoadFromFile("../build-settings.json");
        if (settings is null)
            throw new InvalidOperationException();
        
        var projectFilePath = (AbsolutePath)"../" / settings.ProjectFile;
        References = settings.References;
        Project = new CSharpProject(projectFilePath);
        UnityProjectDir = (AbsolutePath)"../" / settings.UnityProjectDir;
        ManifestAuthor = settings.ManifestAuthor;
        
        UseStubbedLibs = context.Environment.GetEnvironmentVariable("USE_STUBBED_LIBS") is not null;
        GameDir = GetGameDirArg(context);
        
        string deployTargetEnv = context.Environment.GetEnvironmentVariable("DEPLOY_TARGETS");
        if (deployTargetEnv is not null)
        {
            DeployTargets = deployTargetEnv
                .Split(";")
                .Select(dir => new AbsolutePath(dir))
                .ToArray();
        }
        else
        {
            DeployTargets = [];
        }

        BuildDir = Project.Directory / "bin" / MsBuildConfiguration / "netstandard2.1";
        UnityAssetBundlesDir = UnityProjectDir / "AssetBundles" / "StandaloneWindows";
    }

    private AbsolutePath? GetGameDirArg(ICakeContext context)
    {
        return UseStubbedLibs ? null : new AbsolutePath(context.Arg("gameDir"));
    }
}

[TaskName("FetchRefs")]
public sealed class FetchReferences : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return !context.UseStubbedLibs;
    }

    public override void Run(BuildContext context)
    {
        context.GameReferencesDir.EnsureDirectoryExists();
        
        AbsolutePath srcDir = context.GameDir! / "Lethal Company_Data" / "Managed";

        foreach (var reference in context.References)
        {
            AbsolutePath srcFile = srcDir / reference;
            AbsolutePath dstFile = context.GameReferencesDir / reference;
            
            File.Copy(srcFile, dstFile, true);
        }
    }
}

[TaskName("Restore")]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(context.SolutionPath);
    }
}

[TaskName("UpdateAssetBundles")]
public sealed class UpdateAssetBundles : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.UnityAssetBundlesDir.GlobFiles("ui-assets")
            .CopyFilesTo(context.Project.Directory / "AssetBundles");
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(RestoreTask))]
[IsDependentOn(typeof(FetchReferences))]
[IsDependentOn(typeof(UpdateAssetBundles))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild(context.SolutionPath, new DotNetBuildSettings
        {
            Configuration = context.MsBuildConfiguration
        });
    }
}

[TaskName("StubAndDeployRequiredDepsToUnity")]
public sealed class StubAndDeployRequiredUnityDeps : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        if (context.GameDir is null)
            return false;
        
        AbsolutePath destDir = context.UnityProjectDir / "Packages" / context.Project.Name;
        if (!Directory.Exists(destDir))
            return true;

        string[] depsToFind = [ "BepInEx.dll", "0Harmony.dll" ];
        var stubbedDeps = destDir.GlobFiles(depsToFind);
        if (stubbedDeps.Count != depsToFind.Length)
            return true;

        return false;
    }

    public override void Run(BuildContext context)
    {
        AbsolutePath destDir = context.UnityProjectDir / "Packages" / context.Project.Name;
        destDir.EnsureDirectoryExists();
        
        var coreDir = context.GameDir! / "BepInEx" / "core";
        var depsToStub = coreDir.GlobFiles("BepInEx.dll", "0Harmony.dll");

        var stubOptions = new AssemblyPublicizerOptions
        {
            Strip = true,
            IncludeOriginalAttributesAttribute = false
        };
        
        foreach (var dep in depsToStub)
            AssemblyPublicizer.Publicize(dep, destDir / dep.Name, stubOptions);
    }
}

[TaskName("DeployUnity")]
[IsDependentOn(typeof(BuildTask))]
[IsDependentOn(typeof(StubAndDeployRequiredUnityDeps))]
public sealed class DeployToUnity : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        AbsolutePath unityPkgDir = context.UnityProjectDir / "Packages";
        
        var project = context.Project;
        
        AbsolutePath destDir = unityPkgDir / project.Name;
        destDir.EnsureDirectoryExists();
            
        context.BuildDir.GlobFiles("*.dll", "*.pdb")
            .CopyFilesTo(destDir);
    }
}

[TaskName("Deploy")]
[IsDependentOn(typeof(BuildTask))]
public sealed class DeployToGame : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.GameDir is not null;
    }

    public override void Run(BuildContext context)
    {
        var project = context.Project;
        var assetBundlesDir = context.BuildDir / "AssetBundles";
        var localeDir = context.BuildDir / "Locale";

        foreach (var target in context.DeployTargets)
        {
            AbsolutePath destDir = target / project.Name;
            destDir.EnsureDirectoryExists();
            
            context.BuildDir.GlobFiles("*.dll", "*.pdb")
                .CopyFilesTo(destDir);

            assetBundlesDir.GlobFiles("*")
                .CopyFilesTo(destDir / "AssetBundles");
            
            localeDir.GlobFiles("*.json")
                .CopyFilesTo(destDir / "Locale");
        }
    }
}

[TaskName("DebugMod")]
[IsDependentOn(typeof(DeployToGame))]
public sealed class DebugMod : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        string host;
        var args = "start steam://rungameid/1966720";
        
        if (OperatingSystem.IsWindows())
        {
            host = "cmd.exe";
            args = $"/C {args}";
        }
        else
        {
            host = "/bin/bash";
            args = $"-c \"{args}\"";
        }
        
        using var startGame = new Process();
        startGame.StartInfo.FileName = host;
        startGame.StartInfo.Arguments = args;
        startGame.StartInfo.CreateNoWindow = false;
        startGame.StartInfo.UseShellExecute = true;
        startGame.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

        startGame.Start();
        startGame.WaitForExit();
    }
}

[TaskName("BuildThunderstore")]
[IsDependentOn(typeof(BuildTask))]
public sealed class BuildThunderstorePackage : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        AbsolutePath manifestFile = "manifest.json";
        AbsolutePath iconFile = "icon.png";
        AbsolutePath readmeFile = "README.md";
        AbsolutePath changelogFile = "CHANGELOG.md";
        
        var project = context.Project;
        
        AbsolutePath publishDir = context.BuildDir / "publish";
        publishDir.CleanAndCreateDirectory();

        var modDir = publishDir / "plugins" / project.Name;
        modDir.CreateDirectory();
            
        context.BuildDir.GlobFiles("*.dll", "*.pdb")
            .CopyFilesTo(modDir);

        var assetBundlesDir = context.BuildDir / "AssetBundles";
        assetBundlesDir.GlobFiles("*")
            .CopyFilesTo(modDir / "AssetBundles");
        
        var localeDir = context.BuildDir / "Locale";
        localeDir.GlobFiles("*.json")
            .CopyFilesTo(modDir / "Locale");
        
        File.Copy("../" / manifestFile, publishDir / manifestFile, true);
        File.Copy("../" / iconFile, publishDir / iconFile, true);
        File.Copy("../" / readmeFile, publishDir / readmeFile, true);
        File.Copy("../" / changelogFile, publishDir / changelogFile, true);

        var manifest = JsonSerializer.Deserialize<ThunderStoreManifest>(File.ReadAllText(publishDir / manifestFile));

        var uploadDir = context.BuildDir / "upload";
        uploadDir.CleanAndCreateDirectory();

        var version = context.Version ?? manifest!.version_number;
        var destFile = uploadDir / $"{context.ManifestAuthor}-{manifest!.name}-{version}.zip";
        destFile.DeleteFile();
        
        ZipFile.CreateFromDirectory(publishDir, destFile);
    }
}

[TaskName("BuildNuGet")]
[IsDependentOn(typeof(BuildTask))]
public sealed class BuildNuGetPackage : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        AbsolutePath manifestFile = (AbsolutePath)"../" / "manifest.json";
        var manifest = JsonSerializer.Deserialize<ThunderStoreManifest>(File.ReadAllText(manifestFile));
        if (manifest is null)
            throw new InvalidOperationException();

        AbsolutePath readmeFile = (AbsolutePath)"../" / "README.md";
        File.Copy(readmeFile, context.BuildDir / "README.md", true);
        
        AbsolutePath changelogFile = (AbsolutePath)"../" / "CHANGELOG.md";
        File.Copy(changelogFile, context.BuildDir / "CHANGELOG.md", true);
        
        AbsolutePath iconFile = (AbsolutePath)"../" / "icon.png";
        File.Copy(iconFile, context.BuildDir / "icon.png", true);

        AbsolutePath licenseFile = (AbsolutePath)"../" / "LICENSE";
        File.Copy(licenseFile, context.BuildDir / "LICENSE", true);
        
        var dllFile = $"{context.Project.Name}.dll";
        var nuspecContent = $"""
                              <?xml version="1.0" encoding="utf-8"?>
                              <package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
                                  <metadata>
                                      <id>{context.ManifestAuthor}.{manifest.name}</id>
                                      <version>{manifest.version_number}</version>
                                      <description>{manifest.description}</description>
                                      <authors>{context.ManifestAuthor}</authors>
                                      <projectUrl>{manifest.website_url}</projectUrl>
                                      <readme>README.md</readme>
                                      <iconUrl>https://cdn.rune580.dev/icons/lethalcompany_inpututils/icon.png</iconUrl>
                                      <icon>icon.png</icon>
                                      <license type="file">LICENSE</license>
                                  </metadata>
                                  <files>
                                      <file src="{dllFile}" target="lib/{dllFile}" />
                                      <file src="README.md" />
                                      <file src="CHANGELOG.md" />
                                      <file src="icon.png" />
                                      <file src="LICENSE" />
                                  </files>
                              </package>
                              """;

        var nuspecFile = context.BuildDir / $"{context.Project.Name}.nuspec";
        File.WriteAllText(nuspecFile, nuspecContent);

        var msBuildSettings = new DotNetMSBuildSettings
        {
            Properties =
            {
                ["NuspecFile"] = [nuspecFile]
            }
        };

        var packSettings = new DotNetPackSettings
        {
            NoBuild = true,
            Configuration = "Release",
            OutputDirectory = (string)(context.BuildDir / "artifacts"),
            MSBuildSettings = msBuildSettings
        };
        
        context.DotNetPack(context.Project.Directory, packSettings);
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildTask))]
public class DefaultTask : FrostingTask;