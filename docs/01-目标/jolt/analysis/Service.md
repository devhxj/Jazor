# Jazor 分析服务（Analysis Service）

> 状态：已实现
> 定位：Jolt 分析层的核心接口和实现，提供 .jazor 文档的语义分析能力

## 1. 文档定位

本文档描述 Jazor 分析服务，该服务负责分析 .jazor 文档并提供：
1. 诊断信息（Diagnostics）
2. 导入符号（Imports）
3. 编译产物（Artifacts）
4. Source Maps

## 2. 核心类型

### 2.1 `IVueAnalysisClient`

**文件路径**：`src/Jolt/Analysis/IVueAnalysisClient.cs`

**接口定义**：
```csharp
public interface IVueAnalysisClient
{
    ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}
```

**职责**：定义分析服务的统一接口

**实现类**：
- `JazorVueAnalysisService`：默认进程内实现
- `RpcVueAnalysisClient`：RPC 客户端实现

### 2.2 `IVueAnalysisRpcService`

**文件路径**：`src/Jolt/Analysis/IVueAnalysisRpcService.cs`（推断）

**接口定义**：
```csharp
public interface IVueAnalysisRpcService
{
    // RPC 服务端接口，用于处理来自 RpcVueAnalysisClient 的请求
}
```

**职责**：定义 RPC 服务端接口（用于子进程分析服务）

**实现类**：`JazorVueAnalysisService` 同时实现 `IVueAnalysisClient` 和 `IVueAnalysisRpcService`

### 2.3 `JazorVueAnalysisService`

**文件路径**：`src/Jolt/Analysis/JazorVueAnalysisService.cs`

**实现**：
```csharp
public sealed class JazorVueAnalysisService : IVueAnalysisClient, IVueAnalysisRpcService
{
    private readonly FallbackJazorAnalysisService _fallback = new();

    public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
        => _fallback.AnalyzeJazorAsync(request, cancellationToken);
}
```

**职责**：
- 实现 `IVueAnalysisClient` 接口
- 委托给 `FallbackJazorAnalysisService` 执行实际分析
- 同时实现 `IVueAnalysisRpcService`（用于 RPC 服务端）

**设计模式**：
- **外观模式（Facade）**：简化接口
- **委托模式（Delegation）**：委托给后备服务

### 2.4 `FallbackJazorAnalysisService`

**文件路径**：`src/Jolt/Analysis/FallbackJazorAnalysisService.cs`

**职责**：进程内分析服务的默认实现

**核心方法**：
```csharp
public ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
    AnalyzeJazorRequest request,
    CancellationToken cancellationToken)
```

**依赖**：
- `JazorVueParser`：解析 .jazor 源文本
- `JazorVueCompiler`：编译为 Vue SFC 和外部声明
- `LegacyImportDirectiveCatalog`：检测遗留导入语法

## 3. 核心算法

### 3.1 分析流程

**实现**：`FallbackJazorAnalysisService.AnalyzeJazorAsync()`

**分析步骤**：

1. **遥测报告**（可选）：
   ```csharp
   FallbackTelemetry.ReportActivation(
       component: "analysisService",
       mode: "inProcFallback",
       reason: "analysis-rpc-unavailable",
       documentPath: request.JazorDocument.DocumentPath);
   ```

2. **解析文档**：
   ```csharp
   var document = _parser.Parse(
       request.JazorDocument.DocumentPath,
       request.JazorDocument.Text);
   ```

3. **编译文档**：
   ```csharp
   var compilation = _compiler.Compile(document);
   ```

4. **收集诊断**：
   ```csharp
   var diagnostics = new List<DiagnosticRecord>(compilation.Diagnostics.Count + 4);
   diagnostics.AddRange(compilation.Diagnostics
       .Select((message, index) => new DiagnosticRecord(
           id: $"JAZORVUE{index + 1:000}",
           severity: DiagnosticSeverityKind.Warning,
           message: message,
           documentPath: request.JazorDocument.DocumentPath,
           start: 0,
           length: 0)));
   diagnostics.AddRange(LegacyImportDirectiveCatalog.FindOccurrences(request.JazorDocument.Text)
       .Select(occurrence => new DiagnosticRecord(
           id: LegacyImportDirectiveCatalog.DiagnosticCode,
           severity: DiagnosticSeverityKind.Error,
           message: LegacyImportDirectiveCatalog.CreateDiagnosticMessage(occurrence.Kind),
           documentPath: request.JazorDocument.DocumentPath,
           start: occurrence.Start,
           length: occurrence.Length)));
   ```

