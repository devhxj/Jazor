# Jazor 项目开发规则文档

## 1. 项目概述

Jazor 是一个 C# 到 JavaScript 的编译器项目，主要目标是将 C# 代码转换为等价的 JavaScript 代码，支持跨语言的语义保持和 AST 转换。

### 1.1 项目结构

```
Jazor/
├── ECMAScript/                # 核心 ECMAScript 实现
├── ECMAScript.CLR/            # CLR 运行时支持
├── ECMAScript.Analyzer/       # 静态代码分析器
├── ECMAScript.Compiler/       # C# 到 JavaScript 编译器
├── ECMAScript.Server/         # 编译服务器
├── ECMAScript.Test/           # 手动测试控制台
├── ECMAScript.ComplierTest/   # 编译器测试（MSTest）
├── ECMAScript.WebIDL/         # WebIDL绑定生成器（TypeScript）
├── ECMAScript.Common/         # 公共类型和工具
└── ECMASCript.MSBuild/        # MSBuild 集成
```

## 2. 核心转换思想

### 2.1 两层转换架构

Jazor 采用两层转换架构：类级别转换和操作级别转换。

#### 2.1.1 类级别转换：AstConverter

**AstConverter** 负责将整个 C# 类转换为 ES6 module。

```csharp
public class AstConverter(INamedTypeSymbol classSymbol, SemanticModel classModel)
{
    public Module? Convert()
    {
        // 遍历类的所有成员
        foreach (var member in _classSymbol.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol field:
                    members.Add(ConvertModuleField(field));    // 静态字段 → const/let + export
                    break;
                case IPropertySymbol prop:
                    members.AddRange(ConvertModuleProperty(prop)); // 属性 → 字段 + get/set 方法 + export
                    break;
                case IMethodSymbol func:
                    members.Add(ConvertModuleMethod(func));   // 静态方法 → 函数声明 + export
                    break;
                case INamedTypeSymbol @class:
                    members.Add(ConvertModuleClass(@class));  // 嵌套类 → class 声明 + export
                    break;
                case INamedTypeSymbol @enum:
                    members.Add(ConvertModuleEnum(@enum));    // 枚举 → const 对象 + export
                    break;
            }
        }
        return new Module(statements);  // 生成 ES6 module
    }
}
```

**转换规则**：

| C# 成员 | JavaScript 结果 | 导出规则 |
|---------|---------------|---------|
| public 静态字段 | `const/let name = value` | `export` |
| internal 静态字段 | `const/let name = value` | `export` |
| private 静态字段 | `const/let _name = value` | 不导出 |
| public 静态属性 | 字段 + get/set 函数 | `export` |
| public 静态方法 | `function name(...) { ... }` | `export` |
| 嵌套 public 类 | `class ClassName { ... }` | `export` |
| 枚举 | `const EnumName = { ... }` | `export` |

**方法体转换**：使用 `SemanticWalker` 将 IOperation 转换为 JavaScript AST：

```csharp
var walker = new SemanticWalker();
body = walker.Visit(operation, new()) as FunctionBody;
```

#### 2.1.2 操作级别转换：SemanticWalker

**SemanticWalker** 负责将 C# 操作树（IOperation）转换为 JavaScript AST 节点。

```csharp
public sealed partial class SemanticWalker : OperationVisitor<WalkerArgument, Node?>
{
    public override Node? Visit(IOperation? operation, WalkerArgument argument)
    {
        if (operation is null)
            return null;
        _recursionDepth++;
        try
        {
            EnsureSufficientExecutionStack(_recursionDepth);
            return operation.Accept(this, argument);  // 多重分发到具体 Visit 方法
        }
        finally
        {
            _recursionDepth--;
        }
    }
}
```

#### 2.1.3 备用转换路径：SyntaxNode 层面

当编译器优化导致 `IInvalidOperation` 出现时，回退到语法节点层面进行转换。

