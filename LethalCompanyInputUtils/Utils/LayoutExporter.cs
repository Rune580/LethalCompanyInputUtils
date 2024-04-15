using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace LethalCompanyInputUtils.Utils;

internal static class LayoutExporter
{
    private static readonly InputControlLayout[] Layouts = [
        InputSystem.LoadLayout<Keyboard>(),
        InputSystem.LoadLayout<Mouse>(),
        InputSystem.LoadLayout<Gamepad>()
    ];

    /// <summary>
    /// Helper method for exporting device layouts to a json file, checks for the require launch argument `--inputUtilsExportLayoutsToDir=PATH`
    /// <remarks>THIS WILL CLOSE THE GAME AFTER SUCCESSFULLY RUNNING!!!!</remarks>
    /// </summary>
    public static void TryExportLayouts()
    {
        var args = Environment.GetCommandLineArgs();

        string? exportDir = null;
        var exportAll = false;
        foreach (var arg in args)
        {
            if (arg.StartsWith("--inputUtilsExportAllLayouts"))
            {
                exportAll = true;
                continue;
            }
            
            if (!arg.StartsWith("--inputUtilsExportLayoutsToDir="))
                continue;

            var argParts = arg.Split('=');
            if (argParts.Length != 2)
                continue;

            exportDir = argParts[1];
        }

        if (exportDir is null)
            return;

        var layoutsToExport = exportAll
            ? InputSystem.ListLayouts()
                .Select(InputSystem.LoadLayout)
                .ToArray()
            : Layouts;

        try
        {
            ExportLayouts(layoutsToExport, Path.Combine(exportDir, "device_layouts.json"));
            Application.Quit();
        }
        catch (Exception e)
        {
            Logging.Error(e);
        }
    }
    
    public static void ExportLayouts(InputControlLayout[] layouts, string filePath)
    {
        var builder = new StringBuilder();

        builder.AppendLine("{");
        builder.AppendLine("\t\"layouts\": [");

        foreach (var layout in layouts)
        {
            var layoutJson = layout.ToJson();

            var lines = layoutJson.Split("\n");
            foreach (var line in lines)
                builder.AppendLine($"\t\t{line}");

            builder.Remove(builder.Length - 2, 2);
            builder.AppendLine(",");
        }
        
        builder.Remove(builder.Length - 3, 1);
        builder.AppendLine("\t]");
        builder.AppendLine("}");
        
        File.WriteAllText(filePath, builder.ToString());
    }
}