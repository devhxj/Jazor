# Jazor.CompilerTest

Jazor 编译器的单元测试项目，验证 C# 到 JavaScript 代码转换的正确性。

## 项目概述

本测试项目使用 MSTest 框架对 `ECMAScript.Compiler` 项目进行全面测试，确保 C# 代码能够正确转换为语义等价的 JavaScript 代码。

### 测试框架

- **框架**: MSTest.Sdk (v4.0.0-preview)
- **目标框架**: .NET 10.0
- **并行执行**: MethodLevel 级别
- **代码覆盖率**: coverlet.collector

### 项目依赖

| 包 | 版本 | 用途 |
|---|------|------|
| ECMAScript.Compiler | project reference | 被测试的编译器核心 |
| Acornima | 1.2.0 | JavaScript AST 库 |
| Microsoft.CodeAnalysis.CSharp | 5.0.0 | Roslyn C# 编译器 |
| Acornima.Extras | 1.2.0 | Acornima 扩展 |
| Basic.Reference.Assemblies.Net100 | 1.8.4 | .NET 10.0 引用程序集 |

## 测试文件结构

```
Jazor.CompilerTest/
├── AstConverterTests.cs              # 类级别转换测试
├── SemanticWalkerPatternTest.cs      # 模式匹配测试
├── SemanticWalkerLoopTest.cs         # 循环语句测试
├── SemanticWalkerStringTest.cs       # 字符串插值测试
├── SemanticWalkerTryCatchTest.cs     # 异常处理测试
├── SemanticWalkerSwitchTest.cs       # switch 语句测试
├── SemanticWalkerDeclarationTest.cs  # 变量声明测试
├── SemanticWalkerOrdinaryTest.cs     # 普通表达式测试
├── SemanticWalkerReferenceTest.cs    # 引用操作测试
├── SemanticWalkerCreationTest.cs     # 对象/数组创建测试
├── SemanticWalkerTupleTest.cs        # 元组测试
├── SemanticWalkerInvalidTest.cs      # 无效操作回退测试
└── MSTestSettings.cs                 # 测试配置
```

## 测试模块详解

### 1. AstConverterTests.cs

类级别转换器测试，验证 `AstConverter` 将 C# 类转换为 ES6 module 的功能。

| 测试方法 | 验证内容 |
|---------|---------|
| `Convert_SimplePublicClass_ReturnsModule` | 简单 public 类转换 |
| `Convert_NonPublicClass_ThrowsNotSupportedException` | 非 public 类抛出异常 |
| `Convert_ClassWithStaticField_GeneratesVariableDeclaration` | 静态字段生成变量声明 |
| `Convert_ClassWithConstField_GeneratesConstDeclaration` | const 字段生成 const 声明 |
| `Convert_ClassWithPrivateField_DoesNotExport` | private 字段不导出 |
| `Convert_ClassWithMethod_GeneratesFunctionDeclaration` | 静态方法生成函数声明 |
| `Convert_ClassWithProperty_GeneratesPropertyMethods` | 属性生成 getter/setter 方法 |
| `Convert_EmptyClass_ReturnsNull` | 空类返回 null |
| `Convert_ClassWithEnum_GeneratesEnumObject` | 枚举生成对象 |
| `Convert_ClassWithNestedClass_GeneratesClassDeclaration` | 嵌套类处理 |

### 2. SemanticWalkerPatternTest.cs

模式匹配功能测试，涵盖所有 C# 模式类型。

| 模式类型 | C# 示例 | JavaScript 结果 |
|---------|---------|---------------|
| 常量模式 | `obj is 42` | `obj === 42` |
| 类型模式 | `obj is string` | `typeof obj === "string"` |
| 属性模式 | `obj is { Name: "John" }` | `obj.Name === "John"` |
| 关系模式 | `value is > 0` | `value > 0` |
| 递归模式 | `obj is Person("John")` | `obj instanceof Person && obj.Name === "John"` |
| 列表模式 | `list is [1, 2]` | `Array.isArray(list) && list[0] === 1` |
| 切片模式 | `list is [var first, ..]` | 解构赋值 |
| 取反模式 | `obj is not null` | `!(obj === null)` |
| 二元模式 | `value is > 0 and < 100` | `value > 0 && value < 100` |
| 声明模式 | `obj is int value` | 类型检查 + 变量声明 |
| 丢弃模式 | `_` | `true` |

### 3. SemanticWalkerLoopTest.cs

循环语句转换测试。

| 循环类型 | C# 语法 | JavaScript 结果 |
|---------|---------|---------------|
| ForEach | `foreach (var item in items)` | `for (const item of items)` |
| For 循环 | `for (int i = 0; i < 10; i++)` | `for (let i = 0; i < 10; i++)` |
| While 循环 | `while (condition)` | `while (condition)` |
| Do-While | `do { } while (condition)` | `do { } while (condition)` |
| 嵌套循环 | 多层循环嵌套 | 对应嵌套结构 |
| 控制语句 | `break`, `continue`, `return` | 对应语句 |

