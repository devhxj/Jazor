# SemanticWalker 重构计划：引入 Sense 语义上下文

## 1. 需求摘要

重构 SemanticWalker 的 `VisitXX` 方法签名，将 `WalkerArgument` 参数改为 `SenseArgument` 结构体，通过显式传递语义上下文（Sense 枚举 + 扩展属性）替代当前的 `operation.Parent` 向上遍历模式。

### 1.1 目标

1. **提高可测试性**：单个 Visit 方法可独立测试，无需构造完整操作树
2. **提高性能**：消除向上遍历操作树的开销
3. **提高代码清晰度**：意图显式化，减少隐式上下文查找
4. **支持更复杂转换**：通过扩展 Sense 枚举和 SenseArgument 属性支持更复杂场景

### 1.2 当前问题

| 问题 | 当前实现 | 影响 |
|------|---------|------|
| 模式匹配需要向上遍历 | `ExtractPatternReference` 遍历父节点链 | 性能差、复杂度高 |
| 左值/右值判断依赖父节点 | `operation.Parent is ISomeOperation` | 可测试性差 |
| Block 输出类型依赖父节点 | 检查 `operation.Parent` 类型 | 逻辑分散 |
| WalkerArgument.Context 未使用 | 定义了但从未调用 `With()` | 资源浪费 |

---

## 2. 设计方案

### 2.1 Sense 枚举定义

基于代码分析和评审反馈，定义以下 Sense 值（按功能域分组）：

**✅ 已实施**: Sense.cs 已更新，包含完整的枚举定义和详细文档。

```csharp
public enum Sense
{
    // ===== 通用 =====
    /// <summary>不限制，默认值</summary>
    Any,

    // ===== 赋值上下文 =====
    /// <summary>左值上下文（赋值目标）</summary>
    LeftValue,
    /// <summary>右值上下文（赋值源）</summary>
    RightValue,
    /// <summary>属性赋值上下文（对象初始化器中）</summary>
    PropertyAssignment,
    /// <summary>解构赋值上下文</summary>
    Deconstruction,

    // ===== Block 上下文 =====
    /// <summary>函数体上下文（方法、Lambda、局部函数、构造函数）</summary>
    FunctionBody,
    /// <summary>静态初始化块上下文</summary>
    StaticBlock,
    /// <summary>嵌套块上下文</summary>
    NestedBlock,
    /// <summary>Catch 处理器上下文</summary>
    CatchHandler,

    // ===== 模式匹配上下文 =====
    /// <summary>模式匹配输入表达式</summary>
    PatternInput,
    /// <summary>Switch case 模式上下文</summary>
    PatternCase,
    /// <summary>Switch expression arm 上下文</summary>
    SwitchExpressionArm,
    /// <summary>属性子模式上下文</summary>
    PropertySubpattern,
    /// <summary>模式表达式上下文（需要作为独立表达式返回，不需要 SequenceExpression 包装）</summary>
    PatternExpression,

    // ===== 引用上下文 =====
    /// <summary>属性读取</summary>
    PropertyRead,
    /// <summary>属性写入</summary>
    PropertyWrite,
    /// <summary>包含类型实例（this）</summary>
    ContainingTypeInstance,
    /// <summary>隐式接收者</summary>
    ImplicitReceiver,

    // ===== 创建上下文 =====
    /// <summary>对象初始化器上下文</summary>
    ObjectInitializer,
    /// <summary>集合初始化器上下文</summary>
    CollectionInitializer,

    // ===== 异常上下文 =====
    /// <summary>抛出新异常</summary>
    ThrowNew,
    /// <summary>重新抛出异常</summary>
    Rethrow,

    // ===== 声明上下文 =====
    /// <summary>Out 参数声明</summary>
    OutParameter,
    /// <summary>变量声明</summary>
    VariableDeclaration,
    /// <summary>方法参数上下文（用于判断是否需要添加变量声明）</summary>
    Argument,

    // ===== 丢弃上下文 =====
    /// <summary>丢弃赋值</summary>
    DiscardAssignment,
    /// <summary>默认值</summary>
    DefaultValue,
}
```

