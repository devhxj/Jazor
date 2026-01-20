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
    var usings = @"
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;";

    var compilation = CSharpCompilation.Create(
        "TestAssembly",
        syntaxTrees: [
          CSharpSyntaxTree.ParseText(usings),
          CSharpSyntaxTree.ParseText(code)
        ],
        references: Basic.Reference.Assemblies.Net100.References.All,
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

  // ==================== VisitIsPattern 测试 ====================

  /// <summary>
  /// 测试 Visit - IsPattern 常量模式匹配
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_Constant()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42;
  let result = obj === 42;
}", script);
  }

  /// <summary>
  /// 测试 VisitIsPattern - 常量模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_Constant_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var node = walker.VisitIsPattern(isPatternOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("obj === 42", script);
  }

  /// <summary>
  /// 测试 Visit - IsPattern 字符串常量模式
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_StringConstant()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is ""hello"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""hello"";
  let result = obj === ""hello"";
}", script);
  }

  /// <summary>
  /// 测试 VisitIsPattern - 字符串常量模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsPattern_StringConstant_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is ""hello"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var node = walker.VisitIsPattern(isPatternOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"obj === ""hello""", script);
  }

  // ==================== VisitIsType 测试 ====================

  /// <summary>
  /// 测试 Visit - IsType 字符串类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_String()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""hello"";
  let result = typeof obj === ""string"";
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 字符串类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsType_String_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""string""", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 整数类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Int()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is int;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42;
  let result = typeof obj === ""number"";
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 整数类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void VisitIsType_Int_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is int;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""number""", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 布尔类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Boolean()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = true;
                    bool result = obj is bool;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = true;
  let result = typeof obj === ""boolean"";
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 布尔类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Boolean_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = true;
                    bool result = obj is bool;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""boolean""", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 对象类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Object()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new object();
                    bool result = obj is object;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Object;
  let result = typeof obj === ""object"";
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 对象类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Object_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new object();
                    bool result = obj is object;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""object""", script);
  }

  // ==================== VisitIsNull 测试 ====================

  /// <summary>
  /// 测试 Visit - IsNull null 检查
  /// </summary>
  [TestMethod]
  public void Visit_IsNull()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = null;
  let result = obj === null;
}", script);
  }

  /// <summary>
  /// 测试 VisitIsNull - null 检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsNull_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var node = walker.Visit(isPatternOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("obj === null", script);
  }

  // ==================== VisitIsNotNull 测试 ====================

  /// <summary>
  /// 测试 Visit - IsNotNull not null 检查
  /// </summary>
  [TestMethod]
  public void Visit_IsNotNull()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is not null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = null;
  let result = !(obj === null);
}", script);
  }

  /// <summary>
  /// 测试 VisitIsNotNull -not null 检查（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_IsNotNull_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = null;
                    bool result = obj is not null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var node = walker.Visit(isPatternOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("!(obj === null)", script);
  }

  // ==================== VisitDiscardPattern 测试 ====================

  /// <summary>
  /// 测试 Visit - DiscardPattern 丢弃模式（在 switch 表达式中）
  /// 丢弃模式 _ 作为 switch 表达式的默认分支，总是匹配
  /// </summary>
  [TestMethod]
  public void Visit_DiscardPattern_SwitchExpression()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    string result = value switch
                    {
                        1 => ""one"",
                        2 => ""two"",
                        _ => ""default""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 42;
  let result = (() => {
    const v$test = value;
    if (v$test === 1)
      return ""one"";
    if (v$test === 2)
      return ""two"";
    return ""default"";
  })();
}", script);
  }

  /// <summary>
  /// 测试 VisitDiscardPattern - 丢弃模式（直接调用）
  /// 丢弃模式总是返回 true，表示总是匹配
  /// </summary>
  [TestMethod]
  public void Visit_DiscardPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    string result = value switch
                    {
                        _ => ""always matches""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var switchExpressionOperation = declarator.Initializer!.Value as ISwitchExpressionOperation;
    var switchCaseArm = switchExpressionOperation!.Arms.First();
    var discardPatternOperation = (IDiscardPatternOperation)switchCaseArm.Pattern;
    var node = walker.VisitDiscardPattern(discardPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("true", script);
  }

  // ==================== VisitNegatedPattern 测试 ====================

  /// <summary>
  /// 测试 Visit - NegatedPattern 取反模式
  /// </summary>
  [TestMethod]
  public void Visit_NegatedPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is not null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42;
  let result = !(obj === null);
}", script);
  }

  /// <summary>
  /// 测试 VisitNegatedPattern - 取反模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_NegatedPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is not null;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var negatedPatternOperation = (INegatedPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitNegatedPattern(negatedPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("!(obj === null)", script);
  }

  // ==================== VisitBinaryPattern 测试 ====================

  /// <summary>
  /// 测试 Visit - BinaryPattern and 模式
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_And()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0 and < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value > 0 && value < 10;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryPattern - and 模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_And_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0 and < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var binaryPatternOperation = (IBinaryPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitBinaryPattern(binaryPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value > 0 && value < 10", script);
  }

  /// <summary>
  /// 测试 Visit - BinaryPattern or 模式
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_Or()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is 1 or 2 or 3 or 4 or >8;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value === 1 || value === 2 || value === 3 || value === 4 || value > 8;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryPattern - or 模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_Or_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is 1 or 2 or 3 or 4 or >8;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var binaryPatternOperation = (IBinaryPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitBinaryPattern(binaryPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value === 1 || value === 2 || value === 3 || value === 4 || value > 8", script);
  }

  // ==================== VisitRelationalPattern 测试 ====================

  /// <summary>
  /// 测试 Visit - RelationalPattern 大于模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThan()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value > 0;
}", script);
  }

  /// <summary>
  /// 测试 VisitRelationalPattern - 大于模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThan_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitRelationalPattern(relationalPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value > 0", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 小于模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThan()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value < 10;
}", script);
  }

  /// <summary>
  /// 测试 VisitRelationalPattern - 小于模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThan_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is < 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitRelationalPattern(relationalPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value < 10", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 大于等于模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is >= 5;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value >= 5;
}", script);
  }

  /// <summary>
  /// 测试 VisitRelationalPattern - 大于等于模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_GreaterThanOrEqual_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is >= 5;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitRelationalPattern(relationalPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value >= 5", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 小于等于模式
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThanOrEqual()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is <= 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value <= 10;
}", script);
  }

  /// <summary>
  /// 测试 VisitRelationalPattern - 小于等于模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_LessThanOrEqual_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is <= 10;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitRelationalPattern(relationalPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value <= 10", script);
  }

  // ==================== VisitTypePattern 测试 ====================

  /// <summary>
  /// 测试 Visit - TypePattern 类型模式
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""hello"";
  let result = typeof obj === ""string"";
}", script);
  }

  /// <summary>
  /// 测试 VisitTypePattern - 类型模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_TypePattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isTypeOperation = declarator.Initializer!.Value as IIsTypeOperation;
    var node = walker.VisitIsType(isTypeOperation!, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""string""", script);
  }

  // ==================== VisitPropertySubpattern 测试 ====================

  /// <summary>
  /// 测试 Visit - PropertySubpattern 属性子模式
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = new { Name = ""John"", Age = 30 };
                    bool result = person is { Name: ""John"" };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let person = { Name: ""John"", Age: 30 };
  let result = person.Name === ""John"";
}", script);
  }

  /// <summary>
  /// 测试 VisitPropertySubpattern - 属性子模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = new { Name = ""John"", Age = 30 };
                    bool result = person is { Name: ""John"" };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var recursivePatternOperation = (IRecursivePatternOperation)isPatternOperation!.Pattern;
    var propertySubpatternOperation = recursivePatternOperation.PropertySubpatterns.First();
    var node = walker.VisitPropertySubpattern(propertySubpatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"person.Name === ""John""", script);
  }

  // ==================== VisitRecursivePattern 测试 ====================

  /// <summary>
  /// 测试 Visit - RecursivePattern 递归模式（类型+属性）
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_TypeAndProperty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 30 };
                    bool result = obj is { Name: ""John"", Age: > 18 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = { Name: ""John"", Age: 30 };
  let result = obj.Name === ""John"" && obj.Age > 18;
}", script);
  }

  /// <summary>
  /// 测试 VisitRecursivePattern - 递归模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""John"", Age = 30 };
                    bool result = obj is { Name: ""John"", Age: > 18 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var recursivePatternOperation = (IRecursivePatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitRecursivePattern(recursivePatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"obj.Name === ""John"" && obj.Age > 18", script);
  }

  // ==================== VisitListPattern 测试 ====================

  /// <summary>
  /// 测试 Visit - ListPattern 列表模式（固定长度）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_FixedLength()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    bool result = array is [1, 2, 3];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let result = Array.isArray(array) && array.length === 3 && array[0] === 1 && array[1] === 2 && array[2] === 3;
}", script);
  }

  /// <summary>
  /// 测试 VisitListPattern - 列表模式（固定长度）（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_FixedLength_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    bool result = array is [1, 2, 3];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var listPatternOperation = (IListPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitListPattern(listPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("Array.isArray(array) && array.length === 3 && array[0] === 1 && array[1] === 2 && array[2] === 3", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 列表模式（带切片）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_WithSlice()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [1, 2, ..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[1] === 2;
}", script);
  }

  /// <summary>
  /// 测试 VisitListPattern - 列表模式（带切片）（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_WithSlice_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [1, 2, ..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var listPatternOperation = (IListPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitListPattern(listPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[1] === 2", script);
  }

  // ==================== VisitSlicePattern 测试 ====================

  /// <summary>
  /// 测试 Visit - SlicePattern 切片模式
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片带变量捕获（解构赋值）
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_WithVariableCapture()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [.. var rest])
                    {
                        Console.WriteLine(rest.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);

    var node = walker.Visit(block, []);
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let rest;
  if (Array.isArray(array) && array.length >= 0 && (rest = array.slice(0), true)) {
    Console.WriteLine(rest.Length);
  }
}", script);

  }

  /// <summary>
  /// 测试 VisitSlicePattern - 切片带变量捕获（直接调用）
  /// 验证切片模式中的声明模式会被正确处理，变量名被添加到上下文
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_WithVariableCapture_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [.. var rest])
                    {
                        Console.WriteLine(rest.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var ifOperation = GetOperationAt<IConditionalOperation>(block, 1);
    var isPatternOperation = (IIsPatternOperation)ifOperation.Condition;
    var listPatternOperation = (IListPatternOperation)isPatternOperation.Pattern;
    var slicePatternOperation = (ISlicePatternOperation)listPatternOperation.Patterns.First();
    var context = new Queue<VariableDeclarator>();
    var node = walker.VisitSlicePattern(slicePatternOperation, context);
    var script = node?.ToECMAScript();

    // 验证生成的表达式
    Assert.AreEqual(@"rest=array.slice(0),true", script);
    Assert.HasCount(1, context);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片在列表开头
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_AtStart()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [.., 4, 5];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 2 && array[array.length - 2] === 4 && array[array.length - 1] === 5;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片在列表中间
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_InMiddle()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [1, .., 5];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[array.length - 1] === 5;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片在列表末尾（多元素前）
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_AtEnd()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [1, 2, ..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 2 && array[0] === 1 && array[1] === 2;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片带前缀和后缀变量捕获
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_WithPrefixAndSuffix()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [var first, .. var rest])
                    {
                        Console.WriteLine(first);
                        Console.WriteLine(rest.Length);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let first, rest;
  if (Array.isArray(array) && array.length >= 1 && (first = array[0], true) && (rest = array.slice(1), true)) {
    Console.WriteLine(first);
    Console.WriteLine(rest.Length);
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片带前缀、中间和后缀
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_ComplexDestructuring()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [var first, .., var last])
                    {
                        Console.WriteLine(first);
                        Console.WriteLine(last);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let first, last;
  if (Array.isArray(array) && array.length >= 2 && (first = array[0], true) && (last = array[array.length - 1], true)) {
    Console.WriteLine(first);
    Console.WriteLine(last);
  }
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片与 switch 表达式结合
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_InSwitchExpression()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2];
                    string result = array switch
                    {
                        [..] => ""empty or any"",
                        _ => ""other""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2];
  let result = (() => {
    const v$test = array;
    if (Array.isArray(v$test) && v$test.length >= 0)
      return ""empty\ or\ any"";
    return ""other"";
  })();
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 空数组匹配
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_EmptyArray()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [];
                    bool result = array is [..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [];
  let result = Array.isArray(array) && array.length >= 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 单元素数组匹配
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_SingleElementArray()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [42];
                    bool result = array is [..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [42];
  let result = Array.isArray(array) && array.length >= 0;
}", script);
  }

  // ==================== VisitDeclarationPattern 测试 ====================

  /// <summary>
  /// 测试 Visit - DeclarationPattern 声明模式
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    if (obj is int value)
                    {
                        Console.WriteLine(value);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42;
  let value;
  if (typeof obj === ""number"" && (value = obj, true)) {
    Console.WriteLine(value);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitDeclarationPattern - 声明模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    if (obj is int value)
                    {
                        Console.WriteLine(value);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var conditionalOp = GetOperationAt<IConditionalOperation>(block, 1);
    var isPatternOperation = conditionalOp.Condition as IIsPatternOperation;
    var declarationPatternOperation = (IDeclarationPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitDeclarationPattern(declarationPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"typeof obj === ""number"" && (value = obj, true)", script);
  }

  // ==================== 复杂模式匹配测试 ====================

  /// <summary>
  /// 测试复杂模式匹配 - switch 表达式
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_SwitchExpression()
  {
    var block = GetBlockOperation(@"
    class TestClass
    {
      void TestMethod()
      {
        int value = 5;
        string result = Get5(value) switch
        {
          > 0 and < 10 => ""Small"",
          >= 10 => ""Large"",
          _ => ""Unknown""
        };
      }

      static int Get5(int x)
      {
        return x switch
        {
          > 0 and < 10 => 5,
          _ => 0
        };
      }	
    }
    ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = (() => {
    const v$test = TestClass.Get5(value);
    if (v$test > 0 && v$test < 10)
      return ""Small"";
    if (v$test >= 10)
      return ""Large"";
    return ""Unknown"";
  })();
}", script);

  }

  /// <summary>
  /// 测试复杂模式匹配 - 属性模式
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_PropertyPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var person = new { Name = ""John"", Age = 30 };
                    string result = person switch
                    {
                        { Name: ""John"" } => ""Hello John"",
                        { Age: > 18 } => ""Adult"",
                        _ => ""Unknown""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let person = { Name: ""John"", Age: 30 };
  let result = (() => {
    const v$test = person;
    if (v$test.Name === ""John"")
      return ""Hello\ John"";
    if (v$test.Age > 18)
      return ""Adult"";
    return ""Unknown"";
  })();
}", script);
  }

  /// <summary>
  /// 测试复杂模式匹配 - 列表解构
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_ListDestructuring()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    if (array is [var first, var second, .. var rest])
                    {
                        Console.WriteLine(first);
                        Console.WriteLine(second);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let first, second, rest;
  if (Array.isArray(array) && array.length >= 2 && (first = array[0], true) && (second = array[1], true) && (rest = array.slice(2), true)) {
    Console.WriteLine(first);
    Console.WriteLine(second);
  }
}", script);
  }

  /// <summary>
  /// 测试复杂模式匹配 - 嵌套模式
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_NestedPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var data = new { Inner = new { Value = 42 } };
                    bool result = data is { Inner: { Value: > 0 } };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let data = { Inner: { Value: 42 } };
  let result = data.Inner.Value > 0;
}", script);
  }

  /// <summary>
  /// 测试复杂模式匹配 - 类型模式与属性模式组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_TypeAndProperty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is string { Length: > 0 };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""hello"";
  let result = typeof obj === ""string"" && obj.Length > 0;
}", script);
  }

  /// <summary>
  /// 测试复杂模式匹配 - 取反模式与关系模式组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_NegatedAndRelational()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is not < 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = !(value < 0);
}", script);
  }

  /// <summary>
  /// 测试复杂模式匹配 - 多个条件组合
  /// </summary>
  [TestMethod]
  public void Visit_ComplexPattern_MultipleConditions()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is > 0 and < 10 and not 5;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value > 0 && value < 10 && !(value === 5);
}", script);
  }
  // ==================== VisitConstantPattern 测试 ====================

  /// <summary>
  /// 测试 VisitConstantPattern - 常量模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_ConstantPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is 42;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var constantPatternOperation = (IConstantPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitConstantPattern(constantPatternOperation, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual("obj===42", script);
  }

  /// <summary>
  /// 测试 VisitConstantPattern - 字符串常量模式（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_ConstantPattern_String_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""hello"";
                    bool result = obj is ""hello"";
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var isPatternOperation = declarator.Initializer!.Value as IIsPatternOperation;
    var constantPatternOperation = (IConstantPatternOperation)isPatternOperation!.Pattern;
    var node = walker.VisitConstantPattern(constantPatternOperation, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"obj===""hello""", script);
  }

  // ==================== VisitPatternCaseClause 测试 ====================

  /// <summary>
  /// 测试 VisitPatternCaseClause - 模式 case 子句（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PatternCaseClause_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int obj = 42;
                    switch (obj)
                    {
                        case var x when x>10:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (IPatternCaseClauseOperation)switchOperation.Cases.First()!.Clauses.First()!;
    var node = walker.VisitPatternCaseClause(patternCaseClause, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual("(x=obj,true)&&x>10", script);
  }

  // ==================== 特殊类型模式测试 ====================

  /// <summary>
  /// 测试 Visit - IsType DateTime 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_DateTime()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = DateTime.Now;
                    bool result = obj is DateTime;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = Now;
  let result = obj instanceof Date;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType long/Int64 类型检查 (BigInt)
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Long()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42L;
                    bool result = obj is long;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42n;
  let result = typeof obj === ""bigint"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 数组类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Array()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new int[] { 1, 2, 3 };
                    bool result = obj is int[];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = [1, 2, 3];
  let result = Array.isArray(obj);
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType Dictionary/Map 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Dictionary()
  {
    var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new Dictionary<string, int>();
                    bool result = obj is IDictionary<string, int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Map;
  let result = obj instanceof Map;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 自定义 Class 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_CustomClass()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new TestClass();
                    bool result = obj is TestClass;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new TestClass;
  let result = obj instanceof TestClass;
}", script);
  }

  // ==================== 元组模式测试 ====================

  /// <summary>
  /// 测试 Visit - RecursivePattern 元组模式
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_Tuple()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, ""hello"");
                    bool result = tuple is (int x, string s);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let tuple = { Item1: 1, Item2: ""hello"" };
  let x, s;
  let result = typeof tuple === 'object' && (typeof tuple === ""number"" && (x = tuple[0], true)) && (typeof tuple === ""string"" && (s = tuple[1], true));
}", script);
  }

  /// <summary>
  /// 测试 Visit - RecursivePattern 元组模式带条件
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_TupleWithCondition()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tuple = (1, ""hello"");
                    bool result = tuple is (int x, string s) && x > 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let tuple = [1, ""hello""];
  let x, s;
  let result = (x = tuple[0], true) && (s = tuple[1], true) && x > 0;
}", script);
  }

  // ==================== 空列表模式测试 ====================

  /// <summary>
  /// 测试 Visit - ListPattern 空列表模式
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_Empty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [];
                    bool result = array is [];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [];
  let result = Array.isArray(array) && array.length === 0;
}", script);
  }

  // ==================== 可空类型模式测试 ====================

  /// <summary>
  /// 测试 Visit - IsType 可空类型模式
  /// </summary>
  [TestMethod]
  public void Visit_IsType_NullableInt()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? value = null;
                    bool result = value is int;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = null;
  let result = value === null || typeof value === ""number"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - DeclarationPattern 可空类型带变量声明
  /// </summary>
  [TestMethod]
  public void Visit_DeclarationPattern_Nullable()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int? value = 42;
                    if (value is int v)
                    {
                        Console.WriteLine(v);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 42;
  let v;
  if ((value === null || typeof value === ""number"") && (v = value, true)) {
    Console.WriteLine(v);
  }
}", script);
  }

  // ==================== 嵌套声明模式测试 ====================

  /// <summary>
  /// 测试 Visit - BinaryPattern 嵌套声明模式
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_NestedDeclaration()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 42;
                    bool result = obj is int x and int y;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 42;
  let x, y;
  let result = (typeof obj === ""number"" && (x = obj, true)) && (typeof obj === ""number"" && (y = obj, true));
}", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 嵌套声明模式
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_NestedDeclaration()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    if (array is [var a, var b, var c])
                    {
                        Console.WriteLine(a + b + c);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let a, b, c;
  if (Array.isArray(array) && array.length === 3 && (a = array[0], true) && (b = array[1], true) && (c = array[2], true)) {
    Console.WriteLine(a + b + c);
  }
}", script);
  }

  // ==================== VisitSwitchExpressionArm 直接调用测试 ====================

  /// <summary>
  /// 测试 VisitSwitchExpressionArm - 常量模式 arm（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpressionArm_Constant_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value switch
                    {
                        1 => ""one"",
                        2 => ""two"",
                        _ => ""default""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var switchExpressionOperation = declarator.Initializer!.Value as ISwitchExpressionOperation;
    if (switchExpressionOperation is null)
      throw new InvalidOperationException("switchExpressionOperation is null");
    var switchCaseArm = switchExpressionOperation.Arms.First();
    var node = walker.VisitSwitchExpressionArm(switchCaseArm, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"if (v$test === 1)
      return ""one"";", script);
  }

  /// <summary>
  /// 测试 VisitSwitchExpressionArm - 模式 arm 带 when 子句（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpressionArm_WithGuard_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value switch
                    {
                        var x when x > 0 => ""positive"",
                        _ => ""default""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var switchExpressionOperation = declarator.Initializer!.Value as ISwitchExpressionOperation;
    if (switchExpressionOperation is null)
      throw new InvalidOperationException("switchExpressionOperation is null");
    var switchCaseArm = switchExpressionOperation.Arms.First();
    var node = walker.VisitSwitchExpressionArm(switchCaseArm, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"if ((x = v$test, true) && x > 0)
      return ""positive"";", script);
  }

  /// <summary>
  /// 测试 VisitSwitchExpressionArm - 丢弃模式 arm（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpressionArm_Discard_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    string result = value switch
                    {
                        _ => ""default""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var variableDeclarationGroupOp = GetOperationAt<IVariableDeclarationGroupOperation>(block, 1);
    var declaration = variableDeclarationGroupOp.Declarations.First();
    var declarator = declaration.Declarators.First();
    var switchExpressionOperation = declarator.Initializer!.Value as ISwitchExpressionOperation;
    if (switchExpressionOperation is null)
      throw new InvalidOperationException("switchExpressionOperation is null");
    var switchCaseArm = switchExpressionOperation.Arms.First();
    var node = walker.VisitSwitchExpressionArm(switchCaseArm, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"return ""default"";", script);
  }

  // ==================== VisitPatternCaseClause 更多测试 ====================

  /// <summary>
  /// 测试 VisitPatternCaseClause - 常量模式 case（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PatternCaseClause_Constant_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    switch (value)
                    {
                        case 1:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (IPatternCaseClauseOperation)switchOperation.Cases.First()!.Clauses.First()!;
    var node = walker.VisitPatternCaseClause(patternCaseClause, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual("v$test===1", script);
  }

  /// <summary>
  /// 测试 VisitPatternCaseClause - 关系模式 case（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PatternCaseClause_Relational_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    switch (value)
                    {
                        case > 0:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (IPatternCaseClauseOperation)switchOperation.Cases.First()!.Clauses.First()!;
    var node = walker.VisitPatternCaseClause(patternCaseClause, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual("v$test>0", script);
  }

  /// <summary>
  /// 测试 VisitPatternCaseClause - 复杂模式带 when 子句（直接调用）
  /// </summary>
  [TestMethod]
  public void Visit_PatternCaseClause_ComplexWithGuard_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string value = ""hello"";
                    switch (value)
                    {
                        case string s when s.Length > 0:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (IPatternCaseClauseOperation)switchOperation.Cases.First()!.Clauses.First()!;
    var node = walker.VisitPatternCaseClause(patternCaseClause, new());
    var script = node?.ToECMAScript();

    Assert.AreEqual(@"(typeof v$test===""string""&&(s=v$test,true))&&s.Length>0", script);
  }

  // ==================== DateOnly/TimeOnly 类型测试 ====================

  /// <summary>
  /// 测试 Visit - IsType DateOnly 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_DateOnly()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new DateOnly();
                    bool result = obj is DateOnly;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Date;
  let result = obj instanceof Date;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType DateTimeOffset 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_DateTimeOffset()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new DateTimeOffset();
                    bool result = obj is DateTimeOffset;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Date;
  let result = obj instanceof Date;
}", script);
  }

  // ==================== timestamp 类型测试 ====================

  /// <summary>
  /// 测试 Visit - IsType timestamp 类型检查 (BigInt)
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Timestamp()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new timestamp();
                    bool result = obj is timestamp;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 0n;
  let result = typeof obj === ""bigint"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType Char 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Char()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = ""A"";
                    bool result = obj is char;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = ""A"";
  let result = typeof obj === ""string"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType Decimal 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_Decimal()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = 123.45m;
                    bool result = obj is decimal;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = 123.45;
  let result = typeof obj === ""number"";
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType TimeOnly 类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_TimeOnly()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new TimeOnly();
                    bool result = obj is TimeOnly;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = new Date;
  let result = obj instanceof Date;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType IEnumerable（非 IDictionary）类型检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_IEnumerable()
  {
    var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    object obj = new List<int>();
                    bool result = obj is IEnumerable<int>;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = [];
  let result = obj instanceof Map;
}", script);
  }

  /// <summary>
  /// 测试 Visit - IsType 匿名类型模式检查
  /// </summary>
  [TestMethod]
  public void Visit_IsType_AnonymousType()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""Test"", Value = 42 };
                    bool result = obj is { Name: ""Test"" };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = { Name: ""Test"", Value: 42 };
  let result = obj.Name === ""Test"";
}", script);
  }

  // ==================== 复杂切片模式测试 ====================

  /// <summary>
  /// 测试 Visit - SlicePattern 多个切片模式
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_MultipleSlices()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    bool result = array is [.., 2, ..];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let result = Array.isArray(array) && array.length >= 1 && array[array.length - 1] === 2;
}", script);
  }

  /// <summary>
  /// 测试 Visit - SlicePattern 切片带常量模式
  /// </summary>
  [TestMethod]
  public void Visit_SlicePattern_SliceWithConstantPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    if (array is [.., var last] && last > 0)
                    {
                        Console.WriteLine(last);
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let last;
  if (Array.isArray(array) && array.length >= 1 && (last = array, true) && last > 0) {
    Console.WriteLine(last);
  }
}", script);
  }

  // ==================== 嵌套列表模式测试 ====================

  /// <summary>
  /// 测试 Visit - ListPattern 嵌套列表模式
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_NestedList()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var nested = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
                    bool result = nested is [[1, 2], [3, 4]];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let nested = [[1, 2], [3, 4]];
  let result = Array.isArray(nested) && nested.length === 2 && Array.isArray(nested[0]) && nested[0].length === 2 && nested[0][0] === 1 && nested[0][1] === 2 && Array.isArray(nested[1]) && nested[1].length === 2 && nested[1][0] === 3 && nested[1][1] === 4;
}", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 嵌套列表带切片
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_NestedListWithSlice()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var nested = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
                    bool result = nested is [[1, ..], [4, ..]];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let nested = [[1, 2, 3], [4, 5, 6]];
  let result = Array.isArray(nested) && nested.length === 2 && Array.isArray(nested[0]) && nested[0].length >= 1 && nested[0][0] === 1 && Array.isArray(nested[1]) && nested[1].length >= 1 && nested[1][0] === 4;
}", script);
  }

  // ==================== 边界情况测试 ====================

  /// <summary>
  /// 测试 Visit - RecursivePattern 空属性模式
  /// </summary>
  [TestMethod]
  public void Visit_RecursivePattern_EmptyPropertyPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { Name = ""Test"" };
                    bool result = obj is { };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = { Name: ""Test"" };
  let result = true;
}", script);
  }

  /// <summary>
  /// 测试 Visit - ListPattern 单元素列表
  /// </summary>
  [TestMethod]
  public void Visit_ListPattern_SingleElement()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [42];
                    bool result = array is [42];
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let array = [42];
  let result = Array.isArray(array) && array.length === 1 && array[0] === 42;
}", script);
  }

  /// <summary>
  /// 测试 Visit - BinaryPattern 复杂嵌套
  /// </summary>
  [TestMethod]
  public void Visit_BinaryPattern_ComplexNesting()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is (> 0 and < 10) or (>= 100 and <= 200);
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = (value > 0 && value < 10) || (value >= 100 && value <= 200);
}", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 相等和不等
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_Equality()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is == 5;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value === 5;
}", script);
  }

  /// <summary>
  /// 测试 Visit - RelationalPattern 不等
  /// </summary>
  [TestMethod]
  public void Visit_RelationalPattern_Inequality()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is != 0;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value !== 0;
}", script);
  }

  /// <summary>
  /// 测试 Visit - PropertySubpattern 嵌套属性访问
  /// </summary>
  [TestMethod]
  public void Visit_PropertySubpattern_NestedProperty()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var data = new { Inner = new { Value = 42 } };
                    bool result = data is { Inner: { Value: 42 } };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let data = { Inner: { Value: 42 } };
  let result = data.Inner.Value === 42;
}", script);
  }

  /// <summary>
  /// 测试 Visit - Switch 表达式复杂模式组合
  /// </summary>
  [TestMethod]
  public void Visit_SwitchExpression_ComplexPatternCombination()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var obj = new { X = 1, Y = 2 };
                    string result = obj switch
                    {
                        { X: 1, Y: 2 } => ""Point (1,2)"",
                        { X: var x } when x > 0 => ""Positive X"",
                        _ => ""Other""
                    };
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let obj = { X: 1, Y: 2 };
  let result = (() => {
    const v$test = obj;
    if (v$test.X === 1 && v$test.Y === 2)
      return ""Point (1,2)"";
    if ((x = v$test.X, true) && x > 0)
      return ""Positive X"";
    return ""Other"";
  })();
}", script);
  }

  [TestMethod]
  public void Visit_All()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value > 9 && (value is > 0 and < 10 and not 5) && (value is var x && x < 10);
                    switch (value)
                    {
                      case var s when s > 0:
                        Console.WriteLine("">0"");
                        break;
                      case 1:
                        Console.WriteLine(""1"");
                        break;			
                      case 2:
                        Console.WriteLine(""2"");
                        break;
                      default:
                        Console.WriteLine(""Default"");
                        break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let x;
  let result = value > 9 && (value > 0 && value < 10 && !(value === 5)) && ((x = value, true) && x < 10);
  let s;
  (() => {
    const v$test = value;
    if ((s = value, true) && s > 0) {
      Console.WriteLine("">0"");
      return;
    }
    if (v$test === 1) {
      Console.WriteLine(""1"");
      return;
    }
    if (v$test === 2) {
      Console.WriteLine(""2"");
      return;
    }
    Console.WriteLine(""Default"");
    return;
  })();
}", script);
  }


}
