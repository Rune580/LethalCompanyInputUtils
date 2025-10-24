using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using LethalCompanyInputUtils.Localization;
using LethalCompanyInputUtils.Utils;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;

namespace LethalCompanyInputUtils.Config;

public static class InputUtilsConfig
{
    public static ConfigEntry<BindingOverridePriority> bindingOverridePriority = null!;
    public static ConfigEntry<string> localeKey = null!;
    public static ConfigEntry<bool> patchInputManagerUseSaferRegisterOfCustomTypes = null!;
    
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

        patchInputManagerUseSaferRegisterOfCustomTypes = plugin.Config.Bind(
            "Patches",
            "Patch InputManager to use a Safer RegisterCustomTypes method",
            true,
            "This may fix issues where Unity attempts to load optional dependencies from mods that are not installed.\n" +
            "This could potentially increase the start up time.\n"+
            "Restart required in order to take effect"
        );

        if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
            LethalConfigSetup();
    }

    private static void LethalConfigSetup()
    {
        var localeDropdownOptions = new TextDropDownOptions(LocaleManager.GetAllLocales())
        {
            RequiresRestart = false,
        };
        
        LethalConfigManager.AddConfigItem(new TextDropDownConfigItem(localeKey, localeDropdownOptions));
    }
}