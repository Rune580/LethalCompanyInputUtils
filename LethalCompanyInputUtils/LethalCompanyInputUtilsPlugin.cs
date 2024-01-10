using System.Reflection;
using BepInEx;
using HarmonyLib;
using LethalCompanyInputUtils.Glyphs;
using LethalCompanyInputUtils.Utils;
using UnityEngine.SceneManagement;

namespace LethalCompanyInputUtils;

[BepInPlugin(ModId, ModName, ModVersion)]
public class LethalCompanyInputUtilsPlugin : BaseUnityPlugin
{
    public const string ModId = "com.rune580.LethalCompanyInputUtils";
    public const string ModName = "Lethal Company Input Utils";
    public const string ModVersion = "0.4.4";

    private Harmony? _harmony;
    
    private void Awake()
    {
        Logging.SetLogSource(Logger);
        
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModId);
        SceneManager.activeSceneChanged += OnSceneChanged;
        
        FsUtils.EnsureControlsDir();
        
        LoadAssetBundles();
        LoadControllerGlyphs();
        
        Logger.LogInfo($"Plugin {ModId} is loaded!");
    }

    private void LoadAssetBundles()
    {
        Assets.AddBundle("ui-assets");
    }

    private void LoadControllerGlyphs()
    {
        Assets.Load<ControllerGlyph>("controller glyphs/dualsense glyphs.asset");
    }

    private static void OnSceneChanged(Scene current, Scene next)
    {
        LcInputActionApi.ResetLoadedInputActions();
    }
}