5. **收集导入**：
   ```csharp
   var imports = document.Imports
       .SelectMany(import => import.Bindings.Select(binding => new ImportDescriptor(
           localName: binding.LocalName,
           source: import.Source,
           importKind: MapImportKind(import.Kind),
           bindingKind: MapBindingKind(binding.BindingKind),
           importedName: binding.ImportedName,
           templateVisible: import.Kind == JazorImportKind.VueImport)))
       .ToArray();
   ```

6. **创建制品**：
   ```csharp
   var artifacts = new[]
   {
       new ArtifactRecord(
           artifactName: "virtual:" + request.JazorDocument.DocumentPath + ".vue",
           artifactKind: "vue-sfc",
           content: compilation.GeneratedVueText,
           contentHash: null),
       new ArtifactRecord(
           artifactName: "virtual:" + request.JazorDocument.DocumentPath + ".externals.g.cs",
           artifactKind: "csharp-externals",
           content: compilation.GeneratedExternalDeclarationsText,
           contentHash: null)
   };
   ```

7. **创建 Source Maps**：
   ```csharp
   var sourceMaps = new[]
   {
       new SourceMapDescriptor(
           sourcePath: request.JazorDocument.DocumentPath,
           generatedPath: vueArtifact.ArtifactName,
           sourceStart: 0,
           sourceLength: request.JazorDocument.Text.Length,
           generatedStart: 0,
           generatedLength: vueArtifact.Content.Length),
       new SourceMapDescriptor(
           sourcePath: request.JazorDocument.DocumentPath,
           generatedPath: externalsArtifact.ArtifactName,
           sourceStart: 0,
           sourceLength: request.JazorDocument.Text.Length,
           generatedStart: 0,
           generatedLength: externalsArtifact.Content.Length)
   };
   ```

8. **返回响应**：
   ```csharp
   return ValueTask.FromResult(new AnalyzeJazorResponse(
       diagnostics: diagnostics,
       imports: imports,
       artifacts: artifacts,
       sourceMaps: sourceMaps));
   ```

### 3.2 类型映射

**导入类型映射**：
```csharp
private static ImportKind MapImportKind(JazorImportKind importKind)
    => importKind == JazorImportKind.VueImport
        ? ImportKind.VueImport
        : ImportKind.JSImport;
```

**绑定类型映射**：
```csharp
private static ImportBindingKind MapBindingKind(JazorImportBindingKind bindingKind)
    => bindingKind switch
    {
        JazorImportBindingKind.Default => ImportBindingKind.Default,
        JazorImportBindingKind.Namespace => ImportBindingKind.Namespace,
        _ => ImportBindingKind.Named
    };
```

## 4. 线程安全模型

**实例级别线程安全**：
- `JazorVueAnalysisService` 是 sealed class
- 每个实例持有独立的 `_fallback` 实例
- 方法调用不共享可变状态

**FallbackJazorAnalysisService 线程安全**：
- `JazorVueParser` 和 `JazorVueCompiler` 是无状态的
- 每次调用创建新的实例（`new()`）
- 无共享缓存或全局状态

**线程安全保证**：
- 多个线程可以同时调用 `AnalyzeJazorAsync()`
- 每次调用独立执行，无竞态条件
- 异常不影响后续调用

## 5. 错误处理

### 5.1 参数验证

```csharp
ArgumentNullException.ThrowIfNull(request);
cancellationToken.ThrowIfCancellationRequested();
```

### 5.2 遥测异常隔离

```csharp
try
{
    FallbackTelemetry.ReportActivation(...);
}
catch (Exception)
{
    // Telemetry must not break the fallback analysis path.
}
```

**设计原则**：遥测不应破坏分析路径

### 5.3 制品验证

```csharp
var vueArtifact = artifacts.FirstOrDefault(static artifact => artifact.ArtifactKind == "vue-sfc");
var externalsArtifact = artifacts.FirstOrDefault(static artifact => artifact.ArtifactKind == "csharp-externals");
if (vueArtifact is null || externalsArtifact is null)
{
    throw new InvalidOperationException("Fallback analysis did not produce the expected virtual artifacts.");
}
```

