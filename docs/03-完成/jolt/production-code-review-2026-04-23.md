# Jolt 生产级代码审查报告

审查日期：2026-04-23

审查范围：`src/Jolt`、`src/Jolt.Test`、`docs/01-目标/jolt`、`docs/03-完成/jolt/status.md`

审查方式：静态代码审查；本轮未运行 `dotnet test`。测试状态仅引用仓库内既有状态文档。

## 1. 生产结论

结论：在当前文档声明的 Windows、本机/受控内网、可信项目和可信扩展边界内，Jolt 的主干设计可以进入生产试用和持续硬化；不应宣称为公网服务、多租户平台或不可信扩展市场运行时。需要优先处理 LSP 通知可靠性、Deno worker 请求超时、DevServer/HMR 资源上限、静态资源缓存策略这几类 P1 风险。

当前没有发现必须立即阻断本机可信使用场景的 P0 问题。审查中发现的主要问题集中在“协议状态不可丢”“外部 worker 不可无限等待”“开发服务器不可无上限吸收输入”“构建产物应满足长期缓存不变式”四个生产标准上。

## 2. 边界假设

Jolt 架构文档已经明确当前生产边界，`--build` 是正式构建 lane，`--dev` 和 `--preview` 只用于本机或受控内网，`--lsp` 只供本机编辑器和工具链使用。见 `docs/01-目标/jolt/architecture.md:31` 到 `docs/01-目标/jolt/architecture.md:46`。

本报告按上述边界评估。如果把 `jolt --dev`、`jolt --preview`、扩展 worker 或 Deno worker 暴露给公网、不可信项目、不可信扩展或多租户环境，下文多个 P1/P2 风险会升级为发布阻断问题。

## 3. 架构与理论审查

整体实现思路是正确的：Jolt 把 `.jazor` 开发体验拆成 Build、DevServer、LSP、DAP、Extension、Deno/Volar worker 等子系统，并通过投影、SourceMap、`.slnx` 作用域和多 lane LSP 路由来保持边界清晰。这个方向比把 Razor/Vue/TS 逻辑揉成单一大服务更可维护，也更容易分别压测和回归。

LSP 的三 lane 模型是合理的：Jazor lane 处理 `.jazor` 结构和内建语义，Roslyn lane 处理 C# 投影，Volar/Deno lane 处理前端模板和 TypeScript 能力。理论上，三 lane 的正确性依赖同一个事实源：打开文档、投影状态、workspace folder 和 bridge context 必须一致。因此 LSP 状态变更通知不能按“可丢事件”处理，否则后续请求再正确也可能建立在旧状态上。

Build lane 的理论目标应是可复现、可发布、可长期缓存。当前主 bundle/CSS 输出已经按内容 hash 方向处理，路径信任校验也很强；但 public/source 静态资源对大文件跳过 hash，这破坏了“文件名代表内容”的长期缓存不变式。

DevServer 的理论目标更接近 Vite 本地开发服务器，不是生产 HTTP 服务。当前设计适合本机可信开发，但不具备公网服务所需的认证、Origin 约束、请求体限制、连接数限制和代理访问控制。

Extension 与 Deno worker 的理论边界应表述为“可信代码 + Jolt 级策略检查”，不是“强沙箱”。扩展系统已经有 manifest、签名、权限、worker 隔离等措施，但代码本身仍可在宿主或子进程里获得普通 OS 权限；Deno 默认参数也允许读取和环境变量访问。

## 4. 正向观察

| 领域 | 观察 |
| --- | --- |
| 架构分层 | Jolt 的 Build、DevServer、LSP、Debug、Extension、Deno worker 子系统职责清楚，入口 `src/Jolt/Program.cs` 虽然较重，但组合边界可识别。 |
| LSP 设计 | `.jazor`、C#、Volar 前端能力通过 lane router、projection resolver、coordinator 和 aggregator 组合，避免单个 provider 承担所有职责。 |
| Workspace 作用域 | `.slnx` 作为唯一解决方案边界的方向清晰，符合仓库测试规范和文档约束。 |
| Build 安全 | `BuildOrchestrator` 和 `DenoBundleRunner` 对 output/assets 路径做了 inside-directory 和 reparse point 检查，删除/重写前调用可信路径校验，见 `src/Jolt/Build/DenoBundleRunner.cs:50`、`src/Jolt/Build/DenoBundleRunner.cs:691`、`src/Jolt/Build/DenoBundleRunner.cs:993`。 |
| 静态资源 IO | `StaticAssetHandler` 对 symlink/reparse point、路径逃逸、锁文件有防护和降级诊断，且 hash 计算已改为流式读取，见 `src/Jolt/Build/StaticAssetHandler.cs:253`。 |
| 测试基础 | `docs/03-完成/jolt/status.md:67` 记录 `src/Jolt.Test` 最新全量为 748/748 通过，`docs/03-完成/jolt/status.md:114` 记录 compiler 回归 2391/2391 通过。 |

