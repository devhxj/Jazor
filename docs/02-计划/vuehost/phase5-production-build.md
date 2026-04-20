# Phase 5: 生产构建 (Production Build) — 详细实施计划

## 目标

实现 `dotnet run --project Jazor.VueHost -- --build` 生产构建命令，输出优化后的静态资源到 `dist/` 目录，支持 tree shaking、code splitting、minification、CSS 提取、Source Map 链式合并。

**验收标准**: 执行构建命令后，`dist/` 目录包含：
- 压缩后的 JS bundle（带 hash）
- 提取的 CSS 文件（带 hash）
- 可选的 external Source Map
- 静态资源（图片、字体）带 content hash
- `index.html` 自动引用正确产物

---

## 一、生产构建架构

### 1.1 整体数据流

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          VueHost Build Pipeline                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────────────────────┐│
│  │ 入口分析    │────▶│ 编译阶段    │────▶│ 打包阶段 (esbuild)          ││
│  │             │     │             │     │                             ││
│  │ index.html  │     │ .jazor → JS │     │  Tree Shaking               ││
│  │ 入口 JS     │     │ .vue  → JS  │     │  Code Splitting             ││
│  │ 依赖扫描    │     │ .ts   → JS  │     │  Minification               ││
│  │             │     │ CSS 提取    │     │  Chunk 合并                 ││
│  └─────────────┘     └──────┬──────┘     │  Source Map 合并            ││
│                             │            └──────────────┬──────────────┘│
│                             │                           │               │
│                             ▼                           ▼               │
│                    ┌─────────────────┐     ┌─────────────────────────┐  │
│                    │ 编译服务        │     │ 产物后处理              │  │
│                    │ (BuildServer)   │     │                         │  │
│                    │                 │     │  Hash 重命名            │  │
│                    │ HTTP API:       │     │  HTML 注入              │  │
│                    │ /compile?id=... │     │  Source Map 写入       │  │
│                    └─────────────────┘     │  静态资源复制          │  │
│                                            └─────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
                           ┌─────────────────┐
                           │    dist/        │
                           │                 │
                           │  assets/        │
                           │    ├─ *.js      │
                           │    ├─ *.css     │
                           │    └─ *.map     │
                           │  index.html     │
                           │  vite.svg       │
                           └─────────────────┘
```

### 1.2 esbuild 集成策略

**核心问题**: esbuild 是 Go 编写的外部工具，无法直接调用 .NET 编译管道。

**解决方案**: esbuild 子进程 + HTTP 回调插件

```
┌──────────────────────────────────────────────────────────────────┐
│                        BuildOrchestrator                         │
│                       (.NET 进程)                                │
│                                                                  │
│  1. 启动 BuildServer (HTTP API)                                 │
│  2. 生成 esbuild 插件文件 (build-plugin.mjs)                    │
│  3. 启动 esbuild 子进程                                         │
│  4. 等待构建完成                                                │
│  5. 后处理产物                                                  │
└─────────────────────────┬────────────────────────────────────────┘
                          │
          ┌───────────────┼───────────────┐
          │               │               │
          ▼               ▼               ▼
    ┌───────────┐   ┌───────────┐   ┌───────────────┐
    │esbuild    │   │   HTTP    │   │  BuildServer  │
    │子进程     │──▶│  /compile │◀──│  (.NET)       │
    │           │   │           │   │               │
    │onLoad     │   │           │   │ 编译 .jazor   │
    │插件       │   │           │   │ 编译 .vue     │
    └───────────┘   └───────────┘   └───────────────┘
```

**esbuild 插件工作流**:

```javascript
// build-plugin.mjs (由 VueHost 生成)
export default {
  name: 'jazor-plugin',
  setup(build) {
    // 拦截 .jazor 和 .vue 文件
    build.onLoad({ filter: /\.(jazor|vue)$/ }, async (args) => {
      // 通过 HTTP 调用 VueHost 编译服务
      const response = await fetch(`http://localhost:${BUILD_SERVER_PORT}/compile`, {
        method: 'POST',
        body: JSON.stringify({
          id: args.path,           // 文件绝对路径
          resolveDir: rootDir
        })
      });
      
      const result = await response.json();
      
      return {
        contents: result.js,       // 编译后的 JS
        loader: 'js',
        resolveDir: rootDir,
        // Source Map 作为附加数据
        pluginData: { sourceMap: result.sourceMap }
      };
    });
    
    // 拦截 .ts 文件（可选，让 esbuild 内置处理或委托 VueHost）
    build.onLoad({ filter: /\.ts$/ }, async (args) => {
      // 方案 A: 让 esbuild 内置处理（更快）
      // 方案 B: 委托 VueHost（统一 Source Map）
      return null; // null 表示让 esbuild 默认处理
    });
  }
};
```

### 1.3 Source Map 链式合并

```
.jazor 源码
    │
    ▼ JazorVueCompiler + Deno compileSfc
┌────────────────────────────────────┐
│  staging/App.jazor.js              │
│  + App.jazor.js.map (smap1)        │
│                                    │
│  mappings: .jazor → .js            │
└──────────────────┬─────────────────┘
                   │
                   ▼ esbuild bundle
┌────────────────────────────────────┐
│  dist/assets/index-abc123.js       │
│  + index-abc123.js.map (smap2)     │
│                                    │
│  mappings: .js → bundle.js         │
│  包含对 smap1 的引用               │
└──────────────────┬─────────────────┘
                   │
                   ▼ Source Map 合并
┌────────────────────────────────────┐
│  最终 index-abc123.js.map          │
│                                    │
│  mappings: .jazor → bundle.js      │
│  sources: ["../../src/App.jazor"]  │
└────────────────────────────────────┘
```

**esbuild 的 Source Map 处理**:

esbuild 原生支持 Source Map 链：
- 当输入 JS 文件带有 `//# sourceMappingURL=` 时
- esbuild 会自动读取并合并上游 Source Map
- 输出的 Source Map 指向原始源文件

**条件**: 编译阶段必须生成 inline Source Map 或 esbuild 可访问的 external Source Map。

### 1.4 构建产物结构

