# ECMAScript.Analyzer

ECMAScript.Analyzer 是 Jazor 编译器项目的静态代码分析器，作为白名单机制的第一道防线，在编译时验证用户代码是否只使用了支持转换到 JavaScript 的 C# 类型和成员。

## 项目概述

**定位**：Roslyn 诊断分析器 (DiagnosticAnalyzer)

**目标框架**：.NET Standard 2.0

**核心职责**：
- 检查标记有 `[ECMAScript]` 和 `[ECMAScriptModule]` 特性的类型
- 验证使用的类型是否在白名单中
- 验证调用的成员（方法、属性、字段）是否在白名单中
- 报告不支持的使用情况（事件、析构函数等）

## 项目结构

```
ECMAScript.Analyzer/
├── Analyzer.cs                    # 主分析器类（421 行）
├── WhiteList.cs                   # 白名单定义（354 行，自动生成）
├── Resources.Designer.cs          # 资源文件（自动生成）
├── Resources.resx                 # 资源定义文件
├── AnalyzerReleases.Shipped.md    # 已发布分析器规则记录
├── AnalyzerReleases.Unshipped.md  # 未发布分析器规则记录
└── ECMAScript.Analyzer.csproj     # 项目配置文件
```

## 诊断规则

### 诊断描述符

```csharp
Diagnostic ID: JAZOR001
Title: Jazor
MessageFormat: [{0}] is not support in ECMAScript
Category: Security
Severity: Error
```

### 错误示例

```
JAZOR001: [System.Drawing.Bitmap] is not support in ECMAScript
```

## 白名单机制

### WhiteList.cs 结构

白名单由 `ECMAScript.Compiler.WhiteListGenerator` 源生成器自动生成，包含两个主要部分：

#### 类型白名单

```csharp
public static readonly HashSet<string> Types = new()
{
    "void",
    "System.Nullable",
    "System.ValueTuple",
    "System.Array",
    "System.Numerics.BigInteger",
    "bool",
    "object",
    "string"
};
```

#### 成员白名单

包含 200+ 成员，涵盖：
- **BigInteger**：144+ 项（构造函数、运算符、Parse/TryParse、数学运算等）
- **bool**：16+ 项（Parse、TryParse、ToString、GetHashCode 等）
- **object**：5+ 项（GetType、Equals、ReferenceEquals 等）
- **string**：70+ 项（Concat、Format、IndexOf、Substring、Split 等）

### 白名单生成流程

```
1. 开发者在 ECMAScript.CLR 中定义模块
   └── 使用 [WhiteList] 特性标记类型和成员

2. WhiteListGenerator 扫描特性
   └── 生成 WhiteList.cs 到 Analyzer 项目

3. Analyzer 读取 WhiteList.cs
   └── 在编译时验证用户代码

4. 用户代码通过验证后
   └── Compiler 进行 C# → JavaScript 转换
```

## 分析规则

### 支持的类型（无需白名单）

| 类型 | 说明 |
|------|------|
| 枚举 (`Enum`) | 所有枚举类型 |
| 接口 (`Interface`) | 所有接口类型 |
| 委托 (`Delegate`) | 所有委托类型 |
| 匿名类型 | 编译器生成的匿名类型 |
| 抽象类 | 所有抽象类 |
| 特性类 | 所有特性类 |
| 类型参数 | 泛型类型参数 |
| ECMAScriptAttribute | ECMAScript 相关特性 |

### ES 特性约定

约定"ES 特性"包括：
- `[ECMAScript]` - 标记需要转换为 JavaScript 的类型
- `[ECMAScriptModule]` - 标记模块化类型

**重要规则**：
1. ES 特性只能标记最外层的类、接口、枚举、委托等
2. 不支持嵌套类上的 ES 特性标记
3. 只诊断被 ES 特性标记的类

### 分析的操作类型（17 种）

