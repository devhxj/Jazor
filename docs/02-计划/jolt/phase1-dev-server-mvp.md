# Phase 1: Dev Server MVP — 详细实施计划

## 目标

实现 `dotnet run --project Jolt -- --dev` 启动 HTTP 开发服务器，浏览器可访问并运行 `.jazor` 编写的 Vue 应用。

**验收标准**: 浏览器打开 `http://localhost:5173`，加载 `index.html`，其中引用的 `.jazor` 组件被实时编译为 JS 模块并正确渲染。

---

## 一、编译管道分析

### 1.1 现有编译链路

```
.jazor 源码
    │
    ▼  JazorVueParser.Parse()
JazorVueDocument { Template, Code, Imports, ... }
    │
    ▼  JazorVueCompiler.Compile()
JazorVueCompilationResult {
    GeneratedVueText: "<script setup>...<template>..."   ← Vue SFC 文本
    ExternalSymbols: VirtualExternalSymbolTable
    Diagnostics: string[]
}
```

**关键问题**: `JazorVueCompiler.Compile()` 输出的是 **Vue SFC 文本**（包含 `<script setup>` + `<template>`），浏览器不能直接运行。需要一个额外的 **Vue SFC → JS ESM** 编译步骤。

### 1.2 Dev Server 所需的完整管道

```
.jazor 源码
    │
    ▼  Step 1: JazorVueCompiler.Compile()  [已有，.NET 进程内]
Vue SFC 文本
    │
    ▼  Step 2: Vue SFC → JS ESM            [新增，委托 Deno Worker]
JS ESM 模块
    │
    ▼  Step 3: HTTP 响应                     [新增，Kestrel]
浏览器接收并执行
```

### 1.3 各文件类型的编译路由

| 文件类型 | 编译方式 | 产出 |
|---------|---------|------|
| `.jazor` | `JazorVueCompiler` → Deno `compileSfc` | JS ESM |
| `.vue` | Deno `compileSfc` | JS ESM |
| `.ts` | Deno 内置 TypeScript 转译 | JS ESM |
| `.js` | 原样返回 | JS ESM |
| `.css` | 原样返回 + 可选注入 | CSS |
| `index.html` | 转换 script 引用 | HTML |

---

## 二、新增文件清单

### 2.1 目录结构

```
src/Jolt/
├── DevServer/                          # [新建目录]
│   ├── DevHttpServer.cs                # Kestrel HTTP 服务器
│   ├── DevServerOptions.cs             # 配置选项
│   ├── DevServerMiddleware.cs          # 请求路由中间件
│   ├── ModuleResolver.cs               # ESM 模块路径解析
│   ├── OnDemandCompiler.cs             # 按需编译服务
│   ├── CompilationCache.cs             # 编译结果缓存
│   ├── HtmlTransformer.cs              # HTML 入口转换
│   ├── DependencyGraph.cs              # 模块依赖图
│   └── Client/
│       └── jazor-hmr.js                # 客户端 HMR runtime (基础版)
│
├── Frontend/Deno/Protocol/
│   └── DenoCompilationProtocol.cs      # [新增] 编译请求/响应类型
│
├── Frontend/Deno/Worker/
│   └── frontend-worker.ts              # [修改] 添加 compileSfc 方法
│
└── Program.cs                          # [修改] 添加 --dev 入口
```

### 2.2 修改文件清单

| 文件 | 修改内容 |
|------|---------|
| `Program.cs` | 添加 `--dev` 模式分支 |
| `Jolt.csproj` | 嵌入 `Client/jazor-hmr.js` 为资源 |
| `Frontend/Deno/Hosting/IDenoVolarHost.cs` | 添加 `CompileSfcAsync` 方法 |
| `Frontend/Deno/Hosting/DenoFrontendHost.cs` | 实现 `CompileSfcAsync` |
| `Frontend/Deno/Worker/frontend-worker.ts` | 添加 `compile/sfc` 处理 |

---

## 三、接口与类型定义

### 3.1 DevServerOptions

```csharp
// DevServer/DevServerOptions.cs
namespace Jolt.DevServer;

public sealed class DevServerOptions
{
    public string RootDirectory { get; init; } = Directory.GetCurrentDirectory();
    public int Port { get; init; } = 5173;
    public string Host { get; init; } = "localhost";
    public bool OpenBrowser { get; init; } = false;
    public bool HmrEnabled { get; init; } = true;
}
```

### 3.2 DevHttpServer

```csharp
// DevServer/DevHttpServer.cs
namespace Jolt.DevServer;

public sealed class DevHttpServer : IAsyncDisposable
{
    public DevHttpServer(
        DevServerOptions options,
        OnDemandCompiler compiler,
        ModuleResolver moduleResolver,
        HtmlTransformer htmlTransformer);

    public Task StartAsync(CancellationToken cancellationToken);
    public ValueTask DisposeAsync();

    // 内部使用 WebApplication (Kestrel)
    // 不暴露 Kestrel 细节
}
```

