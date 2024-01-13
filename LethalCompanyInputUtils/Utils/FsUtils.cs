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

    public static string AssetBundlesDir { get; } = GetAssetBundlesDir();

    public static void EnsureControlsDir()
    {
        if (!Directory.Exists(ControlsDir))
            Directory.CreateDirectory(ControlsDir);
    }

    private static string GetAssetBundlesDir()
    {
        string BadInstallError()
        {
            var msg =
                "InputUtils can't find it's required AssetBundles! This will cause many issues!\nThis either means your mod manager incorrectly installed InputUtils" +
                "or if you've manually installed InputUtils, you've done so incorrectly. If you manually installed don't bother reporting the issue, I only provide support to people who use mod managers.";
            
            Logging.Error(msg);
            return msg;
        }
        
        var dllLoc = BepInEx.Bootstrap.Chainloader.PluginInfos[LethalCompanyInputUtilsPlugin.ModId].Location;
        var parentDir = Directory.GetParent(dllLoc);

        if (parentDir is null)
            throw new NotSupportedException(BadInstallError());

        string assetBundlesDir = Path.Combine(parentDir.FullName, "AssetBundles");
        if (!Directory.Exists(assetBundlesDir))
            throw new NotSupportedException(BadInstallError());

        return assetBundlesDir;
    }
}