using System.IO;

namespace build.Utils;

public class CSharpProject
{
    public AbsolutePath FilePath { get; }
    public string Name { get; }
    public AbsolutePath Directory { get; }

    public CSharpProject(AbsolutePath filePath)
    {
        FilePath = filePath;
        Name = Path.GetFileNameWithoutExtension(filePath);
        Directory = Path.GetDirectoryName(filePath)!;
    }
}