```csharp
public override Node? VisitInvalid(IInvalidOperation operation, WalkerArgument argument)
    => ConvertFromSyntaxNode(operation.Syntax);

private Node ConvertFromSyntaxNode(SyntaxNode node)
{
    // 基于 C# 语法节点类型进行模式匹配
    var result = node switch
    {
        LiteralExpressionSyntax lit => /* 转换字面量 */,
        IdentifierNameSyntax id => new Identifier(id.Identifier.Text),
        InvocationExpressionSyntax ie => /* 转换方法调用 */,
        BinaryExpressionSyntax be => /* 转换二元运算 */,
        // ... 其他语法节点类型
    };
    return result ?? HandleTransformationFailure(node, $"Unsupported syntax node kind: {node.Kind()}.");
}
```

**原因**：某些表达式在 IOperation 层面可能被编译器优化或折叠，需要回退到语法层面保持原始语义。

### 2.2 白名单机制与 ECMAScript.CLR

#### 2.2.1 ECMAScript.CLR 的作用

**ECMAScript.CLR** 项目使用 C# 编写（但语法贴合 JavaScript）来实现 C# 类型对应的 ES6 module。

**设计目的**：
- 将 C# 类型的属性和方法统一转换成可导出的方法
- 提供类型成员的 JavaScript 运行时实现
- 通过白名单机制与 Analyzer 和 Compiler 协同工作

**实现方式**：

```csharp
[ECMAScriptModule]
[WhiteList("bool")]
public static class BooleanModule
{
    // 使用 ECMAScriptLiteral 直接定义 JavaScript 代码片段
    [WhiteList("override bool.GetHashCode()")]
    [ECMAScriptLiteral("@#{0} ? 1 : 0")]
    public extern static Number BooleanGetHashCode(bool instance);

    // 使用 C# 实现复杂逻辑
    [WhiteList("static bool.Parse(string)")]
    public static bool BooleanParse(string value)
    {
        var str = value.Trim().ToLower();
        if (str == "true")
            return true;
        else if (str == "false")
            return false;
        else
            throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
    }
}
```

**特性**：
- `[ECMAScriptModule]` - 标记为可导出模块
- `[WhiteList("name")]` - 配置白名单映射名称
- `[ECMAScriptLiteral("code")]` - 直接嵌入 JavaScript 代码片段
- `extern` - 声明外部实现（由 ECMAScriptLiteral 提供或 C# 实现）

**支持的模块类型**：
- 基础类型：`BooleanModule`, `CharModule`, `StringModule`, `ObjectModule`
- 数值类型：`SByteModule`, `Int16Module`, `Int32Module`, `Int64Module`, `BigIntegerModule` 等
- 日期时间：`DateTimeModule`, `DateOnlyModule`, `TimeOnlyModule`, `TimeSpanModule`
- 集合类型：`ListModule`, `DictionaryModule`, `HashSetModule`
- 其他：`StringBuilderModule`, `NullableModule`, `ValueTupleModule` 等

#### 2.2.2 白名单机制

白名单是连接 Analyzer、Compiler 和 ECMAScript.CLR 的桥梁。

**白名单结构**：

```csharp
public static class WhiteList
{
    // 允许使用的类型全名
    public static readonly HashSet<string> Types = new HashSet<string>
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

    // 允许调用的成员全名（格式：Namespace.Type.Member(params)）
    public static readonly HashSet<string> Members = new HashSet<string>
    {
        "System.Numerics.BigInteger.BigInteger(int)",
        "System.Numerics.BigInteger.Add(System.Numerics.BigInteger, System.Numerics.BigInteger)",
        "override bool.ToString()",
        "static string.Concat(string, string)",
        // ... 更多成员
    };
}
```

**工作流程**：

```
用户代码编写
       ↓
┌──────────────────┐
│  Analyzer 阶段   │  检查使用的类型和成员是否在白名单中
└────────┬─────────┘
         ↓ (通过白名单验证)
┌──────────────────┐
│ Compiler 阶段    │  根据白名单中的名称反查 ECMAScript.CLR 实现
└────────┬─────────┘
         ↓ (生成对应的 ESTree node)
    JavaScript 代码
```

