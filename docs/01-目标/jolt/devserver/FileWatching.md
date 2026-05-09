# File Watching & Configuration

DevServer 的文件监听和配置系统，包括 `ChangeProcessor`、`FileChangeDebouncer`、`DependencyGraph`、`DevServerOptions` 和 `DevServerOptionsParser`。

## 核心类型

### ChangeProcessor

文件变更处理和 HMR 策略决策。

```csharp
internal sealed class ChangeProcessor
{
    private readonly OnDemandCompiler _compiler;
    private readonly ModuleResolver _moduleResolver;
    private readonly DependencyGraph _dependencyGraph;

    public async ValueTask<ChangeProcessingResult> ProcessChangesAsync(
        IReadOnlyList<string> changedPaths,
        CancellationToken cancellationToken);

    public async ValueTask<ChangeProcessingResult> ProcessWorkspaceDocumentChangeAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken);
}
```

**ChangeProcessingResult**（第 845-862 行）：
```csharp
internal sealed class ChangeProcessingResult
{
    public required ChangeUpdateKind UpdateKind { get; init; }
    public string? FullReloadReason { get; init; }
    public required IReadOnlyList<string> ChangedPaths { get; init; }
    public required IReadOnlyList<string> AffectedPaths { get; init; }
    public IReadOnlyList<string> ChangedCssUrls { get; init; } = [];
    public IReadOnlyList<InlineStyleUpdate> InlineStyleUpdates { get; init; } = [];
    public IReadOnlyList<JavaScriptHotUpdate> JavaScriptUpdates { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
```

**ChangeUpdateKind**：
```csharp
internal enum ChangeUpdateKind
{
    FullReload,        // 全页面重新加载
    StyleUpdate,       // CSS 样式更新（无刷新）
    JavaScriptUpdate,  // JavaScript 模块更新（HMR）
    Error              // 编译错误
}
```

作用域约束：变更处理先解析 owning project 再计算受影响集合；隐式依赖发现只在 owning project 内展开；HMR 广播只覆盖 owning project 的依赖闭包；sibling project 的模块和诊断不会因为这次变更被顺带刷新。

### FileChangeDebouncer

文件系统事件防抖，避免频繁变更触发过多编译。

```csharp
internal sealed class FileChangeDebouncer : IDisposable
{
    private readonly TimeSpan _debounceInterval;
    private readonly Lock _gate = new();
    private readonly HashSet<string> _pendingPaths = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _flushCancellationSource;

    public event Action<IReadOnlyList<string>>? DebouncedChange;

    public FileChangeDebouncer(TimeSpan debounceInterval);
    public void Record(string path);
    public void Dispose();
}
```

防抖逻辑：`Record` 添加路径到 `_pendingPaths`，取消旧超时，创建新超时。`ScheduleFlushAsync` 等待防抖间隔后取出并清空 `_pendingPaths`，触发 `DebouncedChange` 事件。

### DependencyGraph

模块依赖跟踪，支持依赖查询和影响分析。

```csharp
internal sealed class DependencyGraph
{
    private readonly Dictionary<string, HashSet<string>> _dependenciesByModule = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _dependentsByDependency = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();
    private readonly ModuleResolver? _moduleResolver;

    public void Record(string modulePath, IReadOnlyList<string> dependencies);
    public IReadOnlyList<string> GetDependencies(string modulePath);
    public IReadOnlyList<string> GetDependents(string modulePath);
    public IReadOnlyList<string> GetAllAffectedModules(string changedModulePath);
    public void Remove(string modulePath);
    public void Clear();
}
```

`Record` 移除旧依赖关系，规范化依赖路径，记录模块→依赖和依赖→模块（反向索引）。

`GetAllAffectedModules` 使用 BFS 遍历依赖图，从变更模块出发沿反向索引查找所有受影响的模块。

项目边界：`DependencyGraph` 只存放 owning project 的模块关系，跨项目文件不会自动并入，HMR 影响面不会越过 `.slnx` 定义的项目边界。

### DevServerOptions

```csharp
internal sealed record DevServerOptions
{
    public string RootDirectory { get; init; } = Directory.GetCurrentDirectory();
    public int Port { get; init; } = 5173;
    public string Host { get; init; } = "localhost";
    public bool OpenBrowser { get; init; }
    public bool HmrEnabled { get; init; } = true;
    public string FrontendCompiler { get; init; } = "deno";
    public TimeSpan FileChangeDebounceInterval { get; init; } = TimeSpan.FromMilliseconds(100);
    public TimeSpan FileChangePollingInterval { get; init; } = TimeSpan.FromSeconds(1);
    public IReadOnlyDictionary<string, ProxyTarget> ProxyRules { get; init; }
        = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, string> ResolveAliases { get; init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);
}
```

### DevServerOptionsParser

解析 CLI 参数和配置文件。参数覆盖顺序：CLI 参数 > 配置文件 > 默认值。

### JazorConfig

`jolt.config.json` 配置文件结构：

```csharp
internal sealed class JazorConfig
{
    public JazorServerConfig? Server { get; init; }
    public Dictionary<string, JazorProxyConfig>? Proxy { get; init; }
    public JazorResolveConfig? Resolve { get; init; }
    public JazorBuildConfig? Build { get; init; }
    public JazorExtensionsConfig? Extensions { get; init; }
}
```

### HtmlTransformer

