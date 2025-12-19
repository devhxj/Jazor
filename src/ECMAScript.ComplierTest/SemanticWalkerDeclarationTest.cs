using Acornima.Ast;
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

[TestClass]
public sealed class SemanticWalkerDeclarationTest
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
        var operation = block.Operations.Skip(index).First();
        return operation as T ?? throw new InvalidOperationException("未找到可分析的操作");
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

    [TestMethod]
    public void Visit_ArrayInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var intArray = new int[] {1, 2, 3, 4, 5};
                    var stringArray = new string[] {""apple"", ""banana"", ""cherry""};
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let intArray = [1, 2, 3, 4, 5];
  let stringArray = ['apple', 'banana', 'cherry'];
}", script);
    }

    [TestMethod]
    public void Visit_VariableInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 10;
                    string name = ""Hello"";
                    double pi = 3.14;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 10;
  let name = 'Hello';
  let pi = 3.14;
}", script);
    }

    [TestMethod]
    public void Visit_VariableDeclarator()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    string name;
                    bool flag = true;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 5;
  let name;
  let flag = true;
}", script);
    }

    [TestMethod]
    public void Visit_VariableDeclaration()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5, y = 10;
                    string name = $""test{x}{y}"";
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 5, y = 10;
  let name = `test${x}${y}`;
}", script);
    }

    [TestMethod]
    public void Visit_VariableDeclarationGroup()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1, b = 2, c;
                    string x = ""hello"", y = ""world"";
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let a = 1, b = 2, c;
  let x = 'hello', y = 'world';
}", script);

    }

    [TestMethod]
    public void Visit_DeclarationExpression_OutVar()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string input = ""123"";
                    if (int.TryParse(input, out var result))
                    {
                        // 使用 result
                    }
                    
                    var dict = new System.Collections.Generic.Dictionary<string, int>();
                    if (dict.TryGetValue(""key"", out string value))
                    {
                        // 使用 value
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"{
  let input = ""123"";
  let result;
  if (System.Int32.TryParse(input, result)) {
  }
  
  let dict = new System.Collections.Generic.Dictionary(System.String, System.Int32);
  let value;
  if (dict.TryGetValue(""key"", value)) {
  }

}", script);
    }

    [TestMethod]
    public void Visit_FieldInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new TestClassWithFields();
                }
            }
            
            class TestClassWithFields
            {
                public int Field = 42;
                private string _name = ""default"";
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let obj = new TestClassWithFields;
}", script);

    }

    [TestMethod]
    public void Visit_MixedDeclarationTypes()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    // 简单变量声明
                    int x = 10;
                    
                    // 多变量声明
                    int a = 1, b = 2, c;
                    
                    // 数组初始化
                    var numbers = new int[] {1, 2, 3};
                    
                    // out var 声明
                    string input = ""123"";
                    if (int.TryParse(input, out var result))
                    {
                        // 使用 result
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
@"{
  let x = 10;
  let a = 1, b = 2, c;
  let numbers = [1, 2, 3];
  let input = '123';
  let result;
  if (System.Int32.TryParse(input, result)) {
  }
}", script);

    }

    [TestMethod]
    public void DirectVisit_ArrayInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var arr = new int[] { 1, 2, 3 };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclaration = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclarator = variableDeclaration.Declarations.First().Declarators.First();
        var arrayCreation = (IArrayCreationOperation)variableDeclarator.Initializer!.Value;
        var result = walker.VisitArrayInitializer(arrayCreation.Initializer!, new());
        var script = result?.ToECMAScript();

        Assert.AreEqual("[1,2,3]", script);
    }

    [TestMethod]
    public void DirectVisit_VariableInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 42;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclaration = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclarator = variableDeclaration.Declarations.First().Declarators.First();
        var variableInitializer = variableDeclarator.Initializer!;
        
        var result = walker.VisitVariableInitializer(variableInitializer, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("42", script);
    }

    [TestMethod]
    public void DirectVisit_VariableDeclarator()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 100;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclaration = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclarator = variableDeclaration.Declarations.First().Declarators.First();
        
        var result = walker.VisitVariableDeclarator(variableDeclarator, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("x = 100", script);
    }

    [TestMethod]
    public void DirectVisit_VariableDeclaration()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5, y = 10;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = variableDeclarationGroup.Declarations.First();
        
        var result = walker.VisitVariableDeclaration(variableDeclaration, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("let x = 5, y = 10", script);
    }

    [TestMethod]
    public void DirectVisit_VariableDeclarationGroup()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1, b = 2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        
        var result = walker.VisitVariableDeclarationGroup(variableDeclarationGroup, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("let a = 1, b = 2", script);
    }

    [TestMethod]
    public void DirectVisit_DeclarationExpression_OutVar()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    if (int.TryParse(""123"", out var result))
                    {
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var ifOperation = GetOperationAt<IConditionalOperation>(block, 0);
        var invocationOperation = (IInvocationOperation)ifOperation.Condition;
        var argumentOperation = invocationOperation.Arguments[1];
        var declarationExpression = (IDeclarationExpressionOperation)argumentOperation.Value;
        
        var result = walker.VisitDeclarationExpression(declarationExpression, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("let result", script);
    }

    [TestMethod]
    public void DirectVisit_DeclarationExpression_Tuple()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, 2);
                    (int a, int b) = tuple;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var tupleOperation = GetTupleOperationAt(block, 1);
        var declarationExpression = (IDeclarationExpressionOperation)tupleOperation.Elements[0];
        
        var result = walker.VisitDeclarationExpression(declarationExpression, new());
        var script = result?.ToKnRECMAScript();

        Assert.AreEqual("a", script);
    }

    [TestMethod]
    public void DirectVisit_FieldInitializer()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new ClassWithField();
                }
            }
            
            class ClassWithField
            {
                public int Field = 42;
            }
            ");

        var walker = new SemanticWalker(true);
        var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var initializer = variableDeclarationGroupOp
            .Declarations.FirstOrDefault()?
            .Declarators.FirstOrDefault()?
            .Initializer;

        var script = "";
        //var result = walker.VisitFieldInitializer(fieldInitializer!, new());
        //var script = result?.ToECMAScript();

        Assert.AreEqual("42", script);
    }


}