**白名单生成**：
- 白名单由 `WhiteListGenerator.cs` 自动生成
- 扫描 ECMAScript.CLR 中的 `[WhiteList]` 特性
- 自动同步到 ECMAScript.Analyzer 项目

### 2.3 类型安全的访问器模式

提供多层 `Translate` 方法，确保类型安全和错误处理：

| 方法签名 | 用途 | 失败行为 |
|---------|------|---------|
| `Translate<T>(IOperation, WalkerArgument)` | 强制转换为指定类型 | 抛出异常 |
| `Translate<T>(IOperation?, WalkerArgument, T?)` | 可选转换，允许默认值 | 返回默认值 |
| `Translate<T>(ICollection<T>, IOperation?, WalkerArgument)` | 集合转换，跳过失败项 | 记录错误但继续 |
| `TranslateExpression(IOperation, WalkerArgument)` | 专门转换为 Expression | 抛出异常 |

```csharp
// 强制转换 - 必须成功
var expr = Translate<Expression>(operation.Value, argument);

// 可选转换 - 允许默认值
var expr = Translate<Expression>(operation.Value, argument, null);

// 集合转换 - 跳过失败项
Translate(elements, element, argument, null);
```

### 2.3 可空类型的语义保持

项目使用 .NET 10 和 C# 14（开启可空类型），`string` 与 `string?` 有类型区别。

#### 2.3.1 可空类型检测

```csharp
private static bool IsNullableType(ITypeSymbol? type)
    => type is INamedTypeSymbol namedType
        && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
```

#### 2.3.2 类型检查时的可空处理

```csharp
private Expression CreateTypeMatchExpr(IOperation operation, ITypeSymbol typeSymbol, Expression value, bool? nullable = null)
{
    // 根据类型映射生成基础类型检查
    var mapper = GetMapperType(typeSymbol, out _);
    Expression? result = mapper switch
    {
        TypeMapper.String => TypeOfExpr(value, "string"),
        TypeMapper.Number => TypeOfExpr(value, "number"),
        TypeMapper.Date => InstanceOfExpr(value, "Date"),
        // ...
    };

    // 可空类型额外处理：|| value === null
    if (nullable ?? IsNullableType(typeSymbol))
    {
        var expr = new NonLogicalBinaryExpression(Operator.StrictEquality, value, Null);
        result = result is null ? expr : new LogicalExpression(Operator.LogicalOr, result, expr);
    }

    return result;
}
```

**转换示例**：

| C# 类型检查 | JavaScript 结果 |
|------------|---------------|
| `obj is string` | `typeof obj === "string"` |
| `obj is string?` | `typeof obj === "string" \|\| obj === null` |
| `obj is DateTime` | `obj instanceof Date` |
| `obj is DateTime?` | `obj instanceof Date \|\| obj === null` |

### 2.4 编译时类型信息的利用

利用 C# 强类型系统的编译时信息，直接生成对应语义的 JavaScript 代码。

#### 2.4.1 类型映射系统

```csharp
private static TypeMapper GetMapperType(ITypeSymbol typeSymbol, out string typeName)
{
    switch (typeSymbol.OriginalDefinition.SpecialType)
    {
        case SpecialType.System_String:
            return TypeMapper.String;
        case SpecialType.System_Int32:
        case SpecialType.System_Double:
            return TypeMapper.Number;
        case SpecialType.System_Int64:
            return TypeMapper.BigInt;
        case SpecialType.System_DateTime:
            return TypeMapper.Date;
        // ...
    }
}
```

#### 2.4.2 类型检查到 JavaScript 的映射

| C# 类型 | JavaScript 类型检查 | TypeMapper |
|---------|-------------------|-----------|
| `string` | `typeof x === "string"` | `String` |
| `int`, `double` | `typeof x === "number"` | `Number` |
| `long`, `BigInteger` | `typeof x === "bigint"` | `BigInt` |
| `DateTime` | `x instanceof Date` | `Date` |
| `Array<T>`, `List<T>` | `Array.isArray(x)` | `Array` |
| 自定义 class | `x instanceof ClassName` | `Class` |
| `object` | `typeof x === "object"` | `Object` |

