# Jolt 实现代码修复清单

> 审查日期：2026-04-21（2026-04-23 补记增量问题）
> 审查范围：`src/Jolt/` 全部模块（~100+ 源文件）
> 审查维度：Bug、缺陷、不完整实现、缺失的错误处理、线程安全、资源泄漏、性能

---

## 汇总统计

| 严重度 | 数量 | 说明 |
|--------|------|------|
| **Critical** | 18 | 必须修复，可能导致崩溃、数据丢失、安全漏洞 |
| **High** | 35 | 应当修复，影响稳定性或正确性 |
| **Medium** | 43 | 建议修复，影响性能、可维护性或可观测性 |
| **Low** | 17 | 可延后，代码质量改进 |
| **合计** | **113** | |

---

## 一、Critical（必须修复）

### 1.1 并发与线程安全

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-01 | Lsp | `Lsp/StdioLspServer.cs:134-162` | **请求取消 TOCTOU 竞态**：`_pendingCancellationRequests` 检查与 `_activeRequests` 添加之间存在竞态窗口，取消请求可能被遗漏 | 已完成（2026-04-21 当前基线）：pending cancel 检查与 active request 注册已在同一锁范围内处理，执行前取消不再丢失，并补充了立即取消回归测试 |
| C-02 | Lsp | `Lsp/LspSession.ProviderIsolationAndRouting.cs:160-178` | **Provider 隔离字典竞态**：字典遍历与修改不在同一原子操作中，重构时易引入竞态 | 已完成（2026-04-21 当前基线）：provider 隔离窗口读写统一在 `_providerIsolationGate` 下完成 |
| C-03 | Debug | `Debug/CdpClient.cs:629-644` | **CDP 客户端 Dispose 竞态**：`DisposeAsync` 取消 pending 请求时，`ReadLoopAsync` 可能仍在处理响应并完成 TCS | 已完成（2026-04-21 当前基线）：Dispose 设置 `_disposing`，读循环在完成 pending 前检查并短路 |
| C-04 | Debug | `Debug/DapSession.cs:41-52` | **DapSession 属性访问无线程安全**：`CurrentCallFrames` setter 访问 `_currentCallFrames` 无同步保护，`IsPaused` 和 `ResetVariableReferences()` 可能并发调用 | 已完成（2026-04-21 本轮）：使用 `_stateGate` 保护 call frame、paused 状态和变量引用表 |
| C-05 | Services | `Services/JoltService.cs:59-71` | **启动/关闭竞态**：`StartAsync` 和 `StopAsync` 使用 `Interlocked.Exchange` 但不保证操作原子性，可能在启动过程中被关闭打断 | 已完成（2026-04-21 本轮）：`StartAsync`/`StopAsync` 通过 `_lifecycleGate` 串行化状态转换 |
| C-06 | DevServer | `DevServer/DevHttpServer.cs:354-378` | **文件变更去抖器竞态**：`QueueFileChange` 中 `_fileChangeDebouncer` 的 null 检查与方法调用之间存在竞态，Dispose 可能导致 NRE 或 ODE | 已完成（2026-04-21 当前基线）：先捕获 debouncer 局部变量并隔离 shutdown 竞态异常 |
| C-07 | DevServer | `DevServer/FileChangeDebouncer.cs:24-43` | **去抖器锁释放后竞态**：锁释放后调用 `ScheduleFlushAsync`，CTS 可能在此时被 Dispose | 已完成（2026-04-21 当前基线）：调度任务在锁内绑定当前 CTS，flush 端校验引用一致 |

### 1.2 资源泄漏

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-08 | Lsp | `Lsp/StdioLspServer.cs:149-162` | **CancellationTokenSource 未释放**：`cancelledBeforeExecution` 早返回路径上，`requestCancellationSource` 未被 Dispose，每次预取消请求泄漏一个 CTS | 已完成（2026-04-21 当前基线）：预取消早返回路径上的 CTS 已随请求生命周期一致释放 |
| C-09 | Extensions | `ExtensionLoader.cs:519-547` | **ExtensionLoadContext 无法卸载**：扩展订阅事件或持有 Registry 引用时，CollectibleAssemblyLoadContext 无法被 GC 回收 | 已完成（2026-04-21 本轮）：可收集加载上下文卸载后增加 `WeakReference` 探测、5 次 `Forced` GC 和卸载失败 warning，显式暴露残留引用问题 |
| C-10 | Build | `Build/BundlerModuleProxyServer.cs:33-38` | **HttpClient 未配置连接池**：`SocketsHttpHandler` 未设置 `PooledConnectionLifetime`，长时间运行可能导致连接池耗尽 | 已完成（2026-04-21 当前基线）：bundler proxy 使用 `PooledConnectionLifetime` 且在 `DisposeAsync` 中清理 `HttpClient` |

