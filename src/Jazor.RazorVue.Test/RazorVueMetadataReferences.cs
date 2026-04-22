using Basic.Reference.Assemblies;
using ECMAScript.UI.Vue.Vuetify;
using Jazor.Razor;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

internal static class RazorVueMetadataReferences
{
    private const string AuthoringShimSource = """
        namespace Jazor.Razor
        {
            public abstract class JazorComponent
                : global::Microsoft.AspNetCore.Components.ComponentBase,
                  global::Jazor.Razor.IJazorComponent
            {
            }
        }

        namespace Jazor.RazorVue
        {
            public abstract class VueComponent
                : global::Jazor.Razor.JazorComponent,
                  global::Jazor.RazorVue.IVueComponent
            {
            }

            public abstract class VueLibraryComponent
                : global::Jazor.RazorVue.VueComponent,
                  global::Jazor.RazorVue.IVueLibraryComponent
            {
            }
        }

        namespace Jazor.Vue
        {
            public abstract class VueComponent : global::Jazor.RazorVue.VueComponent
            {
            }
        }
        """;

    public static List<MetadataReference> Create(params MetadataReference[] extraReferences)
    {
        var references = Net100.References.All
            .Cast<MetadataReference>()
            .ToList();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var reference in references.OfType<PortableExecutableReference>())
        {
            if (!string.IsNullOrWhiteSpace(reference.FilePath))
            {
                seenPaths.Add(reference.FilePath);
            }
        }

        AddAssemblyReference(references, seenPaths, typeof(ComponentBase));
        AddAssemblyReference(references, seenPaths, typeof(IJazorComponent));
        AddAssemblyReference(references, seenPaths, typeof(IVueComponent));
        AddAssemblyReference(references, seenPaths, typeof(VBtn));

        foreach (var extraReference in extraReferences)
        {
            AddReference(references, seenPaths, extraReference);
        }

        return references;
    }

    public static IReadOnlyList<SyntaxTree> CreateSyntaxTrees(string source)
    {
        if (!RequiresAuthoringShim(source))
        {
            return [CSharpSyntaxTree.ParseText(source)];
        }

        return
        [
            CSharpSyntaxTree.ParseText(AuthoringShimSource),
            CSharpSyntaxTree.ParseText(source)
        ];
    }

    private static void AddAssemblyReference(
        List<MetadataReference> references,
        HashSet<string> seenPaths,
        Type markerType)
    {
        AddReference(
            references,
            seenPaths,
            MetadataReference.CreateFromFile(markerType.Assembly.Location));
    }

    private static void AddReference(
        List<MetadataReference> references,
        HashSet<string> seenPaths,
        MetadataReference reference)
    {
        if (reference is PortableExecutableReference portableReference
            && !string.IsNullOrWhiteSpace(portableReference.FilePath)
            && !seenPaths.Add(portableReference.FilePath))
        {
            return;
        }

        references.Add(reference);
    }

    private static bool RequiresAuthoringShim(string source)
        => source.Contains("VueComponent", StringComparison.Ordinal) ||
           source.Contains("VueLibraryComponent", StringComparison.Ordinal) ||
           source.Contains("JazorComponent", StringComparison.Ordinal) ||
           source.Contains("ComponentBase", StringComparison.Ordinal) ||
           source.Contains("IVueComponent", StringComparison.Ordinal) ||
           source.Contains("IVueLibraryComponent", StringComparison.Ordinal) ||
           source.Contains("Jazor.RazorVue", StringComparison.Ordinal);
}