### 2.5 转换模式分类

#### 2.5.1 支持的转换类型

##### 基础语法转换

- 变量声明：`var/int` → `let`
- 赋值操作：`= += -= *= /= %= &= |= ^= <<= >>= >>>=` → JavaScript 相同运算符
- 二元运算：`+ - * / % == != < > <= >= && ||` → JavaScript 相同运算符
- 一元运算：`+ - ! ~ ++ --` → JavaScript 相同运算符

##### 控制流转换

- if/else 语句：直接映射
- switch 语句：
  - 常量 case → JavaScript switch 语句
  - 模式 case → IIFE + if-else 链
- switch 表达式：转换为 IIFE + if-else 链
- 循环语句：for、foreach、while/do-while 的直接映射

##### 模式匹配转换

- 常量模式：`value is 42` → `value === 42`
- 类型模式：`obj is string` → `typeof obj === "string"`
- 属性模式：`obj is { Name: "John" }` → `obj.hasOwnProperty("Name") && obj.Name === "John"`
- 关系模式：`value is > 0` → `value > 0`
- 递归模式：`obj is Person("John", 18)` → `obj instanceof Person && obj.Name === "John" && obj.Age === 18`
- 列表模式：`list is [1, 2, ..]` → `Array.isArray(list) && list.length >= 2 && list[0] === 1`
- 切片模式：`list is [var first, .. var rest]` → 切片表达式
- 取反模式：`obj is not null` → `!(obj === null)`
- 二元模式：`value is > 0 and < 100` → `value > 0 && value < 100`
- 声明模式：`obj is int value` → 类型检查 + 变量声明
- 丢弃模式：`_` → `true`

##### 字符串转换

- 插值字符串：`$"Hello {name}!"` → 模板字符串 `` `Hello ${name}!` ``
- 字符串拼接：自动优化为模板字符串

##### 异步编程

- async/await：直接映射

#### 2.5.2 不支持的特性

##### 事件系统

- `IEventReferenceOperation`：事件引用
- `IRaiseEventOperation`：事件触发
- `IEventAssignmentOperation`：事件赋值（+=/-=）
- 原因：C# 多播事件模型与 JavaScript 事件模型根本不同

##### 动态类型

- `IDynamicObjectCreationOperation`：动态对象创建
- `IDynamicMemberReferenceOperation`：动态成员引用
- `IDynamicInvocationOperation`：动态方法调用
- `IDynamicIndexerAccessOperation`：动态索引器访问
- 原因：C# 动态绑定语义与 JavaScript 静态分派模型不可通约

##### LINQ

- `ITranslatedQueryOperation`：LINQ 查询表达式
- 原因：LINQ 提供延迟执行、表达式树，JavaScript 没有对应构造

##### 编译器内部操作

- `IStopOperation`、`IEndOperation`：编译器内部标记
- `IMethodBodyOperation`、`IConstructorBodyOperation`：方法体操作
- `ICaughtExceptionOperation`：捕获异常操作
- `IStaticLocalInitializationSemaphoreOperation`：静态本地初始化信号量
- `IFlowAnonymousFunctionOperation`、`IFlowCaptureOperation`、`IFlowCaptureReferenceOperation`：数据流分析操作

##### 类型和内存操作

- `ITypeOfOperation`：typeof 操作符（C# 获取类型 vs JavaScript 获取值类型）
- `ISizeOfOperation`：sizeof 操作符
- `IAddressOfOperation`：取地址运算符
- 原因：JavaScript 是安全语言，没有这些底层操作

##### VB.NET 特有功能

- `IForToLoopOperation`：For-To 循环
- `IReDimOperation`、`IReDimClauseOperation`：ReDim 操作
- `IRangeCaseClauseOperation`：范围 case 子句
- `IRelationalCaseClauseOperation`：关系 case 子句

##### 其他不支持的功能