### 3.3 OnDemandCompiler

```csharp
// DevServer/OnDemandCompiler.cs
namespace Jolt.DevServer;

/// <summary>
/// 按需编译服务：接收文件路径，返回编译后的 JS/CSS 内容。
/// 内部路由到 .NET 编译器或 Deno Worker。
/// </summary>
public sealed class OnDemandCompiler
{
    public OnDemandCompiler(
        JazorVueParser parser,
        JazorVueCompiler compiler,
        IDenoVolarHost? denoVolarHost,
        CompilationCache cache);

    /// <summary>
    /// 编译指定文件，返回编译结果。
    /// </summary>
    public async ValueTask<CompilationResult> CompileAsync(
        string absolutePath,
        CancellationToken cancellationToken);
}

/// <summary>
/// 编译结果：包含 JS 模块内容、可选 CSS、source map、依赖列表。
/// </summary>
public sealed class CompilationResult
{
    public required string ContentType { get; init; }     // "text/javascript" or "text/css"
    public required string Content { get; init; }          // 编译后的代码
    public string? SourceMap { get; init; }                // 可选 Source Map (Phase 2)
    public IReadOnlyList<string> Dependencies { get; init; } = [];  // 导入的模块路径
    public IReadOnlyList<string> Diagnostics { get; init; } = [];    // 编译诊断
    public bool IsError { get; init; }                     // 编译是否失败
    public string? ErrorMessage { get; init; }             // 错误信息
}
```

### 3.4 ModuleResolver

```csharp
// DevServer/ModuleResolver.cs
namespace Jolt.DevServer;

/// <summary>
/// ESM 模块路径解析器：将浏览器请求路径映射到磁盘文件路径。
/// </summary>
public sealed class ModuleResolver
{
    public ModuleResolver(string rootDirectory);

    /// <summary>
    /// 将请求路径解析为磁盘绝对路径。
    /// 处理: 相对路径、裸模块(bare specifier)、.jazor/.vue/.ts 扩展名。
    /// </summary>
    public ResolveResult Resolve(string requestPath, string? importerPath = null);
}

public sealed class ResolveResult
{
    public required string AbsolutePath { get; init; }
    public required string ResolvedUrl { get; init; }      // 返回给浏览器的 URL
    public required DocumentKind DocumentKind { get; init; }
    public required bool IsVirtual { get; init; }          // 虚拟模块 (如 .jazor 编译产物)
    public bool Found { get; init; } = true;
    public string? Error { get; init; }
}
```

### 3.5 CompilationCache

```csharp
// DevServer/CompilationCache.cs
namespace Jolt.DevServer;

/// <summary>
/// 编译结果内存缓存。键为文件绝对路径，值为 (内容哈希, 编译结果)。
/// 文件内容不变时直接返回缓存结果。
/// </summary>
public sealed class CompilationCache
{
    public bool TryGet(string absolutePath, string contentHash, [NotNullWhen(true)] out CompilationResult? result);
    public void Set(string absolutePath, string contentHash, CompilationResult result);
    public void Invalidate(string absolutePath);
    public void InvalidateAll();
}
```

### 3.6 HtmlTransformer

```csharp
// DevServer/HtmlTransformer.cs
namespace Jolt.DevServer;

/// <summary>
/// 转换 index.html：注入开发时 script、重写模块路径。
/// </summary>
public sealed class HtmlTransformer
{
    public HtmlTransformer(DevServerOptions options);

    /// <summary>
    /// 转换 HTML：注入 HMR client script、重写入口模块路径。
    /// </summary>
    public string Transform(string html, string htmlPath);
}
```

### 3.7 DependencyGraph

```csharp
// DevServer/DependencyGraph.cs
namespace Jolt.DevServer;

/// <summary>
/// 模块依赖图：记录哪些文件导入了哪些文件。
/// 用于 Phase 3 HMR 的变更传播计算。
/// Phase 1 仅记录，不用于热更新逻辑。
/// </summary>
public sealed class DependencyGraph
{
    /// <summary>
    /// 记录一个模块的依赖关系。
    /// </summary>
    public void Record(string modulePath, IReadOnlyList<string> dependencies);

    /// <summary>
    /// 获取直接依赖于指定模块的所有模块。
    /// </summary>
    public IReadOnlyList<string> GetDependents(string modulePath);
}
```

### 3.8 Deno 编译协议扩展

```csharp
// Frontend/Deno/Protocol/DenoCompilationProtocol.cs
namespace Jolt.Frontend.Deno.Protocol;

/// <summary>
/// Deno Worker SFC 编译请求。
/// </summary>
internal sealed class DenoSfcCompileRequest
{
    public required string DocumentPath { get; init; }
    public required string SfcText { get; init; }         // Vue SFC 完整文本
    public required string Filename { get; init; }        // 用于 source map 的文件名
}

/// <summary>
/// Deno Worker SFC 编译响应。
/// </summary>
internal sealed class DenoSfcCompileResult
{
    public required string JsContent { get; init; }       // 编译后的 JS 模块
    public string? CssContent { get; init; }              // 提取的 CSS (如有 <style> 块)
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
```