## 6. 配置选项

### 6.1 遥测报告

**组件名称**：`"analysisService"`
**模式**：`"inProcFallback"`
**原因**：`"analysis-rpc-unavailable"`

**触发条件**：当 RPC 分析服务不可用时，使用进程内后备服务

### 6.2 诊断 ID 格式

**编译诊断**：
```csharp
id: $"JAZORVUE{index + 1:000}"
// 示例：JAZORVUE001, JAZORVUE002, ...
```

**遗留导入诊断**：
```csharp
id: LegacyImportDirectiveCatalog.DiagnosticCode
```

### 6.3 制品命名

**Vue SFC**：
```csharp
artifactName: "virtual:" + documentPath + ".vue"
// 示例：virtual:Components/MyComponent.jazor.vue
```

**C# 外部声明**：
```csharp
artifactName: "virtual:" + documentPath + ".externals.g.cs"
// 示例：virtual:Components/MyComponent.jazor.externals.g.cs
```

**制品类型**：
- `"vue-sfc"`：Vue Single File Component
- `"csharp-externals"`：C# 外部声明代码

## 7. 与其他子系统的交互

### 7.1 与解析器和编译器交互

**依赖**：
```csharp
private readonly JazorVueParser _parser = new();
private readonly JazorVueCompiler _compiler = new();
```

**数据流**：
```
AnalyzeJazorRequest (JazorDocument)
    ↓
JazorVueParser.Parse()
    ↓
JazorVueDocument
    ↓
JazorVueCompiler.Compile()
    ↓
JazorVueCompilationResult (GeneratedVueText, GeneratedExternalDeclarationsText, Diagnostics)
    ↓
AnalyzeJazorResponse
```

### 7.2 与 RPC 客户端交互

**被替代者**：`RpcVueAnalysisClient`

**选择逻辑**：`VueAnalysisClientFactory.Create()`
```csharp
public static IVueAnalysisClient Create(string[] args)
{
    // 解析 CLI 参数
    string? command = null;
    string? arguments = null;

    foreach (var arg in args)
    {
        if (arg.StartsWith("--analysis-command=", StringComparison.OrdinalIgnoreCase))
        {
            command = TryReadOptionValue(arg, "--analysis-command");
        }
        else if (arg.StartsWith("--analysis-args=", StringComparison.OrdinalIgnoreCase))
        {
            arguments = TryReadOptionValue(arg, "--analysis-args");
        }
    }

    return !string.IsNullOrWhiteSpace(command)
        ? CreateFromTransport(new ProcessAnalysisRpcTransport(command, arguments))
        : CreateDefault();
}
```

**默认实现**：
```csharp
public static IVueAnalysisClient CreateDefault()
    => new JazorVueAnalysisService();
```

### 7.3 与 LSP 服务交互

**消费者**：`LspSession`、`JoltWorkspaceResolver`

**用途**：
1. **诊断报告**：
   - 编译警告（JAZORVUE001-999）
   - 遗留导入错误

2. **符号查询**：
   - 导入符号列表
   - 模板可见性（`templateVisible`）

3. **制品生成**：
   - 虚拟 Vue 文档（用于 Volar）
   - 虚拟 C# 文档（用于 Roslyn）

4. **Source Maps**：
   - 调试支持
   - 错误位置映射

### 7.4 与 DevServer 交互

**消费者**：`OnDemandCompiler`、`ChangeProcessor`

**用途**：
- 实时分析 .jazor 文件
- 生成虚拟文档用于开发服务器
- 诊断报告（实时反馈）

### 7.5 与 Build Orchestrator 交互

**消费者**：`BuildOrchestrator.RuntimeAndIncremental`

**用途**：
- 增量分析 .jazor 文件
- 生成最终制品
- Source Map 生成

## 8. 设计权衡

### 8.1 进程内 vs 进程外分析

**设计决策**：默认使用进程内分析（`FallbackJazorAnalysisService`），支持进程外 RPC 分析（`RpcVueAnalysisClient`）

**权衡**：
- **进程内（Fallback）**：
  - 优点：快速启动、无 IPC 开销、调试简单
  - 缺点：共享进程空间、崩溃影响主进程、无法隔离资源