```
dist/
├── index.html                    # 入口 HTML（自动注入产物引用）
├── vite.svg                      # favicon
├── assets/
│   ├── index-a1b2c3d4.js         # 入口 JS chunk（带 content hash）
│   ├── index-a1b2c3d4.js.map     # Source Map（可选）
│   ├── vendor-e5f6g7h8.js        # 第三方库 chunk（代码分割）
│   ├── vendor-e5f6g7h8.js.map
│   ├── Counter-i9j0k1l2.js       # 懒加载组件 chunk
│   ├── Counter-i9j0k1l2.js.map
│   ├── index-m3n4o5p6.css        # 提取的 CSS（带 content hash）
│   └── index-m3n4o5p6.css.map    # CSS Source Map（可选）
└── ...
```

---

## 二、新增文件清单

```
src/Jazor.VueHost/
├── Build/                              # [新建目录]
│   ├── BuildOrchestrator.cs            # 构建编排器，入口点
│   ├── BuildServer.cs                  # HTTP 编译服务（供 esbuild 插件回调）
│   ├── BuildOptions.cs                 # 构建配置选项
│   ├── BuildContext.cs                 # 构建上下文（状态管理）
│   ├── BuildResult.cs                  # 构建结果
│   ├── EsbuildRunner.cs                # esbuild 子进程管理
│   ├── EsbuildPluginGenerator.cs       # 生成 build-plugin.mjs
│   ├── AssetProcessor.cs               # 产物后处理（hash、HTML注入）
│   ├── ChunkAnalyzer.cs                # Chunk 分析（代码分割策略）
│   └── StaticAssetHandler.cs           # 静态资源处理
│
├── DevServer/
│   └── JazorConfig.cs                  # [修改] 添加 build 配置
│
└── Frontend/Deno/
    └── Protocol/
        └── BuildCompilationRequest.cs  # [新增] 编译请求/响应类型
```

---

## 三、接口与类型定义

### 3.1 BuildOptions

```csharp
// Build/BuildOptions.cs
namespace Jazor.VueHost.Build;

/// <summary>
/// 生产构建配置选项
/// </summary>
public sealed class BuildOptions
{
    /// <summary>
    /// 项目根目录（包含 index.html）
    /// </summary>
    public required string RootDirectory { get; init; }
    
    /// <summary>
    /// 输出目录，默认 "dist"
    /// </summary>
    public string OutDir { get; init; } = "dist";
    
    /// <summary>
    /// Source Map 配置
    /// </summary>
    public SourceMapOption SourceMap { get; init; } = SourceMapOption.External;
    
    /// <summary>
    /// 是否压缩代码
    /// </summary>
    public bool Minify { get; init; } = true;
    
    /// <summary>
    /// 目标环境
    /// </summary>
    public string Target { get; init; } = "es2020";
    
    /// <summary>
    /// 是否启用代码分割
    /// </summary>
    public bool CodeSplitting { get; init; } = true;
    
    /// <summary>
    /// Chunk 大小警告阈值（字节）
    /// </summary>
    public int ChunkSizeWarningLimit { get; init; } = 500_000;
    
    /// <summary>
    /// 静态资源目录名，默认 "assets"
    /// </summary>
    public string AssetsDir { get; init; } = "assets";
    
    /// <summary>
    /// 静态资源文件名 hash 长度，默认 8
    /// </summary>
    public int AssetHashLength { get; init; } = 8;
    
    /// <summary>
    /// 是否生成 Source Map（等价于 SourceMap != false）
    /// </summary>
    public bool GenerateSourceMap => SourceMap != SourceMapOption.None;
}

/// <summary>
/// Source Map 生成选项
/// </summary>
public enum SourceMapOption
{
    /// <summary>
    /// 不生成 Source Map
    /// </summary>
    None,
    
    /// <summary>
    /// 内联到 JS 文件中
    /// </summary>
    Inline,
    
    /// <summary>
    /// 生成独立的 .map 文件
    /// </summary>
    External
}
```

### 3.2 BuildContext

```csharp
// Build/BuildContext.cs
namespace Jazor.VueHost.Build;

/// <summary>
/// 构建上下文，管理构建过程中的共享状态
/// </summary>
public sealed class BuildContext : IDisposable
{
    public string RootDirectory { get; }
    public string OutDirectory { get; }
    public BuildOptions Options { get; }
    
    /// <summary>
    /// 编译结果缓存（路径 -> 编译产物）
    /// </summary>
    public ConcurrentDictionary<string, CompilationArtifact> CompilationCache { get; }
    
    /// <summary>
    /// 模块依赖图
    /// </summary>
    public DependencyGraph DependencyGraph { get; }
    
    /// <summary>
    /// 构建诊断
    /// </summary>
    public List<BuildDiagnostic> Diagnostics { get; }
    
    /// <summary>
    /// BuildServer 端口（随机分配）
    /// </summary>
    public int BuildServerPort { get; set; }
    
    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; }
    
    public BuildContext(
        BuildOptions options, 
        CancellationToken cancellationToken = default)
    {
        Options = options;
        RootDirectory = options.RootDirectory;
        OutDirectory = Path.Combine(options.RootDirectory, options.OutDir);
        CancellationToken = cancellationToken;
        CompilationCache = new();
        DependencyGraph = new();
        Diagnostics = new();
    }
    
    public void Dispose()
    {
        // 清理临时文件
    }
}
```

### 3.3 CompilationArtifact

```csharp
// Build/CompilationArtifact.cs
namespace Jazor.VueHost.Build;

/// <summary>
/// 编译产物
/// </summary>
public sealed class CompilationArtifact
{
    /// <summary>
    /// 源文件路径（绝对路径）
    /// </summary>
    public required string SourcePath { get; init; }
    
    /// <summary>
    /// 编译后的 JS 代码
    /// </summary>
    public required string JavaScript { get; init; }
    
    /// <summary>
    /// 提取的 CSS（如果有）
    /// </summary>
    public string? Css { get; init; }
    
    /// <summary>
    /// Source Map（如果有）
    /// </summary>
    public SourceMap? SourceMap { get; init; }
    
    /// <summary>
    /// 依赖的模块列表
    /// </summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    
    /// <summary>
    /// 导出的符号列表
    /// </summary>
    public IReadOnlyList<string> Exports { get; init; } = Array.Empty<string>();
}
```

### 3.4 BuildResult

