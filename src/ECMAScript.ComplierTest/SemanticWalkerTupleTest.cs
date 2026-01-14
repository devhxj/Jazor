using Acornima.Ast;
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

    [TestMethod]
    public void VisitTuple_MultipleNamedElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (first: 1, second: 2, third: 3);
                }
            }
            ");

        var operation = GetTupleOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{first:1,second:2,third:3}", script);
        //Assert.AreEqual("Tuple.Create([['first',1],['second',2],['third',3]])", script);
    }

    [TestMethod]
    public void VisitTuple_MixedNamedAndUnnamed()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (name: ""test"", 42, true);
                }
            }
            ");

        var variableDeclarationGroup = GetOperationAt<IVariableDeclarationGroupOperation>(block);
        var variableDeclaration = variableDeclarationGroup!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{name:'test',Item2:42,Item3:true}", script);
        //Assert.AreEqual("Tuple.Create([['name','test'],['Item2',42],['Item3',true]])", script);
    }

    [TestMethod]
    public void VisitTuple_NestedTuples()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (outer: (inner: 1, 2), 3);
                }
            }
            ");

        var operation = GetTupleOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{outer:{inner:1,Item2:2},Item2:3}", script);
        //Assert.AreEqual(@"Tuple.Create([['outer',Tuple.Create([['inner',1],['Item2',2]])],['Item2',3]])", script);
    }

    [TestMethod]
    public void VisitTuple_ComplexTypes()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (str: ""hello"", num: 3.14, flag: false, list: new int[] {1, 2, 3});
                }
            }
            ");

        var operation = GetTupleOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{str:'hello',num:3.14,flag:false,list:[1,2,3]}", script);
        //Assert.AreEqual(@"Tuple.Create([['str','hello'],['num',3.14],['flag',false],['list',[1,2,3]]])", script);
    }

    [TestMethod]
    public void VisitTuple_ExpressionElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5, y = 10;
                    var tuple = (sum: x + y, diff: x - y, product: x * y);
                }
            }
            ");

        var operation = GetTupleOperationAt(block, 1);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{sum:x+y,diff:x-y,product:x*y}", script);
        //Assert.AreEqual(@"Tuple.Create([['sum',x+y],['diff',x-y],['product',x*y]])", script);
    }

    [TestMethod]
    public void VisitTuple_MethodCallElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (len: ""test"".Length, upper: ""test"".ToUpper(), lower: ""TEST"".ToLower());
                }
            }
            ");

        var operation = GetTupleOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{len:'test'.Length,upper:'test'.ToUpper(),lower:'TEST'.ToLower()}", script);
        //Assert.AreEqual(@"Tuple.Create([['len','test'.Length],['upper','test'.ToUpper()],['lower','TEST'.ToLower()]])", script);
    }

    [TestMethod]
    public void VisitTuple_LongTupleMoreThanSevenElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var t = (1,2,3,4,5,6,7,8);
                }
            }
            ");

        var operation = GetTupleOperationAt(block);
        var walker = new SemanticWalker(true);
        var node = walker.VisitTuple(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("{Item1:1,Item2:2,Item3:3,Item4:4,Item5:5,Item6:6,Item7:7,Item8:8}", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_WithTupleRefrence()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (aaa:1,2);
                    (int bbb, int ccc) = tuple;
                }
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 1);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("let bbb=tuple.aaa;let ccc=tuple.Item2;", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_WithExistingVariables()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (aaa:1,2);
                    int bbb, ccc;
                    (bbb, ccc) = tuple;
                }
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 2);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("bbb=tuple.aaa;ccc=tuple.Item2;", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_MixedDeclaration()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (aaa:1,2);
                    int bbb;
                    (bbb, int ccc) = tuple;
                }
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 2);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("bbb=tuple.aaa;let ccc=tuple.Item2;", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_NestedTuple()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (outer: (inner: 1, 2), 3);
                    ((int bbb, int ccc),int aaa) = tuple;
                }
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 1);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("let bbb=tuple.outer.inner;let ccc=tuple.outer.Item2;let aaa=tuple.Item2;", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_MethodCall()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (int aaa, int bbb) = GetTuple();
                }
                
                (int, int) GetTuple() => (1, 2);
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 0);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("const v$test=this.GetTuple();let aaa=v$test.Item1;let bbb=v$test.Item2;", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_WithDiscard()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (aaa:1,2);
                    (_, int ccc) = tuple;
                }
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 1);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("let ccc=tuple.Item2;", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_Equals()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple1 = (1, 2);
                    var tuple2 = (1, 2);
                    var result = tuple1 == tuple2;
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 2);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(tuple1.Item1===tuple2.Item1&&tuple1.Item2===tuple2.Item2)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_NotEquals()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple1 = (1, 2);
                    var tuple2 = (1, 2);
                    var result = tuple1 != tuple2;
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 2);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(tuple1.Item1!==tuple2.Item1||tuple1.Item2!==tuple2.Item2)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_NamedElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple1 = (name: ""test"", value: 42);
                    var tuple2 = (name: ""test"", a: 42);
                    var result = tuple1 == tuple2;
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 2);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(tuple1.name===tuple2.name&&tuple1.value===tuple2.a)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_SimpleAssignmentEquals()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var result = (1,2) == (2,1);
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(1===2&&2===1)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_SimpleAssignmentNotEquals()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var result = (1,2) != (2,1);
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(1!==2||2!==1)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_SimpleAssignmentNestedEquals()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var result = (1,(3,5)) == (2,(4,6));
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(1===2&&3===4&&5===6)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_SimpleAssignmentNestedNotEquals()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var result = (1,(3,5)) != (2,(4,6));
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(1!==2||3!==4||5!==6)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_ThreeElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple1 = (1, 2, 3);
                    var tuple2 = (1, 2, 3);
                    var result = tuple1 == tuple2;
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 2);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(tuple1.Item1===tuple2.Item1&&tuple1.Item2===tuple2.Item2&&tuple1.Item3===tuple2.Item3)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_NestedElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple1 = (1, 2, (4,6));
                    var tuple2 = (1, 2, (4,9));
                    var result = tuple1 == tuple2;
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 2);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(tuple1.Item1===tuple2.Item1&&tuple1.Item2===tuple2.Item2&&tuple1.Item3.Item1===tuple2.Item3.Item1&&tuple1.Item3.Item2===tuple2.Item3.Item2)", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_WithInvocationOperand()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var result = GetTuple() == (1,2);
                }

                (int, int) GetTuple() => (1, 2);
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var queue = new Queue<Acornima.Ast.VariableDeclarator>();
        var node = walker.VisitTupleBinaryOperator(operation, queue);
        var script = node?.ToECMAScript();

        Assert.AreEqual("(v$test.Item1===1&&v$test.Item2===2)", script);
        Assert.HasCount(1, queue);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_InvocationBothSides()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var result = Get1() == Get2();
                }

                (int,int) Get1() => (1,2);
                (int,int) Get2() => (1,2);
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var queue = new Queue<Acornima.Ast.VariableDeclarator>();
        var node = walker.VisitTupleBinaryOperator(operation, queue);
        var script = node?.ToECMAScript();

        Assert.AreEqual("(v$test.Item1===v$test.Item1&&v$test.Item2===v$test.Item2)", script);
        Assert.HasCount(2, queue);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_Conversion()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var result = ((long)1,2) == (1L,2);
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 0);
        var variableDeclaration = statement!.Declarations.First();
        var initializer = variableDeclaration.Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var walker = new SemanticWalker(true);
        var node = walker.VisitTupleBinaryOperator(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("(1===1&&2===2)", script);
    }

    [TestMethod]
    public void VisitDiscardOperation_InDeconstruction()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, 2, 3);
                    var (_, second, _) = tuple;
                }
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 1);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("let second=tuple.Item2;", script);
    }

    [TestMethod]
    public void VisitDiscardOperation_SimpleAssignment()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    _ = SomeMethod();
                }
                
                int SomeMethod() => 42;
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 0);
        var operation = (ISimpleAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitSimpleAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("this.SomeMethod()", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_DeconstructMethod()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, 2);
                    var (x, y) = point;
                }
                
                class Point
                {
                    public int X { get; }
                    public int Y { get; }
                    
                    public Point(int x, int y)
                    {
                        X = x;
                        Y = y;
                    }
                    
                    public void Deconstruct(out int x, out int y)
                    {
                        x = X;
                        y = Y;
                    }
                }
            }
            ");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 1);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("let x,y;point.Deconstruct(x,y);", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_DeconstructMethodNestedTuple()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, (2,3));
                    int x;
                    (x,((int w,(int j,int g)),int z)) = point;
                }
                            
                class Point(int x, (int a, int b) y)
                {
                    private int X { get; } = x;
                    private int A { get; } = y.a;
                    private int B { get; } = y.b;

                    public void Deconstruct(out int x, out ((int,(int,int)),int b) y)
                    {
                        x = X;
                        y = ((A,(1,2)),B);
                    }
                }
            }");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 2);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"let v$test;