- **进程外（RPC）**：
  - 优点：隔离崩溃、独立资源管理、支持不同语言实现
  - 缺点：IPC 开销、启动延迟、调试复杂

**选择理由**：
- 默认进程内提供快速开发体验
- RPC 支持生产环境的稳定性需求
- 可通过 CLI 参数切换

### 8.2 双接口实现（IVueAnalysisClient + IVueAnalysisRpcService）

**设计决策**：`JazorVueAnalysisService` 同时实现客户端和服务端接口

**权衡**：
- **优点**：
  - 同一实现可用于两种场景
  - 减少代码重复
  - 简化测试
- **缺点**：
  - 职责不清（既是客户端又是服务端）
  - 可能违反单一职责原则

**选择理由**：
- 分析逻辑相同，仅调用方式不同
- 简化架构（无需独立的服务端包装）
- 后期可重构分离

### 8.3 遥测集成

**设计决策**：在 `FallbackJazorAnalysisService` 中集成遥测报告

**权衡**：
- **优点**：
  - 监控后备服务激活频率
  - 识别 RPC 服务配置问题
  - 数据驱动优化
- **缺点**：
  - 增加依赖（`FallbackTelemetry`）
  - 遥测失败不应破坏分析路径

**选择理由**：
- 可观测性是关键需求
- 异常隔离确保健壮性
- 可选功能（不影响核心逻辑）

### 8.4 诊断分离

**设计决策**：收集编译诊断和遗留导入诊断，统一返回

**权衡**：
- **优点**：
  - 统一的诊断接口
  - 完整的问题视图
- **缺点**：
  - 诊断来源不同（编译器 vs 解析器）
  - 诊断 ID 格式不统一

**选择理由**：
- LSP 需要统一的诊断流
- 消费者无需区分诊断来源
- 可通过 ID 前缀区分

### 8.5 制品内联

**设计决策**：将生成的 Vue SFC 和 C# 外部声明作为制品内联返回

**权衡**：
- **优点**：
  - 一次调用获取所有产物
  - 减少 IPC 开销（RPC 模式）
  - 简化调用方逻辑
- **缺点**：
  - 响应大小增加
  - 内存占用增加

**选择理由**：
- 制品通常较小（<100KB）
- 网络传输不是瓶颈（本地 IPC）
- 简化集成（无需多次调用）

## 9. 附录：请求/响应契约

### 9.1 AnalyzeJazorRequest

```csharp
public sealed class AnalyzeJazorRequest
{
    public JazorDocumentSnapshot JazorDocument { get; }
    // JazorDocumentSnapshot 包含：
    // - DocumentPath: string
    // - Text: string
}
```

### 9.2 AnalyzeJazorResponse

```csharp
public sealed class AnalyzeJazorResponse
{
    public IReadOnlyList<DiagnosticRecord> Diagnostics { get; }
    public IReadOnlyList<ImportDescriptor> Imports { get; }
    public IReadOnlyList<ArtifactRecord> Artifacts { get; }
    public IReadOnlyList<SourceMapDescriptor> SourceMaps { get; }
}
```

**DiagnosticRecord**：
```csharp
public sealed class DiagnosticRecord
{
    public string Id { get; }
    public DiagnosticSeverityKind Severity { get; }
    public string Message { get; }
    public string DocumentPath { get; }
    public int Start { get; }
    public int Length { get; }
}
```

**ImportDescriptor**：
```csharp
public sealed class ImportDescriptor
{
    public string LocalName { get; }
    public string Source { get; }
    public ImportKind ImportKind { get; }
    public ImportBindingKind BindingKind { get; }
    public string? ImportedName { get; }
    public bool TemplateVisible { get; }
}
```

**ArtifactRecord**：
```csharp
public sealed class ArtifactRecord
{
    public string ArtifactName { get; }
    public string ArtifactKind { get; }
    public string Content { get; }
    public string? ContentHash { get; }
}
```

**SourceMapDescriptor**：
```csharp
public sealed class SourceMapDescriptor
{
    public string SourcePath { get; }
    public string GeneratedPath { get; }
    public int SourceStart { get; }
    public int SourceLength { get; }
    public int GeneratedStart { get; }
    public int GeneratedLength { get; }
}
```

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
