# VueHost — 全功能开发模式

> 对应源码：`src/Jazor.VueHost/`

## 为什么需要

RazorVue 的库模式适合编译时场景，但应用开发需要更多：实时的编辑器智能提示、文件保存后即刻看到效果的 HMR、在 IDE 中打断点调试 .jazor 源码、生产构建输出优化后的静态资源。VueHost 是一个**类似 Vite 的大满贯开发平台**，提供从编写到调试到构建的完整闭环。

## 解决什么问题

1. **开发服务器 + HMR**：`dotnet run -- --dev` 启动 HTTP 开发服务器，文件变更后浏览器 < 500ms 内看到更新
2. **LSP 全语义**：3-Lane 架构（Jazor + Roslyn + Volar）提供 24 种 LSP 方法的完整智能提示
3. **源码级调试**：DAP + CDP 双协议，在 IDE 中对 .jazor 源码设置断点
4. **生产构建**：`dotnet run -- --build` 输出优化后的静态资源（CSS 提取、Source Map、Manifest）
5. **扩展系统**：11 种 Provider 接口，支持内置和外部扩展

## 大致实现思路

### 核心区别：支持 .jazor 和 .vue SFC

VueHost 的核心定位是同时支持两种文件格式：

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

| 维度 | VueHost（全功能模式） | RazorVue（库模式） |
|------|---------------------|-------------------|
| 触发方式 | 独立进程 | Source Generator |
| 输出格式 | .vue SFC + JS/CSS | 纯 JS/TS 模块 |
| 文件支持 | .jazor + .vue SFC | .razor |
| 热更新 | HMR（< 500ms） | 无 |
| 调试 | DAP + CDP 源码级 | 无 |
| LSP | 24 种方法全覆盖 | 仅 Roslyn 分析 |

## 参考文档

- 完成度分析：`docs/03-完成/vuehost/completion-analysis.md`
- 状态快照：`docs/03-完成/vuehost/status.md`
- 实施计划：`docs/02-计划/vuehost/`（Phase 1–7）