### 1.3 进程与连接管理

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-11 | Analysis | `Analysis/ProcessAnalysisRpcTransport.cs:42-73` | **子进程异常路径未终止**：`process.Start()` 后若序列化抛异常，进程不会被终止，成为孤儿进程 | 已完成（2026-04-21 当前基线）：进程启动和 RPC 写读均在 try 内，异常路径终止子进程 |
| C-12 | Debug | `ExtensionWorkerClient.cs:371-413` | **Process.Kill 可能留下孤儿子进程**：Windows 平台上 `entireProcessTree: true` 可能无法终止所有后代进程 | 已完成（2026-04-21 本轮）：统一走 `ChildProcessUtilities.TerminateProcessAsync`，Windows 下增加 `taskkill /T /F` 兜底并限制等待时间 |
| C-13 | Debug | `Debug/CdpConnection.cs:43-69` | **WebSocket 接收不完整消息**：连接在消息帧中间关闭时，返回截断的消息且调用方无法检测 | 已完成（2026-04-21 当前基线）：分片 UTF-8 采用流式解码，消息中途关闭时抛出受控异常而非返回截断文本 |
| C-14 | Debug | `ExtensionWorkerServer.cs:237-271` | **扩展 Provider 调用无超时**：恶意或缺陷扩展可无限挂起，阻塞 Worker 进程 | 已完成（2026-04-21 当前基线）：Provider 调用已纳入超时控制并受宿主取消传播约束 |

### 1.4 安全

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-15 | DevServer | `DevHttpServer.cs:194-227` | **静态文件路径遍历**：`ModuleResolver.Resolve` 后未二次验证路径是否在根目录内，URL 编码绕过可能读取根目录外文件 | 已完成（2026-04-21 当前基线）：解析后统一执行 `IsInsideRoot` 边界校验，越界请求直接拒绝 |
| C-16 | Build | `Build/CssUrlRewriter.cs:93-118` | **CSS URL 重写路径遍历**：`NormalizeLookupPath` 解析后未验证路径是否在项目根内，`../../../etc/passwd` 可能逃逸 | 已完成（2026-04-21 当前基线）：段归一化在遇到超出根目录的 `..` 时直接拒绝 |

### 1.5 内存

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| C-17 | Lsp | `Lsp/Lanes/VolarLaneService.cs:23` | **DenoFailureSnapshots 无界增长**：`ConcurrentDictionary` 只增不减，长期运行导致内存持续增长 | 已完成（2026-04-21 本轮）：失败快照存储增加上限裁剪，长期运行不再单调膨胀 |
| C-18 | Workspace | `Workspace/JoltWorkspaceResolver.cs:9` | **WorkspaceFileCache 无界增长**：静态缓存无大小限制或过期策略，大项目下占用大量内存 | 已完成（2026-04-21 当前基线）：workspace 文件缓存增加 1000 条上限和年龄驱逐 |

---

## 二、High（应当修复）

### 2.1 错误处理缺失

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-01 | Rpc | `Rpc/StdioJoltRpcServer.cs:20-27` | **取消异常未捕获**：`ReadLineAsync` 抛出 `OperationCanceledException` 未被 catch，可能崩溃服务器 | 已完成（2026-04-21 当前基线）：stdio RPC 读循环将取消作为正常关闭路径处理 |
| H-02 | Analysis | `Analysis/StdioVueAnalysisRpcServer.cs:20-27` | **同 H-01**：相同的取消异常处理缺失 | 已完成（2026-04-21 当前基线）：analysis stdio server 将取消作为正常关闭路径处理 |
| H-03 | Roslyn | `Roslyn/InProc/InProcRoslynCodeService.SymbolsAndSemantic.cs:1396-1408` | **MetadataReference 创建可能失败**：`TRUSTED_PLATFORM_ASSEMBLIES` 中的无效路径会导致 `MetadataReference.CreateFromFile` 抛异常，直接崩溃服务 | 已完成（2026-04-21 本轮）：无效 metadata reference 已被捕获并写入 stderr warning，避免服务崩溃 |
| H-04 | Lsp | `Lsp/StdioLspServer.cs:106-109` | **空 catch 块吞掉异常**：通知处理中的异常被静默忽略，无法调试 | 已完成（2026-04-21 本轮）：通知异常写入 stderr 结构化事件后继续隔离 |
| H-05 | Analysis | `Analysis/ProcessAnalysisRpcTransport.cs:71-72` | **JSON 反序列化异常未处理**：畸形响应导致 `JsonException` 未被捕获，进程状态不明 | 已完成（2026-04-21 当前基线）：JSON 反序列化错误被包装为含 stderr/stdout 摘要的受控异常 |
| H-06 | Analysis | `Analysis/FallbackJazorAnalysisService.cs:70-86` | **数组越界风险**：直接访问 `artifacts[0]`、`artifacts[1]` 无边界检查 | 已完成（2026-04-21 当前基线）：改为按 `ArtifactKind` 查找并在缺失时抛出受控异常 |