```csharp
// Build/BuildResult.cs
namespace Jazor.VueHost.Build;

/// <summary>
/// 构建结果
/// </summary>
public sealed class BuildResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// 输出目录
    /// </summary>
    public string? OutDirectory { get; init; }
    
    /// <summary>
    /// 生成的入口 chunk
    /// </summary>
    public IReadOnlyList<ChunkInfo> Chunks { get; init; } = Array.Empty<ChunkInfo>();
    
    /// <summary>
    /// 生成的 CSS 文件
    /// </summary>
    public IReadOnlyList<AssetInfo> CssAssets { get; init; } = Array.Empty<AssetInfo>();
    
    /// <summary>
    /// 静态资源
    /// </summary>
    public IReadOnlyList<AssetInfo> StaticAssets { get; init; } = Array.Empty<AssetInfo>();
    
    /// <summary>
    /// 构建诊断
    /// </summary>
    public IReadOnlyList<BuildDiagnostic> Diagnostics { get; init; } = Array.Empty<BuildDiagnostic>();
    
    /// <summary>
    /// 构建耗时
    /// </summary>
    public TimeSpan Duration { get; init; }
    
    /// <summary>
    /// 总产物大小
    /// </summary>
    public long TotalSize { get; init; }
}

/// <summary>
/// Chunk 信息
/// </summary>
public sealed class ChunkInfo
{
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required long Size { get; init; }
    public bool IsEntry { get; init; }
    public bool IsDynamic { get; init; }
    public IReadOnlyList<string> Imports { get; init; } = Array.Empty<string>();
    public string? SourceMapPath { get; init; }
}

/// <summary>
/// 资源信息
/// </summary>
public sealed class AssetInfo
{
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required long Size { get; init; }
    public string? SourceMapPath { get; init; }
}

/// <summary>
/// 构建诊断
/// </summary>
public sealed class BuildDiagnostic
{
    public required DiagnosticSeverity Severity { get; init; }
    public required string Message { get; init; }
    public string? File { get; init; }
    public (int Line, int Column)? Location { get; init; }
}

public enum DiagnosticSeverity
{
    Error,
    Warning,
    Info
}
```

### 3.5 BuildServer API

```csharp
// Build/BuildServer.cs
namespace Jazor.VueHost.Build;

/// <summary>
/// 构建编译服务，供 esbuild 插件回调
/// </summary>
public sealed class BuildServer : IDisposable
{
    private readonly BuildContext _context;
    private readonly OnDemandCompiler _compiler;
    private readonly WebApplication? _app;
    
    public int Port { get; }
    public bool IsRunning { get; private set; }
    
    public BuildServer(BuildContext context, OnDemandCompiler compiler)
    {
        _context = context;
        _compiler = compiler;
        Port = GetAvailablePort();
    }
    
    /// <summary>
    /// 启动服务
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(opt => 
            opt.Listen(IPAddress.Loopback, Port));
        
        _app = builder.Build();
        
        // POST /compile
        _app.MapPost("/compile", HandleCompile);
        
        // GET /resolve?id=...
        _app.MapGet("/resolve", HandleResolve);
        
        await _app.StartAsync(cancellationToken);
        IsRunning = true;
    }
    
    /// <summary>
    /// 停止服务
    /// </summary>
    public async Task StopAsync()
    {
        if (_app != null)
            await _app.StopAsync();
        IsRunning = false;
    }
    
    // POST /compile
    private async Task<IResult> HandleCompile(
        BuildCompileRequest request,
        CancellationToken cancellationToken)
    {
        // 检查缓存
        if (_context.CompilationCache.TryGetValue(request.Id, out var cached))
        {
            return Results.Ok(new BuildCompileResponse
            {
                Js = cached.JavaScript,
                Css = cached.Css,
                SourceMap = cached.SourceMap?.ToJson(),
                Dependencies = cached.Dependencies
            });
        }
        
        // 编译
        var result = await _compiler.CompileAsync(
            request.Id, 
            _context.Options.GenerateSourceMap,
            cancellationToken);
        
        // 缓存
        _context.CompilationCache[request.Id] = result;
        
        return Results.Ok(new BuildCompileResponse
        {
            Js = result.JavaScript,
            Css = result.Css,
            SourceMap = result.SourceMap?.ToJson(),
            Dependencies = result.Dependencies
        });
    }
    
    // GET /resolve?id=...&resolveDir=...
    private IResult HandleResolve(string id, string resolveDir)
    {
        // 解析模块路径
        var resolved = ModuleResolver.Resolve(id, resolveDir, _context.Options);
        return Results.Ok(new { path = resolved?.FullPath, found = resolved != null });
    }
    
    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
    
    public void Dispose() => StopAsync().GetAwaiter().GetResult();
}

/// <summary>
/// 编译请求
/// </summary>
public sealed class BuildCompileRequest
{
    /// <summary>
    /// 文件绝对路径
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// 解析目录
    /// </summary>
    public string? ResolveDir { get; init; }
    
    /// <summary>
    /// 是否生成 Source Map
    /// </summary>
    public bool SourceMap { get; init; } = true;
}

/// <summary>
/// 编译响应
/// </summary>
public sealed class BuildCompileResponse
{
    /// <summary>
    /// 编译后的 JS
    /// </summary>
    public required string Js { get; init; }
    
    /// <summary>
    /// 提取的 CSS
    /// </summary>
    public string? Css { get; init; }
    
    /// <summary>
    /// Source Map JSON
    /// </summary>
    public string? SourceMap { get; init; }
    
    /// <summary>
    /// 依赖模块列表
    /// </summary>
    public IReadOnlyList<string>? Dependencies { get; init; }
}
```

### 3.6 BuildOrchestrator

