using System.Reflection;
using BepInEx;
using HarmonyLib;
using LethalCompanyInputUtils.Components;
using LethalCompanyInputUtils.Glyphs;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LethalCompanyInputUtils;

[BepInPlugin(ModId, ModName, ModVersion)]
public class LethalCompanyInputUtilsPlugin : BaseUnityPlugin
{
    public const string ModId = "com.rune580.LethalCompanyInputUtils";
    public const string ModName = "Lethal Company Input Utils";
    public const string ModVersion = "0.5.3";

    private Harmony? _harmony;
    
    private void Awake()
    {
        Logging.SetLogSource(Logger);
        
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModId);
        SceneManager.activeSceneChanged += OnSceneChanged;
        InputSystem.onDeviceChange += OnDeviceChanged;
        
        LoadAssetBundles();
        LoadControllerGlyphs();
        
        FsUtils.EnsureControlsDir();
        
        Logging.Info($"InputUtils {ModVersion} has finished loading!");
    }

    private void LoadAssetBundles()
    {
        Assets.AddBundle("ui-assets");
    }

    private void LoadControllerGlyphs()
    {
        Assets.Load<ControllerGlyph>("controller glyphs/xbox series x glyphs.asset");
        Assets.Load<ControllerGlyph>("controller glyphs/dualsense glyphs.asset");
    }

    private static void OnSceneChanged(Scene current, Scene next)
    {
        LcInputActionApi.ResetLoadedInputActions();

        BindsListController.OffsetCompensation = next.name != "MainMenu" ? 20 : 0;
    }
    
    private static void OnDeviceChanged(InputDevice device, InputDeviceChange state)
    {
        RebindButton.ReloadGlyphs();
    }
}
