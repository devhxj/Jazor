using Basic.Reference.Assemblies;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScript.VueDataUi.Test;

internal static class VueDataUiTestCompiler
{
    public static CSharpCompilation CreateCompilation(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            code,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
            path: "/src/VueDataUiModule.cs");
        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueDataUi.Test.Assembly",
            [syntaxTree],
            Net110.References.All.Concat(
            [
                MetadataReference.CreateFromFile(typeof(ECMAScriptModuleAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(VueContract.VueInjectAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Vue.IVueComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Vue).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(VueDataUiConfig).Assembly.Location)
            ]),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsFalse(
            diagnostics.Length > 0,
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        return compilation;
    }

    public static async Task<string?> ConvertModuleAsync(string code, string className)
    {
        var compilation = CreateCompilation(code);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(node => node.Identifier.Text == className);
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        Assert.IsNotNull(classSymbol);
        var converter = new AstConverter(classSymbol, semanticModel);
        var module = await converter.Convert();
        return module?.ToKnRECMAScript();
    }
}
