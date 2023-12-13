using System.IO;

namespace build.Utils;

public class CSharpProject
{
    public string Name { get; }
    public AbsolutePath Directory { get; }

    public CSharpProject(AbsolutePath filePath)
    {
        Name = Path.GetFileNameWithoutExtension(filePath);
        Directory = Path.GetDirectoryName(filePath)!;
    }
}