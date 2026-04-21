# Jolt 实现代码修复清单

> 审查日期：2026-04-21
> 审查范围：`src/Jolt/` 全部模块（~100+ 源文件）
> 审查维度：Bug、缺陷、不完整实现、缺失的错误处理、线程安全、资源泄漏、性能

---

## 汇总统计

| 严重度 | 数量 | 说明 |
|--------|------|------|
| **Critical** | 18 | 必须修复，可能导致崩溃、数据丢失、安全漏洞 |
| **High** | 36 | 应当修复，影响稳定性或正确性 |
| **Medium** | 41 | 建议修复，影响性能、可维护性或可观测性 |
| **Low** | 17 | 可延后，代码质量改进 |
| **合计** | **112** | |

---

## 一、Critical（必须修复）

### 1.1 并发与线程安全

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-01 | Lsp | `Lsp/StdioLspServer.cs:134-162` | **请求取消 TOCTOU 竞态**：`_pendingCancellationRequests` 检查与 `_activeRequests` 添加之间存在竞态窗口，取消请求可能被遗漏 | 将检查和添加操作保持在同一锁范围内 |
| C-02 | Lsp | `Lsp/LspSession.ProviderIsolationAndRouting.cs:160-178` | **Provider 隔离字典竞态**：字典遍历与修改不在同一原子操作中，重构时易引入竞态 | 使用 `ConcurrentDictionary` 原子操作或将所有操作保持在锁内 |
| C-03 | Debug | `Debug/CdpClient.cs:629-644` | **CDP 客户端 Dispose 竞态**：`DisposeAsync` 取消 pending 请求时，`ReadLoopAsync` 可能仍在处理响应并完成 TCS | 添加 `_disposing` 标志，在处理响应前检查 |
| C-04 | Debug | `Debug/DapSession.cs:41-52` | **DapSession 属性访问无线程安全**：`CurrentCallFrames` setter 访问 `_currentCallFrames` 无同步保护，`IsPaused` 和 `ResetVariableReferences()` 可能并发调用 | 使用 `Lock _stateGate` 保护所有状态访问 |
| C-05 | Services | `Services/JoltService.cs:59-71` | **启动/关闭竞态**：`StartAsync` 和 `StopAsync` 使用 `Interlocked.Exchange` 但不保证操作原子性，可能在启动过程中被关闭打断 | 使用 `SemaphoreSlim` 或 `AsyncManualResetEvent` 保护状态转换 |
| C-06 | DevServer | `DevServer/DevHttpServer.cs:354-378` | **文件变更去抖器竞态**：`QueueFileChange` 中 `_fileChangeDebouncer` 的 null 检查与方法调用之间存在竞态，Dispose 可能导致 NRE 或 ODE | 将 debouncer 存入局部变量，或使用 `Interlocked.CompareExchange` 模式 |
| C-07 | DevServer | `DevServer/FileChangeDebouncer.cs:24-43` | **去抖器锁释放后竞态**：锁释放后调用 `ScheduleFlushAsync`，CTS 可能在此时被 Dispose | 将 `ScheduleFlushAsync` 调用移入锁内 |

### 1.2 资源泄漏

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-08 | Lsp | `Lsp/StdioLspServer.cs:149-162` | **CancellationTokenSource 未释放**：`cancelledBeforeExecution` 早返回路径上，`requestCancellationSource` 未被 Dispose，每次预取消请求泄漏一个 CTS | 在早返回前显式 Dispose，或调整 `using` 声明位置 |
| C-09 | Extensions | `ExtensionLoader.cs:519-547` | **ExtensionLoadContext 无法卸载**：扩展订阅事件或持有 Registry 引用时，CollectibleAssemblyLoadContext 无法被 GC 回收 | 使用弱引用回调，卸载前显式清除扩展引用并添加卸载失败日志 |
| C-10 | Build | `Build/BundlerModuleProxyServer.cs:33-38` | **HttpClient 未配置连接池**：`SocketsHttpHandler` 未设置 `PooledConnectionLifetime`，长时间运行可能导致连接池耗尽 | 设置 `PooledConnectionLifetime` 并实现 `IDisposable` 确保清理 |

