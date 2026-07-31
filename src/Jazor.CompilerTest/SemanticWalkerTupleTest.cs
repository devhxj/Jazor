using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Text.RegularExpressions;

namespace Jazor.ComplierTest;

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
        var usings = @"
          global using System;
          global using System.Collections.Generic;
          global using System.Linq;
          global using System.Numerics;
          global using ECMAScript;
          global using static ECMAScript.Global;";

        var references = TestMetadataReferences.Net11
          .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
          assemblyName: "TestAssembly",
          syntaxTrees: [
            CSharpSyntaxTree.ParseText(usings),
            CSharpSyntaxTree.ParseText(code)
          ],
          references: references,
          options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // 输出编译诊断信息
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            var errorMessages = string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"));
            throw new InvalidOperationException(errorMessages);
        }

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();

        // 查找第一个方法体
        var methodDeclaration = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(static method => method.Identifier.ValueText == "TestMethod" && method.Body is not null)
            ?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
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

    private static void AssertTupleScriptEqual(string expected, string? actual)
        => Assert.AreEqual(
            ExpectedJsNaming.Normalize(expected).ReplaceLineEndings(),
            actual?.ReplaceLineEndings());

    private static string NormalizeExpectedJsNaming(string expected)
    {
        expected = Regex.Replace(expected, @"\bItem([0-9]+)\b", "item$1");
        expected = Regex.Replace(expected, @"([\{\[,]\s*)([A-Z][A-Za-z0-9_]*)(\s*:)", static m => m.Groups[1].Value + Camel(m.Groups[2].Value) + m.Groups[3].Value);
        expected = Regex.Replace(expected, @"(^\s*)([A-Z][A-Za-z0-9_]*)(\s*:)", static m => m.Groups[1].Value + Camel(m.Groups[2].Value) + m.Groups[3].Value, RegexOptions.Multiline);
        expected = Regex.Replace(expected, @"(\?*\.)([A-Z][A-Za-z0-9_]*)", static m => m.Groups[1].Value + Camel(m.Groups[2].Value));
        expected = Regex.Replace(expected, @"""([A-Z][A-Za-z0-9_]*)""(\s+in\b)", static m => "\"" + Camel(m.Groups[1].Value) + "\"" + m.Groups[2].Value);
        expected = Regex.Replace(expected, @"\[""([A-Z][A-Za-z0-9_]*)""\]", static m => "[\"" + Camel(m.Groups[1].Value) + "\"]");
        return expected;
    }

    private static string Camel(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return name;

        if (name.Length == 1)
            return char.ToLowerInvariant(name[0]).ToString();

        var chars = name.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);
        for (var index = 1; index < chars.Length; index++)
        {
            if (!char.IsUpper(chars[index]))
                break;

            var hasNext = index + 1 < chars.Length;
            if (hasNext && !char.IsUpper(chars[index + 1]))
                break;

            chars[index] = char.ToLowerInvariant(chars[index]);
        }

        return new string(chars);
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

        AssertTupleScriptEqual(
@"{
  let bbb, ccc, qqq, fff, ggg, hhh, f44, g44, h44, v$0, zzz, yyy, z99, y99, v$1;
  let tuple = { aaa: 1, Item2: 2 };
  bbb = tuple.aaa, ccc = tuple.Item2;
  let ddd, eee;
  ddd = tuple.aaa, eee = tuple.Item2;
  let kkk;
  kkk = tuple.aaa, qqq = tuple.Item2;
  fff = 2, ggg = tuple.aaa, hhh = tuple.Item2;
  f44 = 2, g44 = tuple.aaa, h44 = tuple.Item2;
  let func = (x, y) => {
    return { mmm: x, y: y };
  };
  v$0 = func(2, 5), zzz = v$0.mmm, yyy = v$0.y;
  let p = new Point;
  v$1 = p.Deconstruct(z99, y99), z99 = v$1[0], y99 = v$1[1];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

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

        AssertTupleScriptEqual("{first:1,second:2,third:3}", script);
        //AssertTupleScriptEqual("Tuple.Create([['first',1],['second',2],['third',3]])", script);
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

        AssertTupleScriptEqual(@"{name:""test"",Item2:42,Item3:true}", script);
        //AssertTupleScriptEqual("Tuple.Create([['name','test'],['Item2',42],['Item3',true]])", script);
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

        AssertTupleScriptEqual("{outer:{inner:1,Item2:2},Item2:3}", script);
        //AssertTupleScriptEqual(@"Tuple.Create([['outer',Tuple.Create([['inner',1],['Item2',2]])],['Item2',3]])", script);
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

        AssertTupleScriptEqual(@"{str:""hello"",num:3.14,flag:false,list:[1,2,3]}", script);
        //AssertTupleScriptEqual(@"Tuple.Create([['str','hello'],['num',3.14],['flag',false],['list',[1,2,3]]])", script);
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

        AssertTupleScriptEqual("{sum:x+y,diff:x-y,product:x*y}", script);
        //AssertTupleScriptEqual(@"Tuple.Create([['sum',x+y],['diff',x-y],['product',x*y]])", script);
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

        AssertTupleScriptEqual(@"{len:""test"".length,upper:""test"".toUpperCase(),lower:""TEST"".toLowerCase()}", script);
        //AssertTupleScriptEqual(@"Tuple.Create([['len','test'.Length],['upper','test'.ToUpper()],['lower','TEST'.ToLower()]])", script);
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

        AssertTupleScriptEqual("{item1:1,Item2:2,Item3:3,Item4:4,Item5:5,Item6:6,Item7:7,Item8:8}", script);
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
        //var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let bbb, ccc;
  let tuple = { aaa: 1, Item2: 2 };
  bbb = tuple.aaa, ccc = tuple.Item2;
}", script);

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

        //var statement = GetOperationAt<IExpressionStatementOperation>(block, 2);
        //var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let tuple = { aaa: 1, Item2: 2 };
  let bbb, ccc;
  bbb = tuple.aaa, ccc = tuple.Item2;
}", script);

    }

    [TestMethod]
    public void VisitDeconstructionAssignment_WithFieldTargets()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                private int _bbb;
                private int _ccc;

                void TestMethod()
                {
                    var tuple = (aaa:1,2);
                    (this._bbb, this._ccc) = tuple;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let tuple = { aaa: 1, Item2: 2 };
  this._bbb = tuple.aaa, this._ccc = tuple.Item2;
}", script);
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

        //var statement = GetOperationAt<IExpressionStatementOperation>(block, 2);
        //var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let ccc;
  let tuple = { aaa: 1, Item2: 2 };
  let bbb;
  bbb = tuple.aaa, ccc = tuple.Item2;
}", script);

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

        //var statement = GetOperationAt<IExpressionStatementOperation>(block, 1);
        //var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let bbb, ccc, aaa;
  let tuple = { outer: { inner: 1, Item2: 2 }, Item2: 3 };
  bbb = tuple.outer.inner, ccc = tuple.outer.Item2, aaa = tuple.Item2;
}", script);

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

        //var statement = GetOperationAt<IExpressionStatementOperation>(block, 0);
        //var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, aaa, bbb;
  v$0 = this.GetTuple(), aaa = v$0.Item1, bbb = v$0.Item2;
}", script);

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

        //var statement = GetOperationAt<IExpressionStatementOperation>(block, 1);
        //var operation = (IDeconstructionAssignmentOperation)statement.Operation;
        var walker = new SemanticWalker(true);
        //var node = walker.VisitDeconstructionAssignment(operation, new());
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let ccc;
  let tuple = { aaa: 1, Item2: 2 };
  ccc = tuple.Item2;
}", script);

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

        AssertTupleScriptEqual("(tuple1.item1===tuple2.item1&&tuple1.item2===tuple2.item2)", script);
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

        AssertTupleScriptEqual("(tuple1.item1!==tuple2.item1||tuple1.item2!==tuple2.item2)", script);
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

        AssertTupleScriptEqual("(tuple1.name===tuple2.name&&tuple1.value===tuple2.a)", script);
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

        AssertTupleScriptEqual("(1===2&&2===1)", script);
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

        AssertTupleScriptEqual("(1!==2||2!==1)", script);
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

        AssertTupleScriptEqual(@"(1===2&&(3===4&&5===6))", script);
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

        AssertTupleScriptEqual(@"(1!==2||(3!==4||5!==6))", script);
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

        AssertTupleScriptEqual("(tuple1.item1===tuple2.item1&&tuple1.item2===tuple2.item2&&tuple1.item3===tuple2.item3)", script);
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
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"(tuple1.item1 === tuple2.item1 && tuple1.item2 === tuple2.item2 && (tuple1.item3.item1 === tuple2.item3.item1 && tuple1.item3.item2 === tuple2.item3.item2))", script);
    }

    [TestMethod]
    public void VisitTupleBinaryOperator_NestedFirstElement_StartsRecursiveComparisonChain()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple1 = ((1, 2), 3);
                    var tuple2 = ((1, 2), 4);
                    var result = tuple1 == tuple2;
                }
            }
            ");

        var statement = GetOperationAt<IVariableDeclarationGroupOperation>(block, 2);
        var initializer = statement!.Declarations.First().Declarators.First().Initializer;
        var operation = (ITupleBinaryOperation)initializer!.Value;
        var node = new SemanticWalker(true).VisitTupleBinaryOperator(operation, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
            @"(tuple1.item1.item1 === tuple2.item1.item1 && tuple1.item1.item2 === tuple2.item1.item2 && tuple1.item2 === tuple2.item2)",
            script);
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
        var arg = new SenseArgument(PatternInput: new Identifier("v$0"));
        var node = walker.VisitTupleBinaryOperator(operation, arg);
        var script = node?.ToECMAScript();

        AssertTupleScriptEqual("(v$0=this.getTuple(),v$0.Item1===1&&v$0.Item2===2)", script);
        Assert.IsTrue(arg.HasVarDeclarator);
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

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, v$1;
  let result = (v$0 = this.Get1(), v$1 = this.Get2(), v$0.Item1 === v$1.Item1 && v$0.Item2 === v$1.Item2);
}", script);

    }

    [TestMethod]
    public void VisitTupleBinaryOperator_WithPriorStatement_InitializesCachedOperandAtUseSite()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int seed)
                {
                    Console.WriteLine(seed);
                    var result = GetTuple(seed) == (1,2);
                }

                (int, int) GetTuple(int value) => (value, 2);
            }
            ");

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let v$0;");
        StringAssert.Contains(script, "console.log(seed);");
        StringAssert.Contains(script, "v$0 = this.getTuple(seed)");
        Assert.IsFalse(script.Contains("let v$0 = this.getTuple(seed);", StringComparison.Ordinal), script);

        var consoleIndex = script.IndexOf("console.log(seed);", StringComparison.Ordinal);
        var assignmentIndex = script.IndexOf("v$0 = this.getTuple(seed)", StringComparison.Ordinal);
        Assert.IsTrue(
            consoleIndex >= 0 && assignmentIndex > consoleIndex,
            $"Expected tuple cache assignment to remain at the comparison site, after prior statements.{Environment.NewLine}{script}");
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

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let result = (BigInt(1) === 1n && 2 === 2);
}", script);

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

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let second;
  let tuple = {
    item1: 1,
    Item2: 2,
    Item3: 3
  };
  second = tuple.Item2;
}", script);

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

        AssertTupleScriptEqual("this.someMethod()", script);
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

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let x, y, v$0;
  let point = new Point(1, 2);
  v$0 = point.Deconstruct(x, y), x = v$0[0], y = v$0[1];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    [TestMethod]
    public void VisitDeconstructionAssignment_Record_UsesStructuralMembers()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, 2);
                    var (x, y) = point;
                }

                record Point(int X, int Y);
            }
            ");

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let x, y;
  let point = { x: 1, y: 2 };
  x = point.x, y = point.y;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_DeconstructMethodFieldTargets()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                private int _x;
                private int _y;

                void TestMethod()
                {
                    var point = new Point(1, 2);
                    (this._x, this._y) = point;
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

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, v$1, v$2;
  let point = new Point(1, 2);
  v$2 = point.Deconstruct(v$0, v$1), this._x = v$2[0], this._y = v$2[1];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, v$1, w, j, g, z;
  let point = new Point(1, { a: 2, b: 3 });
  let x;
  v$1 = point.Deconstruct(x, v$0), v$0 = v$1[1], x = v$1[0], w = v$0.Item1.Item1, j = v$0.Item1.Item2.Item1, g = v$0.Item1.Item2.Item2, z = v$0.b;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    [TestMethod]
    public void VisitDeconstructionAssignment_Record_WithNestedTuple_UsesStructuralMembers()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = new Point(1, (2,3));
                    int x;
                    (x, (int a, int b)) = point;
                }

                record Point(int X, (int A, int B) Pair);
            }");

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let a, b;
  let point = { x: 1, pair: { a: 2, b: 3 } };
  let x;
  x = point.x, a = point.pair.a, b = point.pair.b;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, v$1, w, j, g, z;
  let point = new Point(1, { a: 2, b: 3 });
  let x;
  v$1 = point.Deconstruct(x, v$0), v$0 = v$1[1], x = v$1[0], w = v$0.Item1.Item1, j = v$0.Item1.Item2.Item1, g = v$0.Item1.Item2.Item2, z = v$0.b;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

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

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let a, b;
  a = 1, b = 2;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    [TestMethod]
    public void VisitDeconstructionAssignment_ConversionInvocationOperand()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (int a, int b) = ((int, int))GetTuple();
                }

                (int, int) GetTuple() => (1, 2);
            }");

        var walker = new SemanticWalker(true);
        var ctx = SenseArgument.Default;
        var node = walker.Visit(block, ctx);
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, a, b;
  v$0 = this.GetTuple(), a = v$0.Item1, b = v$0.Item2;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    [TestMethod]
    public void VisitDeconstructionAssignment_SelfReferentialTupleValues_AreStagedBeforeWrites()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int left = 1;
                    int right = 2;
                    (left, right) = (right, left);
                    (left, right) = ((int, int))(right, left);
                }
            }");

        var walker = new SemanticWalker(true);
        var script = walker.Visit(block, new())?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, v$1, v$2, v$3;
  let left = 1;
  let right = 2;
  v$0 = right, v$1 = left, left = v$0, right = v$1;
  v$2 = right, v$3 = left, left = v$2, right = v$3;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_NestedSelfReferentialTupleValue_IsStagedBeforeNestedWrites()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int left = 1;
                    int right = 2;
                    int id = 3;
                    ((left, right), id) = ((right, left), id);
                }
            }");

        var walker = new SemanticWalker(true);
        var script = walker.Visit(block, new())?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, v$1;
  let left = 1;
  let right = 2;
  let id = 3;
  v$0 = { item1: right, Item2: left }, v$1 = id, left = v$0.Item1, right = v$0.Item2, id = v$1;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void VisitDeconstructionAssignment()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
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
            }");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let z99, y99, v$0;
  let p = new Point;
  v$0 = p.Deconstruct(z99, y99), z99 = v$0[0], y99 = v$0[1];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

    }

    #region 扩展测试用例 - 元组成员访问

    /// <summary>
    /// 测试元组成员访问 - Item属性
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_ItemAccess()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, 2, 3);
                    int first = tuple.Item1;
                    int second = tuple.Item2;
                    int third = tuple.Item3;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let tuple = {
    item1: 1,
    Item2: 2,
    Item3: 3
  };
  let first = tuple.Item1;
  let second = tuple.Item2;
  let third = tuple.Item3;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组成员访问 - 命名元素
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_NamedElementAccess()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = (name: ""John"", age: 30);
                    string name = person.name;
                    int age = person.age;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let person = { name: ""John"", age: 30 };
  let name = person.name;
  let age = person.age;
}", script);
    }

    /// <summary>
    /// 测试元组成员访问 - 混合命名和未命名
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_MixedElementAccess()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (name: ""test"", 42, true);
                    string n = tuple.name;
                    int i2 = tuple.Item2;
                    bool i3 = tuple.Item3;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let tuple = {
    name: ""test"",
    Item2: 42,
    Item3: true
  };
  let n = tuple.name;
  let i2 = tuple.Item2;
  let i3 = tuple.Item3;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    #endregion

    #region 扩展测试用例 - 元组作为方法参数

    /// <summary>
    /// 测试元组作为方法参数
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_AsMethodParameter()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, 2);
                    PrintTuple(tuple);
                }

                void PrintTuple((int, int) t) { }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let tuple = { item1: 1, Item2: 2 };
  this.PrintTuple(tuple);
}", script);
    }

    /// <summary>
    /// 测试元组字面量作为方法参数
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_LiteralAsParameter()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    PrintTuple((1, 2));
                }

                void PrintTuple((int, int) t) { }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  this.printTuple({ item1: 1, Item2: 2 });
}", script);
    }

    /// <summary>
    /// 测试命名元组作为方法参数
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_NamedTupleAsParameter()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    PrintPerson((name: ""John"", age: 30));
                }

                void PrintPerson((string name, int age) person) { }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  this.printPerson({ name: ""John"", age: 30 });
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_Tuple_AsParameter_RemapNames()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (string name, int age) person = (""John"", 30);
                    PrintPerson(person);
                }

                void PrintPerson((string first, int years) person) { }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let person = { name: ""John"", age: 30 };
  this.PrintPerson({ first: person.name, years: person.age });
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    #endregion

    #region 扩展测试用例 - 元组作为返回值

    /// <summary>
    /// 测试元组作为返回值
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_AsReturnValue()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                (int, int) GetPoint()
                {
                    return (1, 2);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  return { item1: 1, Item2: 2 };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组方法调用
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_MethodReturn()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = GetPoint();
                    int x = point.Item1;
                    int y = point.Item2;
                }

                (int, int) GetPoint() => (1, 2);
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let point = this.getPoint();
  let x = point.item1;
  let y = point.item2;
}", script);
    }

    /// <summary>
    /// 测试命名元组返回值
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_NamedReturn()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = GetPerson();
                    string name = person.name;
                    int age = person.age;
                }

                (string name, int age) GetPerson() => (""John"", 30);
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let person = this.getPerson();
  let name = person.name;
  let age = person.age;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_Tuple_NamedReturn_RemapNames()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                (string first, int years) GetPerson()
                {
                    return (""John"", 30);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  return { first: ""John"", years: 30 };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_Tuple_NamedReturn_RemapNames_FromLocal()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                (string first, int years) GetPerson()
                {
                    (string name, int age) source = (""John"", 30);
                    return source;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let source = { name: ""John"", age: 30 };
  return { first: source.name, years: source.age };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_Tuple_NamedReturn_RemapNames_FromLambdaLocal()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<(string first, int years)> get = () =>
                    {
                        (string name, int age) source = (""John"", 30);
                        return source;
                    };
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let get = () => {
    let source = { name: ""John"", age: 30 };
    return { first: source.name, years: source.age };
  };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_Tuple_Assignment_RemapNames_AfterDeclaration()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (string name, int age) source = (""John"", 30);
                    (string first, int years) target = (""A"", 1);
                    target = source;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let source = { name: ""John"", age: 30 };
  let target = { first: ""A"", years: 1 };
  target = { first: source.name, years: source.age };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_Tuple_Assignment_RemapNames_Nested()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    ((int left, int right) pair, int total) source = ((1, 2), 3);
                    ((int x, int y) point, int sum) target = ((4, 5), 6);
                    target = source;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let source = { pair: { left: 1, right: 2 }, total: 3 };
  let target = { point: { x: 4, y: 5 }, sum: 6 };
  target = { point: { x: source.pair.left, y: source.pair.right }, sum: source.total };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_Tuple_Assignment_RemapNames()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    (string name, int age) source = (""John"", 30);
                    (string first, int years) target = source;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let source = { name: ""John"", age: 30 };
  let target = { first: source.name, years: source.age };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    #endregion

    #region 扩展测试用例 - 嵌套元组

    /// <summary>
    /// 测试嵌套元组访问
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_NestedAccess()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var nested = (1, (2, 3));
                    int a = nested.Item1;
                    int b = nested.Item2.Item1;
                    int c = nested.Item2.Item2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let nested = { item1: 1, Item2: { Item1: 2, Item2: 3 } };
  let a = nested.Item1;
  let b = nested.Item2.Item1;
  let c = nested.Item2.Item2;
}", script);
    }

    /// <summary>
    /// 测试深层嵌套元组
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_DeeplyNested()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var deep = (1, (2, (3, 4)));
                    int d1 = deep.Item1;
                    int d2 = deep.Item2.Item1;
                    int d3 = deep.Item2.Item2.Item1;
                    int d4 = deep.Item2.Item2.Item2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let deep = { item1: 1, Item2: { Item1: 2, Item2: { Item1: 3, Item2: 4 } } };
  let d1 = deep.Item1;
  let d2 = deep.Item2.Item1;
  let d3 = deep.Item2.Item2.Item1;
  let d4 = deep.Item2.Item2.Item2;
}", script);
    }

    #endregion

    #region 扩展测试用例 - 元组与var模式

    /// <summary>
    /// 测试元组解构到var
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_DeconstructVar()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var (a, b) = (1, 2);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let a, b;
  a = 1, b = 2;
}", script);
    }

    [TestMethod]
    public void Visit_Tuple_DeconstructVarThenRead_DeclaresTargetsInFunctionScope()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var pair = (Title: ""ready"", Suffix: ""!"");
                    var (label, suffix) = pair;
                    var result = label + suffix;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let label, suffix;
  let pair = { title: ""ready"", suffix: ""!"" };
  label = pair.title, suffix = pair.suffix;
  let result = label + suffix;
}", script);
    }

    /// <summary>
    /// 测试元组解构带丢弃
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_DeconstructWithDiscard()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var (a, _, c) = (1, 2, 3);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let a, c;
  a = 1, c = 3;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试多丢弃解构
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_MultipleDiscards()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var (_, _, _) = (1, 2, 3);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        // 全部丢弃，应该不产生任何变量
        AssertTupleScriptEqual(@"{ }".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    #endregion

    #region 扩展测试用例 - 元组比较变体

    /// <summary>
    /// 测试四元素元组比较
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_FourElementCompare()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var t1 = (1, 2, 3, 4);
                    var t2 = (1, 2, 3, 4);
                    bool equal = t1 == t2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let t1 = {
    item1: 1,
    Item2: 2,
    Item3: 3,
    Item4: 4
  };
  let t2 = {
    Item1: 1,
    Item2: 2,
    Item3: 3,
    Item4: 4
  };
  let equal = (t1.Item1 === t2.Item1 && t1.Item2 === t2.Item2 && t1.Item3 === t2.Item3 && t1.Item4 === t2.Item4);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组不等比较
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_NotEqualCompare()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var t1 = (1, 2);
                    var t2 = (3, 4);
                    bool notEqual = t1 != t2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let t1 = { item1: 1, Item2: 2 };
  let t2 = { Item1: 3, Item2: 4 };
  let notEqual = (t1.Item1 !== t2.Item1 || t1.Item2 !== t2.Item2);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    #region 扩展测试用例 - 更多元组场景

    /// <summary>
    /// 测试元组创建 - 空元组
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_ValueTupleEmpty()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var empty = ValueTuple.Create();
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  let empty = null;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    [TestMethod]
    public void Visit_Tuple_ValueTupleCreateTwoElements()
    {
        var block = GetBlockOperation(@"
            using System;

            class TestClass
            {
                void TestMethod()
                {
                    var pair = ValueTuple.Create(1, 2);
                    int sum = pair.Item1 + pair.Item2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  let pair = { item1: 1, Item2: 2 };
  let sum = pair.Item1 + pair.Item2;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组 - 五元素元组
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_FiveElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var t = (1, 2, 3, 4, 5);
                    int sum = t.Item1 + t.Item2 + t.Item3 + t.Item4 + t.Item5;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let t = {
    item1: 1,
    Item2: 2,
    Item3: 3,
    Item4: 4,
    Item5: 5
  };
  let sum = t.Item1 + t.Item2 + t.Item3 + t.Item4 + t.Item5;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组 - 六元素元组
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_SixElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var t = (1, 2, 3, 4, 5, 6);
                    int sum = t.Item1 + t.Item2 + t.Item3 + t.Item4 + t.Item5 + t.Item6;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  let t = {
    item1: 1,
    Item2: 2,
    Item3: 3,
    Item4: 4,
    Item5: 5,
    Item6: 6
  };
  let sum = t.Item1 + t.Item2 + t.Item3 + t.Item4 + t.Item5 + t.Item6;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组 - 带表达式计算
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_WithComputedValues()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 10;
                    var t = (a + 1, a * 2, a / 3);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let a = 10;
  let t = {
    item1: a + 1,
    Item2: a * 2,
    Item3: a / 3
  };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组解构 - 交换变量
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_Swap()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1, b = 2;
                    (a, b) = (b, a);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let v$0, v$1;
  let a = 1, b = 2;
  v$0 = b, v$1 = a, a = v$0, b = v$1;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组 - 嵌套访问
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_NestedAccess1()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var nested = ((1, 2), (3, 4));
                    int a = nested.Item1.Item1;
                    int b = nested.Item2.Item2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  let nested = { item1: { item1: 1, Item2: 2 }, Item2: { Item1: 3, Item2: 4 } };
  let a = nested.Item1.Item1;
  let b = nested.Item2.Item2;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组 - 方法返回元组
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_MethodReturn1()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var (x, y) = GetPoint();
                }

                (int, int) GetPoint() => (10, 20);
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  let v$0, x, y;
  v$0 = this.GetPoint(), x = v$0.Item1, y = v$0.Item2;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组 - 字符串类型元素
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_StringElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var t = (""hello"", ""world"");
                    string greeting = t.Item1 + "" "" + t.Item2;
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let t = { item1: ""hello"", Item2: ""world"" };
  let greeting = t.Item1 + "" "" + t.Item2;
}", script);
    }

    /// <summary>
    /// 测试元组 - 混合类型元素
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_MixedTypeElements()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var t = (1, ""two"", 3.0, true);
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(
@"{
  let t = {
    item1: 1,
    Item2: ""two"",
    Item3: 3,
    Item4: true
  };
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组 - 作为字典键
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_AsDictionaryKey()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<(int, int), string>();
                    dict[(1, 2)] = ""value"";
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() =>
        {
            _ = walker.Visit(block, new());
        });

        StringAssert.Contains(exception.Message, "JS-stable default equality");
        StringAssert.Contains(exception.Message, "System.Collections.Generic.Dictionary<(int, int), string>");
        StringAssert.Contains(exception.Message, "(int, int)");
    }

    /// <summary>
    /// 测试元组 - 循环中使用
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_InLoop()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var points = new (int, int)[] { (1, 2), (3, 4), (5, 6) };
                    foreach (var (x, y) in points)
                    {
                        Console.WriteLine(x + "","" + y);
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  let points = [{ item1: 1, Item2: 2 }, { Item1: 3, Item2: 4 }, { Item1: 5, Item2: 6 }];
  for (let { item1: x, item2: y } of points) {
    console.log(x + "","" + y);
  }
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

    /// <summary>
    /// 测试元组 - switch中使用
    /// </summary>
    [TestMethod]
    public void Visit_Tuple_InSwitch()
    {
        var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var point = (0, 0);
                    switch (point)
                    {
                        case (0, 0):
                            Console.WriteLine(""origin"");
                            break;
                        default:
                            Console.WriteLine(""other"");
                            break;
                    }
                }
            }
            ");

        var walker = new SemanticWalker(true);
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        AssertTupleScriptEqual(@"{
  let point = { item1: 0, Item2: 0 };
  (() => {
    const v$0 = point;
    if (v$0.Item1 === 0 && v$0.Item2 === 0) {
      console.log(""origin"");
      return;
    }
    console.log(""other"");
    return;
  })();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
    }

#endregion
}
#endregion
