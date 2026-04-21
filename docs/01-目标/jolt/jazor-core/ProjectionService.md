# Jazor 投影服务（Projection Service）

> 状态：已实现
> 定位：Jazor 核心系统的投影层，负责将 .jazor 文档投影为虚拟 Vue 和 C# 文档供 LSP 和编译器使用

## 1. 文档定位

本文档描述 Jazor 投影服务，该服务负责将 .jazor 源文档转换为两种虚拟文档：
1. **虚拟 Vue 文档**：生成的 Vue SFC，供 Volar/LSP 使用
2. **虚拟 C# 文档**：桥接代码，供 Roslyn 语义分析使用

## 2. 核心类型

### 2.1 `JazorProjectionService`

**文件路径**：`src/Jolt/Jazor/Projection/JazorProjectionService.cs`

**职责**：将 .jazor 文档投影为虚拟文档

**核心方法**：
```csharp
public ValueTask<IReadOnlyList<VirtualDocument>> ProjectAsync(
    DocumentSnapshot document,
    CancellationToken cancellationToken)

public ValueTask<IReadOnlyList<VirtualDocument>> ProjectCodeAsync(
    DocumentSnapshot document,
    CancellationToken cancellationToken)
```

**依赖**：
- `JazorVueParser`：解析 .jazor 源文本
- `JazorVueCompiler`：编译为 Vue SFC
- `InProcRoslynCodeService`：创建 C# 投影

### 2.2 `VirtualDocument`

**文件路径**：`src/Jolt/VirtualDocuments/Models/VirtualDocument.cs`

**数据结构**：
```csharp
public sealed class VirtualDocument
{
    public VirtualDocumentIdentity Identity { get; }
    public string Text { get; }
    public ProjectionMap ProjectionMap { get; }
    public int Version { get; }
}
```

**虚拟文档类型**：
- `VirtualDocumentKind.Vue`：生成的 Vue SFC
- `VirtualDocumentKind.CSharp`：桥接的 C# 代码

### 2.3 `ProjectionMap`

**文件路径**：`src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs`

**数据结构**：
```csharp
public sealed class ProjectionMap
{
    public string SourceDocumentPath { get; }
    public string ProjectedDocumentPath { get; }
    public IReadOnlyList<ProjectionSegment> Segments { get; }
}

public readonly struct ProjectionSegment
{
    public int SourceStart { get; }
    public int SourceLength { get; }
    public int ProjectedStart { get; }
    public int ProjectedLength { get; }
}
```

**用途**：维护源文档与投影文档之间的位置映射关系

## 3. 核心算法

### 3.1 完整投影（ProjectAsync）

**实现**：`JazorProjectionService.ProjectAsync()`

**投影流程**：
1. 解析 .jazor 源文本为 `JazorVueDocument`
2. 编译为生成的 Vue 文本和外部声明
3. 创建虚拟 Vue 文档投影
4. 创建虚拟 C# 文档投影
5. 返回投影文档列表

**输出**：
```csharp
IReadOnlyList<VirtualDocument>
{
    VirtualDocument (Vue),
    VirtualDocument (CSharp)
}
```

### 3.2 仅代码投影（ProjectCodeAsync）

**实现**：`JazorProjectionService.ProjectCodeAsync()`

**投影流程**：
1. 解析 .jazor 源文本为 `JazorVueDocument`
2. 调用 `InProcRoslynCodeService.CreateProjection()` 生成 C# 投影
3. 返回仅包含 C# 投影的列表

**使用场景**：
- 仅需 C# 语义分析（如 Go to Definition）
- 无需 Vue SFC 生成

### 3.3 Vue 投影创建

**实现**：`CreateVueProjectionDocument()`

**虚拟路径**：
```csharp
var vueProjectedPath = "virtual:" + document.DocumentPath + ".g.vue";
```

**投影映射创建**：`CreateVueProjectionMap()`

#### 3.3.1 模板内容映射

**启发式算法**：
```csharp
private static bool TryFindGeneratedTemplateContentStart(
    string generatedVueText,
    string templateText,
    out int generatedStart)
```

