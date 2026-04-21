# Jolt — 全功能开发模式

> 对应源码：`src/Jolt/`

## 为什么需要

RazorVue 的库模式适合编译时场景，但应用开发需要更多：实时的编辑器智能提示、文件保存后即刻看到效果的 HMR、在 IDE 中打断点调试 .jazor 源码、生产构建输出优化后的静态资源。Jolt 是一个**类似 Vite 的大满贯开发平台**，提供从编写到调试到构建的完整闭环。

## 解决什么问题

1. **开发服务器 + HMR**：`dotnet run -- --dev` 启动 HTTP 开发服务器，文件变更后浏览器 < 500ms 内看到更新
2. **LSP 全语义**：3-Lane 架构（Jazor + Roslyn + Volar）提供 24 种 LSP 方法的完整智能提示
3. **源码级调试**：DAP + CDP 双协议，在 IDE 中对 .jazor 源码设置断点
4. **生产构建**：`dotnet run -- --build` 输出优化后的静态资源（CSS 提取、Source Map、Manifest）
5. **扩展系统**：11 种 Provider 接口，支持内置和外部扩展

## 大致实现思路

### 核心区别：支持 .jazor 和 .vue SFC

Jolt 的核心定位是同时支持两种文件格式：

- **.jazor**：Jazor 自己的单文件组件格式，包含模板 + C# 代码后置
- **.vue SFC**：标准 Vue 单文件组件，通过 Deno Volar 提供语义支持

### 架构概览

```
CLI (Program.cs)
  │
  ├── Jazor Core (Jazor/)
  │     JazorVueParser → JazorVueDocument
  │     JazorVueCompiler → JazorVueCompilationResult (.vue SFC + source maps)
  │     JazorVueExternalDeclarationEmitter (.cs externals)
  │
  ├── LSP Layer (Lsp/) — 3-Lane 架构
  │     ├── JazorLaneService  (.jazor 模板智能)
  │     ├── RoslynLaneService (进程内 C# 全语义)
  │     └── VolarLaneService  (Deno Volar for .vue/.ts/.js)
  │
  ├── DevServer Pipeline (DevServer/)
  │     HTTP + WebSocket HMR + 文件监视 + 按需编译
  │
  ├── Build Pipeline (Build/)
  │     BuildOrchestrator → CSS 提取 → Source Map → Manifest
  │
  ├── Debug Pipeline (Debug/)
  │     DAP Server + CDP Client → 断点/调用栈/变量映射
  │
  └── Extension System (Extensions/)
        ExtensionLoader → 4 内置扩展 + 外部扩展代理
```

### 3-Lane LSP 路由

请求按文档类型自动路由到最合适的 Lane：

| 文件类型 | 路由目标 | 能力 |
|---------|---------|------|
| `.jazor` 模板区 | Jazor Lane | 指令补全、结构诊断 |
| `.jazor` 代码区 | Roslyn Lane | 全 C# 语义（Completion、Hover、Call Hierarchy） |
| `.vue` / `.ts` / `.js` | Volar Lane | Vue/TS 语义（通过 Deno Worker） |
| 跨 Lane 结果 | LspResultAggregator | 合并多 Lane 返回 |

### 运行模式

| 模式 | 用途 |
|------|------|
| `--dev` | 开发服务器 + HMR + LSP |
| `--language-server` | 纯 LSP 模式（供 IDE 集成） |
| `--build` | 生产构建 |

### 与 RazorVue 的对比

| 维度 | Jolt（全功能模式） | RazorVue（库模式） |
|------|---------------------|-------------------|
| 触发方式 | 独立进程 | Source Generator |
| 输出格式 | .vue SFC + JS/CSS | 纯 JS/TS 模块 |
| 文件支持 | .jazor + .vue SFC | .razor |
| 热更新 | HMR（< 500ms） | 无 |
| 调试 | DAP + CDP 源码级 | 无 |
| LSP | 24 种方法全覆盖 | 仅 Roslyn 分析 |

## 功能设计文档索引

以下是按子系统组织的细粒度设计文档，均从代码实现反推编写：

### 整体架构
| 文档 | 覆盖范围 |
|------|---------|
| [architecture.md](architecture.md) | 整体架构总览、7 种运行模式、子系统全景图、模式组合矩阵 |

