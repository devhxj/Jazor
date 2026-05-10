using Basic.Reference.Assemblies;
using ECMAScript;
using ECMAScript.Contract;
using ECMAScript.Vuetify;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

internal static class RazorVueMetadataReferences
{
    private const string AuthoringGlobalUsings = """
        global using static ECMAScript.Vue3;
        global using ECMAScript.VueContract;
        global using ECMAScript.VueContract.Descriptor;
        global using Microsoft.AspNetCore.Components;
        """;

    public static List<MetadataReference> Create(params MetadataReference[] extraReferences)
    {
        var references = Net110.References.All
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
        AddAssemblyReference(references, seenPaths, typeof(ECMAScript.Contract.IUIComponent));
        AddAssemblyReference(references, seenPaths, typeof(SpreadAttribute));
        AddAssemblyReference(references, seenPaths, typeof(ECMAScript.Vue3.IVueComponent));
        AddAssemblyReference(references, seenPaths, typeof(VueRoute));
        AddAssemblyReference(references, seenPaths, typeof(ECMAScript.VueContract.VueLibraryComponentAttribute));
        AddAssemblyReference(references, seenPaths, typeof(VBtn));
        AddAssemblyReference(references, seenPaths, typeof(Jazor.RazorVue.JazorVueCompiler));

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
            CSharpSyntaxTree.ParseText(AuthoringGlobalUsings, path: "RazorVueTestGlobalUsings.g.cs"),
            CSharpSyntaxTree.ParseText(source)
        ];

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
}