### 3.9 IDenoVolarHost 扩展

```csharp
// 在现有 IDenoVolarHost.cs 中添加:
ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
    string documentPath,
    string sfcText,
    string filename,
    CancellationToken cancellationToken);
```

---

## 四、核心实现细节

### 4.1 OnDemandCompiler.CompileAsync — 编译路由逻辑

这是 Dev Server 的核心。根据文件类型路由到不同的编译管道：

```csharp
public async ValueTask<CompilationResult> CompileAsync(
    string absolutePath, CancellationToken cancellationToken)
{
    var content = await File.ReadAllTextAsync(absolutePath, cancellationToken);
    var hash = ComputeContentHash(content);

    if (_cache.TryGet(absolutePath, hash, out var cached))
        return cached;

    var kind = JoltWorkspaceResolver.MapDocumentKind(absolutePath);
    CompilationResult result = kind switch
    {
        DocumentKind.Jazor => await CompileJazorAsync(absolutePath, content, cancellationToken),
        DocumentKind.Vue   => await CompileVueAsync(absolutePath, content, cancellationToken),
        DocumentKind.TypeScript => await CompileTypeScriptAsync(absolutePath, content, cancellationToken),
        DocumentKind.JavaScript => CreatePassthroughResult(content, "text/javascript"),
        _ => CreatePassthroughResult(content, GetContentType(absolutePath))
    };

    _cache.Set(absolutePath, hash, result);
    return result;
}
```

### 4.2 CompileJazorAsync — .jazor 编译 (两阶段)

```
阶段 1 (.NET 进程内):
  JazorVueParser.Parse(filePath, sourceText) → JazorVueDocument
  JazorVueCompiler.Compile(document)         → JazorVueCompilationResult
                                               → GeneratedVueText (Vue SFC)

阶段 2 (委托 Deno Worker):
  denoVolarHost.CompileSfcAsync(path, GeneratedVueText, filename)
                                              → DenoSfcCompileResult
                                               → JsContent + CssContent

合并结果:
  → CompilationResult { Content = JsContent, Dependencies = ... }
```

```csharp
private async ValueTask<CompilationResult> CompileJazorAsync(
    string path, string sourceText, CancellationToken ct)
{
    // Stage 1: Parse + Compile to Vue SFC
    var document = _parser.Parse(path, sourceText);
    var compilation = _compiler.Compile(document);

    if (_denoVolarHost is not { IsRunning: true })
    {
        // Deno 不可用时，返回原始 Vue SFC 作为降级
        // 浏览器需要 @vue/compiler-sfc 才能运行，这是预期的降级
        return new CompilationResult
        {
            ContentType = "text/javascript",
            Content = WrapAsVueSfcModule(compilation.GeneratedVueText, path),
            Dependencies = ExtractImportPaths(document),
            Diagnostics = compilation.Diagnostics,
            IsError = compilation.Diagnostics.Count > 0
        };
    }

    // Stage 2: Compile Vue SFC to JS ESM
    var sfcResult = await _denoVolarHost.CompileSfcAsync(
        path, compilation.GeneratedVueText, Path.GetFileName(path), ct);

    if (sfcResult is null)
    {
        return new CompilationResult
        {
            ContentType = "text/javascript",
            Content = WrapAsVueSfcModule(compilation.GeneratedVueText, path),
            Dependencies = ExtractImportPaths(document),
            IsError = true,
            ErrorMessage = "Vue SFC compilation returned no result"
        };
    }

    return new CompilationResult
    {
        ContentType = "text/javascript",
        Content = sfcResult.JsContent,
        Dependencies = ExtractImportPaths(document),
        Diagnostics = [..compilation.Diagnostics, ..sfcResult.Diagnostics]
    };
}
```

### 4.3 CompileVueAsync — .vue 编译 (直接 Deno)

```csharp
private async ValueTask<CompilationResult> CompileVueAsync(
    string path, string sourceText, CancellationToken ct)
{
    if (_denoVolarHost is not { IsRunning: true })
    {
        return ErrorResult("Deno worker is not available for .vue compilation");
    }

    var result = await _denoVolarHost.CompileSfcAsync(
        path, sourceText, Path.GetFileName(path), ct);

    return result is null
        ? ErrorResult("Vue SFC compilation returned no result")
        : new CompilationResult
        {
            ContentType = "text/javascript",
            Content = result.JsContent,
            Dependencies = ExtractJsDependencies(result.JsContent)
        };
}
```

### 4.4 CompileTypeScriptAsync — .ts 转译

