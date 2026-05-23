using Basic.Reference.Assemblies;
using ECMAScript;
using ECMAScript.Contract;
using ECMAScript.Vben;
using ECMAScript.TDesign;
using ECMAScript.Vuetify;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Threading;

namespace Jazor.RazorVue.Test;

internal static class RazorVueMetadataReferences
{
    private const string AuthoringGlobalUsings = """
        global using static ECMAScript.Vue3;
        global using ECMAScript.VueContract;
        global using ECMAScript.VueContract.Descriptor;
        global using Microsoft.AspNetCore.Components;
        """;
    private static readonly ConcurrentDictionary<string, PortableExecutableReference> MetadataReferenceCache =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Lazy<ImmutableArray<MetadataReference>> BaseReferences =
        new(CreateBaseReferences, LazyThreadSafetyMode.ExecutionAndPublication);

    public static List<MetadataReference> Create(params MetadataReference[] extraReferences)
    {
        var references = BaseReferences.Value.ToList();
        var seenPaths = CreateSeenPaths(references);

        foreach (var extraReference in extraReferences)
        {
            AddReference(references, seenPaths, extraReference);
        }

        return references;
    }

    public static IReadOnlyList<SyntaxTree> CreateSyntaxTrees(string source)
        // 最新 authoring 模型直接使用 ComponentBase + IVueComponent/IVueLibraryComponent，
        // 测试只补全命名空间导入，不再注入 VueComponent/JazorComponent 兼容基类。
        => [
            CSharpSyntaxTree.ParseText(AuthoringGlobalUsings, options: PreviewParseOptions, path: "RazorVueTestGlobalUsings.g.cs"),
            CSharpSyntaxTree.ParseText(source, options: PreviewParseOptions)
        ];

    private static readonly CSharpParseOptions PreviewParseOptions = new(LanguageVersion.Preview);

    private static ImmutableArray<MetadataReference> CreateBaseReferences()
    {
        var references = Net110.References.All
            .Cast<MetadataReference>()
            .ToList();
        var seenPaths = CreateSeenPaths(references);

        AddAssemblyReference(references, seenPaths, typeof(ComponentBase));
        AddAssemblyReference(references, seenPaths, typeof(WebRenderTreeBuilderExtensions));
        AddAssemblyReference(references, seenPaths, typeof(ECMAScript.Contract.IUIComponent));
        AddAssemblyReference(references, seenPaths, typeof(SpreadAttribute));
        AddAssemblyReference(references, seenPaths, typeof(ECMAScript.Vue3.IVueComponent));
        AddAssemblyReference(references, seenPaths, typeof(VueRoute));
        AddAssemblyReference(references, seenPaths, typeof(Pinia));
        AddAssemblyReference(references, seenPaths, typeof(ECMAScript.VueContract.VueLibraryComponentAttribute));
        AddAssemblyReference(references, seenPaths, typeof(TButton));
        AddAssemblyReference(references, seenPaths, typeof(VbenNavItem));
        AddAssemblyReference(references, seenPaths, typeof(VbenAdminLayout));
        AddAssemblyReference(references, seenPaths, typeof(VBtn));
        AddAssemblyReference(references, seenPaths, typeof(Jazor.RazorVue.JazorVueCompiler));

        return references.ToImmutableArray();
    }

    private static void AddAssemblyReference(
        List<MetadataReference> references,
        HashSet<string> seenPaths,
        Type markerType)
    {
        if (TryCreateCachedPortableExecutableReference(markerType.Assembly.Location, out var reference))
        {
            AddReference(references, seenPaths, reference);
        }
    }

    private static void AddReference(
        List<MetadataReference> references,
        HashSet<string> seenPaths,
        MetadataReference reference)
    {
        if (reference is PortableExecutableReference portableReference
            && TryNormalizeMetadataReferencePath(portableReference.FilePath, out var normalizedPath))
        {
            if (!seenPaths.Add(normalizedPath))
            {
                return;
            }

            references.Add(GetOrCreatePortableExecutableReference(normalizedPath));
            return;
        }

        references.Add(reference);
    }

    private static HashSet<string> CreateSeenPaths(IEnumerable<MetadataReference> references)
    {
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var reference in references.OfType<PortableExecutableReference>())
        {
            if (TryNormalizeMetadataReferencePath(reference.FilePath, out var normalizedPath))
            {
                seenPaths.Add(normalizedPath);
            }
        }

        return seenPaths;
    }

    private static bool TryCreateCachedPortableExecutableReference(
        string path,
        out PortableExecutableReference reference)
    {
        reference = null!;
        if (!TryNormalizeMetadataReferencePath(path, out var normalizedPath))
        {
            return false;
        }

        reference = GetOrCreatePortableExecutableReference(normalizedPath);
        return true;
    }

    private static PortableExecutableReference GetOrCreatePortableExecutableReference(string normalizedPath)
        => MetadataReferenceCache.GetOrAdd(
            normalizedPath,
            static path => MetadataReference.CreateFromFile(path));

    private static bool TryNormalizeMetadataReferencePath(string? path, out string normalizedPath)
    {
        normalizedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var candidatePath = Path.GetFullPath(path);
        if (!System.IO.File.Exists(candidatePath))
        {
            return false;
        }

        normalizedPath = candidatePath;
        return true;
    }
}
