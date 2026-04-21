# Deno Volar 集成宿主

> 状态：已实现
> 定位：Deno Volar 前端服务的顶层管理器，负责进程生命周期、智能感知请求路由和容错重试

## 1. 文档定位

本文档描述 Jolt 中 Deno Volar 集成宿主的实现，包括 `IDenoVolarHost` 接口、`DenoVolarHost` 实现类和相关配置选项。该系统为 Vue 模板提供完整的 LSP 智能感知服务（诊断、补全、定义、引用、重命名等），并封装了 Deno 子进程的生命周期管理和故障恢复机制。

**相关源文件**：
- `src/Jolt/Frontend/Deno/Hosting/IDenoFrontendHost.cs` - 主接口定义
- `src/Jolt/Frontend/Deno/Hosting/DenoVolarHost.cs` - 主实现类
- `src/Jolt/Frontend/Deno/Hosting/DenoVolarHostOptions.cs` - 配置选项
- `src/Jolt/Frontend/Deno/Hosting/DenoWorkerProcess.cs` - 子进程管理（独立文档）
- `src/Jolt/Frontend/Deno/Protocol/DenoFrontendProtocol.cs` - 请求/响应协议（独立文档）

## 2. 核心类型

### 2.1 `IDenoVolarHost` 接口

完整的 Deno Volar 集成接口，继承自 `IAsyncDisposable`，提供编译和智能感知两大类功能。

**状态属性**：
- `bool IsEnabled { get; }` - Volar 服务是否启用（由配置决定）
- `bool IsRunning { get; }` - Deno worker 进程是否正在运行

**生命周期方法**：
- `ValueTask StartAsync(CancellationToken cancellationToken)` - 启动 Deno worker 进程
- `ValueTask StopAsync(CancellationToken cancellationToken)` - 停止 Deno worker 进程
- `ValueTask DisposeAsync()` - 释放资源（调用 StopAsync）

**编译方法**：
- `ValueTask<DenoSfcCompileResult?> CompileSfcAsync(...)` - 编译 Vue SFC 文件
- `ValueTask<DenoTypeScriptCompileResult?> CompileTypeScriptAsync(...)` - 编译 TypeScript 文件
- `ValueTask<DenoCssModuleCompileResult?> CompileCssModuleAsync(...)` - 编译 CSS 模块

**智能感知方法**（所有方法接收 `DenoVolarIntelliSenseContext? context` 参数）：
- `GetTemplateDiagnosticsAsync` - 获取模板诊断信息
- `GetTemplateCompletionItemsAsync` - 获取补全项
- `GetTemplateDocumentSymbolsAsync` - 获取文档符号
- `GetTemplateSemanticTokensAsync` - 获取语义标记
- `GetTemplateDocumentLinksAsync` - 获取文档链接（默认返回空数组）
- `GetTemplateInlayHintsAsync` - 获取内联提示（默认返回空数组）
- `GetTemplateFoldingRangesAsync` - 获取折叠范围（默认返回空数组）
- `GetTemplateHoverAsync` - 获取悬停信息
- `GetTemplateDefinitionAsync` - 获取定义位置
- `GetTemplateImplementationAsync` - 获取实现位置（默认返回空数组）
- `GetTemplateReferencesAsync` - 获取引用位置
- `GetTemplateRenameAsync` - 获取重命名编辑

### 2.2 `DenoVolarHost` 实现

核心实现类，通过 `IDenoWorkerProcess` 管理 Deno 子进程，并提供自动重试和故障恢复机制。

**构造函数**：
- `public DenoVolarHost(DenoVolarHostOptions options)` - 生产构造函数，创建真实的 DenoWorkerProcess
- `internal DenoVolarHost(DenoVolarHostOptions options, IDenoWorkerProcess? workerProcess)` - 测试构造函数，支持注入 mock worker

**核心字段**：
```csharp
private readonly DenoVolarHostOptions _options;
private readonly IDenoWorkerProcess _workerProcess;
private readonly SemaphoreSlim _lifecycleGate = new(1, 1);  // 生命周期门控
private const int MaxSendAttempts = 3;  // 最大重试次数
private static readonly TimeSpan RetryBackoffBase = TimeSpan.FromMilliseconds(100);  // 指数退避基数
```