## 5. 风险总览

| ID | 严重级别 | 领域 | 结论 |
| --- | --- | --- | --- |
| P1-01 | Important | LSP | 状态变更通知可能在队列满时被丢弃，违反 LSP 状态一致性要求。 |
| P1-02 | Important | LSP | 未知 `$/cancelRequest` id 会进入无界集合，异常客户端可造成内存增长。 |
| P1-03 | Important | LSP / Workspace | LSP 请求路径从磁盘 fallback 后会写入 open document store，污染打开文档状态。 |
| P1-04 | Important | Deno / Volar | Deno worker 请求无默认超时和 circuit breaker，worker 卡死时 LSP/Build 可能无限等待。 |
| P1-05 | Important | DevServer / HMR | 文件变更队列、WebSocket 入站消息和快照轮询缺少生产级上限。 |
| P1-06 | Important | DevServer / Security | DevServer/HMR/proxy 没有认证或 Origin 约束，只能保留在本机/受控内网边界。 |
| P1-07 | Important | Build / Cache | 大于等于 4KB 的 hashable 静态资源不加内容 hash，存在长期缓存陈旧风险。 |
| P2-01 | Medium | Composition | `Program.cs` 启动路径偏 eager，且 JoltService 与 LSP 组合存在 Roslyn 投影服务重复实例。 |
| P2-02 | Medium | CLI | Dev/preview 参数校验不一致，非法端口等输入会静默 fallback。 |
| P2-03 | Medium | Workspace / Performance | 静态 workspace cache 有容量上限但无 TTL，依赖通知失效会带来陈旧扫描结果。 |
| P2-04 | Medium | Extension | 扩展“沙箱”是策略检查，不是 OS 级沙箱；不能承载不可信扩展。 |
| P2-05 | Medium | Deno | Deno worker 默认 `--allow-env --allow-read` 权限较宽，应限制到需要的路径和变量。 |
| P2-06 | Medium | Debug | DAP/CDP 请求和 WebSocket 消息缺少默认上限，调试链路卡死或超大消息时不够健壮。 |

## 6. 发现详情

### P1-01 LSP 状态变更通知可能被丢弃

证据：`src/Jolt/Lsp/StdioLspServer.cs:59` 创建有界队列，`src/Jolt/Lsp/StdioLspServer.cs:96` 只对带 id 的 request 做 `_requestAdmissionGate`，`src/Jolt/Lsp/StdioLspServer.cs:109` 使用 `TryWrite`，`src/Jolt/Lsp/StdioLspServer.cs:121` 对 notification 只记录 `lspNotificationDropped`。

问题：`textDocument/didChange`、`textDocument/didClose`、`workspace/didChangeWorkspaceFolders` 这类 notification 是 workspace 状态转换，不是可丢日志事件。一旦队列满时丢弃 `didChange`，后续 completion/diagnostic/definition 会建立在旧文本、旧投影或旧 workspace folder 上，可能表现为幽灵诊断、跳转错误、HMR 错误或 close 后仍持有文档。

建议：把协议消息分成控制/状态变更/普通请求三类。状态变更通知应通过不可丢通道或受控背压路径处理，至少对 `didOpen`、`didChange`、`didClose`、workspace folder 变更保证有序处理。普通请求仍可返回 server busy。需要增加“队列满时 didChange 不丢且后续 hover 使用新文本”的回归测试。

### P1-02 未知 `$/cancelRequest` id 会无界累计

证据：`src/Jolt/Lsp/StdioLspServer.cs:471` 在 active request 不存在时把 request key 加入 `_pendingCancellationRequests`，`src/Jolt/Lsp/StdioLspServer.cs:227` 只在未来同 id request 开始执行时移除，`src/Jolt/Lsp/StdioLspServer.cs:482` 只在 shutdown 清空。

