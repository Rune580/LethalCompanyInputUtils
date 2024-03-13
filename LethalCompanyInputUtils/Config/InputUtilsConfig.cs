using BepInEx;
using BepInEx.Configuration;
using LethalCompanyInputUtils.Utils;

namespace LethalCompanyInputUtils.Config;

public static class InputUtilsConfig
{
    private static ConfigEntry<BindingOverridePriority>? _bindingOverridePriority;

    public static BindingOverridePriority BindingOverridePriority => _bindingOverridePriority!.Value;
    
    internal static void Init(BepInPlugin plugin)
    {
        var persistentConfig = new ConfigFile(FsUtils.PersistentConfigPath, true, plugin);

        _bindingOverridePriority = persistentConfig.Bind(
            "General",
            "Binding Overrides Priority",
            BindingOverridePriority.GlobalThenLocal,
            "Determines the priority when loading controls.\n\n" +
            "\tGlobalThenLocal: Global defined controls take priority over Local/Profile/ModPack defined controls\n" +
            "\tLocalThenGlobal: Local/Profile/ModPack defined controls take priority over Global defined controls\n" +
            "\tGlobalOnly: Only loads Global defined controls\n" +
            "\tLocalOnly: Only loads Local/Profile/ModPack defined controls\n"
        );
    }
}