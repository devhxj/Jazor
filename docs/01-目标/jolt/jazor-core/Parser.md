# Jazor 解析器（Parser）

> Status: 活跃参考
> Positioning: Jazor 核心编译管线的前端，负责将 .jazor 源文本解析为结构化文档模型

## 1. 文档定位

本文档描述 Jazor 核心编译系统的解析器组件，该组件负责将 .jazor 源文件文本解析为结构化的 `JazorVueDocument` 对象，为后续编译和投影提供基础数据结构。

## 2. 核心类型

### 2.1 `JazorVueParser`

**文件路径**：`src/Jolt/Jazor/Core/JazorVueParser.cs`

**职责**：将 .jazor 源文本解析为 `JazorVueDocument` 对象

**核心方法**：
```csharp
public JazorVueDocument Parse(string filePath, string sourceText)
```

**解析流程**：
1. 调用 `ParseCode()` 提取 `@code` 块内容
2. 调用 `ParseTemplate()` 提取模板内容（支持 `<template>` 标签或隐式 markup）
3. 调用 `BuildImports()` 构建导入指令集合（显式声明 + 推断 Vue 组件导入）

### 2.2 `JazorVueDocument`

**文件路径**：`src/Jolt/Jazor/Core/JazorVueContracts.cs`

**数据结构**：
```csharp
public sealed class JazorVueDocument
{
    public string FilePath { get; }              // 源文件路径
    public string SourceText { get; }            // 完整源文本
    public IReadOnlyList<JazorImportDirective> Imports { get; }  // 导入指令
    public string Template { get; }               // 模板内容
    public string Code { get; }                   // @code 块内容
    public int CodeStartIndex { get; }            // @code 块起始位置
    public int TemplateStartIndex { get; }        // 模板起始位置
    public int TemplateLength { get; }            // 模板长度
    public int CodeLength { get; }                // @code 块长度
}
```

**设计特点**：
- 保留源文本的完整索引信息，支持源映射（source map）生成
- 分离显式导入和推断导入，支持兼容性检查
- 支持两种模板模式：显式 `<template>` 标签和隐式 markup

### 2.3 `JazorMarkupPatterns`

**文件路径**：`src/Jolt/Jazor/Core/JazorMarkupPatterns.cs`

**核心正则表达式**：
```csharp
internal static readonly Regex ComponentTagPattern = new Regex(
    @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
    RegexOptions.Compiled);
```

**用途**：识别 PascalCase 组件标签（如 `<MyComponent>`），用于推断 Vue 组件导入

## 3. 核心算法

### 3.1 Code 块解析

**实现**：`JazorCodeDirectiveLocator.TryFindCodeDirectiveWithBlockBody()`

**解析规则**：
- 使用 `RazorBlockDirectiveLocator` 查找 `@code` 指令
- 支持块体语法：`@code { ... }`
- 跳过字符串字面量、注释（单行 `//`、块级 `/* */`、Razor 注释 `@* *@`）
- 验证闭合括号匹配，未闭合时抛出 `FormatException`

**位置追踪**：
- `DirectiveIndex`：`@code` 指令起始位置
- `OpeningBraceIndex`：开括号 `{` 位置
- `ClosingBraceIndex`：闭括号 `}` 位置（-1 表示未闭合）

### 3.2 Template 解析

**实现**：`JazorVueParser.ParseTemplate()`

**两种模式**：

1. **显式 `<template>` 标签**：
   ```csharp
   private static readonly Regex TemplatePattern = new Regex(
       @"<template>(?<content>[\s\S]*?)</template>",
       RegexOptions.IgnoreCase | RegexOptions.Compiled);
   ```
   - 提取标签内内容并 trim
   - 计算相对源文本的索引位置

2. **隐式 Markup**：
   - 定位到 `@code` 指令或文件末尾
   - 移除顶层导入指令行（`@module`、`@import` 等）
   - Trim 结果作为模板内容

**位置计算**：
- `TemplateStartIndex`：模板内容在源文本中的起始行号
- `TemplateLength`：模板内容的字符长度

### 3.3 Import 指令解析

**实现**：`JazorImportDirectiveLocator.EnumerateDirectiveLines()`

**支持的语法**：

1. **现代 `@module` 语法**（推荐）：
   ```csharp
   @module { defaultBinding, named as alias } from "source.js"
   @module * as namespace from "library"
   ```
   - 正则模式：`@"^@module\s+(?<clause>.+?)\s+from\s+(?<quote>[""'])(?<source>[^""']+)\k<quote>\s*$"`
   - 解析导入子句（clause）和源路径

2. **遗留语法**（兼容性支持）：
   ```csharp
   @import ...     // 通用 JavaScript 导入
   @jsimport ...   // 显式 JavaScript 导入
   @vueimport ...  // Vue 组件导入
   ```

