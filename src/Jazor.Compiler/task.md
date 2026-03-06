# Jazor.Compiler 任务追踪文档

> 更新时间：2026-03-07
> 分析范围：Jazor.Compiler 核心转换模块
> 分析依据：rule.md 开发规则文档

---

## 一、当前状态概览

### 1.1 核心转换完成度

| 功能模块 | 完成状态 | 测试覆盖 | 说明 |
|----------|----------|----------|------|
| 模式匹配 (Pattern) | ✅ 完成 | ✅ 完整 | 全部模式类型支持 |
| 循环语句 (Loop) | ✅ 完成 | ✅ 完整 | for/foreach/while/do-while |
| Switch 语句/表达式 | ✅ 完成 | ✅ 完整 | 常量和模式匹配 |
| 字符串插值 (String) | ✅ 完成 | ✅ 完整 | 模板字符串转换 |
| 异常处理 (TryCatch) | ✅ 完成 | ✅ 完整 | try-catch-finally |
| 元组 (Tuple) | ✅ 完成 | ✅ 完整 | 创建和解构 |
| 创建表达式 (Creation) | ✅ 完成 | ✅ 完整 | 对象/数组创建 |
| 引用操作 (Reference) | ✅ 完成 | ✅ 完整 | 白名单检查已修复 |
| 变量声明 (Declaration) | ✅ 完成 | ✅ 完整 | 局部变量/参数 |
| 普通运算 (Ordinary) | ✅ 完成 | ✅ 完整 | 二元/一元/条件表达式 |
| 无效操作处理 (Invalid) | ✅ 完成 | ✅ 完整 | 语法节点回退机制 |
| 不支持操作 (NotSupport) | ✅ 完成 | N/A | 明确抛出异常 |
| AST 优化器 (Optimizer) | ✅ 完成 | ✅ 完整 | AST 节点优化 |

> **测试状态说明**: 当前所有 533 个测试全部通过（100% 通过率）。

### 1.2 架构改进完成度

| 改进项 | 完成状态 | 说明 |
|--------|----------|------|
| Sense 语义上下文 | ✅ 完成 | 引入 Sense 枚举和 SenseArgument 结构体 |
| 移除向上遍历 | ✅ 完成 | 所有 operation.Parent 检查已替换为 Sense 判断 |
| PatternInput 上下文传递 | ✅ 完成 | 模式匹配输入显式传递 |
| CatchExceptionVar 上下文 | ✅ 完成 | 异常参数名显式传递 |
| 线程安全修复 | ✅ 完成 | 移除静态 Parser 实例 |
| 变量声明提升到块顶 | ✅ 完成 | VisitBlock flush 移到循环后 |
| 函数边界隔离 | ✅ 完成 | LocalFunction/AnonymousFunction 隔离 scope |
| try/catch/finally 体隔离 | ✅ 完成 | 各自 WithNewScope() 防止变量泄漏 |
| switch case 体隔离 | ✅ 完成 | IIFE 内 case 体隔离 scope |

### 1.3 核心文件状态

| 文件 | 状态 | 代码行数 | 说明 |
|------|------|----------|------|
| `Sense.cs` | ✅ 稳定 | ~95 | 语义上下文枚举（27个值） |
| `SenseArgument.cs` | ✅ 稳定 | ~105 | 语义上下文参数结构体 |
| `WalkerArgument.cs` | ✅ 稳定 | ~90 | 依赖项收集器 |
| `AstConverter.cs` | ✅ 稳定 | ~300 | 类级别转换器 |
| `SemanticWalker.cs` | ✅ 稳定 | ~300 | 主入口文件 |
| `SemanticWalker.cs.Pattern.cs` | ✅ 稳定 | ~800 | 模式匹配实现 |
| `SemanticWalker.cs.Tuple.cs` | ✅ 优化 | ~550 | 元组实现（已重构） |
| `SemanticWalker.cs.TryCatch.cs` | ✅ 优化 | ~240 | 异常处理（已重构） |
| `SemanticWalker.cs.Ordinary.cs` | ✅ 优化 | ~800 | 普通运算（已重构） |
| `SemanticWalker.cs.Reference.cs` | ✅ 稳定 | ~200 | 白名单检查已修复 |
| `SemanticWalker.cs.Loop.cs` | ✅ 稳定 | ~200 | 循环语句实现 |
| `SemanticWalker.cs.Switch.cs` | ✅ 稳定 | ~300 | Switch 实现 |

### 1.4 构建状态

**当前构建状态**: ✅ 成功