### 协议层 (`protocol/`)
| 文档 | 覆盖范围 |
|------|---------|
| [Contracts.md](protocol/Contracts.md) | 协议契约类型：DocumentSnapshot, Descriptors, Requests, HostInfo, RpcMessages |
| [RpcTransport.md](protocol/RpcTransport.md) | RPC 传输：Processor, Dispatcher, StdioServer, Serializer, 错误码映射 |
| [Documents.md](protocol/Documents.md) | 文档原语：DocumentVersion, TextSpan, TextChange |

### 工作区 (`workspace/`)
| 文档 | 覆盖范围 |
|------|---------|
| [Store.md](workspace/Store.md) | 文档存储：IJoltWorkspaceStore, InMemoryWorkspaceStore, ChangeSink |
| [Resolver.md](workspace/Resolver.md) | 路径解析：JoltWorkspaceResolver (4 种 Vue 组件策略), JazorRelatedDocumentResolver |

### 虚拟文档 (`virtual-documents/`)
| 文档 | 覆盖范围 |
|------|---------|
| [ModelAndRegistry.md](virtual-documents/ModelAndRegistry.md) | 虚拟文档模型：VirtualDocument, VirtualDocumentIdentity, Registry |
| [ProjectionMap.md](virtual-documents/ProjectionMap.md) | 双向位置映射引擎：ProjectionSegment, ProjectionMap, 边界处理策略 |

### Source Map (`sourcemap/`)
| 文档 | 覆盖范围 |
|------|---------|
| [Service.md](sourcemap/Service.md) | Source Map 服务：VLQ 解码器、双向映射算法、评分启发式 |

### Jazor 核心 (`jazor-core/`)
| 文档 | 覆盖范围 |
|------|---------|
| [Parser.md](jazor-core/Parser.md) | Jazor Vue 解析器：JazorVueParser, JazorVueDocument, JazorMarkupPatterns |
| [Compiler.md](jazor-core/Compiler.md) | Jazor Vue 编译器：JazorVueCompiler, 成员提取, Vue 代码生成 |
| [ProjectionService.md](jazor-core/ProjectionService.md) | 投影服务：.jazor → Vue/C# 虚拟文档投影 |

### 分析服务 (`analysis/`)
| 文档 | 覆盖范围 |
|------|---------|
| [Service.md](analysis/Service.md) | 分析服务：IVueAnalysisClient, JazorVueAnalysisService, FallbackJazorAnalysisService |
| [RpcClient.md](analysis/RpcClient.md) | RPC 分析客户端：RpcVueAnalysisClient, ProcessAnalysisRpcTransport, ClientFactory |

### 前端 Deno (`frontend/`)
| 文档 | 覆盖范围 |
|------|---------|
| [DenoVolarHost.md](frontend/DenoVolarHost.md) | Deno Volar 宿主：IDenoVolarHost, 生命周期管理, 自动重启, 12 种智能方法 |
| [DenoWorkerProcess.md](frontend/DenoWorkerProcess.md) | Deno 工作进程：子进程管理, JSON-RPC, 工作区隔离, stderr pump |
| [DenoProtocol.md](frontend/DenoProtocol.md) | Deno 协议类型：编译请求/响应, 模板智能请求, FrontendContext |

### Roslyn (`roslyn/`)
| 文档 | 覆盖范围 |
|------|---------|
| [InProcService.md](roslyn/InProcService.md) | 进程内 Roslyn 服务：14 种 LSP 能力, 位置映射, 回退机制, Rename |
| [HotReloadMetadata.md](roslyn/HotReloadMetadata.md) | 热重载元数据：SHA256 签名, HMR 边界分类 (TemplateOnly/LogicSafe/FullReload) |

### Razor (`razor/`)
| 文档 | 覆盖范围 |
|------|---------|
| [SdkToolset.md](razor/SdkToolset.md) | Razor SDK 工具集：.NET SDK 发现, 多路径搜索策略, 版本比较 |
| [DesignTimeProjection.md](razor/DesignTimeProjection.md) | 设计时代码投影：RazorProjectEngine, Source mapping 提取, 3 层回退策略 |

