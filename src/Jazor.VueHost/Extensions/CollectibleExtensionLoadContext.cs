using System.Reflection;
using System.Runtime.Loader;

namespace Jazor.VueHost.Extensions;

internal sealed class CollectibleExtensionLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public CollectibleExtensionLoadContext(string mainAssemblyPath)
        : base($"jazor-extension:{Path.GetFileNameWithoutExtension(mainAssemblyPath)}:{Guid.NewGuid():N}", isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    public Assembly LoadMainAssembly(string assemblyPath)
    {
        using var stream = OpenReadStream(assemblyPath);
        return LoadFromStream(stream);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var dependencyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (string.IsNullOrWhiteSpace(dependencyPath) || !File.Exists(dependencyPath))
        {
            return null;
        }

        using var stream = OpenReadStream(dependencyPath);
        return LoadFromStream(stream);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (string.IsNullOrWhiteSpace(libraryPath) || !File.Exists(libraryPath))
        {
            return nint.Zero;
        }

        return LoadUnmanagedDllFromPath(libraryPath);
    }

    private static FileStream OpenReadStream(string path)
        => new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
}