### 2.2 资源管理

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-07 | Program | `Program.cs:122-126` | **Console.CancelKeyPress 委托泄漏**：事件处理器注册后未取消注册，长期运行或测试场景中委托累积 | 已完成（2026-04-21 当前基线）：事件处理器在 `finally` 中取消注册 |
| H-08 | Frontend | `Frontend/Deno/Hosting/DenoWorkerProcess.cs:401-404` | **启动工作区清理未执行**：`ProcessExit` 仅在 AppDomain 卸载时触发，正常 Ctrl+C 关闭不执行清理 | 已完成（2026-04-21 当前基线）：`StopAsync`/失败路径显式清理 launch workspace，`ProcessExit` 仅作兜底 |
| H-09 | Debug | `Debug/CdpClient.cs:37` | **ScriptUrl 字典无界增长**：`_scriptUrlById` 随脚本解析单调增长，多次页面导航后内存持续增长 | 已完成（2026-04-21 本轮）：脚本 URL 跟踪增加上限和先进先出驱逐 |
| H-10 | DevServer | `DevServer/DevServerReloadHub.cs:10-12` | **WebSocket 跟踪字典泄漏**：`_sockets` 中 `HmrClientState`（含 SemaphoreSlim）可能因竞态未被 Dispose | 已完成（2026-04-21 本轮）：连接移除统一释放状态，并增加 heartbeat sweep 清理过期客户端 |
| H-11 | DevServer | `DevServer/CompilationCache.cs:80-86` | **InvalidateAll 非原子**：`GetPaths` 返回快照后，新条目可能在 `InvalidateAll` 期间被添加导致遗漏 | 已完成（2026-04-21 当前基线）：`InvalidateAll` 在缓存锁内清空并返回快照 |

### 2.3 超时与死锁

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-12 | Analysis | `Analysis/ProcessAnalysisRpcTransport.cs:75-89` | **ReadResponseJsonAsync 无限循环**：子进程只输出非 JSON 行时方法永远循环 | 已完成（2026-04-21 当前基线）：非 JSON stdout 探测限制为 1000 行并保留尾部摘要 |
| H-13 | Analysis | `Analysis/ProcessAnalysisRpcTransport.cs:21-73` | **RPC 通信无超时**：子进程挂起时方法无限等待（除非调用方提供 CT） | 已完成（2026-04-21 当前基线）：analysis RPC 增加 30 秒默认超时并终止子进程 |
| H-14 | Frontend | `Frontend/Deno/Hosting/DenoVolarHost.cs:335-414` | **重试无退避**：失败后立即重启 Worker 并重试一次，瞬态故障需要更长恢复时间 | 已完成（2026-04-21 本轮）：Worker 恢复改为最多 3 次指数退避，逐次记录失败并在重试前重置状态 |
| H-15 | Frontend | `Frontend/Deno/Hosting/DenoWorkerProcess.cs:125-184` | **请求重试可能嵌套获取锁**：`DenoVolarHost.SendAsync` 失败后重试可能导致双重发送 | 已完成（2026-04-21 本轮）：请求恢复路径已收敛为受控重试序列，避免失败重发时的嵌套发送状态漂移 |
| H-16 | DevServer | `DevServer/Proxy/DevServerProxy.cs:209-221` | **HttpClient 无超时配置**：上游服务器挂起时请求阻塞 100 秒（默认超时） | 已完成（2026-04-21 当前基线）：dev proxy `HttpClient` 默认超时收敛为 30 秒 |