| 操作类型 | 检查内容 | 示例 |
|---------|---------|------|
| `FieldInitializer` | 字段初始值类型 | `int field = 1;` |
| `PropertyInitializer` | 属性初始值类型 | `int Prop { get; } = 1;` |
| `ParameterInitializer` | 参数默认值类型 | `void M(int param = 1)` |
| `ObjectCreation` | 构造函数白名单 | `new MyClass()` |
| `ArrayCreation` | 数组元素类型 | `new int[10]` |
| `DelegateCreation` | 委托创建 | `new Action(MyMethod)` |
| `Invocation` | 方法白名单 | `obj.MyMethod()` |
| `FieldReference` | 字段白名单 | `obj.myField` |
| `PropertyReference` | 属性白名单 | `obj.MyProperty` |
| `MethodReference` | 方法引用 | `Action a = obj.MyMethod;` |
| `TypeOf` | typeof 操作数类型 | `typeof(MyClass)` |
| `Conversion` | 类型转换 | `(MyType)obj` |
| `ConditionalAccess` | 条件访问类型 | `obj?.Method()` |
| `Await` | await 表达式类型 | `await task` |
| `Using` | using 语句类型 | `using var obj = ...` |
| `EventReference` | 事件引用（不支持） | `obj.MyEvent` |
| `EventAssignment` | 事件赋值（不支持） | `obj.MyEvent += handler` |
| `AnonymousFunction` | Lambda 参数类型 | `(int x) => x` |

### 符号结束分析

在类分析结束时检查：
1. **基类类型**（除 `System.Object` 外）
2. **接口实现**
3. **泛型类型参数约束**
4. **字段类型**
5. **属性类型**
6. **事件**（总是报错）
7. **方法**：
   - 返回类型
   - 参数类型
   - 析构函数（报错）

## 不支持的功能

### 事件系统

- **原因**：C# 多播事件模型与 JavaScript 事件模型根本不同
- **诊断**：`EventReference`、`EventAssignment`

### 析构函数

- **原因**：JavaScript 没有析构函数概念
- **诊断**：`MethodKind.Destructor`

### 嵌套类标记

- **原因**：特性只能标记最外层类型
- **诊断**：嵌套类上的 `[ECMAScript]` 特性

## 代码示例

### 允许的代码

```csharp
[ECMAScript]
public class MyClass
{
    void Method()
    {
        // 白名单中的类型 - 允许
        var str = "Hello";
        var num = System.Numerics.BigInteger.Parse("123");

        // 白名单中的成员 - 允许
        var len = str.Length;
        var result = string.Concat("a", "b");
    }
}
```

### 错误的代码

```csharp
[ECMAScript]
public class MyClass
{
    // JAZOR001: 不支持的类型
    void Method()
    {
        var bmp = new System.Drawing.Bitmap(100, 100);  // 错误
    }

    // JAZOR001: 不支持事件
    public event EventHandler MyEvent;  // 错误

    // JAZOR001: 不支持析构函数
    ~MyClass() { }  // 错误
}
```

## 依赖关系

### 项目引用

```
ECMAScript.Common
    └── Util.NameFormat（符号显示格式工具）
```

### NuGet 包

```
Microsoft.CodeAnalysis.Analyzers (v4.14.0)
    └── Roslyn 分析器基础设施
```

## 工作流程

### 初始化阶段

```
Initialize(AnalysisContext)
    ↓
配置并发执行 + 禁用生成代码分析
    ↓
注册 SymbolStartAction（仅 Class 类型）
    ↓
判断是否有 [ECMAScript] 特性
    ↓
注册 17 种 OperationAction
    ↓
注册 SymbolEndAction
```

### 分析阶段

```
操作分析（17 种操作）
    ↓
类型检查（递归检查数组、元组、泛型）
    ↓
白名单匹配
    ↓
报告诊断（如果失败）
```

## 性能优化

### 并发执行

```csharp
context.EnableConcurrentExecution();  // 启用并发分析
```

### 嵌套类跳过

```csharp
if (hasAttribute && symbol.ContainingType is not null)
{
    // 不诊断嵌套类（性能优化）
    return;
}
```

### 生成代码分析配置

```csharp
context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
```

## 类型检查策略

### 递归类型检查

```csharp
// 数组类型
CheckType(report, arrayType.ElementType, location);

// 元组类型
foreach (var field in namedType.TupleElements)
    CheckType(report, field.Type, location);

// 泛型类型
foreach (var typeArg in namedType.TypeArguments)
    CheckType(report, typeArg, location);
```

