using Acornima.Ast;
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

[TestClass]
public sealed class SemanticWalkerPatternTest
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
        syntaxTrees: [CSharpSyntaxTree.ParseText(code)],
        references: Basic.Reference.Assemblies.Net100.References.All,
        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

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
  private static T GetOperationAt<T>(IBlockOperation block, int index = 0) where T : class, IOperation
  {
    var operation = block.Operations.Skip(index).First() as T;
    return operation ?? throw new InvalidOperationException("未找到可分析的操作");
  }

  /// <summary>
  /// 获取元组操作
  /// </summary>
  /// <param name="code"></param>
  /// <param name="index"></param>
  /// <returns></returns>
  private static ITupleOperation GetTupleOperationAt(IBlockOperation block, int index = 0)
  {
    var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, index);
    var variableDeclaration = variableDeclarationGroup!.Declarations.First();
    var initializer = variableDeclaration.Declarators.First().Initializer;
    var operation = (ITupleOperation)initializer!.Value;
    return operation;
  }

  /// <summary>
  /// 这只是一个整体测试例子
  /// </summary>
  [TestMethod]
  public void Visit_TupleBlockCode()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (aaa:1,2);
                    (int bbb, int ccc) = tuple;
                    int ddd,eee;
                    (ddd, eee) = tuple;
                    int kkk;
                    (kkk,int qqq) = tuple;
                    (int fff, (int ggg,int hhh)) = (2,tuple);
                    (int f44, (int g44,int h44)) = (y8:2,y9:tuple);
                    var func = (int x,int y)=>(mmm:x,y);
                    (int zzz,int yyy)= func(2,5);
                    var p = new Point();
                    (int z99,int y99)= p;
                }
                
                class Point
                {
                    public int X{get;set;}
                    public int Y{get;set;}

                    public void Deconstruct(out int x, out int y)
                    {
                        x = X;
                        y = Y;
                    }
                }                 
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let tuple = { aaa: 1, Item2: 2 };
    let bbb = tuple.aaa;
  let ccc = tuple.Item2;

  let ddd, eee;
    ddd = tuple.aaa;
  eee = tuple.Item2;

  let kkk;
    kkk = tuple.aaa;
  let qqq = tuple.Item2;

    let fff = 2;
  let ggg = tuple.aaa;
  let hhh = tuple.Item2;

    let f44 = 2;
  let g44 = tuple.aaa;
  let h44 = tuple.Item2;

  let func = (x, y) => {
    return { mmm: x, y: y };
  };
    const v$test = func.Invoke(2, 5);
  let zzz = v$test.mmm;
  let yyy = v$test.y;

  let p = new Point;
    p.Deconstruct(z99, y99);

}", script);
  }
}