### 2.3 `DenoVolarIntelliSenseContext`

智能感知上下文记录，包含前端编译的语义信息：

```csharp
internal sealed record DenoVolarIntelliSenseContext(
    SemanticContext SemanticContext,        // 语义上下文（从 C# 编译获取）
    IReadOnlyList<ArtifactRecord> Artifacts); // 编译产物记录
```

### 2.4 `DenoVolarHostOptions`

配置选项类，控制 Deno Volar 行为：

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Enabled` | `bool` | - | 是否启用 Deno Volar 服务 |
| `ExecutablePath` | `string` | `""` | Deno 可执行文件路径 |
| `HasExplicitExecutableOverride` | `bool` | - | 是否显式覆盖可执行文件路径 |
| `WorkerScriptPath` | `string` | `""` | Worker 脚本路径（frontend-worker.ts） |
| `CacheDirectory` | `string` | `""` | Deno 缓存目录（DENO_DIR） |
| `Arguments` | `string[]` | `[]` | 传递给 Deno 的命令行参数 |
| `WorkingDirectory` | `string?` | `null` | Deno 工作目录 |
| `IgnoreStartupFailure` | `bool` | `true` | 是否忽略启动失败（默认 true） |

## 3. 核心算法

### 3.1 启动流程（StartAsync）

```
1. 检查 _options.Enabled，如果未启用则直接返回
2. 等待 _lifecycleGate（确保线程安全）
3. 检查 IsRunning，如果已运行则直接返回
4. 调用 _workerProcess.StartAsync(cancellationToken)
5. 捕获异常：
   - 如果 _options.IgnoreStartupFailure 为 true，调用 ResetWorkerStateCoreAsync() 吞掉异常
   - 否则重新抛出异常