### 2.2 数据流分析与设计决策

基于代码分析，WalkerArgument 当前的数据流模式：

| 模式 | 当前实现 | 目的 |
|------|---------|------|
| **向下传递** | Visit 调用时传入 | 子节点可添加变量声明/导入 |
| **作用域隔离** | VisitBlock 创建新实例 | 每个块有独立的变量声明作用域 |
| **共享累积** | With 方法共享字典引用 | 导入在模块级别累积 |

**设计决策**：

1. **WalkerArgument 保持为 class** - 引用语义允许共享累积
2. **SenseArgument.Depend 可为 null** - 但提供 `DependOrNew` 属性确保使用时有值
3. **移除 Parent 属性** - 通过 Sense 和其他扩展属性已能表达所需语义
4. **保留必要的扩展属性** - PatternInput、CatchExceptionVar、SwitchExpressionVar

### 2.3 新参数结构

```csharp
/// <summary>
/// 语义上下文参数，传递给 Visit 方法。
/// 作为值类型，通过 with 语法创建新实例传递不同的语义上下文。
/// </summary>
/// <param name="Sense">语义场景标识，决定 Visit 方法的处理方式</param>
/// <param name="Depend">依赖项（引用类型），用于变量声明和导入收集，可为 null</param>
/// <param name="PatternInput">模式匹配输入表达式，用于 is pattern / switch pattern 等场景</param>
/// <param name="CatchExceptionVar">Catch 子句异常参数名，用于 re-throw 场景</param>
/// <param name="SwitchExpressionVar">Switch 表达式输入变量名，用于 switch expression 编译为 IIFE</param>
public readonly record struct SenseArgument(
    Sense Sense = Sense.Any,
    WalkerArgument? Depend = null,
    Expression? PatternInput = null,
    string? CatchExceptionVar = null,
    string? SwitchExpressionVar = null)
{
    /// <summary>默认参数（Depend 为 null，首次使用时创建）</summary>
    public static readonly SenseArgument Default = new();

    // ===== 核心：获取 Depend（确保非 null）=====
    /// <summary>
    /// 获取 Depend，如果为 null 则创建新实例。
    /// 这是访问 Depend 的推荐方式。
    /// </summary>
    public WalkerArgument DependOrNew => Depend ?? new WalkerArgument();

    // ===== Sense 变更 =====
    /// <summary>创建新实例，设置 Sense</summary>
    public SenseArgument With(Sense sense) => this with { Sense = sense };

    // ===== Depend 变更（作用域隔离）=====
    /// <summary>
    /// 创建新实例，使用新的 Depend（用于块级作用域隔离）。
    /// 新的 WalkerArgument 会共享导入字典（如果需要）。
    /// </summary>
    public SenseArgument WithNewScope()
    {
        // 共享导入，新的变量声明字典
        var newDepend = Depend is not null
            ? Depend.WithNewDeclarators()
            : new WalkerArgument();
        return this with { Depend = newDepend };
    }

    // ===== 模式匹配上下文 =====
    /// <summary>设置模式匹配输入表达式</summary>
    public SenseArgument WithPatternInput(Expression? input) => this with { PatternInput = input };

    // ===== 异常处理上下文 =====
    /// <summary>设置 Catch 异常参数名</summary>
    public SenseArgument WithCatchVar(string? varName) => this with { CatchExceptionVar = varName };

    // ===== Switch 表达式上下文 =====
    /// <summary>设置 Switch 表达式变量名</summary>
    public SenseArgument WithSwitchVar(string? varName) => this with { SwitchExpressionVar = varName };

    // ===== 组合设置 =====
    /// <summary>设置 Sense 和 PatternInput</summary>
    public SenseArgument With(Sense sense, Expression patternInput)
        => this with { Sense = sense, PatternInput = patternInput };
}
```