### 1.3 进程与连接管理

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-11 | Analysis | `Analysis/ProcessAnalysisRpcTransport.cs:42-73` | **子进程异常路径未终止**：`process.Start()` 后若序列化抛异常，进程不会被终止，成为孤儿进程 | 将 `process.Start()` 移入 try 块，catch 中调用 `TerminateProcessAsync` |
| C-12 | Debug | `ExtensionWorkerClient.cs:371-413` | **Process.Kill 可能留下孤儿子进程**：Windows 平台上 `entireProcessTree: true` 可能无法终止所有后代进程 | 添加 Job Object 回退或 WMI 进程枚举确保完全清理 |
| C-13 | Debug | `Debug/CdpConnection.cs:43-69` | **WebSocket 接收不完整消息**：连接在消息帧中间关闭时，返回截断的消息且调用方无法检测 | 添加消息完整性验证，返回 `Result<string?, bool>` 指示完整/截断 |
| C-14 | Debug | `ExtensionWorkerServer.cs:237-271` | **扩展 Provider 调用无超时**：恶意或缺陷扩展可无限挂起，阻塞 Worker 进程 | 所有 Provider invoke 添加 `CancellationToken` 及 30 秒超时 |

### 1.4 安全

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-15 | DevServer | `DevHttpServer.cs:194-227` | **静态文件路径遍历**：`ModuleResolver.Resolve` 后未二次验证路径是否在根目录内，URL 编码绕过可能读取根目录外文件 | 解析后添加 `IsInsideRoot` 边界验证 |
| C-16 | Build | `Build/CssUrlRewriter.cs:93-118` | **CSS URL 重写路径遍历**：`NormalizeLookupPath` 解析后未验证路径是否在项目根内，`../../../etc/passwd` 可能逃逸 | URI 解析后添加根边界验证 |

### 1.5 内存

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-17 | Lsp | `Lsp/Lanes/VolarLaneService.cs:23` | **DenoFailureSnapshots 无界增长**：`ConcurrentDictionary` 只增不减，长期运行导致内存持续增长 | 实现定期清理或 LRU 驱逐 |
| C-18 | Workspace | `Workspace/JoltWorkspaceResolver.cs:9` | **WorkspaceFileCache 无界增长**：静态缓存无大小限制或过期策略，大项目下占用大量内存 | 实现 LRU 驱逐（如 max 1000 条目）或 TTL 过期 |

---

## 二、High（应当修复）

### 2.1 错误处理缺失

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-01 | Rpc | `Rpc/StdioJoltRpcServer.cs:20-27` | **取消异常未捕获**：`ReadLineAsync` 抛出 `OperationCanceledException` 未被 catch，可能崩溃服务器 | 添加 `catch (OperationCanceledException)` 作为正常关闭路径 |
| H-02 | Analysis | `Analysis/StdioVueAnalysisRpcServer.cs:20-27` | **同 H-01**：相同的取消异常处理缺失 | 同上 |
| H-03 | Roslyn | `Roslyn/InProc/InProcRoslynCodeService.SymbolsAndSemantic.cs:1396-1408` | **MetadataReference 创建可能失败**：`TRUSTED_PLATFORM_ASSEMBLIES` 中的无效路径会导致 `MetadataReference.CreateFromFile` 抛异常，直接崩溃服务 | try-catch 跳过无效引用并记录警告 |
| H-04 | Lsp | `Lsp/StdioLspServer.cs:106-109` | **空 catch 块吞掉异常**：通知处理中的异常被静默忽略，无法调试 | 至少记录日志 |
| H-05 | Analysis | `Analysis/ProcessAnalysisRpcTransport.cs:71-72` | **JSON 反序列化异常未处理**：畸形响应导致 `JsonException` 未被捕获，进程状态不明 | try-catch 包装并提供有意义的错误信息 |
| H-06 | Analysis | `Analysis/FallbackJazorAnalysisService.cs:70-86` | **数组越界风险**：直接访问 `artifacts[0]`、`artifacts[1]` 无边界检查 | 添加长度验证 |

