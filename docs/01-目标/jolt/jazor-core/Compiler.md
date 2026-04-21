# Jazor 编译器（Compiler）

> 状态：已实现
> 定位：Jazor 核心编译管线的后端，负责将解析后的文档编译为 Vue SFC 和 C# 外部声明

## 1. 文档定位

本文档描述 Jazor 核心编译系统的编译器组件，该组件负责将 `JazorVueDocument` 编译为：
1. 生成的 Vue Single File Component (SFC) 文本
2. C# 外部声明代码（用于 C# 代码对 Vue 组件的引用）
3. Source Map（用于调试和错误映射）
4. 编译诊断信息

## 2. 核心类型

### 2.1 `JazorVueCompiler`

**文件路径**：`src/Jolt/Jazor/Core/JazorVueCompiler.cs`

**职责**：将 `JazorVueDocument` 编译为 `JazorVueCompilationResult`

**核心方法**：
```csharp
public JazorVueCompilationResult Compile(JazorVueDocument document)
```

**编译产物**：
- `GeneratedVueText`：完整的 Vue SFC 文本
- `GeneratedExternalDeclarationsText`：C# 外部声明代码
- `Diagnostics`：编译诊断列表（警告、错误）
- `GeneratedVueSourceMap`：Source Map JSON（可选）

### 2.2 `JazorVueCompilationResult`

**文件路径**：`src/Jolt/Jazor/Core/JazorVueContracts.cs`

**数据结构**：
```csharp
public sealed class JazorVueCompilationResult
{
    public JazorVueDocument Document { get; }
    public VirtualExternalSymbolTable ExternalSymbols { get; }
    public string GeneratedVueText { get; }
    public string GeneratedExternalDeclarationsText { get; }
    public IReadOnlyList<string> Diagnostics { get; }
    public JazorVueHotReloadMetadata? HotReload { get; }
    public string? GeneratedVueSourceMap { get; }
}
```

## 3. 核心算法

### 3.1 成员提取（Member Extraction）

编译器使用正则表达式从 C# 代码中提取 Vue 相关成员。

#### 3.1.1 Props 提取

**正则模式**：
```csharp
private static readonly Regex PropPattern = new Regex(
    @"\[Prop\]\s*public\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*\{",
    RegexOptions.Multiline | RegexOptions.Compiled);
```

**C# 示例**：
```csharp
[Prop]
public string Title { get; set; }
```

**生成的 Vue 代码**：
```javascript
const props = defineProps({
  title: String,
});
const title = toRef(props, "title");
```

**类型映射**（`MapVueType`）：
- `string`、`String` → `String`
- `bool`、`Boolean` → `Boolean`
- `int`、`long`、`short`、`float`、`double`、`decimal`、`byte` → `Number`
- 其他类型 → `null`（运行时检查）

#### 3.1.2 State 提取

**正则模式**：
```csharp
private static readonly Regex StatePattern = new Regex(
    @"\[State\]\s*private\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*(=\s*(?<initializer>[^;]+))?;",
    RegexOptions.Multiline | RegexOptions.Compiled);
```

**C# 示例**：
```csharp
[State]
private int count = 0;
```

**生成的 Vue 代码**：
```javascript
const count = ref(0);
```

**特性**：
- 自动推断初始值（如果未提供，使用 `undefined`）
- 支持 C# 初始化表达式

#### 3.1.3 Computed 提取

**正则模式**：
```csharp
private static readonly Regex ComputedPattern = new Regex(
    @"\[Computed\]\s*public\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*=>\s*(?<expression>[^;]+);",
    RegexOptions.Multiline | RegexOptions.Compiled);
```

**C# 示例**：
```csharp
[Computed]
public string DisplayName => $"{FirstName} {LastName}";
```

**生成的 Vue 代码**：
```javascript
const displayName = computed(() => `${firstName} ${lastName}`);
```

**降级逻辑**（`TryLowerComputed`）：
- 简单表达式：直接转换
- 复杂表达式：添加诊断警告，返回 `undefined`

#### 3.1.4 Methods 提取

**正则模式**：
```csharp
private static readonly Regex MethodPattern = new Regex(
    @"public\s+(?<async>async\s+)?(?<return>[\w\.\?\<\>\[\]]+)\s+(?<name>\w+)\s*\((?<parameters>[^\)]*)\)\s*\{",
    RegexOptions.Multiline | RegexOptions.Compiled);
```

**C# 示例**：
```csharp
public async Task SaveAsync()
{
    await Task.CompletedTask;
    Count++;
}
```

