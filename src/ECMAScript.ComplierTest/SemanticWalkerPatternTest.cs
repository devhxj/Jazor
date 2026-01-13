using System.Text;
using Acornima.Ast;
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Context = System.Collections.Generic.Queue<Acornima.Ast.VariableDeclaration>;

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
  let obj = 'hello';
  let result = obj === 'hello';
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

    Assert.AreEqual("obj === 'hello'", script);
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
  let obj = 'hello';
  let result = typeof obj === 'string';
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

    Assert.AreEqual("typeof obj === 'string'", script);
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
  let result = typeof obj === 'number';
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

    Assert.AreEqual("typeof obj === 'number'", script);
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
  let result = typeof obj === 'boolean';
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

    Assert.AreEqual("typeof obj === 'boolean'", script);
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
  let result = typeof obj === 'object';
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

    Assert.AreEqual("typeof obj === 'object'", script);
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
      return 'one';
    if (v$test === 2)
      return 'two';
    return 'default';
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
  let obj = 'hello';
  let result = typeof obj === 'string';
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

    Assert.AreEqual("typeof obj === 'string'", script);
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
  let person = { Name: 'John', Age: 30 };
  let result = person.Name === 'John';
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

    Assert.AreEqual("person.Name === 'John'", script);
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
  let obj = { Name: 'John', Age: 30 };
  let result = obj.Name === 'John' && obj.Age > 18;
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

    Assert.AreEqual("obj.Name === 'John' && obj.Age > 18", script);
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
  let rest;
  let array = [1, 2, 3, 4, 5];
  if (Array.isArray(array) && array.length >= 0 && (rest = array, true)) {
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

    var b = walker.Visit(block, new())?.ToECMAScript();
    var context = new Context();
    var node = walker.VisitSlicePattern(slicePatternOperation, context);

    var builder = new StringBuilder();
    while (context.TryDequeue(out var decl))
      builder.AppendLine(decl.ToECMAScript());

    var vars = builder.ToString();
    var script = node?.ToECMAScript();

    // 验证生成的表达式
    Assert.AreEqual(@"rest=array,true", script);
    Assert.AreEqual(@"let rest
", vars);
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
  let first, rest;
  let array = [1, 2, 3, 4, 5];
  if (Array.isArray(array) && array.length >= 1 && (first = array[0], true) && (rest = array, true)) {
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
  let first, last;
  let array = [1, 2, 3, 4, 5];
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
                        [1] => ""single one"",
                        [1, 2] => ""one two"",
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
    if (Array.isArray(array) && array.length >= 0)
      return 'empty or any';
    if (Array.isArray(array) && array.length === 1 && array[0] === 1)
      return 'single one';
    if (Array.isArray(array) && array.length === 2 && array[0] === 1 && array[1] === 2)
      return 'one two';
    return 'other';
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
  let value;
  let obj = 42;
  if (typeof obj === 'number' && (value = obj, true)) {
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

    Assert.AreEqual("typeof obj === 'number' && (value = obj, true)", script);
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
                    string result = value switch
                    {
                        > 0 and < 10 => ""Small"",
                        >= 10 => ""Large"",
                        _ => ""Unknown""
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
    const v$test = value;
    if (value > 0 && value < 10)
      return 'Small';
    if (value >= 10)
      return 'Large';
    return 'Unknown';
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
  let person = { Name: 'John', Age: 30 };
  let result = typeof person === 'object' && person.Name === 'John' ? 'Hello John' : typeof person === 'object' && person.Age > 18 ? 'Adult' : 'Unknown';
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
  if (Array.isArray(array) && array.length >= 2) {
    Console.WriteLine(array[0]);
    Console.WriteLine(array[1]);
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
  let result = typeof data === 'object' && data.Inner.Value > 0;
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
  let obj = 'hello';
  let result = typeof obj === 'string' && obj.Length > 0;
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var constantPatternOperation = (IConstantPatternOperation)isPatternOperation.Pattern;
    var node = walker.VisitConstantPattern(constantPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("42", script);
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var constantPatternOperation = (IConstantPatternOperation)isPatternOperation.Pattern;
    var node = walker.VisitConstantPattern(constantPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("'hello'", script);
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
                    object obj = 42;
                    switch (obj)
                    {
                        case 42:
                            break;
                    }
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var switchOperation = GetOperationAt<ISwitchOperation>(block, 1);
    var patternCaseClause = (IPatternCaseClauseOperation)switchOperation.Cases.First();
    var node = walker.VisitPatternCaseClause(patternCaseClause, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("42", script);
  }
}