### 2.2 资源管理

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-07 | Program | `Program.cs:122-126` | **Console.CancelKeyPress 委托泄漏**：事件处理器注册后未取消注册，长期运行或测试场景中委托累积 | 存储委托引用并在 finally 中取消注册 |
| H-08 | Frontend | `Frontend/Deno/Hosting/DenoWorkerProcess.cs:401-404` | **启动工作区清理未执行**：`ProcessExit` 仅在 AppDomain 卸载时触发，正常 Ctrl+C 关闭不执行清理 | 实现 `IDisposable` 或在 `StopAsync` 中调用清理 |
| H-09 | Debug | `Debug/CdpClient.cs:37` | **ScriptUrl 字典无界增长**：`_scriptUrlById` 随脚本解析单调增长，多次页面导航后内存持续增长 | 页面导航时清理旧条目或实现 LRU 缓存 |
| H-10 | DevServer | `DevServer/DevServerReloadHub.cs:10-12` | **WebSocket 跟踪字典泄漏**：`_sockets` 中 `HmrClientState`（含 SemaphoreSlim）可能因竞态未被 Dispose | 使用显式清理跟踪或 WeakReference 模式 |
| H-11 | DevServer | `DevServer/CompilationCache.cs:80-86` | **InvalidateAll 非原子**：`GetPaths` 返回快照后，新条目可能在 `InvalidateAll` 期间被添加导致遗漏 | 锁定缓存和 SourceMap 操作，或将操作合并为原子操作 |

### 2.3 超时与死锁

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-12 | Analysis | `Analysis/ProcessAnalysisRpcTransport.cs:75-89` | **ReadResponseJsonAsync 无限循环**：子进程只输出非 JSON 行时方法永远循环 | 添加最大行数限制（如 1000 行） |
| H-13 | Analysis | `Analysis/ProcessAnalysisRpcTransport.cs:21-73` | **RPC 通信无超时**：子进程挂起时方法无限等待（除非调用方提供 CT） | 添加默认超时（如 30 秒） |
| H-14 | Frontend | `Frontend/Deno/Hosting/DenoVolarHost.cs:335-414` | **重试无退避**：失败后立即重启 Worker 并重试一次，瞬态故障需要更长恢复时间 | 添加指数退避、限制重试次数、记录所有失败 |
| H-15 | Frontend | `Frontend/Deno/Hosting/DenoWorkerProcess.cs:125-184` | **请求重试可能嵌套获取锁**：`DenoVolarHost.SendAsync` 失败后重试可能导致双重发送 | 实现请求生命周期状态机，确保幂等性 |
| H-16 | DevServer | `DevServer/Proxy/DevServerProxy.cs:209-221` | **HttpClient 无超时配置**：上游服务器挂起时请求阻塞 100 秒（默认超时） | 设置合理超时（如 30 秒） |

### 2.4 正确性

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-17 | Lsp | `Lsp/LspProtocolHelpers.cs:26-49` | **偏移量计算溢出**：`GetOffset` 对超出文档范围的行号不返回错误，返回错误偏移 | 验证行号在有效范围内 |
| H-18 | Lsp | `Lsp/LspSession.cs:524-528` | **Rename 缺少标识符验证**：仅检查空白，未验证非法字符或保留关键字 | 添加标识符有效性验证 |
| H-19 | Lsp | `Lsp/LspSession.WorkspaceFolders.cs:83-93` | **工作区文件夹返回可变数组**：克隆后返回可变数组，调用方修改可能引起问题 | 返回不可变数组或只读集合 |
| H-20 | Razor | `Razor/InProc/RazorDesignTimeCodeProjectionService.cs:365-376` | **反射访问无保护**：通过反射获取私有属性 `SourceMappings` 无异常保护，Razor SDK 变更将导致运行时失败 | 添加 try-catch 并记录回退激活 |
| H-21 | VirtualDocs | `VirtualDocuments/Registry/InMemoryVirtualDocumentRegistry.cs:71-87` | **RemoveBySourceDocumentAsync 竞态**：`TryRemove` 获取 projectedPaths 后迭代删除期间，另一线程可重新添加条目 | 锁定整个操作或使用事务性方法 |
| H-22 | SourceMap | `SourceMap/InMemorySourceMapService.cs:231-253` | **DecodeVlq 潜在无限循环**：恶意输入含大量 continuation bit 时循环长时间运行 | 添加最大数字计数限制（如 10 位） |
| H-23 | VirtualDocs | `VirtualDocuments/Mapping/ProjectionSegment.cs:14-18` | **ProjectionSegment 边界计算无验证**：`OriginalLength` 和 `ProjectedLength` 可能为负数 | 构造函数中添加 `>= 0` 验证 |
| H-24 | Build | `Build/BuildOrchestrator.RuntimeAndIncremental.cs:649-689` | **增量构建 TOCTOU**：`File.Exists` 检查与后续文件读取之间存在竞态，文件可能被删除 | try-catch 包装文件访问 |
| H-25 | Build | `Build/BundlerModuleProxyServer.cs:100-137` | **代理未处理上游错误**：4xx/5xx 响应体被直接转发，可能泄露内部错误信息 | 检查 `IsSuccessStatusCode`，返回清理后的错误页面 |
| H-26 | Extensions | `ExtensionLoader.cs:540-546` | **扩展双重停用**：激活失败路径上 finally 块调用 `TryDeactivateSilentlyAsync`，调用方 catch 块也调用，导致双重停用 | 移除 catch 块中的冗余停用调用 |
| H-27 | Extensions | `OutOfProcessExtensionProxy.cs:331-376` | **Worker 重启无熔断**：Worker 立即崩溃后无限重启，无退避或失败阈值 | 添加重启计数器、指数退避和最大重启限制（如 1 分钟内 3 次） |
| H-28 | DevServer | `DevServer/DependencyGraph.cs` | **依赖图清理非原子**：与 H-11 类似，清理操作与缓存操作不同步 | 合并操作为原子步骤 |
| H-29 | Program | `Program.cs:477-506` | **workspaceStore 可能为 null**：传递给 `DevHttpServer` 时未验证是否可为 null | 添加 `ArgumentNullException.ThrowIfNull` 或文档说明 null 场景 |
| H-30 | Workspace | `Workspace/JoltWorkspaceResolver.cs:52-56` | **缓存清空过于激进**：`InvalidatePath(null)` 调用 `Clear()` 删除所有缓存，可能导致反复全量重建 | 使用版本化缓存，仅清除相关条目 |

