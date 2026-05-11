# Jazor.Compiler 生产就绪审计报告

> 审计日期：2026-05-11
> 审计范围：`src/Jazor.Compiler/core/SemanticWalker*.cs`、`src/Jazor.CompilerTest/`
> 目标：识别上线前必须修复的语义 bug、风险项、测试覆盖缺口
> 修复状态：P0 + P1 全部修复完成，1878 个编译器测试通过（2026-05-11）

---

## 一、关键语义 Bug（上线阻塞）

### BUG-1: `typeof null === "object"` 导致 `is object` 匹配 null

- **位置**: `SemanticWalker.cs` `CreateTypeMatchExpr` (~L1524)
- **现象**: `obj is object` 编译为 `typeof obj === "object"`，但 JS 中 `typeof null` 也是 `"object"`，导致 null 值错误匹配
- **影响**: 所有 Object 映射类型的类型检查在 null 输入时产生错误结果
- **修复方向**: 发射 `obj !== null && typeof obj === "object"`

### BUG-2: 带标签的 break/continue 丢失标签

- **位置**: `SemanticWalker.cs.Ordinary.cs` L192-193
- **现象**:
  ```csharp
  BranchKind.Break    => new BreakStatement(null),
  BranchKind.Continue => new ContinueStatement(null),
  ```
  `IBranchOperation.Target`（标签符号）从未被读取。`break outerLoop;` 编译为 `break;`
- **影响**: 嵌套循环中带标签跳转语义完全错误
- **修复方向**: 从 `operation.Target` 提取标签名，传入 `BreakStatement` / `ContinueStatement`

### BUG-3: pattern-matching switch 中 branch 操作全部变成 `return null`

- **位置**: `SemanticWalker.cs.Pattern.cs` L2108-2109
- **现象**:
  ```csharp
  if (bodyOp.Kind == OperationKind.Branch)
      casePending.Add(new ReturnStatement(null));
  ```
  所有 `IBranchOperation` 不区分 `BranchKind`，统一发射 `return null`
- **影响**: pattern switch case 内的 `continue` 外层循环变成 `return null`，循环断裂
- **修复方向**: 检查 `BranchKind`，分别发射 `break` / `continue` / 对应语句

### BUG-4: default case body 执行两次

- **位置**: `SemanticWalker.cs.Pattern.cs` L2121-2136
- **现象**: 当 `switchCase` 同时有 pattern 条件和 default 子句时，`bodyStatements` 在 `if` 块内追加一次（L2131），又在外部无条件追加一次（L2136）
- **影响**: default 分支的副作用执行两次
- **修复方向**: 确保 body 只在正确的分支路径中追加一次

### BUG-7: `Deconstruct` 查找不匹配 arity，多重载时选错或崩溃

- **位置**: `SemanticWalker.cs.Tuple.cs` L770-773
- **现象**:
  ```csharp
  method = (IMethodSymbol)valueType
      .GetMembers()
      .First(x => x.Kind == SymbolKind.Method && x.Name == "Deconstruct");
  ```
  1. 无 `Deconstruct` 时 `First()` 抛 `InvalidOperationException`
  2. 多重载时取第一个，不检查参数数量
- **影响**: 多重载 `Deconstruct` 的类型解构产生参数数量不匹配的 JS 调用
- **修复方向**: 按解构目标数量匹配 arity，无匹配时走 `HandleTransformationFailure`

---

## 二、中等严重度 Bug

### BUG-5: positional subpattern 属性为 null 时静默丢弃后续条件

- **位置**: `SemanticWalker.cs.Pattern.cs` L1942-1943
- **现象**: `GetPositionalPropertyExpression` 返回 null 时直接 `return`，后续所有 positional subpattern 条件被丢弃
- **影响**: 非 tuple/record 类型的 positional pattern 生成不完整的条件表达式
- **修复方向**: 走 `HandleTransformationFailure` 或发射诊断

### BUG-6: list pattern 嵌套模式未守卫 null expr

