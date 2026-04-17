# Phase 7: 扩展系统与生态完善 — 详细实施计划

## 目标

实现 VueHost 的可扩展分析器系统、高级 LSP 特性补充（Signature Help、Inlay Hints、Workspace Symbol、Folding Range）、VS Code 扩展集成、降级与容错机制完善，构建完整的开发生态。

**验收标准**:
- 自定义分析器插件可注册并提供诊断/代码操作
- Signature Help 在方法调用时显示参数信息
- Inlay Hints 显示类型注解和参数名
- Workspace Symbol 可搜索项目内所有符号
- Folding Range 支持 @code 块和组件标签折叠
- VS Code 扩展可一键启动 VueHost
- Lane 故障时自动降级，不影响其他功能

## 当前实现状态（2026-04-17）

### 已完成（最小闭环）

- 新增扩展核心接口与元数据模型：`IExtension` / `ExtensionMetadata` / `ExtensionContext`。
- 新增 LSP 扩展 provider 抽象：`ILspDiagnosticProvider`、`ILspCodeActionProvider` 及统一上下文对象。
- 新增扩展注册表与空实现：`ExtensionRegistry` / `NullExtensionRegistry`，支持 provider 按优先级注册与读取。
- 新增扩展加载器与配置解析：`ExtensionLoader` + `ExtensionHostOptionsResolver`，支持 builtin 与目录扩展加载（`extension.json`）。
- `LspSession` 已接入扩展 provider 聚合点：诊断发布链路与 codeAction 返回链路都会合并扩展结果，且 provider 失败不会中断主链路。
- `Program` 的 `--lsp` 启动路径已集成扩展加载与注入。
- 新增 Phase7 回归测试，覆盖注册表、加载器、options 解析，以及 diagnostics/codeAction 的端到端接入断言。

### 尚未完成

- completion / hover / references / rename / documentSymbol 等 provider 面尚未扩展。
- 扩展健康监控、超时隔离、权限约束与沙箱策略尚未实现。
- VS Code 扩展与生态层（市场、发布、安装）尚未落地。

---

## 一、扩展系统架构

### 1.1 设计原则

来自 vuehost-capabilities.md:

> - **不要**把自定义规则直接塞进 LSP handler
> - **要**定义稳定的分析插件接口，例如 `IJazorDiagnosticProvider`、`IJazorCodeActionProvider`
> - **要**让每个 provider 接收统一的文档快照、ProjectionMap、Lane 结果快照
> - **要**让 CodeAction 明确声明它修改的是 source document 还是某个虚拟投影片段

### 1.2 扩展点分类

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          VueHost Extension Points                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                        Analysis Extensions                            │   │
│  │                                                                       │   │
│  │  ┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐ │   │
│  │  │IJazorDiagnostic   │  │IJazorCodeAction   │  │IJazorCompletion   │ │   │
│  │  │Provider            │  │Provider            │  │Provider            │ │   │
│  │  │                   │  │                   │  │                   │ │   │
│  │  │ - 结构诊断        │  │ - Quick Fix       │  │ - 自定义补全      │ │   │
│  │  │ - 语义诊断        │  │ - Refactor        │  │ - 指令补全        │ │   │
│  │  │ - 跨 Lane 诊断    │  │ - 代码生成        │  │ - 组件补全        │ │   │
│  │  └───────────────────┘  └───────────────────┘  └───────────────────┘ │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                        Build Extensions                               │   │
│  │                                                                       │   │
│  │  ┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐ │   │
│  │  │IBuildPlugin       │  │ICompileHook       │  │IAssetProcessor    │ │   │
│  │  │                   │  │                   │  │                   │ │   │
│  │  │ - 自定义打包      │  │ - 编译前后处理    │  │ - 资源后处理      │ │   │
│  │  │ - 插件钩子        │  │ - 代码转换        │  │ - 自定义输出      │ │   │
│  │  └───────────────────┘  └───────────────────┘  └───────────────────┘ │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                        Dev Server Extensions                          │   │
│  │                                                                       │   │
│  │  ┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐ │   │
│  │  │IMiddleware        │  │IVirtualModule     │  │IProxyHandler      │ │   │
│  │  │                   │  │                   │  │                   │ │   │
│  │  │ - 自定义中间件    │  │ - 虚拟模块        │  │ - 代理扩展        │ │   │
│  │  │ - 请求处理        │  │ - 动态内容        │  │ - API 扩展        │ │   │
│  │  └───────────────────┘  └───────────────────┘  └───────────────────┘ │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.3 扩展加载机制

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Extension Loading                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐    │
│  │ 内置扩展         │     │ 用户扩展         │     │ 第三方扩展       │    │
│  │ (Bundled)        │     │ (Local)          │     │ (Package)        │    │
│  │                  │     │                  │     │                  │    │
│  │ - Jazor 规则     │     │ .jazor/extensions│     │ npm package      │    │
│  │ - 基础诊断       │     │ jazor.config.json│     │ NuGet package    │    │
│  │ - 标准代码操作   │     │                  │     │                  │    │
│  └────────┬─────────┘     └────────┬─────────┘     └────────┬─────────┘    │
│           │                        │                        │               │
│           └────────────────────────┼────────────────────────┘               │
│                                    ▼                                        │
│                    ┌───────────────────────────────┐                        │
│                    │    ExtensionLoader            │                        │
│                    │                               │                        │
│                    │  - 程序集扫描                 │                        │
│                    │  - 依赖注入注册               │                        │
│                    │  - 配置合并                   │                        │
│                    │  - 生命周期管理               │                        │
│                    └───────────────┬───────────────┘                        │
│                                    │                                        │
│                                    ▼                                        │
│                    ┌───────────────────────────────┐                        │
│                    │    ExtensionRegistry          │                        │
│                    │                               │                        │
│                    │  - Provider 注册表            │                        │
│                    │  - 执行顺序管理               │                        │
│                    │  - 激活/停用控制              │                        │
│                    └───────────────────────────────┘                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 二、新增文件清单

```
src/Jazor.VueHost/
├── Extensions/                         # [新建目录]
│   ├── Abstractions/
│   │   ├── IJazorDiagnosticProvider.cs    # [新增] 诊断提供者接口
│   │   ├── IJazorCodeActionProvider.cs    # [新增] 代码操作提供者接口
│   │   ├── IJazorCompletionProvider.cs    # [新增] 补全提供者接口
│   │   ├── IBuildPlugin.cs                # [新增] 构建插件接口
│   │   ├── IExtension.cs                  # [新增] 扩展基础接口
│   │   └── ExtensionMetadata.cs           # [新增] 扩展元数据
│   │
│   ├── Loading/
│   │   ├── ExtensionLoader.cs             # [新增] 扩展加载器
│   │   ├── ExtensionRegistry.cs           # [新增] 扩展注册表
│   │   └── ExtensionDependencyResolver.cs # [新增] 依赖解析
│   │
│   ├── Builtins/
│   │   ├── JazorStructureDiagnosticProvider.cs  # [新增] 结构诊断
│   │   ├── JazorDirectiveCompletionProvider.cs  # [新增] 指令补全
│   │   └── JazorComponentCodeActionProvider.cs  # [新增] 组件代码操作
│   │
│   └── Context/
│       ├── AnalysisContext.cs             # [新增] 分析上下文
│       └── ProviderResult.cs              # [新增] 提供者结果
│
├── Lsp/
│   ├── Handlers/
│   │   ├── SignatureHelpHandler.cs        # [新增] 签名帮助
│   │   ├── InlayHintHandler.cs            # [新增] 内联提示
│   │   ├── WorkspaceSymbolHandler.cs      # [新增] 工作区符号
│   │   └── FoldingRangeHandler.cs         # [新增] 折叠范围
│   │
│   └── Resilience/
│       ├── LaneHealthMonitor.cs           # [新增] Lane 健康监控
│       ├── DegradationManager.cs          # [新增] 降级管理器
│       └── CircuitBreaker.cs              # [新增] 熔断器
│
├── IdeIntegration/
│   ├── VsCode/
│   │   ├── package.json                  # [新增] VS Code 扩展清单
│   │   ├── extension.ts                  # [新增] 扩展入口
│   │   └── configuration.ts              # [新增] 配置定义
│   │
│   └── JazorConfigSchemaGenerator.cs     # [新增] 配置 Schema 生成
│
└── Workspace/
    └── WorkspaceSymbolIndex.cs           # [新增] 工作区符号索引
```