### 2.4 正确性

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-17 | Lsp | `Lsp/LspProtocolHelpers.cs:26-49` | **偏移量计算溢出**：`GetOffset` 对超出文档范围的行号不返回错误，返回错误偏移 | 已完成（2026-04-21 当前基线）：越界行/列现在抛出 `ArgumentOutOfRangeException`，EOF 合法位置仍返回 `text.Length` |
| H-18 | Lsp | `Lsp/LspSession.cs:524-528` | **Rename 缺少标识符验证**：仅检查空白，未验证非法字符或保留关键字 | 已完成（2026-04-21 当前基线）：`textDocument/rename` 现在拒绝数字开头、空格和非法字符的新名称 |
| H-19 | Lsp | `Lsp/LspSession.WorkspaceFolders.cs:83-93` | **工作区文件夹返回可变数组**：克隆后返回可变数组，调用方修改可能引起问题 | 已完成（2026-04-21 当前基线）：工作区文件夹/root 快照改为返回只读集合 |
| H-20 | Razor | `Razor/InProc/RazorDesignTimeCodeProjectionService.cs:365-376` | **反射访问无保护**：通过反射获取私有属性 `SourceMappings` 无异常保护，Razor SDK 变更将导致运行时失败 | 已完成（2026-04-21 本轮）：`SourceMappings` 反射访问已加异常保护并写 warning，回退行为可观测 |
| H-21 | VirtualDocs | `VirtualDocuments/Registry/InMemoryVirtualDocumentRegistry.cs:71-87` | **RemoveBySourceDocumentAsync 竞态**：`TryRemove` 获取 projectedPaths 后迭代删除期间，另一线程可重新添加条目 | 已完成（2026-04-21 当前基线）：源文档移除路径已原子化处理并覆盖回归场景 |
| H-22 | SourceMap | `SourceMap/InMemorySourceMapService.cs:231-253` | **DecodeVlq 潜在无限循环**：恶意输入含大量 continuation bit 时循环长时间运行 | 已完成（2026-04-21 当前基线）：VLQ 解码已增加 continuation 位数上限，恶意输入会快速失败 |
| H-23 | VirtualDocs | `VirtualDocuments/Mapping/ProjectionSegment.cs:14-18` | **ProjectionSegment 边界计算无验证**：`OriginalLength` 和 `ProjectedLength` 可能为负数 | 已完成（2026-04-21 当前基线）：ProjectionSegment 构造已拒绝负长度输入 |
| H-24 | Build | `Build/BuildOrchestrator.RuntimeAndIncremental.cs:649-689` | **增量构建 TOCTOU**：`File.Exists` 检查与后续文件读取之间存在竞态，文件可能被删除 | 已完成（2026-04-21 当前基线）：文件存在性与读取已统一落在异常边界内，瞬时消失文件不再炸掉增量路径 |
| H-25 | Build | `Build/BundlerModuleProxyServer.cs:100-137` | **代理未处理上游错误**：4xx/5xx 响应体被直接转发，可能泄露内部错误信息 | 已完成（2026-04-21 当前基线）：非成功上游响应已转换为清洗后的错误正文，不再透传内部堆栈/原文 |
| H-26 | Extensions | `ExtensionLoader.cs:540-546` | **扩展双重停用**：激活失败路径上 finally 块调用 `TryDeactivateSilentlyAsync`，调用方 catch 块也调用，导致双重停用 | 已完成（2026-04-21 当前基线）：激活失败的停用收敛为单一路径，不再双重停用 |
| H-27 | Extensions | `OutOfProcessExtensionProxy.cs:331-376` | **Worker 重启无熔断**：Worker 立即崩溃后无限重启，无退避或失败阈值 | 已完成（2026-04-21 本轮）：worker 重启增加窗口计数、指数退避和熔断阈值，单次异常仍可恢复，连续崩溃会停止无限拉起并返回受控错误 |
| H-28 | DevServer | `DevServer/DependencyGraph.cs` | **依赖图清理非原子**：与 H-11 类似，清理操作与缓存操作不同步 | 已完成（2026-04-21 当前基线）：依赖图清理与缓存失效已收敛为原子步骤 |
| H-29 | Program | `Program.cs:477-506` | **workspaceStore 可能为 null**：传递给 `DevHttpServer` 时未验证是否可为 null | 已完成（2026-04-21 本轮）：standalone dev 显式传 null，LSP+dev 使用非空专用工厂并校验 |
| H-30 | Workspace | `Workspace/JoltWorkspaceResolver.cs:52-56` | **缓存清空过于激进**：`InvalidatePath(null)` 调用 `Clear()` 删除所有缓存，可能导致反复全量重建 | 已完成（2026-04-21 当前基线）：工作区缓存改为定向失效/版本化清理，避免频繁全量重建 |

### 2.5 安全

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-31 | Extensions | `ExtensionSandboxProfile.cs:74-76` | **网络沙箱通配符绕过**：`AllowedHosts: ["*"]` 允许任何主机，可在 loopback 限制模式下绕过限制 | 已完成（2026-04-21 当前基线）：loopback 模式下 `*` 现在显式拒绝，不能再旁路主机限制 |
| H-32 | Extensions | `ExtensionSecurityPolicy.cs:397-401` | **SHA256 计算加载整个文件**：`File.ReadAllBytes` 将大型扩展程序全部加载到内存 | 已完成（2026-04-21 当前基线）：SHA256 改为 `FileStream` 流式计算 |
| H-33 | Extensions | `ExtensionWorkerClient.cs:344-347` | **stderr 缓冲区可无限增长**：超过 8KB 阈值后才裁剪，快速输出时缓冲区可能远超限制 | 已完成（2026-04-21 本轮）：stderr 缓冲改为追加前裁剪并保留超长单行尾部，增长已被严格封顶 |

### 2.6 增量问题

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| H-34 | DevServer | `DevServer/Proxy/DevServerProxy.cs:81`, `DevServer/Proxy/DevServerProxy.cs:109`, `DevServer/DevHttpServer.cs:113` | **Dev proxy 上游不可达未收敛为受控 502/504**：HTTP 转发直接等待 `_httpClient.SendAsync(...)`，WebSocket 转发直接等待 `ConnectAsync(...)`，路由层直接等待 `_proxy.TryProxyAsync(context)`；`HttpRequestException`、连接超时和 WebSocket 握手失败可能穿透为未受控错误 | 已完成（2026-04-23 本轮）：proxy 边界已把上游连接失败收敛为 502、超时收敛为 504，并输出 `devProxyUpstreamFailure` 结构化错误；补充 HTTP upstream unavailable 与 WebSocket handshake failure 回归 |
| H-35 | Frontend | `Volar/Deno/Hosting/DenoWorkerProcess.cs:245-262` | **DenoWorkerProcess.StopAsync 存在 HasExited -> Kill/WaitForExitAsync TOCTOU 竞态**：进程可能在 `HasExited` 判断后、`Kill` 或 `WaitForExitAsync` 前自行退出，导致停机路径抛出非预期异常或产生不稳定清理结果 | 已完成（2026-04-23 本轮）：StopAsync 现在基于本地 process 快照执行终止，并将进程已退出/已分离视为幂等成功路径；补充已退出 worker 清理回归 |

