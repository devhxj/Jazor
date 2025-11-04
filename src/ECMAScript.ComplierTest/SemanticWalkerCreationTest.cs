using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

[TestClass]
public sealed class SemanticWalkerCreationTest
{
    /// <summary>
    /// 编译代码并获取roslyn代码块
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static IBlockOperation GetBlockOperation(string code)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(code)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var syntaxTree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();

        // 查找第一个方法体
        var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration?.Body is not null)
        {
            var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
            if (operation is not null)
                return operation;
        }

        throw new InvalidOperationException("未找到可分析的操作");
    }

    /// <summary>
    /// 获取指定索引的操作
    /// </summary>
    private static T GetOperationAt<T>(string code, int index = 0) where T : class, IOperation
    {
        var block = GetBlockOperation(code);
        var operation = block.Operations.Skip(index).First() as T;

        return operation ?? throw new InvalidOperationException("未找到可分析的操作");
    }

    /// <summary>
    /// 获取元组操作
    /// </summary>
    /// <param name="code"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private static ITupleOperation GetTupleOperationAt(string code, int index = 0)
    {
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(code,index);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleOperation)initializer!.Value;   
        return operation;
    }
}