### 2.4 WalkerArgument 职责界定与扩展

**WalkerArgument 保持简单**，主要职责：
- 变量声明收集（`_declarators`）
- 导入说明符收集（`_specifiers`）

**需要添加一个方法**用于作用域隔离：

```csharp
public sealed class WalkerArgument
{
    private readonly Dictionary<string, List<ImportDeclarationSpecifier>> _specifiers = [];
    private readonly Dictionary<string, VariableDeclarator> _declarators;

    // 现有成员保持不变...

    /// <summary>
    /// 创建新实例，共享导入字典，但使用新的变量声明字典。
    /// 用于块级作用域隔离。
    /// </summary>
    public WalkerArgument WithNewDeclarators()
        => new(_specifiers, new Dictionary<string, VariableDeclarator>());

    // 移除未使用的 Context 属性
}
```

### 2.5 为什么移除 Parent 属性

经过分析，Parent 属性是**冗余的**：

| 场景 | 原设计 | 优化后 |
|------|--------|--------|
| 模式匹配 | `Parent ?? PatternInput` | 直接使用 `PatternInput` |
| 赋值左值/右值 | 需要 Parent 判断 | 使用 `Sense.LeftValue/RightValue` |
| 对象初始化器 | 需要 Parent 判断 | 使用 `Sense.ObjectInitializer` |
| Block 输出类型 | 需要 Parent 判断 | 使用 `Sense.FunctionBody/StaticBlock` |

**结论**：通过 Sense 枚举 + 专用扩展属性（PatternInput 等），可以完全替代 Parent 的作用，且语义更清晰。

---

## 3. 实施步骤

### 3.1 第一阶段：基础结构（估计 2-3 小时）

#### Step 1.1: 更新 Sense.cs 枚举定义

**文件**: `Sense.cs`

- 扩展枚举，添加所有定义的 Sense 值
- 添加 XML 注释说明每个值的用途

#### Step 1.2: 创建 SenseArgument 结构体

**文件**: `SenseArgument.cs`（新建）

- 定义 `readonly record struct SenseArgument`
- 实现便捷方法 `With(...)` 系列

#### Step 1.3: 清理 WalkerArgument（可选）

**文件**: `WalkerArgument.cs`

- 移除未使用的 `Context` 属性（或标记为过时）
- 保持 WalkerArgument 只负责变量声明和导入收集

### 3.2 第二阶段：核心转换（估计 4-6 小时）

#### Step 2.1: 更新基类签名

**文件**: `SemanticWalker.cs`

```csharp
// 修改前
public sealed partial class SemanticWalker : OperationVisitor<WalkerArgument, Node?>

// 修改后
public sealed partial class SemanticWalker : OperationVisitor<SenseArgument, Node?>
```

更新 `Translate<T>` 系列方法签名。

#### Step 2.2: 更新入口点

**文件**: `SemanticWalker.cs`

```csharp
// 公共入口
public FunctionBody? Visit(IOperation? operation)
{
    var argument = SenseArgument.Default;
    return Visit(operation, argument) as FunctionBody;
}
```

### 3.3 第三阶段：分文件迁移（估计 8-12 小时）

按优先级迁移各分文件：

