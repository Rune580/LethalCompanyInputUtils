using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LethalCompanyInputUtils.Utils;

internal static class Assets
{
    private static readonly List<AssetBundle> AssetBundles = [];
    private static readonly Dictionary<string, int> AssetIndices = new();

    public static void AddBundle(string bundleName)
    {
        var assetBundleLoc = Path.Combine(FsUtils.AssetBundlesDir, bundleName);
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleLoc);

        int index = AssetBundles.Count;
        AssetBundles.Add(assetBundle);

        foreach (var assetName in assetBundle.GetAllAssetNames())
        {
            var path = assetName.ToLowerInvariant();
            
            if (path.StartsWith("assets/"))
                path = path["assets/".Length..];

            AssetIndices[path] = index;
        }
    }

    public static T? Load<T>(string assetName) where T : Object
    {
        try
        {
            assetName = assetName.ToLowerInvariant();
            if (assetName.StartsWith("assets/"))
                assetName = assetName["assets/".Length..];

            int index = AssetIndices[assetName];
            return AssetBundles[index].LoadAsset<T>($"assets/{assetName}");
        }
        catch (Exception e)
        {
            Logging.Error($"Couldn't load asset [{assetName}] exception: {e}");
            return null;
        }
    }
}