**匹配步骤**：
1. 查找最后一个 `<template>` 标签位置
2. 从标签后搜索模板内容
3. 验证内容在 `</template>` 标签内

**位置验证**：
```csharp
var closeTagIndex = generatedVueText.IndexOf(TemplateCloseTag, generatedStart, StringComparison.OrdinalIgnoreCase);
return closeTagIndex >= generatedStart;
```

**映射创建**：
```csharp
segments.Add(new ProjectionSegment(
    document.TemplateStartIndex,        // 源文档中的模板起始位置
    document.TemplateLength,             // 源文档中的模板长度
    generatedStart,                      // 生成文档中的模板起始位置
    document.Template.Length));          // 生成文档中的模板长度
```

#### 3.3.2 Code 内容映射

**启发式算法**：
```csharp
private static bool TryFindGeneratedCodeCommentStart(
    string generatedVueText,
    string codeText,
    out int generatedStart)
```

**匹配步骤**：
1. 查找注释标记：`"Original @code block retained for bridge diagnostics:"`
2. 从标记后搜索 code 内容
3. 验证 code 不包含 `*/`（避免注释冲突）

**前置条件**：
```csharp
if (!document.Code.Contains("*/", StringComparison.Ordinal))
{
    // 只有在 code 不包含 */ 时才尝试映射
}
```

**映射创建**：
```csharp
segments.Add(new ProjectionSegment(
    document.CodeStartIndex,             // 源文档中的 code 起始位置
    document.CodeLength,                 // 源文档中的 code 长度
    generatedStart,                      // 生成文档中的 code 起始位置
    document.Code.Length));              // 生成文档中的 code 长度
```

### 3.4 C# 投影创建

**实现**：`CreateCodeProjectionDocument()`

**委托给 Roslyn 服务**：
```csharp
var csharpProjection = _inProcRoslynCodeService.CreateProjection(document, parsedDocument);
```

**InProcRoslynCodeService.CreateProjection()** 返回：
- `ProjectedDocumentPath`：虚拟 C# 文档路径
- `SourceText`：生成的 C# 代码
- `ProjectionMap`：源文档到 C# 代码的映射

## 4. 线程安全模型

**实例级别线程安全**：
- `JazorProjectionService` 是 sealed class
- 每个实例持有独立的 `_parser`、`_compiler`、`_inProcRoslynCodeService`
- 方法调用不共享可变状态

**线程安全保证**：
- 多个线程可以同时调用 `ProjectAsync()` 和 `ProjectCodeAsync()`
- 底层组件（Parser、Compiler、RoslynService）都是线程安全的
- 每次调用创建新的 `VirtualDocument` 实例

## 5. 错误处理

### 5.1 参数验证

```csharp
ArgumentNullException.ThrowIfNull(document);
cancellationToken.ThrowIfCancellationRequested();
```

### 5.2 文档类型验证

```csharp
if (document.DocumentKind != DocumentKind.Jazor)
{
    return ValueTask.FromResult<IReadOnlyList<VirtualDocument>>(Array.Empty<VirtualDocument>());
}
```

**行为**：非 Jazor 文档返回空列表（不抛出异常）

### 5.3 启发式匹配失败

**模板匹配失败**：
```csharp
if (string.IsNullOrEmpty(templateText))
{
    return false;  // 不创建映射段
}
```

**Code 匹配失败**：
```csharp
if (string.IsNullOrEmpty(codeText))
{
    return false;  // 不创建映射段
}
```

**行为**：
- 不创建投影段
- 生成的 `ProjectionMap.Segments` 为空或部分
- LSP 功能降级（但不会崩溃）

## 6. 配置选项

### 6.1 虚拟路径格式

**Vue 投影**：
```csharp
"virtual:" + document.DocumentPath + ".g.vue"
// 示例：virtual:Components/MyComponent.jazor.g.vue
```

**C# 投影**：
```csharp
// 由 InProcRoslynCodeService.CreateProjection() 决定
// 典型格式：virtual:Components/MyComponent.jazor.g.cs
```

### 6.2 投影映射常量