问题：LSP 客户端可以发送任意数量的未知 cancel id。当前集合没有大小上限、TTL、LRU 或拒绝策略。正常客户端影响不大，但损坏插件、测试工具或恶意本地客户端可以让长期运行的 LSP 进程内存增长。

建议：只记录已入队但未激活的 request id；未知 id 可直接忽略。若保留“未来 cancel”语义，应设置小容量 LRU 和短 TTL，并记录一次性 warning。需要增加“随机未知 cancel storm 不增长超过上限”的测试。

### P1-03 LSP 磁盘 fallback 会污染 open document store

证据：`src/Jolt/Lsp/LspSession.ProviderIsolationAndRouting.cs:341` 的 `GetRequiredDocumentAsync` 找不到 document 时从磁盘读取，`src/Jolt/Lsp/LspSession.ProviderIsolationAndRouting.cs:365` 调用 `File.ReadAllTextAsync`，随后 `src/Jolt/Lsp/LspSession.ProviderIsolationAndRouting.cs:367` 调用 `_workspaceStore.UpsertDocumentAsync`。`src/Jolt/Workspace/InMemoryWorkspaceStore.cs:40` 的 `GetOpenDocumentsAsync` 返回 `_documents.Values`，因此 fallback 读入的文档会成为“打开文档”。

问题：`textDocument/*` 请求理论上应针对客户端已打开或明确同步的文档。当前 fallback 把未打开磁盘文件永久写入 open document store，可能扩大诊断和 workspace bridge 的输入集合，增加内存占用，也可能让“打开文档”语义失真。若磁盘文件之后被外部修改，store 中的 snapshot 还可能继续陈旧。

建议：把磁盘 fallback 改为只读 snapshot，不写入 open document store；或在 store 层区分 `OpenDocument` 与 `DiskSnapshot`。需要使 `GetOpenDocumentsAsync` 只返回真实 didOpen/upsert 的打开文档，并增加 unopened file request 不污染 open document 列表的测试。

### P1-04 Deno worker 请求无默认超时和 circuit breaker

证据：`src/Jolt/Volar/Deno/Hosting/DenoVolarHost.cs:367` 的 `SendAsync` 最终在 `src/Jolt/Volar/Deno/Hosting/DenoVolarHost.cs:382` 调用 `_workerProcess.SendRequestAsync`。`src/Jolt/Volar/Deno/Hosting/DenoWorkerProcess.cs:195` 只用调用方 `CancellationToken` 等待响应。当前没有每请求默认 timeout，也没有连续超时熔断或重启策略。

问题：Volar lane 的 completion、hover、diagnostic、rename 等都依赖 Deno worker。worker 进程未退出但停止响应时，LSP request 可能一直挂住；Build lane 中复用 Deno worker 的场景也可能挂起。扩展 provider 已经有 timeout 思路，Deno worker 作为外部进程也应采用同级保护。

建议：给 `DenoVolarHostOptions` 增加默认 per-request timeout，例如 10 到 30 秒，并允许 CLI/config 覆盖。连续超时后标记 worker unhealthy，取消 pending request，尝试重启或降级到空结果，并输出结构化诊断。需要增加“worker 接收请求但不响应时请求按时失败且 pending response 清理”的测试。

### P1-05 DevServer/HMR 资源缺少生产级上限

证据：`src/Jolt/DevServer/DevHttpServer.cs:25` 使用 `Channel.CreateUnbounded<IReadOnlyList<string>>()` 保存文件变化。`src/Jolt/DevServer/DevServerReloadHub.cs:240` 每条 WebSocket 入站消息使用无界 `MemoryStream`，`src/Jolt/DevServer/DevServerReloadHub.cs:251` 持续写入直到 `EndOfMessage`。`src/Jolt/DevServer/DevServerOptions.cs:19` 默认每秒轮询，`src/Jolt/DevServer/DevServerFileSnapshotPoller.cs:128` 每轮全量 capture，`src/Jolt/DevServer/DevServerFileSnapshotPoller.cs:165` 递归枚举文件。忽略目录只有 `node_modules`、`.git`、`bin`、`obj`、`.vs`、`.deno`，见 `src/Jolt/DevServer/DevServerFileWatchFilter.cs:18` 到 `src/Jolt/DevServer/DevServerFileWatchFilter.cs:25`。