- **位置**: `SemanticWalker.cs.Pattern.cs` L780-781
- **现象**: constant 和 declaration 分支都检查 `if (expr is not null)`，但 else 分支（非 constant、非 declaration 的嵌套 pattern）直接使用 `expr` 构造 `LogicalExpression`
- **影响**: 特定嵌套 pattern 组合触发 NullReferenceException
- **修复方向**: 加 null 守卫，与相邻分支保持一致

### BUG-8: `with` 表达式中非标准 LHS 静默丢弃属性更新

- **位置**: `SemanticWalker.cs.Ordinary.cs` L2239-2256
- **现象**: initializer 左侧既非 `Identifier` 也非 `MemberExpression` 时，`key` 为 null，该属性被跳过
- **影响**: 复杂 `with` 表达式中部分属性更新丢失，无错误提示
- **修复方向**: 对无法识别的 LHS 走 `HandleTransformationFailure`

---

## 三、风险项（建议修复或标注为已知限制）

| ID | 位置 | 说明 | 建议 |
|---|---|---|---|
| RISK-1 | `VisitBranch` default 分支 | 未知 `BranchKind` 静默返回 null | 改为 `HandleTransformationFailure` |
| RISK-2 | `VisitUnaryOperator` Hat | `^` index-from-end 是空 stub | 实现或显式拒绝 |
| RISK-3 | `VisitWhileLoop` | condition 为 null 时返回 null | 改为显式错误 |
| RISK-4 | `VisitCoalesce` | JS `??` 对 `undefined` 也触发，C# `??` 仅对 null | 文档标注或加 `!== undefined` 守卫 |
| RISK-5 | local/anonymous function 参数 | `ref`/`out`/default 值静默忽略 | 显式拒绝或实现 |
| RISK-7 | `ContainsAwaitOperation` | syntax fallback 不尊重 lambda 边界 | 可能误标非 async 函数为 async |
| RISK-10 | `BuildGroupChain` 多 catch | 第二个无 `when` 的 catch 无条件执行 | 加类型检查条件 |
| RISK-12 | `TryBuildConditionalAccessNullishGuard` | 用 `==` 而非 `===` 做 null 检查 | 统一为 `===` |

---

## 四、测试覆盖缺口

### 4.1 零覆盖（需新增测试文件或显式 not-supported 断言）

| 领域 | 建议动作 |
|---|---|
| `yield return` / 生成器函数 | 加 `VisitYieldReturn_NotSupported` 或实现测试 |
| `unsafe` / `fixed` / `stackalloc` | 加 not-supported 测试 |
| 带标签的 `break` / `continue` | 补回归测试（对应 BUG-2） |
| async local function | 补充到 `SemanticWalkerDeclarationTest` |
| `ValueTask` / `ValueTask<T>` await | 补充到 `SemanticWalkerOrdinaryTest` |
| `await using` / `IAsyncDisposable` | 加 not-supported 或实现测试 |

### 4.2 覆盖薄弱（需补充用例）

| 领域 | 现状 | 建议补充 |
|---|---|---|
| LINQ 方法链 | 仅 Where/Select（6 个测试） | OrderBy, GroupBy, Any/All, First/Single, Skip/Take, Distinct, SelectMany |
| `Nullable<T>` 成员访问 | 仅声明测试 | `.HasValue`, `.Value`, nullable 算术, nullable 比较 |
| Enum 操作 | 仅值擦除和 switch | HasFlag, Parse/TryParse, GetValues |
| Collection expression | 仅 Span/ReadOnlySpan（2 个测试） | `List<T>`, `IEnumerable<T>`, 嵌套, 非数组 spread |
| 泛型 | 仅 AstConverter 层 | SemanticWalker 层显式类型参数、约束 |
| 继承 | 仅 AstConverter 层 | SemanticWalker 层 `base.Method()` / `base.Property` lowering |

### 4.3 覆盖充分（无需额外动作）