---

## 三、扩展系统接口定义

### 3.1 核心扩展接口

```csharp
// Extensions/Abstractions/IExtension.cs
namespace Jazor.VueHost.Extensions;

/// <summary>
/// 扩展基础接口
/// </summary>
public interface IExtension
{
    /// <summary>
    /// 扩展元数据
    /// </summary>
    ExtensionMetadata Metadata { get; }
    
    /// <summary>
    /// 初始化扩展
    /// </summary>
    Task InitializeAsync(ExtensionContext context, CancellationToken cancellationToken);
    
    /// <summary>
    /// 激活扩展
    /// </summary>
    Task ActivateAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// 停用扩展
    /// </summary>
    Task DeactivateAsync(CancellationToken cancellationToken);
}

/// <summary>
/// 扩展元数据
/// </summary>
public sealed record ExtensionMetadata
{
    /// <summary>
    /// 扩展 ID
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// 扩展名称
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// 扩展版本
    /// </summary>
    public required string Version { get; init; }
    
    /// <summary>
    /// 扩展描述
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// 扩展作者
    /// </summary>
    public string? Author { get; init; }
    
    /// <summary>
    /// 依赖的其他扩展
    /// </summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    
    /// <summary>
    /// 激活事件
    /// </summary>
    public IReadOnlyList<string> ActivationEvents { get; init; } = Array.Empty<string>();
}

/// <summary>
/// 扩展上下文
/// </summary>
public sealed class ExtensionContext
{
    public IServiceProvider Services { get; }
    public string ExtensionDirectory { get; }
    public IConfiguration Configuration { get; }
    public ILogger Logger { get; }
    
    public ExtensionContext(
        IServiceProvider services,
        string extensionDirectory,
        IConfiguration configuration,
        ILogger logger)
    {
        Services = services;
        ExtensionDirectory = extensionDirectory;
        Configuration = configuration;
        Logger = logger;
    }
}
```

### 3.2 诊断提供者接口

```csharp
// Extensions/Abstractions/IJazorDiagnosticProvider.cs
namespace Jazor.VueHost.Extensions;

/// <summary>
/// Jazor 诊断提供者接口
/// </summary>
public interface IJazorDiagnosticProvider
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 提供者优先级（数字越小优先级越高）
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    bool IsEnabled { get; }
    
    /// <summary>
    /// 提供诊断
    /// </summary>
    Task<IReadOnlyList<Diagnostic>> ProvideDiagnosticsAsync(
        AnalysisContext context,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// 获取诊断代码（用于筛选）
    /// </summary>
    IReadOnlySet<string>? SupportedDiagnosticCodes { get; }
}

/// <summary>
/// 分析上下文
/// </summary>
public sealed class AnalysisContext
{
    /// <summary>
    /// 源文档 URI
    /// </summary>
    public required DocumentUri DocumentUri { get; init; }
    
    /// <summary>
    /// 文档快照
    /// </summary>
    public required IDocumentSnapshot Document { get; init; }
    
    /// <summary>
    /// ProjectionMap
    /// </summary>
    public ProjectionMap? ProjectionMap { get; init; }
    
    /// <summary>
    /// Roslyn 语义模型（如果可用）
    /// </summary>
    public SemanticModel? RoslynSemanticModel { get; init; }
    
    /// <summary>
    /// Volar 分析结果（如果可用）
    /// </summary>
    public VolarAnalysisResult? VolarResult { get; init; }
    
    /// <summary>
    /// Lane 可用性状态
    /// </summary>
    public LaneAvailability LaneAvailability { get; init; }
    
    /// <summary>
    /// 配置选项
    /// </summary>
    public JazorConfig Configuration { get; init; } = new();
    
    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// Lane 可用性状态
/// </summary>
[Flags]
public enum LaneAvailability
{
    None = 0,
    JazorLane = 1,
    RoslynLane = 2,
    VolarLane = 4,
    All = JazorLane | RoslynLane | VolarLane
}
```

### 3.3 代码操作提供者接口

```csharp
// Extensions/Abstractions/IJazorCodeActionProvider.cs
namespace Jazor.VueHost.Extensions;

/// <summary>
/// Jazor 代码操作提供者接口
/// </summary>
public interface IJazorCodeActionProvider
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 支持的 CodeAction 类型
    /// </summary>
    IReadOnlySet<CodeActionKind> SupportedKinds { get; }
    
    /// <summary>
    /// 提供代码操作
    /// </summary>
    Task<IReadOnlyList<CodeAction>> ProvideCodeActionsAsync(
        CodeActionContext context,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// 解析代码操作（懒加载编辑）
    /// </summary>
    Task<CodeAction?> ResolveCodeActionAsync(
        CodeAction codeAction,
        CancellationToken cancellationToken);
}

/// <summary>
/// 代码操作上下文
/// </summary>
public sealed class CodeActionContext
{
    public required DocumentUri DocumentUri { get; init; }
    public required Range Range { get; init; }
    public required CodeActionKind[] RequestedKinds { get; init; }
    public IReadOnlyList<Diagnostic>? Diagnostics { get; init; }
    public AnalysisContext? AnalysisContext { get; init; }
}

/// <summary>
/// 代码操作编辑目标
/// </summary>
public enum EditTarget
{
    /// <summary>
    /// 编辑源文档（.jazor）
    /// </summary>
    SourceDocument,
    
    /// <summary>
    /// 编辑投影文档（如 @code 块的 C# 投影）
    /// </summary>
    ProjectedDocument,
    
    /// <summary>
    /// 编辑多个文档
    /// </summary>
    MultipleDocuments
}
```

### 3.4 扩展注册表

```csharp
// Extensions/Loading/ExtensionRegistry.cs
namespace Jazor.VueHost.Extensions.Loading;

/// <summary>
/// 扩展注册表
/// </summary>
public sealed class ExtensionRegistry
{
    private readonly Dictionary<string, IExtension> _extensions = new();
    private readonly List<IJazorDiagnosticProvider> _diagnosticProviders = new();
    private readonly List<IJazorCodeActionProvider> _codeActionProviders = new();
    private readonly List<IJazorCompletionProvider> _completionProviders = new();
    private readonly ILogger<ExtensionRegistry> _logger;
    
    public ExtensionRegistry(ILogger<ExtensionRegistry> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 注册扩展
    /// </summary>
    public void RegisterExtension(IExtension extension)
    {
        var id = extension.Metadata.Id;
        
        if (_extensions.ContainsKey(id))
        {
            _logger.LogWarning("Extension {Id} is already registered, skipping", id);
            return;
        }
        
        _extensions[id] = extension;
        _logger.LogInformation("Registered extension: {Name} v{Version}", 
            extension.Metadata.Name, extension.Metadata.Version);
    }
    
    /// <summary>
    /// 注册诊断提供者
    /// </summary>
    public void RegisterDiagnosticProvider(IJazorDiagnosticProvider provider)
    {
        _diagnosticProviders.Add(provider);
        _diagnosticProviders.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        _logger.LogDebug("Registered diagnostic provider: {Name}", provider.Name);
    }
    
    /// <summary>
    /// 注册代码操作提供者
    /// </summary>
    public void RegisterCodeActionProvider(IJazorCodeActionProvider provider)
    {
        _codeActionProviders.Add(provider);
        _logger.LogDebug("Registered code action provider: {Name}", provider.Name);
    }
    
    /// <summary>
    /// 获取所有诊断提供者
    /// </summary>
    public IReadOnlyList<IJazorDiagnosticProvider> GetDiagnosticProviders() => _diagnosticProviders;
    
    /// <summary>
    /// 获取所有代码操作提供者
    /// </summary>
    public IReadOnlyList<IJazorCodeActionProvider> GetCodeActionProviders() => _codeActionProviders;
    
    /// <summary>
    /// 获取所有补全提供者
    /// </summary>
    public IReadOnlyList<IJazorCompletionProvider> GetCompletionProviders() => _completionProviders;
    
    /// <summary>
    /// 获取扩展
    /// </summary>
    public IExtension? GetExtension(string id) => _extensions.GetValueOrDefault(id);
    
    /// <summary>
    /// 获取所有扩展
    /// </summary>
    public IReadOnlyDictionary<string, IExtension> GetAllExtensions() => _extensions;
}
```