```csharp
// Build/BuildOrchestrator.cs
namespace Jazor.VueHost.Build;

/// <summary>
/// 构建编排器，构建流程入口
/// </summary>
public sealed class BuildOrchestrator
{
    private readonly OnDemandCompiler _compiler;
    
    public BuildOrchestrator(OnDemandCompiler compiler)
    {
        _compiler = compiler;
    }
    
    /// <summary>
    /// 执行生产构建
    /// </summary>
    public async Task<BuildResult> BuildAsync(
        BuildOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var diagnostics = new List<BuildDiagnostic>();
        
        try
        {
            // 1. 创建构建上下文
            using var context = new BuildContext(options, cancellationToken);
            
            // 2. 启动 BuildServer
            var buildServer = new BuildServer(context, _compiler);
            await buildServer.StartAsync(cancellationToken);
            context.BuildServerPort = buildServer.Port;
            
            try
            {
                // 3. 生成 esbuild 插件
                var pluginGenerator = new EsbuildPluginGenerator(context);
                var pluginPath = await pluginGenerator.GenerateAsync();
                
                // 4. 运行 esbuild
                var esbuildRunner = new EsbuildRunner(context);
                var esbuildResult = await esbuildRunner.RunAsync(pluginPath, cancellationToken);
                
                if (!esbuildResult.Success)
                {
                    return new BuildResult
                    {
                        Success = false,
                        Diagnostics = esbuildResult.Errors
                    };
                }
                
                // 5. 后处理产物
                var processor = new AssetProcessor(context);
                var assets = await processor.ProcessAsync(esbuildResult, cancellationToken);
                
                // 6. 生成 HTML
                await GenerateHtmlAsync(context, assets, cancellationToken);
                
                stopwatch.Stop();
                
                return new BuildResult
                {
                    Success = true,
                    OutDirectory = context.OutDirectory,
                    Chunks = assets.Chunks,
                    CssAssets = assets.CssAssets,
                    StaticAssets = assets.StaticAssets,
                    Diagnostics = diagnostics,
                    Duration = stopwatch.Elapsed,
                    TotalSize = assets.TotalSize
                };
            }
            finally
            {
                await buildServer.StopAsync();
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = ex.Message
            });
            
            return new BuildResult
            {
                Success = false,
                Diagnostics = diagnostics,
                Duration = stopwatch.Elapsed
            };
        }
    }
    
    private async Task GenerateHtmlAsync(
        BuildContext context, 
        ProcessedAssets assets,
        CancellationToken cancellationToken)
    {
        // 读取入口 HTML
        var htmlPath = Path.Combine(context.RootDirectory, "index.html");
        var html = await File.ReadAllTextAsync(htmlPath, cancellationToken);
        
        // 替换 script 引用
        var entryChunk = assets.Chunks.FirstOrDefault(c => c.IsEntry);
        if (entryChunk != null)
        {
            html = HtmlTransformer.InjectScript(html, $"/{entryChunk.FileName}");
        }
        
        // 替换 CSS 引用
        foreach (var css in assets.CssAssets)
        {
            html = HtmlTransformer.InjectCss(html, $"/{css.FileName}");
        }
        
        // 写入 dist/index.html
        var outPath = Path.Combine(context.OutDirectory, "index.html");
        await File.WriteAllTextAsync(outPath, html, cancellationToken);
    }
}
```

### 3.7 EsbuildRunner

```csharp
// Build/EsbuildRunner.cs
namespace Jazor.VueHost.Build;

/// <summary>
/// esbuild 子进程运行器
/// </summary>
public sealed class EsbuildRunner
{
    private readonly BuildContext _context;
    
    public EsbuildRunner(BuildContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 运行 esbuild 构建
    /// </summary>
    public async Task<EsbuildResult> RunAsync(
        string pluginPath, 
        CancellationToken cancellationToken)
    {
        // 查找 esbuild
        var esbuildPath = FindEsbuild();
        if (esbuildPath == null)
        {
            return new EsbuildResult
            {
                Success = false,
                Errors = new[]
                {
                    new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Error,
                        Message = "esbuild not found. Please install esbuild: npm install -D esbuild"
                    }
                }
            };
        }
        
        // 生成 esbuild 配置
        var configPath = await GenerateEsbuildConfigAsync(pluginPath);
        
        // 启动子进程
        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"--experimental-vm-modules {configPath}",
            WorkingDirectory = _context.RootDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        
        process.OutputDataReceived += (_, e) => 
        { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => 
        { if (e.Data != null) errorBuilder.AppendLine(e.Data); };
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await process.WaitForExitAsync(cancellationToken);
        
        if (process.ExitCode != 0)
        {
            return new EsbuildResult
            {
                Success = false,
                Errors = ParseErrors(errorBuilder.ToString())
            };
        }
        
        // 解析构建结果
        return ParseResult(outputBuilder.ToString());
    }
    
    private string? FindEsbuild()
    {
        // 优先使用 node_modules/.bin/esbuild
        var localEsbuild = Path.Combine(
            _context.RootDirectory, 
            "node_modules", ".bin", 
            OperatingSystem.IsWindows() ? "esbuild.cmd" : "esbuild");
        
        if (File.Exists(localEsbuild))
            return localEsbuild;
        
        // 尝试全局 esbuild
        var globalEsbuild = OperatingSystem.IsWindows() 
            ? "esbuild.cmd" 
            : "esbuild";
        
        if (FindOnPath(globalEsbuild) is { } found)
            return found;
        
        return null;
    }
    
    private async Task<string> GenerateEsbuildConfigAsync(string pluginPath)
    {
        var configPath = Path.Combine(_context.RootDirectory, ".jazor", "esbuild.config.mjs");
        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        
        var config = $@"
import * as esbuild from 'esbuild';
import jazorPlugin from '{pluginPath}';

const config = {{
  entryPoints: ['src/main.js'],
  bundle: true,
  outdir: '{_context.Options.OutDir}',
  outbase: 'src',
  splitting: {_context.Options.CodeSplitting.ToString().ToLower()},
  format: 'esm',
  target: ['{_context.Options.Target}'],
  minify: {_context.Options.Minify.ToString().ToLower()},
  sourcemap: {_context.Options.SourceMap switch
  {
      SourceMapOption.None => "false",
      SourceMapOption.Inline => "'inline'",
      SourceMapOption.External => "true",
      _ => "true"
  }},
  metafile: true,
  plugins: [jazorPlugin],
  assetNames: '{_context.Options.AssetsDir}/[name]-[hash]',
  chunkNames: '{_context.Options.AssetsDir}/[name]-[hash]',
  publicPath: '/',
  define: {{
    'process.env.NODE_ENV': JSON.stringify('production')
  }}
}};

try {{
  const result = await esbuild.build(config);
  console.log(JSON.stringify(result.metafile));
}} catch (error) {{
  console.error(error.message);
  process.exit(1);
}}
";
        
        await File.WriteAllTextAsync(configPath, config);
        return configPath;
    }
    
    private static string? FindOnPath(string name)
    {
        // 使用 `where` (Windows) 或 `which` (Unix)
        var cmd = OperatingSystem.IsWindows() ? "where" : "which";
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = cmd,
                Arguments = name,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            return process?.StandardOutput.ReadLine();
        }
        catch
        {
            return null;
        }
    }
    
    private IReadOnlyList<BuildDiagnostic> ParseErrors(string stderr)
    {
        // 解析 esbuild 错误输出
        var errors = new List<BuildDiagnostic>();
        // TODO: 解析 JSON 格式的 esbuild 错误
        if (!string.IsNullOrEmpty(stderr))
        {
            errors.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = stderr
            });
        }
        return errors;
    }
    
    private EsbuildResult ParseResult(string metafileJson)
    {
        // 解析 esbuild metafile
        // TODO: 实现解析逻辑
        return new EsbuildResult { Success = true };
    }
}

public sealed class EsbuildResult
{
    public bool Success { get; init; }
    public IReadOnlyList<BuildDiagnostic> Errors { get; init; } = Array.Empty<BuildDiagnostic>();
    public IReadOnlyDictionary<string, EsbuildOutputFile>? Outputs { get; init; }
}

public sealed class EsbuildOutputFile
{
    public required string Path { get; init; }
    public required string[] Inputs { get; init; }
    public required EsbuildOutputInfo Info { get; init; }
}

public sealed class EsbuildOutputInfo
{
    public required long Bytes { get; init; }
    public required string[] Imports { get; init; }
    public required string[] Exports { get; init; }
}
```