```csharp
private const string TemplateOpenTag = "<template>";
private const string TemplateCloseTag = "</template>";
private const string CodeCommentMarker = "Original @code block retained for bridge diagnostics:";
```

## 7. 与其他子系统的交互

### 7.1 与解析器和编译器交互

**依赖注入**：
```csharp
private readonly JazorVueParser _parser = new();
private readonly JazorVueCompiler _compiler = new();
```

**调用流程**：
```
DocumentSnapshot (.jazor)
    ↓
JazorVueParser.Parse()
    ↓
JazorVueDocument
    ↓
JazorVueCompiler.Compile()
    ↓
JazorVueCompilationResult (GeneratedVueText)
    ↓
CreateVueProjectionDocument()
    ↓
VirtualDocument (Vue)
```

### 7.2 与 InProcRoslynCodeService 交互

**依赖注入**：
```csharp
private readonly InProcRoslynCodeService _inProcRoslynCodeService;

public JazorProjectionService(InProcRoslynCodeService? inProcRoslynCodeService = null)
{
    _inProcRoslynCodeService = inProcRoslynCodeService ?? new InProcRoslynCodeService();
}
```

**调用方法**：
```csharp
var csharpProjection = _inProcRoslynCodeService.CreateProjection(document, parsedDocument);
```

**InProcRoslynCodeService.CreateProjection()** 返回：
- `ProjectedDocumentPath`：虚拟 C# 文档路径
- `SourceText`：桥接的 C# 代码（包含类定义、成员等）
- `ProjectionMap`：源文档到 C# 代码的映射

**桥接代码特点**：
- 将 `@code` 块中的 C# 代码包装为类定义
- 为 `[Prop]`、`[State]`、`[Computed]`、`[Method]` 生成对应的 C# 成员
- 支持完整的 Roslyn 语义分析（Go to Definition、Find References 等）

### 7.3 与 LSP 服务交互

**消费者**：`LspSession`、`JoltWorkspaceResolver`

**用途**：
1. **Vue LSP 功能**：
   - 语法高亮（通过虚拟 Vue 文档）
   - 模板补全（通过 Volar）
   - Vue 特定诊断

2. **C# LSP 功能**：
   - Go to Definition（通过 Roslyn 语义分析）
   - Find References（通过 Roslyn 符号查找）
   - Rename（通过 Roslyn 重构）
   - Hover 信息（通过 Roslyn 符号信息）
   - Completion（通过 Roslyn 符号查找）

**投影查询流程**：
```
LSP 请求 (position)
    ↓
LspSession 确定投影类型（Vue 或 C#）
    ↓
ProjectionService.ProjectAsync() 或 ProjectCodeAsync()
    ↓
VirtualDocument + ProjectionMap
    ↓
投影位置映射 (Source → Projected)
    ↓
LSP 请求处理（Volar 或 Roslyn）
    ↓
结果位置映射 (Projected → Source)
    ↓
返回 LSP 响应
```

### 7.4 与 DevServer 交互

**消费者**：`OnDemandCompiler`、`ChangeProcessor`

**用途**：
- 实时编译 .jazor 文件
- 生成虚拟 Vue 文档用于开发服务器
- 支持 Hot Module Replacement (HMR)

**HMR 集成**：
```csharp
// JazorVueCompilationResult.HotReload
public sealed class JazorVueHotReloadMetadata
{
    public string DescriptorSignature { get; }
    public string TemplateSignature { get; }
    public string LogicSignature { get; }
    public RazorVueHmrBoundaryKind HmrBoundaryKind { get; }
}
```

### 7.5 与 Build Orchestrator 交互

**消费者**：`BuildOrchestrator.RuntimeAndIncremental`

**用途**：
- 增量构建 .jazor 文件
- 生成最终输出（Vue SFC + C# 外部声明）
- Source Map 生成

**构建流程**：
```
.jazor 源文件
    ↓
ProjectionService.ProjectAsync()
    ↓
VirtualDocument (Vue) + VirtualDocument (C#)
    ↓
BuildOrchestrator
    ↓
输出产物：
  - Component.vue (最终 Vue SFC)
  - Component.externals.g.cs (C# 外部声明)
  - Component.vue.map (Source Map)
```

