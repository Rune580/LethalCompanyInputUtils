using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using build.Utils;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Core;
using Cake.Frosting;

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
public class BuildContext(ICakeContext context) : FrostingContext(context)
{
    #region Arguments

    public readonly string MsBuildConfiguration = context.Argument<string>("configuration", "Debug");
    public readonly AbsolutePath GameDir = context.Arg("gameDir");

    #endregion

    #region Project

    private string? _solutionPath;

    public string SolutionPath
    {
        get
        {
            _solutionPath ??= context.GetFiles("../*.sln")
                .First()
                .FullPath;

            return _solutionPath;
        }
    }

    private SolutionParserResult? _solution;

    public SolutionParserResult Solution
    {
        get
        {
            _solution ??= context.ParseSolution(SolutionPath);

            return _solution;
        }
    }

    #endregion

    public readonly AbsolutePath GameReferencesDir = new AbsolutePath("../") / ".gameReferences";

    public readonly string[] References =
    {
        "Assembly-CSharp.dll",
        "Newtonsoft.Json.dll",
        "Unity.InputSystem.dll"
    };
}

[TaskName("FetchRefs")]
public sealed class FetchReferences : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (!Directory.Exists(context.GameReferencesDir))
            Directory.CreateDirectory(context.GameReferencesDir);
        
        AbsolutePath srcDir = context.GameDir / "Lethal Company_Data" / "Managed";

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

[TaskName("Deploy")]
[IsDependentOn(typeof(BuildTask))]
public sealed class DeployToGame : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (!Directory.Exists(context.GameDir))
            throw new Exception("Please make sure the game directory actually exists!");

        foreach (var project in context.Solution.Projects)
        {
            if (string.Equals(project.Name, "build"))
                continue;
            
            AbsolutePath buildDir = (AbsolutePath)project.Path.GetDirectory() / "bin" / context.MsBuildConfiguration / "netstandard2.1";
            AbsolutePath destDir = context.GameDir / "BepInEx" / "plugins" / project.Name;

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            
            buildDir.GlobFiles("*.dll", "*.pdb")
                .ForEach(file =>
                {
                    var destFile = destDir / file.Name;
                    File.Copy(file, destFile, true);
                });
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

[TaskName("Default")]
[IsDependentOn(typeof(BuildTask))]
public class DefaultTask : FrostingTask;