### 2.5 安全

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-31 | Extensions | `ExtensionSandboxProfile.cs:74-76` | **网络沙箱通配符绕过**：`AllowedHosts: ["*"]` 允许任何主机，可在 loopback 限制模式下绕过限制 | loopback 模式下拒绝通配符 |
| H-32 | Extensions | `ExtensionSecurityPolicy.cs:397-401` | **SHA256 计算加载整个文件**：`File.ReadAllBytes` 将大型扩展程序全部加载到内存 | 使用 `FileStream` 流式计算哈希 |
| H-33 | Extensions | `ExtensionWorkerClient.cs:344-347` | **stderr 缓冲区可无限增长**：超过 8KB 阈值后才裁剪，快速输出时缓冲区可能远超限制 | 先裁剪再追加，或使用环形缓冲区 |

---

## 三、Medium（建议修复）

### 3.1 性能

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-01 | Roslyn | `Roslyn/InProc/InProcRoslynCodeService.cs:913-923` | **每次请求创建新 Compilation**：`TryCreateContext` 每次 LSP 请求都创建 `CSharpCompilation`，大工作区下延迟明显 | 实现基于内容哈希的 Compilation 缓存 |
| M-02 | Roslyn | `InProcRoslynCodeService.ProjectionAndContext.cs:304` | **热路径 SHA256 无缓存**：`CreateContainerName` 每次投影创建都计算哈希 | 添加 `ConcurrentDictionary<string, string>` 缓存 |
| M-03 | Lsp | `Lsp/Aggregation/LspResultAggregator.cs:10-22` | **GroupBy 使用字符串拼接**：诊断去重使用 `string.Join` 拼接键，效率低 | 实现 `IEqualityComparer<T>` |
| M-04 | Lsp | `Lsp/Lanes/VolarLaneService.cs:563-574` | **正则匹配阻塞**：`TryGetTagCompletionPrefix` 每次补全请求都对前缀做正则匹配 | 使用手写字符串解析替代 |
| M-05 | Lsp | `Lsp/LspSession.cs:19-21` | **TagNamePattern 正则热路径**：编译正则反复匹配大文档 | 缓存匹配结果或使用更高效的扫描方法 |
| M-06 | SourceMap | `SourceMap/InMemorySourceMapService.cs:75-117` | **GeneratedPositionFor 全扫描**：遍历所有 SourceMap 所有段，O(n*m) 复杂度 | 构建反向索引实现 O(1) 查找 |
| M-07 | Build | `Build/BuildOrchestrator.CssPipeline.cs:235-322` | **CSS 压缩低效字符串操作**：逐行正则替换创建大量临时字符串 | 使用 `Span<char>` 或整体处理后分割 |
| M-08 | Roslyn | `InProcRoslynCodeService.cs:19-21` | **静态 Regex 编译不可卸载**：`RegexOptions.Compiled` 生成本机代码无法卸载 | 低频使用场景移除 `Compiled` 选项 |
| M-09 | DevServer | `DevServer/CompilationCache.cs:5-9` | **缓存无大小限制**：`_entries` 字典无界增长 | 实现 LRU 缓存或使用 `MemoryCache` |
| M-10 | Debug | `Debug/CdpClient.cs:40` | **请求 ID 可能溢出**：`int` 类型在极长调试会话中溢出变负 | 改用 `long` |
| M-11 | Build | `BundlerModuleProxyServer.cs:14-15` | **正则 ReDoS 风险**：`JavaScriptImportSpecifierPattern` 复杂交替分支可能被恶意 JS 触发回溯爆炸 | 使用 `RegexOptions.NonBacktracking` 或实现简单解析器 |

