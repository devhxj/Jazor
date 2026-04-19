# Jazor.VueHost 能力设计

## 概述

Jazor.VueHost 是 Jazor 项目的统一前端开发宿主，提供完整的开发时能力。

**核心定位**：

```
VueHost = 全前端 LSP + 开发服务器 + 编译管道 + Source Map + 调试支持
```

类似 Vite，在支持`.vue`/`.ts`/`.js`/`.css`/`.html`的同时扩展了对`.jazor`/`.cs`的支持，对`.vue`/`.ts`/`.js`/`.jazor`/`.cs`提供完整的智能感知。

---

## 一、设计原则

### 1.1 Razor-first

- `.jazor` 就是 `.razor`，作者写的是标准 Razor 语法
- `.jazor` 不引入 template 这类新 DSL
- `.jazor` 根据需要会扩展一批专用指令
- `.jazor` 的源码语义首先是 Razor，不是 Vue 模板语言

### 1.2 单一宿主边界

- VueHost 是唯一宿主边界，不拆分项目
- 前端/运行时统一是 Deno，不要 Vite

### 1.3 阶段分离

智能感知和编译打包是两个阶段，不可混在一起设计：

| 阶段 | 特点 |
|------|------|
| **智能感知** | 不需要先生成 `.vue` 或 `.cs`，直接基于源码 |
| **编译打包** | 才涉及投影/转译成内部需要的 `.vue`、桥接代码、产物 |

### 1.4 复用LSP

- `.jazor` / `.cs` 的设计时语义必须复用 Razor LSP、Roslyn 的原生能力
- `.vue` / `.ts` / `.js` / `.css` / `.html` 的设计时语义必须复用 Volar、TSServer、Deno 的原生能力
- VueHost 不重新发明这些语言服务，只负责桥接、路由、聚合、映射
- 智能感知不依赖最终生成的 `.g.cs`，也不依赖 `.jazor -> .g.vue`

### 1.5 能提供就提供

- 所有前端文件类型，能提供 LSP 能力就提供
- 不局限于 `.jazor`
- 单一入口，IDE 只需连接 VueHost

**边界条件**：

- **不能提供时静默降级**，不报错、不阻塞。例如 Deno 不可用时 `.ts/.vue` 补全消失，但 `.jazor` 的 C# 补全不受影响
- **优先级排序**：`.jazor/.cs/.vue/.ts/.js` > `.css/.html` > `.json`。精力有限时先保证靠前的文件类型
- **最低质量标准**：能力矩阵中标注的能力必须达到"结果正确且不误导用户"的阈值。不确定时宁可不给，不给虚假补全
- **分阶段交付**：并非所有能力一次性到位，能力矩阵以 P1/P2/P3 标注交付阶段

### 1.6 内部投影

- 构建期的 `.vue`、虚拟代码文档只是内部物化产物
- 作者面对的是 `.jazor`，IDE 看到的也是 `.jazor`
- `.jazor` 设计时智能感知只允许生成 Roslyn 所需的最小代码投影，以及喂给 Volar 的桥接元数据
- `.jazor` 设计时智能感知不以虚拟 `.vue` 文本作为 Volar 的主输入

---

## 二、整体架构

### 2.1 进程模型

VueHost 是一个单一的 .NET 进程，内部承载所有子系统。VolarLane 通过子进程中的 Deno Worker（Volar + TSServer）提供前端语言能力。

