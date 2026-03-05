# Jazor.CompilerTest 测试规则文档

## 1. 测试框架

本项目使用 **MSTest** 测试框架进行单元测试。

### 1.1 测试类标识

```csharp
[TestClass]
public sealed class SemanticWalkerXxxTest
{
    // 测试方法
}
```

### 1.2 测试方法标识

```csharp
[TestMethod]
public void Visit_MethodName_Scenario()
{
    // 测试代码
}
```

## 2. 测试文件组织

### 2.1 文件命名规范

| 测试文件 | 测试内容 |
|---------|---------|
| `SemanticWalkerPatternTest.cs` | 模式匹配测试 |
| `SemanticWalkerLoopTest.cs` | 循环语句测试 (for, foreach, while, do-while) |
| `SemanticWalkerSwitchTest.cs` | Switch 语句测试 |
| `SemanticWalkerStringTest.cs` | 字符串插值测试 |
| `SemanticWalkerTryCatchTest.cs` | 异常处理测试 |
| `SemanticWalkerDeclarationTest.cs` | 变量声明测试 |
| `SemanticWalkerOrdinaryTest.cs` | 二元/一元运算、条件表达式测试 |
| `SemanticWalkerReferenceTest.cs` | 字段、属性、方法引用、数组索引测试 |
| `SemanticWalkerCreationTest.cs` | 对象/数组创建测试 |
| `SemanticWalkerTupleTest.cs` | 元组和解构测试 |
| `SemanticWalkerBoundaryTest.cs` | 边界条件测试 |
| `SemanticWalkerInvalidTest.cs` | 无效操作测试 |
| `AstConverterTests.cs` | 类级别转换器测试 |
| `OptimizerTest.cs` | 优化器单元测试 |

### 2.2 测试类内部组织

使用 `#region` 块对测试进行分组：

```csharp
#region 基础功能测试

[TestMethod]
public void Visit_Scenario_Basic()
{
    // 测试代码
}

#endregion

#region 边界条件测试

[TestMethod]
public void Visit_Scenario_Boundary()
{
    // 测试代码
}

#endregion
```

## 3. 辅助方法

### 3.1 GetBlockOperation 方法

每个测试类都包含一个标准的 `GetBlockOperation` 方法：

```csharp
/// <summary>
/// 编译代码并获取roslyn代码块
/// </summary>
private static IBlockOperation GetBlockOperation(string code)
{
    var usings = @"
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;
        global using System.Numerics;
        global using ECMAScript;
        global using static ECMAScript.Global;";

    var references = Basic.Reference.Assemblies.Net100.References.All
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
    var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
    if (methodDeclaration?.Body is not null)
    {
        var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
        if (operation is not null)
            return operation;
    }

    throw new InvalidOperationException("未找到可分析的操作");
}
```

### 3.2 GetOperationAt 方法

获取指定索引的操作：

```csharp
/// <summary>
/// 获取指定索引的操作
/// </summary>
private static T GetOperationAt<T>(IBlockOperation block, int index = 0) where T : class, IOperation
{
    var operation = block.Operations.Skip(index).First();
    return operation as T ?? throw new InvalidOperationException("未找到可分析的操作");
}
```

## 4. 测试方法命名规范

### 4.1 标准命名格式

```
Visit_<MethodName>_<Scenario>
```

或

```
Visit_<MethodName>_<Scenario>_<ExpectedBehavior>
```

### 4.2 命名示例

| 测试方法名 | 说明 |
|-----------|------|
| `Visit_ForEachLoop` | 测试 foreach 循环转换 |
| `Visit_ForLoop_Simple` | 测试简单 for 循环 |
| `Visit_ForLoop_NoInit` | 测试无初始化的 for 循环 |
| `Visit_Switch_SingleCase` | 测试单个 case 的 switch |
| `Visit_Switch_PatternMatching_TypePattern` | 测试类型模式的 switch |
| `Visit_InterpolatedString_Simple` | 测试简单插值字符串 |
| `Visit_LocalReference_Simple` | 测试简单局部变量引用 |
| `Visit_Try_SingleCatch` | 测试单个 catch 的 try |

### 4.3 直接方法测试命名

```
DirectVisit_<MethodName>_<Scenario>
```

示例：
- `DirectVisit_ArrayInitializer`
- `DirectVisit_VariableDeclarator`

## 5. 测试方法结构

### 5.1 标准测试结构

```csharp
/// <summary>
/// 测试 XXX - 场景描述
/// C# 示例：
/// // C# 代码示例
/// 转换结果：// JavaScript 代码示例
/// </summary>
[TestMethod]
public void Visit_MethodName_Scenario()
{
    // Arrange: 准备测试代码
    var block = GetBlockOperation(@"
        class TestClass
        {
            void TestMethod()
            {
                // 测试代码
            }
        }
    ");

    // Act: 执行转换
    var walker = new SemanticWalker(true);
    var node = walker.Visit(block, new());
    var script = node?.ToKnRECMAScript();

    // Assert: 验证结果
    Assert.AreEqual(@"{
  // 期望的 JavaScript 代码
}", script);
}
```