### 3.2 可观测性

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-12 | Lsp | `Lsp/LspSession.cs:798-808` | **工作区文档变更异常被吞**：通用 catch 吞掉所有异常，无法检测 HMR 协调问题 | 添加结构化日志 |
| M-13 | Analysis | `Analysis/FallbackJazorAnalysisService.cs:18-22` | **遥测失败影响核心功能**：`FallbackTelemetry.ReportActivation` 异常导致分析操作失败 | try-catch 包装遥测调用 |
| M-14 | Hosting | `Hosting/ChildProcessUtilities.cs:61-66` | **空 catch 块**：进程终止时的异常被静默吞掉 | 添加 debug 级别日志 |
| M-15 | Rpc | `Analysis/RpcVueAnalysisClient.cs:28-37` | **RPC 失败无日志**：错误抛出但未记录，生产环境难以调试 | 添加 debug/trace 日志 |
| M-16 | Roslyn | `InProcRoslynCodeService.cs:17` | **LanguageVersion.Preview 不稳定**：预览语言特性变更可能导致解析失败 | 使用 `CSharp14` 或通过设置配置 |

### 3.3 正确性

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-17 | Lsp | `Lsp/LspSession.TextAndFormatting.cs:153-156` | **混合换行符处理**：仅检查是否包含 CRLF，未处理同一文档内的混合换行符 | 按行保留原始换行或使用最常见换行风格 |
| M-18 | Lsp | `Lsp/LspProtocolHelpers.cs:15-24` | **URI 解析路径遍历**：`ToDocumentPath` 未验证解析后路径是否在工作区边界内 | 验证路径在工作区边界内 |
| M-19 | Lsp | `Lsp/Lanes/VolarLaneService.cs:190-210` | **冗余调用**：`PrepareCallHierarchyAsync` 第一次返回空后，第二次无 `openDocuments` 调用不会产生不同结果 | 移除冗余的第二次调用 |
| M-20 | Lsp | `Lsp/Coordination/MarkupComponentBridgeService.cs:733-793` | **文件迭代中无取消检查**：`AddDocumentsFromDirectoryAsync` 循环内未检查 `cancellationToken` | 在每次迭代开始添加 `ThrowIfCancellationRequested` |
| M-21 | VirtualDocs | `VirtualDocuments/Mapping/ProjectionMap.cs:92-107` | **偏移映射边界错误**：当投影内容短于原始内容时，偏移可能越界 | 添加额外边界检查 |
| M-22 | VirtualDocs | `VirtualDocuments/Mapping/ProjectionMap.cs:8-16` | **投影段无重叠验证**：构造函数不验证段是否重叠或有序 | 添加重叠检查并确保段已排序 |
| M-23 | SourceMap | `InMemorySourceMapService.cs:246` | **VLQ 解码整数溢出**：`result += digit << shift` 在 shift 较大时溢出 | 检查 `shift < 32` |
| M-24 | Workspace | `JoltWorkspaceResolver.cs:78-142` | **路径规范化无深度限制**：恶意或损坏路径含 10000+ 层级导致内存问题 | 添加最大路径深度限制（如 256 层） |
| M-25 | Workspace | `JazorRelatedDocumentResolver.cs:99` | **文件解析失败静默返回 null**：调用方无法知道失败原因 | 返回 `Result<DocumentSnapshot, Error>` 或记录失败日志 |
| M-26 | VirtualDocs | `InMemoryVirtualDocumentRegistry.cs:8-11` | **虚拟文档无自动清理**：源文档关闭或删除后虚拟文档不会自动清理 | 实现弱引用或定期清理 |
| M-27 | Rpc | `Protocol/Contracts/RpcMessages.cs:3-13` | **请求 ID 可为 null**：错误响应无法关联到请求 | 自动生成 ID 或强制要求非 null |
| M-28 | Rpc | `Protocol/Contracts/ProtocolJsonSerializer.cs:10-14` | **反序列化输入无验证**：传入 null 或空字符串抛通用 `JsonException` | 添加输入验证抛出更有意义的异常 |
| M-29 | Analysis | `VueAnalysisClientFactory.cs:23-30` | **命令行参数越界**：`arg["--analysis-command=".Length..]` 当参数等于前缀时抛异常 | 添加长度检查 |
| M-30 | Roslyn | `InProcRoslynCodeService.cs:18` | **静态 MetadataReferences 永不释放**：长时间运行进程中阻止程序集卸载 | 改为实例级或使用弱引用跟踪 |
| M-31 | Razor | `Razor/Toolset/RazorSdkToolsetResolver.cs:109-120` | **非 Windows 平台路径错误**：硬编码 `/usr/share/dotnet` 在部分 Linux 发行版或 macOS 上不存在 | 添加备选路径和 `dotnet --info` 解析回退 |
| M-32 | DevServer | `DevServer/ChangeProcessor.cs:322-327` | **错误编译结果可能被下游使用**：部分路径未检查 `result.IsError` 就访问内容 | 所有路径统一检查 `IsError` |
| M-33 | Debug | `Debug/DapRequestHandler.cs:258-278` | **evaluate 请求无速率限制**：快速连续请求可能压垮 CDP 后端 | 添加 Semaphore 或速率限制器 |
| M-34 | Debug | `Debug/BreakpointManager.cs:9-17` | **断点源路径无存在性验证**：无效路径静默映射为 null | 添加 `File.Exists` 检查或日志 |
| M-35 | Extensions | `ExtensionLoader.cs:156-160` | **GC 周期不足**：强制 3 次 GC 可能不足以回收 CollectibleAssemblyLoadContext | 增至 5-10 次并使用 `GCCollectionMode.Forced` |
| M-36 | Extensions | `ExtensionRegistry.cs:15-77` | **Provider 调用重放污染健康数据**：日志重放时 `ReportProviderInvocation` 更新当前健康统计 | 添加 `isReplay` 参数跳过健康更新 |
| M-37 | Extensions | `ExtensionHostOptionsResolver.cs:238-253` | **布尔解析不一致**：`TryParseBoolean` 结果被忽略，重复解析 | 简化为一次解析并提前返回 |