HTML 转换：脚本注入、link 重写和资源引用重写。

```csharp
internal sealed class HtmlTransformer
{
    public string Transform(string html, string? htmlPath);
    public static string GetDevClientScript();
    public static string InjectScript(string html, string scriptPath);
    public static string InjectCss(string html, string cssPath);
    public static string RemoveDevScriptRefs(string html);
    public static string RewriteAssetReferences(string html, IReadOnlyList<AssetInfo> assets);
}
```

Transform：重写入口脚本（添加 `type="module"`），准备注入内容（Vue import map + HMR client script），注入到 `</head>` 前（回退到 `</body>` 前）。

## 核心算法

### 变更处理流程

**ProcessChangesCoreAsync**（第 124-208 行）：

1. 路由变更路径（处理 .jazor 的同伴文件）
2. 检查分类重载条件（index.html、配置文件变更 → 全页面重新加载）
3. 尝试 SFC 热更新（.jazor、.vue 文件 → 组件级 HMR）
4. 尝试脚本热更新（.ts、.js 文件 → 模块级 HMR）
5. 尝试 CSS Module 热更新（.module.css 文件 → 样式映射更新）
6. 尝试样式更新（.css 文件 → 样式刷新）
7. 回退到全页面重新加载

HMR 策略优先级（从高到低）：分类重载 > SFC 热更新 > 脚本热更新 > CSS Module 热更新 > 样式更新 > 全页面重新加载。

### SFC 热更新检测

**TryCreateSfcHotUpdateAsync**（第 398-512 行）：

1. 检查是否全部为 SFC 文件（.vue/.jazor）
2. 按文件分组处理
3. 检查缓存（无缓存则回退到全页面重新加载）
4. 重新编译
5. 计算影响路径（通过 `GetAllAffectedModules`）
6. 检查模块签名是否变更（未变则仅检查样式更新）
7. 检查是否支持 HMR
8. Jazor 文件特殊处理（`TryDiffJazorHotReload`）
9. 创建 JavaScript 热更新

### 配置解析

**Parse**（第 7-85 行）：先处理 `--dev-root` 定位配置文件，应用配置文件（`ApplyConfigFile`），再应用 CLI 参数覆盖。

**ApplyConfigFile**（第 87-170 行）：读取 `jolt.config.json`，应用服务器配置（port/host/open/hmr）、路径别名配置（`resolve.alias`）、代理配置（`proxy`）。

## 线程安全模型

**FileChangeDebouncer**：`Lock _gate` 保护 `_pendingPaths` 和 `_flushCancellationSource`。`Record` 在锁内添加路径和创建新超时。

**DependencyGraph**：`Lock _gate` 保护所有字典操作。所有公共方法都在 lock 内执行。

## 错误处理

配置文件解析错误：`JsonException` 包装为 `InvalidOperationException`。

路径别名解析错误：`TryNormalizeDependency` 跳过外部依赖、裸模块说明符、解析失败和虚拟路径。

## 配置选项

CLI 参数：

| 参数 | 说明 | 示例 |
|------|------|------|
| `--dev-root` | 项目根目录 | `--dev-root=/path/to/project` |
| `--dev-port` | HTTP 端口 | `--dev-port=3000` |
| `--dev-host` | HTTP 主机 | `--dev-host=0.0.0.0` |
| `--open-browser` | 自动打开浏览器 | `--open-browser` |
| `--no-hmr` | 禁用 HMR | `--no-hmr` |
| `--dev-frontend` | 前端编译器 | `--dev-frontend=deno` |
| `--dev-proxy` | API 代理 | `--dev-proxy=/api=http://localhost:8080` |
| `--dev-alias` | 路径别名 | `--dev-alias=@=/src` |

jolt.config.json 示例：
```json
{
  "server": {
    "port": 5173,
    "host": "localhost",
    "open": true,
    "hmr": true
  },
  "resolve": {
    "alias": {
      "@": "/src",
      "@components": "/src/components"
    }
  },
  "proxy": {
    "/api": {
      "target": "http://localhost:8080",
      "secure": false,
      "webSocket": true,
      "rewritePath": false
    }
  }
}
```

## 与其他子系统的交互

**DevHttpServer**：文件变更监听（`FileSystemWatcher` + `DevServerFileSnapshotPoller` 轮询备份），防抖器处理事件，`ChangeProcessor` 处理变更，`DevServerReloadHub` 广播 HMR 更新。

**OnDemandCompiler**：编译结果发布时记录依赖（`_dependencyGraph?.Record`），缓存失效时清理依赖（`_dependencyGraph?.Remove`）。

**DevServerReloadHub**：`BroadcastChangeResultAsync` 根据更新类型广播不同消息（StyleUpdate/JavaScriptUpdate/Error/FullReload）。

## 设计权衡

防抖间隔 100ms：对人类感知接近即时，大多数编辑器保存操作在 100ms 内完成，平衡响应速度和资源消耗。

轮询备份间隔 1s：CPU 开销可接受（单次扫描约 5-10ms），与 FileSystemWatcher 配合提供可靠保障。

依赖图规范化：过滤外部依赖和裸模块说明符（如 `vue`、`lodash`），减少依赖图大小提升性能。

Workspace 文档覆盖处理：.jazor 文件编译依赖同伴 C# 文档，优先使用 LSP 跟踪的文档版本（未保存的编辑），确保编译结果与编辑器内容一致。
