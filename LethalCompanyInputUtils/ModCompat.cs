using BepInEx;
using BepInEx.Bootstrap;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;

namespace LethalCompanyInputUtils;

internal static class ModCompat
{
    public static void Init(BaseUnityPlugin plugin)
    {
        if (Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility"))
            LoadLobbyCompatibilityCompat(plugin.Info.Metadata);
    }

    private static void LoadLobbyCompatibilityCompat(BepInPlugin plugin)
    {
        PluginHelper.RegisterPlugin(plugin.GUID, plugin.Version, CompatibilityLevel.ClientOnly, VersionStrictness.None);
    }
}