**生成的 Vue 代码**：
```javascript
async function saveAsync() {
  await Promise.resolve();
  count.value++;
}
```

**参数解析**：
- 支持默认参数（通过 `Split('=')` 去除默认值）
- 提取参数名（类型后的最后一个标识符）

### 3.2 方法体降级（Method Body Lowering）

**实现**：`TryLowerMethodBody()`

**降级规则**：

#### 3.2.1 表达式重写（`LowerExpression`）

1. **成员访问重写**：
   - `this.MemberName` → `memberName`
   - C# 属性/字段 → Vue ref/props (`.value`)

2. **字符串插值转换**：
   - C#：`$"Hello {Name}"`
   - Vue：`` `Hello ${name}` ``

3. **内置类型替换**：
   - `string.Empty` → `""`
   - `Task.CompletedTask` → `Promise.resolve()`

4. **异常构造简化**：
   - `new InvalidOperationException(...)` → `new Error(...)`
   - 保留非 `Exception` 后缀的类型名

#### 3.2.2 变量声明降级

**局部变量声明**：
```csharp
// C#
var name = value;
int count = 0;
```
```javascript
// Vue
let name = value;
let count = 0;
```

**模式**：
```csharp
private static readonly Regex LocalDeclarationWithInitializerPattern = new(
    @"^(?<indent>\s*)(?<type>" + LocalTypePattern + @")\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<expression>.+);\s*$",
    RegexOptions.Compiled);
```

#### 3.2.3 循环语句降级

**For 循环**：
```csharp
// C#
for (int i = 0; i < 10; i++)
```
```javascript
// Vue
for (let i = 0; i < 10; i++)
```

**Foreach 循环**：
```csharp
// C#
foreach (var item in items)
```
```javascript
// Vue
for (const item of items)
```

**Catch 子句**：
```csharp
// C#
catch (Exception ex)
```
```javascript
// Vue
catch (ex)
```

#### 3.2.4 作用域处理

**作用域栈**：
```csharp
var scopeStack = new Stack<HashSet<string>>();
scopeStack.Push(new HashSet<string>(method.Parameters, StringComparer.Ordinal));
```

**作用域规则**：
- `{` 开启新作用域
- `}` 关闭作用域
- `let` 声明的变量在当前作用域可见
- `for`/`foreach` 变量在下一作用域可见

**遮蔽名称检测**：
- 避免重写被遮蔽的变量名
- 使用 `shadowedNames` 集合追踪

### 3.3 Vue 代码生成

**生成顺序**（`Compile` 方法）：

1. **`<script setup>` 标签**：
   ```javascript
   <script setup>
   ```

2. **Vue Helpers 导入**：
   ```javascript
   import { ref, computed, toRef } from "vue";
   ```

3. **用户导入**：
   ```javascript
   import { defaultBinding, named } from "source.js";
   ```

4. **Props 定义**：
   ```javascript
   const props = defineProps({
     propName: String,
   });
   const propName = toRef(props, "propName");
   ```

5. **State 定义**：
   ```javascript
   const stateName = ref(initialValue);
   ```

6. **Computed 定义**：
   ```javascript
   const computedName = computed(() => expression);
   ```

7. **Methods 定义**：
   ```javascript
   function methodName() {
     // body
   }
   ```

8. **原始 @code 块注释**：
   ```javascript
   /*
    Original @code block retained for bridge diagnostics:
    @code {
       ...
    }
   */
   ```

9. **`</script>` 标签**

10. **`<template>` 标签**：
    ```javascript
    <template>
      <!-- 模板内容或 <div /> 占位符 -->
    </template>
    ```

### 3.4 Source Map 生成

**实现**：`CreateGeneratedVueSourceMap()`

**映射策略**：
1. **行级别映射**：
   - 每个生成的 Vue 行映射到源文件行号
   - 基于 `GeneratedVueLine.SourceLine`

2. **列级别映射**：
   - 通过 `EnumerateSharedTokenAnchors()` 找到共享标识符
   - 匹配标识符在生成文本和源文本中的位置
   - 使用 `SourceMapAnchorTokenPattern`：`@"[A-Za-z_][A-Za-z0-9_]*|\d+"`

3. **后备映射**：
   - 如果没有标识符匹配，使用第一个非空白字符位置

**Source Map 结构**：
```json
{
  "version": 3,
  "file": "Component.jazor.vue",
  "sources": ["Component.jazor"],
  "mappings": "AAAA,GAAG,GAAG,..."
}
```