问题：本机小项目可接受，但在大型 monorepo、生成目录、HMR 客户端异常、文件风暴或受控内网多人联调时，内存、CPU 和队列延迟都可能不可控。特别是 `.jazor`、`dist`、`.artifacts`、`.dotnet`、测试输出目录和自定义 outDir 未统一排除时，轮询可能扫描大量无关产物。

建议：文件变化通道改为有界队列并合并/coalesce 路径；WebSocket 入站消息设置最大字节数，例如 64KB，超过立即 close；快照轮询支持动态排除 outDir、`.jazor` 缓存、`.artifacts`、`.dotnet`、coverage/test-results 等目录；大型工作区默认降低轮询频率或只在 FileSystemWatcher 不可靠时启用补偿轮询。需要增加文件风暴和超大 HMR 消息测试。

### P1-06 DevServer/HMR/proxy 不能脱离本机/受控内网边界

证据：`src/Jolt/DevServer/DevServerOptions.cs:9` 默认 host 是 localhost，但 `src/Jolt/DevServer/DevServerOptionsParser.cs:42` 允许 `--dev-host` 任意设置；`src/Jolt/DevServer/DevHttpServer.cs:77` 直接绑定该 host；`src/Jolt/DevServer/DevHttpServer.cs:87` 到 `src/Jolt/DevServer/DevHttpServer.cs:98` 接受 HMR WebSocket，没有 token 或 Origin 检查；`src/Jolt/DevServer/DevHttpServer.cs:113` 在应用路由前尝试 proxy。

问题：这在本机开发服务器里常见，但不是公网服务标准。若用户把 host 设为 `0.0.0.0`，HMR 连接和代理入口会对局域网开放。proxy 可访问用户配置的后端，HMR 可建立长连接并消耗资源，源图和编译错误也可能暴露项目内部结构。

建议：文档和 CLI warning 明确 `--dev-host` 非 localhost 时的风险。若要支持团队内网联调，增加随机 dev token、Origin/Host allow-list、proxy prefix allow-list、连接数上限和请求大小限制。公网部署仍应要求使用 `--build` 输出后的静态站点，不应直接暴露 `--dev` 或 `--preview`。

### P1-07 大静态资源不加内容 hash

证据：`src/Jolt/Build/StaticAssetHandler.cs:18` 定义 `HashSizeThreshold = 4 * 1024`，`src/Jolt/Build/StaticAssetHandler.cs:80` 和 `src/Jolt/Build/StaticAssetHandler.cs:182` 只在 hashable 文件小于阈值时 hash。测试 `src/Jolt.Test/JoltStaticAssetHandlerTests.cs:54` 明确锁定了“大于等于阈值不 hash”的行为。

问题：`.png`、`.jpg`、`.woff2`、`.mp4` 等生产静态资源通常远大于 4KB。大文件更需要长期缓存和内容寻址。当前策略会让这些资源保留稳定文件名，例如 `logo.png` 或 `font.woff2`，CDN 或浏览器长期缓存后会产生陈旧资源。

建议：默认对所有 hashable 类型做内容 hash。若担心大文件 hash 成本，可使用流式 hash、并发限制和 build cache，而不是跳过 hash。兼容方案是新增 `assetHashMaxBytes` 配置，默认无限制，并把现有“不 hash 大文件”测试改为显式配置下的行为。

### P2-01 启动组合偏 eager，且 Roslyn 投影服务重复实例

证据：`src/Jolt/Program.cs:242` 创建共享 `InProcRoslynCodeService` 给 LSP/projection 使用，`src/Jolt/Services/JoltService.cs:36` 又私有创建一个 `InProcRoslynCodeService`。`src/Jolt/Program.cs:244` 创建 Deno host，`src/Jolt/Program.cs:260` 调用 `entry.RunAsync`，而 `src/Jolt/Services/JoltService.cs:71` 会启动 Deno host。`--inspect-razor-toolset` 和 `--probe-inproc-razor` 的处理位于 `src/Jolt/Program.cs:263` 之后。

问题：同一进程内存在两套 Roslyn/projection 服务，容易出现缓存、toolset、投影行为不一致，也增加内存和启动成本。诊断型命令在输出前启动 host/Deno，会让本应轻量的 probe 受到 Deno 启动失败或慢启动影响。

