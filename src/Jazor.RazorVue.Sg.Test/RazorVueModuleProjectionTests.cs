using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueModuleProjectionTests
{
    [TestMethod]
    public async Task Convert_InternalComponentWithNestedRuntimeClass_FlattensArtifactMembersWithStableNames()
    {
        const string source = """
            internal sealed class Counter
            {
                internal string Label => "counter";

                internal string Build(string value)
                {
                    return new RuntimeState("ready:").Format(value);
                }

                private sealed class RuntimeState
                {
                    private readonly string _prefix;

                    public RuntimeState(string prefix)
                    {
                        _prefix = prefix;
                    }

                    public string Format(string value)
                    {
                        return _prefix + value;
                    }
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.ModuleProjection.Tests",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var componentType = compilation.GetTypeByMetadataName("Counter");

        Assert.IsNotNull(componentType);
        var converter = new AstConverter(
            componentType,
            semanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                Host: new RazorVueSemanticWalkerHost(componentType),
                ModulePolicy: RazorVueModulePolicy.Instance));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "function label()", StringComparison.Ordinal);
        StringAssert.Contains(script, "export { label as get_Label };", StringComparison.Ordinal);
        StringAssert.Contains(script, "export function build(value)", StringComparison.Ordinal);
        StringAssert.Contains(script, "return new RuntimeState(\"ready:\").format(value);", StringComparison.Ordinal);
        StringAssert.Contains(script, "class RuntimeState", StringComparison.Ordinal);
        StringAssert.Contains(script, "return this.#_prefix + value;", StringComparison.Ordinal);
        _ = new Acornima.Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_ComponentBaseHelper_ProjectsReachableBaseMethodIntoArtifactModule()
    {
        const string source = """
            public abstract class ComponentBase
            {
                protected string BuildCssClass(string value)
                {
                    return value;
                }
            }

            public sealed class Counter : ComponentBase
            {
                public void Build()
                {
                    var cssClass = BuildCssClass("ready");
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.ModuleProjection.Tests",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var componentType = compilation.GetTypeByMetadataName("Counter");

        Assert.IsNotNull(componentType);
        var baseMethod = componentType.BaseType!.GetMembers("BuildCssClass").OfType<IMethodSymbol>().Single();
        var buildMethod = componentType.GetMembers("Build").OfType<IMethodSymbol>().Single();
        var converter = new AstConverter(
            componentType,
            semanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                MemberFilter: symbol =>
                    SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, baseMethod.OriginalDefinition) ||
                    SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, buildMethod.OriginalDefinition),
                Host: new RazorVueSemanticWalkerHost(componentType),
                ModulePolicy: RazorVueModulePolicy.Instance));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "function buildCssClass(value)", StringComparison.Ordinal);
        StringAssert.Contains(script, "let cssClass = buildCssClass(\"ready\");", StringComparison.Ordinal);
    }
}