- `IUsingOperation`、`IUsingDeclarationOperation`：using 语句/声明
- `ILockOperation`：lock 语句
- `IInterpolatedStringHandlerCreationOperation`、`IInterpolatedStringAppendOperation`：插值字符串处理器
- `IFunctionPointerInvocationOperation`：函数指针调用
- `IUtf8StringOperation`：UTF-8 字符串
- `IInlineArrayAccessOperation`：内联数组访问
- `IRangeOperation`：独立的范围操作（在数组切片中支持）

### 2.6 AST 节点构造规范

使用 Acornima ESTree 节点类型直接构造 JavaScript AST。

#### 2.6.1 节点类型选择

- 逻辑操作：`LogicalExpression`（&&、||、??）
- 比较操作：`NonLogicalBinaryExpression`（==、!=、<、>等）
- 一元操作：`NonUpdateUnaryExpression`（!、-、typeof）或 `UpdateExpression`（++、--）
- `NullLiteral` 必须提供 raw 参数：`new NullLiteral("null")`
- `BooleanLiteral` 第一个参数为 bool 值，第二个参数为 string 原始值
- `StringLiteral` 必须提供原始值参数：`new StringLiteral("hello", "'hello'")`

#### 2.6.2 字符串处理规范

- 所有字符串字面量需要适当的转义处理
- 插值字符串转换为 `TemplateLiteral`（模板字符串）
- 使用 `CookedToRaw` 方法处理转义字符

### 2.7 GetUniqueName 唯一名称生成

**GetUniqueName** 方法用于生成稳定的唯一变量名称，避免命名冲突。

```csharp
private string GetUniqueName(SyntaxNode node)
{
    var syntaxTree = node.SyntaxTree;
    var sourceSpan = node.GetLocation().SourceSpan;

    // 方便单元测试，生成固定名称
    if (_test)
        return $"v$test";

    // 使用 SHA256 哈希生成稳定名称
    using var sha256 = SHA256.Create();
    var key = $"{syntaxTree.FilePath}${node.Kind()}${sourceSpan.Start}${sourceSpan.End}";
    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
    var sb = new StringBuilder("_");
    for (int i = 0; i < 8; i++)
        sb.Append(hashBytes[i].ToString("x2"));
    return sb.ToString();
}
```

**使用场景**：
- 对象创建时的临时变量 (`new object() { ... }`)
- switch 表达式的输入变量
- try-catch 的异常参数
- 元组解构的临时变量
- 其他需要避免命名冲突的场景

**特点**：
- 基于语法节点位置生成，确保稳定性
- 使用 SHA256 哈希避免冲突
- 测试模式下返回固定名称 `v$test` 便于测试
- 生成的名称以 `_` 开头，避免与用户代码冲突

### 2.8 变量作用域处理

#### 2.8.1 当前实现方式

**WalkerArgument 变量声明机制**：

```csharp
public sealed class WalkerArgument
{
    private readonly Dictionary<string, VariableDeclarator> _declarators = [];

    public void AddVarDeclarator(VariableDeclarator declarator, int depth)
    {
        var name = declarator.Id is Identifier identifier
            ? identifier.Name
            : declarator.Id.ToECMAScript();
        var key = $"{depth}:{name}";  // 使用深度+名称作为键，处理嵌套作用域
        if (!_declarators.ContainsKey(key))
            _declarators.Add(key, declarator);
    }

    public NodeList<VariableDeclarator> FlushVarDeclarator()
    {
        var list = NodeList.From(_declarators.Values);
        _declarators.Clear();
        return list;
    }
}
```

**VisitBlock 中的处理**：

```csharp
public override Node? VisitBlock(IBlockOperation operation, WalkerArgument argument)
{
    var ctx = new WalkerArgument();
    var statements = new List<Statement>();
    foreach (var stmt in operation.Operations)
    {
        var node = Visit(stmt, ctx);

        // 在每个 statement 处理完后，检查是否有变量声明
        // 将变量声明插入到当前 statement 位置
        if (ctx.HasVarDeclarator)
        {
            var declarators = ctx.FlushVarDeclarator();
            var declaration = new VariableDeclaration(VariableDeclarationKind.Let, declarators);
            statements.Add(declaration);
        }

        if (node is Statement statement)
            statements.Add(statement);
        // ...
    }
    return new NestedBlockStatement(NodeList.From(statements));
}
```