### 4. SemanticWalkerStringTest.cs

字符串插值测试，验证模板字符串转换。

| C# 插值字符串 | JavaScript 模板字符串 |
|--------------|---------------------|
| `$"Hello {name}!"` | `` `Hello ${name}!` `` |
| `$"{a} + {b} = {a + b}"` | `` `${a} + ${b} = ${a + b}` `` |
| `$"Person: {person.Name}"` | `` `Person: ${person.Name}` `` |

### 5. SemanticWalkerTryCatchTest.cs

异常处理测试。

| 结构 | C# 语法 | JavaScript 结果 |
|------|---------|---------------|
| Try-Catch | `try { } catch (Exception e) { }` | `try { } catch (e) { }` |
| Try-Finally | `try { } finally { }` | `try { } finally { }` |
| 多 Catch | 多个 catch 子句 | if-else 链 |
| Throw | `throw new Exception()` | `throw new Error()` |
| 嵌套 | 嵌套 try-catch | 对应嵌套结构 |

### 6. SemanticWalkerSwitchTest.cs

switch 语句和表达式测试。

| 类型 | C# 语法 | JavaScript 结果 |
|------|---------|---------------|
| 常量 Case | `case 1:` | `case 1:` |
| 模式 Case | `case int v when v > 0:` | IIFE + if-else |
| Switch 表达式 | `x switch { 1 => "one", _ => "other" }` | IIFE + if-else |
| Default | `default:` | `default:` |

### 7. SemanticWalkerDeclarationTest.cs

变量和声明测试。

| 声明类型 | C# 语法 | JavaScript 结果 |
|---------|---------|---------------|
| 变量声明 | `var x = 1;` | `let x = 1;` |
| 数组初始化 | `int[] arr = {1, 2, 3};` | `let arr = [1, 2, 3];` |
| Out 参数 | `int.TryParse(s, out int result)` | 对应转换 |

### 8. SemanticWalkerOrdinaryTest.cs

普通表达式和运算符测试。

| 类别 | 操作符 | C# 示例 |
|------|-------|---------|
| 算术 | `+`, `-`, `*`, `/`, `%` | `a + b` |
| 逻辑 | `&&`, `\|\|`, `!` | `a && b` |
| 比较 | `==`, `!=`, `<`, `>`, `<=`, `>=` | `a == b` |
| 位运算 | `&`, `\|`, `^`, `~`, `<<`, `>>` | `a & b` |
| 赋值 | `=`, `+=`, `-=` 等 | `a += b` |
| 三元 | `? :` | `a ? b : c` |
| Null 合并 | `??`, `??=` | `a ?? b` |
| Lambda | `=>` | `x => x * 2` |
| Await | `await` | `await task` |
| 递增递减 | `++`, `--` | `i++`, `--i` |

### 9. SemanticWalkerReferenceTest.cs

引用操作测试。

| 引用类型 | C# 语法 | JavaScript 结果 |
|---------|---------|---------------|
| 局部变量 | `x` | `x` |
| 参数引用 | `param` | `param` |
| 字段引用 | `obj.Field` | `obj.Field` |
| 静态字段 | `Math.PI` | `Math.PI` |
| 属性引用 | `obj.Name` | `obj.Name` |
| 方法引用 | `obj.Method` | `obj.Method` |
| This | `this` | `this` |
| 数组索引 | `arr[0]` | `arr[0]` |
| 范围索引 | `arr[^1]`, `arr[1..3]` | `arr[arr.length-1]`, `arr.slice(1, 3)` |

### 10. SemanticWalkerCreationTest.cs

对象和数组创建测试。

| 创建类型 | C# 语法 | JavaScript 结果 |
|---------|---------|---------------|
| 构造函数 | `new Person()` | `new Person()` |
| 匿名对象 | `new { Name = "John" }` | `{ Name: "John" }` |
| 数组创建 | `new int[3]` | `new Array(3)` |
| 数组初始化 | `new int[] {1, 2, 3}` | `[1, 2, 3]` |
| 对象初始化 | `new Person { Name = "John" }` | 临时变量 + 属性赋值 |
| 委托创建 | `new Func<int, int>(x => x * 2)` | 箭头函数 |
| 集合初始化 | `new List<int> {1, 2, 3}` | `[1, 2, 3]` |

### 11. SemanticWalkerTupleTest.cs

元组和解构测试。

| 元组操作 | C# 语法 | JavaScript 结果 |
|---------|---------|---------------|
| 元组创建 | `(1, "hello")` | `[1, "hello"]` |
| 命名元组 | `(Name: "John", Age: 30)` | 对象或注释 |
| 元组比较 | `t1 == t2` | 深度比较 |
| 解构 | `var (name, age) = GetPerson()` | 解构赋值 |
| 丢弃 | `var (_, age) = GetPerson()` | 部分解构 |

### 12. SemanticWalkerInvalidTest.cs

无效操作回退测试，验证 `IInvalidOperation` 处理和语法节点回退机制。

## 测试工具方法

### GetBlockOperation

