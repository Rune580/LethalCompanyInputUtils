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

    private static string? _assetBundlesDir;

    public static string AssetBundlesDir
    {
        get
        {
            _assetBundlesDir ??= GetAssetBundlesDir();

            // This occurs when another mod depends on InputUtils without using a BepInDependency attribute.
            if (string.IsNullOrEmpty(_assetBundlesDir))
            {
                var mods = BepInEx.Bootstrap.Chainloader.PluginInfos.ToPrettyString();
                Logging.Warn($"InputUtils is loading in an invalid state!\n\tOne of the following mods may be the culprit:\n{mods}");
                
                return "";
            }

            return _assetBundlesDir;
        }
    }
    
    private static string? GetAssetBundlesDir()
    {
        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(LethalCompanyInputUtilsPlugin.ModId, out var pluginInfo))
            return null;
        
        var dllLoc = pluginInfo.Location;
        var parentDir = Directory.GetParent(dllLoc);

        if (parentDir is null)
            throw new NotSupportedException(BadInstallError());

        string assetBundlesDir = Path.Combine(parentDir.FullName, "AssetBundles");
        if (!Directory.Exists(assetBundlesDir))
            throw new NotSupportedException(BadInstallError());

        return assetBundlesDir;

        string BadInstallError()
        {
            var msg =
                "InputUtils can't find it's required AssetBundles! This will cause many issues!\nThis either means your mod manager incorrectly installed InputUtils" +
                "or if you've manually installed InputUtils, you've done so incorrectly. If you manually installed don't bother reporting the issue, I only provide support to people who use mod managers.";
            
            Logging.Error(msg);
            return msg;
        }
    }

    private static string? _localeDir;

    public static string LocaleDir
    {
        get
        {
            _localeDir ??= GetLocaleDir();

            // This occurs when another mod depends on InputUtils without using a BepInDependency attribute.
            if (string.IsNullOrEmpty(_localeDir))
            {
                var mods = BepInEx.Bootstrap.Chainloader.PluginInfos.ToPrettyString();
                Logging.Warn($"InputUtils is loading in an invalid state!\n\tOne of the following mods may be the culprit:\n{mods}");
                
                return "";
            }

            return _localeDir;
        }
    }

    private static string? GetLocaleDir()
    {
        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(LethalCompanyInputUtilsPlugin.ModId, out var pluginInfo))
            return null;
        
        var dllLoc = pluginInfo.Location;
        var parentDir = Directory.GetParent(dllLoc);

        if (parentDir is null)
            throw new NotSupportedException(BadInstallError());

        string localeDir = Path.Combine(parentDir.FullName, "Locale");
        if (!Directory.Exists(localeDir))
            throw new NotSupportedException(BadInstallError());

        return localeDir;
        
        string BadInstallError()
        {
            var msg =
                "InputUtils can't find it's required AssetBundles! This will cause many issues!\nThis either means your mod manager incorrectly installed InputUtils" +
                "or if you've manually installed InputUtils, you've done so incorrectly. If you manually installed don't bother reporting the issue, I only provide support to people who use mod managers.";
            
            Logging.Error(msg);
            return msg;
        }
    }

    public static void EnsureControlsDir()
    {
        if (!Directory.Exists(ControlsDir))
            Directory.CreateDirectory(ControlsDir);
    }
}