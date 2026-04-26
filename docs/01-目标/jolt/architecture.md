# Jolt 整体架构

> Status: 活跃参考
> Positioning: Jolt 子系统的顶层架构视图，串联所有子模块

## 目录

- [2. 运行模式](#2-运行模式)
- [3. 子系统全景图](#3-子系统全景图)
- [4. 核心设计原则](#4-核心设计原则)
- [5. 模式组合矩阵](#5-模式组合矩阵)
- [6. 关键依赖](#6-关键依赖)
- [7. 数据流关键路径](#7-数据流关键路径)
- [8. 文件组织结构](#8-文件组织结构)
- [9. 配置与扩展点](#9-配置与扩展点)
- [10. 可观测性与监控](#10-可观测性与监控)
- [11. 性能优化策略](#11-性能优化策略)
- [12. 错误处理与容错](#12-错误处理与容错)
- [13. 安全性考虑](#13-安全性考虑)
- [14. 总结](#14-总结)

本文档为 Jolt 子系统提供完整的架构视图，解释各模块职责和交互关系。Jolt 是一个 .NET 10 控制台应用，作为 .jazor 文件的多功能开发工具链，提供 LSP 语言服务、开发服务器、生产构建、调试适配器、扩展系统等核心能力。

## 2. 运行模式

Jolt 支持 7 种 CLI 运行模式，每种模式组合不同的服务子系统：

| 模式 | CLI 标志 | 用途 | 核心服务 |
|------|---------|------|---------|
| **Extension Worker** | `--extension-worker` | 扩展工作进程隔离执行 | `ExtensionWorkerServer` |
| **Build** | `--build` | 生产构建与打包 | `BuildOrchestrator` |
| **Preview** | `--preview` | 静态文件预览服务器 | ASP.NET Core `FileServer` |
| **Analysis Stdio** | `--analysis-stdio` | 分析协议 STDIO 服务器 | `StdioVueAnalysisRpcServer` |
| **DAP** | `--dap` | Debug Adapter Protocol | `DapServer` + `DapSession` |
| **Dev Server** | `--dev` | 开发服务器（HMR） | `DevHttpServer` + `OnDemandCompiler` |
| **LSP** | `--lsp` | Language Server Protocol | `LspSession` + 三车道架构 |

**模式组合规则**：
- `--lsp` 可与 `--dev` 组合：LSP 会话 + 开发服务器（支持 HMR 协调）
- `--dap` 可与 `--dev` 组合：调试适配器 + 开发服务器（支持 Source Map 映射）
- `--lsp + --dev` 可选 CDP 连接：支持 Chrome DevTools Protocol 调试

### 2.1 当前部署边界（Windows）

当前架构上的“生产”目标需要按模式拆开理解，而不是把所有模式都当成公网服务：

- `--build`：正式构建 lane，可用于 CI、构建机和发布链路
- `--dev`：本机开发或受控内网联调，不作为公网服务
- `--preview`：本地或受控内网预览，不作为正式站点
- `--lsp`：仅供本机编辑器和工具链接入

当前推荐拓扑是：

`Jolt --build` -> `dist/` 静态产物 -> `IIS/Nginx/CDN/对象存储`

而不是：

`公网用户` -> `Jolt --dev/--preview`

这意味着 `Jolt` 当前并不以“直接面向公网”作为架构目标，也不把公网入站安全能力视为现阶段验收项。

## 3. 子系统全景图

```
┌─────────────────────────────────────────────────────────────────┐
│                         Jolt Entry Point                         │
│                      (src/Jolt/Program.cs)                       │
└───────────────────────────┬─────────────────────────────────────┘
                            │
          ┌─────────────────┼─────────────────┐
          │                 │                 │
    ┌─────▼─────┐     ┌────▼────┐     ┌─────▼─────┐
    │   Build   │     │   DAP   │     │    LSP    │
    │  Pipeline │     │  Server  │     │  Session  │
    └─────┬─────┘     └────┬────┘     └─────┬─────┘
          │                 │                 │
          │                 │                 │
    ┌─────▼─────────────────▼─────────────────▼─────┐
    │           Core Service Layer                  │
    │  (src/Jolt/Services/JoltService.cs)           │
    │  • Workspace Store                            │
    │  • Vue Analysis Client                        │
    │  • Deno Volar Host                            │
    │  • Projection Service                         │
    └─────┬─────────────────────────────────────────┘
          │
    ┌─────▼─────────────────────────────────────────┐
    │           Subsystem Foundations               │
    ├───────────────────────────────────────────────┤
    │  LSP Layer         │  DevServer    │  Build  │
    │  • LspSession      │  • DevHttp    │  • Bundle│
    │  • Lanes (3x)      │  • Compiler   │  • Minify│
    │  • Coordinators    │  • HMR        │  • Manifest│
    ├───────────────────────────────────────────────┤
    │  Projection Layer      │  Analysis Layer     │
    │  • Razor Design-Time   │  • Vue Analysis     │
    │  • Virtual Documents   │  • RPC Protocol     │
    ├───────────────────────────────────────────────┤
    │  Runtime Layer          │  Extension Layer   │
    │  • Roslyn In-Proc       │  • Load/Unload     │
    │  • Razor SDK            │  • Providers       │
    │  • Deno Host            │  • Isolation       │
    └───────────────────────────────────────────────┘
```

## 4. 核心设计原则

### 4.1 三车道 LSP 架构

Jolt LSP 采用三车道架构（Three-Lane Architecture），将语言功能按语义域分离到独立车道：

```
┌─────────────────────────────────────────────────────────┐
│                    LspSession Core                       │
│  (src/Jolt/Lsp/LspSession.cs)                           │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  Jazor Lane  │  │  Roslyn Lane │  │  Volar Lane  │ │
│  │              │  │              │  │              │ │
│  │ • .jazor     │  │ • C# code    │  │ • .vue       │ │
│  │ • Components │  │ • Projection │  │ • TS/JS      │ │
│  │ • Templates  │  │ • IntelliSense│  │ • TypeScript │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
│          │                 │                 │         │
│          └─────────────────┼─────────────────┘         │
│                            │                           │
│                    ┌───────▼────────┐                  │
│                    │ Lane Router    │                  │
│                    │ (Region-based) │                  │
│                    └───────┬────────┘                  │
└────────────────────────────┼──────────────────────────┘
                               │
                    ┌──────────▼──────────┐
                    │ Result Aggregator   │
                    │ (Merge+Deduplicate) │
                    └─────────────────────┘
```

**车道职责**：

1. **Jazor Lane** (`JazorLaneService`): .jazor 文件的语义分析
   - 模板语法高亮
   - 组件引用解析
   - Jazor 特有诊断

2. **Roslyn Lane** (`RoslynLaneService`): C# 代码的语义分析
   - 投影后的 C# 代码（.razor.g.cs / .inproc.g.cs）
   - 完整的 C# IntelliSense
   - 类型推导、定义查找、引用查找

3. **Volar Lane** (`VolarLaneService`): 前端代码的语义分析
   - .vue / .ts / .js 文件分析
   - Deno TypeScript 类型检查
   - Volar 协议桥接

**路由机制** (`LspLaneRouter`):
- 基于文档区域分类（`DocumentRegionKind`）路由请求
- Jazor 文件被分类为：`JazorMarkup`, `JazorScript`, `JazorCodeBehind`
- 每个区域路由到对应车道处理

**协调器模式**:
- `ReferenceCoordinator`: 跨车道引用查找（Jazor ↔ C# ↔ Vue）
- `RenameCoordinator`: 跨车道重命名（同步修改所有相关文档）
- `CodeActionCoordinator`: 跨车道代码动作聚合
- `MarkupBridgeFanoutCoordinator`: Markup 变更通知到所有车道

### 4.2 `.slnx` 解决方案 / 项目作用域

Jolt 可以在一个进程里服务多个解决方案，但解决方案边界只认 `.slnx`。Owning project 必须从 `.slnx` 的 project entries 解析，不应通过 `.sln`、`.csproj` 或目录邻近关系推断。

**关键约束**：
- 隐式 discovery 只在 owning project 内展开
- HMR 只传播到 owning project 的受影响集合
- 诊断刷新只重算 owning project 的相关文档
- 找不到 `.slnx` 时，项目级发现必须返回英文错误，而不是继续退回到磁盘猜测

### 4.3 文档投影系统

**投影管道** (`JazorProjectionService`):

```
.jazor 文件
    │
    ▼
┌─────────────────────────────────────────┐
│  Razor Design-Time Projection Service   │
│  (src/Jolt/Razor/InProc/...)           │
└──────────────┬──────────────────────────┘
               │
               ▼
       .razor.g.cs 或 .inproc.g.cs
               │
               ▼
┌─────────────────────────────────────────┐
│  Virtual Document Registry              │
│  (src/Jolt/VirtualDocuments/Registry/) │
└──────────────┬──────────────────────────┘
               │
               ▼
    Roslyn Lane 消费
```

**投影类型**：
- `razor-design-time`: 使用 Razor SDK 生成（标准路径）
- `fallback`: 使用 In-Proc fallback 生成（兼容模式）

**投影映射** (`ProjectionMap`):
- 记录源文档位置 ↔ 投影文档位置的映射关系
- 支持 LSP 位置转换（Source → Projection → Source）

### 4.4 Source Map 链

**Source Map 服务** (`ISourceMapService`):

```
.jazor → 编译 → .js (带 Source Map)
                    │
                    ▼
            ISourceMapService
            (InMemorySourceMapService)
                    │
                    ▼
        映射链：.jazor → .js → Runtime Position
```

**使用场景**：
- DAP 调试：.jazor 断点 → JavaScript 位置
- DevServer HMR：错误堆栈映射到源文件
- LSP 诊断：JavaScript 错误映射到 .jazor 位置

### 4.5 可卸载扩展系统

**扩展加载架构**:

```
┌─────────────────────────────────────────┐
│     ExtensionRegistry                   │
│  (src/Jolt/Extensions/...)             │
│  • LoadEventRetention                  │
│  • ProviderEventRetention              │
│  • Health Monitoring                   │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│     ExtensionLoader                    │
│  • LoadBuiltinExtensionsAsync          │
│  • LoadUserExtensionsAsync             │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│  CollectibleAssemblyLoadContext         │
│  (可卸载加载上下文)                      │
└─────────────────────────────────────────┘
```

**扩展隔离机制**：
- 每个扩展加载到独立的 `AssemblyLoadContext`
- 支持卸载（`UnloadAsync`）释放资源
- Provider 超时隔离（失败 N 次后隔离 M 秒）
- 可观测性：加载日志、Provider 调用统计

**扩展能力**（`IExtensionCapabilityDescriptor`）:
- LSP 诊断提供者
- LSP 补全提供者
- LSP Hover 提供者
- LSP 代码动作提供者
- 工作区符号提供者
- 其他自定义能力

## 5. 模式组合矩阵

| 模式 | Workspace | Projection | Deno Host | LSP Lanes | DevServer | DAP | Build |
|------|-----------|------------|-----------|-----------|-----------|-----|-------|
| `--extension-worker` | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `--build` | ❌ | ❌ | ✅ (Bundle) | ❌ | ❌ | ❌ | ✅ |
| `--preview` | ❌ | ❌ | ❌ | ❌ | ✅ (Static) | ❌ | ❌ |
| `--analysis-stdio` | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `--dap` | ❌ | ❌ | ✅ (Optional) | ❌ | ✅ (Optional) | ✅ | ❌ |
| `--dev` | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ | ❌ |
| `--lsp` | ✅ | ✅ | ✅ | ✅ (3x) | ✅ (Optional) | ❌ | ❌ |
| `--lsp --dev` | ✅ | ✅ | ✅ | ✅ (3x) | ✅ | ❌ | ❌ |

**关键组合**：
- `--lsp` 激活完整三车道架构
- `--lsp --dev` 激活 LSP + DevServer + HMR 协调
- `--dap --dev` 激活调试适配器 + Source Map 映射

## 6. 关键依赖

### 6.1 外部依赖

| 依赖 | 版本 | 用途 |
|------|------|------|
| **Roslyn** | .NET SDK 内置 | C# 编译器平台、代码分析、投影生成 |
| **Razor SDK** | .NET SDK 内置 | Razor 设计时投影、.razor.g.cs 生成 |
| **Deno** | DenoHost.Core | TypeScript 类型检查、模块捆绑、前端编译 |
| **ASP.NET Core** | .NET 10 | Kestrel HTTP 服务器（DevServer、Preview） |
| **Acornima** | (通过 Jazor.Vue) | JavaScript AST 解析（间接依赖） |
| **Jazor.Vue** | 项目引用 | .jazor 编译器、AST 转换 |

### 6.2 内部项目依赖

```
Jolt
├── Jazor.Common (契约)
│   ├── DocumentSnapshot
│   ├── JazorConfig
│   └── Protocol Contracts
├── Jazor.Vue (编译器核心)
│   ├── JazorVueParser
│   ├── JazorVueCompiler
│   └── AST Conversion
├── Jazor.SourceMaps (Source Map)
│   ├── ISourceMapService
│   └── InMemorySourceMapService
└── Jazor.VueContracts (协议)
    ├── RPC Protocol
    └── LSP Protocol
```

### 6.3 NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `DenoHost.Core` | Deno 运行时嵌入、进程管理 |
| `DenoHost.Runtime.win-x64` | Windows 平台 Deno 二进制 |
| `Microsoft.CodeAnalysis` | Roslyn 编译器 API |
| `Microsoft.CodeAnalysis.CSharp` | C# 语言服务 |
| `Microsoft.CodeAnalysis.Razor.Compiler` | Razor 编译器 |

## 7. 数据流关键路径

### 7.1 LSP 请求处理流

```
Client Request
    │
    ▼
LspSession.HandleRequestAsync
    │
    ▼
LspLaneRouter (基于 DocumentRegionKind)
    │
    ├──► JazorLaneService (Jazor 区域)
    ├──► RoslynLaneService (C# 区域)
    └──► VolarLaneService (Vue/TS 区域)
    │
    ▼
LspResultAggregator (聚合结果)
    │
    ▼
Client Response
```

### 7.2 DevServer 编译流

```
浏览器请求 .jazor
    │
    ▼
DevHttpServer.OnDemandCompiler
    │
    ▼
JazorVueParser → JazorVueCompiler
    │
    ▼
DenoFrontendModuleCompiler (TS → JS)
    │
    ▼
SourceMapService (记录映射)
    │
    ▼
返回 JavaScript + SourceMap
```

### 7.3 Build 构建流

```
BuildOrchestrator.BuildAsync
    │
    ▼
OnDemandCompiler (编译所有 .jazor)
    │
    ▼
DenoBundleRunner (Deno 打包)
    │
    ├──► Chunks (.js)
    ├──► CSS Assets (.css)
    └── Source Maps (.js.map)
    │
    ▼
StaticAssetHandler (复制 public/)
    │
    ▼
HtmlTransformer (生成 index.html)
    │
    ▼
BuildManifest (写入清单)
    │
    ▼
dist/ 目录
```

## 8. 文件组织结构

```
src/Jolt/
├── Program.cs                          # 入口点、模式选择
├── Services/
│   └── JoltService.cs                  # 核心服务组合根
├── Lsp/
│   ├── LspSession.cs                   # LSP 会话核心
│   ├── Lanes/                          # 三车道实现
│   │   ├── JazorLaneService.cs
│   │   ├── RoslynLaneService.cs
│   │   └── VolarLaneService.cs
│   ├── Routing/                        # 路由与分类
│   │   ├── LspLaneRouter.cs
│   │   └── DocumentRegionKind.cs
│   ├── Coordination/                   # 跨车道协调器
│   │   ├── ReferenceCoordinator.cs
│   │   ├── RenameCoordinator.cs
│   │   └── CodeActionCoordinator.cs
│   └── Aggregation/
│       └── LspResultAggregator.cs      # 结果聚合器
├── Build/
│   └── BuildOrchestrator.cs            # 构建编排器
├── DevServer/
│   ├── DevHttpServer.cs                # 开发 HTTP 服务器
│   ├── OnDemandCompiler.cs             # 按需编译器
│   └── HtmlTransformer.cs              # HTML 转换器
├── Debug/
│   ├── DapServer.cs                    # DAP 服务器
│   └── DapRequestHandler.cs            # DAP 请求处理
├── Extensions/
│   ├── ExtensionLoader.cs              # 扩展加载器
│   ├── ExtensionRegistry.cs            # 扩展注册表
│   └── CollectibleExtensionLoadContext.cs
├── Jazor/Projection/
│   └── JazorProjectionService.cs       # Jazor 投影服务
├── Razor/InProc/
│   └── RazorDesignTimeCodeProjectionService.cs
├── Roslyn/InProc/
│   └── InProcRoslynCodeService.cs      # Roslyn 内存中服务
├── Frontend/Deno/Hosting/
│   └── DenoVolarHost.cs                # Deno Volar 主机
├── Analysis/
│   └── VueAnalysisRpcProcessor.cs      # Vue 分析 RPC
├── Rpc/
│   └── JoltRpcProcessor.cs             # Jolt RPC 协议
├── Workspace/
│   └── InMemoryWorkspaceStore.cs       # 工作区存储
└── VirtualDocuments/
    └── Registry/                       # 虚拟文档注册表
```

## 9. 配置与扩展点

### 9.1 配置文件

- **jolt.config.json**: 项目配置
  - `dev.root`: 开发服务器根目录
  - `dev.port`: 开发服务器端口
  - `resolve.aliases`: 模块解析别名
  - `extension.root`: 扩展根目录
  - `extension.loadEventRetention`: 加载事件保留时间
  - `extension.providerEventRetention`: Provider 事件保留时间

### 9.2 扩展点

**LSP 扩展点**（通过 `IExtension` 接口）:
- `LspDiagnosticProvider`: 提供自定义诊断
- `LspCompletionProvider`: 提供自定义补全
- `LspHoverProvider`: 提供 Hover 信息
- `LspCodeActionProvider`: 提供代码动作
- `LspReferenceProvider`: 提供引用查找
- `LspRenameProvider`: 提供重命名
- `LspDocumentSymbolProvider`: 提供文档符号
- `LspWorkspaceSymbolProvider`: 提供工作区符号
- `LspFoldingRangeProvider`: 提供折叠范围
- `LspSignatureHelpProvider`: 提供签名帮助
- `LspInlayHintProvider`: 提供内联提示

**扩展加载**:
- 内置扩展：`BuiltinExtensionCatalog.Create()`
- 用户扩展：扫描 `extension.root` 目录
- 清单文件：`jazor-extension.json`

## 10. 可观测性与监控

### 10.1 日志输出

Jolt 通过 `stderr` 输出结构化 JSON 日志：

```json
{
  "eventType": "extensionLoad",
  "timestamp": "2026-04-21T10:30:00Z",
  "source": "builtin",
  "extensionId": "workspace-symbol",
  "status": "loaded"
}
```

**事件类型**:
- `extensionLoad`: 扩展加载事件
- `extensionProvider`: Provider 调用事件
- `lspWorkspaceDocumentChangeSinkFailed`: 工作区变更通知失败

### 10.2 健康检查端点

**LSP 自定义方法**:
- `jazor/extensionProviderHealth`: Provider 健康状态
- `jazor/extensionLoadHealth`: 扩展加载状态
- `jazor/extensionObservabilityDashboard`: 可观测性仪表板

**健康指标**:
- Provider 调用次数、成功/失败率
- Provider 超时次数、隔离状态
- 扩展加载次数、失败原因
- 平均调用延迟、P99 延迟

## 11. 性能优化策略

### 11.1 增量构建

**BuildOrchestrator 增量模式** (`BuildOptions.Incremental`):
- 计算输入文件指纹哈希
- 缓存构建状态（`jazor-build-state.json`）
- 指纹未变时复用输出
- 支持 HTML 快速刷新（仅更新资源引用）

### 11.2 按需编译

**DevServer 按需编译** (`OnDemandCompiler`):
- 懒加载：仅编译浏览器请求的模块
- 编译缓存：`CompilationCache` 缓存编译结果
- 依赖图：`DependencyGraph` 跟踪模块依赖
- Source Map 复用：避免重复生成

### 11.3 LSP 投影缓存

**投影服务缓存**:
- Razor SDK 投影结果缓存
- 虚拟文档注册表（`InMemoryVirtualDocumentRegistry`）
- 文档版本跟踪（`DocumentSnapshot.Version`）

### 11.4 扩展隔离

**Provider 隔离机制**:
- 超时保护：`extensionProviderTimeout`（默认 2s）
- 失败隔离：连续失败 N 次后隔离 M 秒（可配置）
- 跳过执行：隔离期间直接跳过，不阻塞主流程

## 12. 错误处理与容错

### 12.1 LSP 诊断聚合

**LspResultAggregator**:
- 合并多车道诊断结果
- 去重重复诊断
- 严重程度排序（Error > Warning > Info）

### 12.2 投影失败回退

**Razor 投影回退** (`InProcRoslynCodeService`):
- Razor SDK 失败 → 使用 In-Proc fallback
- 生成 `.inproc.g.cs` 作为备选投影
- 确保基础 IntelliSense 可用

### 12.3 分析客户端回退

**Vue Analysis 回退** (`FallbackJazorAnalysisService`):
- 外部分析客户端不可用 → 使用内置回退
- `AnalyzeWithFallbackAsync` 自动降级
- 确保基本分析功能可用

### 12.4 CDP 连接失败处理

**DAP CDP 连接** (`TryCreateCdpClientAsync`):
- CDP 端点无效 → 启动无 CDP 模式
- 连接失败 → 记录错误并继续
- 断线处理：DAP Session 保持活跃

## 13. 安全性考虑

### 13.1 扩展沙箱

**扩展隔离** (`CollectibleAssemblyLoadContext`):
- 每个扩展独立加载上下文
- 卸载时释放所有资源
- 防止扩展间干扰

### 13.2 路径安全

**路径规范化** (`JoltWorkspaceResolver.NormalizePath`):
- 统一路径分隔符
- 防止路径遍历攻击
- 大小写不敏感（Windows）

### 13.3 进程隔离

**Extension Worker 模式**:
- 扩展在独立进程运行
- STDIO 通信隔离
- 崩溃不影响主进程

## 14. 总结

Jolt 架构核心特点：

1. **三车道 LSP 架构**：Jazor、Roslyn、Volar 车道协同工作
2. **文档投影系统**：.jazor → C# 投影 → Roslyn 分析
3. **Source Map 链**：编译产物到源文件的完整映射
4. **可卸载扩展系统**：动态加载/卸载，Provider 隔离
5. **多模式组合**：LSP、DevServer、DAP、Build 灵活组合
6. **增量优化**：构建缓存、按需编译、投影缓存
7. **容错设计**：多层回退机制、诊断聚合

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
