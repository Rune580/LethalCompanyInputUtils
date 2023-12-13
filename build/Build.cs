using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using build.Utils;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
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
    public string ManifestAuthor { get; }

    #endregion

    
    #region Env

    public AbsolutePath SolutionPath { get; }
    public bool UseStubbedLibs { get; }
    public AbsolutePath[] DeployTargets { get; }

    #endregion

    public readonly AbsolutePath GameReferencesDir = new AbsolutePath("../") / ".gameReferences";
    public AbsolutePath BuildDir { get; }
    
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

[TaskName("Build")]
[IsDependentOn(typeof(RestoreTask))]
[IsDependentOn(typeof(FetchReferences))]
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

[TaskName("DeployUnity")]
[IsDependentOn(typeof(BuildTask))]
public sealed class DeployToUnity : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        AbsolutePath unityPkgDir = (AbsolutePath)"../" / "Unity-LethalCompanyInputUtils" / "Packages";
        
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

        foreach (var target in context.DeployTargets)
        {
            AbsolutePath destDir = target / project.Name;
            destDir.EnsureDirectoryExists();
            
            context.BuildDir.GlobFiles("*.dll", "*.pdb")
                .CopyFilesTo(destDir);
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
        
        var project = context.Project;
        
        AbsolutePath publishDir = context.BuildDir / "publish";
        publishDir.CleanAndCreateDirectory();

        var modDir = publishDir / project.Name;
        modDir.CreateDirectory();
            
        context.BuildDir.GlobFiles("*.dll")
            .CopyFilesTo(modDir);
            
        File.Copy("../" / manifestFile, publishDir / manifestFile, true);
        File.Copy("../" / iconFile, publishDir / iconFile, true);
        File.Copy("../" / readmeFile, publishDir / readmeFile, true);

        var manifest = JsonSerializer.Deserialize<ThunderStoreManifest>(File.ReadAllText(publishDir / manifestFile));

        var uploadDir = context.BuildDir / "upload";
        uploadDir.CleanAndCreateDirectory();

        var version = context.Version ?? manifest!.version_number;
        var destFile = uploadDir / $"{context.ManifestAuthor}-{manifest!.name}-{version}.zip";
        destFile.DeleteFile();
        
        ZipFile.CreateFromDirectory(publishDir, destFile);
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildTask))]
public class DefaultTask : FrostingTask;