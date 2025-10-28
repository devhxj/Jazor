using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

[TestClass]
public sealed class SemanticWalkerTupleTest
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

    [TestMethod]
    public void Visit_TupleBlockCode()
    {
        var code = @"
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
            ";

        var block = GetBlockOperation(code);
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
    const v571$580 = func.Invoke(2, 5);
  let zzz = v571$580.mmm;
  let yyy = v571$580.y;

  let p = new Point;
  
}", script);
    }

    [TestMethod]
    public void VisitTuple_MultipleNamedElements()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (first: 1, second: 2, third: 3);
                }
            }
            ";

        var operation = GetTupleOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();
                     
        Assert.AreEqual("{first:1,second:2,third:3}", script);
    }

    [TestMethod]
    public void VisitTuple_MixedNamedAndUnnamed()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (name: ""test"", 42, true);
                }
            }
            ";

        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(code);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();
                     
        Assert.AreEqual("{name:\"test\",Item2:42,Item3:true}", script);
    }

    [TestMethod]
    public void VisitTuple_NestedTuples()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (outer: (inner: 1, 2), 3);
                }
            }
            ";

        var operation = GetTupleOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();
                     
        Assert.AreEqual(@"{outer:{inner:1,Item2:2},Item2:3}", script);
    }

    [TestMethod]
    public void VisitTuple_ComplexTypes()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (str: ""hello"", num: 3.14, flag: false, list: new int[] {1, 2, 3});
                }
            }
            ";

        var operation = GetTupleOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();
                     
        Assert.AreEqual(@"{str:""hello"",num:3.14,flag:false,list:[1,2,3]}", script);
    }

    [TestMethod]
    public void VisitTuple_ExpressionElements()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5, y = 10;
                    var tuple = (sum: x + y, diff: x - y, product: x * y);
                }
            }
            ";

        var operation = GetTupleOperationAt(code, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();
                     
        Assert.AreEqual(@"{sum:x+y,diff:x-y,product:x*y}", script);
    }

    [TestMethod]
    public void VisitTuple_MethodCallElements()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (len: ""test"".Length, upper: ""test"".ToUpper(), lower: ""TEST"".ToLower());
                }
            }
            ";

        var operation = GetTupleOperationAt(code);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();
                     
        Assert.AreEqual(@"{len:""test"".Length,upper:""test"".ToUpper(),lower:""TEST"".ToLower()}", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment()
    {
        var code = @"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (aaa:1,2);
                    (int bbb, int ccc) = tuple;
                }
            }
            ";

        var statement = GetOperationAt<IExpressionStatementOperation>(code,1);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();
                     
        Assert.AreEqual("let bbb=tuple.aaa;let ccc=tuple.Item2;", script);
    }
}