**排除范围**：
- 跳过 `<template>...</template>` 标签内的内容
- 跳过 `@code {...}` 和 `@functions {...}` 块内的内容
- 跳过单行注释 `//`、块注释 `/* */`、Razor 注释 `@* *@`

**绑定解析**：`ParseBindings(string clause)`
- 默认绑定：`defaultBinding` → `JazorImportBindingKind.Default`
- 命名绑定：`{ named1, named2 as alias }` → `JazorImportBindingKind.Named`
- 命名空间绑定：`* as ns` → `JazorImportBindingKind.Namespace`

### 3.4 Vue 组件导入推断

**实现**：`JazorVueParser.InferVueImports()`

**触发条件**：模板中存在 PascalCase 标签（通过 `ComponentTagPattern` 匹配）

**推断逻辑**：
1. 扫描模板中的所有 `<PascalCase>` 标签
2. 对每个组件名，搜索文件系统：
   - 当前文档目录
   - 当前目录的 `Components/` 和 `components/` 子目录
   - 父目录
   - 父目录的 `Components/` 和 `components/` 子目录
3. 找到 `.vue` 文件时，生成相对导入路径
4. 生成推断的 `@module` 指令注释：`/* inferred vue import ComponentName from "./path" */`

**兼容性处理**：
- 如果用户显式声明了导入（与推断的组件名和本地名匹配），保留显式导入
- 仅推断未显式声明的组件

### 3.5 字符串和注释跳过

**实现**：`RazorBlockDirectiveLocator.TrySkipCodeLiteralOrComment()`

**支持的字面量类型**：
1. **常规字符串**：`'...'`、`"..."`
2. **插值字符串**：`$"..."`、`$'...'`
3. **Verbatim 字符串**：`@"..."`、`@$"..."`、`$@"..."`
4. **Raw 字符串**（C# 11+）：`$$"..."`（任意数量的 `$` 前缀）

**支持的注释类型**：
- 单行注释：`//`
- 块注释：`/* */`
- Razor 注释：`@* *@`

**算法特点**：
- 递归深度控制（括号匹配）
- 转义字符处理（`\"`、`\\`）
- Verbatim 字符串中的双引号转义（`""`）
- Raw 字符串的分隔符长度匹配

## 4. 线程安全模型

**无状态解析器**：
- `JazorVueParser` 是无状态的 partial class
- 所有方法都是静态或实例方法，不共享可变状态
- 每次调用 `Parse()` 都创建新的 `JazorVueDocument` 实例

**线程安全保证**：
- 多个线程可以同时调用 `Parse()` 方法
- 正则表达式使用 `RegexOptions.Compiled`，是线程安全的
- 无共享缓存或全局状态

## 5. 错误处理

### 5.1 参数验证

```csharp
if (filePath is null)
    throw new ArgumentNullException(nameof(filePath));
if (sourceText is null)
    throw new ArgumentNullException(nameof(sourceText));
```

### 5.2 格式验证

**未闭合的 `@code` 块**：
```csharp
if (!codeDirective.IsClosed)
{
    throw new FormatException("The .jazor document contains an unterminated @code block.");
}
```

### 5.3 遗留语法诊断

**检测位置**：`JazorImportDirectiveLocator.EnumerateLegacyDirectives()`

**诊断代码**：`LegacyImportDirectiveCatalog.DiagnosticCode`

**诊断消息**：`LegacyImportDirectiveCatalog.CreateDiagnosticMessage(occurrence.Kind)`

**严重级别**：`DiagnosticSeverityKind.Error`

## 6. 配置选项

### 6.1 正则表达式配置

所有正则表达式都使用 `RegexOptions.Compiled` 以提升性能：
- `TemplatePattern`：模板标签匹配
- `ComponentTagPattern`：组件标签匹配（`JazorMarkupPatterns`）
- `ModuleDirectivePattern`：`@module` 指令解析
- `LegacyDirectivePattern`：遗留指令检测
- `NamespaceBindingPattern`、`NamedBindingPattern`：绑定解析

### 6.2 文件系统搜索路径

**实现**：`JazorVueParser.GetSearchDirectories()`

**搜索顺序**：
1. 文档所在目录
2. `文档目录/Components/`
3. `文档目录/components/`
4. 父目录
5. `父目录/Components/`
6. `父目录/components/`

**去重**：使用 `HashSet<string>` 和 `StringComparer.OrdinalIgnoreCase`（Windows）或 `StringComparer.Ordinal`（非 Windows）

## 7. 与其他子系统的交互

### 7.1 与编译器交互

**下游消费者**：`JazorVueCompiler`