建议：把 `InProcRoslynCodeService` 通过构造函数注入 `JoltService`，让 Volar context 和 LSP projection 共用同一实例。把 inspect/probe 这类只读命令提前到 Deno/JoltService 启动前。`Program.cs` 可继续拆成 mode-specific bootstrap，降低入口复杂度。

### P2-02 CLI 参数校验不一致

证据：Build 参数解析在 `src/Jolt/Build/BuildCommandOptionsResolver.cs:148` 对非法整数 fail-fast；Dev 参数中 `src/Jolt/DevServer/DevServerOptionsParser.cs:35` 到 `src/Jolt/DevServer/DevServerOptionsParser.cs:39` 对非法 `--dev-port=abc` 静默忽略；Preview 中 `src/Jolt/Program.cs:120` 到 `src/Jolt/Program.cs:125` 对非法端口回退到 4173。

问题：生产工具链应该让用户输入错误尽早失败。静默 fallback 会导致脚本、CI 或 IDE 集成以为使用了指定端口，实际却监听默认端口，排查成本高，也可能和其他服务冲突。

建议：Dev/preview 与 Build 采用同一套 option resolver，非法端口、非法 host、非法 proxy/alias 都应输出明确错误并返回非 0 exit code。配置文件里的非法项也应记录错误，而不是静默跳过。

### P2-03 Workspace 静态 cache 依赖通知失效，无 TTL

证据：`src/Jolt/Workspace/JoltWorkspaceResolver.cs:16` 到 `src/Jolt/Workspace/JoltWorkspaceResolver.cs:21` 定义进程级静态 cache，`src/Jolt/Workspace/JoltWorkspaceResolver.cs:891` 到 `src/Jolt/Workspace/JoltWorkspaceResolver.cs:895` 在 miss 时扫描并缓存，`src/Jolt/Workspace/JoltWorkspaceResolver.cs:1491` 到 `src/Jolt/Workspace/JoltWorkspaceResolver.cs:1519` 只有容量裁剪。`src/Jolt/Workspace/JoltWorkspaceResolver.cs:66` 提供按路径失效，但依赖调用者在正确事件发生时触发。

问题：缓存上限是好事，但没有 TTL 或目录 version 检查。若文件变更未经过 LSP notification、DevServer watcher 或显式 invalidation，workspace file cache 和 solution project root cache 可能长期陈旧。大 workspace 中，这类静态 cache 还会让不同测试/工具场景共享状态，虽然现有测试拓扑已在清理上做了约束。

建议：保留容量上限，同时加入短 TTL 或基于 `.slnx`/project file last-write 的轻量 version key。测试中继续强制清理 cache；生产中对关键 miss/invalidated scan 加结构化指标，便于发现扫描压力。

### P2-04 Extension 沙箱是策略检查，不是 OS 级沙箱

证据：`src/Jolt/Extensions/ExtensionSecurityPolicy.cs:9` 注释明确 process isolation 是独立 worker 加请求面检查，不是 OS sandbox。`src/Jolt/Extensions/ExtensionLoader.cs:986` 对非 process-isolated 扩展直接 `Activator.CreateInstance`。`src/Jolt/Extensions/ExtensionHostOptions.cs:33` 的 `RequireProcessIsolation` 默认 false。

问题：manifest 签名、hash、provider permission、IO/network capability 都是必要防线，但它们不能阻止扩展代码在进程内或子进程内直接调用 .NET/OS API。当前默认更适合可信团队扩展，不适合加载第三方不可信 marketplace 扩展。

建议：文档将扩展系统标为 trusted extension model。若未来要支持不可信扩展，默认应要求 out-of-process，结合 OS 级 AppContainer/Job Object/ACL、受限工作目录、网络策略和 brokered IO。当前可先考虑把高风险环境中的 `RequireProcessIsolation` 默认设为 true。

### P2-05 Deno worker 默认权限较宽

证据：`src/Jolt/Volar/Deno/Hosting/DenoVolarHostOptionsParser.cs:81` 到 `src/Jolt/Volar/Deno/Hosting/DenoVolarHostOptionsParser.cs:92` 默认生成 `deno run --quiet --cached-only --allow-env --allow-read <workerPath>`。

问题：对 bundled trusted worker 来说可接受，但不是严格沙箱。`--allow-read` 未限制路径，worker 可读取当前用户可读的任意文件；`--allow-env` 未限制变量，可能暴露本机环境信息。