### 3.5 外部声明生成

**实现**：`JazorVueExternalDeclarationEmitter.Emit()`（在 `JazorVueExternalDeclarationEmitter.cs` 中）

**用途**：为 C# 代码提供对导入符号的引用能力

**示例输出**：
```csharp
namespace Jazor.Generated.Externals
{
    internal static partial class Component_jazor_externals
    {
        [ExternalSymbol(Default)]
        public static void defaultBinding();

        [ExternalSymbol(Named)]
        public static void namedBinding();
    }
}
```

## 4. 线程安全模型

**无状态编译器**：
- `JazorVueCompiler` 是 sealed class，但所有字段都是 `static readonly Regex`
- 实例方法不维护可变状态
- 每次调用 `Compile()` 都创建新的 `JazorVueCompilationResult`

**线程安全保证**：
- 多个线程可以同时调用 `Compile()`
- 正则表达式是线程安全的（`RegexOptions.Compiled`）
- 无共享缓存或全局状态

## 5. 错误处理

### 5.1 参数验证

```csharp
if (document is null)
    throw new ArgumentNullException(nameof(document));
```

### 5.2 降级失败诊断

**Computed 降级失败**：
```csharp
diagnostics.Add($"Computed member '{computed.SourceName}' could not be lowered by the local fallback compiler.");
```

**Method 降级失败**：
```csharp
diagnostics.Add($"Method '{method.SourceName}' could not be lowered by the local fallback compiler.");
```

### 5.3 无公共方法警告

```csharp
if (methods.Count == 0 && document.Code.Length > 0)
    diagnostics.Add("No public methods were lowered. The current bridge compiler emits method stubs only for public instance methods.");
```

## 6. 配置选项

### 6.1 正则表达式配置

所有正则表达式都使用 `RegexOptions.Compiled`：
- `PropPattern`：Props 提取
- `StatePattern`：State 提取
- `ComputedPattern`：Computed 提取
- `MethodPattern`：Methods 提取
- `LocalDeclarationPattern`：局部变量声明
- `ForLoopPattern`、`ForeachLoopPattern`：循环语句
- `TypedCatchPattern`：Catch 子句
- `ExceptionConstructorPattern`：异常构造

### 6.2 Source Map 配置

```csharp
private const int MaxResponseProbeLines = 1000;
private const int MaxCapturedErrorChars = 16 * 1024;
private const int MaxCapturedOutputLines = 200;
```

### 6.3 Vue Helpers 配置

**自动导入的 Vue 组合式 API**（`GetVueHelpers`）：
- `computed`：存在 computed 成员时
- `ref`：存在 state 成员时
- `toRef`：存在 props 成员时

## 7. 与其他子系统的交互

### 7.1 与解析器交互

**数据流**：
```
JazorVueDocument (Parser 输出)
    ↓
JazorVueCompiler.Compile()
    ↓
JazorVueCompilationResult
```

**依赖**：
- `document.Imports`：生成导入语句
- `document.Code`：提取成员
- `document.Template`：生成 `<template>` 内容
- `document.CodeStartIndex`、`document.TemplateStartIndex`：Source Map 行号计算

### 7.2 与投影服务交互

**消费者**：`JazorProjectionService`

**用途**：
- 生成虚拟 Vue 文档投影
- 生成 C# 外部声明投影

**关键调用**：
```csharp
var compilation = _compiler.Compile(parsedDocument);
var vueProjectedPath = "virtual:" + document.DocumentPath + ".g.vue";
```

### 7.3 与分析服务交互

**消费者**：`FallbackJazorAnalysisService`

**用途**：当 RPC 分析服务不可用时，使用进程内编译器

**遥测报告**：
```csharp
FallbackTelemetry.ReportActivation(
    component: "analysisService",
    mode: "inProcFallback",
    reason: "analysis-rpc-unavailable",
    documentPath: request.JazorDocument.DocumentPath);
```

### 7.4 与 Source Map 服务交互

**生成器**：`SourceMapWriter.Write()`（在 `Jazor.SourceMaps` 中）

**输出格式**：标准的 Source Map v3 JSON

**用途**：
- DevTools 调试
- 错误堆栈映射
- LSP 诊断位置映射

## 8. 设计权衡

### 8.1 正则表达式 vs 完整编译器

**设计决策**：使用正则表达式提取成员，而非完整 C# 编译器