| 优先级 | 文件 | 关键 Sense | 复杂度 |
|--------|------|-----------|--------|
| 1 | `SemanticWalker.cs.Pattern.cs` | PatternInput, PatternCase, SwitchExpressionArm | 高 |
| 2 | `SemanticWalker.cs.Ordinary.cs` | LeftValue, RightValue, PropertyAssignment | 中 |
| 3 | `SemanticWalker.cs.Declaration.cs` | OutParameter, VariableDeclaration | 中 |
| 4 | `SemanticWalker.cs.Reference.cs` | PropertyRead, PropertyWrite | 中 |
| 5 | `SemanticWalker.cs.Creation.cs` | ObjectInitializer, CollectionInitializer | 低 |
| 6 | `SemanticWalker.cs.TryCatch.cs` | ThrowNew, Rethrow, CatchHandler | 中 |
| 7 | `SemanticWalker.cs.Switch.cs` | PatternCase, SwitchExpressionArm | 高 |
| 8 | `SemanticWalker.cs.Tuple.cs` | Deconstruction | 中 |
| 9 | `SemanticWalker.cs.Loop.cs` | - | 低 |
| 10 | `SemanticWalker.cs.String.cs` | - | 低 |
| 11 | `SemanticWalker.cs.Invalid.cs` | - | 低 |
| 12 | `SemanticWalker.cs.NotSupport.cs` | - | 低 |

#### Step 3.1: 模式匹配迁移（最关键）

**文件**: `SemanticWalker.cs.Pattern.cs`

**关键改动**:

```csharp
// 修改前
public override Node? VisitIsPattern(IIsPatternOperation operation, WalkerArgument argument)
{
    var targetExpr = ExtractPatternReference(operation);  // 向上遍历
    // ...
}

// 修改后
public override Node? VisitIsPattern(IIsPatternOperation operation, SenseArgument argument)
{
    // 直接从参数获取模式匹配输入
    var targetExpr = argument.PatternInput
        ?? throw new InvalidOperationException("PatternInput is required for pattern matching");
    // ...
}
```

**调用点更新**:

```csharp
// VisitSwitchExpression 中
var inputVar = GetUniqueName(operation.Syntax);
var inputExpr = new Identifier(inputVar);
var patternArg = argument.WithPatternInput(inputExpr);
foreach (var arm in operation.Arms)
{
    var armNode = Visit(arm, patternArg);
    // ...
}
```

#### Step 3.2: 赋值表达式迁移

**文件**: `SemanticWalker.cs.Ordinary.cs`

**VisitSimpleAssignment 改动**:

```csharp
public override Node? VisitSimpleAssignment(ISimpleAssignmentOperation operation, SenseArgument argument)
{
    // 判断上下文
    var isObjectInitializer = argument.Sense == Sense.ObjectInitializer;

    var targetArg = argument.With(Sense.LeftValue);
    var target = Translate<Expression>(operation.Target, targetArg);

    var valueArg = argument.With(Sense.RightValue);
    var value = Translate<Expression>(operation.Value, valueArg);

    if (isObjectInitializer)
        return new ObjectProperty((Identifier)target, value);

    return new AssignmentExpression(Operator.Assign, target, value);
}
```

#### Step 3.3: Block 迁移

**文件**: `SemanticWalker.cs.Ordinary.cs`

**VisitBlock 改动**:

```csharp
public override Node? VisitBlock(IBlockOperation operation, SenseArgument argument)
{
    // 直接使用 Sense 判断输出类型
    return argument.Sense switch
    {
        Sense.FunctionBody => BuildFunctionBody(operation, argument),
        Sense.StaticBlock => BuildStaticBlock(operation, argument),
        Sense.CatchHandler => BuildCatchHandler(operation, argument),
        _ => BuildNestedBlock(operation, argument)
    };
}
```

### 3.4 第四阶段：测试更新（估计 4-6 小时）

#### Step 4.1: 更新现有测试

更新所有测试文件，使用新的 `SenseArgument.Default`:

```csharp
// 修改前
var result = walker.Visit(operation, new WalkerArgument());

// 修改后
var result = walker.Visit(operation, SenseArgument.Default);

// 或使用 DependOrNew 确保有 Depend
var depend = argument.DependOrNew;
```

#### Step 4.2: 新增 Sense 场景测试

为每个 Sense 值添加专门的测试用例：