```bash
dotnet build src/Jazor.Compiler/Jazor.Compiler.csproj
# 已成功生成
```

---

## 二、待办任务清单

### 🔴 P0 - 紧急（影响核心功能）

当前无 P0 级别任务。

#### ~~P0-1: VisitFieldReference 缺少白名单检查~~ ✅ 已修复 (2026-03-06)

**修复内容**: 在 `VisitFieldReference` 方法中添加了白名单检查逻辑

**修改文件**: `core/SemanticWalker.cs.Reference.cs`

**修改要点**:
1. 在方法开始处添加 `GetWhiteListExpression` 调用检查白名单
2. 支持 `Alias`、`Inline`、`Import`、`Discard` 等 Op 类型
3. 参考 `VisitPropertyReference` 的实现模式
4. 所有 533 个测试通过

#### ~~P0-2: 类型/属性/字段/方法名称获取缺少白名单和特性别名检查~~ ✅ 已修复 (2026-03-07)

**修复内容**: 完善白名单与特性别名处理机制

**修改文件**:
- `core/SemanticWalker.cs` - `GetMapperType` 方法
- `core/SemanticWalker.cs.Creation.cs` - 对象初始化器、`VisitTypeParameterObjectCreation`
- `core/SemanticWalker.cs.Ordinary.cs` - with 表达式
- `core/SemanticWalker.cs.Pattern.cs` - 模式匹配属性
- `core/SemanticWalker.cs.Reference.cs` - `GetFieldName` 方法

**修改要点**:
1. **类型名称**: `GetMapperType` 使用 `GetTypeConfigOrWhiteListName` 支持白名单别名和特性配置
2. **泛型类型参数**: `VisitTypeParameterObjectCreation` 添加白名单检查，不在白名单中的类型报错
3. **初始化器成员**: 使用 `GetConfigOrSymbolName` 获取属性/字段/方法名称，支持 `[ECMAScriptName]` 特性
4. **模式匹配属性**: 使用 `GetConfigOrSymbolName` 替代 `m.Member.Name`
5. **with 表达式**: 使用 `GetConfigOrSymbolName` 获取成员名称
6. **字段名**: `GetFieldName` 使用 `GetConfigOrSymbolName` 支持特性别名
7. 所有 533 个测试通过

---

### 🟡 P1 - 高优先级（改进项）

| 序号 | 任务 | 涉及文件 | 状态 | 说明 |
|-----|------|----------|------|------|
| 1 | WalkerArgument 上下文优化 | 全部 | ✅ 完成 | 已通过 SenseArgument 实现，移除向上遍历 |
| 2 | 变量声明位置优化 | `SemanticWalker.cs.Ordinary.cs` 等 | ✅ 完成 | 声明提升到块顶，函数边界隔离 |

### 🟢 P2 - 中优先级（增强项）

| 序号 | 任务 | 涉及文件 | 状态 | 说明 |
|-----|------|----------|------|------|
| 3 | 测试覆盖率统计 | 测试项目 | ⏳ 待执行 | 运行覆盖率工具统计具体数值 |
| 4 | 注释统一为 XML 文档格式 | 所有分文件 | ⏳ 待执行 | 统一 XML 文档注释格式 |

### 🟣 P3 - 低优先级（可选优化）

| 序号 | 任务 | 涉及文件 | 状态 | 说明 |
|-----|------|----------|------|------|
| 5 | 性能优化评估 | 全部 | ⏳ 待评估 | 评估转换器性能，识别瓶颈 |

---

## 三、白名单检查审查报告

### 3.1 白名单检查完整性

| 文件 | 操作类型 | 白名单检查 | 状态 |
|------|---------|----------|------|
| `SemanticWalker.cs` (主文件) | 类型映射 (`GetMapperType`) | ✅ `WhiteList.Types` | 正确 |
| `SemanticWalker.cs` (主文件) | 成员映射 (`GetWhiteListExpression`) | ✅ `WhiteList.Members` | 正确 |
| `SemanticWalker.cs.Reference.cs` | `VisitPropertyReference` | ✅ 第375行 | 正确 |
| `SemanticWalker.cs.Reference.cs` | `VisitMethodReference` | ✅ 第424行 | 正确 |
| `SemanticWalker.cs.Reference.cs` | `VisitInvocation` | ✅ 第529行 | 正确 |
| `SemanticWalker.cs.Reference.cs` | `VisitFieldReference` | ❌ **缺失** | **问题** |
| `SemanticWalker.cs.Creation.cs` | `BuildObjectCreation` | ✅ 第44行 | 正确 |
| `SemanticWalker.cs.Pattern.cs` | 模式匹配属性/方法 | ✅ 通过 `GetWhiteListSymbol` | 正确 |