### 3.8 EsbuildPluginGenerator

```csharp
// Build/EsbuildPluginGenerator.cs
namespace Jazor.VueHost.Build;

/// <summary>
/// 生成 esbuild 插件代码
/// </summary>
public sealed class EsbuildPluginGenerator
{
    private readonly BuildContext _context;
    
    public EsbuildPluginGenerator(BuildContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 生成插件文件
    /// </summary>
    public async Task<string> GenerateAsync()
    {
        var pluginPath = Path.Combine(
            _context.RootDirectory, 
            ".jazor", 
            "build-plugin.mjs");
        
        Directory.CreateDirectory(Path.GetDirectoryName(pluginPath)!);
        
        var pluginCode = $@"
// Auto-generated by VueHost BuildOrchestrator
// Do not edit manually

const BUILD_SERVER_URL = 'http://localhost:{_context.BuildServerPort}';

/**
 * @type {{import('esbuild').Plugin}}
 */
export default {{
  name: 'jazor-plugin',
  
  setup(build) {{
    // 拦截 .jazor 文件
    build.onLoad({{ filter: /\.jazor$/ }}, async (args) => {{
      return await compileFile(args.path, args.resolveDir);
    }});
    
    // 拦截 .vue 文件
    build.onLoad({{ filter: /\.vue$/ }}, async (args) => {{
      return await compileFile(args.path, args.resolveDir);
    }});
    
    // 解析裸模块（可选，如果需要特殊处理）
    build.onResolve({{ filter: /^[\w@]/ }}, async (args) => {{
      // 委托 esbuild 默认解析
      return null;
    }});
  }}
}};

/**
 * 通过 HTTP 调用 VueHost 编译服务
 */
async function compileFile(id, resolveDir) {{
  try {{
    const response = await fetch(`${{BUILD_SERVER_URL}}/compile`, {{
      method: 'POST',
      headers: {{ 'Content-Type': 'application/json' }},
      body: JSON.stringify({{
        id,
        resolveDir,
        sourceMap: {_context.Options.GenerateSourceMap.ToString().ToLower()}
      }})
    }});
    
    if (!response.ok) {{
      return {{ errors: [{{ text: `Compile failed: ${{response.status}}` }}] }};
    }}
    
    const result = await response.json();
    
    // 构建 Source Map data URL（如果需要 inline）
    let contents = result.js;
    if (result.sourceMap && '{_context.Options.SourceMap}' === 'Inline') {{
      const base64 = Buffer.from(result.sourceMap).toString('base64');
      contents += `\n//# sourceMappingURL=data:application/json;base64,${{base64}}\n`;
    }}
    
    return {{
      contents,
      loader: 'js',
      resolveDir
    }};
  }} catch (error) {{
    return {{ errors: [{{ text: error.message }}] }};
  }}
}}
";
        
        await File.WriteAllTextAsync(pluginPath, pluginCode);
        return pluginPath;
    }
}
```

---

## 四、jazor.config.json 扩展

```jsonc
{
    "server": {
        "port": 5173,
        "host": "localhost",
        "open": false,
        "hmr": true
    },
    "proxy": {
        "/api": {
            "target": "http://localhost:5000",
            "secure": false,
            "websocket": true
        }
    },
    "resolve": {
        "alias": {
            "@": "./src"
        }
    },
    "build": {
        // 输出目录
        "outDir": "dist",
        
        // Source Map: "inline" | "external" | false
        "sourcemap": "external",
        
        // 是否压缩
        "minify": true,
        
        // 目标环境
        "target": "es2020",
        
        // 代码分割
        "codeSplitting": true,
        
        // 静态资源目录
        "assetsDir": "assets",
        
        // 文件名 hash 长度
        "assetHashLength": 8,
        
        // Chunk 大小警告阈值（字节）
        "chunkSizeWarningLimit": 500000,
        
        // 手动 chunk 分割
        "rollupOptions": {
            "output": {
                "manualChunks": {
                    "vue": ["vue"],
                    "vendor": ["axios", "lodash-es"]
                }
            }
        }
    }
}
```

```csharp
// DevServer/JazorConfig.cs — 扩展
public sealed class JazorConfig
{
    public ServerConfig? Server { get; init; }
    public ResolveConfig? Resolve { get; init; }
    public Dictionary<string, ProxyConfig>? Proxy { get; init; }
    public BuildConfig? Build { get; init; }  // 新增
}

/// <summary>
/// 生产构建配置
/// </summary>
public sealed class BuildConfig
{
    /// <summary>
    /// 输出目录，默认 "dist"
    /// </summary>
    public string OutDir { get; init; } = "dist";
    
    /// <summary>
    /// Source Map: "inline" | "external" | false
    /// </summary>
    public string? SourceMap { get; init; }
    
    /// <summary>
    /// 是否压缩，默认 true
    /// </summary>
    public bool Minify { get; init; } = true;
    
    /// <summary>
    /// 目标环境，默认 "es2020"
    /// </summary>
    public string Target { get; init; } = "es2020";
    
    /// <summary>
    /// 是否启用代码分割，默认 true
    /// </summary>
    public bool CodeSplitting { get; init; } = true;
    
    /// <summary>
    /// 静态资源目录，默认 "assets"
    /// </summary>
    public string AssetsDir { get; init; } = "assets";
    
    /// <summary>
    /// 文件名 hash 长度，默认 8
    /// </summary>
    public int AssetHashLength { get; init; } = 8;
    
    /// <summary>
    /// Chunk 大小警告阈值（字节），默认 500000
    /// </summary>
    public int ChunkSizeWarningLimit { get; init; } = 500_000;
    
