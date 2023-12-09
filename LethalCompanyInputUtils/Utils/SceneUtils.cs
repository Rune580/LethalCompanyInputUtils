using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalCompanyInputUtils.Utils;

internal static class SceneUtils
{
    public static GameObject? FindMenuButtons(this Scene scene)
    {
        var canvas = scene
            .GetRootGameObjects()
            .First(gameObject => gameObject.name == "Canvas");
        
        if (canvas)
        {
            return canvas.transform
                .Find("MenuContainer")
                .Find("MainButtons")
                .gameObject;
        }

        var systems = scene
            .GetRootGameObjects()
            .First(gameObject => gameObject.name == "Systems");

        if (systems)
        {
            return systems.transform
                .Find("UI")
                .Find("Canvas")
                .Find("QuickMenu")
                .Find("MainButtons")
                .gameObject;
        }

        return null;
    }
}