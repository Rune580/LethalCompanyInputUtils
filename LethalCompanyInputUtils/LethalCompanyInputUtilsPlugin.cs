using System.Reflection;
using BepInEx;
using HarmonyLib;
using LethalCompanyInputUtils.Components;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Glyphs;
using LethalCompanyInputUtils.Localization;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LethalCompanyInputUtils;

[BepInPlugin(ModId, ModName, ModVersion)]
public class LethalCompanyInputUtilsPlugin : BaseUnityPlugin
{
    public const string ModId = "com.rune580.LethalCompanyInputUtils";
    public const string ModName = "Lethal Company Input Utils";
    public const string ModVersion = "0.7.1";

    private Harmony? _harmony;
    
    private void Awake()
    {
        Logging.SetLogSource(Logger);
        
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModId);
        SceneManager.activeSceneChanged += OnSceneChanged;
        InputSystem.onDeviceChange += OnDeviceChanged;
        
        LoadAssetBundles();
        
        ControllerGlyph.LoadGlyphs();
        
        FsUtils.EnsureRequiredDirs();
        
        InputUtilsConfig.Init(this);
        
        LocaleManager.LoadLocaleData();

        RegisterExtendedMouseLayout();
        
        Logging.Info($"InputUtils {ModVersion} has finished loading!");
    }

    private static void LoadAssetBundles()
    {
        Assets.AddBundle("ui-assets");
    }

    private static void OnSceneChanged(Scene current, Scene next)
    {
        LcInputActionApi.ResetLoadedInputActions();
        CameraUtils.ClearUiCameraReference();

        BindsListController.OffsetCompensation = next.name != "MainMenu" ? 20 : 0;
    }
    
    private static void OnDeviceChanged(InputDevice device, InputDeviceChange state)
    {
        RebindButton.ReloadGlyphs();
    }
    
    private static void RegisterExtendedMouseLayout()
    {
        string extendedMouseJson = """
                               {
                                  "name": "InputUtilsExtendedMouse",
                                  "extend": "Mouse",
                                  "controls": [
                                      {
                                          "name": "scroll/up",
                                          "layout": "Button",
                                          "useStateFrom": "scroll/up",
                                          "format": "BIT",
                                          "synthetic": true
                                      },
                                      {
                                          "name": "scroll/down",
                                          "layout": "Button",
                                          "useStateFrom": "scroll/down",
                                          "format": "BIT",
                                          "synthetic": true
                                      },
                                      {
                                          "name": "scroll/left",
                                          "layout": "Button",
                                          "useStateFrom": "scroll/left",
                                          "format": "BIT",
                                          "synthetic": true
                                      },
                                      {
                                          "name": "scroll/right",
                                          "layout": "Button",
                                          "useStateFrom": "scroll/right",
                                          "format": "BIT",
                                          "synthetic": true
                                      }
                                  ]
                               }
                               """;
        
        InputSystem.RegisterLayoutOverride(extendedMouseJson);
        Logging.Info("Registered InputUtilsExtendedMouse Layout Override!");
    }
}