### 5.2 直接方法调用测试

```csharp
[TestMethod]
public void DirectVisit_MethodName_Scenario()
{
    var block = GetBlockOperation(@"
        class TestClass
        {
            void TestMethod()
            {
                // 测试代码
            }
        }
    ");

    var walker = new SemanticWalker(true);
    var operation = GetOperationAt<IXxxOperation>(block, 0);
    var node = walker.VisitXxx(operation, new());
    var script = node?.ToECMAScript();  // 或 ToKnRECMAScript()

    Assert.AreEqual("期望结果", script);
}
```

## 6. XML 文档注释规范

### 6.1 必需的文档元素

```csharp
/// <summary>
/// 测试 XXX - 简要描述
/// C# 示例：
/// // C# 代码示例
/// 转换结果：// JavaScript 代码示例
/// </summary>
```

### 6.2 完整示例

```csharp
/// <summary>
/// 测试 VisitForEachLoop - ForEach 循环操作
/// C# 示例：
/// foreach (var num in numbers) { Console.WriteLine(num); }
/// 转换结果：for (num of numbers) { console.log(num); }
/// </summary>
[TestMethod]
public void Visit_ForEachLoop()
{
    // ...
}
```

## 7. 输出格式

### 7.1 ToKnRECMAScript()

用于生成可读性强的多行格式输出：

```csharp
var script = node?.ToKnRECMAScript();

// 输出格式：
@"{
  let x = 1;
  let y = 2;
}"
```

### 7.2 ToECMAScript()

用于生成紧凑的单行格式输出：

```csharp
var script = node?.ToECMAScript();

// 输出格式："{let x=1;let y=2}"
```

## 8. 断言规范

### 8.1 字符串比较

使用 `Assert.AreEqual` 进行精确字符串匹配：

```csharp
Assert.AreEqual(@"{
  let x = 1;
}", script);
```

### 8.2 类型检查

```csharp
Assert.IsInstanceOfType<Identifier>(result);
Assert.AreEqual("a", ((Identifier)result).Name);
```

### 8.3 空值检查

```csharp
Assert.IsNull(node);
Assert.IsNotNull(result);
```

### 8.4 集合验证

```csharp
Assert.HasCount(2, operands);
Assert.IsTrue(operands.Any(op => op is Identifier id && id.Name == "a"));
```

## 9. 测试模式

### 9.1 整体转换测试

测试完整的代码块转换：

```csharp
var walker = new SemanticWalker(true);
var node = walker.Visit(block, new());
var script = node?.ToKnRECMAScript();
```

### 9.2 单方法测试

测试单个 Visit 方法：

```csharp
var operation = GetOperationAt<IXxxOperation>(block, 0);
var node = walker.VisitXxx(operation, new());
```

### 9.3 带上下文测试

使用 `WalkerArgument` 传递上下文：

```csharp
var ctx = new WalkerArgument();
var node = walker.Visit(block, ctx);
// 可检查 ctx.HasVarDeclarator 等状态
Assert.IsTrue(ctx.HasVarDeclarator);
```

## 10. 特殊测试场景

### 10.1 边界条件测试

测试极端值、空值、最大/最小值等：

```csharp
[TestMethod]
public void BitwiseOp_WithZero() { /* ... */ }

[TestMethod]
public void NumericBoundary_IntMaxValue() { /* ... */ }
```

### 10.2 嵌套结构测试

测试深层嵌套的结构：

```csharp
[TestMethod]
public void NestedObjectCreation_DeepNesting() { /* ... */ }
```

### 10.3 异常测试

测试不支持的操作：

```csharp
[TestMethod]
public void Convert_NonPublicClass_ThrowsNotSupportedException()
{
    var exception = Assert.Throws<NotSupportedException>(() => converter.Convert());
    Assert.Contains("不是 public", exception.Message);
}
```

## 11. 命名空间和引用

### 11.1 标准引用

```csharp
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
```

### 11.2 命名空间

```csharp
namespace Jazor.ComplierTest;
```

## 12. 测试数据准备

### 12.1 使用内联代码

```csharp
var block = GetBlockOperation(@"
    class TestClass
    {
        void TestMethod()
        {
            int x = 5;
            string name = ""Hello"";
        }
    }
");
```

### 12.2 使用原始字符串字面量

```csharp
var code = """
    public static class TestClass
    {
        public static int Field = 42;
    }
    """;
```

---

**文档版本**: v1.0
**最后更新**: 2026-03-04