### 3.4 内存

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-38 | DevServer | `DevServer/DevHttpServer.cs:214-227` | **静态文件缺少 MIME 类型验证**：仅按扩展名确定类型，不验证内容匹配 | 生产构建时验证文件签名 |
| M-39 | Workspace | `InMemoryWorkspaceStore.cs:8-9` | **快照不一致**：`GetOpenDocumentsAsync` 返回的有序快照在枚举期间可能不一致 | 文档此限制为可接受行为 |
| M-40 | SourceMap | `InMemorySourceMapService.cs:140-176` | **Parse 不验证数组长度**：`sourcesContent` 和 `sources` 长度不匹配时 `IndexOutOfRangeException` | 添加长度匹配验证 |

---

## 四、Low（可延后）

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| L-01 | Lsp | `Lsp/Lanes/VolarLaneService.cs:1339` | 魔法数字 `2` 表示诊断严重度 | 使用枚举常量 |
| L-02 | Lsp | `Lsp/Coordination/MarkupComponentBridgeService.cs:75-83` | 字符串比较模式不一致（Ordinal vs OrdinalIgnoreCase） | 标准化同类操作的比较模式 |
| L-03 | Lsp | `Lsp/Lanes/RoslynLaneService.cs` | Lane Service 间重复代码（IsCodeTarget 等模式） | 提取公共逻辑到基类或帮助方法 |
| L-04 | Lsp | 多个文件 | 公共/内部方法缺少 XML 文档注释 | 添加 `<summary>` 文档 |
| L-05 | Roslyn | `InProcRoslynCodeService.ProjectionAndContext.cs:305-306` | SHA256 截断 4 字节（8 hex）碰撞概率约 1/40 亿 | 增至 8-16 字节或文档说明可接受原因 |
| L-06 | Frontend | `DenoWorkerProcess.cs:12, 299-309` | stderr 缓冲区 32 行无截断指示 | 添加丢弃行数计数器 |
| L-07 | Frontend | `DenoWorkerProcess.cs:387` | `File.Copy` 使用 `overwrite: true` 可能覆盖用户修改 | 文档说明临时工作区隔离设计 |
| L-08 | Frontend | `DenoWorkerProcess.cs:328` | 大 stderr 输出使用 `string.Join` 效率低 | 使用 `StringBuilder` 或限制行数 |
| L-09 | Rpc | `Protocol/Contracts/Requests.cs:138-143` | JSON 属性名未显式标注 `[JsonPropertyName]` | 添加显式 JSON 属性名标注 |
| L-10 | Rpc | `Protocol/Documents/DocumentVersion.cs:3-9` | DocumentVersion 无输入验证 | 添加 `TryCreate` 方法 |
| L-11 | VirtualDocs | `RazorDesignTimeCodeProjectionService.cs:127-147` | null/空字符串路径处理意图不明确 | 提取 `IsRelevantMappingSegment` 帮助方法 |
| L-12 | Debug | `DapProtocol.cs:116-128` | 缺少 Content-Length 头与载荷长度匹配验证 | 添加协议合规验证 |
| L-13 | Debug | `VariableMapper.cs:37-56` | 变量值可能包含敏感信息或内部 CDP 错误消息 | 清理或截断超长值 |
| L-14 | Debug | `NullExtensionRegistry.cs:17-123` | 所有操作静默无操作，调试困难 | 添加 debug 级别日志 |
| L-15 | Build | `BuildOrchestrator.CssPipeline.cs:244-280` | CSS 压缩中使用魔法字符串（`;}`等） | 提取为命名常量 |
| L-16 | DevServer | `DevServerReloadHub.cs:273-276` | 无心跳超时检测，僵尸 WebSocket 连接不清理 | 添加定期检查 `LastSeenUtc` 并移除过期客户端 |
| L-17 | Debug | `CallStackMapper.cs:29` | 匿名函数格式化潜在的 null 字符问题 | 使用 `?.Trim() ?? "(anonymous)"` |

