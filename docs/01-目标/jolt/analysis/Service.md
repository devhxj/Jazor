# Jazor 分析服务（Analysis Service）

Jolt 分析层的核心接口和实现，提供 .jazor 文档的语义分析能力：诊断信息、导入符号、编译产物、Source Maps。

## 核心类型

### `IVueAnalysisClient`

**文件路径**：`src/Jolt/Analysis/IVueAnalysisClient.cs`

```csharp
public interface IVueAnalysisClient
{
    ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}
```

分析服务的统一接口。实现类：`JazorVueAnalysisService`（默认进程内）和 `RpcVueAnalysisClient`（RPC 客户端）。

### `IVueAnalysisRpcService`

**文件路径**：`src/Jolt/Analysis/IVueAnalysisRpcService.cs`（推断）

RPC 服务端接口，用于处理来自 `RpcVueAnalysisClient` 的请求。`JazorVueAnalysisService` 同时实现 `IVueAnalysisClient` 和 `IVueAnalysisRpcService`。

### `JazorVueAnalysisService`

**文件路径**：`src/Jolt/Analysis/JazorVueAnalysisService.cs`

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

外观模式：简化接口，委托给 `FallbackJazorAnalysisService` 执行实际分析。同时实现 `IVueAnalysisRpcService` 用于 RPC 服务端场景。

### `FallbackJazorAnalysisService`

**文件路径**：`src/Jolt/Analysis/FallbackJazorAnalysisService.cs`

进程内分析服务的默认实现。依赖 `JazorVueParser`（解析 .jazor 源文本）、`JazorVueCompiler`（编译为 Vue SFC 和外部声明）、`LegacyImportDirectiveCatalog`（检测遗留导入语法）。

## 核心算法

### 分析流程

**实现**：`FallbackJazorAnalysisService.AnalyzeJazorAsync()`

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

### 类型映射

导入类型映射：
```csharp
private static ImportKind MapImportKind(JazorImportKind importKind)
    => importKind == JazorImportKind.VueImport
        ? ImportKind.VueImport
        : ImportKind.JSImport;
```

绑定类型映射：
```csharp
private static ImportBindingKind MapBindingKind(JazorImportBindingKind bindingKind)
    => bindingKind switch
    {
        JazorImportBindingKind.Default => ImportBindingKind.Default,
        JazorImportBindingKind.Namespace => ImportBindingKind.Namespace,
        _ => ImportBindingKind.Named
    };
```

## 线程安全模型

`JazorVueAnalysisService` 是 sealed class，每个实例持有独立的 `_fallback` 实例，方法调用不共享可变状态。

`FallbackJazorAnalysisService` 中 `JazorVueParser` 和 `JazorVueCompiler` 是无状态的，每次调用创建新实例，无共享缓存或全局状态。

多个线程可以同时调用 `AnalyzeJazorAsync()`，每次调用独立执行，异常不影响后续调用。

## 错误处理

参数验证：
```csharp
ArgumentNullException.ThrowIfNull(request);
cancellationToken.ThrowIfCancellationRequested();
```

遥测异常隔离（遥测不应破坏分析路径）：
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

制品验证：
```csharp
var vueArtifact = artifacts.FirstOrDefault(static artifact => artifact.ArtifactKind == "vue-sfc");
var externalsArtifact = artifacts.FirstOrDefault(static artifact => artifact.ArtifactKind == "csharp-externals");
if (vueArtifact is null || externalsArtifact is null)
{
    throw new InvalidOperationException("Fallback analysis did not produce the expected virtual artifacts.");
}
```

## 配置选项

遥测报告：组件名 `"analysisService"`，模式 `"inProcFallback"`，原因 `"analysis-rpc-unavailable"`。

诊断 ID 格式：编译诊断 `$"JAZORVUE{index + 1:000}"`（JAZORVUE001, JAZORVUE002...），遗留导入诊断 `LegacyImportDirectiveCatalog.DiagnosticCode`。

制品命名：
- Vue SFC：`"virtual:" + documentPath + ".vue"`（如 `virtual:Components/MyComponent.jazor.vue`）
- C# 外部声明：`"virtual:" + documentPath + ".externals.g.cs"`（如 `virtual:Components/MyComponent.jazor.externals.g.cs`）

制品类型：`"vue-sfc"`（Vue Single File Component）、`"csharp-externals"`（C# 外部声明代码）。

## 与其他子系统的交互

### 与解析器和编译器交互

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

### 与 RPC 客户端交互

选择逻辑：`VueAnalysisClientFactory.Create()` 根据 CLI 参数决定使用进程内还是进程外：
```csharp
public static IVueAnalysisClient Create(string[] args)
{
    // 解析 --analysis-command 和 --analysis-args
    // 有 command → CreateFromTransport(new ProcessAnalysisRpcTransport(command, arguments))
    // 无 command → CreateDefault() → new JazorVueAnalysisService()
}
```

### 与 LSP 服务交互

消费者：`LspSession`、`JoltWorkspaceResolver`。

用途：诊断报告（编译警告 JAZORVUE001-999、遗留导入错误）；符号查询（导入符号列表、模板可见性）；制品生成（虚拟 Vue 文档给 Volar、虚拟 C# 文档给 Roslyn）；Source Maps（调试支持、错误位置映射）。

### 与 DevServer 交互

消费者：`OnDemandCompiler`、`ChangeProcessor`。实时分析 .jazor 文件，生成虚拟文档，诊断报告实时反馈。

### 与 Build Orchestrator 交互

消费者：`BuildOrchestrator.RuntimeAndIncremental`。增量分析、生成最终制品、Source Map 生成。

## 设计权衡

### 进程内 vs 进程外分析

默认进程内（`FallbackJazorAnalysisService`），支持进程外 RPC（`RpcVueAnalysisClient`）。进程内快速启动、无 IPC 开销、调试简单，但共享进程空间。进程外隔离崩溃、独立资源管理、支持多语言，但有 IPC 开销。可通过 CLI 参数切换。

### 双接口实现

`JazorVueAnalysisService` 同时实现 `IVueAnalysisClient` 和 `IVueAnalysisRpcService`。分析逻辑相同仅调用方式不同，减少代码重复简化测试。后期可重构分离。

### 遥测集成

在 `FallbackJazorAnalysisService` 中集成遥测报告，监控后备服务激活频率、识别 RPC 配置问题。异常隔离确保遥测不破坏分析路径。

### 诊断分离

收集编译诊断和遗留导入诊断统一返回。LSP 需要统一的诊断流，消费者无需区分来源，可通过 ID 前缀区分。

### 制品内联

将 Vue SFC 和 C# 外部声明作为制品内联返回。一次调用获取所有产物，减少 IPC 开销，简化调用方逻辑。制品通常 <100KB，本地 IPC 场景下网络传输不是瓶颈。

## 附录：请求/响应契约

### AnalyzeJazorRequest

```csharp
public sealed class AnalyzeJazorRequest
{
    public JazorDocumentSnapshot JazorDocument { get; }
    // JazorDocumentSnapshot 包含：
    // - DocumentPath: string
    // - Text: string
}
```

### AnalyzeJazorResponse

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