- Pattern matching（6200+ 行，所有 C# 11/12 pattern 类型）
- String 操作（3400+ 行，所有方法和插值形式）
- Tuple lowering（2393 行，含嵌套、解构、比较）
- Try-catch-finally（2934 行，含多 catch、when、嵌套）
- Loop（2573 行，所有循环形式和嵌套）
- Switch（2708 行，传统和 pattern-matching）
- Object/array creation（4100+ 行）
- Source map 和 source origin

---

## 五、修复优先级建议

### P0 — 上线阻塞（必须修复）

1. BUG-1: `is object` null 匹配
2. BUG-2: 带标签 break/continue
3. BUG-3: pattern switch branch → `return null`
4. BUG-4: default case body 双重执行
5. BUG-7: `Deconstruct` arity 不匹配

### P1 — 高优先级（强烈建议修复）

6. BUG-5: positional subpattern 静默丢弃
7. BUG-6: list pattern null expr
8. BUG-8: `with` 表达式属性丢失
9. RISK-1: 未知 BranchKind 静默 null
10. RISK-10: 多 catch 无条件执行

### P2 — 中优先级（可标注为已知限制）

11. RISK-2: `^` 运算符未实现
12. RISK-4: `??` undefined 语义差异
13. RISK-5: local function ref/out/default 忽略
14. RISK-12: `==` vs `===` 不一致

### P3 — 测试补全

15. 补 not-supported 显式拒绝测试
16. 补 BUG 修复对应的回归测试
17. 补覆盖薄弱区域的边界用例

---

## 六、结论

编译器核心 lowering 逻辑在主路径上质量较高，pattern matching、string、tuple、loop 等高频场景覆盖充分。但存在 **5 个高严重度语义 bug**（BUG-1 ~ BUG-4, BUG-7），这些 bug 在特定输入组合下会产生运行时错误结果且无编译期警告，**建议修复后再上生产**。

风险项中 RISK-10（多 catch）和 RISK-1（未知 BranchKind）也可能在边界场景下产生错误行为，建议一并处理。

---

## 七、修复记录（2026-05-11）

| 项目 | 处置 | 改动文件 |
|---|---|---|
| BUG-1 | ✅ 已修复：`TypeMapper.Object` 和 tuple/anonymous 加 `!== null` 前置守卫 | `SemanticWalker.cs.Pattern.cs` |
| BUG-2 | ⚠️ 误报：C# 无带标签 break/continue 语法，当前实现正确 | — |
| BUG-3 | ✅ 已修复：区分 `BranchKind.Break`（→ return）和 `Continue`（→ 显式拒绝） | `SemanticWalker.cs.Pattern.cs` |
| BUG-4 | ✅ 已修复：default body 改为 `else if (hasDefault)` 分支 | `SemanticWalker.cs.Pattern.cs` |
| BUG-5 | ✅ 已修复：null propertyExpr 走 `HandleTransformationFailure` | `SemanticWalker.cs.Pattern.cs` |
| BUG-6 | ✅ 已修复：嵌套 pattern expr 加 `is not null` 守卫 | `SemanticWalker.cs.Pattern.cs` |
| BUG-7 | ✅ 已修复：按 arity 匹配 `Deconstruct`，无匹配时显式失败 | `SemanticWalker.cs.Tuple.cs` |
| BUG-8 | ✅ 已修复：非标准 LHS 走 `HandleTransformationFailure` | `SemanticWalker.cs.Ordinary.cs` |
| RISK-1 | ✅ 已修复：未知 `BranchKind` 走 `HandleTransformationFailure` | `SemanticWalker.cs.Ordinary.cs` |
| RISK-10 | ⚠️ 误报：多 catch 按类型分组，组内无 when 的 catch-all 是正确语义 | — |

测试期望值同步更新：`SemanticWalkerPatternTest.cs`（`Visit_IsType_Object`、`Visit_IsType_Object_Direct`）。

验证结果：`dotnet test` 1878 通过 / 0 失败。