#### 2.8.2 使用场景

**out 参数变量声明**：
```csharp
// C# 代码
if (int.TryParse(input, out int result))
{
    Console.WriteLine(result);
}
```

**模式匹配变量声明**：
```csharp
// C# 代码
switch (obj)
{
    case int value when value > 0:
        Console.WriteLine(value);
        break;
}
```

**元组解构临时变量**：
```csharp
// C# 代码
var (name, age) = GetPerson();
```

#### 2.8.3 潜在问题与注意事项

**当前实现的限制**：
1. **变量声明位置分散**：变量声明被插入到各个 statement 之间，而非集中在块开头
2. **与 C# 作用域的差异**：C# 中变量从声明点开始生效，JavaScript 中 let/const 有 TDZ（暂时性死区）
3. **代码可读性**：生成的 JavaScript 代码中，变量声明位置可能与 C# 源代码不一致

**需要关注的场景**：
- 复杂嵌套块中的变量作用域
- 模式匹配中的变量声明与使用顺序
- out 参数在被调用方法之前的引用

**设计权衡**：
- 当前实现优先保证功能正确性
- 变量声明位置优化可作为后续改进方向
- `{depth}:{name}` 键设计确保了嵌套作用域的正确处理

### 2.9 当前设计权衡与优化思路

#### 2.8.1 当前设计权衡

**功能完整优先**：
- 当前版本优先实现完整的 C# 到 JavaScript 转换功能
- 生成的 JavaScript 代码不一定是最优的
- 转译器本身的效率也不是最优的

**模式匹配的实现限制**：
- 模式匹配严重依赖 `ExtractPatternReference` 方法去查找对象
- 需要向上遍历操作树找到模式匹配的输入表达式
- 这种方式增加了复杂度，也限制了单个 Visit 方法的可测试性

#### 2.8.2 后续优化思路

**通过 Argument 传入上下文**：
- 不再通过 `ExtractPatternReference` 查找对象
- 通过 `WalkerArgument` 传入模式匹配的输入对象
- 未传入时使用 `@ctx` 占位符表示上下文

**优化后的优势**：
1. **可测试性**：单个 Visit 方法可以独立测试，无需构造完整的操作树
2. **完整性**：在 Visit 方法内部就能实现完整的转换逻辑
3. **性能**：减少向上遍历操作树的开销
4. **简洁性**：代码逻辑更清晰，减少依赖

**示例改进**：

```csharp
// 当前实现（简化）
public override Node? VisitIsPattern(IIsPatternOperation operation, WalkerArgument argument)
{
    // 需要向上查找被测试的表达式
    var targetExpr = ExtractPatternReference(operation);
    // ...
}

// 优化后实现
public override Node? VisitIsPattern(IIsPatternOperation operation, WalkerArgument argument)
{
    // 直接从 argument 获取或使用占位符
    var targetExpr = argument.TargetExpression ?? new Identifier("@ctx");
    // ...
}
```

## 3. 异常处理策略

### 3.1 异常类型定义

#### 3.1.1 `OperationTransformationException`

- **用途**：当 C# 操作无法转换为等价的 JavaScript AST 时抛出
- **构造参数**：
  - `IOperation? operation`：导致异常的操作
  - `string? message`：详细错误信息
  - `Exception? innerException`：（可选）内部异常

#### 3.1.2 `SymbolTransformationException`

- **用途**：当符号转换失败时抛出
- **构造参数**：
  - `ISymbol symbol`：导致异常的符号
  - `string? message`：详细错误信息
  - `Exception? innerException`：（可选）内部异常

#### 3.1.3 `SyntaxNodeTransformationException`

- **用途**：当语法节点转换失败时抛出（用于 `ConvertFromSyntaxNode`）
- **构造参数**：
  - `SyntaxNode? node`：导致异常的语法节点
  - `string? message`：详细错误信息
  - `Exception? innerException`：（可选）内部异常

### 3.2 异常处理规则