```csharp
private async ValueTask<CompilationResult> CompileTypeScriptAsync(
    string path, string sourceText, CancellationToken ct)
{
    if (_denoVolarHost is not { IsRunning: true })
    {
        // Deno 不可用时，原样返回 .ts 内容（浏览器可能不支持）
        return CreatePassthroughResult(sourceText, "text/javascript");
    }

    // 添加 compile/ts IPC 方法，或复用 compile/sfc
    // 对于纯 .ts，Deno 内置 transpileOnly 即可
    var result = await _denoVolarHost.CompileTypeScriptAsync(path, sourceText, ct);
    // ...
}
```

### 4.5 DevHttpServer — Kestrel 集成

```csharp
public async Task StartAsync(CancellationToken cancellationToken)
{
    var builder = WebApplication.CreateSlimBuilder();
    builder.WebHost.UseUrls($"http://{_options.Host}:{_options.Port}");

    // 静态文件（用户 workspace 根目录）
    builder.WebHost.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(_options.RootDirectory),
        ServeUnknownFileTypes = true
    });

    var app = builder.Build();

    // === 中间件管道 ===

    // 1. 日志中间件（可选）
    // app.Use(async (ctx, next) => { ... await next(); });

    // 2. 核心模块服务中间件
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value;
        if (path is null) { await next(); return; }

        // 3a. HTML 入口
        if (path == "/" || path.Equals("/index.html", StringComparison.OrdinalIgnoreCase))
        {
            await ServeIndexHtmlAsync(context);
            return;
        }

        // 3b. 可编译模块 (.jazor, .vue, .ts)
        var resolveResult = _moduleResolver.Resolve(path);
        if (resolveResult.Found && resolveResult.IsVirtual)
        {
            await ServeCompiledModuleAsync(context, resolveResult, cancellationToken);
            return;
        }

        // 3c. HMR client script
        if (path.Equals("/@jazor/hmr", StringComparison.OrdinalIgnoreCase))
        {
            await ServeEmbeddedResourceAsync(context, "jazor-hmr.js");
            return;
        }

        // 3d. 静态文件 fallback
        await next();
    });

    // 3. 静态文件中间件
    app.UseStaticFiles();

    // 4. SPA fallback (未匹配路径 → index.html)
    app.Use(async (context, next) =>
    {
        if (!Path.HasExtension(context.Request.Path.Value ?? ""))
        {
            context.Request.Path = "/index.html";
        }
        await next();
    });

    await app.StartAsync(cancellationToken);
    Console.WriteLine($"  Dev Server: http://{_options.Host}:{_options.Port}");
}
```

### 4.6 ServeCompiledModuleAsync — 模块服务

```csharp
private async Task ServeCompiledModuleAsync(
    HttpContext context, ResolveResult resolve, CancellationToken ct)
{
    var result = await _compiler.CompileAsync(resolve.AbsolutePath, ct);

    if (result.IsError)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/javascript";
        await context.Response.WriteAsync(
            $"console.error('[jazor-dev] Compilation error: {EscapeJs(result.ErrorMessage)}');",
            ct);
        return;
    }

    context.Response.ContentType = result.ContentType;
    context.Response.Headers["Cache-Control"] = "no-cache, no-store";
    await context.Response.WriteAsync(result.Content, ct);
}
```

### 4.7 ModuleResolver.Resolve — 路径解析逻辑

```csharp
public ResolveResult Resolve(string requestPath, string? importerPath = null)
{
    // 去掉前导 /
    var relativePath = requestPath.TrimStart('/');

    // 处理 .jazor → 返回虚拟 JS 模块
    if (relativePath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
    {
        var absolutePath = Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));
        return new ResolveResult
        {
            AbsolutePath = absolutePath,
            ResolvedUrl = requestPath,
            DocumentKind = DocumentKind.Jazor,
            IsVirtual = true,
            Found = File.Exists(absolutePath)
        };
    }

    // 处理 .vue → 返回编译后 JS
    if (relativePath.EndsWith(".vue", StringComparison.OrdinalIgnoreCase))
    {
        var absolutePath = Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));
        return new ResolveResult
        {
            AbsolutePath = absolutePath,
            ResolvedUrl = requestPath,
            DocumentKind = DocumentKind.Vue,
            IsVirtual = true,
            Found = File.Exists(absolutePath)
        };
    }

    // 处理 .ts → 返回转译后 JS
    if (relativePath.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
    {
        var absolutePath = Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));
        return new ResolveResult
        {
            AbsolutePath = absolutePath,
            ResolvedUrl = requestPath,
            DocumentKind = DocumentKind.TypeScript,
            IsVirtual = true,
            Found = File.Exists(absolutePath)
        };
    }

    // .js / .css / 其他静态文件 → 原样返回
    var staticPath = Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));
    return new ResolveResult
    {
        AbsolutePath = staticPath,
        ResolvedUrl = requestPath,
        DocumentKind = JoltWorkspaceResolver.MapDocumentKind(staticPath),
        IsVirtual = false,
        Found = File.Exists(staticPath)
    };
}
```

### 4.8 HtmlTransformer.Transform — HTML 转换