```
┌─────────────────────────────────────────────────────────────────────┐
│                     VueHost (.NET 进程)                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                      Lane 层                                │   │
│  │                                                             │   │
│  │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐        │   │
│  │  │  JazorLane   │ │ RoslynLane   │ │ VolarLane    │        │   │
│  │  │              │ │              │ │              │        │   │
│  │  │ .jazor 解析  │ │ C# 语义      │ │ Volar+TS     │        │   │
│  │  │ 元数据协调   │ │ @code 补全   │ │ Vue/TS/JS    │        │   │
│  │  │ 符号协调     │ │ C# 诊断      │ │ CSS/HTML     │        │   │
│  │  │ Lane 路由    │ │ 导航/重命名  │ │ 组件解析     │        │   │
│  │  └──────┬───────┘ └──────┬───────┘ └──────┬───────┘        │   │
│  │         │                │                │                 │   │
│  │         └──────── 聚合 ──┴────────────────┘                 │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐          │
│  │  LSP 服务     │  │  开发服务器    │  │  编译管道     │          │
│  │               │  │               │  │               │          │
│  │ Lane 路由     │  │ 文件服务      │  │ .jazor 转译   │          │
│  │ 结果聚合      │  │ HMR           │  │ .ts 转译      │          │
│  │ 协议适配      │  │ 模块解析      │  │ .vue 编译     │          │
│  └───────────────┘  └───────────────┘  └───────────────┘          │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │              映射系统（双层）                                  │  │
│  │                                                               │  │
│  │  ProjectionMap（设计时）: .jazor ↔ 设计时目标 段级映射      │  │
│  │  Source Map（构建时）:    源码 → 产物       标准链式映射      │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────┐                                                   │
│  │ Deno Worker  │ ← 长驻子进程，IPC 通信                           │
│  │ Volar        │   Vue SFC / TS / JS / CSS / HTML                 │
│  │ TSServer     │   类型检查 / 补全 / 导航                         │
│  └──────────────┘                                                   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Lane 模型

VueHost 采用三 Lane 架构，但**真正的请求路由、bridge supplement、结果聚合发生在 LSP / coordinator 层**，不是某个单独 Lane：

| Lane | 职责 | 不拥有 |
|------|------|--------|
| **JazorLane** | `.jazor` 解析、结构诊断、模板补全/悬停这类宿主本地能力 | C# 语义、Vue/TS 语义、全局请求聚合 |
| **RoslynLane** | `@code` 块补全/悬停/签名、C# 诊断/导航/重命名/代码操作 | `.jazor` 指令、前端语义 |
| **VolarLane** | 实际 `.vue/.ts/.js/.css/.html` 的 Volar/tsserver 原生语义、组件/属性解析、前端导航/重命名 | `.jazor` 规则、C# 语义 |

**关键约束**：没有任何 Lane 直接向 IDE 发布结果。所有 Lane 输出都先进入 `LspSession`、`DocumentProjectionResolver`、`LspLaneRouter` 以及 shared coordinators（如 `MarkupBridgeFanoutCoordinator`、`ReferenceCoordinator`、`RenameCoordinator`），再由 LSP 层统一发送。

**双向桥接约束**：

- `.jazor/.cs -> VueHost bridge -> .vue/.ts/.js/.css/.html`
- `.vue/.ts/.js/.css/.html -> VueHost bridge -> .jazor/.cs`
- VueHost 负责维护跨 Lane 的共享符号身份、位置锚点和结果聚合，不能只做单向透传
- `definition / references / rename` 的 bridge supplement 必须集中在 session/coordinator 层，而不是散落在 JazorLane / VolarLane 内部

**当前实现状态**：

- `.jazor` 模板位置会先走 VolarLane，再由 VueHost 把原生 Volar 结果映射回源文档
- `.vue` / `.ts` / `.js` 位置上的原生 Volar / tsserver definition 结果保持原样，不由 VueHost 伪造替代结果
- `.jazor` 和 `.vue/.ts/.js` 两侧的 `definition / references / rename` bridge supplement 已开始收口到 shared fan-out，而不是分散在各 Lane 内部
- 当前已经打通 `script import -> native .vue declaration -> .jazor markup references/rename` 这条双向桥接链路
- 当前 `.jazor` markup tag / `@module` 与 `.vue/.ts/.js` import 的 bridge identity 已收口到共享 `MarkupBridgeSymbol`
- 当前 `definition / references / rename` 的桥接补充路径已收口到共享 `MarkupBridgeFanoutCoordinator + MarkupBridgeSymbol`
- 当前 RoslynLane 已通过 bounded workspace scan 把未打开的 `.cs` / `.jazor` 源文档纳入 code-region IntelliSense 候选集合，`@code`/`.cs` 发起的 completion / hover / signatureHelp / diagnostics / definition / references / rename 都可以覆盖项目内未打开文件
- 当前 Roslyn 仍是 host 内的轻量 in-proc compilation，不等同于完整 MSBuild/Roslyn project system；项目级能力以源码发现和有界扫描为主

### 2.2.1 Razor / Roslyn 集成策略

VueHost 对 `.jazor` 的设计时语义不应自研一套新的 Razor/C# 语言服务，而应采用“**宿主协调 + 原生语义复用 + 自定义分析叠加**”的策略。

**结论先行**：

- Razor 语义优先复用 Razor 侧已有的语言服务/投影能力
- C# 语义优先复用 Roslyn 能力
- VueHost 自己负责 `.jazor` 解析、投影、位置映射、请求路由、结果聚合
- 自定义规则不直接 fork 官方 Razor/Roslyn server，而是在 VueHost 内部做可组合的附加分析层

#### 集成边界

| 层 | 职责 | 是否在 VueHost 自研 |
|----|------|-------------------|
| **Razor 语义层** | Razor 文档理解、Razor/C# 混合文档的设计时语义 | 否 |
| **Roslyn 语义层** | C# 语义模型、诊断、补全、导航、重命名、CodeAction | 否 |
| **Volar/TSServer 语义层** | Vue/TS/JS/CSS/HTML 的语言能力 | 否 |
| **Custom 语义层** | VueHost 特有规则、跨 Lane 规则、产品约束 | 是 |

#### 为什么不能只靠 Roslyn

- `.jazor` / `.razor` 不是纯 C# 文档，标记区、指令区、组件标签、属性绑定不属于 Roslyn 的原生输入
- 只分析投影后的 C# 会丢失一部分 Razor 源文档结构语义
- 组件标签、指令约束、模板结构类规则需要保留对原始 `.jazor` 文本和段级映射的访问

#### 为什么不建议 fork 官方 LSP

- Razor 与 Roslyn 的项目系统、虚拟文档、源映射和工作区装载逻辑都很重
- 直接 fork 语言服务会让 VueHost 被迫长期跟随上游实现细节
- VueHost 的核心价值不在替代这些 server，而在于把 `.jazor`、Razor/C#、Vue 三套语义统一成一个宿主体验

#### 自定义分析分层

自定义分析按语义归属分两类：

| 类型 | 推荐承载方式 | 典型示例 |
|------|-------------|---------|
| **C# 语义规则** | Roslyn Analyzer / CodeFix | `@code` 中的 API 约束、命名约束、符号使用规则 |
| **Razor / Jazor 结构规则** | VueHost sidecar analyzer | 非法指令组合、组件标签约束、属性绑定约束、跨区块一致性检查 |
| **跨 Lane 规则** | VueHost 聚合分析器 | `.jazor` 标记引用与 `@code` 符号的一致性、组件参数和前端 props 对齐 |

#### 诊断聚合原则

VueHost 应把诊断来源显式分层，但对 IDE 暴露单一结果集：

1. JazorLane 产出结构级诊断
2. Razor/Roslyn 产出 C#/Razor 语义诊断
3. VolarLane 产出 Vue/TS/CSS/HTML 诊断
4. Custom Analysis Layer 产出产品特有诊断
5. 聚合阶段执行 source mapping、去重、排序、severity 归一、owner 标注

#### 可扩展实现建议

- **不要**把自定义规则直接塞进 LSP handler
- **要**定义稳定的分析插件接口，例如 `IJazorDiagnosticProvider`、`IJazorCodeActionProvider`
- **要**让每个 provider 接收统一的文档快照、ProjectionMap、Lane 结果快照
- **要**让 CodeAction 明确声明它修改的是 source document 还是某个虚拟投影片段

#### 当前设计决策

- `.jazor` 的宿主入口始终是 VueHost，而不是直接把 IDE 接到 Razor LSP 或 Roslyn LSP
- VueHost 内部可以复用 Razor/Roslyn 的设计时能力，但外部协议边界只暴露 VueHost 自己的 LSP
- 自定义分析的主扩展面应在 VueHost 聚合层，不在上游 server fork 上做长期定制
- 能沉到 Roslyn Analyzer 的规则优先下沉，只有依赖 Razor/Jazor 原始结构的规则才保留在 VueHost

### 2.3 VolarLane 语言服务来源

VolarLane 通过 **Volar + TSServer**（运行在 Deno Worker 中）提供所有前端语言能力。对 `.jazor` 而言，VolarLane 消费的是 VueHost 协调后的 Razor/Roslyn bridge metadata，而不是临时生成的 `.g.vue` 或虚拟 `.vue` IntelliSense 文本。

| 文件类型 | 语言服务 | 说明 |
|---------|---------|------|
| `.vue` | Volar | SFC 各块（template/script/style）的完整语义 |
| `.ts` / `.js` | TSServer（Volar 内嵌） | TypeScript/JavaScript 类型检查、补全、导航 |
| `.css` | Volar CSS 模式 | Vue SFC 内 `<style>` 块的基础 CSS 智能感知 |
| `.html` | Volar template 模式 | Vue 模板的 HTML/指令补全 |
| `.json` | 轻量 JSON 解析 | 基础语法验证，不依赖独立语言服务 |

### 2.4 LSP 与 Dev Server 的关系

LSP 服务和 Dev Server 运行在同一进程中，共享同一个 Workspace Store：

| 维度 | 说明 |
|------|------|
| **进程** | 单一 .NET 进程，同时监听 LSP stdio 和 Dev Server HTTP 端口 |
| **工作区** | 共享 `IWorkspaceStore`，文档状态、依赖图、组件注册表统一 |
| **变更检测** | IDE 场景由 LSP `didChange` 驱动；纯浏览器场景由文件监听驱动 |
| **编译触发** | LSP 变更 → 增量投影 → 通知 Dev Server 更新虚拟模块 → HMR 推送 |
| **独立运行** | Dev Server 可脱离 LSP 单独运行（无 IDE 场景），此时退化为文件监听模式 |

---

## 三、文件感知范围

VueHost 可感知的文件类型：

| 文件类型 | 语义来源 | Lane 归属 |
|---------|---------|----------|
| `.jazor` | JazorLane（解析/桥接） + Razor/Roslyn + VolarLane（经桥接元数据协同） | 三 Lane 协作 |
| `.cs` | Roslyn（原生 C# 文档） | RoslynLane |
| `.vue` | Volar | VolarLane |
| `.ts` | TSServer（Volar 内嵌） | VolarLane |
| `.js` | TSServer（Volar 内嵌） | VolarLane |
| `.css` | Volar CSS 模式 | VolarLane |
| `.html` | Volar template 模式 | VolarLane |
| `.json` | 轻量 JSON 解析 | VueHost 基础能力 |

> `.vue` / `.ts` / `.js` / `.css` / `.html` 的 LSP 请求通过 VolarLane 直接转发给 Deno Worker 中的 Volar + TSServer，属于轻量管道。VueHost 自身不重新实现这些语言的分析逻辑，只消费它们的原生结果并在跨 Lane 场景补桥接结果。

---

## 四、LSP 能力

### 4.1 能力矩阵

> **阶段说明**：P1 = 最小可用体验，P2 = 增强导航与重构，P3 = 完整覆盖。
> `.jazor` 的能力依赖源文档 + 协调后元数据 + 跨 Lane 聚合，是主要工程难点。
> 其他真实前端文件的能力通过 VolarLane 管道转发，工程量较轻。

| 文件类型 | 诊断 | 补全 | 悬停 | 定义 | 引用 | 重命名 | 代码操作 |
|---------|:----:|:----:|:----:|:----:|:----:|:------:|:--------:|
| `.jazor` | P1 | P1 | P1 | P1 | P2 | P2 | P2 |
| `.vue` | P1 | P1 | P1 | P1 | P2 | P2 | P3 |
| `.ts` | P1 | P1 | P1 | P2 | P2 | P2 | P3 |
| `.js` | P2 | P2 | P1 | P2 | P2 | P3 | P3 |
| `.css` | P2 | P2 | P2 | P3 | — | P3 | P3 |
| `.html` | P2 | P2 | P2 | P3 | — | P3 | P3 |
| `.json` | P3 | P3 | P3 | P3 | — | P3 | — |

**P1 最小可用范围**：`.jazor` 的诊断/补全/悬停/定义（三 Lane 聚合）+ `.vue`/`.ts` 的诊断/补全/悬停（VolarLane 管道转发）。

### 4.2 `.jazor` 智能感知流程

```
.jazor 源码
     │
     ▼
