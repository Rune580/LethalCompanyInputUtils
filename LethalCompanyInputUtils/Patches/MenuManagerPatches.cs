using HarmonyLib;
using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils.Patches;

public static class MenuManagerPatches
{
    [HarmonyPatch(typeof(MenuManager), "Awake")]
    public static class AwakePatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix(MenuManager __instance)
        {
            var parent = __instance.menuAnimator.gameObject.transform;
            
            Object.Instantiate(Assets.Load<GameObject>("Prefabs/PopOver Layer.prefab"), parent);
        }
    }
}