```csharp
public string Transform(string html, string htmlPath)
{
    var sb = new StringBuilder(html);

    // 在 </head> 前注入 HMR client
    if (_options.HmrEnabled)
    {
        var hmrScript = """
            <script type="module" src="/@jazor/hmr"></script>
            """;
        var headClose = sb.ToString().IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (headClose >= 0)
        {
            sb.Insert(headClose, hmrScript);
        }
        else
        {
            sb.Insert(0, hmrScript);
        }
    }

    // 不改写 script 路径 — 浏览器自然请求 .jazor 路径，
    // Dev Server 的中间件负责编译并返回 JS
    return sb.ToString();
}
```

---

## 五、Deno Worker 扩展

### 5.1 frontend-worker.ts — 新增 compile/sfc 方法

```typescript
// 在现有方法分发 switch 中添加:
case "compile/sfc": {
    const req = payload as {
        documentPath: string;
        sfcText: string;
        filename: string;
    };
    const result = compileVueSfc(req.sfcText, req.filename);
    return result;
}

// 新增编译函数:
function compileVueSfc(sfcText: string, filename: string): {
    jsContent: string;
    cssContent: string | null;
    diagnostics: string[];
} {
    const { parse, compileScript, compileTemplate, compileStyle } =
        vueCompilerSfc; // npm:@vue/compiler-sfc

    const descriptor = parse(sfcText, { filename });

    const diagnostics: string[] = [];
    let jsContent = "";
    let cssContent: string | null = null;

    // 编译 <script setup>
    if (descriptor.scriptSetup) {
        const compiled = compileScript(descriptor, {
            id: filename,
            isProd: false,
            genDefaultAs: "_sfc_main",
        });
        if (compiled.errors.length > 0) {
            for (const err of compiled.errors) {
                diagnostics.push(String(err));
            }
        } else {
            jsContent += compiled.content + "\n";
        }
    } else if (descriptor.script) {
        jsContent += descriptor.script.content + "\n\n";
        jsContent += `const _sfc_main = ${descriptor.script.setup ? "/* setup */" : ""}{};\n`;
    }

    // 编译 <template>
    if (descriptor.template) {
        const templateCompiled = compileTemplate({
            source: descriptor.template.content,
            filename,
            id: filename,
            isProd: false,
            compilerOptions: {
                // 使用 Vue 3.x 运行时
            },
        });
        if (templateCompiled.errors.length > 0) {
            for (const err of templateCompiled.errors) {
                diagnostics.push(String(err));
            }
        } else {
            jsContent += `import { ${[...new Set([
                ...extractRuntimeHelpers(templateCompiled.code)
            ])].join(", ")} } from "vue";\n\n`;
            jsContent += templateCompiled.code + "\n";
        }
    }

    // 编译 <style>
    if (descriptor.styles.length > 0) {
        const cssParts: string[] = [];
        for (const styleBlock of descriptor.styles) {
            const styleCompiled = compileStyle({
                source: styleBlock.content,
                filename,
                id: filename,
                isProd: false,
            });
            cssParts.push(styleCompiled.code);
        }
        cssContent = cssParts.join("\n");
    }

    // 导出组件
    jsContent += "\n_sfc_main.render = render;\n";
    jsContent += "export default _sfc_main;\n";

    return { jsContent, cssContent, diagnostics };
}
```

**注意**: 实际实现需要在 `deno.json` 的 imports 中添加 `@vue/compiler-sfc`。当前 `deno.json` 可能已有部分 Vue 依赖。

### 5.2 IDenoVolarHost 扩展

```csharp
// Frontend/Deno/Hosting/IDenoVolarHost.cs — 新增方法
ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
    string documentPath,
    string sfcText,
    string filename,
    CancellationToken cancellationToken);
```

### 5.3 DenoVolarHost 实现

```csharp
// Frontend/Deno/Hosting/DenoFrontendHost.cs — 新增方法
public async ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
    string documentPath, string sfcText, string filename, CancellationToken ct)
{
    if (_workerProcess is not { IsRunning: true })
        return null;

    var request = new DenoSfcCompileRequest
    {
        DocumentPath = documentPath,
        SfcText = sfcText,
        Filename = filename
    };

    return await _workerProcess.SendRequestAsync<DenoSfcCompileResult>(
        "compile/sfc", request, ct);
}
```

### 5.4 Deno Worker TypeScript 编译

同样需要在 `frontend-worker.ts` 中添加 `compile/ts` 方法：

```typescript
case "compile/ts": {
    const req = payload as {
        documentPath: string;
        text: string;
    };
    // 使用 Deno 内置 TypeScript 转译
    const result = Deno.transpileOnly
        ? ts.transpileModule(req.text, {
            compilerOptions: {
                module: ts.ModuleKind.ESNext,
                target: ts.ScriptTarget.ESNext,
                jsx: ts.JsxEmit.Preserve,
                sourceMap: false,
            },
            reportDiagnostics: true,
        })
        : null;
    return {
        jsContent: result?.outputText ?? req.text,
        diagnostics: result?.diagnostics?.map(d => String(d.messageText)) ?? [],
    };
}
```

