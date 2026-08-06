using System.Reflection;
using System.Runtime.Loader;

namespace Jazor.Emit;

/// <summary>Isolates emitted assemblies while resolving dependencies beside the root assembly.</summary>
internal sealed class EmitLoadContext(string rootAssemblyPath) : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(rootAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }
}