### 3.2 白名单检查完整性审查 (2026-03-07)

**审查结论**: ✅ 所有需要白名单检查的位置都已正确实现

| 文件 | 操作类型 | 白名单检查 | 状态 |
|------|---------|----------|------|
| `SemanticWalker.cs.Reference.cs` | `VisitFieldReference` | ✅ 第342行 | 已修复 |
| `SemanticWalker.cs.Reference.cs` | `VisitPropertyReference` | ✅ 第400行 | 正确 |
| `SemanticWalker.cs.Reference.cs` | `VisitMethodReference` | ✅ 第449行 | 正确 |
| `SemanticWalker.cs.Reference.cs` | `VisitInvocation` | ✅ 第554行 | 正确 |
| `SemanticWalker.cs.Creation.cs` | `BuildObjectCreation` | ✅ 第44行 | 正确 |
| `SemanticWalker.cs` | `GetMapperType` | ✅ 第130行 | 已修复 |
| `SemanticWalker.cs.Creation.cs` | `VisitTypeParameterObjectCreation` | ✅ 第323行 | 已修复 |
| `SemanticWalker.cs.Pattern.cs` | 属性子模式 (第354行) | ✅ `GetWhiteListSymbol` | 已修复 |
| `SemanticWalker.cs.Pattern.cs` | 属性子模式 (第478行) | ✅ `GetWhiteListSymbol` | 已修复 |
| `SemanticWalker.cs.Ordinary.cs` | with 表达式初始化器 | ✅ `GetConfigOrSymbolName` | 已修复 |
| `SemanticWalker.cs.Creation.cs` | 对象初始化器成员名 | ✅ `GetConfigOrSymbolName` | 已修复 |

### 3.3 名称获取规范 (2026-03-07)

**核心方法**:

| 方法 | 用途 | 检查顺序 |
|------|------|---------|
| `GetTypeConfigOrWhiteListName(ITypeSymbol)` | 获取类型名称 | 白名单别名 → 特性配置 → 原始名称 |
| `GetConfigOrSymbolName(ISymbol)` | 获取成员名称 | 特性配置 → 原始名称 |
| `GetInitializerMemberName(ISymbol)` | 获取初始化器成员名称 | 白名单别名(setter) → 特性配置 → 原始名称 |
| `GetMethodConfigOrWhiteListName(IMethodSymbol)` | 获取方法名称 | 白名单别名 → 特性配置 → 原始名称 |
| `GetWhiteListExpression(ISymbol, ...)` | 处理白名单操作 | `Alias`/`Inline`/`Import` 操作 |

**初始化器白名单处理**:
- 属性初始化器：检查 setter 的 `Inline`/`Import` 操作
- 方法初始化器：检查方法的 `Inline`/`Import` 操作
- `Inline` 操作：生成内联代码表达式
- `Import` 操作：生成模块导入调用

**特性配置支持**:
- `[ECMAScriptName("jsName")]` - 指定 JavaScript 名称
- `[Description("jsName")]` - 作为备选的别名配置

**需要名称转换的位置**:

| 位置 | 方法 | 使用的方法 |
|------|------|-----------|
| 类型名称 | `GetMapperType` | `GetTypeConfigOrWhiteListName` |
| 泛型类型参数对象创建 | `VisitTypeParameterObjectCreation` | `GetTypeConfigOrWhiteListName` |
| 字段名 | `GetFieldName` | `GetConfigOrSymbolName` |
| 属性/字段初始化器 | `BuildObjectCreationInitializer` | `GetConfigOrSymbolName` |
| 方法调用初始化器 | `BuildObjectCreationInitializer` | `GetConfigOrSymbolName` |
| with 表达式成员 | `VisitWith` | `GetConfigOrSymbolName` |
| 模式匹配属性 | `VisitRecursivePattern`/`VisitPropertySubpattern` | `GetConfigOrSymbolName` |

**不需要白名单检查的位置**:

| 类型 | 原因 |
|------|------|
| 本地变量/参数引用 | 用户定义的变量，不涉及跨模块调用 |
| 对象初始化器中的成员名 | 用户定义的类成员 |
| 元组元素名 | 编译器生成或用户定义的解构名称 |
| 控制流结构（循环、条件、异常等） | 不涉及成员访问 |
| 字符串插值 | 模板字符串转换 |
| 类型检查 (`is` 操作符) | 通过 `GetMapperType` 已处理 |