---

## 六、Program.cs 修改

### 6.1 新增 --dev 入口

在 `Program.cs` 顶层语句中添加：

```csharp
var useDev = args.Any(static arg => string.Equals(arg, "--dev", StringComparison.OrdinalIgnoreCase));

// ... 现有服务创建代码 (workspaceStore, denoVolarHost 等) ...

if (useDev)
{
    var devOptions = ParseDevOptions(args);  // --port, --host, --root
    var parser = new JazorVueParser();
    var compiler = new JazorVueCompiler();
    var compilationCache = new CompilationCache();
    var onDemandCompiler = new OnDemandCompiler(parser, compiler, denoVolarHost, compilationCache);
    var moduleResolver = new ModuleResolver(devOptions.RootDirectory);
    var htmlTransformer = new HtmlTransformer(devOptions);
    var dependencyGraph = new DependencyGraph();

    var devServer = new DevHttpServer(
        devOptions,
        onDemandCompiler,
        moduleResolver,
        htmlTransformer);

    Console.WriteLine($"  Jazor Dev Server");
    Console.WriteLine($"  Root: {devOptions.RootDirectory}");
    Console.WriteLine($"  Ready: http://{devOptions.Host}:{devOptions.Port}");

    await devServer.StartAsync(cancellationToken);
    // 阻塞直到 Ctrl+C
    await Task.Delay(Timeout.Infinite, cancellationToken);
    return;
}
```

### 6.2 Dev 选项解析

```csharp
static DevServerOptions ParseDevOptions(string[] args)
{
    int port = 5173;
    string host = "localhost";
    string root = Directory.GetCurrentDirectory();

    foreach (var arg in args)
    {
        if (arg.StartsWith("--port=", StringComparison.OrdinalIgnoreCase))
            int.TryParse(arg[7..], out port);
        else if (arg.StartsWith("--host=", StringComparison.OrdinalIgnoreCase))
            host = arg[7..];
        else if (arg.StartsWith("--root=", StringComparison.OrdinalIgnoreCase))
            root = Path.GetFullPath(arg[7..]);
    }

    return new DevServerOptions
    {
        RootDirectory = root,
        Port = port,
        Host = host
    };
}
```

---

## 七、客户端 HMR Runtime (基础版)

### 7.1 jazor-hmr.js — Phase 1 最小实现

Phase 1 的 HMR runtime 只做两件事：
1. 维护 EventSource 连接（为 Phase 3 完整 WebSocket 做准备）
2. 提供 `import.meta.hot` 基础桩

```javascript
// jazor-hmr.js — Phase 1 基础版本
(function () {
    "use strict";

    // import.meta.hot 桩 — Phase 3 会实现完整 HMR
    if (!window.__JAZOR_HMR__) {
        window.__JAZOR_HMR__ = {
            register: function (id, deps, accept) {
                // Phase 1: 记录但不执行热更新
                console.log(`[jazor-hmr] registered: ${id}`);
            },
            accept: function (cb) {
                // Phase 1: 不执行热更新，需手动刷新
            }
        };
    }

    // 暴露到 import.meta
    if (typeof importMeta !== "undefined") {
        importMeta.hot = window.__JAZOR_HMR__;
    }

    console.log("[jazor-dev] Dev server connected.");
})();
```

---

## 八、用户工作流

### 8.1 最小可运行示例

用户 workspace 结构：

```
my-app/
├── index.html
├── App.jazor
├── Counter.jazor
└── main.js
```

**index.html**:
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Jazor App</title>
</head>
<body>
    <div id="app"></div>
    <script type="module" src="/main.js"></script>
</body>
</html>
```

**main.js**:
```javascript
import { createApp } from "vue";
import App from "./App.jazor";

createApp(App).mount("#app");
```

**App.jazor**:
```
@code {
    [Prop] public string Title { get; set; }

    public string Greet(string name)
    {
        return $"Hello, {name}!";
    }
}

<div>
    <h1>@Title</h1>
    <Counter count="5" />
</div>
```

**Counter.jazor**:
```
@code {
    [Prop] public int Count { get; set; }
    [State] private int current = 0;

    public void Increment()
    {
        current++;
    }
}

<div>
    <p>Count: @current (from @Count)</p>
    <button @@click="Increment">+1</button>
</div>
```

### 8.2 浏览器请求流程

```
1. GET /                     → index.html (注入 HMR script)
2. GET /main.js              → 静态文件，原样返回
3. GET /App.jazor            → 编译:
   3a. JazorVueParser.Parse  → JazorVueDocument
   3b. JazorVueCompiler.Compile → Vue SFC 文本
   3c. Deno compileSfc       → JS ESM
