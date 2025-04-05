using System.Reflection;
using BepInEx;
using HarmonyLib;
using LethalCompanyInputUtils.Components;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Glyphs;
using LethalCompanyInputUtils.Localization;
using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static BepInEx.BepInDependency.DependencyFlags;

namespace LethalCompanyInputUtils;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("ainavt.lc.lethalconfig", SoftDependency)]
[BepInDependency("BMX.LobbyCompatibility", SoftDependency)]
public class LethalCompanyInputUtilsPlugin : BaseUnityPlugin
{
    private static ExampleActions? _instance;
    
    private Harmony? _harmony;
    
    private void Awake()
    {
        Logging.SetLogSource(Logger);
        
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_GUID);
        SceneManager.activeSceneChanged += OnSceneChanged;
        InputSystem.onDeviceChange += OnDeviceChanged;
        
        LoadAssetBundles();
        
        ControllerGlyph.LoadGlyphs();
        
        FsUtils.EnsureRequiredDirs();
        
        InputUtilsConfig.Init(this);
        
        LocaleManager.LoadLocaleData();

        RegisterExtendedMouseLayout();
        
        ModCompat.Init(this);
        
        Logging.Info($"InputUtils {PluginInfo.PLUGIN_VERSION} has finished loading!");
        
        SceneManager.activeSceneChanged += TryExportLayoutsOnLoad;
        
        _instance = new ExampleActions();
        
        _instance.ExampleMovementAction.started += ExampleMovementActionOnperformed;
        _instance.ExampleMovementAction.performed += ExampleMovementActionOnperformed;
        _instance.ExampleMovementAction.canceled += ExampleMovementActionOnperformed;
        
        _instance.Enable();
    }

    private static void ExampleMovementActionOnperformed(InputAction.CallbackContext obj)
    {
        var axisValue = obj.ReadValue<Vector2>();
        
        Debug.Log($"ExampleMovementActionOnperformed {axisValue}");
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
    
    private static void TryExportLayoutsOnLoad(Scene arg0, Scene arg1)
    {
        SceneManager.activeSceneChanged -= TryExportLayoutsOnLoad;
        
        LayoutExporter.TryExportLayouts();
    }
}