**权衡**：
- **优点**：
  - 轻量级，无 Roslyn 依赖
  - 快速启动（无需编译器初始化）
  - 适合简单场景
- **缺点**：
  - 不支持复杂 C# 语法（泛型、特性参数、partial 方法等）
  - 脆弱，依赖代码格式
  - 需要手动维护正则模式

**选择理由**：
- 快速原型开发
- 大多数常见场景已覆盖
- 后备方案（Fallback 编译器）

### 8.2 降级编译器 vs 完整转译器

**设计决策**：实现局部降级编译器（方法体重写），而非完整 C#→JavaScript 转译器

**权衡**：
- **优点**：
  - 实现简单（约 1000 行代码）
  - 覆盖常见模式
  - 可逐步扩展
- **缺点**：
  - 不支持高级 C# 特性（LINQ、async 迭代器等）
  - 降级失败时返回 `undefined`
  - 诊断信息有限

**选择理由**：
- Jazor 主要目标是简单的桥接层
- 复杂逻辑应放在独立的 JS/TS 模块中
- 后备方案：完整的 Jazor.Compiler（SemanticWalker）

### 8.3 原始 @code 块注释

**设计决策**：在生成的 Vue SFC 中保留原始 @code 块的注释版本

**权衡**：
- **优点**：
  - 保留源代码上下文
  - 支持调试和诊断
  - 便于问题排查
- **缺点**：
  - 增加生成代码大小
  - 可能暴露实现细节

**选择理由**：
- "bridge diagnostics" 是关键需求
- 注释不影响运行时行为
- 可通过配置禁用（未来）

### 8.4 Source Map 精度

**设计决策**：实现行级别 + 列级别的混合 Source Map

**权衡**：
- **优点**：
  - 调试体验良好
  - 支持精确的错误位置
  - 标准 Source Map v3 格式
- **缺点**：
  - 启发式匹配可能不准确
  - 增加编译时间和内存

**选择理由**：
- DevTools 集成是关键功能
- 列级别映射提升断点调试体验
- 后备到行级别映射保证健壮性

### 8.5 Vue Helpers 自动导入

**设计决策**：根据使用的成员自动导入 Vue 组合式 API

**权衡**：
- **优点**：
  - 减少样板代码
  - 与现代 Vue 开发体验一致
- **缺点**：
  - 隐式依赖 Vue
  - 可能与用户导入冲突

**选择理由**：
- `<script setup>` 是标准 Vue 模式
- 自动导入提升开发体验
- 用户可以通过显式导入覆盖

## 9. 附录：关键正则模式

```csharp
// 成员提取
private static readonly Regex PropPattern = new Regex(
    @"\[Prop\]\s*public\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*\{",
    RegexOptions.Multiline | RegexOptions.Compiled);

private static readonly Regex StatePattern = new Regex(
    @"\[State\]\s*private\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*(=\s*(?<initializer>[^;]+))?;",
    RegexOptions.Multiline | RegexOptions.Compiled);

private static readonly Regex ComputedPattern = new Regex(
    @"\[Computed\]\s*public\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*=>\s*(?<expression>[^;]+);",
    RegexOptions.Multiline | RegexOptions.Compiled);

private static readonly Regex MethodPattern = new Regex(
    @"public\s+(?<async>async\s+)?(?<return>[\w\.\?\<\>\[\]]+)\s+(?<name>\w+)\s*\((?<parameters>[^\)]*)\)\s*\{",
    RegexOptions.Multiline | RegexOptions.Compiled);

// 语句降级
private static readonly Regex LocalDeclarationWithInitializerPattern = new(
    @"^(?<indent>\s*)(?<type>" + LocalTypePattern + @")\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<expression>.+);\s*$",
    RegexOptions.Compiled);

private static readonly Regex ForLoopPattern = new(
    @"^(?<indent>\s*)for\s*\((?<initializer>.*?);(?<condition>.*?);(?<iterator>.*?)\)(?<suffix>\s*\{?\s*)$",
    RegexOptions.Compiled);

private static readonly Regex ForeachLoopPattern = new(
    @"^(?<indent>\s*)foreach\s*\(\s*(?<type>" + LocalTypePattern + @")\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s+in\s+(?<expression>.+?)\s*\)(?<suffix>\s*\{?\s*)$",
    RegexOptions.Compiled);

// Source Map
private static readonly Regex SourceMapAnchorTokenPattern = new(
    @"[A-Za-z_][A-Za-z0-9_]*|\d+",
    RegexOptions.Compiled);
```

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
