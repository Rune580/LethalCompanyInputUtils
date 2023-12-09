using System.IO;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils;

public static class LcInputActionApi
{
    internal static InputActionAsset Init(InputActionAsset origAsset)
    {
        InputActionAsset asset = LoadInputActionAsset(origAsset);

        return asset;
    }

    private static InputActionAsset LoadInputActionAsset(InputActionAsset asset)
    {
        string controlsPath = Path.Join(FsUtils.SaveDir, "controls.json");

        if (!File.Exists(controlsPath))
            File.WriteAllText(controlsPath, asset.SaveBindingOverridesAsJson());
        
        try
        {
            asset.LoadBindingOverridesFromJson(File.ReadAllText(controlsPath));
            return asset;
        }
        catch
        {
            File.WriteAllText(controlsPath, asset.SaveBindingOverridesAsJson());
            return asset;
        }
    }
}