┌─────────────────────────────────────┐
│  JazorLane                          │
│                                     │
│  1. 解析 .jazor 结构                │
│     - 组件标记识别 (<Counter />)     │
│     - 指令解析 (@code 等)           │
│     - 区域分类 (C#区 / 标记区)      │
│                                     │
│  2. 生成设计时桥接元数据            │
│     - @code 块 → Roslyn 代码投影   │
│     - 标记区  → Volar 协调元数据      │
│                                     │
│  3. 路由到对应 Lane                 │
└──────────┬──────────────┬───────────┘
           │              │
           ▼              ▼
┌───────────────┐  ┌──────────────┐
│  RoslynLane   │  │ VolarLane    │
│               │  │              │
│ C# 投影片段     │  │源 .jazor 标记区 +│──▶ Deno Worker
│  → C# 类型检查 │  │Volar 协调元数据  │    (Volar+TSServer / Deno)
│  → 补全/诊断   │  │  → 组件/属性     │
│  → 导航/重命名  │  │  → 前端语义      │
└───────┬───────┘  └──────┬───────┘
        │                 │
        └──── 双向符号/定位聚合 ────┘
                 ▼
┌─────────────────────────────────────┐
│  LSP/coordinator 聚合               │
│  - 通过 ProjectionMap / Volar 锚点映射回 .jazor     │
│  - 去重/排序                        │
│  - 跨 Lane 符号身份合并             │
│  - shared bridge supplement         │
└─────────────────┬───────────────────┘
                  ▼
         LSP 层 → IDE
```

### 4.2.1 投影触发时机

| 事件 | 投影行为 | 说明 |
|------|---------|------|
| `textDocument/didOpen` | 建立设计时上下文 | 解析 `.jazor`，建立 Roslyn 投影和 Volar 协调元数据；不生成 `.g.vue` IntelliSense 文档 |
| `textDocument/didChange` | 增量刷新上下文 | 判断变更影响区域，仅重建受影响的 Roslyn 投影或 Volar 协调元数据 |
| 组件注册表变更 | 刷新受影响上下文 | 新增/删除 `.vue` 文件时，刷新引用该组件的 `.jazor` Volar 协调元数据 |
| 无 IDE 连接 | 不投影 | Dev Server 独立运行时不触发 LSP 投影 |

**增量投影策略**：
- 变更仅在 `@code` 块内 → 仅重建 Roslyn 投影片段，Volar 协调元数据不变
- 变更仅在标记区 → 仅重建 Volar 协调元数据，Roslyn 投影片段不变
- 变更跨区域或影响结构（如指令变更） → 全量重建
- ProjectionMap 与 Volar 锚点随设计时上下文一起重建，保证位置映射始终一致

### 4.3 组件发现机制

组件候选由 VueHost 统一协调的 Volar 上下文提供，不需要把 `.jazor` 模板先投影成 `.vue` 文本。磁盘扫描只允许作为 host 内部派生上下文的实现细节，不能成为 worker 直接编造语义的来源。

#### 4.3.1 搜索范围

```
当前 .jazor 文件
        │
        ▼ 搜索路径（按优先级）
┌─────────────────────────────────────────────────────┐
│  1. 同目录下的 .vue 文件                             │
│  2. ./Components / ./components 下的 .vue 文件      │
│  3. 父目录及其 Components 下的 .vue 文件            │
│  4. workspace 根目录的 bounded 扫描                  │
│     - 深度限制：最大 5 层                             │
│     - 跳过 node_modules、.git、bin、obj              │
└─────────────────────────────────────────────────────┘
```

#### 4.3.2 组件注册表

```
┌─────────────────────────────────────┐
│  组件注册表 (内存)                    │
│  - ComponentName → .vue 路径         │
│  - 支持同名组件的优先级解析           │
│  - 支持自动导入推断                   │
└─────────────────────────────────────┘
```

#### 4.3.3 缓存与失效

| 策略 | 说明 |
|------|------|
| **首次扫描** | `didOpen` 时触发当前目录及 Components 的扫描 |
| **增量更新** | 文件监听（`FileSystemWatcher`）检测 `.vue` 文件的增删改 |
| **缓存失效** | 组件注册变更 → 通知引用该组件的 `.jazor` 刷新诊断 |
| **bounded 扫描** | workspace 级扫描仅在首次打开和手动触发时执行，不每次 `didChange` 都全扫 |

#### 4.3.4 外部组件

| 来源 | 处理方式 |
|------|---------|
| npm/Deno import 的组件库 | 不在磁盘扫描范围内，需要显式声明或配置 `jazor.config.json` |
| workspace 内其他项目 | bounded 扫描可能覆盖，但不保证；建议通过配置指定 |
| `node_modules` 中的 `.vue` | 默认跳过，不扫描 |

### 4.4 ProjectionMap：段级位置映射

ProjectionMap 是 VueHost **设计时**的核心映射基础设施，负责 `.jazor` 源码与设计时目标之间的精确位置转换。这里的目标可以是 Roslyn 代码投影片段，也可以是 Volar 协调元数据所锚定的前端语义位置。

> **ProjectionMap ≠ Source Map**。ProjectionMap 服务于 LSP 路由和诊断定位；Source Map 服务于构建时调试和产物追溯。两者目标不同、格式不同、生命周期不同。

#### 4.4.1 为什么需要 ProjectionMap

`.jazor` 文件中的不同区域会被映射到不同类型的设计时目标：

```
.jazor 文件
├── @code { ... }        → Roslyn 代码投影片段（RoslynLane 消费）
├── <Counter prop="x"/>  → Volar 锚点（关联真实组件/属性语义）
├── @functions { ... }   → Roslyn 代码投影片段
└── <div>静态 HTML</div> → Volar 锚点
```

LSP 请求（如光标在 `.jazor:15:10`）需要知道：
- 这个位置应该路由到哪个设计时目标？
- 对应目标中的哪个位置或哪个锚点？
- 结果返回时如何映射回 `.jazor:15:10`？

#### 4.4.2 映射精度

| 粒度 | 适用场景 | 最低要求 |
|------|---------|---------|
| **段落级** | 诊断、定义、引用 | 必须有。没有段落级映射，诊断会漂移、定义会跳错 |
| **行级** | 补全、悬停 | 推荐。在段落级基础上补充行号映射 |
| **字符级** | 重命名、代码操作 | 理想。精确到列的映射，用于编辑操作 |

**全文件映射不够用**。必须支持段级（segment-level）映射，否则诊断漂移、重命名损坏源码、定义跳转错误。

#### 4.4.3 ProjectionMap vs Source Map

| 维度 | ProjectionMap | Source Map |
|------|--------------|------------|
| **用途** | 设计时 LSP 路由和诊断定位 | 构建时调试和产物追溯 |
| **格式** | 自定义（`ProjectionMapEntry[]`） | 标准 Source Map v3 |
| **映射方向** | `.jazor` ↔ 设计时目标 | 源码 → 编译产物 |
| **生命周期** | 与设计时上下文同步，每次刷新重建 | 编译时生成，部署后使用 |
| **消费者** | JazorLane、LSP 层 | 浏览器 DevTools、调试器 |
| **精度要求** | 段落级（最低），字符级（理想） | 行级/列级 |

#### 4.4.4 数据模型

```csharp
// 段级映射条目
record ProjectionMapEntry(
    string SourceUri,        // .jazor 文件路径
    TextSpan SourceSpan,     // .jazor 中的原始范围
    string TargetUri,        // Roslyn 投影片段或相关前端文档路径
    TextSpan TargetSpan,     // 目标中的关联范围
    LaneKind TargetLane      // 目标 Lane: Roslyn / Volar
);

// 投影映射表
class ProjectionMap {
    ProjectionMapEntry[] Entries;

    // 正向映射：.jazor 位置 → 设计时目标
    ProjectionMapEntry? MapToTarget(Position source);

    // 逆向映射：设计时目标 → .jazor 位置
    ProjectionMapEntry? MapToSource(Position target, string targetUri);
}
```

#### 4.4.5 当前推进说明

当前代码已经先落地了这条更窄的实现切片：

- shared workspace resolver：统一 nearby lookup、tracked document lookup、bounded workspace scan、缓存失效
- shared workspace resolver：workspace roots 现在同时接受 tracked `.jazor` / `.cs` / `.vue` 作为 bounded scan 种子
- projection-aware routing metadata：LSP 路由阶段已经保留 projected document path 和 projected position
- 双向 offset/position 映射 API：为后续 segment-level `ProjectionMap` 收口预留统一入口

当前仍未完全到位的部分：

- 现有 lane handler 仍然主要基于源文档快照和桥接元数据工作，尚未完全收敛到统一的跨 Lane 符号身份模型
- `ProjectionMap` 的真实精度目标仍然是段级/字符级，而不是 whole-document fallback

这意味着当前阶段已经进入“投影感知基础设施”而不是“最终精度闭环”。

---

## 五、开发服务器

### 5.1 能力概览

Dev Server 运行在 VueHost 的 .NET 进程中（不是 Deno），负责开发时的文件服务和热更新。

```
┌─────────────────────────────────────────────────────────────┐
│                   Dev Server (.NET)                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  文件服务                                                   │
│  ├── 静态文件托管 (index.html)                              │
│  ├── 模块解析 (ESM)                                         │
│  └── 虚拟模块 (.jazor → .vue/.js)                           │
│                                                             │
│  编译服务（由 .NET 编译管道提供）                            │
│  ├── .jazor 实时编译（C# 编译器核心）                       │
│  ├── .ts 实时转译（委托 Deno Worker）                       │
│  └── .vue SFC 编译（委托 Deno Worker）                      │
│                                                             │
│  HMR 服务                                                   │
│  ├── WebSocket 连接                                         │
│  ├── 变更检测（来自 LSP didChange 或文件监听）              │
│  ├── 增量编译                                               │
│  └── 热更新推送                                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### 5.1.1 编译职责划分

| 编译任务 | 执行位置 | 说明 |
|---------|---------|------|
| `.jazor` → `.vue` / JS | .NET 进程 | 使用 Jazor 编译器核心（SemanticWalker 等） |
| `.ts` → `.js` | Deno Worker | 委托 Deno 内置的 TypeScript 转译 |
| `.vue` SFC 编译 | Deno Worker | 委托 `@vue/compiler-sfc`（通过 Volar） |
| 打包（bundle） | .NET 进程或外部 | 生产构建阶段，见第六章 |

### 5.2 HMR 流程

```
.jazor 文件变更
        │
        ▼ VueHost 检测
┌─────────────────────────────────────┐
│  增量编译                            │
│  - 判断影响范围                      │
│  - 仅重新编译变更部分                │
└─────────────────┬───────────────────┘
                  │
                  ▼ HMR Payload
┌─────────────────────────────────────┐
│  通知浏览器                          │
│  - 变更的模块 ID                     │
│  - 新的模块内容                      │
│  - 更新的 Source Map                 │
└─────────────────┬───────────────────┘
                  │
                  ▼
         热更新，无需刷新页面
```

---

## 六、编译管道

### 6.1 与智能感知的分离

| 维度 | 智能感知阶段 | 编译打包阶段 |
|------|-------------|-------------|
| **输入** | `.jazor` 源码 | `.jazor` 源码 |
| **输出** | LSP 消息 (诊断/补全等) | `.vue` / JS 产物 |
| **触发** | 用户编辑时 | 构建/发布时 |
| **性能要求** | 毫秒级响应 | 可接受秒级 |
| **产物可见性** | 内部使用，不可见 | 最终产物，可部署 |

### 6.2 编译流程

```
.jazor (Razor 源码)
        │
        ▼ 解析
┌─────────────────────────────────────┐
│  JazorParser                        │
│  - 提取 @code 块                    │
│  - 提取标记部分                      │
│  - 提取顶层指令                      │
│  - 区域分类                          │
└─────────────────┬───────────────────┘
                  │
                  ▼ 构建期物化
┌─────────────────────────────────────┐
│  内部中间产物生成                    │
│  - 内部 .vue / JS 桥接产物（可选）   │
│  - 虚拟 C# 片段（可选）              │
│  ┌───────────────────────────────┐  │
│  │ 关于虚拟 C# 文档：             │  │
│  │                               │  │
│  │ 是否生成虚拟 .cs 取决于        │  │
│  │ RoslynLane 的接入方式。        │  │
│  │                               │  │
│  │ 方案 A: 投影虚拟 .cs           │  │
│  │   生成最小 C# 上下文片段，     │  │
│  │   让 Roslyn 完整分析。         │  │
│  │   优点: 复用 Roslyn 全能力。   │  │
│  │   代价: 维护投影和映射。       │  │
│  │                               │  │
│  │ 方案 B: 直接分析 @code 片段    │  │
│  │   将 @code 块作为 C# 脚本     │  │
│  │   片段提交给 Roslyn。          │  │
│  │   优点: 无需维护 .cs 投影。    │  │
│  │   代价: 可能丢失外部上下文。   │  │
│  │                               │  │
│  │ 具体方案在实现阶段决定，       │  │
│  │ 不在设计文档中固化。           │  │
│  │                               │  │
│  │ 设计时 ProjectionMap 不属于    │  │
│  │ 此构建期物化步骤。             │  │
│  └───────────────────────────────┘  │
└─────────────────┬───────────────────┘
                  │
                  ▼ 转译
┌─────────────────────────────────────┐
│  JazorCompiler                      │
│  - C# → JavaScript 转译             │
│  - 模板 → Vue 渲染函数               │
│  - 生成桥接代码                      │
└─────────────────┬───────────────────┘
                  │
                  ▼ 打包
┌─────────────────────────────────────┐
│  产物输出                            │
│  - .vue SFC (可选)                   │
│  - JS 模块                           │
│  - Source Map                        │
└─────────────────────────────────────┘
```

### 6.3 打包接口

| 接口 | 用途 |
|------|------|
| `compile(jazorPath)` | 编译单个 `.jazor` 到 `.vue`/JS |
| `resolveImport(source, importer)` | 解析模块导入路径 |
| `getVirtualModule(id)` | 获取虚拟模块内容（用于 bundler 插件） |
| `getSourceMap(compiledPath)` | 获取编译产物的 Source Map |

### 6.4 生产构建

#### 6.4.1 Bundler 选型

生产构建需要一个 bundler 来处理 tree shaking、code splitting、minification。候选方案：

| 方案 | 优点 | 缺点 |
|------|------|------|
| **esbuild** | 极快，成熟的插件系统 | Go 编写，需要子进程调用 |
| **Rollup** | 原生 ESM，tree shaking 好 | 较慢，需要插件 |
| **自定义（基于 Deno）** | 与开发时一致 | 工程量大，生态不成熟 |

**推荐路径**：以 esbuild 作为生产 bundler，通过插件接口接入 VueHost 的编译管道。esbuild 的 `onLoad` 插件钩子可以拦截 `.jazor` 文件，调用 `compile()` 后返回 JS 产物。

#### 6.4.2 构建产物

| 产物 | 说明 |
|------|------|
| JS bundle | 编译后的 JavaScript 模块 |
| CSS 提取 | 从 `.vue` SFC 中提取的样式 |
| Source Map | external 格式，可选 |
| 静态资源 | 图片、字体等原样复制 |

---

## 七、映射系统

VueHost 的映射分为两个独立的层次，分别服务于不同场景：

```
┌───────────────────────────────────────────────────────────┐
│                                                           │
│  第一层：ProjectionMap（设计时）                           │
│  ─────────────────────────────                            │
│  .jazor 源码 ↔ 设计时目标（Roslyn 投影 / Volar 锚点）    │
│  用途: LSP 路由、诊断定位、跨 Lane 符号协调               │
│  格式: 自定义 ProjectionMapEntry[]                        │
│  消费者: JazorLane、LSP 层                                │
│                                                           │
│  ═══════════════════════════════════════════               │
│                                                           │
│  第二层：Source Map（构建时）                              │
│  ────────────────────────                                 │
│  源码(.jazor/.ts/.vue) → 编译产物(.js/.css) → bundle      │
│  用途: 浏览器调试、断点映射、调用栈还原                    │
│  格式: 标准 Source Map v3                                 │
│  消费者: 浏览器 DevTools、DAP 调试适配器                   │
│                                                           │
└───────────────────────────────────────────────────────────┘
```

### 7.1 ProjectionMap（设计时）

见 [4.4 ProjectionMap：段级位置映射](#44-projectionmap段级位置映射)。

ProjectionMap 在每次 `.jazor` 设计时上下文刷新时生成，与 Roslyn 投影和 Volar 锚点生命周期一致。它不需要持久化，也不使用 Source Map 格式。

### 7.2 Source Map（构建时）

Source Map 仅在编译管道产出 JS/CSS 时生成，遵循标准 Source Map v3 格式。

#### 7.2.1 链式映射

```
.jazor ────▶ .js（编译产物）───▶ bundle.js（打包产物）───▶ 浏览器
   │               │                    │
   │   smap 1      │       smap 2       │
   └───────────────┴────────────────────┘
                  链式合并
                       │
                       ▼
                最终 source map
                .jazor → bundle.js
```

#### 7.2.2 Source Map 生成策略

| 编译阶段 | 产物 | Source Map |
|---------|------|------------|
| `.jazor` → `.js` | Jazor 编译器输出 | 记录 `.jazor` 到 `.js` 的行/列映射 |
| `.ts` → `.js` | Deno TypeScript 转译 | 记录 `.ts` 到 `.js` 的映射 |
| `.vue` SFC → 渲染函数 | Vue SFC 编译器输出 | 记录模板到渲染函数的映射 |
| 打包合并 | `bundle.js` | 合并所有上游 Source Map |

> **注意**：设计时桥接使用 ProjectionMap，而构建期 `.jazor` → 内部 `.vue` / JS 物化使用 Source Map。这两条链路必须严格分开。

### 7.3 位置映射能力

```
正向映射（编译时）:
  .jazor:15:10  ──▶  bundle.js:1042:5

逆向映射（调试时）:
  bundle.js:1042:5  ──▶  .jazor:15:10
```

### 7.4 Source Map 服务接口

| 接口 | 说明 |
|------|------|
| `getSourceMap(compiledPath)` | 获取编译产物的 Source Map |
| `originalPosition(compiledPath, line, col)` | 逆向映射：产物位置 → 源码位置 |
| `generatedPosition(sourcePath, line, col)` | 正向映射：源码位置 → 产物位置 |
| `getSourceContent(sourcePath)` | 获取源码内容（用于调试器显示） |

---

## 八、调试支持

### 8.1 调试能力矩阵

| 调试功能 | 实现方式 | VueHost 职责 |
|---------|---------|-------------|
| **断点设置** | 在 `.jazor` 中设置 | Source Map 逆向映射到 bundle 位置 |
| **断点命中** | 浏览器暂停 | 接收 DevTools 事件，映射回源码 |
| **调用栈** | 浏览器提供 | 逐帧映射，显示 `.jazor` 文件名和行号 |
| **变量查看** | 运行时值 | 符号名保持，支持原始变量名 |
| **Watch 表达式** | 在调试器中计算 | 符号名映射 |
| **条件断点** | 表达式求值 | 表达式转译 |
| **单步执行** | 浏览器控制 | Source Map 行映射 |
| **异常断点** | 浏览器暂停 | 映射异常位置到源码 |

### 8.2 调试数据流

```
┌─────────────┐                      ┌─────────────────┐
│     IDE     │                      │    浏览器        │
│  (VS Code)  │                      │   DevTools      │
└──────┬──────┘                      └────────┬────────┘
       │                                     │
       │  1. 设置断点                         │
       │  .jazor:15                          │
       ▼                                     │
┌─────────────────────────────────────────────┴─────────────┐
│                        VueHost                            │
│                                                           │
│  2. Source Map 映射                                        │
│     .jazor:15 ──▶ bundle.js:1042                          │
│                                                           │
│  3. 发送映射后断点给浏览器                                  │
│                                                           │
│  4. 接收断点命中事件                                        │
│                                                           │
│  5. 调用栈逆向映射                                          │
│     bundle.js:1042 ──▶ .jazor:15                          │
│     bundle.js:1089 ──▶ .jazor:23                          │
│                                                           │
│  6. 返回源码调用栈给 IDE                                    │
│                                                           │
└───────────────────────────────────────────────────────────┘
```

### 8.3 协议支持

| 协议 | 用途 |
|------|------|
| **LSP** | 智能感知（诊断、补全等） |
| **DAP** | 调试适配协议 (Debug Adapter Protocol) |
| **CDP** | Chrome DevTools Protocol |
| **HTTP/WebSocket** | Dev Server / HMR |

### 8.4 DAP 接口

```
┌─────────────────────────────────────────────────────────────┐
│                    VueHost Debug Adapter                    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  DAP (Debug Adapter Protocol)                               │
│  ├── initialize         初始化调试会话                      │
│  ├── setBreakpoints     设置断点（映射后）                   │
│  ├── configurationDone  配置完成                            │
│  ├── threads            获取线程                            │
│  ├── stackTrace         获取调用栈（映射后）                 │
│  ├── scopes             获取作用域                          │
│  ├── variables          获取变量                            │
│  └── continue/step      执行控制                            │
│                                                             │
│  CDP (Chrome DevTools Protocol)                             │
│  ├── Debugger.enable     启用调试器                         │
│  ├── Debugger.pause      暂停                               │
│  ├── Debugger.resume     继续                               │
│  ├── Debugger.stepOver   单步跳过                           │
│  ├── Debugger.stepInto   单步进入                           │
│  └── Runtime.evaluate    执行表达式                         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 8.5 符号保持策略

| 模式 | 策略 | 适用场景 |
|------|------|---------|
| **开发模式** | 完全保留原始变量名 | 调试友好 |
| **生产模式** | 可压缩，但保留 Source Map 中的映射 | 可选调试支持 |

```javascript
// .jazor 源码
let userName = "John";

// 开发模式产物
let userName = "John";  // 保留原名

// 生产模式产物
let a = "John";         // 压缩后

// 但 Source Map 中记录: a → userName
```

### 8.6 DAP 部署模型

VueHost 同时作为 LSP server 和 DAP server 运行在同一个 .NET 进程中：

| 连接 | 协议 | 传输方式 | 用途 |
|------|------|---------|------|
| IDE 智能感知 | LSP | stdio | 编辑器交互 |
| IDE 调试 | DAP | stdio 或 TCP | 断点/调用栈/变量 |
| 浏览器调试 | CDP | WebSocket | DevTools 协议 |

```
┌─────────────┐      stdio LSP       ┌──────────────────┐
│     IDE     │─────────────────────▶│                  │
│             │                      │                  │
│             │      stdio DAP       │   VueHost        │
│             │─────────────────────▶│   (.NET 进程)    │
└─────────────┘                      │                  │
                                     │   LSP 服务       │
┌─────────────┐      CDP/WS         │   DAP 服务       │
│   浏览器     │◀────────────────────│   Dev Server     │
│  DevTools   │                      │   Source Map 服务│
└─────────────┘                      └──────────────────┘
```

DAP 服务和 LSP 服务共享同一套 Source Map 和 ProjectionMap 基础设施，避免重复映射逻辑。

### 8.7 映射失败处理

断点和调用栈映射可能因 Source Map 精度不足而失败：

| 场景 | 处理方式 |
|------|---------|
| `.jazor` 位置无法映射到 bundle | 断点在 IDE 中显示为 "unbound"，不设为验证态 |
| bundle 位置无法映射回 `.jazor` | 调用栈显示 bundle 原始位置，标注 "(source map unavailable)" |
| ProjectionMap 不精确导致诊断漂移 | 诊断仍发布，但标注来源 Lane 和置信度 |
| Source Map 链式合并失败 | 使用最近一级可用 Source Map，跳过不可用的中间层 |

---

## 九、开发模式 vs 生产模式

| 维度 | 开发模式 | 生产模式 |
|------|---------|---------|
| **Source Map** | 完整、inline | 可选、external |
| **符号名** | 保留原始名 | 可压缩（有映射） |
| **编译优化** | 关闭 | 开启 |
| **HMR** | 启用 | 禁用 |
| **调试支持** | 完整 | 可选 |
| **产物体积** | 较大 | 最小化 |

---

## 十、IDE 集成

### 10.1 单一连接点

IDE 只需连接 VueHost 一个进程，通过不同协议获取全部能力：

```
┌─────────────┐
│     IDE     │
│  VS Code    │
│  Cursor     │
│  其他编辑器  │
└──────┬──────┘
       │
       ├──── LSP (stdio) ──────▶ 智能感知（所有前端文件）
       │
       └──── DAP (stdio/TCP) ──▶ 调试支持（断点/调用栈/变量）
```

VueHost 内部由 `LspSession`、`DocumentProjectionResolver`、`LspLaneRouter` 和 shared coordinators 负责路由与聚合：`.jazor` 请求按区域分发到 RoslynLane / VolarLane / JazorLane，`.vue/.ts/.js` 请求保持 native Volar / tsserver 结果，再由 host 做必要的 bridge supplement。

**好处**：
- IDE 只需配置一个进程入口
- 跨文件类型统一的智能感知体验
- 共享组件图、文档关系、Source Map 基础设施
- 调试和智能感知共享同一套映射系统

---

## 十一、与其他工具的关系

| 工具 | 与 VueHost 的关系 |
|------|------------------|
| **Volar + TSServer** | VueHost 通过 Deno Worker 托管，提供 `.vue`/`.ts`/`.js`/CSS/HTML 的统一语言服务 |
| **Roslyn** | VueHost 通过 RoslynLane 接入，提供 `@code` 块的 C# 语义 |
| **esbuild** | 生产构建的 bundler，通过插件接口接入 VueHost 编译管道 |
| **Chrome DevTools** | VueHost 通过 CDP 与浏览器调试器通信 |
| **IDE (VS Code 等)** | 通过 LSP + DAP 与 VueHost 通信，单一连接点 |

---

## 十二、能力清单总结

| 能力域 | 具体能力 | 说明 |
|--------|---------|------|
| **三 Lane 架构** | JazorLane（解析/桥接/路由）、RoslynLane（C#）、VolarLane（Volar+TSServer） | 协调中枢 + 两个复用语义 Lane |
| **ProjectionMap** | 段级双向映射（.jazor ↔ 设计时目标） | 设计时 LSP 路由、诊断定位、跨 Lane 聚合的基础设施 |
| **全前端 LSP** | 诊断、补全、悬停、定义、引用、重命名、代码操作 | 覆盖 `.jazor` `.vue` `.ts` `.js` `.css` `.html` `.json`，分阶段交付 |
| **开发服务器** | 文件服务、模块服务、虚拟模块 | 所有前端文件，.NET 进程内运行 |
| **HMR** | 变更检测、增量编译、热更新 | 由 LSP `didChange` 或文件监听触发 |
| **编译管道** | `.jazor` 转译（.NET）、`.ts`/`.vue` 编译（委托 Deno Worker） | 编译职责按进程划分 |
| **打包** | 生产构建（esbuild 插件）、Tree shaking、Code splitting | 通过 bundler 插件接入 |
| **Source Map** | 链式映射（构建时）、正向/逆向映射 | 标准 Source Map v3，与 ProjectionMap 分层 |
| **调试支持** | 断点映射、调用栈还原、变量查看、映射失败降级 | LSP + DAP + CDP 三协议，共享映射基础设施 |
| **协议支持** | LSP（智能感知）、DAP（调试）、CDP（浏览器）、HTTP/WS（Dev Server） | 单一进程，多协议 |

---

## 十三、目标总结

**不是** "Vue 化 Razor 语法"

**而是** "Razor-first 的 Vue 宿主"

- Authoring 是 Razor-first
- VueHost 负责把 Razor/Roslyn 与 Volar 双向桥接成单一宿主体验
- 能提供就提供，不局限于 `.jazor`
- 单一入口，简化 IDE 集成

---

## 十四、降级与容错

VueHost 的各个子系统可能因外部依赖不可用而失败。每个失败模式都需要有明确的降级策略，确保整体不会因局部故障而完全不可用。

### 14.1 子系统降级矩阵

| 故障场景 | 影响范围 | 降级行为 | 恢复方式 |
|---------|---------|---------|---------|
| **Deno Worker 崩溃** | VolarLane 全部能力 | `.jazor` 的 C# 补全/诊断继续工作（RoslynLane 独立可用）；`.vue`/`.ts`/`.js`/`.css`/`.html` 智能感知全部不可用；自动重启 Deno Worker | 自动重启 + 重连 + 重推工作区文档与桥接元数据 |
| **Deno Worker 启动失败** | VolarLane 持续不可用 | 同上降级；启动重试（指数退避，最多 3 次）；重试全部失败后 VolarLane 标记为 unavailable | 用户重启 VueHost |
| **RoslynLane 异常** | `@code` 块的 C# 智能感知 | 标记区（Vue 模板部分）的补全/诊断继续工作（VolarLane 独立可用）；`@code` 区域诊断消失 | 重置 Roslyn 工作区 |
| **投影失败** | 双 Lane 都无法工作 | JazorLane 发布结构级诊断（"无法解析 .jazor 文件"）；退化为纯文本编辑体验 | 修复 `.jazor` 源码 |
| **ProjectionMap 精度不足** | 诊断位置漂移、定义跳转偏差 | 诊断仍发布但降低精度；标注来源 Lane 和置信度 | 逐步改进映射精度 |
| **Source Map 生成失败** | 调试体验降级 | Dev Server 正常运行（文件服务 + HMR 不受影响）；调试时无法映射回源码 | 编译管道修复 |
| **组件注册表过期** | 组件补全/诊断不准确 | 已缓存的组件仍可用，新增组件不出现 | 文件监听触发重新扫描 |
| **esbuild 不可用** | 生产构建失败 | 开发模式完全不受影响；生产构建报错提示 | 安装/配置 esbuild |

### 14.2 降级原则

1. **Lane 独立性**：RoslynLane 和 VolarLane 应能独立工作。一个 Lane 失效不应拖垮另一个。
2. **静默降级**：能力消失时不在 IDE 中弹出错误通知（除非是启动级故障）。用户只应看到"补全少了"而非"出错了"。
3. **自动恢复**：外部进程（Deno Worker）崩溃后自动重启，无需用户干预。
4. **部分可用优于完全不可用**：即使只能提供纯文本编辑体验，也不应阻塞用户正常编辑 `.jazor` 文件。

### 14.3 启动级故障

以下故障会阻止 VueHost 正常启动，需要明确的错误信息：

| 故障 | 表现 |
|------|------|
| Deno 未安装 | 启动时检测，输出 "Deno is required for Volar/TSServer language services. Install: https://deno.land" |
| 端口被占用 | Dev Server 端口冲突，输出具体端口号和占用进程信息 |
| 工作区路径无效 | 无法解析 workspace root，输出路径错误信息 |