建议：尽量改成路径和变量白名单，例如 `--allow-read=<workerDir>,<workspaceRoot>`、`--allow-env=NO_COLOR,JOLT_*`。若 Deno 版本或依赖解析需要更宽权限，应在文档中明确原因，并对 `JOLT_DENO_ARGS` 覆盖做安全提示。

### P2-06 Debug/DAP 链路缺少默认超时和消息上限

证据：`src/Jolt/Debug/CdpClient.cs:181` 到 `src/Jolt/Debug/CdpClient.cs:216` 创建 pending request 后等待 `completion.Task`，仅依赖外部 cancellation token。`src/Jolt/Debug/CdpConnection.cs:54` 到 `src/Jolt/Debug/CdpConnection.cs:96` 用 `StringBuilder` 拼接 WebSocket 消息，没有最大消息长度。`src/Jolt/Debug/DapSession.cs:16` 到 `src/Jolt/Debug/DapSession.cs:17` 持有断点和 variable reference 字典，没有明显会话内上限。

问题：Debug 主要用于本机 IDE，风险低于 LSP/Build 主链路。但如果浏览器/CDP endpoint 停止响应、发送异常大消息，或用户长时间展开大量对象，DAP 会话可能卡住或内存增长。

建议：给 CDP command 增加默认 timeout，给 WebSocket message 设置最大大小，给 variable reference 做 per-session 上限和 pause/resume 时清理策略。现有 `src/Jolt.Test/JoltDebugCdpClientTests.cs:245` 覆盖连接关闭时 pending request 失败，但缺少“endpoint 不返回时超时”的测试。

## 7. 测试覆盖评估

已有测试覆盖相当充分，尤其是 Build、LSP cancel、DevServer reload hub、Deno worker 并发、Debug CDP parse/close、Extension security 等方向。状态文档记录 `src/Jolt.Test` 748/748 通过，compiler 回归 2391/2391 通过。

本轮未运行测试，因此不能声明当前工作区实时通过。以下是建议新增或调整的回归测试：

| 风险 | 建议测试 |
| --- | --- |
| P1-01 | 构造 request queue 满载，同时发送多次 `textDocument/didChange`，验证后续 hover/completion 看到最后一次文本。 |
| P1-02 | 发送大量随机未知 `$/cancelRequest`，验证 `_pendingCancellationRequests` 有上限或不增长。 |
| P1-03 | 对未打开但存在磁盘文件发送 textDocument request，验证 `GetOpenDocumentsAsync` 不包含该文件。 |
| P1-04 | Deno worker 接收请求后永不返回，验证请求按配置 timeout 失败，pending response 清理，后续请求可恢复或明确降级。 |
| P1-05 | 超大 HMR WebSocket message 被关闭；文件风暴下 file change channel 不无界增长且最终 coalesce。 |
| P1-07 | 大于 4KB 的 png/woff2 默认输出 hash 文件名；只有显式配置才跳过 hash。 |
| P2-02 | 非法 `--dev-port`、`--preview --dev-port`、非法 proxy/alias 配置返回非 0 并输出明确错误。 |
| P2-06 | CDP command 超时、超大 CDP message、variable reference 上限。 |

## 8. 建议修复优先级

P0：当前边界下暂无。

P1：先修 LSP notification 不可丢和 unknown cancel 上限，因为它们直接影响编辑器长期运行正确性；再修 Deno worker timeout，因为它直接影响 LSP/Build 可用性；随后修 DevServer 资源上限和大静态资源 hash，因为它们影响大型项目和发布缓存质量。

P2：统一 CLI 参数解析、拆分 Program bootstrap、注入共享 Roslyn service、为 workspace cache 增加 TTL/version key、收紧 Deno 权限、把扩展安全边界写入用户文档。

## 9. 最终判断

Jolt 的总体方向和核心架构是可持续的，特别是 Build 路径信任、`.slnx` 作用域、SourceMap/CSS 产物链和多 lane LSP 都有生产化意识。当前主要短板不是“功能不存在”，而是几个长时运行系统常见的生产硬化点：状态事件不能丢，外部进程不能无限等，队列和消息不能无界，内容缓存必须可验证。

如果按本报告处理 P1 项，Jolt 可以更稳妥地作为可信本机/CI 工具链进入生产使用。若目标升级为公网、多租户或不可信插件平台，需要另开安全架构设计，不应只在现有 DevServer/Extension/Deno worker 上小修小补。