#### 3.2.1 强制抛出异常的场景

1. **语义不等价**：无法保证 1:1 语义映射时
2. **不支持的操作**：遇到明确不支持的 C# 特性
3. **编译器内部操作**：处理编译器专用操作时
4. **动态语义退化**：动态类型相关操作

#### 3.2.2 异常信息规范

- **必须包含**：具体的操作类型和原因
- **必须提供**：替代方案建议（如果有）
- **格式标准**：`"{操作类型} operations are not supported in JavaScript conversion: {具体原因}"`

### 3.3 辅助方法

使用 `HandleTransformationFailure<T>` 方法统一处理转换失败：

```csharp
return HandleTransformationFailure<Node>(operation, "Unsupported operation");
```

## 4. SemanticWalker 文件组织

`SemanticWalker` 采用分文件组织，每个文件负责特定类型的操作转换：

| 文件 | 功能 |
|------|------|
| `SemanticWalker.cs` | 主文件，包含类型映射 (`TypeMapper`)、入口方法、`ConvertFromSyntaxNode`、`Translate` 访问器 |
| `SemanticWalker.cs.Declaration.cs` | 变量声明、局部函数 |
| `SemanticWalker.cs.Ordinary.cs` | 二元/一元运算、条件表达式 |
| `SemanticWalker.cs.Reference.cs` | 字段、属性、方法引用、数组索引 |
| `SemanticWalker.cs.Loop.cs` | for/foreach/while/do-while 循环 |
| `SemanticWalker.cs.Switch.cs` | switch 语句和表达式 |
| `SemanticWalker.cs.Pattern.cs` | 所有模式匹配（常量、类型、属性、关系、递归、列表、切片等） |
| `SemanticWalker.cs.String.cs` | 字符串插值（模板字符串） |
| `SemanticWalker.cs.TryCatch.cs` | 异常处理 |
| `SemanticWalker.cs.Creation.cs` | 对象/数组创建 |
| `SemanticWalker.cs.Tuple.cs` | 元组和解构 |
| `SemanticWalker.cs.Invalid.cs` | `IInvalidOperation` 处理（语法节点回退） |
| `SemanticWalker.cs.NotSupport.cs` | 所有不支持的操作（抛出异常） |

## 5. 性能优化策略

### 5.1 编译时优化

- **利用强类型信息**：使用 C# 编译时类型信息避免运行时检测
- **递归深度控制**：防止栈溢出，使用 `EnsureSufficientExecutionStack`
- **AST 节点复用**：优先复用现有的 Visit 方法

### 5.2 运行时优化

- **最简 AST 生成**：生成最简洁的 JavaScript 代码
- **避免不必要的包装**：除非必要，避免复杂的 IIFE 包装
- **类型转换优化**：依赖编译时类型安全进行优化

## 6. 代码注释规范

### 6.1 方法级别注释要求

#### 6.1.1 标准格式

```csharp
/// <summary>
/// 处理 {操作类型} 操作
/// C# 示例：
/// {C# 代码示例}
/// 转换结果：{JavaScript 结果}
/// </summary>
/// <param name="operation">当前访问的operation</param>
/// <param name="argument">用于存放当前operation内部需要的全局变量定义</param>
/// <returns>Acornima的ESTree的Node</returns>
```

## 7. 测试和验证规范

### 7.1 单元测试要求

- **覆盖率要求**：每个转换方法必须有对应的单元测试
- **测试场景**：包括正常转换和异常情况
- **验证内容**：AST 结构正确性和语义等价性

### 7.2 测试文件组织

测试项目使用 MSTest 框架，按功能模块组织：

- `SemanticWalkerPatternTest.cs` - 模式匹配测试
- `SemanticWalkerLoopTest.cs` - 循环语句测试
- `SemanticWalkerStringTest.cs` - 字符串插值测试
- `SemanticWalkerTryCatchTest.cs` - 异常处理测试
- `SemanticWalkerSwitchTest.cs` - switch 测试
- `SemanticWalkerDeclarationTest.cs` - 声明测试
- `SemanticWalkerOrdinaryTest.cs` - 普通运算测试
- `SemanticWalkerReferenceTest.cs` - 引用测试
- `SemanticWalkerCreationTest.cs` - 创建表达式测试
- `SemanticWalkerTupleTest.cs` - 元组测试
- `SemanticWalkerInvalidTest.cs` - 无效操作测试