### 白名单检查

```csharp
// 获取类型全名
var fullName = typeSymbol.OriginalDefinition.ToDisplayString(Util.NameFormat);

// 检查是否在白名单中
if (WhiteList.Types.Contains(fullName))
    return;  // 允许使用

// 检查是否有 [ECMAScript] 特性
if (!InECMAScriptAttribute(typeSymbol.OriginalDefinition))
    report(Diagnostic.Create(Rule, location, fullName));
```

## API 参考

### 主要方法

| 方法 | 功能 | 参数 |
|------|------|------|
| `Initialize(AnalysisContext)` | 注册分析回调 | Roslyn 分析上下文 |
| `AnalysisOperationAction(OperationAnalysisContext)` | 分析操作 | 操作分析上下文 |
| `AnalysisSymbolEndAction(SymbolAnalysisContext)` | 分析符号结束 | 符号分析上下文 |
| `CheckType(Action<Diagnostic>, ITypeSymbol?, Location)` | 类型检查 | 报告回调、类型符号、位置 |
| `InECMAScriptAttribute(ITypeSymbol)` | 检查特性（向上遍历） | 类型符号 |
| `HasECMAScriptAttribute(ITypeSymbol)` | 检查直接特性 | 类型符号 |
| `IsAttribute(ITypeSymbol)` | 判断特性类型 | 类型符号 |

### 辅助方法

| 方法 | 功能 |
|------|------|
| `GetLocation(ImmutableArray<Location>)` | 获取源码位置 |

## 与其他项目的关系

### 依赖关系图

```
┌─────────────────────────────────────────┐
│         ECMAScript.CLR                  │
│  (运行时实现 + [WhiteList] 特性)        │
└──────────────────┬──────────────────────┘
                   │ 扫描
                   ↓
┌─────────────────────────────────────────┐
│    WhiteListGenerator (源生成器)        │
└──────────────────┬──────────────────────┘
                   │ 生成
                   ↓
┌─────────────────────────────────────────┐
│      ECMAScript.Analyzer                │
│  (读取 WhiteList.cs，验证用户代码)      │
└──────────────────┬──────────────────────┘
                   │ 验证通过
                   ↓
┌─────────────────────────────────────────┐
│      ECMAScript.Compiler                │
│  (C# → JavaScript 转换)                 │
└─────────────────────────────────────────┘
```

### 数据流

```
开发者代码（带 [ECMAScript] 特性）
    ↓
ECMAScript.Analyzer（编译时验证）
    ↓
白名单检查（类型 + 成员）
    ↓
验证通过
    ↓
ECMAScript.Compiler（转换）
    ↓
JavaScript 代码
```

## 构建和测试

### 构建项目

```bash
dotnet build src/ECMAScript.Analyzer
```

### 运行分析器

分析器会自动作为 NuGet 包的一部分被引用，在编译被 `[ECMAScript]` 标记的代码时自动运行。

### 查看诊断

诊断结果会显示在 Visual Studio 的错误列表中，或通过 `dotnet build` 命令的输出显示。

## 设计特点

- ✅ 使用 Roslyn 分析器框架
- ✅ 支持并发分析
- ✅ 递归类型检查（数组、元组、泛型）
- ✅ 详细的中文文档和注释
- ✅ 自动化白名单生成
- ✅ 17 种操作类型覆盖
- ✅ 清晰的错误报告（JAZOR001）

## 代码质量

- **主分析器**：421 行代码
- **白名单定义**：354 行（200+ 成员）
- **XML 文档注释**：完整
- **代码组织**：清晰的单一职责

## 版本历史

参见 `AnalyzerReleases.Shipped.md` 和 `AnalyzerReleases.Unshipped.md`。

---

**维护者**：developerhan
**最后更新**：2026-01-28
**相关项目**：
- [ECMAScript.Compiler](../ECMAScript.Compiler/README.md)
- [ECMAScript.CLR](../ECMAScript.CLR/README.md)
- [CLAUDE.md](../../CLAUDE.md) - Jazor 项目开发规则文档