6. 释放 _lifecycleGate
```

**设计意图**：`IgnoreStartupFailure` 允许 Jolt 在 Deno 运行时缺失时优雅降级（禁用 Volar 功能），而不是导致整个服务启动失败。

### 3.2 停止流程（StopAsync）

```
1. 检查 _options.Enabled，如果未启用则直接返回
2. 等待 _lifecycleGate
3. 调用 _workerProcess.StopAsync(cancellationToken)
4. 释放 _lifecycleGate
```

### 3.3 请求发送流程（SendAsync）

所有智能感知请求的统一发送方法，支持自动重试和指数退避：

```csharp
private async ValueTask<TResult?> SendAsync<TResult>(
    string method,
    object payload,
    CancellationToken cancellationToken)
{
    for (var attempt = 1; attempt <= MaxSendAttempts; attempt++)
    {
        // 1. 确保 worker 已启动
        await EnsureStartedAsync(cancellationToken);
        if (!IsRunning) return default;

        try
        {
            // 2. 发送请求
            return await _workerProcess.SendRequestAsync<TResult>(method, payload, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // 用户取消，直接重新抛出
        }
        catch (Exception ex) when (IsRecoverableWorkerFailure(ex))
        {
            // 3. 可恢复故障：重置 worker 状态并重试
            await TryResetWorkerStateAsync();
            WriteSendRetryWarning(method, attempt, ex);

            if (attempt == MaxSendAttempts)
                throw; // 最后一次尝试失败，抛出异常

            // 4. 指数退避：100ms, 200ms, 400ms
            var delay = TimeSpan.FromMilliseconds(
                RetryBackoffBase.TotalMilliseconds * Math.Pow(2, attempt - 1));
            await Task.Delay(delay, cancellationToken);
        }
    }
    return default;
}
```

**可恢复故障类型**（`IsRecoverableWorkerFailure`）：
- `ObjectDisposedException` - worker 进程已意外终止
- `IOException` - IO 通道故障
- `InvalidOperationException` - worker 状态无效
- `JsonException` - JSON 反序列化失败
- `NotSupportedException` - 不支持的操作

**重试策略**：
- 最多重试 3 次（`MaxSendAttempts`）
- 指数退避：100ms → 200ms → 400ms
- 每次重试前调用 `TryResetWorkerStateAsync()` 停止旧 worker
- 下一次请求时会重新启动 worker（`EnsureStartedAsync`）

### 3.4 Worker 重置流程（TryResetWorkerStateAsync）

```
1. 等待 _lifecycleGate（防止并发重置）
2. 调用 ResetWorkerStateCoreAsync()
3. 释放 _lifecycleGate
```

**ResetWorkerStateCoreAsync 实现**：
- 调用 `_workerProcess.StopAsync(CancellationToken.None)`
- 吞掉所有异常（`ObjectDisposedException`, `IOException`, `Win32Exception`, `InvalidOperationException`, `PlatformNotSupportedException`, `NotSupportedException`）
- 设计意图：确保下一次请求可以干净地重启 worker，不受旧 worker 失败的影响

### 3.5 智能感知请求封装

所有智能感知方法遵循统一模式：

```csharp
public async ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
    DocumentSnapshot document,
    LspPosition position,
    DenoVolarIntelliSenseContext? context,
    CancellationToken cancellationToken)
{
    var request = new DenoTemplateRequest
    {
        DocumentPath = document.DocumentPath,
        Text = document.Text,
        Position = position,
        FrontendContext = context?.SemanticContext,
        FrontendArtifacts = context?.Artifacts
    };
    var items = await SendAsync<LspCompletionItem[]>("template/completion", request, cancellationToken);
    return items ?? Array.Empty<LspCompletionItem>();
}
```

**请求类型映射**：

| 方法名 | 请求类型 | RPC Method |
|--------|---------|------------|
| `GetTemplateDiagnosticsAsync` | `DenoTemplateDiagnosticRequest` | `template/diagnostics` |
| `GetTemplateCompletionItemsAsync` | `DenoTemplateRequest` | `template/completion` |
| `GetTemplateDocumentSymbolsAsync` | `DenoTemplateDocumentRequest` | `template/documentSymbols` |
| `GetTemplateSemanticTokensAsync` | `DenoTemplateSemanticTokensRequest` | `template/semanticTokens` |
| `GetTemplateDocumentLinksAsync` | `DenoTemplateDocumentRequest` | `template/documentLinks` |
| `GetTemplateInlayHintsAsync` | `DenoTemplateRangeRequest` | `template/inlayHints` |
| `GetTemplateFoldingRangesAsync` | `DenoTemplateDocumentRequest` | `template/foldingRanges` |
| `GetTemplateHoverAsync` | `DenoTemplateRequest` | `template/hover` |
| `GetTemplateDefinitionAsync` | `DenoTemplateRequest` | `template/definition` |
| `GetTemplateImplementationAsync` | `DenoTemplateRequest` | `template/implementation` |
| `GetTemplateReferencesAsync` | `DenoTemplateReferenceRequest` | `template/references` |
| `GetTemplateRenameAsync` | `DenoTemplateRenameRequest` | `template/rename` |

## 4. 线程安全模型

### 4.1 生命周期门控（`_lifecycleGate`）

**类型**：`SemaphoreSlim(1, 1)` - 互斥锁

**保护的操作**：
- `StartAsync` - 防止并发启动
- `StopAsync` - 防止并发停止
- `TryResetWorkerStateAsync` - 防止并发重置

**设计意图**：
- 确保 Deno worker 进程的生命周期变更操作串行化
- 防止竞态条件（如同时启动和停止）
- 配合 `IgnoreStartupFailure` 实现优雅降级

### 4.2 Worker 进程内部锁

`DenoWorkerProcess` 内部维护两个独立的锁：

| 锁名称 | 类型 | 保护的操作 |
|--------|------|-----------|
| `_lifecycleGate` | `SemaphoreSlim(1, 1)` | StartAsync, StopAsync |
| `_requestGate` | `SemaphoreSlim(1, 1)` | SendRequestAsync |
| `_standardErrorGate` | `Lock` | stderr 缓冲区访问 |

**设计意图**：
- 生命周期锁和请求锁分离：启动/停止操作不会阻塞请求发送（反之亦然）
- stderr 锁独立：避免 stderr pump 阻塞请求处理

### 4.3 无锁读取

**只读属性**（无需锁保护）：
- `IsEnabled` - 配置项，初始化后不变
- `IsRunning` - 代理到 `_workerProcess.IsRunning`（底层使用 `Process.HasExited`，线程安全）

## 5. 错误处理

### 5.1 启动失败处理（IgnoreStartupFailure）

**场景**：Deno 运行时缺失、权限不足、路径错误等

**处理策略**：
- 如果 `IgnoreStartupFailure = true`（默认）：吞掉异常，将 worker 状态设为未运行
- 如果 `IgnoreStartupFailure = false`：重新抛出异常

**捕获的异常类型**：
- `Win32Exception` - Deno 可执行文件无法启动
- `UnauthorizedAccessException` - 权限不足
- `IOException` - IO 错误
- `InvalidOperationException` - 无效操作
- `ArgumentException` - 参数错误
- `NotSupportedException` - 不支持的操作

**设计意图**：
- 允许 Jolt 在 Deno 不可用时降级运行（禁用 Volar 功能）
- 避免因 Deno 问题导致整个 Jolt 服务崩溃
- 用户通过 `IsEnabled` 和 `IsRunning` 检测 Volar 可用性

### 5.2 请求失败处理（自动重试）

**场景**：Worker 进程崩溃、IO 通道故障、JSON 反序列化失败

**处理策略**（见 3.3 节）：
- 最多重试 3 次
- 每次重试前重置 worker 状态
- 指数退避避免风暴重试
- 记录警告日志到 stderr（JSON 格式）

**警告日志格式**：
```json
{
  "eventType": "denoVolarWorkerRetry",
  "method": "template/completion",
  "attempt": 1,
  "errorType": "System.ObjectDisposedException",
  "message": "Cannot access a disposed object.",
  "timestamp": "2026-04-21T10:30:00.000Z"
}
```

### 5.3 Worker 停止失败处理

**场景**：StopAsync 调用时 worker 已经异常终止

**处理策略**：
- 吞掉所有异常（`ObjectDisposedException`, `IOException`, `Win32Exception`, `InvalidOperationException`, `PlatformNotSupportedException`, `NotSupportedException`）
- 确保下一次请求可以干净地重启 worker

**设计意图**：
- 停止失败不应该阻止后续操作
- 允许系统从任何状态恢复

## 6. 配置选项

### 6.1 必需配置

| 配置项 | 说明 | 示例 |
|--------|------|------|
| `Enabled` | 是否启用 Deno Volar 服务 | `true` |
| `ExecutablePath` | Deno 可执行文件路径 | `runtimes/win-x64/native/deno.exe` |
| `WorkerScriptPath` | Worker 脚本路径 | `Frontend/Deno/Worker/frontend-worker.ts` |

### 6.2 可选配置

| 配置项 | 说明 | 默认值 | 推荐值 |
|--------|------|--------|--------|
| `CacheDirectory` | Deno 缓存目录（DENO_DIR） | `""` | `Frontend/Deno/Cache` |
| `WorkingDirectory` | Deno 工作目录 | `null` | Worker 脚本所在目录 |
| `Arguments` | Deno 命令行参数 | `[]` | `["task", "run", "frontend-worker.ts"]` |
| `IgnoreStartupFailure` | 是否忽略启动失败 | `true` | `true`（生产环境），`false`（开发环境） |

### 6.3 路径解析辅助类

**DenoRuntimeAssetResolver**（`src/Jolt/Frontend/Deno/Hosting/DenoRuntimeAssetResolver.cs`）提供静态方法：

| 方法 | 说明 |
|------|------|
| `ResolveBundledExecutablePath()` | 解析打包的 Deno 可执行文件路径（按 RID 查找） |
| `ResolveWorkerPath()` | 解析 frontend-worker.ts 路径（先找输出目录，再找源目录） |
| `ResolveWorkingDirectory()` | 解析工作目录（优先显式配置，否则用 worker 所在目录） |
| `ResolveCacheDirectory()` | 解析缓存目录（`Frontend/Deno/Cache`） |
| `CreateMissingRuntimeMessage()` | 创建缺失运行时错误消息 |

**支持的 RID**（`PortableRuntimeIdentifiers`）：
- `win-x64`, `win-arm64`
- `linux-x64`, `linux-arm64`
- `osx-x64`, `osx-arm64`

## 7. 与其他子系统的交互

### 7.1 与 DenoWorkerProcess 的交互

**关系**：`DenoVolarHost` 是 `IDenoWorkerProcess` 的包装器

**交互模式**：
- 生命周期管理：调用 `StartAsync` / `StopAsync`
- 请求发送：调用 `SendRequestAsync<TResult>(method, payload, cancellationToken)`
- 状态查询：访问 `IsRunning` 属性

**关注点分离**：
- `DenoVolarHost`：高级 API、重试逻辑、故障恢复
- `DenoWorkerProcess`：低级进程管理、stdin/stdout 通信、线程安全

### 7.2 与 LSP 客户端的交互

**入口**：`DenoVolarHost` 的智能感知方法

**数据流**：
```
LSP 客户端请求
    ↓