    /// <summary>
    /// 转换为 BuildOptions
    /// </summary>
    public BuildOptions ToBuildOptions(string rootDirectory)
    {
        return new BuildOptions
        {
            RootDirectory = rootDirectory,
            OutDir = OutDir,
            SourceMap = SourceMap?.ToLowerInvariant() switch
            {
                "inline" => SourceMapOption.Inline,
                "external" => SourceMapOption.External,
                "false" => SourceMapOption.None,
                _ => SourceMapOption.External
            },
            Minify = Minify,
            Target = Target,
            CodeSplitting = CodeSplitting,
            AssetsDir = AssetsDir,
            AssetHashLength = AssetHashLength,
            ChunkSizeWarningLimit = ChunkSizeWarningLimit
        };
    }
}
```

---

## 五、CSS 提取策略

### 5.1 esbuild 内置 CSS 处理

esbuild 原生支持 CSS：
- 从 JS 中 `import './style.css'` 自动提取
- 支持 CSS bundle 和 minification
- 支持 CSS Source Map

**配置**:

```javascript
// esbuild 配置
{
  loader: { '.css': 'css' },
  // CSS 输出与 JS 同目录
}
```

### 5.2 Vue SFC 样式提取

Vue SFC 中的 `<style>` 块：
1. 编译阶段：`compileSfc` 返回 CSS 文本
2. 打包阶段：通过 `onLoad` 返回 CSS，esbuild 自动处理

```javascript
// build-plugin.mjs
async function compileFile(id, resolveDir) {
  const result = await fetch(...);
  
  // 如果有 CSS，创建虚拟 CSS 模块
  if (result.css) {
    // 方案 A: 将 CSS 作为 JS 副作用注入
    const cssModule = `
      import { injectCss } from 'jazor/runtime';
      injectCss(${JSON.stringify(result.css)}, ${JSON.stringify(id)});
    `;
    
    return {
      contents: cssModule,
      loader: 'js'
    };
  }
  
  return {
    contents: result.js,
    loader: 'js'
  };
}
```

### 5.3 CSS 运行时注入（开发模式）

```javascript
// DevServer/Client/jazor-hmr.js
export function injectCss(css, id) {
  const style = document.createElement('style');
  style.setAttribute('data-jazor-id', id);
  style.textContent = css;
  document.head.appendChild(style);
}
```

### 5.4 CSS 提取（生产模式）

esbuild 会自动将 CSS 提取到单独文件：

```javascript
// esbuild 配置
{
  loader: { '.css': 'css' },
  // 输出: assets/index-abc123.css
}
```

---

## 六、静态资源处理

### 6.1 资源类型

| 类型 | 处理方式 | 输出 |
|------|---------|------|
| 图片 `.png/.jpg/.gif/.svg/.webp` | 复制 + hash 重命名 | `assets/[name]-[hash].[ext]` |
| 字体 `.woff/.woff2/.ttf/.eot` | 复制 + hash 重命名 | `assets/[name]-[hash].[ext]` |
| 媒体 `.mp4/.webm/.mp3/.ogg` | 复制 + hash 重命名 | `assets/[name]-[hash].[ext]` |
| JSON `.json` | 内联或复制 | 取决于大小 |
| 其他 | 原样复制 | `assets/` |

### 6.2 esbuild 资源处理

```javascript
// esbuild 配置
{
  loader: {
    '.png': 'file',
    '.jpg': 'file',
    '.gif': 'file',
    '.svg': 'file',
    '.woff': 'file',
    '.woff2': 'file',
    '.ttf': 'file',
    '.mp4': 'file',
    '.webm': 'file',
    '.json': 'json'
  },
  assetNames: 'assets/[name]-[hash]',
  publicPath: '/'
}
```

### 6.3 公共资源复制

```csharp
// Build/StaticAssetHandler.cs
public sealed class StaticAssetHandler
{
    private readonly BuildContext _context;
    
    /// <summary>
    /// 复制公共静态资源到 dist/
    /// </summary>
    public async Task<IReadOnlyList<AssetInfo>> CopyPublicAssetsAsync(
        CancellationToken cancellationToken)
    {
        var assets = new List<AssetInfo>();
        var publicDir = Path.Combine(_context.RootDirectory, "public");
        
        if (!Directory.Exists(publicDir))
            return assets;
        
        foreach (var file in Directory.EnumerateFiles(publicDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(publicDir, file);
            var destPath = Path.Combine(_context.OutDirectory, relativePath);
            
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            
            // 小文件添加 hash
            var fileInfo = new FileInfo(file);
            if (fileInfo.Length < 4 * 1024 && ShouldHash(relativePath))
            {
                var hash = ComputeFileHash(file);
                var newName = $"{Path.GetFileNameWithoutExtension(relativePath)}-{hash}{Path.GetExtension(relativePath)}";
                destPath = Path.Combine(_context.OutDirectory, newName);
            }
            
            File.Copy(file, destPath, overwrite: true);
            
            assets.Add(new AssetInfo
            {
                FileName = Path.GetRelativePath(_context.OutDirectory, destPath),
                FilePath = destPath,
                Size = fileInfo.Length
            });
        }
        
        return assets;
    }
    
    private static bool ShouldHash(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".gif" or ".svg" or ".webp" 
            or ".woff" or ".woff2" or ".ttf";
    }
    
    private static string ComputeFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash[..4]).ToLowerInvariant();
    }
}
```

---

## 七、Source Map 链式合并

### 7.1 编译阶段 Source Map

Phase 2 已实现 `SourceMapGenerator`，编译产物包含 inline Source Map。

**关键**: esbuild 需要 Source Map 可访问。

**方案**: 编译阶段生成 inline Source Map（嵌入 JS 中）

```csharp
// OnDemandCompiler.CompileAsync
if (generateSourceMap)
{
    var sourceMapJson = sourceMap.ToJson();
    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceMapJson));
    js += $"\n//# sourceMappingURL=data:application/json;base64,{base64}\n";
}
```

### 7.2 esbuild Source Map 合并

esbuild 原生支持 Source Map 链：

```javascript
// esbuild 配置
{
  sourcemap: true,  // 或 'inline'
  // esbuild 会自动读取输入文件中的 sourceMappingURL
  // 并合并到输出的 Source Map 中
}
```

### 7.3 验证 Source Map 链

```javascript
// 测试脚本
const { SourceMapConsumer } = require('source-map');
const fs = require('fs');