point.Deconstruct(x, v$test);
let w = v$test.Item1.Item1;
let j = v$test.Item1.Item2.Item1;
let g = v$test.Item1.Item2.Item2;
let z = v$test.b;
", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_DeconstructMethodDictTuple()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, (2,3));
                    int x;
                    (x,((int w,(int j,int g)),int z)) = point;
                }
                            
                class Point(int x, (int a, int b) y)
                {
                    private int X { get; } = x;
                    private int A { get; } = y.a;
                    private int B { get; } = y.b;

                    public void Deconstruct(out int x, out ((int,(int,int)),int b) y)
                    {
                        x = X;
                        y = ((A,(1,2)),B);
                    }
                }
            }");

        var statement = GetOperationAt<IExpressionStatementOperation>(block, 2);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(@"let v$test;
point.Deconstruct(x, v$test);
let w = v$test.Item1.Item1;
let j = v$test.Item1.Item2.Item1;
let g = v$test.Item1.Item2.Item2;
let z = v$test.b;
", script);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_ConversionOperand()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (int a, int b) = ((int,int))(1,2);
                }
            }");

        var statement = block.Operations
            .OfType<IExpressionStatementOperation>()
            .First(op => op.Operation is IDeconstructionAssignmentOperation);
        var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var node = walker.VisitDeconstructionAssignment(operation, new());
        var script = node?.ToECMAScript();

        Assert.AreEqual("let a=1;let b=2;", script);
    }
}