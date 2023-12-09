using System;
using System.IO;

namespace LethalCompanyInputUtils.Utils;

internal static class Paths
{
    public static string SaveDir { get; } = GetSaveDir();

    private static string GetSaveDir()
    {
        var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return Path.Combine(userDir, "AppData", "LocalLow", "ZeekerssRBLX", "Lethal Company");
    }

    public static string ControlsPath { get; } = Path.Combine(BepInEx.Paths.BepInExRootPath, "controls");
}