---

## 三、Medium（建议修复）

### 3.1 性能

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-01 | Roslyn | `Roslyn/InProc/InProcRoslynCodeService.cs:913-923` | **每次请求创建新 Compilation**：`TryCreateContext` 每次 LSP 请求都创建 `CSharpCompilation`，大工作区下延迟明显 | 已完成（2026-04-21 本轮）：`TryCreateContext` 已接入有界 compilation context cache，重复请求不再每次重建 Compilation |
| M-02 | Roslyn | `InProcRoslynCodeService.ProjectionAndContext.cs:304` | **热路径 SHA256 无缓存**：`CreateContainerName` 每次投影创建都计算哈希 | 已完成（2026-04-21 本轮）：container name 生成已增加哈希缓存 |
| M-03 | Lsp | `Lsp/Aggregation/LspResultAggregator.cs:10-22` | **GroupBy 使用字符串拼接**：诊断去重使用 `string.Join` 拼接键，效率低 | 已完成（2026-04-21 本轮）：诊断聚合改用 `IEqualityComparer<LspDiagnostic>` 和稳定 HashSet 去重 |
| M-04 | Lsp | `Lsp/Lanes/VolarLaneService.cs:563-574` | **正则匹配阻塞**：`TryGetTagCompletionPrefix` 每次补全请求都对前缀做正则匹配 | 已完成（2026-04-21 本轮）：补全前缀检测已改为手写扫描 |
| M-05 | Lsp | `Lsp/LspSession.cs:19-21` | **TagNamePattern 正则热路径**：编译正则反复匹配大文档 | 已完成（2026-04-21 本轮）：linked editing / 标签名检测已改为手写扫描，不再依赖热路径 regex |
| M-06 | SourceMap | `SourceMap/InMemorySourceMapService.cs:75-117` | **GeneratedPositionFor 全扫描**：遍历所有 SourceMap 所有段，O(n*m) 复杂度 | 已完成（2026-04-21 本轮）：source map 已建立按 sourcePath 的反向索引，查询不再全量扫描所有段 |
| M-07 | Build | `Build/BuildOrchestrator.CssPipeline.cs:235-322` | **CSS 压缩低效字符串操作**：逐行正则替换创建大量临时字符串 | 已完成（2026-04-21 本轮）：CSS 压缩逻辑已收敛为复用静态模式和命名常量，避免热路径临时字符串膨胀 |
| M-08 | Roslyn | `InProcRoslynCodeService.cs:19-21` | **静态 Regex 编译不可卸载**：`RegexOptions.Compiled` 生成本机代码无法卸载 | 已完成（2026-04-21 本轮）：低频 using-directive regex 已移除 `Compiled` |
| M-09 | DevServer | `DevServer/CompilationCache.cs:5-9` | **缓存无大小限制**：`_entries` 字典无界增长 | 已完成（2026-04-21 当前基线）：`CompilationCache` 已实现 `DefaultMaxEntries = 512` 的 LRU 驱逐 |
| M-10 | Debug | `Debug/CdpClient.cs:40` | **请求 ID 可能溢出**：`int` 类型在极长调试会话中溢出变负 | 已完成（2026-04-21 本轮）：CDP request id 改为 `long` |
| M-11 | Build | `BundlerModuleProxyServer.cs:14-15` | **正则 ReDoS 风险**：`JavaScriptImportSpecifierPattern` 复杂交替分支可能被恶意 JS 触发回溯爆炸 | 已完成（2026-04-21 本轮）：import/specifier 重写已迁移到语法感知扫描器，移除了这类复杂 regex 风险 |

### 3.2 可观测性

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-12 | Lsp | `Lsp/LspSession.cs:798-808` | **工作区文档变更异常被吞**：通用 catch 吞掉所有异常，无法检测 HMR 协调问题 | 已完成（2026-04-21 本轮）：workspace change sink 异常写入 stderr 结构化事件后继续隔离 |
| M-13 | Analysis | `Analysis/FallbackJazorAnalysisService.cs:18-22` | **遥测失败影响核心功能**：`FallbackTelemetry.ReportActivation` 异常导致分析操作失败 | 已完成（2026-04-21 当前基线）：遥测调用已被 try/catch 包裹，不再影响 fallback 分析 |
| M-14 | Hosting | `Hosting/ChildProcessUtilities.cs:61-66` | **空 catch 块**：进程终止时的异常被静默吞掉 | 已完成（2026-04-21 本轮）：终止异常已写 debug 日志 |
| M-15 | Rpc | `Analysis/RpcVueAnalysisClient.cs:28-37` | **RPC 失败无日志**：错误抛出但未记录，生产环境难以调试 | 已完成（2026-04-21 当前基线）：RPC 失败路径已记录 transport/rpc/empty-payload 等类型化日志 |
| M-16 | Roslyn | `InProcRoslynCodeService.cs:17` | **LanguageVersion.Preview 不稳定**：预览语言特性变更可能导致解析失败 | 已完成（2026-04-21 本轮）：解析选项已固定为 `CSharp14` |