所有测试类共用的辅助方法，用于编译 C# 代码并提取 `IBlockOperation`。

```csharp
private static IBlockOperation GetBlockOperation(string code)
{
    var compilation = CSharpCompilation.Create(
        "TestAssembly",
        [CSharpSyntaxTree.ParseText(code)],
        [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    var syntaxTree = compilation.SyntaxTrees.First();
    var semanticModel = compilation.GetSemanticModel(syntaxTree);
    var methodDeclaration = syntaxTree.GetRoot()
        .DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .First();

    return semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
}
```

### 测试模式

标准测试模式遵循 AAA（Arrange-Act-Assert）：

```csharp
[TestMethod]
public void TestMethod()
{
    // Arrange: 准备 C# 代码和编译环境
    var code = "public static void Test() { /* C# code */ }";
    var block = GetBlockOperation(code);

    // Act: 执行转换
    var walker = new SemanticWalker(true); // true = 测试模式
    var result = walker.Visit(block, new());

    // Assert: 验证结果
    Assert.IsNotNull(result);
    Assert.AreEqual(expected, result.ToECMAScript());
}
```

## 运行测试

### 运行所有测试

```bash
pwsh ./scripts/test-dotnet.ps1
```

### 运行特定测试类

```bash
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"
```

### 运行单个测试方法

```bash
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest.Visit_IsPattern_Constant"
```

## 代码覆盖率

### 生成覆盖率报告

#### 使用 coverlet.runsettings 配置（推荐）

```bash
# 使用配置文件运行测试并生成覆盖率
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --settings src/Jazor.CompilerTest/coverlet.runsettings

# 覆盖率结果将输出到输出目录
# 格式：opencover, cobertura, json, lcov
```

#### 快速覆盖率收集

```bash
# 使用 XPlat Code Coverage 收集器
dotnet test --collect:"XPlat Code Coverage"

# 结果输出到：coverage.opencover.xml
```

### 查看覆盖率报告

#### 使用 ReportGenerator 生成 HTML 报告

```bash
# 安装 ReportGenerator 工具
dotnet tool install -g dotnet-reportgenerator-globaltool

# 生成 HTML 报告
dotnet-reportgenerator \
  -reports:src/Jazor.CompilerTest/TestResults/**/*.coverage.opencover.xml \
  -targetdir:coverage-report \
  -reporttypes:Html

# 在浏览器中打开报告
start coverage-report/index.html  # Windows
open coverage-report/index.html   # macOS
```

### 覆盖率目标

| 指标 | 目标值 | 说明 |
|------|-------|------|
| 行覆盖率 | ≥85% | 代码行覆盖率 |
| 分支覆盖率 | ≥80% | 条件分支覆盖率 |

### 覆盖率配置

覆盖率配置在 `coverlet.runsettings` 文件中定义：

```xml
<CoverageThreshold>
  <LineMinimum>85</LineMinimum>
  <BranchMinimum>80</BranchMinimum>
</CoverageThreshold>
```

要修改覆盖率阈值，编辑 `coverlet.runsettings` 文件中的对应值。

### CI/CD 集成

在 CI/CD 管道中，可以配置覆盖率检查：

```yaml
# GitHub Actions 示例
- name: Run tests with coverage
  run: dotnet test --settings coverlet.runsettings

- name: Check coverage threshold
  run: |
    dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --settings coverlet.runsettings \
      --logger "trx;LogFileName=test-results.trx"
```

## 测试模式说明

### 唯一名称生成

测试模式下，`SemanticWalker` 生成稳定的唯一名称便于测试：

```csharp
// 测试模式: new SemanticWalker(true)
// 生成的临时变量名: v$test

// 生产模式: new SemanticWalker()
// 生成的临时变量名: _a1b2c3d4 (SHA256 哈希)
```

### 并行执行

测试配置为方法级别并行执行，提高测试效率：

```csharp
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
```

## 测试覆盖的 C# 特性

### ✅ 完全支持

- 变量声明和初始化
- 所有算术、逻辑、位运算符
- if/else 条件语句
- for/foreach/while/do-while 循环
- switch 语句和表达式
- 模式匹配（所有类型）
- 字符串插值
- 异常处理（try-catch-finally）
- Lambda 表达式
- async/await
- 数组和对象创建
- 元组和解构
- 局部函数

### ❌ 不支持

- 事件系统（事件订阅/触发）
- 动态类型（`dynamic`）
- LINQ 查询表达式
- unsafe 代码和指针
- `sizeof`/`typeof` 操作符
- using 语句和 lock 语句
- 函数指针

## 贡献指南

添加新测试时：

1. 确定测试所属的功能模块
2. 在对应的测试类中添加测试方法
3. 使用 `GetBlockOperation` 辅助方法编译 C# 代码
4. 使用测试模式 `new SemanticWalker(true)` 生成稳定名称
5. 验证生成的 JavaScript AST 结构和语义
6. 遵循命名约定：`Visit_[Feature]_[Scenario]`

## 许可证

本项目遵循 Jazor 项目的许可证。
