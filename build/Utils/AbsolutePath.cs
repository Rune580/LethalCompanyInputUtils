using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.Core.IO;
using Path = System.IO.Path;

namespace build.Utils;

public class AbsolutePath(string path)
{
    private readonly string _path = path;

    public string Name => Path.GetFileName(_path);

    public List<AbsolutePath> GlobFiles(params string[] patterns)
    {
        return GlobFiles(SearchOption.AllDirectories, patterns);
    }

    public List<AbsolutePath> GlobFiles(SearchOption option, params string[] patterns)
    {
        return patterns.SelectMany(pattern => Directory.GetFiles(_path, pattern, option))
            .Select(x => (AbsolutePath) x)
            .ToList();
    }
    
    public static implicit operator AbsolutePath(string path) => new(path);
    public static implicit operator AbsolutePath(FilePath path) => new(path.FullPath);
    public static implicit operator AbsolutePath(DirectoryPath path) => new(path.FullPath);

    public static implicit operator string(AbsolutePath path) => path._path;
    public static implicit operator FilePath(AbsolutePath path) => path._path;

    public static AbsolutePath operator /(AbsolutePath left, AbsolutePath right)
    {
        return new AbsolutePath($"{left._path}{Path.DirectorySeparatorChar}{right._path}");
    }

    public static AbsolutePath operator /(AbsolutePath left, string right)
    {
        return left / (AbsolutePath)right;
    }
}