LspSession（Jolt）
    ↓
DenoVolarHost.GetTemplateCompletionItemsAsync()
    ↓
SendAsync<LspCompletionItem[]>("template/completion", ...)
    ↓
DenoWorkerProcess.SendRequestAsync<TResult>()
    ↓
Deno worker 进程（frontend-worker.ts）
    ↓
Volar 服务
    ↓
返回结果
```

### 7.3 与编译系统的交互

**入口**：`DenoVolarIntelliSenseContext`

**数据流**：
```
C# 编译（Razor/Vue）
    ↓
生成 SemanticContext + ArtifactRecord
    ↓
传入 DenoVolarHost 智能感知方法
    ↓
打包到 DenoTemplateRequest.FrontendContext/FrontendArtifacts
    ↓
发送到 Deno worker
    ↓
Volar 使用 C# 语义信息增强智能感知
```

**设计意图**：
- 将 C# 编译的语义信息传递给 Volar
- 支持跨语言引用（C# 组件 → Vue 模板）
- 实现完整的全栈智能感知

### 7.4 与 DevServer/HMR 的交互

**场景**：开发模式下，Volar 提供实时智能感知

**交互模式**：
- DevServer 启动时：调用 `DenoVolarHost.StartAsync()`
- 文件变更时：更新 `DenoVolarIntelliSenseContext`，重新请求智能感知
- DevServer 关闭时：调用 `DenoVolarHost.StopAsync()` / `DisposeAsync()`

## 8. 设计权衡

### 8.1 IgnoreStartupFailure 默认为 true

**权衡**：
- **优点**：Deno 缺失时不影响 Jolt 核心功能（编译、构建），系统可以优雅降级
- **缺点**：用户可能不知道 Volar 功能被禁用了

**缓解措施**：
- 提供 `IsEnabled` 和 `IsRunning` 属性供检测
- 建议开发环境设置为 `false`，尽早发现配置问题
- 生产环境使用 `true` 避免单点故障

### 8.2 自动重试 + 指数退避

**权衡**：
- **优点**：自动从临时故障恢复（worker 崩溃、网络抖动），提高可用性
- **缺点**：可能掩盖系统性问题（如 Deno 版本不兼容），延迟失败反馈

**缓解措施**：
- 限制重试次数（3 次）
- 记录警告日志（JSON 格式，便于监控）
- 3 次失败后抛出异常，避免无限重试

### 8.3 生命周期锁和请求锁分离

**权衡**：
- **优点**：启动/停止操作不会阻塞正在进行的请求，提高并发性能
- **缺点**：增加了复杂度，需要协调两个锁

**设计正确性保证**：
- `SendAsync` 中调用 `EnsureStartedAsync` 时不会持有 `_lifecycleGate`
- `TryResetWorkerStateAsync` 需要先获取 `_lifecycleGate`，防止与 StartAsync/StopAsync 竞态
- `DenoWorkerProcess` 内部的两个锁独立，互不阻塞

### 8.4 吞掉 StopAsync 的所有异常

**权衡**：
- **优点**：确保系统可以从任何状态恢复，停止失败不会阻止后续操作
- **缺点**：可能掩盖资源泄漏问题

**设计依据**：
- .NET 进程退出时会清理所有子进程
- Deno worker 是无状态的，重启即可恢复
- 优先保证系统可用性，而不是完美清理

### 8.5 JSON-RPC 协议选择

**权衡**：
- **优点**：简单、跨语言、易于调试（可读的 JSON 文本）
- **缺点**：性能不如二进制协议（如 gRPC、MessagePack）

**适用性分析**：
- LSP 智能感知请求的频率相对较低（用户键入触发）
- JSON 性能损失可接受（< 1ms）
- 简单性更重要：易于与 TypeScript/JavaScript 互操作
- Volar 本身使用 JSON-RPC（与 LSP 协议一致）