### 3.5 扩展加载器

```csharp
// Extensions/Loading/ExtensionLoader.cs
namespace Jazor.VueHost.Extensions.Loading;

/// <summary>
/// 扩展加载器
/// </summary>
public sealed class ExtensionLoader
{
    private readonly ExtensionRegistry _registry;
    private readonly IServiceProvider _services;
    private readonly ILogger<ExtensionLoader> _logger;
    
    public ExtensionLoader(
        ExtensionRegistry registry,
        IServiceProvider services,
        ILogger<ExtensionLoader> logger)
    {
        _registry = registry;
        _services = services;
        _logger = logger;
    }
    
    /// <summary>
    /// 加载内置扩展
    /// </summary>
    public async Task LoadBuiltinExtensionsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading builtin extensions...");
        
        // 通过 DI 发现所有 IExtension 实现
        var extensions = _services.GetServices<IExtension>();
        
        foreach (var extension in extensions)
        {
            try
            {
                await LoadExtensionAsync(extension, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load builtin extension: {Name}", 
                    extension.Metadata.Name);
            }
        }
    }
    
    /// <summary>
    /// 加载用户扩展（从指定目录）
    /// </summary>
    public async Task LoadUserExtensionsAsync(
        string extensionsDirectory,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(extensionsDirectory))
        {
            _logger.LogDebug("Extensions directory not found: {Dir}", extensionsDirectory);
            return;
        }
        
        _logger.LogInformation("Loading user extensions from {Dir}", extensionsDirectory);
        
        // 扫描扩展目录
        foreach (var extensionDir in Directory.EnumerateDirectories(extensionsDirectory))
        {
            try
            {
                await LoadExtensionFromDirectoryAsync(extensionDir, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load extension from {Dir}", extensionDir);
            }
        }
    }
    
    /// <summary>
    /// 加载单个扩展
    /// </summary>
    private async Task LoadExtensionAsync(IExtension extension, CancellationToken cancellationToken)
    {
        var context = new ExtensionContext(
            _services,
            extension.GetType().Assembly.Location,
            _services.GetRequiredService<IConfiguration>(),
            _services.GetRequiredService<ILoggerFactory>().CreateLogger(extension.Metadata.Name));
        
        // 初始化
        await extension.InitializeAsync(context, cancellationToken);
        
        // 注册
        _registry.RegisterExtension(extension);
        
        // 注册提供者
        if (extension is IJazorDiagnosticProvider diagnosticProvider)
            _registry.RegisterDiagnosticProvider(diagnosticProvider);
        
        if (extension is IJazorCodeActionProvider codeActionProvider)
            _registry.RegisterCodeActionProvider(codeActionProvider);
        
        if (extension is IJazorCompletionProvider completionProvider)
            _registry.RegisterCompletionProvider(completionProvider);
        
        // 激活
        await extension.ActivateAsync(cancellationToken);
        
        _logger.LogInformation("Extension activated: {Name} v{Version}",
            extension.Metadata.Name, extension.Metadata.Version);
    }
    
    /// <summary>
    /// 从目录加载扩展
    /// </summary>
    private async Task LoadExtensionFromDirectoryAsync(
        string directory,
        CancellationToken cancellationToken)
    {
        // 查找扩展清单
        var manifestPath = Path.Combine(directory, "extension.json");
        if (!File.Exists(manifestPath))
        {
            _logger.LogDebug("No extension.json found in {Dir}", directory);
            return;
        }
        
        // 解析清单
        var manifest = await ParseManifestAsync(manifestPath, cancellationToken);
        if (manifest == null)
            return;
        
        // 加载程序集
        var assemblyPath = Path.Combine(directory, manifest.Assembly);
        if (!File.Exists(assemblyPath))
        {
            _logger.LogWarning("Extension assembly not found: {Path}", assemblyPath);
            return;
        }
        
        var assembly = Assembly.LoadFrom(assemblyPath);
        
        // 查找扩展类型
        var extensionType = assembly.GetType(manifest.ExtensionClass);
        if (extensionType == null || !typeof(IExtension).IsAssignableFrom(extensionType))
        {
            _logger.LogWarning("Invalid extension class: {Class}", manifest.ExtensionClass);
            return;
        }
        
        // 创建实例
        var extension = (IExtension)ActivatorUtilities.CreateInstance(_services, extensionType);
        await LoadExtensionAsync(extension, cancellationToken);
    }
    
    private async Task<ExtensionManifest?> ParseManifestAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<ExtensionManifest>(json);
    }
}

/// <summary>
/// 扩展清单
/// </summary>
public sealed class ExtensionManifest
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Assembly { get; init; }
    public required string ExtensionClass { get; init; }
    public string[]? Dependencies { get; init; }
}
```

---

## 四、高级 LSP 特性

### 4.1 SignatureHelpHandler

```csharp
// Lsp/Handlers/SignatureHelpHandler.cs
namespace Jazor.VueHost.Lsp.Handlers;

/// <summary>
/// 签名帮助处理器
/// </summary>
public sealed class SignatureHelpHandler : IJsonRpcRequestHandler<SignatureHelpParams, SignatureHelp?>
{
    private readonly LspSession _session;
    private readonly ProjectionMap _projectionMap;
    private readonly RoslynLane _roslynLane;
    private readonly VolarLane _volarLane;
    
    public async Task<SignatureHelp?> Handle(
        SignatureHelpParams parameters,
        CancellationToken cancellationToken)
    {
        var uri = parameters.TextDocument.Uri;
        var position = parameters.Position;
        
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            return await GetJazorSignatureHelpAsync(uri, position, cancellationToken);
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            return await _volarLane.GetSignatureHelpAsync(uri, position, cancellationToken);
        }
        else if (uri.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return await _roslynLane.GetSignatureHelpAsync(uri, position, cancellationToken);
        }
        
        return null;
    }
    
    private async Task<SignatureHelp?> GetJazorSignatureHelpAsync(
        DocumentUri uri,
        Position position,
        CancellationToken cancellationToken)
    {
        var projection = _projectionMap.MapToTarget(uri, position);
        if (projection == null)
            return null;
        
        switch (projection.TargetLane)
        {
            case LaneKind.Roslyn:
            {
                // 在 @code 块中调用方法
                var signatureHelp = await _roslynLane.GetSignatureHelpAsync(
                    projection.TargetUri,
                    projection.TargetPosition,
                    cancellationToken);
                
                // 映射参数位置（如果需要）
                return MapSignatureHelp(signatureHelp, projection);
            }
            
            case LaneKind.Volar:
            {
                // 在模板中调用组件或方法
                return await _volarLane.GetSignatureHelpAsync(
                    uri, position, cancellationToken);
            }
            
            default:
                return null;
        }
    }
    
    private static SignatureHelp? MapSignatureHelp(SignatureHelp? help, ProjectionMapEntry projection)
    {
        // 签名帮助通常不需要位置映射，直接返回
        return help;
    }
}
```