### 3.4 白名单操作类型处理

| Op 类型 | 处理位置 | 处理方式 |
|---------|---------|---------|
| `Alias` | `GetWhiteListExpression` | 替换名称 |
| `Inline` | `GetWhiteListExpression` | 解析并替换占位符 |
| `Import` | `GetWhiteListExpression` | 生成导入调用 |
| `Allowed` | - | 原生支持 |
| `Discard` | - | 不支持（需在检查时抛出异常） |
| `Compile` | `SemanticWalker.cs.Generate.cs` | 编译器生成处理 |

### 3.5 无需白名单检查的操作

以下操作类型不需要白名单检查：

| 文件 | 操作类型 | 原因 |
|------|---------|------|
| `SemanticWalker.cs.Loop.cs` | 循环语句 | 控制流结构，不涉及成员访问 |
| `SemanticWalker.cs.Switch.cs` | Switch 语句 | 控制流结构 |
| `SemanticWalker.cs.Declaration.cs` | 变量声明 | 局部变量不涉及外部成员 |
| `SemanticWalker.cs.String.cs` | 字符串插值 | 模板字符串转换 |
| `SemanticWalker.cs.TryCatch.cs` | 异常处理 | 控制流结构 |
| `SemanticWalker.cs.Tuple.cs` | 元组操作 | 语言内置结构 |
| `SemanticWalker.cs.NotSupport.cs` | 不支持操作 | 直接抛出异常 |
| `SemanticWalker.cs.Syntax.cs` | 语法转换 | 语法层面转换 |

---

## 四、已完成的重构

### 4.1 变量声明位置与作用域隔离重构 (2026-03-06)

**目标**: 将变量声明集中提升到块顶，并确保函数边界正确隔离作用域

**问题背景**:
- 变量声明被分散插入到各 statement 之间
- 函数体（LocalFunction、AnonymousFunction）未隔离 scope，变量声明泄漏到外部块
- try/catch/finally 体未隔离 scope
- switch case 体未隔离 scope

**完成内容**:
1. ✅ `VisitBlock`: flush 从循环内移到循环后，声明提升到块顶
2. ✅ `VisitLocalFunction`: `WithNewScope()` + 内部 flush
3. ✅ `VisitAnonymousFunction`: `WithNewScope()` + 内部 flush
4. ✅ `VisitTry`: try/catch/finally 体各自 `WithNewScope()`
5. ✅ `VisitSwitchPatternMatching`: case 体 `WithNewScope()`
6. ✅ 所有 533 个测试通过

**设计原则**:
- `_declarators`（变量声明）→ 不能穿越函数/块边界
- `_specifiers`（import 声明）→ 必须穿越函数边界传播到模块顶层

**提交记录**:
- `34256b4` - 变量声明提升和函数边界隔离重构

### 4.2 测试模式命名优化 (2026-03-06)

**目标**: 优化测试模式下的固定命名机制

**问题背景**:
- 测试模式返回固定名称 `v$test`，多个临时变量无法区分

**完成内容**:
1. ✅ 引入 `_testCache` 缓存机制
2. ✅ 相同位置返回相同索引，不同位置返回不同索引
3. ✅ 格式：`v$0`, `v$1`, `v$2`...

**提交记录**:
- `7a7ef39` - 测试模式命名优化

### 4.3 Sense 语义上下文重构 (2026-03-06)

**目标**: 通过显式传递语义上下文替代 `operation.Parent` 向上遍历

**完成内容**:
1. ✅ 创建 `Sense` 枚举（27个值，覆盖所有语义场景）
2. ✅ 创建 `SenseArgument` 结构体（不可变、值类型）
3. ✅ 迁移所有 Visit 方法到新签名
4. ✅ 移除所有 `operation.Parent` 检查
5. ✅ 删除 `ExtractPatternRefrence` 方法
6. ✅ 所有 533 个测试通过

**提交记录**:
- `75f16dc` - 移除 ExtractPatternRefrence 方法
- `ed29d36` - 替换 operation.Parent 检查为 Sense 枚举
- `0aa8386` - 修复静态 Parser 线程安全问题
- `566a746` - 传递 OutParameter 上下文
- `ccef3bf` - 移除剩余 operation.Parent 使用
- `aae9717` - 简化 VisitDeconstructionAssignment
- `c5b316d` - 提取 BuildTupleBinaryExpression 辅助方法

### 4.4 设计改进