### 3.3 正确性

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-17 | Lsp | `Lsp/LspSession.TextAndFormatting.cs:153-156` | **混合换行符处理**：仅检查是否包含 CRLF，未处理同一文档内的混合换行符 | 已完成（2026-04-21 当前基线）：格式化已按 dominant newline 选择输出换行风格，避免单纯布尔判断 CRLF |
| M-18 | Lsp | `Lsp/LspProtocolHelpers.cs:15-24` | **URI 解析路径遍历**：`ToDocumentPath` 未验证解析后路径是否在工作区边界内 | 已完成（2026-04-21 本轮）：LSP 文档入口统一增加工作区边界校验，越界 URI 不再进入文档状态机或读盘路径 |
| M-19 | Lsp | `Lsp/Lanes/VolarLaneService.cs:190-210` | **冗余调用**：`PrepareCallHierarchyAsync` 第一次返回空后，第二次无 `openDocuments` 调用不会产生不同结果 | 已完成（2026-04-21 本轮）：冗余第二次调用已移除 |
| M-20 | Lsp | `Lsp/Coordination/MarkupComponentBridgeService.cs:733-793` | **文件迭代中无取消检查**：`AddDocumentsFromDirectoryAsync` 循环内未检查 `cancellationToken` | 已完成（2026-04-21 本轮）：循环内增加取消检查，并预索引 open documents |
| M-21 | VirtualDocs | `VirtualDocuments/Mapping/ProjectionMap.cs:92-107` | **偏移映射边界错误**：当投影内容短于原始内容时，偏移可能越界 | 已完成（2026-04-21 当前基线）：投影偏移映射已增加额外边界保护 |
| M-22 | VirtualDocs | `VirtualDocuments/Mapping/ProjectionMap.cs:8-16` | **投影段无重叠验证**：构造函数不验证段是否重叠或有序 | 已完成（2026-04-21 当前基线）：ProjectionMap 构造已验证段顺序与重叠关系 |
| M-23 | SourceMap | `InMemorySourceMapService.cs:246` | **VLQ 解码整数溢出**：`result += digit << shift` 在 shift 较大时溢出 | 已完成（2026-04-21 当前基线）：VLQ 解码已限制 shift/位数，避免整数溢出 |
| M-24 | Workspace | `JoltWorkspaceResolver.cs:78-142` | **路径规范化无深度限制**：恶意或损坏路径含 10000+ 层级导致内存问题 | 已完成（2026-04-21 当前基线）：路径规范化增加 256 段安全上限 |
| M-25 | Workspace | `JazorRelatedDocumentResolver.cs:99` | **文件解析失败静默返回 null**：调用方无法知道失败原因 | 已完成（2026-04-21 本轮）：磁盘文档解析 IO/ACL/不支持路径失败写入 stderr 结构化事件 |
| M-26 | VirtualDocs | `InMemoryVirtualDocumentRegistry.cs:8-11` | **虚拟文档无自动清理**：源文档关闭或删除后虚拟文档不会自动清理 | 已完成（2026-04-21 当前基线）：源文档关闭路径已调用 `RemoveBySourceDocumentAsync` 自动清理虚拟文档 |
| M-27 | Rpc | `Protocol/Contracts/RpcMessages.cs:3-13` | **请求 ID 可为 null**：错误响应无法关联到请求 | 已完成（2026-04-21 当前基线）：RPC 请求 envelope 已强制非空 ID，错误响应可稳定关联 |
| M-28 | Rpc | `Protocol/Contracts/ProtocolJsonSerializer.cs:10-14` | **反序列化输入无验证**：传入 null 或空字符串抛通用 `JsonException` | 已完成（2026-04-21 当前基线）：对 null/空白 JSON 先抛出有意义的 `ArgumentException` |
| M-29 | Analysis | `VueAnalysisClientFactory.cs:23-30` | **命令行参数越界**：`arg["--analysis-command=".Length..]` 当参数等于前缀时抛异常 | 已完成（2026-04-21 当前基线）：读取命令行值前先校验前缀长度，空值不再切片越界 |
| M-30 | Roslyn | `InProcRoslynCodeService.cs:18` | **静态 MetadataReferences 永不释放**：长时间运行进程中阻止程序集卸载 | 已完成（2026-04-21 本轮）：metadata references 已下沉为实例级，placeholder semantic model 也改为实例上下文持有 |
| M-31 | Razor | `Razor/Toolset/RazorSdkToolsetResolver.cs:109-120` | **非 Windows 平台路径错误**：硬编码 `/usr/share/dotnet` 在部分 Linux 发行版或 macOS 上不存在 | 已完成（2026-04-21 当前基线）：增加多个非 Windows 备选根和 `dotnet --info` Base Path 回退 |
| M-32 | DevServer | `DevServer/ChangeProcessor.cs:322-327` | **错误编译结果可能被下游使用**：部分路径未检查 `result.IsError` 就访问内容 | 已完成（2026-04-21 当前基线）：ChangeProcessor 下游访问前已统一检查 `IsError` |
| M-33 | Debug | `Debug/DapRequestHandler.cs:258-278` | **evaluate 请求无速率限制**：快速连续请求可能压垮 CDP 后端 | 已完成（2026-04-21 本轮）：evaluate 处理增加 `SemaphoreSlim` 串行化 |
| M-34 | Debug | `Debug/BreakpointManager.cs:9-17` | **断点源路径无存在性验证**：无效路径静默映射为 null | 已完成（2026-04-21 本轮）：断点映射失败且缺少可用 rooted 源路径时输出结构化 stderr 警告，避免静默丢失诊断线索 |
| M-35 | Extensions | `ExtensionLoader.cs:156-160` | **GC 周期不足**：强制 3 次 GC 可能不足以回收 CollectibleAssemblyLoadContext | 已完成（2026-04-21 本轮）：强制 GC 周期已提升到 5 次并显式使用 `GCCollectionMode.Forced` |
| M-36 | Extensions | `ExtensionRegistry.cs:15-77` | **Provider 调用重放污染健康数据**：日志重放时 `ReportProviderInvocation` 更新当前健康统计 | 已完成（2026-04-21 本轮）：`isReplay` 跳过健康统计和 live sink，仅保留历史快照 |
| M-37 | Extensions | `ExtensionHostOptionsResolver.cs:238-253` | **布尔解析不一致**：`TryParseBoolean` 结果被忽略，重复解析 | 已完成（2026-04-21 当前基线）：所有布尔 override 只解析一次并在失败时跳过 |