### 4.2 InlayHintHandler

```csharp
// Lsp/Handlers/InlayHintHandler.cs
namespace Jazor.VueHost.Lsp.Handlers;

/// <summary>
/// 内联提示处理器
/// </summary>
public sealed class InlayHintHandler : IJsonRpcRequestHandler<InlayHintParams, InlayHint[]>
{
    private readonly LspSession _session;
    private readonly ProjectionMap _projectionMap;
    private readonly RoslynLane _roslynLane;
    private readonly VolarLane _volarLane;
    private readonly JazorConfig _config;
    
    public async Task<InlayHint[]> Handle(
        InlayHintParams parameters,
        CancellationToken cancellationToken)
    {
        var uri = parameters.TextDocument.Uri;
        var range = parameters.Range;
        
        var hints = new List<InlayHint>();
        
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            hints.AddRange(await GetJazorInlayHintsAsync(uri, range, cancellationToken));
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
        {
            hints.AddRange(await _volarLane.GetInlayHintsAsync(uri, range, cancellationToken));
        }
        
        // 应用用户配置过滤
        return FilterByConfiguration(hints);
    }
    
    private async Task<List<InlayHint>> GetJazorInlayHintsAsync(
        DocumentUri uri,
        Range range,
        CancellationToken cancellationToken)
    {
        var hints = new List<InlayHint>();
        
        // 获取范围内的投影
        var projections = _projectionMap.GetProjectionsInRange(uri, range);
        
        foreach (var projection in projections.Where(p => p.TargetLane == LaneKind.Roslyn))
        {
            // 从 Roslyn 获取 inlay hints
            var targetRange = _projectionMap.MapRangeToTarget(uri, range, projection.TargetUri);
            if (targetRange == null) continue;
            
            var roslynHints = await _roslynLane.GetInlayHintsAsync(
                projection.TargetUri, targetRange, cancellationToken);
            
            // 映射回源文档
            foreach (var hint in roslynHints)
            {
                var mapped = MapInlayHintToSource(hint, projection);
                if (mapped != null)
                    hints.Add(mapped);
            }
        }
        
        // Jazor 特有的 hints
        hints.AddRange(await GetJazorSpecificHintsAsync(uri, range, cancellationToken));
        
        return hints;
    }
    
    private async Task<List<InlayHint>> GetJazorSpecificHintsAsync(
        DocumentUri uri,
        Range range,
        CancellationToken cancellationToken)
    {
        var hints = new List<InlayHint>();
        var document = await _session.GetDocumentAsync(uri);
        if (document == null) return hints;
        
        // 组件属性类型提示
        foreach (var component in document.GetComponentsInRange(range))
        {
            if (component.Attributes.Count > 0)
            {
                // 显示 props 类型
                // 例如: <Counter count={123} /> → 显示 count: number
                var props = await _volarLane.GetComponentPropsAsync(component.TagName, cancellationToken);
                if (props != null)
                {
                    foreach (var attr in component.Attributes)
                    {
                        var prop = props.FirstOrDefault(p => p.Name == attr.Name);
                        if (prop != null && prop.Type != "any")
                        {
                            hints.Add(new InlayHint
                            {
                                Position = attr.ValueRange.End,
                                Label = $": {prop.Type}",
                                Kind = InlayHintKind.Type,
                                Tooltip = new MarkupContent
                                {
                                    Kind = MarkupKind.Markdown,
                                    Value = $"**{prop.Name}**: `{prop.Type}`"
                                }
                            });
                        }
                    }
                }
            }
        }
        
        return hints;
    }
    
    private InlayHint? MapInlayHintToSource(InlayHint hint, ProjectionMapEntry projection)
    {
        var sourcePosition = _projectionMap.MapToSource(projection.TargetUri, hint.Position);
        if (sourcePosition == null)
            return null;
        
        return hint with { Position = sourcePosition.Range.Start };
    }
    
    private InlayHint[] FilterByConfiguration(List<InlayHint> hints)
    {
        var settings = _config.InlayHints;
        if (!settings.Enabled)
            return Array.Empty<InlayHint>();
        
        return hints.Where(h =>
        {
            return h.Kind switch
            {
                InlayHintKind.Type => settings.ShowTypeHints,
                InlayHintKind.Parameter => settings.ShowParameterHints,
                _ => true
            };
        }).ToArray();
    }
}

public enum InlayHintKind
{
    Type,
    Parameter
}
```

### 4.3 WorkspaceSymbolHandler

```csharp
// Lsp/Handlers/WorkspaceSymbolHandler.cs
namespace Jazor.VueHost.Lsp.Handlers;

/// <summary>
/// 工作区符号处理器
/// </summary>
public sealed class WorkspaceSymbolHandler : IJsonRpcRequestHandler<WorkspaceSymbolParams, SymbolInformation[]>
{
    private readonly WorkspaceSymbolIndex _symbolIndex;
    private readonly RoslynLane _roslynLane;
    private readonly VolarLane _volarLane;
    
    public async Task<SymbolInformation[]> Handle(
        WorkspaceSymbolParams parameters,
        CancellationToken cancellationToken)
    {
        var query = parameters.Query;
        var results = new List<SymbolInformation>();
        
        // 1. 从符号索引搜索
        var indexedSymbols = await _symbolIndex.SearchAsync(query, cancellationToken);
        results.AddRange(indexedSymbols);
        
        // 2. 从 Roslyn 搜索 C# 符号
        var roslynSymbols = await _roslynLane.SearchWorkspaceSymbolsAsync(query, cancellationToken);
        results.AddRange(roslynSymbols);
        
        // 3. 从 Volar 搜索前端符号
        var volarSymbols = await _volarLane.SearchWorkspaceSymbolsAsync(query, cancellationToken);
        results.AddRange(volarSymbols);
        
        // 去重和排序
        return DeduplicateAndRank(results, query);
    }
    
    private static SymbolInformation[] DeduplicateAndRank(
        List<SymbolInformation> symbols,
        string query)
    {
        // 按名称和位置去重
        var unique = symbols
            .GroupBy(s => (s.Name, s.Location.Uri, s.Location.Range.Start))
            .Select(g => g.First())
            .ToList();
        
        // 按匹配度排序
        var queryLower = query.ToLowerInvariant();
        return unique
            .OrderBy(s =>
            {
                var nameLower = s.Name.ToLowerInvariant();
                if (nameLower == queryLower) return 0;
                if (nameLower.StartsWith(queryLower)) return 1;
                if (nameLower.Contains(queryLower)) return 2;
                return 3;
            })
            .ThenBy(s => s.Name)
            .Take(100) // 限制结果数量
            .ToArray();
    }
}
```

### 4.4 WorkspaceSymbolIndex