### LSP (`lsp/`)
| 文档 | 覆盖范围 |
|------|---------|
| [ServerAndSession.md](lsp/ServerAndSession.md) | LSP 传输与会话：StdioLspServer, LspSession (30+ 方法), 请求取消 |
| [ThreeLaneArchitecture.md](lsp/ThreeLaneArchitecture.md) | 三车道架构：ILspLane, JazorLane, RoslynLane, VolarLane (1565 行) |
| [RoutingAndAggregation.md](lsp/RoutingAndAggregation.md) | 路由与聚合：LspLaneRouter, 区域分类, 投影解析, 结果去重 |
| [Coordination.md](lsp/Coordination.md) | 跨车道协调：Reference/Rename/CodeAction Coordinator, 扇出模式 |
| [MarkupBridge.md](lsp/MarkupBridge.md) | 标记桥接：MarkupComponentBridgeService (842 行), 组件标签解析, JS trivia masking |

### 开发服务器 (`devserver/`)
| 文档 | 覆盖范围 |
|------|---------|
| [HttpServer.md](devserver/HttpServer.md) | HTTP 服务器：Kestrel, 模块服务, HTML 转换, API 代理, ModuleResolver |
| [OnDemandCompiler.md](devserver/OnDemandCompiler.md) | 按需编译器：多格式编译 (.jazor/.vue/.ts/.css), Source Map 链, CSS Modules |
| [Hmr.md](devserver/Hmr.md) | 热模块替换：WebSocket 广播, 心跳机制, 背压处理, 过期客户端清理 |
| [FileWatching.md](devserver/FileWatching.md) | 文件监听：ChangeProcessor, FileChangeDebouncer, DependencyGraph, 配置解析 |

### 构建管线 (`build/`)
| 文档 | 覆盖范围 |
|------|---------|
| [Pipeline.md](build/Pipeline.md) | 构建管线总览：BuildOrchestrator, BuildOptions, 入口点解析, Manifest |
| [Incremental.md](build/Incremental.md) | 增量构建：SHA256 指纹, BuildIncrementalState, 缓存命中, 文件系统容错 |
| [CssPipeline.md](build/CssPipeline.md) | CSS 管线：CSS 提取, 分块归属, 哈希输出, 压缩, url() 重写 |
| [BundleAndAssets.md](build/BundleAndAssets.md) | 打包与资产：DenoBundleRunner, 代理服务器, Import Map, 静态资产哈希 |

### 调试 (`debug/`)
| 文档 | 覆盖范围 |
|------|---------|
| [DapServer.md](debug/DapServer.md) | DAP 服务器：DapServer, DapRequestHandler, 协议类型, LaunchConfiguration |
| [CdpClient.md](debug/CdpClient.md) | CDP 客户端：CdpClient, CdpConnection, WebSocket 传输, 脚本 URL 淘汰 |
| [SessionAndMapping.md](debug/SessionAndMapping.md) | 调试会话：DapSession 状态机, BreakpointManager, CallStackMapper, VariableMapper |

### 扩展系统 (`extension/`)
| 文档 | 覆盖范围 |
|------|---------|
| [Lifecycle.md](extension/Lifecycle.md) | 扩展生命周期：IExtension, ExtensionLoader (1210 行), ExtensionRegistry, Collectible LoadContext |
| [Security.md](extension/Security.md) | 安全沙箱：ExtensionSandboxProfile, SHA256 验证, RS256 签名, 权限模型 |
| [Worker.md](extension/Worker.md) | 进程隔离宿主：ExtensionWorkerServer (1166 行), JSON-RPC, 沙箱执行, 11 种 Provider |
| [Builtin.md](extension/Builtin.md) | 内置扩展：ComponentCodeActionExtension, WorkspaceSymbolIndex 等 4 个扩展 |

## 参考文档

- 完成度分析：`docs/03-完成/jolt/completion-analysis.md`
- 状态快照：`docs/03-完成/jolt/status.md`
- 修复清单：`docs/03-完成/jolt/jolt-fix-list.md`, `docs/03-完成/jolt/jolt-fix-list1.md`
- 实施计划：`docs/02-计划/jolt/`（Phase 1–7）
- 模块桥接索引：[modules-bridge.md](modules-bridge.md)
