using System;
using System.IO;
using BepInEx;

namespace LethalCompanyInputUtils.Utils;

internal static class FsUtils
{
    public static string SaveDir { get; } = GetSaveDir();

    private static string GetSaveDir()
    {
        var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return Path.Combine(userDir, "AppData", "LocalLow", "ZeekerssRBLX", "Lethal Company");
    }

    public static string Pre041ControlsDir { get; } = Path.Combine(Paths.BepInExRootPath, "controls");
    public static string ControlsDir { get; } = Path.Combine(Paths.ConfigPath, "controls");

    public static void EnsureControlsDir()
    {
        if (!Directory.Exists(ControlsDir))
            Directory.CreateDirectory(ControlsDir);
    }
}