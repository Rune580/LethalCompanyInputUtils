using BepInEx.Logging;
using UnityEngine;

namespace LethalCompanyInputUtils.Utils;

internal static class Logging
{
    private static ManualLogSource? _logSource;

    internal static void SetLogSource(ManualLogSource logSource)
    {
        _logSource = logSource;
    }

    public static void Error(object data) => Error(data.ToString());
    public static void Warn(object data) => Warn(data.ToString());
    public static void Info(object data) => Info(data.ToString());

    public static void Error(string msg)
    {
        if (_logSource is null)
        {
            Debug.LogError($"[{LethalCompanyInputUtilsPlugin.ModName}] [Error] {msg}");
        }
        else
        {
            _logSource.LogError(msg);
        }
    }

    public static void Warn(string msg)
    {
        if (_logSource is null)
        {
            Debug.LogWarning($"[{LethalCompanyInputUtilsPlugin.ModName}] [Warning] {msg}");
        }
        else
        {
            _logSource.LogWarning(msg);
        }
    }

    public static void Info(string msg)
    {
        if (_logSource is null)
        {
            Debug.Log($"[{LethalCompanyInputUtilsPlugin.ModName}] [Info] {msg}");
        }
        else
        {
            _logSource.LogInfo(msg);
        }
    }
}