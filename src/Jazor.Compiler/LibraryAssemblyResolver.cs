using ICSharpCode.Decompiler.Metadata;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Jazor.Compiler;

/// <summary>Resolve decompiler references from the consumer's fixed Roslyn graph, without probing SDKs.</summary>
internal sealed class LibraryAssemblyResolver : IAssemblyResolver
{
    // The compilation owns these metadata images. A non-owning MetadataFile view lets ILSpy
    // reuse in-memory references as well as file-backed ones, with no second package resolver.
    private sealed class ReferenceMetadataFile(string name, ModuleMetadata module)
        : MetadataFile(MetadataFileKind.Metadata, name, module.GetMetadataReader(), 0, false);

    private readonly Dictionary<string, MetadataFile> _assemblies = new(StringComparer.Ordinal);

    internal LibraryAssemblyResolver(Compilation compilation)
    {
        foreach (var reference in compilation.References.OfType<PortableExecutableReference>())
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly ||
                reference.GetMetadata() is not AssemblyMetadata metadata) continue;
            _assemblies[assembly.Name] = new ReferenceMetadataFile(assembly.Name, metadata.GetModules()[0]);
        }
    }

    public MetadataFile? Resolve(IAssemblyReference reference)
        => _assemblies.TryGetValue(reference.Name, out var assembly) ? assembly : null;

    public MetadataFile? ResolveModule(MetadataFile mainModule, string moduleName) => null;
    public Task<MetadataFile?> ResolveAsync(IAssemblyReference reference) => Task.FromResult(Resolve(reference));
    public Task<MetadataFile?> ResolveModuleAsync(MetadataFile mainModule, string moduleName) => Task.FromResult<MetadataFile?>(null);
    public IDisposable? BeginSnapshot() => null;
}
