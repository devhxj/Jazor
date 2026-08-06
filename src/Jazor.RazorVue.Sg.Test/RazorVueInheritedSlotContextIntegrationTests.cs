using Acornima;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueInheritedSlotContextIntegrationTests
{
    [TestMethod]
    public async Task Convert_TypedComponentWithInheritedDefaultSlot_UsesCompilerHierarchyContext()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo;

            public abstract record BaseSlots : VueSlots
            {
                [Description("@#default")]
                public VueSlotCallback ChildContent { get; init; } = default!;
            }

            public sealed record EditorSlots : BaseSlots;

            [ECMAScriptModule("components/inherited-slot.mjs")]
            public static class EditorModule
            {
                public static IVueSlotComponent<EditorSlots> Child = Vue3.DefineComponent(new VueSlotComponentOptions<EditorSlots>
                {
                    Name = "ChildView"
                });

                public static IVNode Render(IVNode child)
                    => H(Child, child);
            }
            """;
        var (_, semanticModel) = CompileAndGetSymbol(code, "EditorModule");
        var moduleSymbol = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "EditorModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        var converter = new AstConverter(
            moduleSymbol,
            semanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                Host: ChildrenToSlotSemanticWalkerHost.Instance));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
            """
            import { defineComponent, h } from "vue";
            let Child = defineComponent({ name: "ChildView" });
            export { Child as child };
            export function render(child) {
              return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(Child, child);
            }

            """.ReplaceLineEndings("\n"),
            script?.ReplaceLineEndings("\n"));
        Assert.IsNotNull(script);
        _ = new Parser().ParseModule(script!);
    }

    private static (INamedTypeSymbol Module, SemanticModel SemanticModel) CompileAndGetSymbol(string source, string moduleName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVueInheritedSlotContextIntegration",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, diagnostics, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var module = syntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(declaration => declaration.Identifier.ValueText == moduleName)
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return (module, semanticModel);
    }
}
