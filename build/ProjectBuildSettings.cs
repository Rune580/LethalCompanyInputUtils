using System.IO;
using System.Text.Json;
using build.Utils;

namespace build;

public class ProjectBuildSettings
{
    public required string[] References { get; init; }
    public required string ProjectFile { get; init; }
    public required string UnityProjectDir { get; init; }
    public required string ManifestAuthor { get; init; }

    public static ProjectBuildSettings? LoadFromFile(AbsolutePath filePath)
    {
        return JsonSerializer.Deserialize<ProjectBuildSettings>(File.ReadAllText(filePath));
    }
}