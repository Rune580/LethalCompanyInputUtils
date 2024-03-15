using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using LethalCompanyInputUtils.Components;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Glyphs;
using LethalCompanyInputUtils.Localization;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.SceneManagement;

namespace LethalCompanyInputUtils;

[BepInPlugin(ModId, ModName, ModVersion)]
[BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
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

        DebugDeviceLayouts();
        
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

    private static void DebugDeviceLayouts()
    {
        Logging.Info("Listing Device Layouts");
        
        foreach (var layoutName in InputSystem.ListLayouts())
        {
            LogLayout(InputSystem.LoadLayout(layoutName));
        }
        
        InputSystem.onDeviceChange += InputSystemOnDeviceChange;
    }

    private static void InputSystemOnDeviceChange(InputDevice device, InputDeviceChange status)
    {
        var descBuilder = new StringBuilder();

        descBuilder.AppendLine($"\tProduct: {device.description.product}");
        descBuilder.AppendLine($"\tSerial: {device.description.serial}");
        descBuilder.AppendLine($"\tManufacturer: {device.description.manufacturer}");
        descBuilder.AppendLine($"\tVersion: {device.description.version}");
        descBuilder.AppendLine($"\tDeviceClass: {device.description.deviceClass}");
        descBuilder.AppendLine($"\tCapabilities: {device.description.capabilities}");
        
        Logging.Info($"{device.name}\n{descBuilder}\n\tDevice specified Layout {device.layout}");
        LogLayout(InputSystem.LoadLayout(device.layout));

        var matchingLayout = InputSystem.TryFindMatchingLayout(device.description);
        Logging.Info($"\tBest Matching Layout {matchingLayout}");
        LogLayout(InputSystem.LoadLayout(matchingLayout));
    }

    private static void LogLayout(InputControlLayout layout)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Layout: {layout.name.ToString()}\n\tControls:");
            
        foreach (var control in layout.controls)
        {
            builder.AppendLine($"\t\t{control.name.ToString()}");
        }
            
        Logging.Info(builder.ToString());
    }
}