```csharp
// Workspace/WorkspaceSymbolIndex.cs
namespace Jazor.VueHost.Workspace;

/// <summary>
/// 工作区符号索引
/// </summary>
public sealed class WorkspaceSymbolIndex
{
    private readonly Dictionary<string, List<IndexedSymbol>> _symbolsByFile = new();
    private readonly Dictionary<string, List<IndexedSymbol>> _symbolsByName = new();
    private readonly ILogger<WorkspaceSymbolIndex> _logger;
    
    public WorkspaceSymbolIndex(ILogger<WorkspaceSymbolIndex> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 索引文件符号
    /// </summary>
    public void IndexFile(string uri, IEnumerable<IndexedSymbol> symbols)
    {
        // 清除旧索引
        ClearFile(uri);
        
        var symbolList = symbols.ToList();
        _symbolsByFile[uri] = symbolList;
        
        // 建立名称索引
        foreach (var symbol in symbolList)
        {
            var nameKey = symbol.Name.ToLowerInvariant();
            if (!_symbolsByName.TryGetValue(nameKey, out var list))
            {
                list = new List<IndexedSymbol>();
                _symbolsByName[nameKey] = list;
            }
            list.Add(symbol);
        }
        
        _logger.LogDebug("Indexed {Count} symbols in {Uri}", symbolList.Count, uri);
    }
    
    /// <summary>
    /// 清除文件索引
    /// </summary>
    public void ClearFile(string uri)
    {
        if (_symbolsByFile.TryGetValue(uri, out var symbols))
        {
            foreach (var symbol in symbols)
            {
                var nameKey = symbol.Name.ToLowerInvariant();
                if (_symbolsByName.TryGetValue(nameKey, out var nameSymbols))
                {
                    nameSymbols.RemoveAll(s => s.Uri == uri);
                }
            }
            
            _symbolsByFile.Remove(uri);
        }
    }
    
    /// <summary>
    /// 搜索符号
    /// </summary>
    public Task<List<SymbolInformation>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var results = new List<SymbolInformation>();
        var queryLower = query.ToLowerInvariant();
        
        // 精确匹配
        if (_symbolsByName.TryGetValue(queryLower, out var exactMatches))
        {
            results.AddRange(exactMatches.Select(ToSymbolInformation));
        }
        
        // 前缀匹配
        foreach (var (name, symbols) in _symbolsByName)
        {
            if (name.StartsWith(queryLower) && name != queryLower)
            {
                results.AddRange(symbols.Select(ToSymbolInformation));
            }
        }
        
        // 包含匹配（如果结果不足）
        if (results.Count < 50)
        {
            foreach (var (name, symbols) in _symbolsByName)
            {
                if (name.Contains(queryLower) && !name.StartsWith(queryLower))
                {
                    results.AddRange(symbols.Select(ToSymbolInformation));
                }
                
                if (results.Count >= 100)
                    break;
            }
        }
        
        return Task.FromResult(results);
    }
    
    private static SymbolInformation ToSymbolInformation(IndexedSymbol symbol)
    {
        return new SymbolInformation
        {
            Name = symbol.Name,
            Kind = symbol.Kind,
            Location = new Location
            {
                Uri = symbol.Uri,
                Range = symbol.Range
            },
            ContainerName = symbol.ContainerName
        };
    }
}

/// <summary>
/// 索引的符号
/// </summary>
public sealed record IndexedSymbol
{
    public required string Name { get; init; }
    public required SymbolKind Kind { get; init; }
    public required string Uri { get; init; }
    public required Range Range { get; init; }
    public string? ContainerName { get; init; }
    public string? Detail { get; init; }
}
```

### 4.5 FoldingRangeHandler

```csharp
// Lsp/Handlers/FoldingRangeHandler.cs
namespace Jazor.VueHost.Lsp.Handlers;

/// <summary>
/// 折叠范围处理器
/// </summary>
public sealed class FoldingRangeHandler : IJsonRpcRequestHandler<FoldingRangeParams, FoldingRange[]>
{
    private readonly LspSession _session;
    private readonly VolarLane _volarLane;
    
    public async Task<FoldingRange[]> Handle(
        FoldingRangeParams parameters,
        CancellationToken cancellationToken)
    {
        var uri = parameters.TextDocument.Uri;
        
        if (uri.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            return await GetJazorFoldingRangesAsync(uri, cancellationToken);
        }
        else if (uri.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                 uri.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            return await _volarLane.GetFoldingRangesAsync(uri, cancellationToken);
        }
        
        return Array.Empty<FoldingRange>();
    }
    
    private async Task<FoldingRange[]> GetJazorFoldingRangesAsync(
        DocumentUri uri,
        CancellationToken cancellationToken)
    {
        var ranges = new List<FoldingRange>();
        var document = await _session.GetDocumentAsync(uri);
        if (document == null)
            return Array.Empty<FoldingRange>();
        
        // @code 块
        foreach (var block in document.GetCodeBlocks())
        {
            ranges.Add(new FoldingRange
            {
                StartLine = block.Range.Start.Line,
                StartCharacter = block.Range.Start.Character,
                EndLine = block.Range.End.Line,
                EndCharacter = block.Range.End.Character,
                Kind = FoldingRangeKind.Region,
                CollapsedText = "@code { ... }"
            });
        }
        
        // @functions 块
        foreach (var block in document.GetFunctionsBlocks())
        {
            ranges.Add(new FoldingRange
            {
                StartLine = block.Range.Start.Line,
                StartCharacter = block.Range.Start.Character,
                EndLine = block.Range.End.Line,
                EndCharacter = block.Range.End.Character,
                Kind = FoldingRangeKind.Region,
                CollapsedText = "@functions { ... }"
            });
        }
        
        // 组件标签
        foreach (var component in document.GetComponents())
        {
            if (component.HasChildren)
            {
                ranges.Add(new FoldingRange
                {
                    StartLine = component.OpenTagRange.End.Line,
                    StartCharacter = component.OpenTagRange.End.Character,
                    EndLine = component.CloseTagRange!.Start.Line,
                    EndCharacter = component.CloseTagRange.Start.Character,
                    Kind = FoldingRangeKind.Region,
                    CollapsedText = $"<{component.TagName}> ... </{component.TagName}>"
                });
            }
        }
        
        // HTML 元素（委托 Volar）
        var volarRanges = await _volarLane.GetFoldingRangesAsync(uri, cancellationToken);
        ranges.AddRange(volarRanges.Where(r => r.Kind != FoldingRangeKind.Imports));
        
        // 导入语句
        var imports = document.GetImports();
        if (imports.Count > 1)
        {
            ranges.Add(new FoldingRange
            {
                StartLine = imports[0].Range.Start.Line,
                EndLine = imports[^1].Range.End.Line,
                Kind = FoldingRangeKind.Imports,
                CollapsedText = $"// {imports.Count} imports"
            });
        }
        
        // 注释
        foreach (var comment in document.GetComments())
        {
            if (comment.IsMultiLine)
            {
                ranges.Add(new FoldingRange
                {
                    StartLine = comment.Range.Start.Line,
                    EndLine = comment.Range.End.Line,
                    Kind = FoldingRangeKind.Comment,
                    CollapsedText = "/* ... */"
                });
            }
        }
        
        return ranges.ToArray();
    }
}

public enum FoldingRangeKind
{
    Comment,
    Imports,
    Region
}
```

---

## 五、降级与容错机制

### 5.1 LaneHealthMonitor