4. GET /Counter.jazor        → 同上编译流程
5. GET /vue (bare specifier) → node_modules 或 Deno CDN
```

### 8.3 Vue 运行时解析

`.jazor` 编译后的 JS 会包含 `import { ref, computed, ... } from "vue"` 等裸模块引用。Phase 1 需要处理 Vue 运行时的解析：

**方案 A (推荐 MVP)**: 用户 workspace 安装 Vue 到 `node_modules`，ModuleResolver 映射裸模块到 `node_modules/vue/dist/vue.runtime.esm-bundler.js`。

**方案 B**: 在 HTML 中通过 CDN `<script>` 全局引入 Vue，编译产物改为全局引用。

```csharp
// ModuleResolver — 裸模块解析
public ResolveResult Resolve(string requestPath, string? importerPath = null)
{
    // ... 已有逻辑 ...

    // 裸模块 (如 "vue", "vue/jsx-runtime")
    if (!requestPath.StartsWith('.') && !requestPath.StartsWith('/'))
    {
        var nodeModulesPath = Path.Combine(_rootDirectory, "node_modules", requestPath);
        // 尝试精确路径
        if (File.Exists(nodeModulesPath))
            return ResolveTo(nodeModulesPath, DocumentKind.JavaScript);
        // 尝试 package.json → module/main 字段
        var packageJsonPath = Path.Combine(
            Path.GetDirectoryName(nodeModulesPath)!, "package.json");
        if (File.Exists(packageJsonPath))
        {
            var packageJson = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
            var entry = packageJson.RootElement
                .TryGetProperty("module", out var mod) ? mod.GetString()
                : packageJson.RootElement.TryGetProperty("main", out var main) ? main.GetString()
                : "index.js";
            var entryPath = Path.Combine(
                Path.GetDirectoryName(nodeModulesPath)!, entry!);
            if (File.Exists(entryPath))
                return ResolveTo(entryPath, DocumentKind.JavaScript);
        }
        return new ResolveResult { Found = false, Error = $"Cannot resolve bare module: {requestPath}" };
    }
}
```

---

## 九、jolt.config.json (可选配置)

Phase 1 支持最小配置文件。如果不存在，使用默认值。

```jsonc
// jolt.config.json
{
    "server": {
        "port": 5173,
        "host": "localhost",
        "open": false
    },
    "resolve": {
        // 裸模块别名
        "alias": {
            "@": "./src"
        }
    }
}
```

```csharp
// DevServer/JazorConfig.cs
public sealed class JazorConfig
{
    public ServerConfig? Server { get; init; }
    public ResolveConfig? Resolve { get; init; }
}

public sealed class ServerConfig
{
    public int Port { get; init; } = 5173;
    public string Host { get; init; } = "localhost";
    public bool Open { get; init; }
}

public sealed class ResolveConfig
{
    public Dictionary<string, string>? Alias { get; init; }
}