| 改进项 | 修改前 | 修改后 |
|--------|--------|--------|
| 模式匹配输入查找 | 向上遍历操作树 | 通过 `SenseArgument.PatternInput` 显式传递 |
| re-throw 异常处理 | 向上遍历查找 ITryOperation | 通过 `SenseArgument.CatchExceptionVar` 传递 |
| Block 输出类型判断 | `operation.Parent is IMethodBodyOperation` | `argument.Sense == Sense.FunctionBody` |
| 条件访问操作数获取 | 向上遍历查找 IConditionalAccessOperation | 通过 `SenseArgument.PatternInput` 传递 |
| 变量声明位置 | 分散在各 stmt 之间 | 提升到块顶 |
| 函数体变量泄漏 | 无边界隔离 | `WithNewScope()` 隔离 |

---

## 五、已知问题追踪

### 5.1 已解决的问题

| 问题 | 解决状态 | 说明 |
|------|----------|------|
| 模式匹配依赖向上遍历 | ✅ 已解决 | 通过 PatternInput 显式传递 |
| re-throw 需要向上遍历 | ✅ 已解决 | 通过 CatchExceptionVar 显式传递 |
| 测试结果不稳定 | ✅ 已解决 | 移除静态 Parser 实例，解决线程安全问题 |
| out 参数变量声明丢失 | ✅ 已解决 | 在 VisitInvocation 中传递 OutParameter 上下文 |
| 变量声明位置分散 | ✅ 已解决 | 提升到块顶 |
| 函数体变量泄漏到外部块 | ✅ 已解决 | WithNewScope() 隔离 |
| try/catch/finally 体变量泄漏 | ✅ 已解决 | 各自 WithNewScope() |
| switch case 体变量泄漏 | ✅ 已解决 | WithNewScope() 隔离 |
| 测试模式下固定命名 | ✅ 已解决 | 使用 _testCache 缓存，返回带索引的固定名称 `v$0`, `v$1`... |

### 5.2 待解决的问题

当前无待解决的问题。

---

## 六、版本历史

### v1.6 - 2026-03-07 (当前)

- 完善白名单与特性别名处理机制
- 修复 `GetMapperType` 自定义类型名称获取
- 修复 `VisitTypeParameterObjectCreation` 添加白名单检查
- 修复对象初始化器成员名称使用 `GetConfigOrSymbolName`
- 修复模式匹配属性名称使用 `GetConfigOrSymbolName`
- 修复 with 表达式成员名称使用 `GetConfigOrSymbolName`
- 更新测试用例预期值（`string.Empty` → `""`, `TimeSpan.Zero` → `0n`）
- 所有 533 个测试通过

### v1.5 - 2026-03-06

- 修复 VisitFieldReference 缺少白名单检查问题
- 所有 533 个测试通过

### v1.4 - 2026-03-06

- 完成白名单检查审查
- 发现 VisitFieldReference 缺少白名单检查问题

### v1.3 - 2026-03-06

- 优化测试模式下固定命名机制
- 使用 _testCache 缓存，返回带索引的固定名称

### v1.2 - 2026-03-06

- 完成变量声明位置优化
- 完成函数边界隔离（LocalFunction、AnonymousFunction）
- 完成 try/catch/finally 体作用域隔离
- 完成 switch case 体作用域隔离
- 所有测试通过

### v1.1 - 2026-03-06

- 完成 Sense 语义上下文重构
- 移除所有向上遍历逻辑
- 修复线程安全问题
- 优化 VisitDeconstructionAssignment 和 BuildTupleBinaryExpression

### v1.0 - 2026-03-03

- 创建 rule.md 开发规则文档
- 创建 task.md 任务追踪文档
- 完成项目状态分析
- 确认核心转换功能完成

---

## 七、验收标准

### 7.1 转换正确性

- [x] 所有单元测试通过 (533/533)
- [x] 生成的 JavaScript AST 结构正确
- [x] 语义等价性验证通过

### 7.2 代码质量

- [x] 构建无警告
- [x] 代码符合项目规范
- [ ] 注释完整清晰 (部分完成)

### 7.3 架构质量

- [x] 无向上遍历操作树逻辑
- [x] 语义上下文显式传递
- [x] 单个 Visit 方法可独立测试
- [x] 函数边界正确隔离作用域
- [x] 变量声明集中提升到块顶
- [x] 所有引用操作都有白名单检查
- [x] 类型/属性/字段/方法名称获取支持白名单和特性别名

---

*本报告最后更新时间：2026-03-07*
*状态：核心功能完成，架构重构完成，作用域隔离完成，白名单与特性别名检查完成*