```csharp
// Lsp/Resilience/LaneHealthMonitor.cs
namespace Jazor.VueHost.Lsp.Resilience;

/// <summary>
/// Lane 健康监控器
/// </summary>
public sealed class LaneHealthMonitor : IDisposable
{
    private readonly Dictionary<LaneKind, LaneHealthStatus> _healthStatus = new();
    private readonly Timer _healthCheckTimer;
    private readonly ILogger<LaneHealthMonitor> _logger;
    
    public event EventHandler<LaneHealthChangedEventArgs>? HealthChanged;
    
    public LaneHealthMonitor(ILogger<LaneHealthMonitor> logger)
    {
        _logger = logger;
        
        // 初始化健康状态
        foreach (var lane in Enum.GetValues<LaneKind>())
        {
            _healthStatus[lane] = new LaneHealthStatus
            {
                Lane = lane,
                State = HealthState.Unknown,
                LastCheck = DateTimeOffset.MinValue
            };
        }
        
        // 定期健康检查
        _healthCheckTimer = new Timer(PerformHealthCheck, null, 
            TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }
    
    /// <summary>
    /// 获取 Lane 健康状态
    /// </summary>
    public LaneHealthStatus GetHealth(LaneKind lane) => _healthStatus[lane];
    
    /// <summary>
    /// 获取所有可用 Lane
    /// </summary>
    public LaneAvailability GetAvailableLanes()
    {
        var availability = LaneAvailability.None;
        
        foreach (var (lane, status) in _healthStatus)
        {
            if (status.State == HealthState.Healthy)
            {
                availability |= (LaneAvailability)lane;
            }
        }
        
        return availability;
    }
    
    /// <summary>
    /// 报告 Lane 成功
    /// </summary>
    public void ReportSuccess(LaneKind lane)
    {
        var status = _healthStatus[lane];
        var previousState = status.State;
        
        _healthStatus[lane] = status with
        {
            State = HealthState.Healthy,
            LastCheck = DateTimeOffset.UtcNow,
            ConsecutiveFailures = 0,
            LastSuccess = DateTimeOffset.UtcNow
        };
        
        if (previousState != HealthState.Healthy)
        {
            OnHealthChanged(lane, previousState, HealthState.Healthy);
        }
    }
    
    /// <summary>
    /// 报告 Lane 失败
    /// </summary>
    public void ReportFailure(LaneKind lane, Exception? exception = null)
    {
        var status = _healthStatus[lane];
        var previousState = status.State;
        var consecutiveFailures = status.ConsecutiveFailures + 1;
        
        var newState = consecutiveFailures switch
        {
            >= 3 => HealthState.Unhealthy,
            >= 1 => HealthState.Degraded,
            _ => HealthState.Healthy
        };
        
        _healthStatus[lane] = status with
        {
            State = newState,
            LastCheck = DateTimeOffset.UtcNow,
            ConsecutiveFailures = consecutiveFailures,
            LastError = exception?.Message,
            LastErrorTime = DateTimeOffset.UtcNow
        };
        
        if (previousState != newState)
        {
            OnHealthChanged(lane, previousState, newState);
        }
    }
    
    private void PerformHealthCheck(object? state)
    {
        // 检查各 Lane 健康状态
        foreach (var (lane, status) in _healthStatus)
        {
            // 如果超过 60 秒没有成功，标记为不健康
            if (status.State == HealthState.Healthy && 
                DateTimeOffset.UtcNow - status.LastSuccess > TimeSpan.FromSeconds(60))
            {
                ReportFailure(lane, new TimeoutException("Health check timeout"));
            }
        }
    }
    
    private void OnHealthChanged(LaneKind lane, HealthState previous, HealthState current)
    {
        _logger.LogInformation("Lane {Lane} health changed: {Previous} -> {Current}", 
            lane, previous, current);
        
        HealthChanged?.Invoke(this, new LaneHealthChangedEventArgs
        {
            Lane = lane,
            PreviousState = previous,
            CurrentState = current
        });
    }
    
    public void Dispose()
    {
        _healthCheckTimer.Dispose();
    }
}

/// <summary>
/// Lane 健康状态
/// </summary>
public sealed record LaneHealthStatus
{
    public required LaneKind Lane { get; init; }
    public HealthState State { get; init; }
    public DateTimeOffset LastCheck { get; init; }
    public DateTimeOffset LastSuccess { get; init; }
    public int ConsecutiveFailures { get; init; }
    public string? LastError { get; init; }
    public DateTimeOffset? LastErrorTime { get; init; }
}

public enum HealthState
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy
}

public sealed class LaneHealthChangedEventArgs : EventArgs
{
    public required LaneKind Lane { get; init; }
    public required HealthState PreviousState { get; init; }
    public required HealthState CurrentState { get; init; }
}
```

### 5.2 DegradationManager

```csharp
// Lsp/Resilience/DegradationManager.cs
namespace Jazor.VueHost.Lsp.Resilience;

/// <summary>
/// 降级管理器
/// </summary>
public sealed class DegradationManager
{
    private readonly LaneHealthMonitor _healthMonitor;
    private readonly ILogger<DegradationManager> _logger;
    
    public DegradationManager(
        LaneHealthMonitor healthMonitor,
        ILogger<DegradationManager> logger)
    {
        _healthMonitor = healthMonitor;
        _logger = logger;
        
        _healthMonitor.HealthChanged += OnHealthChanged;
    }
    
    /// <summary>
    /// 获取当前能力可用性
    /// </summary>
    public CapabilityAvailability GetCapabilityAvailability(Capability capability)
    {
        var availability = _healthMonitor.GetAvailableLanes();
        
        return capability switch
        {
            // C# 相关能力：需要 RoslynLane
            Capability.CSharpCompletion or
            Capability.CSharpDiagnostics or
            Capability.CSharpNavigation =>
                availability.HasFlag(LaneAvailability.RoslynLane)
                    ? CapabilityAvailability.Available
                    : CapabilityAvailability.Unavailable,
            
            // 前端相关能力：需要 VolarLane
            Capability.VueCompletion or
            Capability.VueDiagnostics or
            Capability.TypeScriptCompletion =>
                availability.HasFlag(LaneAvailability.VolarLane)
                    ? CapabilityAvailability.Available
                    : CapabilityAvailability.Unavailable,
            
            // .jazor 能力：需要至少一个 Lane
            Capability.JazorCompletion =>
                availability != LaneAvailability.None
                    ? CapabilityAvailability.Available
                    : CapabilityAvailability.Limited,
            
            // 调试能力：独立
            Capability.Debugging => CapabilityAvailability.Available,
            
            // Dev Server：独立
            Capability.DevServer => CapabilityAvailability.Available,
            
            _ => CapabilityAvailability.Unknown
        };
    }
    
    /// <summary>
    /// 执行操作（带降级）
    /// </summary>
    public async Task<T?> ExecuteWithFallbackAsync<T>(
        LaneKind primaryLane,
        Func<Task<T>> primaryAction,
        Func<Task<T>>? fallbackAction,
        CancellationToken cancellationToken)
    {
        var health = _healthMonitor.GetHealth(primaryLane);
        
        if (health.State == HealthState.Healthy)
        {
            try
            {
                var result = await primaryAction();
                _healthMonitor.ReportSuccess(primaryLane);
                return result;
            }
            catch (Exception ex)
            {
                _healthMonitor.ReportFailure(primaryLane, ex);
                
                // 尝试降级
                if (fallbackAction != null)
                {
                    _logger.LogWarning("Primary lane {Lane} failed, using fallback", primaryLane);
                    return await fallbackAction();
                }
                
                throw;
            }
        }
        else if (health.State == HealthState.Degraded && fallbackAction != null)
        {
            _logger.LogInformation("Lane {Lane} is degraded, using fallback", primaryLane);
            return await fallbackAction();
        }
        else if (fallbackAction != null)
        {
            return await fallbackAction();
        }
        
        return default;
    }
    
    private void OnHealthChanged(object? sender, LaneHealthChangedEventArgs e)
    {
        var status = _healthMonitor.GetHealth(e.Lane);
        
        _logger.LogInformation(
            "Lane {Lane} health: {State}, failures: {Failures}, available: {Availability}",
            e.Lane, status.State, status.ConsecutiveFailures,
            _healthMonitor.GetAvailableLanes());
    }
}

public enum Capability
{
    CSharpCompletion,
    CSharpDiagnostics,
    CSharpNavigation,
    VueCompletion,
    VueDiagnostics,
    TypeScriptCompletion,
    JazorCompletion,
    Debugging,
    DevServer
}

public enum CapabilityAvailability
{
    Available,
    Limited,
    Unavailable,
    Unknown
}
```