| 测试文件 | 测试场景 |
|---------|---------|
| `SemanticWalkerPatternTest.cs` | PatternInput, PatternCase, SwitchExpressionArm |
| `SemanticWalkerOrdinaryTest.cs` | LeftValue, RightValue, PropertyAssignment |
| `SemanticWalkerDeclarationTest.cs` | OutParameter, VariableDeclaration |
| `SemanticWalkerTryCatchTest.cs` | ThrowNew, Rethrow, CatchHandler |

---

## 4. 接受标准

### 4.1 功能完整性

- [ ] 所有现有测试通过
- [ ] Sense 枚举覆盖所有识别的语义场景
- [ ] SenseArgument 结构体提供便捷 API
  - [ ] `DependOrNew` 属性确保 Depend 非空
  - [ ] `WithNewScope()` 方法用于块级作用域隔离
  - [ ] `WithPatternInput()` 等扩展方法
- [ ] WalkerArgument 添加 `WithNewDeclarators()` 方法
- [ ] 移除 WalkerArgument 中未使用的 `Context` 属性

### 4.2 代码质量

- [ ] 移除所有 `ExtractPatternReference` 调用（改为参数传递）
- [ ] 移除所有 `operation.Parent is ...` 模式匹配（改为 Sense 判断）
- [ ] 每个 Visit 方法有对应的 XML 注释

### 4.3 性能指标

- [ ] 模式匹配场景无向上遍历
- [ ] 编译测试项目性能不退化

### 4.4 可测试性

- [ ] 单个 Visit 方法可独立调用测试
- [ ] 测试无需构造完整操作树

---

## 5. 风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| 大规模签名变更导致编译错误 | 高 | 分阶段迁移，保持中间兼容 |
| 遗漏某些 Parent 检查场景 | 中 | 全局搜索 `operation.Parent` 确保覆盖 |
| Sense 值设计不完整 | 中 | 预留扩展空间，添加注释说明 |
| 性能意外下降 | 低 | 基准测试对比 |

---

## 6. 验证步骤

1. **编译验证**: `dotnet build` 无错误
2. **测试验证**: `dotnet test` 全部通过
3. **覆盖率验证**: 新代码有对应测试
4. **代码审查**: 检查是否移除了所有 Parent 遍历模式

---

## 7. 文件变更清单

| 文件 | 操作 | 说明 |
|------|------|------|
| `Sense.cs` | 修改 | 扩展枚举定义（23 个值） |
| `SenseArgument.cs` | 新建 | 参数结构体，包含 `DependOrNew`、`WithNewScope()` 等 |
| `WalkerArgument.cs` | 修改 | 添加 `WithNewDeclarators()` 方法，移除 `Context` 属性 |
| `SemanticWalker.cs` | 修改 | 基类签名改为 `OperationVisitor<SenseArgument, Node?>`，更新 Translate 方法 |
| `SemanticWalker.cs.Pattern.cs` | 修改 | 模式匹配迁移，使用 `PatternInput` 替代 `ExtractPatternReference` |
| `SemanticWalker.cs.Ordinary.cs` | 修改 | 赋值、Block 迁移，使用 Sense 判断上下文 |
| `SemanticWalker.cs.Declaration.cs` | 修改 | 声明迁移 |
| `SemanticWalker.cs.Reference.cs` | 修改 | 引用迁移，使用 `Sense.PropertyRead/Write` |
| `SemanticWalker.cs.Creation.cs` | 修改 | 创建迁移，使用 `Sense.ObjectInitializer` |
| `SemanticWalker.cs.TryCatch.cs` | 修改 | 异常迁移，使用 `CatchExceptionVar` |
| `SemanticWalker.cs.Switch.cs` | 修改 | Switch 迁移，使用 `PatternInput`、`SwitchExpressionVar` |
| `SemanticWalker.cs.Tuple.cs` | 修改 | 元组迁移，使用 `Sense.Deconstruction` |
| `SemanticWalker.cs.Loop.cs` | 修改 | 循环迁移 |
| `SemanticWalker.cs.String.cs` | 修改 | 字符串迁移 |
| `SemanticWalker.cs.Invalid.cs` | 修改 | 无效操作迁移 |
| `SemanticWalker.cs.NotSupport.cs` | 修改 | 不支持操作迁移 |
| `*Test.cs` | 修改 | 更新测试参数为 `SenseArgument.Default` |

