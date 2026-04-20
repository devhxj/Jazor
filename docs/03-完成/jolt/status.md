# Jolt 状态（2026-04-20）

> Status: 当前状态快照
> Positioning: 活跃的 `Jolt` 开发时边界的仓库级状态快照
> Scope: `.jazor` 编辑宿主、LSP、开发服务器/HMR、Source Map 管道、调试适配器基线和生产构建 lane

## 总结

`Jolt` 当前七阶段里程碑验收项已全部收口，且 2026-04-20 的迁移收尾项已经完成，进入稳态巡检与持续压实阶段。

当前更准确的描述是：

- Phase 1/2/3：主路径与稳定性目标达成，进入持续巡检
- Phase 4：DAP + CDP 闭环目标达成（并发断点、异常栈映射与链路稳态回归通过）
- Phase 5：生产构建目标达成（失败路径与边界场景回归补齐）
- Phase 6：高级 LSP 核心目标达成（P2 + 关键 P3 稳定可用）
- Phase 7：扩展系统核心目标达成并继续硬化（provider 扩展面 + 超时隔离 + 健康查询 + 目录边界约束 + 权限/哈希校验 + builtin 生产 provider + VS Code 最小集成）
- 基础设施迁移：`Jazor.VueHost -> Jolt` 重命名完成，共享 RazorVue manifest/SourceMap 基元已上提到 `Jazor.Common`，`Jolt` 与 `Jazor.Emit` 解耦完成

## 当前状态判断

### 1. Host 边界和 Lane 架构已稳定

- `Jolt` 作为唯一 active host 边界已经成型
- In-proc Razor/Roslyn + Deno frontend worker 的组合已进入主干
- LSP 路由、桥接聚合和 workspace resolver 形成了稳定主线
- `Jazor.Vue` 与 `Jazor.SourceMaps` 这类中性共享定义已落在 `Jazor.Common`，`Jolt` 不再依赖 `Jazor.Emit`

### 2. Dev Server / HMR / SourceMap 已形成闭环主路径

- `--dev` 路径、HMR 通道、workspace 变更去重已具备系统级实现
- `.jazor/.vue/.ts` 的编译与 SourceMap 回传/链式处理已在主路径可用
- SourceMap 已被 DAP/CDP 调试链路消费于断点映射与调用栈回填；`.jazor` 多锚点列映射与 `.vue` 原生 script/template 链式列映射已落地，并有真实浏览器/CDP + HMR 长链路压测
- `resolve.alias` 已接入 `jazor.config.json -> DevServerOptions/BuildOptions -> ModuleResolver`，dev/build 共用解析链路

### 3. 高级 LSP 能力已超过 phase-one 最小范围

- `references` / `rename` / `codeAction` / `documentSymbol` / `semanticTokens` 已进入主线能力
- `.jazor` 与 `.vue/.ts/.js` 之间的 bridge supplement 已有实装与回归用例
- Roslyn lane 已覆盖部分未打开磁盘文档场景（有界扫描）

### 4. 测试覆盖规模已具备"回归网"属性

- `src/Jazor.CompilerTest` 中 `Jolt*Tests` 文件当前为 **32** 个，`[TestMethod]` 为 **578** 个，覆盖 DevServer/LSP/Build/Debug/SourceMap 等主域
- `JoltFrontendLaneTests` / `JoltLspTests` 的并发稳定性问题已通过运行时隔离修复，不再依赖类级串行化回避
- 当前本地工作区若有长时间运行的 `Jolt --lsp` 或测试宿主进程，构建阶段仍可能出现短暂文件占用重试，但本轮 full compiler 回归已验证可稳定通过

## 阶段进展矩阵

| 阶段 | 当前完成度 | 进展判断 | 说明 |
|------|------------|---------|------|
| Phase 1 Dev Server MVP | 100% | 里程碑验收完成 | 核心 HTTP 模块服务与编译路径稳定；WebSocket upstream 端口竞态回归通过 |
| Phase 2 Source Map | 100% | 里程碑验收完成 | 多锚点列映射与链式映射主路径稳定；DAP/CDP 消费链路回归通过 |
| Phase 3 HMR | 100% | 里程碑验收完成 | 变更去重、style/js/full-reload 分类与 alias 统一链路稳定；disk/workspace 双来源去重硬化 |
| Phase 4 Debug (DAP + CDP) | 100% | 里程碑验收完成 | stackTrace/scopes/variables/evaluate/continue 闭环稳定；并发断点与异常栈映射回归通过 |
| Phase 5 Production Build | 100% | 里程碑验收完成 | build lane 串台问题前置修复（端口竞态 + bundler 前缀/路径归一化），失败路径/边界回归通过 |
| Phase 6 Advanced LSP | 100% | 里程碑验收完成 | references/rename/codeAction/documentSymbol/semantic tokens 主能力稳定，跨 lane 补桥接用例回归通过 |
| Phase 7 Extension System | 100% | 里程碑验收完成 | 扩展 provider 聚合 + 超时隔离 + 健康查询 + signature/inlay/workspace/folding + 目录越界防护 + trusted/hash/provider-permission 前置拦截 + builtin 结构诊断/指令补全/组件 codeAction/workspace-symbol + VS Code 最小扩展骨架 |

## 本轮验证（2026-04-20）

- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter 'FullyQualifiedName~JoltFrontendLaneTests|FullyQualifiedName~JoltLspTests' -v minimal`：**127/127 通过**
- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter 'FullyQualifiedName~Jolt_StdioLspServer_CancelRequest_CancelsQueuedRequestBeforeExecution|FullyQualifiedName~Jolt_DenoFrontendHost_CompileSfcAsync_WithBundledWorker_ReturnsCompiledVueModuleAndColumnAwareSourceMap|FullyQualifiedName~Jolt_Lsp_TypeScriptDocument_ReturnsFrontendScriptCompletionHoverAndDefinition' -v minimal`：**3/3 通过**
- `pwsh ./scripts/test-dotnet.ps1 -Project compiler`：**2350/2350 通过**

## 近期推进信号（截至 2026-04-20）

- 最近一周 `jolt` 相关提交持续高频，且覆盖 feature/refactor/test/docs 四类
- 最新提交聚焦在：
  - `Jazor.VueHost -> Jolt` 的命名收口与文档同步
  - RazorVue manifest / SourceMap 共享基元上提到 `Jazor.Common`
  - Deno worker 并发隔离、`stderr` 可观测性与 LSP queued cancel 语义修复

## 下一步行动

1. **稳态巡检（持续项）**
   - 扩展真实浏览器/CDP 长时压测矩阵（资源占用、并发负载）
   - 持续补调试可视化与诊断端点，降低问题定位成本

2. **构建持续压实（持续项）**
   - 继续压实构建产物一致性（manifest/css/js/source map）
   - 增补跨平台路径与缓存污染回归
   - 继续观察 Deno worker 隔离 workspace 的长时缓存行为与清理策略

3. **LSP 收敛与扩展（持续项）**
   - 继续修正跨 lane supplement 的一致性和保守边界
   - 明确哪些能力是"native first"，哪些由 host 负责补桥接

4. **生态拓展（后续项）**
   - 在最小 VS Code 集成骨架基础上补 LanguageClient 传输层与发布链路
   - 继续细化扩展权限与沙箱策略（进程/IO/网络分级约束）

## 风险与注意项

- 文档与实现迭代速度不一致时，最容易造成阶段误读；应优先维护 repo-level 状态页
- 需持续防止"临时 fallback 重新进入主路径"，保持前置硬化优先
- 构建与 LSP 并行演进时，需持续防止"测试路径与真实路径分叉"