### 5.3 CircuitBreaker

```csharp
// Lsp/Resilience/CircuitBreaker.cs
namespace Jazor.VueHost.Lsp.Resilience;

/// <summary>
/// 熔断器
/// </summary>
public sealed class CircuitBreaker
{
    private readonly string _name;
    private readonly int _failureThreshold;
    private readonly TimeSpan _resetTimeout;
    private readonly ILogger<CircuitBreaker> _logger;
    
    private int _failureCount;
    private DateTimeOffset _lastFailureTime;
    private CircuitState _state = CircuitState.Closed;
    
    public CircuitBreaker(
        string name,
        int failureThreshold = 5,
        TimeSpan? resetTimeout = null,
        ILogger<CircuitBreaker>? logger = null)
    {
        _name = name;
        _failureThreshold = failureThreshold;
        _resetTimeout = resetTimeout ?? TimeSpan.FromSeconds(30);
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CircuitBreaker>.Instance;
    }
    
    public CircuitState State => _state;
    
    public bool IsAllowed
    {
        get
        {
            if (_state == CircuitState.Closed)
                return true;
            
            if (_state == CircuitState.Open)
            {
                // 检查是否可以尝试重置
                if (DateTimeOffset.UtcNow - _lastFailureTime > _resetTimeout)
                {
                    _state = CircuitState.HalfOpen;
                    _logger.LogInformation("Circuit breaker {Name} entering half-open state", _name);
                    return true;
                }
                
                return false;
            }
            
            // HalfOpen: 允许一个请求通过
            return true;
        }
    }
    
    public void RecordSuccess()
    {
        if (_state == CircuitState.HalfOpen)
        {
            _state = CircuitState.Closed;
            _failureCount = 0;
            _logger.LogInformation("Circuit breaker {Name} closed after successful request", _name);
        }
        else if (_state == CircuitState.Closed)
        {
            _failureCount = 0;
        }
    }
    
    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTimeOffset.UtcNow;
        
        if (_state == CircuitState.HalfOpen)
        {
            _state = CircuitState.Open;
            _logger.LogWarning("Circuit breaker {Name} opened after failure in half-open state", _name);
        }
        else if (_failureCount >= _failureThreshold)
        {
            _state = CircuitState.Open;
            _logger.LogWarning("Circuit breaker {Name} opened after {Count} failures", _name, _failureCount);
        }
    }
    
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        if (!IsAllowed)
        {
            throw new CircuitBreakerOpenException($"Circuit breaker {_name} is open");
        }
        
        try
        {
            var result = await action();
            RecordSuccess();
            return result;
        }
        catch (Exception ex)
        {
            RecordFailure();
            throw new CircuitBreakerException($"Circuit breaker {_name} caught exception", ex);
        }
    }
    
    public async Task<T?> ExecuteWithFallbackAsync<T>(
        Func<Task<T>> action,
        Func<Task<T>>? fallback = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsAllowed)
        {
            if (fallback != null)
                return await fallback();
            
            return default;
        }
        
        try
        {
            var result = await action();
            RecordSuccess();
            return result;
        }
        catch (Exception ex)
        {
            RecordFailure();
            _logger.LogError(ex, "Circuit breaker {Name} action failed", _name);
            
            if (fallback != null)
                return await fallback();
            
            return default;
        }
    }
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}

public sealed class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
}

public sealed class CircuitBreakerException : Exception
{
    public CircuitBreakerException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

---

## 六、VS Code 扩展集成

### 6.1 package.json

```json
{
  "name": "jazor-vuehost",
  "displayName": "Jazor VueHost",
  "description": "Jazor VueHost - Razor-first Vue development experience",
  "version": "0.1.0",
  "publisher": "jazor",
  "engines": {
    "vscode": "^1.85.0"
  },
  "categories": [
    "Programming Languages",
    "Debuggers",
    "Other"
  ],
  "activationEvents": [
    "onLanguage:jazor",
    "onLanguage:vue",
    "onLanguage:typescript",
    "workspaceContains:jazor.config.json"
  ],
  "main": "./out/extension.js",
  "contributes": {
    "languages": [
      {
        "id": "jazor",
        "aliases": ["Jazor", "jazor"],
        "extensions": [".jazor"],
        "configuration": "./language-configuration.json"
      }
    ],
    "grammars": [
      {
        "language": "jazor",
        "scopeName": "source.jazor",
        "path": "./syntaxes/jazor.tmLanguage.json"
      }
    ],
    "configuration": {
      "title": "Jazor VueHost",
      "properties": {
        "jazor.trace.server": {
          "type": "string",
          "enum": ["off", "messages", "verbose"],
          "default": "off",
          "description": "Traces the communication between VS Code and the Jazor VueHost server."
        },
        "jazor.server.path": {
          "type": "string",
          "default": "",
          "description": "Path to the VueHost executable. If empty, uses bundled version."
        },
        "jazor.inlayHints.enabled": {
          "type": "boolean",
          "default": true,
          "description": "Enable inlay hints."
        },
        "jazor.inlayHints.showTypeHints": {
          "type": "boolean",
          "default": true,
          "description": "Show type hints inlay hints."
        },
        "jazor.inlayHints.showParameterHints": {
          "type": "boolean",
          "default": true,
          "description": "Show parameter name inlay hints."
        }
      }
    },
    "commands": [
      {
        "command": "jazor.restartServer",
        "title": "Restart VueHost Server",
        "category": "Jazor"
      },
      {
        "command": "jazor.startDevServer",
        "title": "Start Dev Server",
        "category": "Jazor"
      },
      {
        "command": "jazor.build",
        "title": "Build for Production",
        "category": "Jazor"
      }
    ],
    "menus": {
      "commandPalette": [
        {
          "command": "jazor.restartServer",
          "when": "jazor:active"
        },
        {
          "command": "jazor.startDevServer",
          "when": "jazor:active"
        },
        {
          "command": "jazor.build",
          "when": "jazor:active"
        }
      ]
    },
    "debuggers": [
      {
        "type": "jazor",
        "label": "Jazor Debug",
        "program": "./out/debugAdapter.js",
        "runtime": "node",
        "configurationAttributes": {
          "launch": {
            "properties": {
              "url": {
                "type": "string",
                "description": "URL to open in browser"
              },
              "webRoot": {
                "type": "string",
                "description": "Web root folder"
              }
            }
          }
        },
        "initialConfigurations": [
          {
            "type": "jazor",
            "request": "launch",
            "name": "Launch Jazor App",
            "url": "http://localhost:5173",
            "webRoot": "${workspaceFolder}"
          }
        ]
      }
    ]
  },
  "scripts": {
    "vscode:prepublish": "npm run compile",
    "compile": "tsc -p ./",
    "watch": "tsc -watch -p ./",
    "lint": "eslint src --ext ts"
  },
  "devDependencies": {
    "@types/node": "^20.0.0",
    "@types/vscode": "^1.85.0",
    "@vscode/test-electron": "^2.3.0",
    "typescript": "^5.3.0"
  }
}
```

### 6.2 extension.ts

```typescript
// IdeIntegration/VsCode/extension.ts
import * as vscode from 'vscode';
import * as path from 'path';
import { LanguageClient, LanguageClientOptions, ServerOptions } from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