### 3.4 内存

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-38 | DevServer | `DevServer/DevHttpServer.cs:214-227` | **静态文件缺少 MIME 类型验证**：仅按扩展名确定类型，不验证内容匹配 | 已处理（2026-04-21 本轮）：dev server 当前仅服务编译器已知文本模块，并新增 `X-Content-Type-Options: nosniff`；二进制资产签名校验不属于当前路由面的职责 |
| M-39 | Workspace | `InMemoryWorkspaceStore.cs:8-9` | **快照不一致**：`GetOpenDocumentsAsync` 返回的有序快照在枚举期间可能不一致 | 已处理（2026-04-21 本轮）：该接口语义已明确为“调用时点有序快照”，后续变更不会回写到既有枚举结果，按设计接受 |
| M-40 | SourceMap | `InMemorySourceMapService.cs:140-176` | **Parse 不验证数组长度**：`sourcesContent` 和 `sources` 长度不匹配时 `IndexOutOfRangeException` | 已完成（2026-04-21 当前基线）：`Parse` 现在先验证 `sourcesContent` 与 `sources` 长度一致 |

### 3.5 增量问题

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| M-41 | Workspace | `Workspace/InMemoryWorkspaceStore.cs:8`, `Workspace/InMemoryWorkspaceStore.cs:45`, `Workspace/InMemoryWorkspaceStore.cs:82`, `Workspace/JoltWorkspaceResolver.cs:16`, `Workspace/JoltWorkspaceResolver.cs:55`, `Workspace/JoltWorkspaceResolver.cs:217`, `Workspace/JoltWorkspaceResolver.cs:354` | **路径比较固定使用 OrdinalIgnoreCase**：文档存储、缓存、去重和路径相等判断大量按大小写不敏感处理；在 Linux/macOS 上 `Foo.jazor` 与 `foo.jazor` 会被覆盖、去重或错命中缓存 | 已完成（2026-04-23 本轮，当前生产仅 Windows）：新增 `WorkspacePathComparison`，workspace 存储、缓存、去重和包含关系统一走平台相关路径比较；Windows 保持忽略大小写，非 Windows 改为 `Ordinal` |
| M-42 | Extensions | `Extensions/ExtensionLoader.cs:83`, `Extensions/ExtensionLoader.cs:95`, `Program.cs:294`, `Program.cs:305` | **用户扩展目录枚举失败没有隔离**：`LoadUserExtensionsAsync` 在 `Directory.Exists(...)` 后直接 `Directory.EnumerateDirectories(...)`，根目录枚举异常会沿启动链穿透到 `Program`，导致 LSP 启动失败 | 已完成（2026-04-23 本轮）：用户扩展根目录枚举改为 safe wrapper，目录消失、ACL/IO、非法路径等枚举失败降级为 `ExtensionLoadStatus.Failed` 记录并继续启动；补充 root enumeration failure 回归 |
| M-43 | Lsp | `Lsp/StdioLspServer.cs:37`, `Lsp/StdioLspServer.cs:44`, `Lsp/StdioLspServer.cs:111`, `Lsp/StdioLspServer.cs:144`, `Lsp/StdioLspServer.cs:187` | **StdioLspServer 请求队列无界**：`Channel.CreateUnbounded<LspRequestMessage>` 允许客户端无限入队，`_requestExecutionGate` 只限制执行并发，不限制排队长度 | 已完成（2026-04-23 本轮）：请求通道改为有界队列，并增加 request admission gate 限制已接收未完成请求总量；过载请求返回 JSON-RPC `-32000` 并输出 `lspRequestQueueFull` warning，补充 burst/backpressure 回归 |