---

**计划创建日期**: 2026-03-06
**预计总工时**: 18-27 小时
**最后更新**: 2026-03-06（评审改进已实施）

---

## 8. 评审改进实施状态

### 8.1 已完成的改进 ✅

| 改进项 | 状态 | 说明 |
|--------|------|------|
| 补充遗漏的 Sense 枚举值 | ✅ 完成 | 添加了 `PatternExpression` 和 `Argument` |
| 删除 WalkerArgument.Context | ✅ 完成 | 已删除未使用的 Context 属性和 With() 方法 |
| 添加 WithNewDeclarators() 方法 | ✅ 完成 | 用于块级作用域隔离 |
| 创建 SenseArgument 结构体 | ✅ 完成 | 包含所有便捷方法和 DependOrNew 属性 |
| 添加 Sense 使用文档 | ✅ 完成 | 在 Sense.cs 中添加了详细的 XML 注释 |
| 更新重构方案文档 | ✅ 完成 | 整合评审反馈 |

### 8.2 待实施的改进 📋

| 改进项 | 优先级 | 说明 |
|--------|--------|------|
| 添加测试用例清单 | 高 | 为每个 Sense 值提供最小可验证测试 |
| 添加性能基准测试 | 中 | 验证性能提升目标（20-30%） |
| 添加 Sense 验证机制 | 低 | 调试模式下验证 Sense 使用正确性 |
| 添加迁移检查清单 | 中 | 确保迁移过程不遗漏 |

### 8.3 测试用例清单（待补充）

#### 8.3.1 赋值上下文测试

```csharp
[TestMethod]
public void VisitSimpleAssignment_WithLeftValueSense_GeneratesCorrectTarget()
{
    // Arrange
    var code = "x = 5;";
    var operation = GetOperation(code);
    var argument = SenseArgument.Default.With(Sense.LeftValue);

    // Act
    var result = walker.Visit(operation.Target, argument);

    // Assert
    Assert.IsInstanceOfType(result, typeof(Identifier));
    Assert.AreEqual("x", ((Identifier)result).Name);
}

[TestMethod]
public void VisitSimpleAssignment_WithRightValueSense_GeneratesCorrectValue()
{
    // Arrange
    var code = "x = 5;";
    var operation = GetOperation(code);
    var argument = SenseArgument.Default.With(Sense.RightValue);

    // Act
    var result = walker.Visit(operation.Value, argument);

    // Assert
    Assert.IsInstanceOfType(result, typeof(Literal));
}
```

#### 8.3.2 Block 上下文测试

```csharp
[TestMethod]
public void VisitBlock_WithFunctionBodySense_ReturnsFunctionBody()
{
    // Arrange
    var code = "void Method() { int x = 5; }";
    var operation = GetBlockOperation(code);
    var argument = SenseArgument.Default.With(Sense.FunctionBody);

    // Act
    var result = walker.Visit(operation, argument);

    // Assert
    Assert.IsInstanceOfType(result, typeof(FunctionBody));
}

[TestMethod]
public void VisitBlock_WithNestedBlockSense_ReturnsNestedBlockStatement()
{
    // Arrange
    var code = "{ int x = 5; }";
    var operation = GetBlockOperation(code);
    var argument = SenseArgument.Default.With(Sense.NestedBlock);

    // Act
    var result = walker.Visit(operation, argument);

    // Assert
    Assert.IsInstanceOfType(result, typeof(NestedBlockStatement));
}
```

#### 8.3.3 模式匹配上下文测试