## 8. 设计权衡

### 8.1 启发式映射 vs 精确映射

**设计决策**：使用启发式算法（内容匹配）而非精确位置追踪

**权衡**：
- **优点**：
  - 实现简单（约 50 行代码）
  - 不依赖编译器内部状态
  - 容错性强（格式变化不影响）
- **缺点**：
  - 可能匹配失败（返回空映射）
  - 依赖生成代码的稳定性
  - 无法处理重复内容

**选择理由**：
- 模板和 code 内容通常唯一
- 标记字符串（如诊断注释）提供强锚点
- 失败时降级（不影响其他功能）

### 8.2 双投影 vs 单投影

**设计决策**：同时生成 Vue 和 C# 两种投影

**权衡**：
- **优点**：
  - 完整的 LSP 支持（Vue + C#）
  - 灵活的查询路径（ProjectCodeAsync 仅需 C#）
  - 清晰的职责分离
- **缺点**：
  - 双倍内存开销（两个虚拟文档）
  - 双倍编译时间（Parser + Compiler 两次）

**选择理由**：
- Vue 和 C# 的语义模型完全不同
- LSP 需要同时支持两种语言服务
- 按需投影（ProjectCodeAsync）减少开销

### 8.3 虚拟路径 vs 物理路径

**设计决策**：使用 `virtual:` 前缀的虚拟路径

**权衡**：
- **优点**：
  - 避免与物理文件冲突
  - 清晰的命名约定
  - 易于过滤和处理
- **缺点**：
  - 不符合标准文件系统路径
  - 需要特殊处理（LSP、构建器）

**选择理由**：
- 虚拟文档不应写入磁盘
- `virtual:` 前缀广泛使用（Vite、Volar）
- 易于在工具链中识别

### 8.4 完整投影 vs 增量投影

**设计决策**：每次调用 `ProjectAsync()` 都完整投影，而非增量更新

**权衡**：
- **优点**：
  - 实现简单（无状态）
  - 线程安全（无共享状态）
  - 结果可预测（无缓存污染）
- **缺点**：
  - 重复计算开销
  - 无法复用之前的投影结果

**选择理由**：
- 解析和编译速度足够快（<10ms）
- 避免缓存失效的复杂性
- LSP 会话层可以缓存投影结果

### 8.5 Code 内容映射的条件性

**设计决策**：仅在 code 不包含 `*/` 时才创建映射

**权衡**：
- **优点**：
  - 避免注释边界冲突
  - 防止映射错误
- **缺点**：
  - 部分场景无映射
  - 不对称的行为（模板始终映射）

**选择理由**：
- `*/` 会终止生成的注释块
- 无法安全映射（会破坏 Vue SFC）
- 降级总比错误映射好

## 9. 附录：投影映射示例

### 9.1 Vue 投影映射

**源文档**（`Component.jazor`）：
```csharp
@module { MyButton } from "./MyButton.vue"

<template>
  <MyButton />
</template>

@code {
  [Prop]
  public string Title { get; set; }
}
```

**虚拟 Vue 文档**（`virtual:Component.jazor.g.vue`）：
```vue
<script setup>
import { MyButton } from "./MyButton.vue";
import { toRef } from "vue";

const props = defineProps({
  title: String,
});
const title = toRef(props, "title");
</script>

<template>
  <MyButton />
</template>
```

**投影映射**：
```
Source: Line 3-5 (Template)
  → Projected: Line 9-11 (Template)

Source: Line 9-13 (Code)
  → Projected: Line 6-7 (Props definition)
```

### 9.2 C# 投影映射

**源文档**（同上）

**虚拟 C# 文档**（`virtual:Component.jazor.g.cs`）：
```csharp
namespace Jazor.Generated
{
    public partial class Component_jazor
    {
        [Prop]
        public string Title { get; set; }
    }
}
```

**投影映射**：
```
Source: Line 9-13 (Code)
  → Projected: Line 4-6 (Property definition)
```

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