## 8. 类型映射 (TypeMapper)

| C# 类型 | JavaScript 类型 | TypeMapper 枚举 |
|---------|-----------------|-----------------|
| `object` | `object` | `Object` |
| `string` | `string` | `String` |
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `decimal`, `double`, `float` | `Number` | `Number` |
| `long`, `ulong`, `Int128`, `UInt128`, `TimeSpan`, `BigInteger` | `BigInt` | `BigInt` |
| `DateOnly`, `TimeOnly`, `DateTime`, `DateTimeOffset` | `Date` | `Date` |
| `bool` | `boolean` | `Boolean` |
| `char` | `string` | `String` |
| `Array<T>`, `List<T>`, `IList<T>`, `IEnumerable<T>` | `Array` | `Array` |
| `Dictionary<K,V>`, `IDictionary<K,V>` | `Map` | `Map` |
| `HashSet<T>`, `ISet<T>` | `Set` | `Set` |
| 自定义 class/struct | `class` | `Class` |
| 其他 | - | `Unknown` |

## 9. 附录

### 9.1 相关文件清单

#### 9.1.1 转换器核心

| 文件 | 功能 |
|------|------|
| `AstConverter.cs` | 类级别转换器，将 C# 类转换为 ES6 module |
| `SemanticWalker.cs` | 操作级别转换器，将 IOperation 转换为 JavaScript AST（分文件组织） |

#### 9.1.2 转换器分文件（SemanticWalker）

| 文件 | 功能 |
|------|------|
| `SemanticWalker.cs` | 主文件，类型映射、Translate 访问器、ConvertFromSyntaxNode |
| `SemanticWalker.cs.Declaration.cs` | 变量声明、局部函数 |
| `SemanticWalker.cs.Ordinary.cs` | 二元/一元运算、条件表达式 |
| `SemanticWalker.cs.Reference.cs` | 字段、属性、方法引用、数组索引 |
| `SemanticWalker.cs.Loop.cs` | for/foreach/while/do-while 循环 |
| `SemanticWalker.cs.Switch.cs` | switch 语句和表达式 |
| `SemanticWalker.cs.Pattern.cs` | 所有模式匹配 |
| `SemanticWalker.cs.String.cs` | 字符串插值（模板字符串） |
| `SemanticWalker.cs.TryCatch.cs` | 异常处理 |
| `SemanticWalker.cs.Creation.cs` | 对象/数组创建 |
| `SemanticWalker.cs.Tuple.cs` | 元组和解构 |
| `SemanticWalker.cs.Invalid.cs` | IInvalidOperation 处理（语法节点回退） |
| `SemanticWalker.cs.NotSupport.cs` | 所有不支持的操作（抛出异常） |

#### 9.1.3 辅助文件

| 文件 | 功能 |
|------|------|
| `AstTransformationException.cs` | 异常类型定义 |
| `WalkerArgument.cs` | 转换上下文参数 |
| `StatementGroup.cs` | 语句分组工具，包含 AstToECMAScriptConverter 扩展 |
| `AstConverter.cs` | 完整的类转换器 |
| `ESGenerator.cs` | 增量源生成器 |

### 9.2 技术依赖

- Microsoft.CodeAnalysis（Roslyn）
- Acornima（JavaScript AST 库）
- .NET 10.0 运行时环境

### 9.3 构建和测试命令

```bash
# 构建整个解决方案
dotnet build

# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test src/ECMAScript.ComplierTest

# 运行单个测试类
dotnet test --filter "SemanticWalkerPatternTest"

# 运行单个测试方法
dotnet test --filter "SemanticWalkerPatternTest.Visit_IsPattern_Constant"
```

---

**文档维护者**：developerhan
**最后更新**：2026-01-23
**文档版本**：v3.0
