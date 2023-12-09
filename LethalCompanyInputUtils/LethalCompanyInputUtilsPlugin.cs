using System.Reflection;
using BepInEx;
using HarmonyLib;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LethalCompanyInputUtils;

[BepInPlugin(ModId, ModName, ModVersion)]
public class LethalCompanyInputUtilsPlugin : BaseUnityPlugin
{
    public const string ModId = "com.rune580.LethalCompanyInputUtils";
    public const string ModName = "Lethal Company Input Utils";
    public const string ModVersion = "0.1.0";

    private static LethalCompanyInputUtilsPlugin? _instance;

    private Harmony? _harmony;

    public TestActions InputTestActions;
    
    private void Awake()
    {
        _instance = this;
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModId);
        SceneManager.activeSceneChanged += OnSceneChanged;
        
        FsUtils.EnsureControlsDir();

        InputTestActions = new TestActions();
        
        InputTestActions.Enable();
        InputTestActions.EmoteWheel.Enable();
        
        InputTestActions.EmoteWheel.started += EmoteWheelCallback;
        InputTestActions.EmoteWheel.performed += EmoteWheelCallback;
        InputTestActions.EmoteWheel.canceled += EmoteWheelCallback;
        
        Logger.LogInfo($"Plugin {ModId} is loaded!");
    }

    private void EmoteWheelCallback(InputAction.CallbackContext context)
    {
        Logger.LogInfo("Emotema");
    }

    private static void OnSceneChanged(Scene current, Scene next)
    {
        var menuButtons = next.FindMenuButtons();
        if (!menuButtons)
            return;
        
        
    }
}
