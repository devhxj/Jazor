using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

/// <summary>
/// 测试辅助类：用于获取编译器输出
/// </summary>
public static class TestHelper
{
    private static (INamedTypeSymbol?, SemanticModel) CompileAndGetSymbol(string code)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(code)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var syntaxTree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        return (classSymbol, semanticModel);
    }

    public static async Task<string?> GetOutputAsync(string code)
    {
        try
        {
            var (classSymbol, semanticModel) = CompileAndGetSymbol(code);
            if (classSymbol is null) return null;

            var converter = new AstConverter(classSymbol, semanticModel);
            var result = await converter.Convert();
            return result?.ToKnRECMAScript() ?? "";
        }
        catch
        {
            return null;
        }
    }
}
