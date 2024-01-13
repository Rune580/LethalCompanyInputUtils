using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.Core.IO;
using Path = System.IO.Path;

namespace build.Utils;

public class AbsolutePath
{
    private readonly string _path;
    
    public string Name => Path.GetFileName(_path);
    
    public AbsolutePath(string path)
    {
        if (path.StartsWith("./"))
            path = path[2..];
        
        _path = path;
    }
    
    public void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
    }

    public void CleanAndCreateDirectory()
    {
        if (Directory.Exists(_path))
            Directory.Delete(_path, true);

        Directory.CreateDirectory(_path);
    }

    public void CreateDirectory()
    {
        Directory.CreateDirectory(_path);
    }

    public void DeleteFile()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }

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
        var separator = !left._path.EndsWith('/') && !right._path.StartsWith('/')
            ? $"{Path.DirectorySeparatorChar}"
            : "";
        
        return new AbsolutePath($"{left._path}{separator}{right._path}");
    }

    public static AbsolutePath operator /(AbsolutePath left, string right)
    {
        return left / (AbsolutePath)right;
    }
}

internal static class AbsolutePathUtils
{
    public static void CopyFilesTo(this IEnumerable<AbsolutePath> files, AbsolutePath destDir)
    {
        destDir.EnsureDirectoryExists();
        
        foreach (var file in files)
        {
            var destFile = destDir / file.Name;
            File.Copy(file, destFile, true);
        }
    }

    public static void DeleteFiles(this IEnumerable<AbsolutePath> files)
    {
        foreach (var file in files)
            File.Delete(file);
    }
}