async function verifySourceMap() {
  const bundleMap = JSON.parse(fs.readFileSync('dist/assets/index-abc123.js.map'));
  const consumer = await new SourceMapConsumer(bundleMap);
  
  // 测试映射: bundle.js:1042 → .jazor:15
  const original = consumer.originalPositionFor({
    line: 1042,
    column: 5
  });
  
  console.log(original);
  // 应输出: { source: '../../src/App.jazor', line: 15, column: 10 }
}
```

---

## 八、代码分割策略

### 8.1 自动分割

esbuild 自动进行代码分割：

```
src/
├── main.js              # 入口
├── App.jazor            # 主应用
└── components/
    ├── Counter.jazor    # 懒加载组件
    └── Modal.jazor      # 懒加载组件

dist/assets/
├── index-a1b2c3d4.js    # 主入口 chunk
├── Counter-e5f6g7h8.js  # 懒加载 chunk
├── Modal-i9j0k1l2.js    # 懒加载 chunk
└── vendor-m3n4o5p6.js   # 第三方库 chunk
```

### 8.2 动态导入

```javascript
// main.js
import { createApp } from 'vue';
import App from './App.jazor';

const app = createApp(App);

// 懒加载组件
app.component('Counter', () => import('./components/Counter.jazor'));
app.component('Modal', () => import('./components/Modal.jazor'));

app.mount('#app');
```

### 8.3 手动 Chunk 分割

```jsonc
// jazor.config.json
{
  "build": {
    "rollupOptions": {
      "output": {
        "manualChunks": {
          "vue": ["vue", "vue-router", "pinia"],
          "ui": ["element-plus", "@element-plus/icons-vue"]
        }
      }
    }
  }
}
```

### 8.4 Chunk 大小警告

```csharp
// Build/AssetProcessor.cs
public sealed class AssetProcessor
{
    private void CheckChunkSizes(IReadOnlyList<ChunkInfo> chunks)
    {
        foreach (var chunk in chunks)
        {
            if (chunk.Size > _context.Options.ChunkSizeWarningLimit)
            {
                _context.Diagnostics.Add(new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Warning,
                    Message = $"Chunk '{chunk.FileName}' is {chunk.Size / 1024:N0} KB, " +
                              $"exceeds recommended limit of {_context.Options.ChunkSizeWarningLimit / 1024:N0} KB. " +
                              "Consider code splitting."
                });
            }
        }
    }
}
```

---

## 九、HTML 处理

### 9.1 入口 HTML 解析

```csharp
// DevServer/HtmlTransformer.cs (扩展)
public static class HtmlTransformer
{
    /// <summary>
    /// 注入 script 标签
    /// </summary>
    public static string InjectScript(string html, string scriptPath)
    {
        // 移除开发时的 script 引用
        html = RemoveDevScriptRefs(html);
        
        // 在 </body> 前注入
        var scriptTag = $"<script type=\"module\" src=\"{scriptPath}\"></script>";
        return html.Replace("</body>", $"{scriptTag}\n</body>");
    }
    
    /// <summary>
    /// 注入 CSS link 标签
    /// </summary>
    public static string InjectCss(string html, string cssPath)
    {
        var linkTag = $"<link rel=\"stylesheet\" href=\"{cssPath}\">";
        
        // 在 </head> 前注入
        if (html.Contains("</head>"))
        {
            return html.Replace("</head>", $"{linkTag}\n</head>");
        }
        
        // 没有 </head>，在 <body> 前注入
        return html.Replace("<body>", $"{linkTag}\n<body>");
    }
    
    private static string RemoveDevScriptRefs(string html)
    {
        // 移除指向 /src/ 的 script 标签
        var pattern = @"<script[^>]*src=[""'][^""']*src/[^""']*[""'][^>]*>\s*</script>";
        return Regex.Replace(html, pattern, "", RegexOptions.IgnoreCase);
    }
}
```

### 9.2 环境变量注入

```csharp
// 生成环境变量定义
{
  "define": {
    "import.meta.env.PROD": "true",
    "import.meta.env.DEV": "false",
    "import.meta.env.MODE": "\"production\"",
    "import.meta.env.BASE_URL": "\"/\""
  }
}
```

---

## 十、命令行接口

### 10.1 构建命令

```bash
# 开发模式
dotnet run --project Jazor.VueHost -- --dev

# 生产构建
dotnet run --project Jazor.VueHost -- --build

# 指定配置
dotnet run --project Jazor.VueHost -- --build --config ./jazor.prod.json

# 预览构建产物
dotnet run --project Jazor.VueHost -- --preview
```

### 10.2 Program.cs 扩展

```csharp
// Program.cs
var builder = ConsoleApp.CreateBuilder(args);
builder.Services.AddSingleton<OnDemandCompiler>();
builder.Services.AddSingleton<BuildOrchestrator>();

var app = builder.Build();
app.AddCommand("dev", DevCommand);
app.AddCommand("build", BuildCommand);
app.AddCommand("preview", PreviewCommand);

await app.RunAsync();

static async Task<int> DevCommand(
    [Option("-p", "--port")] int? port,
    [Option("-h", "--host")] string? host,
    CancellationToken cancellationToken)
{
    var config = JazorConfigLoader.Load(Directory.GetCurrentDirectory());
    var options = new DevServerOptions
    {
        Port = port ?? config.Server?.Port ?? 5173,
        Host = host ?? config.Server?.Host ?? "localhost"
    };
    
    var server = new DevHttpServer(options);
    await server.StartAsync(cancellationToken);
    return 0;
}

