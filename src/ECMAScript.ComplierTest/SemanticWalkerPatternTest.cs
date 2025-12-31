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
  public void VisitIsPattern_Constant_Direct()
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
    var node = walker.VisitIsPattern(isPatternOperation, new());
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
  public void VisitIsPattern_StringConstant_Direct()
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
    var node = walker.VisitIsPattern(isPatternOperation, new());
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
  public void VisitIsType_String_Direct()
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
    var isTypeOperation = GetOperationAt<IIsTypeOperation>(block, 1);
    var node = walker.VisitIsType(isTypeOperation, new());
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
    var isTypeOperation = GetOperationAt<IIsTypeOperation>(block, 1);
    var node = walker.VisitIsType(isTypeOperation, new());
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
  public void VisitIsType_Boolean_Direct()
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
    var isTypeOperation = GetOperationAt<IIsTypeOperation>(block, 1);
    var node = walker.VisitIsType(isTypeOperation, new());
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
  let obj = {};
  let result = typeof obj === 'object' && obj !== null;
}", script);
  }

  /// <summary>
  /// 测试 VisitIsType - 对象类型检查（直接调用）
  /// </summary>
  [TestMethod]
  public void VisitIsType_Object_Direct()
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
    var isTypeOperation = GetOperationAt<IIsTypeOperation>(block, 1);
    var node = walker.VisitIsType(isTypeOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("typeof obj === 'object' && obj !== null", script);
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
  public void VisitIsNull_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var node = walker.Visit(isPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("obj === null", script);
  }

  // ==================== VisitDiscardPattern 测试 ====================

  /// <summary>
  /// 测试 Visit - DiscardPattern 丢弃模式
  /// </summary>
  [TestMethod]
  public void Visit_DiscardPattern()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    bool result = value is _;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 42;
  let result = true;
}", script);
  }

  /// <summary>
  /// 测试 VisitDiscardPattern - 丢弃模式（直接调用）
  /// </summary>
  [TestMethod]
  public void VisitDiscardPattern_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 42;
                    bool result = value is _;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var discardPatternOperation = (IDiscardPatternOperation)isPatternOperation.Pattern;
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
  public void VisitNegatedPattern_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var negatedPatternOperation = (INegatedPatternOperation)isPatternOperation.Pattern;
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
  public void VisitBinaryPattern_And_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var binaryPatternOperation = (IBinaryPatternOperation)isPatternOperation.Pattern;
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
                    bool result = value is 1 or 2 or 3;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual(@"{
  let value = 5;
  let result = value === 1 || value === 2 || value === 3;
}", script);
  }

  /// <summary>
  /// 测试 VisitBinaryPattern - or 模式（直接调用）
  /// </summary>
  [TestMethod]
  public void VisitBinaryPattern_Or_Direct()
  {
    var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int value = 5;
                    bool result = value is 1 or 2 or 3;
                }
            }
            ");

    var walker = new SemanticWalker(true);
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var binaryPatternOperation = (IBinaryPatternOperation)isPatternOperation.Pattern;
    var node = walker.VisitBinaryPattern(binaryPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("value === 1 || value === 2 || value === 3", script);
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
  public void VisitRelationalPattern_GreaterThan_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation.Pattern;
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
  public void VisitRelationalPattern_LessThan_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation.Pattern;
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
  public void VisitRelationalPattern_GreaterThanOrEqual_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation.Pattern;
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
  public void VisitRelationalPattern_LessThanOrEqual_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var relationalPatternOperation = (IRelationalPatternOperation)isPatternOperation.Pattern;
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
  public void VisitTypePattern_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var typePatternOperation = (ITypePatternOperation)isPatternOperation.Pattern;
    var node = walker.VisitTypePattern(typePatternOperation, new());
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
  let result = typeof person === 'object' && person.Name === 'John';
}", script);
  }

  /// <summary>
  /// 测试 VisitPropertySubpattern - 属性子模式（直接调用）
  /// </summary>
  [TestMethod]
  public void VisitPropertySubpattern_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var recursivePatternOperation = (IRecursivePatternOperation)isPatternOperation.Pattern;
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
  let result = typeof obj === 'object' && obj.Name === 'John' && obj.Age > 18;
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var recursivePatternOperation = (IRecursivePatternOperation)isPatternOperation.Pattern;
    var node = walker.VisitRecursivePattern(recursivePatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("typeof obj === 'object' && obj.Name === 'John' && obj.Age > 18", script);
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
  let result = Array.isArray(array) && array.length === 3 && (array[0] === 1 && array[1] === 2 && array[2] === 3);
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var listPatternOperation = (IListPatternOperation)isPatternOperation.Pattern;
    var node = walker.VisitListPattern(listPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("Array.isArray(array) && array.length === 3 && (array[0] === 1 && array[1] === 2 && array[2] === 3)", script);
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
  let result = Array.isArray(array) && array.length >= 2 && (array[0] === 1 && array[1] === 2);
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var listPatternOperation = (IListPatternOperation)isPatternOperation.Pattern;
    var node = walker.VisitListPattern(listPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("Array.isArray(array) && array.length >= 2 && (array[0] === 1 && array[1] === 2)", script);
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
  let result = Array.isArray(array);
}", script);
  }

  /// <summary>
  /// 测试 VisitSlicePattern - 切片模式（直接调用）
  /// </summary>
  [TestMethod]
  public void VisitSlicePattern_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var listPatternOperation = (IListPatternOperation)isPatternOperation.Pattern;
    var slicePatternOperation = (ISlicePatternOperation)listPatternOperation.Patterns.First();
    var node = walker.VisitSlicePattern(slicePatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("Array.isArray(array)", script);
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
  if (typeof obj === 'number') {
    Console.WriteLine(obj);
  }
}", script);
  }

  /// <summary>
  /// 测试 VisitDeclarationPattern - 声明模式（直接调用）
  /// </summary>
  [TestMethod]
  public void VisitDeclarationPattern_Direct()
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
    var isPatternOperation = GetOperationAt<IIsPatternOperation>(block, 1);
    var declarationPatternOperation = (IDeclarationPatternOperation)isPatternOperation.Pattern;
    var node = walker.VisitDeclarationPattern(declarationPatternOperation, new());
    var script = node?.ToKnRECMAScript();

    Assert.AreEqual("let value;", script);
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
  let result = value > 0 && value < 10 ? 'Small' : value >= 10 ? 'Large' : 'Unknown';
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
  public void VisitConstantPattern_Direct()
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
  public void VisitConstantPattern_String_Direct()
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
  public void VisitPatternCaseClause_Direct()
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
