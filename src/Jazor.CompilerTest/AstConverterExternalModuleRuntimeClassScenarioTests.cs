using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterExternalModuleRuntimeClassScenarioTests
{
    [TestMethod]
    public async Task Convert_ExternalModuleRuntimeTypeReference_ImportsDeclaredConstructor()
    {
        const string source = """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("./runtime.mjs")]
                public sealed class ExternalRuntime
                {
                    public ExternalRuntime(string name) { }
                }

                [ECMAScript.ECMAScriptModule("./app.mjs")]
                public static class AppModule
                {
                    public static ExternalRuntime Create(string name)
                        => new ExternalRuntime(name);
                }
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            "external-module-runtime-type.cs");
        var compilation = CSharpCompilation.Create(
            "ExternalModuleRuntimeType",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var appModule = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "AppModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();

        var module = await new AstConverter(appModule, semanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        const string expected = """
            import { ExternalRuntime } from "./runtime.mjs";
            export function Create(name) {
              return new ExternalRuntime(name);
            }
            """;
        Assert.AreEqual(expected + "\n", script?.ReplaceLineEndings("\n"));
        _ = new Parser().ParseModule(script!);
    }

    [TestMethod]
    public async Task Convert_ExternalModuleNestedRuntimeClassConstruction_ImportsFlattenedConstructor()
    {
        const string source = """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("./dialogs.mjs")]
                public static class DialogModule
                {
                    public sealed class Dialog
                    {
                        public Dialog(string title) { }
                    }
                }

                [ECMAScript.ECMAScriptModule("./app.mjs")]
                public static class AppModule
                {
                    public static DialogModule.Dialog Create(string title)
                        => new DialogModule.Dialog(title);
                }
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            "external-module-runtime-class.cs");
        var compilation = CSharpCompilation.Create(
            "ExternalModuleRuntimeClass",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var appModule = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "AppModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();

        var module = await new AstConverter(appModule, semanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        const string expected = """
            import { Dialog } from "./dialogs.mjs";
            export function Create(title) {
              return new Dialog(title);
            }
            """;
        Assert.AreEqual(expected + "\n", script?.ReplaceLineEndings("\n"));
        _ = new Parser().ParseModule(script!);
    }
}