---

## 四、Low（可延后）

| # | 模块 | 文件 | 问题 | 修复方向 |
|---|------|------|------|----------|
| L-01 | Lsp | `Lsp/Lanes/VolarLaneService.cs:1339` | 魔法数字 `2` 表示诊断严重度 | 已完成（2026-04-21 本轮）：诊断严重度魔法数字已替换为命名常量 |
| L-02 | Lsp | `Lsp/Coordination/MarkupComponentBridgeService.cs:75-83` | 字符串比较模式不一致（Ordinal vs OrdinalIgnoreCase） | 已完成（2026-04-21 当前基线）：当前已按数据域统一比较模式，路径/文档键忽略大小写，符号名与 import 标识符保持区分大小写 |
| L-03 | Lsp | `Lsp/Lanes/RoslynLaneService.cs` | Lane Service 间重复代码（IsCodeTarget 等模式） | 已完成（2026-04-21 本轮）：Roslyn lane 已抽取统一的 open-document/fallback helper，减少重复分支逻辑 |
| L-04 | Lsp | 多个文件 | 公共/内部方法缺少 XML 文档注释 | 已处理（2026-04-21 本轮）：`Jolt` 当前是 internal implementation 为主的可执行项目，未把 XML docs 作为编译契约；对外说明继续放在模块文档而非对内部方法批量补 `<summary>` |
| L-05 | Roslyn | `InProcRoslynCodeService.ProjectionAndContext.cs:305-306` | SHA256 截断 4 字节（8 hex）碰撞概率约 1/40 亿 | 已完成（2026-04-21 本轮）：container hash 截断已提升到 8 bytes |
| L-06 | Frontend | `DenoWorkerProcess.cs:12, 299-309` | stderr 缓冲区 32 行无截断指示 | 已完成（2026-04-21 当前基线）：stderr 摘要已增加丢弃行数计数器 |
| L-07 | Frontend | `DenoWorkerProcess.cs:387` | `File.Copy` 使用 `overwrite: true` 可能覆盖用户修改 | 已处理（2026-04-21 当前基线）：覆盖仅发生在每次启动新建的临时 launch workspace，设计边界已文档化，不触碰用户工作目录 |
| L-08 | Frontend | `DenoWorkerProcess.cs:328` | 大 stderr 输出使用 `string.Join` 效率低 | 已完成（2026-04-21 本轮）：stderr 汇总已改为 `StringBuilder` 迭代拼接 |
| L-09 | Rpc | `Protocol/Contracts/Requests.cs:138-143` | JSON 属性名未显式标注 `[JsonPropertyName]` | 已完成（2026-04-21 本轮）：请求/响应 DTO 字段补齐显式 `[JsonPropertyName]` |
| L-10 | Rpc | `Protocol/Documents/DocumentVersion.cs:3-9` | DocumentVersion 无输入验证 | 已完成（2026-04-21 当前基线）：构造函数增加空白校验，并补充 `TryCreate(string?)` / `TryCreate(int)` |
| L-11 | VirtualDocs | `RazorDesignTimeCodeProjectionService.cs:127-147` | null/空字符串路径处理意图不明确 | 已完成（2026-04-21 本轮）：相关判断已提取为 `IsRelevantMappingSegment` 帮助方法 |
| L-12 | Debug | `DapProtocol.cs:116-128` | 缺少 Content-Length 头与载荷长度匹配验证 | 已完成（2026-04-21 当前基线）：DAP 读帧路径已验证 framing/JSON 边界，畸形长度或负载不会再静默穿透 |
| L-13 | Debug | `VariableMapper.cs:37-56` | 变量值可能包含敏感信息或内部 CDP 错误消息 | 已完成（2026-04-21 本轮）：调试变量展示值增加长度上限和截断标记 |
| L-14 | Debug | `NullExtensionRegistry.cs:17-123` | 所有操作静默无操作，调试困难 | 已完成（2026-04-21 本轮）：no-op 操作已增加一次性 debug 提示 |
| L-15 | Build | `BuildOrchestrator.CssPipeline.cs:244-280` | CSS 压缩中使用魔法字符串（`;}`等） | 已完成（2026-04-21 本轮）：CSS 压缩魔法字符串已提取为命名常量 |
| L-16 | DevServer | `DevServerReloadHub.cs:273-276` | 无心跳超时检测，僵尸 WebSocket 连接不清理 | 已完成（2026-04-21 本轮）：后台 heartbeat sweep 定期检查 `LastSeenUtc` 并移除过期客户端 |
| L-17 | Debug | `CallStackMapper.cs:29` | 匿名函数格式化潜在的 null 字符问题 | 已完成（2026-04-21 本轮）：函数名裁剪 null/空白后再回退到 `(anonymous)` |

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
