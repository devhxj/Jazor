using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class CurrentComponentMemberClosureTests
{
    [TestMethod]
    public void Build_FromBuildRenderTreeRoot_IncludesReachableMembersAndExcludesUnreachableMembers()
    {
        var fixture = CompileComponent(
            """
            public sealed class CounterComponent : ComponentBase
            {
                private int _count = Seed();
                private int _unused = UnusedSeed();

                [Parameter]
                public string? Label { get; set; } = "Count";

                private int Count => _count;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "onclick", (Action)(() => Increment()));
                    builder.AddContent(2, Label);
                    builder.AddContent(3, Count);
                    builder.CloseElement();
                }

                private void Increment()
                {
                    _count++;
                }

                private static int Seed() => 0;

                private static int UnusedSeed() => 1;

                private void Unused()
                {
                    _unused++;
                }
            }
            """);

        var buildRenderTree = fixture.GetMethod("BuildRenderTree");
        var closure = CurrentComponentMemberClosure.Build(fixture.ComponentType, fixture.SemanticModel, [buildRenderTree]);

        Assert.IsTrue(closure.Contains(buildRenderTree), "The root BuildRenderTree method must be included.");
        Assert.IsTrue(closure.Contains(fixture.GetField("_count")), "State read/write from reachable render and event paths must be included.");
        Assert.IsTrue(closure.Contains(fixture.GetProperty("Label")), "Parameter reads from render must be included.");
        Assert.IsTrue(closure.Contains(fixture.GetProperty("Count")), "Computed current-component properties must be included.");
        Assert.IsTrue(closure.Contains(fixture.GetMethod("Increment")), "Event handler lambda target must be included.");
        Assert.IsTrue(closure.Contains(fixture.GetMethod("Seed")), "Reachable field initializer dependencies must be included.");

        Assert.IsFalse(closure.Contains(fixture.GetField("_unused")), "Unreachable state must not enter the closure.");
        Assert.IsFalse(closure.Contains(fixture.GetMethod("UnusedSeed")), "Unreachable initializer dependencies must not enter the closure.");
        Assert.IsFalse(closure.Contains(fixture.GetMethod("Unused")), "Unreachable methods must not enter the closure.");
    }

    [TestMethod]
    public void Build_RepeatedRuns_ReturnStableSourceOrder()
    {
        var fixture = CompileComponent(
            """
            public sealed class CounterComponent : ComponentBase
            {
                private int _count = Seed();

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, _count);
                    Increment();
                }

                private void Increment()
                {
                    _count++;
                }

                private static int Seed() => 0;
            }
            """);

        var buildRenderTree = fixture.GetMethod("BuildRenderTree");
        var first = CurrentComponentMemberClosure.Build(fixture.ComponentType, fixture.SemanticModel, [buildRenderTree]);
        var second = CurrentComponentMemberClosure.Build(fixture.ComponentType, fixture.SemanticModel, [buildRenderTree]);

        CollectionAssert.AreEqual(
            first.Members.Select(static member => member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)).ToArray(),
            second.Members.Select(static member => member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)).ToArray());
    }

    [TestMethod]
    public async Task Build_MemberFilter_EmitsOnlyReachableComponentMembers()
    {
        var fixture = CompileComponent(
            """
            public sealed class CounterComponent : ComponentBase
            {
                private int _count = Seed();
                private int _unused = UnusedSeed();

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "onclick", (Action)(() => Increment()));
                    builder.AddContent(2, _count);
                    builder.CloseElement();
                }

                private void Increment()
                {
                    _count++;
                }

                private static int Seed() => 0;

                private static int UnusedSeed() => 1;
            }
            """);

        var closure = CurrentComponentMemberClosure.Build(
            fixture.ComponentType,
            fixture.SemanticModel,
            [fixture.GetMethod("BuildRenderTree")]);
        var converter = new AstConverter(
            fixture.ComponentType,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                MemberFilter: closure.ShouldInclude,
                Host: new RazorVueSemanticWalkerHost(fixture.ComponentType),
                ModulePolicy: RazorVueModulePolicy.Instance));

        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "function buildRenderTree(builder)", StringComparison.Ordinal);
        StringAssert.Contains(script!, "function increment()", StringComparison.Ordinal);
        StringAssert.Contains(script!, "function seed()", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.openElement(\"button\");", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("unusedSeed", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("_unused", StringComparison.Ordinal), script);
    }

    private static ComponentClosureFixture CompileComponent(string componentSource)
    {
        var usings =
            """
            global using System;
            global using ECMAScript;
            global using static ECMAScript.Global;
            global using Microsoft.AspNetCore.Components;
            global using Microsoft.AspNetCore.Components.Rendering;
            """;

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(ComponentBase).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(RenderTreeBuilder).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "CurrentComponentMemberClosure.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings, TestMetadataReferences.PreviewParseOptions),
                CSharpSyntaxTree.ParseText(componentSource, TestMetadataReferences.PreviewParseOptions)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException(string.Join("\n", errors.Select(static error => $"{error.Id}: {error.GetMessage()}")));

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var componentSyntax = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static type => type.Identifier.ValueText == "CounterComponent");
        var componentType = semanticModel.GetDeclaredSymbol(componentSyntax)
            ?? throw new InvalidOperationException("Component type symbol was not available.");

        return new ComponentClosureFixture(componentType, semanticModel);
    }

    private sealed class ComponentClosureFixture
    {
        public ComponentClosureFixture(INamedTypeSymbol componentType, SemanticModel semanticModel)
        {
            ComponentType = componentType;
            SemanticModel = semanticModel;
        }

        public INamedTypeSymbol ComponentType { get; }

        public SemanticModel SemanticModel { get; }

        public IFieldSymbol GetField(string name)
            => ComponentType.GetMembers(name).OfType<IFieldSymbol>().Single();

        public IPropertySymbol GetProperty(string name)
            => ComponentType.GetMembers(name).OfType<IPropertySymbol>().Single();

        public IMethodSymbol GetMethod(string name)
            => ComponentType.GetMembers(name).OfType<IMethodSymbol>().Single(static method => method.Parameters.Length == 0 || method.Name == "BuildRenderTree");
    }
}