---

## 五、模块级优先修复建议

按影响范围和风险排序：

### 第一优先级：核心稳定性

1. **LSP 请求处理管道**（C-01, C-08, H-01, H-04）— 修复竞态、资源泄漏和异常处理
2. **进程生命周期管理**（C-11, C-12, H-07, H-08）— 确保子进程不泄漏、清理可靠执行
3. **JoltService 启停**（C-05）— 修复启停竞态

### 第二优先级：安全与正确性

4. **静态文件服务安全**（C-15, C-16, H-31）— 路径遍历防护
5. **调试会话管理**（C-03, C-04, C-13, C-14）— 竞态、消息完整性、超时
6. **Roslyn 编译管道**（H-03, M-01, M-02）— 错误处理和性能

### 第三优先级：长期运行稳定性

7. **内存增长控制**（C-17, C-18, H-09, M-09）— 无界集合限制
8. **扩展系统健壮性**（C-09, H-26, H-27）— 卸载、重启、熔断
9. **DevServer 文件监听**（C-06, C-07, H-11, H-24）— 竞态和原子性

### 第四优先级：性能与可维护性

10. **缓存与索引优化**（M-06, M-01, M-03）— 编译缓存、SourceMap 索引
11. **可观测性增强**（M-12, M-13, M-14, M-15）— 日志和错误可见性
12. **代码清理**（Low 级别项目）— 魔法数字、文档、常量提取

---

> 本清单由 6 个并行审查 agent 生成，覆盖 Analysis、Rpc、Protocol、Lsp、Build、DevServer、Debug、Extensions、Workspace、VirtualDocuments、SourceMap、Roslyn、Razor、Services 共 14 个模块。