export async function activate(context: vscode.ExtensionContext) {
    console.log('Jazor VueHost extension is activating...');

    const config = vscode.workspace.getConfiguration('jazor');
    const serverPath = config.get<string>('server.path') || findBundledServer(context);
    
    if (!serverPath) {
        vscode.window.showErrorMessage('Jazor VueHost server not found. Please install VueHost.');
        return;
    }

    // Server options
    const serverOptions: ServerOptions = {
        command: 'dotnet',
        args: ['run', '--project', serverPath, '--', '--lsp'],
        options: {
            cwd: vscode.workspace.workspaceFolders?.[0]?.uri.fsPath
        }
    };

    // Client options
    const clientOptions: LanguageClientOptions = {
        documentSelector: [
            { scheme: 'file', language: 'jazor' },
            { scheme: 'file', language: 'vue' },
            { scheme: 'file', language: 'typescript' },
            { scheme: 'file', language: 'javascript' },
            { scheme: 'file', language: 'css' },
            { scheme: 'file', language: 'html' }
        ],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher('**/.jazor*')
        }
    };

    // Create the language client
    client = new LanguageClient(
        'jazorVueHost',
        'Jazor VueHost',
        serverOptions,
        clientOptions
    );

    // Start the client
    await client.start();
    
    // Register commands
    registerCommands(context);
    
    // Update context
    vscode.commands.executeCommand('setContext', 'jazor:active', true);
    
    console.log('Jazor VueHost extension is now active!');
}

function findBundledServer(context: vscode.ExtensionContext): string | undefined {
    // Look for bundled VueHost
    const bundledPath = path.join(context.extensionPath, 'server', 'Jazor.VueHost.csproj');
    if (require('fs').existsSync(bundledPath)) {
        return bundledPath;
    }
    return undefined;
}

function registerCommands(context: vscode.ExtensionContext) {
    // Restart server
    context.subscriptions.push(
        vscode.commands.registerCommand('jazor.restartServer', async () => {
            if (client) {
                vscode.window.showInformationMessage('Restarting Jazor VueHost server...');
                await client.stop();
                await client.start();
                vscode.window.showInformationMessage('Jazor VueHost server restarted.');
            }
        })
    );

    // Start dev server
    context.subscriptions.push(
        vscode.commands.registerCommand('jazor.startDevServer', async () => {
            const terminal = vscode.window.createTerminal('Jazor Dev Server');
            terminal.sendText('dotnet run --project Jazor.VueHost -- --dev');
            terminal.show();
        })
    );

    // Build
    context.subscriptions.push(
        vscode.commands.registerCommand('jazor.build', async () => {
            const terminal = vscode.window.createTerminal('Jazor Build');
            terminal.sendText('dotnet run --project Jazor.VueHost -- --build');
            terminal.show();
        })
    );
}

export async function deactivate() {
    if (client) {
        await client.stop();
    }
    vscode.commands.executeCommand('setContext', 'jazor:active', false);
}
```

---

## 七、jazor.config.json 扩展

```jsonc
{
    "server": { /* ... */ },
    "build": { /* ... */ },
    "extensions": {
        // 启用/禁用扩展
        "enabled": true,
        
        // 扩展目录（相对于项目根目录）
        "directory": ".jazor/extensions",
        
        // 禁用的扩展 ID 列表
        "disabled": ["some-extension-id"],
        
        // 扩展配置
        "configuration": {
            "my-extension": {
                "option1": "value1"
            }
        }
    },
    "inlayHints": {
        // 是否启用 inlay hints
        "enabled": true,
        
        // 显示类型提示
        "showTypeHints": true,
        
        // 显示参数名提示
        "showParameterHints": true,
        
        // 显示隐式导入提示
        "showImplicitImports": false
    },
    "diagnostics": {
        // 启用的诊断规则
        "enabled": true,
        
        // 禁用的诊断代码
        "disabled": ["JAZOR001"],
        
        // 诊断严重性覆盖
        "severityOverrides": {
            "JAZOR002": "warning"
        }
    }
}
```

---

## 八、实施步骤（严格顺序）

### Step 1: 扩展基础接口

**产出文件**:
- 新增 `Extensions/Abstractions/IExtension.cs`
- 新增 `Extensions/Abstractions/ExtensionMetadata.cs`
- 新增 `Extensions/Loading/ExtensionRegistry.cs`

**测试**:
- 扩展元数据解析
- 注册表操作

**退出标准**: 扩展接口定义完成。

---

### Step 2: 诊断提供者接口

**产出文件**:
- 新增 `Extensions/Abstractions/IJazorDiagnosticProvider.cs`
- 新增 `Extensions/Context/AnalysisContext.cs`
- 新增 `Extensions/Builtins/JazorStructureDiagnosticProvider.cs`

**测试**:
- 诊断提供
- Lane 可用性检查
- 诊断过滤

**退出标准**: 自定义诊断可注册并提供诊断。

---

### Step 3: 代码操作提供者接口

**产出文件**:
- 新增 `Extensions/Abstractions/IJazorCodeActionProvider.cs`
- 新增 `Extensions/Builtins/JazorComponentCodeActionProvider.cs`

**依赖**: Step 2

**测试**:
- CodeAction 提供
- CodeAction 解析
- 编辑执行

**退出标准**: 自定义 CodeAction 可注册。

---

### Step 4: 扩展加载器

**产出文件**:
- 新增 `Extensions/Loading/ExtensionLoader.cs`
- 修改 `Program.cs` — 集成扩展加载

**依赖**: Step 1-3

**测试**:
- 内置扩展加载
- 用户扩展加载
- 依赖解析

**退出标准**: 扩展可从目录加载。

---

### Step 5: SignatureHelp + InlayHints

**产出文件**:
- 新增 `Lsp/Handlers/SignatureHelpHandler.cs`
- 新增 `Lsp/Handlers/InlayHintHandler.cs`

**依赖**: Phase 6 的 ProjectionMap

**测试**:
- 签名帮助显示
- Inlay hint 显示
- 配置过滤

**退出标准**: Signature Help 和 Inlay Hints 可用。

---

### Step 6: WorkspaceSymbol + FoldingRange

**产出文件**:
- 新增 `Lsp/Handlers/WorkspaceSymbolHandler.cs`
- 新增 `Lsp/Handlers/FoldingRangeHandler.cs`
- 新增 `Workspace/WorkspaceSymbolIndex.cs`

**测试**:
- 符号搜索
- 折叠范围

**退出标准**: 工作区符号搜索和折叠可用。

---

### Step 7: 降级与容错机制

**产出文件**:
- 新增 `Lsp/Resilience/LaneHealthMonitor.cs`
- 新增 `Lsp/Resilience/DegradationManager.cs`
- 新增 `Lsp/Resilience/CircuitBreaker.cs`

**测试**:
- 健康监控
- 故障检测
- 自动降级
- 熔断恢复

**退出标准**: Lane 故障时自动降级。

---

### Step 8: VS Code 扩展

**产出文件**:
- 新增 `IdeIntegration/VsCode/package.json`
- 新增 `IdeIntegration/VsCode/extension.ts`
- 新增 `IdeIntegration/VsCode/configuration.ts`

**测试**:
- 扩展激活
- LSP 连接
- 命令执行
- 配置加载

**退出标准**: VS Code 可安装并使用扩展。

---

## 九、风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| **扩展冲突** | 诊断/代码操作冲突 | 优先级排序；用户可禁用 |
| **扩展性能问题** | 响应延迟 | 超时限制；异步执行 |
| **扩展安全** | 恶意代码执行 | 沙箱隔离；权限控制 |
| **Lane 故障传播** | 级联失败 | 熔断器；隔离执行 |
| **VS Code 兼容性** | 扩展不工作 | 版本检查；降级提示 |

---

## 十、后续优化方向

### 10.1 扩展市场

- 在线扩展发布
- 扩展搜索和安装
- 版本管理

### 10.2 性能优化

- 增量符号索引
- 后台预计算
- 结果缓存

### 10.3 其他 IDE 支持

- JetBrains 插件
- Vim/Neovim LSP 集成
- Emacs LSP 集成

---

**文档维护者**: developerhan  
**最后更新**: 2026-04-15  
**文档版本**: v1.0