public static class JazorConfigLoader
{
    public static JazorConfig Load(string rootDirectory)
    {
        var configPath = Path.Combine(rootDirectory, "jolt.config.json");
        if (!File.Exists(configPath))
            return new JazorConfig();

        var json = File.ReadAllText(configPath);
        return ProtocolJsonSerializer.Deserialize<JazorConfig>(json)
            ?? new JazorConfig();
    }
}
```

---

## 十、实施步骤（严格顺序）

### Step 1: Deno Worker 编译能力 (前置)

**产出文件**:
- 修改 `Frontend/Deno/Worker/frontend-worker.ts` — 添加 `compile/sfc` 和 `compile/ts`
- 修改 `Frontend/Deno/Worker/deno.json` — 添加 `@vue/compiler-sfc` 依赖
- 新增 `Frontend/Deno/Protocol/DenoCompilationProtocol.cs`
- 修改 `Frontend/Deno/Hosting/IDenoVolarHost.cs` — 添加 `CompileSfcAsync` + `CompileTypeScriptAsync`
- 修改 `Frontend/Deno/Hosting/DenoFrontendHost.cs` — 实现

**测试**:
- 手动验证: 通过 `StdioJoltRpcServer` 发送 `compile/sfc` 请求，验证返回 JS
- 新增 `JoltCompilationTests.cs`: 测试 `JazorVueCompiler` 输出 → Deno 编译 → JS 结果

**退出标准**: 通过 IPC 调用 Deno Worker 编译一个简单的 Vue SFC，获得有效的 JS ESM 输出。

### Step 2: OnDemandCompiler + CompilationCache

**产出文件**:
- 新增 `DevServer/OnDemandCompiler.cs`
- 新增 `DevServer/CompilationCache.cs`

**不依赖 HTTP 服务器**，可独立测试。

**测试**:
- 单元测试: 给定 `.jazor` 文件路径，验证 `CompileAsync` 返回非空 JS 内容
- 单元测试: 给定 `.vue` 文件路径，验证编译路由到 Deno Worker
- 单元测试: 验证缓存命中 (同样输入第二次调用不触发编译)

**退出标准**: `OnDemandCompiler` 可以将 `.jazor` / `.vue` / `.ts` 编译为 JS ESM。

### Step 3: ModuleResolver

**产出文件**:
- 新增 `DevServer/ModuleResolver.cs`

**测试**:
- 单元测试: `/App.jazor` → 磁盘路径映射
- 单元测试: 裸模块 `vue` → `node_modules/vue/...` 解析
- 单元测试: 相对路径 `./Counter.jazor` → 正确解析
- 单元测试: 不存在路径 → `Found = false`

**退出标准**: 所有路径解析规则正确映射。

### Step 4: HtmlTransformer + jazor-hmr.js

**产出文件**:
- 新增 `DevServer/HtmlTransformer.cs`
- 新增 `DevServer/Client/jazor-hmr.js`
- 修改 `Jolt.csproj` — 嵌入 `jazor-hmr.js`

**测试**:
- 单元测试: HTML 转换注入 HMR script
- 单元测试: 无 `</head>` 标签时插入到开头

**退出标准**: HTML 转换正确注入客户端脚本。

### Step 5: DevHttpServer 集成

**产出文件**:
- 新增 `DevServer/DevHttpServer.cs`
- 新增 `DevServer/DevServerOptions.cs`
- 新增 `DevServer/DevServerMiddleware.cs` (或内联在 DevHttpServer)
- 新增 `DevServer/DependencyGraph.cs`
- 修改 `Program.cs` — 添加 `--dev` 模式

**测试**:
- 集成测试: 启动 Dev Server → HTTP GET `/` → 返回转换后的 index.html
- 集成测试: HTTP GET `/App.jazor` → 返回编译后的 JS
- 集成测试: HTTP GET 不存在的文件 → 404
- 端到端测试: 完整 workspace → 浏览器加载 → 页面渲染成功

**退出标准**: 浏览器打开 Dev Server 地址，可以看到 `.jazor` 组件渲染的页面。

### Step 6: jolt.config.json 支持 (可选，可后移)

**产出文件**:
- 新增 `DevServer/JazorConfig.cs`
- 修改 `Program.cs` — 读取配置

---

## 十一、风险与降级

| 风险 | 影响 | 降级方案 |
|------|------|---------|
| `@vue/compiler-sfc` 在 Deno Worker 中不可用 | 无法编译 Vue SFC → JS | 直接返回 SFC 文本，在浏览器端使用运行时编译 (vue.runtime.esm-browser.js) |
| Deno Worker 启动失败 | 所有前端编译不可用 | `.jazor` 返回原始 SFC 文本 + 警告；`.js/.css` 静态文件仍可用 |
| 裸模块解析失败 | Vue 运行时加载失败 | 在 `index.html` 中通过 `<script>` 全局加载 Vue |
| Kestrel 端口被占用 | Dev Server 无法启动 | 尝试自动递增端口 (5173→5174→...) |
| JazorVueCompiler 编译错误 | 单个 .jazor 编译失败 | 返回 JS 错误注入代码，浏览器 console 显示错误，其他文件继续工作 |

---

## 十二、关键依赖关系

```
Step 1 (Deno Worker 编译)
    ↓
Step 2 (OnDemandCompiler) ←── Step 3 (ModuleResolver)
    ↓                               ↓
    └───────┬───────────────────────┘
            ↓
Step 4 (HtmlTransformer)
            ↓
Step 5 (DevHttpServer 集成)
            ↓
Step 6 (配置支持, 可选)
```

Step 2 和 Step 3 可以并行开发。Step 5 是最终集成，依赖前面所有步骤。

---

## 十三、与现有系统的关系

### 13.1 与 LSP 的关系

- Dev Server 和 LSP **共享** `JoltService`、`JazorVueCompiler`、`DenoVolarHost`
- Dev Server **不共享** LSP 的 `LspSession`、Lane 体系 — 这是两个独立的运行模式
- 两者共享 `IJoltWorkspaceStore`，但 Dev Server 使用文件系统而非 LSP `didChange`

### 13.2 与 RPC Server 的关系

- Dev Server 是新的运行模式，与 `--stdio` RPC 模式互斥
- 可以考虑未来通过 RPC 暴露编译能力（复用 `GetVirtualArtifact`），但 Phase 1 不做

### 13.3 与 Deno Worker 的关系

- Phase 1 新增 `compile/sfc` 和 `compile/ts` 两个 IPC 方法
- 现有 8 个 LSP 方法不受影响
- Deno Worker 仍然是单请求串行处理（SemaphoreSlim gate）

---

## 十四、不做的事情 (Phase 1 明确排除)

| 排除项 | 原因 |
|--------|------|
| HMR 热更新推送 | Phase 3 |
| Source Map | Phase 2 |
| 文件监听 (FileSystemWatcher) | Phase 3 |
| API 代理 | Phase 3 |
| 生产构建 | Phase 5 |
| DAP 调试 | Phase 4 |
| node_modules 深度解析 | Phase 1 只支持 main/module 字段 |
| .css 预处理 (Sass/Less) | 远期 |
| 多页面应用支持 | 远期 |
| HTTPS | 远期 |