```csharp
[TestMethod]
public void VisitIsPattern_WithPatternInput_UsesProvidedExpression()
{
    // Arrange
    var code = "obj is int x";
    var operation = GetIsPatternOperation(code);
    var inputExpr = new Identifier("obj");
    var argument = SenseArgument.Default.WithPatternInput(inputExpr);

    // Act
    var result = walker.Visit(operation.Pattern, argument);

    // Assert
    // 验证生成的代码使用了 inputExpr 而不是向上遍历
    Assert.IsNotNull(result);
}

[TestMethod]
public void VisitConstantPattern_WithPatternExpressionSense_ReturnsExpression()
{
    // Arrange
    var code = "obj is 42";
    var operation = GetConstantPatternOperation(code);
    var argument = SenseArgument.Default.With(Sense.PatternExpression);

    // Act
    var result = walker.Visit(operation, argument);

    // Assert
    // 验证返回的是表达式，不是 SequenceExpression 包装
    Assert.IsInstanceOfType(result, typeof(Expression));
    Assert.IsNotInstanceOfType(result, typeof(SequenceExpression));
}
```

#### 8.3.4 参数上下文测试

```csharp
[TestMethod]
public void VisitDeclarationExpression_WithArgumentSense_AddsVariableDeclarator()
{
    // Arrange
    var code = "TryParse(input, out int result)";
    var operation = GetDeclarationExpressionOperation(code);
    var argument = SenseArgument.Default.With(Sense.Argument);

    // Act
    var result = walker.Visit(operation, argument);

    // Assert
    // 验证变量声明被添加到 Depend
    Assert.IsTrue(argument.DependOrNew.HasVarDeclarator);
}
```

### 8.4 性能基准测试计划（待实施）

```csharp
[TestClass]
public class SemanticWalkerPerformanceTests
{
    [TestMethod]
    public void Performance_PatternMatching_NoUpwardTraversal()
    {
        // 测试重构前后的性能差异
        var code = @"
            switch (obj)
            {
                case int x when x > 0: return x;
                case string s: return s.Length;
                case Person { Name: ""John"", Age: > 18 }: return 1;
                default: return 0;
            }";

        var stopwatch = Stopwatch.StartNew();
        var result = walker.Visit(operation, SenseArgument.Default);
        stopwatch.Stop();

        // 预期：重构后性能提升 > 20%
        Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}ms");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < baselineMs * 0.8);
    }

    [TestMethod]
    public void Performance_ComplexNestedPatterns_Benchmark()
    {
        // 测试复杂嵌套模式的性能
        var code = @"
            obj is Person {
                Address: {
                    City: ""Beijing"",
                    ZipCode: > 100000
                },
                Age: > 18 and < 65
            }";

        // 运行基准测试
        var iterations = 1000;
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            walker.Visit(operation, SenseArgument.Default);
        }
        stopwatch.Stop();

        var avgMs = stopwatch.ElapsedMilliseconds / (double)iterations;
        Console.WriteLine($"Average: {avgMs}ms per iteration");
    }
}
```

### 8.5 迁移检查清单（待实施）

#### 文件级别检查
- [ ] 所有 `operation.Parent is ...` 已替换为 Sense 判断
- [ ] 所有 `ExtractPatternReference` 调用已移除
- [ ] 所有 Visit 方法签名已更新为 SenseArgument
- [ ] 所有 Translate 方法调用已更新参数类型

#### 测试级别检查
- [ ] 所有现有测试通过
- [ ] 每个 Sense 值有对应测试
- [ ] 性能测试通过（无退化）
- [ ] 边界情况测试覆盖

#### 代码质量检查
- [ ] 无编译警告
- [ ] 无 TODO/FIXME 注释
- [ ] XML 注释完整
- [ ] 代码审查通过

---

## 9. 参考文档

- [评审报告](./semantic-walker-sense-refactoring-review.md) - 详细的评审分析和建议
- [CLAUDE.md](../../CLAUDE.md) - 项目开发规则文档