static async Task<int> BuildCommand(
    [Option("-c", "--config")] string? configPath,
    [Option("--sourcemap")] string? sourcemap,
    [Option("--minify")] bool? minify,
    CancellationToken cancellationToken)
{
    var rootDir = Directory.GetCurrentDirectory();
    var config = JazorConfigLoader.Load(rootDir, configPath);
    var options = config.Build?.ToBuildOptions(rootDir) ?? new BuildOptions { RootDirectory = rootDir };
    
    // 命令行参数覆盖配置文件
    if (sourcemap != null)
    {
        options = options with { SourceMap = sourcemap.ToLowerInvariant() switch
        {
            "inline" => SourceMapOption.Inline,
            "external" => SourceMapOption.External,
            "false" => SourceMapOption.None,
            _ => options.SourceMap
        }};
    }
    
    if (minify.HasValue)
    {
        options = options with { Minify = minify.Value };
    }
    
    var compiler = new OnDemandCompiler(...);
    var orchestrator = new BuildOrchestrator(compiler);
    var result = await orchestrator.BuildAsync(options, cancellationToken);
    
    if (!result.Success)
    {
        Console.Error.WriteLine("Build failed:");
        foreach (var diag in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
        {
            Console.Error.WriteLine($"  {diag.Message}");
        }
        return 1;
    }
    
    Console.WriteLine($"Build completed in {result.Duration.TotalSeconds:F2}s");
    Console.WriteLine($"Output: {result.OutDirectory}");
    Console.WriteLine($"Total size: {result.TotalSize / 1024:N0} KB");
    
    return 0;
}

static async Task<int> PreviewCommand(
    [Option("-p", "--port")] int? port,
    CancellationToken cancellationToken)
{
    var rootDir = Directory.GetCurrentDirectory();
    var distDir = Path.Combine(rootDir, "dist");
    
    if (!Directory.Exists(distDir))
    {
        Console.Error.WriteLine("dist/ directory not found. Run 'build' first.");
        return 1;
    }
    
    var previewPort = port ?? 4173;
    Console.WriteLine($"Preview server running at http://localhost:{previewPort}");
    
    // 使用 Kestrel 静态文件服务
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.ConfigureKestrel(opt => opt.ListenAnyIP(previewPort));
    
    var app = builder.Build();
    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(distDir),
        RequestPath = "",
        EnableDirectoryBrowsing = false
    });
    
    // SPA fallback
    app.MapFallbackToFile("index.html");
    
    await app.RunAsync(cancellationToken);
    return 0;
}
```

---

## 十一、实施步骤（严格顺序）

### Step 1: BuildOptions + BuildContext + BuildResult 数据模型

**产出文件**:
- 新增 `Build/BuildOptions.cs`
- 新增 `Build/BuildContext.cs`
- 新增 `Build/BuildResult.cs`
- 新增 `Build/CompilationArtifact.cs`

**不依赖外部组件**，纯数据模型定义。

**测试**:
- BuildOptions 默认值正确
- BuildContext 初始化成功
- BuildResult 序列化/反序列化

**退出标准**: 数据模型定义完成，单元测试通过。

---

### Step 2: JazorConfig 扩展 + BuildConfig

**产出文件**:
- 修改 `DevServer/JazorConfig.cs` — 添加 BuildConfig
- 新增 `Build/BuildConfig.cs`

**测试**:
- 解析完整 jazor.config.json
- BuildConfig.ToBuildOptions() 转换正确
- 默认值与配置文件合并正确

**退出标准**: 配置解析正确，命令行可读取 build 配置。

---

### Step 3: BuildServer HTTP API

**产出文件**:
- 新增 `Build/BuildServer.cs`
- 新增 `Build/BuildCompileRequest.cs`
- 新增 `Build/BuildCompileResponse.cs`

**依赖**: Phase 1 的 `OnDemandCompiler`

**测试**:
- 启动 BuildServer
- POST /compile 返回编译结果
- 缓存命中测试

**退出标准**: HTTP 服务可响应编译请求。

---

### Step 4: EsbuildPluginGenerator

**产出文件**:
- 新增 `Build/EsbuildPluginGenerator.cs`

**测试**:
- 生成的插件代码语法正确
- 插件能正确引用 BuildServer URL

**退出标准**: 生成的 `build-plugin.mjs` 可被 esbuild 加载。

---

### Step 5: EsbuildRunner

**产出文件**:
- 新增 `Build/EsbuildRunner.cs`
- 新增 `Build/EsbuildResult.cs`

**测试**:
- 检测 esbuild 安装
- 启动 esbuild 子进程
- 解析构建结果

**退出标准**: 可运行 esbuild 构建简单项目。

---

### Step 6: StaticAssetHandler

**产出文件**:
- 新增 `Build/StaticAssetHandler.cs`

**测试**:
- 复制 public/ 目录文件
- hash 重命名正确
- 大小计算正确

**退出标准**: 静态资源正确复制到 dist/。

---

### Step 7: AssetProcessor

**产出文件**:
- 新增 `Build/AssetProcessor.cs`

**测试**:
- 解析 esbuild metafile
- 收集 chunk 信息
- 收集 CSS 信息

**退出标准**: 产物信息正确收集。

---

### Step 8: BuildOrchestrator 集成

**产出文件**:
- 新增 `Build/BuildOrchestrator.cs`

**依赖**: Step 1-7 所有组件

**测试**:
- 端到端构建测试
- 构建简单 .jazor 项目
- 验证 dist/ 产物

**退出标准**: 完整构建流程可运行。

---

### Step 9: 命令行集成

**产出文件**:
- 修改 `Program.cs` — 添加 build 和 preview 命令

**测试**:
- `--build` 命令执行构建
- `--preview` 命令启动预览服务器
- 错误处理正确

**退出标准**: 命令行接口可用。

---

### Step 10: 端到端测试

**测试场景**:
1. 简单项目构建
2. 多组件项目构建
3. CSS 提取
4. Source Map 链验证
5. 代码分割验证
6. 大项目构建性能

**退出标准**: 所有测试场景通过。

---

## 十二、风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| **esbuild 不兼容** | 构建失败 | 版本锁定 + 兼容性测试；提供 Rollup 备选 |
| **Source Map 链断裂** | 调试不可用 | 每阶段验证 Source Map；提供诊断工具 |
| **CSS 提取失败** | 样式丢失 | esbuild 内置 CSS 处理；回退到 inline CSS |
| **构建性能差** | 开发体验差 | 增量编译缓存；并行处理 |
| **内存占用高** | 大项目崩溃 | 分批编译；流式处理 |
| **跨平台问题** | 部分系统不可用 | CI 多平台测试；路径处理规范化 |
| **插件通信失败** | 编译中断 | 重试机制 + 超时处理；详细错误日志 |
| **产物损坏** | 运行时错误 | 构建后校验；产物完整性检查 |

---

## 十三、后续优化方向

### 13.1 增量构建

- 文件变更检测
- 只重新编译变更文件
- 缓存持久化

### 13.2 构建分析

- Bundle 大小分析
- 依赖可视化
- 性能瓶颈定位

### 13.3 插件生态

- 支持自定义 esbuild 插件
- 支持构建钩子
- 支持自定义 loader

### 13.4 多环境支持

- SSR 构建
- Library 模式
- 多页面应用

---

**文档维护者**: developerhan  
**最后更新**: 2026-04-15  
**文档版本**: v1.0
