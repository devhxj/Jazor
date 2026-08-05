using Acornima;
using ECMAScript;
using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstConverterDeclaredNameCollisionTests
{
    [TestMethod]
    public async Task Convert_ModuleBindingCollision_ReservesStableNamesForNamedImports()
    {
        var initialFixture = CompileModule(
            """
            using ECMAScript;

            namespace Demo;

            [ECMAScriptModule("consumer")]
            public static class TestModule
            {
                public static int release = 2;

                public static int Read()
                {
                    var release = 3;
                    return release + TestModule.release;
                }
            }
            """);
        var field = initialFixture.Module.GetMembers("release").OfType<IFieldSymbol>().Single();
        var generatedAlias = "m$" + Format.HashName(field.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_');

        var fixture = CompileModule(
            $$"""
            using ECMAScript;
            using Runtime = Demo.RuntimeModule;

            namespace Demo;

            [ECMAScriptModule("runtime")]
            public static class RuntimeModule
            {
                public static int release() => 1;
            }

            [ECMAScriptModule("consumer")]
            public static class TestModule
            {
                [ECMAScriptName("{{generatedAlias}}")]
                public static int ReserveGeneratedAlias() => 4;

                public static int release = 2;

                public static int Read()
                {
                    var release = 3;
                    return release + TestModule.release + Runtime.release();
                }
            }
            """);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module);
        var script = module.ToKnRECMAScript();
        var suffixedAlias = generatedAlias + "$1";
        StringAssert.Contains(script, $"function {generatedAlias}()", StringComparison.Ordinal);
        StringAssert.Contains(script, $"let {suffixedAlias} = 2;", StringComparison.Ordinal);
        StringAssert.Contains(script, $"return release + {suffixedAlias} +", StringComparison.Ordinal);
        StringAssert.Contains(script, "import { release as", StringComparison.Ordinal);
        _ = new Parser().ParseModule(script);
    }

    private static ModuleFixture CompileModule(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterDeclaredNameCollision.cs");
        var compilation = CSharpCompilation.Create(
            "AstConverterDeclaredNameCollision_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScriptAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var module = syntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new ModuleFixture(module, semanticModel);
    }

    private sealed record ModuleFixture(INamedTypeSymbol Module, SemanticModel SemanticModel);
}
