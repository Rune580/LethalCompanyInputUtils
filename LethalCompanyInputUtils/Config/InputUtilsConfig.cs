using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using LethalCompanyInputUtils.Localization;
using LethalCompanyInputUtils.Utils;
using LethalConfig;
using LethalConfig.ConfigItems;

namespace LethalCompanyInputUtils.Config;

public static class InputUtilsConfig
{
    public static ConfigEntry<BindingOverridePriority> bindingOverridePriority = null!;
    public static ConfigEntry<string> localeKey = null!;
    
    internal static void Init(BaseUnityPlugin plugin)
    {
        var persistentConfig = new ConfigFile(FsUtils.PersistentConfigPath, true, plugin.Info.Metadata);

        bindingOverridePriority = persistentConfig.Bind(
            "General",
            "Binding Overrides Priority",
            BindingOverridePriority.GlobalThenLocal,
            "Determines the priority when loading controls.\n\n" +
            "\tGlobalThenLocal: Global defined controls take priority over Local/Profile/ModPack defined controls\n" +
            "\tLocalThenGlobal: Local/Profile/ModPack defined controls take priority over Global defined controls\n" +
            "\tGlobalOnly: Only loads Global defined controls\n" +
            "\tLocalOnly: Only loads Local/Profile/ModPack defined controls\n"
        );

        localeKey = plugin.Config.Bind(
            "General",
            "Locale",
            "en_US",
            "Valid Locales can be found in the 'Locale' folder where InputUtils is installed at.\n" +
            "Can reload live in-game when using LethalConfig."
        );

        localeKey.SettingChanged += (_, _) => LocaleManager.LoadLocaleData();

        if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
            LethalConfigSetup();
    }

    private static void LethalConfigSetup()
    {
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(localeKey, false));
    }
}