**数据流**：
```
.jazor 源文本
    ↓
JazorVueParser.Parse()
    ↓
JazorVueDocument (结构化模型)
    ↓
JazorVueCompiler.Compile()
    ↓
生成的 Vue SFC + 外部声明
```

### 7.2 与投影服务交互

**消费者**：`JazorProjectionService`

**用途**：
- 创建虚拟 Vue 文档投影（用于 Volar/LSP）
- 创建 C# 代码投影（用于 Roslyn 语义分析）

**关键方法**：
```csharp
var parsedDocument = _parser.Parse(document.DocumentPath, document.Text);
var compilation = _compiler.Compile(parsedDocument);
```

### 7.3 与分析服务交互

**消费者**：`FallbackJazorAnalysisService`

**用途**：当 RPC 分析服务不可用时，使用进程内解析器作为后备方案

**遥测报告**：
```csharp
FallbackTelemetry.ReportActivation(
    component: "analysisService",
    mode: "inProcFallback",
    reason: "analysis-rpc-unavailable",
    documentPath: request.JazorDocument.DocumentPath);
```

### 7.4 与 Code Directive Locator 交互

**共享组件**：`RazorBlockDirectiveLocator`

**职责**：
- 通用的 Razor 块指令定位（`@code`、`@functions`、`@using` 等）
- 字符串字面量和注释跳过
- 括号匹配和闭合验证

**复用场景**：
- `JazorCodeDirectiveLocator`：`@code` 指令定位
- `JazorImportDirectiveLocator`：排除 `@code` 块内的导入指令

## 8. 设计权衡

### 8.1 两种模板模式

**设计决策**：支持显式 `<template>` 标签和隐式 markup

**权衡**：
- **优点**：
  - 显式模式：与标准 Vue SFC 一致，清晰的边界
  - 隐式模式：减少冗余标签，更像 Razor/RazorVue 体验
- **缺点**：
  - 解析逻辑复杂度增加
  - 需要启发式算法确定模板边界

**选择理由**：渐进式迁移路径，允许现有 RazorVue 代码逐步迁移到 .jazor

### 8.2 导入推断 vs 显式导入

**设计决策**：自动推断 PascalCase 组件的导入，保留显式导入优先级

**权衡**：
- **优点**：
  - 减少样板代码
  - 与 Vue 生态系统工具（如 `unplugin-vue-components`）一致
- **缺点**：
  - 隐式行为可能让开发者困惑
  - 文件系统扫描增加 I/O 开销

**选择理由**：提升开发体验，显式导入提供覆盖机制

### 8.3 遗留语法支持

**设计决策**：通过诊断报告错误，但继续解析遗留语法（`@import`、`@jsimport`、`@vueimport`）

**权衡**：
- **优点**：
  - 渐进式升级路径
  - 向后兼容性
- **缺点**：
  - 维护两套解析逻辑
  - 可能延长技术债务

**选择理由**：降低现有代码库的迁移成本

### 8.4 字符串和注释跳过

**设计决策**：实现完整的 C# 字符串字面量和注释跳过逻辑

**权衡**：
- **优点**：
  - 准确识别指令边界
  - 避免误报（如字符串中的 `@code`）
- **缺点**：
  - 实现复杂度高（约 200 行代码）
  - 需要跟上 C# 语言演进

**选择理由**：正确性优先，减少语法边缘情况的错误

### 8.5 位置追踪

**设计决策**：在 `JazorVueDocument` 中保留所有索引和长度信息

**权衡**：
- **优点**：
  - 支持精确的源映射生成
  - 支持投影服务的内容匹配启发式
  - 支持 LSP 诊断的精确位置报告
- **缺点**：
  - 内存开销增加（每个文档多存储 4 个整数）
  - 解析逻辑需要额外计算索引

**选择理由**：源映射和 LSP 集成是关键功能，值得额外开销

## 9. 附录：关键常量

```csharp
// JazorVueParser
private static readonly Regex TemplatePattern = new Regex(
    @"<template>(?<content>[\s\S]*?)</template>",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

// JazorMarkupPatterns
internal static readonly Regex ComponentTagPattern = new Regex(
    @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
    RegexOptions.Compiled);

// JazorImportDirectiveLocator
private static readonly Regex ModuleDirectivePattern = new Regex(
    @"^@module\s+(?<clause>.+?)\s+from\s+(?<quote>[""'])(?<source>[^""']+)\k<quote>\s*$",
    RegexOptions.Compiled);

private static readonly Regex LegacyDirectivePattern = new Regex(
    @"^@(?<kind>import|jsimport|vueimport)\b.*$",
    RegexOptions.Compiled);

// JazorCodeDirectiveLocator
private static readonly string[] CodeDirectives = ["@code"];
```

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
