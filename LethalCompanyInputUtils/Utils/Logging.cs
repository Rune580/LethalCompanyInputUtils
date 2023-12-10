using BepInEx.Logging;

namespace LethalCompanyInputUtils.Utils;

internal static class Logging
{
    private static ManualLogSource? _logSource;

    internal static ManualLogSource Logger { get; } = _logSource!;

    internal static void SetLogSource(ManualLogSource logSource)
    {
        _logSource = logSource;
    }
}