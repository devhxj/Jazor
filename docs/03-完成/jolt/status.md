# Jolt 状态（2026-04-23）

> Status: 当前状态快照（Windows 生产目标收敛后更新）
> Positioning: 活跃的 `Jolt` 开发时边界的仓库级状态快照
> Scope: `.jazor` 编辑宿主、LSP、开发服务器/HMR、Source Map 管道、调试适配器基线和生产构建 lane

## 总结

`Jolt` 当前七阶段里程碑验收项已全部收口。最新一轮 Windows-only 复评进一步把“生产”范围收敛为：本机开发、受控内网调试、正式构建 lane；不再把公网服务作为当前目标。对应的失败路径、配置文件边界和 CLI 参数 fail-fast 已补齐，架构和代码质量保持良好。

当前更准确的描述是：

- Phase 1/2/3：主路径与稳定性目标达成，进入持续巡检
- Phase 4：DAP + CDP 闭环目标达成（并发断点、异常栈映射与链路稳态回归通过）
- Phase 5：Windows 生产构建目标达成（失败路径、配置边界与 smoke 回归补齐）
- Phase 6：高级 LSP 核心目标达成（P2 + 关键 P3 稳定可用）
- Phase 7：扩展系统核心目标达成并继续硬化（provider 扩展面 + 超时隔离 + 健康查询 + 目录边界约束 + 权限/哈希校验 + builtin 生产 provider + VS Code 最小集成）
- 基础设施迁移：`Jazor.VueHost -> Jolt` 重命名完成，共享 RazorVue manifest/SourceMap 基元已上提到 `Jazor.Common`，`Jolt` 与 `Jazor.Emit` 解耦完成
- 架构重构（第四轮）：`@code`/`@functions` 与 `@module`/`@import` 指令定位逻辑已从 `JazorVueParser` 提取为独立共享定位器 `JazorCodeDirectiveLocator`（484 行）与 `JazorImportDirectiveLocator`（445 行），`JazorVueParser` 精简至 640 行、`DocumentRegionClassifier` 精简至 265 行、`MarkupComponentBridgeService` 扩展至 844 行支持 script-import bridge
- 稳态补强：深目录 workspace 组件解析、`@functions` / CRLF 文档区域分类、代码块内字符串/注释括号误判、标准 Razor 指令即使未预热 projection map 也会回落到 Razor/Roslyn、无 `@code` 的 `.jazor` 也会建立 Razor design-time projection、Roslyn completion 已补齐 namespace/type 以覆盖 `@using` 等标准指令场景、directive-only 文档在 EOF 仍保持 `Directive` 区域判定、comment-only 前导行不再把后续 directive 误切进 template/markup 边界、注释中的 `@code {` 也不再污染后续顶层 directive 分类、`/* ... */` 与 `@* ... *@` 前导注释中的伪 `@code` 也不会再把文档误切进 code lane、`JazorVueParser`/Razor fallback/semantic tokens/builtin 结构诊断现已统一复用共享真实 `@code` 扫描器并正确跳过注释与代码块内字符串/注释中的括号、compile parser 也会跳过无块体的前导 `@code` 并命中后续真实代码块、`DocumentRegionClassifier` 现已与 `@code/@functions` 共用顶层块指令扫描器，前导无块体 `@code` 不会再把后续 `@module` 指令或真实代码块误吞进 code lane、重复真实 `@code` 已有显式结构诊断、顶层 `@module` / legacy `@import` 识别也已统一复用共享扫描器，parser、document links、semantic tokens、legacy diagnostics 与 builtin component code action 均会跳过注释、template 和代码块中的伪指令，`@module` 绑定子句解析也已共享给 parser 与 builtin component code action，quick-fix 现在按 local binding 精确匹配，不再因为 source path 或 imported name 假命中而误判”已导入”、`.vue/.ts` script-import bridge 会先屏蔽 JS/TS 注释与字符串文本，不再把块注释或 template string 中的伪 `import` 当成组件绑定、`StdioLspServer` 在 handler 执行前补上 queued cancel 落地窗口，已发出的 `$/cancelRequest` 可稳定转为 `-32800 Request cancelled.`、builtin `@module` 补全已限制在顶层 directive 区域且不再泄漏到 template 表达式、`@module` lane 收口、builtin 指令补全收窄到 `@module`、取消构建/分析时的子进程清理已修复并补回归、`--lsp --dev` 也已抑制 ASP.NET hosting 日志以保持 stdout 为纯 LSP 协议流
- 第四轮收口：`JazorVueParser` 残余扫描实现已删除并统一复用 locator，`JazorMarkupPatterns` 已统一组件标签 regex，`MarkupComponentBridgeService` 死代码与 Build/Deno 6 处目录 null 风险已修复，`completion-analysis` 已回到 **100%**

## 当前生产目标（Windows）

当前确认的生产目标不是“Jolt 直接对公网提供服务”，而是以下更窄、更现实的目标边界：

- `Jolt --build`：作为正式构建链路投入使用，输出 `dist/` 静态产物
- `Jolt --dev`：用于本机开发或受控内网联调，不作为公网服务
- `Jolt --preview`：用于本地或受控内网验收预览，不作为正式站点
- `Jolt --lsp`：仅服务本机编辑器/工具链，不暴露到公网
- 公开访问应由 `--build` 产物后的静态站点承接，例如 IIS、Nginx、CDN 或对象存储
- 信任边界仍限定为可信项目/可信扩展；当前不以多租户、不可信代码执行或强沙箱隔离为目标

按这个目标判断，`Jolt` 现阶段结论是：

- Windows 下的 `--build`、`--lsp`、本地 `--dev` 已达标
- Windows 下的受控内网 `--dev` / `--preview` 可用，但前提是只开放给指定机器或网段
- “Jolt 进程直接面向公网”不在当前生产目标内，因此不作为验收项

## 当前状态判断

### 1. Host 边界和 Lane 架构已稳定

- `Jolt` 作为唯一 active host 边界已经成型
- In-proc Razor/Roslyn + Deno frontend worker 的组合已进入主干
- LSP 路由、桥接聚合和 workspace resolver 形成了稳定主线
- 必须区分 compile 与 LSP 两阶段：compile 仍由 `Jolt/Jazor` 编译管线负责；智能感知阶段的 `.jazor` 已明确为“标准 Razor 指令默认走 Razor/Roslyn，哪怕 projection map 尚未预热也会优先留在 Roslyn lane；host 只保留 `@module` 这一个自定义指令入口”，builtin 指令补全也只保留 `@module`
- `Jazor.Vue` 与 `Jazor.SourceMaps` 这类中性共享定义已落在 `Jazor.Common`，`Jolt` 不再依赖 `Jazor.Emit`

### 2. Dev Server / HMR / SourceMap 已形成闭环主路径

- `--dev` 路径、HMR 通道、workspace 变更去重已具备系统级实现
- `.jazor/.vue/.ts` 的编译与 SourceMap 回传/链式处理已在主路径可用
- SourceMap 已被 DAP/CDP 调试链路消费于断点映射与调用栈回填；`.jazor` 多锚点列映射与 `.vue` 原生 script/template 链式列映射已落地，并有真实浏览器/CDP + HMR 长链路压测
- `resolve.alias` 已接入 `jolt.config.json -> DevServerOptions/BuildOptions -> ModuleResolver`，dev/build 共用解析链路
- `--lsp --dev` 共存时，`DevHttpServer` 已清空 ASP.NET hosting logger provider，`stdout` 不再泄漏 `info:` 日志污染 LSP stream
- Windows 当前生产目标下，`--dev` / `--preview` 的定位明确为“本地或受控内网调试/预览”，不是公网承载面

### 3. 高级 LSP 能力已超过 phase-one 最小范围

- `references` / `rename` / `codeAction` / `documentSymbol` / `semanticTokens` 已进入主线能力
- `.jazor` 与 `.vue/.ts/.js` 之间的 bridge supplement 已有实装与回归用例
- Roslyn lane 已覆盖部分未打开磁盘文档场景（有界扫描），并已接住无 `@code` 的 `.jazor` 标准 Razor 指令智能感知

### 4. 测试覆盖规模已具备"回归网"属性

- `src/Jolt.Test` 当前最新全量验证为 **748/748 通过**，覆盖 DevServer/LSP/Build/Debug/SourceMap/组件桥接等主域
- `JoltFrontendLaneTests` / `JoltLspTests` 的并发稳定性问题已通过运行时隔离修复，不再依赖类级串行化回避
- 当前本地工作区若有长时间运行的 `Jolt --lsp` 或测试宿主进程，构建阶段仍可能出现短暂文件占用重试；但 `--lsp --dev` 的 stdout 污染已修复，且本轮 full compiler 回归已验证 **2391/2391** 可稳定通过

## 阶段进展矩阵

| 阶段 | 当前完成度 | 进展判断 | 说明 |
|------|------------|---------|------|
| Phase 1 Dev Server MVP | 100% | 里程碑验收完成 | 核心 HTTP 模块服务与编译路径稳定；WebSocket upstream 端口竞态回归通过 |
| Phase 2 Source Map | 100% | 里程碑验收完成 | 多锚点列映射与链式映射主路径稳定；DAP/CDP 消费链路回归通过 |
| Phase 3 HMR | 100% | 里程碑验收完成 | 变更去重、style/js/full-reload 分类与 alias 统一链路稳定；disk/workspace 双来源去重硬化 |
| Phase 4 Debug (DAP + CDP) | 100% | 里程碑验收完成 | stackTrace/scopes/variables/evaluate/continue 闭环稳定；并发断点与异常栈映射回归通过 |
| Phase 5 Production Build | 100% | 里程碑验收完成 | build lane 串台问题前置修复（端口竞态 + bundler 前缀/路径归一化），失败路径/边界回归通过 |
| Phase 6 Advanced LSP | 100% | 里程碑验收完成 | references/rename/codeAction/documentSymbol/semantic tokens 主能力稳定，跨 lane 补桥接用例回归通过 |
| Phase 7 Extension System | 100% | 里程碑验收完成 | 扩展 provider 聚合 + 超时隔离 + 健康查询 + signature/inlay/workspace/folding + 目录越界防护 + trusted/hash/provider-permission 前置拦截 + builtin 结构诊断/仅 `@module` 指令补全/组件 codeAction/workspace-symbol + VS Code 最小扩展骨架 |

## 本轮验证（2026-04-23，Windows 生产目标收敛）

- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --disable-build-servers -m:1 -v minimal`：**748/748 通过**
- `dotnet publish src/Jolt/Jolt.csproj --no-restore -c Release -o .artifacts/publish-jolt-review -v minimal`：**通过**
- 发布版 `Jolt.exe --build` + 有效 `jolt.config.json` smoke：**通过**
- 发布版 `Jolt.exe --build` + 非法 JSON `jolt.config.json` smoke：**非 0 失败，stderr 明确**
- 发布版 `Jolt.exe --build` + 非法 `build.sourceMap` 配置 smoke：**非 0 失败，stderr 明确**
- 发布版 `Jolt.exe --build/--preview` + 非法 CLI `--sourcemap=bogus` smoke：**非 0 失败，stderr 明确**

## 历史验证（2026-04-21）

- `dotnet build src/Jolt/Jolt.csproj --no-restore -v minimal`：**通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~JazorVueCompilerTests|FullyQualifiedName~JazorVueAnalysisRuntimeTests|FullyQualifiedName~JoltBuildTests|FullyQualifiedName~JoltFrontendLaneTests|FullyQualifiedName~JoltMarkupComponentBridgeTests|FullyQualifiedName~JoltPhase6LspTests|FullyQualifiedName~JoltLspTests|FullyQualifiedName~JoltPhase7ExtensionSecurityAndBuiltinTests' --disable-build-servers -m:1 -v minimal`：**312/312 通过**

- `dotnet test src/Jolt.Test/Jolt.Test.csproj --filter 'FullyQualifiedName~JoltFrontendLaneTests|FullyQualifiedName~JoltLspTests' -v minimal`：**127/127 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --filter 'FullyQualifiedName~Jolt_StdioLspServer_CancelRequest_CancelsQueuedRequestBeforeExecution|FullyQualifiedName~Jolt_DenoFrontendHost_CompileSfcAsync_WithBundledWorker_ReturnsCompiledVueModuleAndColumnAwareSourceMap|FullyQualifiedName~Jolt_Lsp_TypeScriptDocument_ReturnsFrontendScriptCompletionHoverAndDefinition' -v minimal`：**3/3 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~DocumentProjectionResolver_ResolveAsync_RoutesStandardRazorDirectiveToRoslynWithoutProjectionMap|FullyQualifiedName~Jolt_Lsp_StandardRazorDirectiveCompletion_UsesRoslynWithoutCodeBlock' -v minimal`：**2/2 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~BuiltinDirectiveCompletionProvider_ServesDirectiveCompletionsThroughLspSession|FullyQualifiedName~BuiltinDirectiveCompletionProvider_DoesNotServeStandardRazorDirectivesThroughLspSession|FullyQualifiedName~BuiltinDirectiveCompletionProvider_DoesNotServeModuleDirectiveInsideTemplateExpression|FullyQualifiedName~DocumentProjectionResolver_ResolveAsync_RoutesStandardRazorDirectiveToRoslynWithoutProjectionMap|FullyQualifiedName~Jolt_Lsp_StandardRazorDirectiveCompletion_UsesRoslynWithoutCodeBlock|FullyQualifiedName~DocumentRegionClassifier_Classify_DirectiveOnlyDocumentAtEndOfFile_RemainsDirective' -v minimal`：**6/6 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~DocumentRegionClassifier_Classify_CommentedCodeDirectiveMarker_DoesNotCaptureFollowingDirective|FullyQualifiedName~BuiltinDirectiveCompletionProvider_ServesDirectiveCompletionsAfterCommentedCodeDirectiveMarker' -v minimal`：**2/2 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~DocumentProjectionResolver_ResolveAsync_LeavesModuleDirectiveOnJazorLane_WhenNoBlockCodeDirectivePrecedesRealCodeBlock|FullyQualifiedName~DocumentRegionClassifier_Classify_NoBlockCodeDirectiveBeforeRealCodeBlock_DoesNotCaptureDirectiveGap' --disable-build-servers -m:1 -v minimal`：**2/2 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~JazorLaneService_GetSemanticTokens_IgnoresCommentedFakeCodeDirectiveMarkers|FullyQualifiedName~BuiltinStructureDiagnosticProvider_IgnoresCommentedFakeCodeDirectiveMarkersWithoutBlockBody|FullyQualifiedName~BuiltinStructureDiagnosticProvider_IgnoresCommentedFakeCodeDirectiveMarkersWithOpenBrace' -v minimal`：**3/3 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~JazorVue_Parser_SkipsDirectiveWithoutBlockBodyBeforeRealCodeBlock|FullyQualifiedName~JazorLaneService_GetSemanticTokens_ReturnsCodeDirectiveTokenForEachRealCodeBlock|FullyQualifiedName~BuiltinStructureDiagnosticProvider_ReportsMultipleRealCodeBlocks' -v minimal`：**3/3 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~JazorVue_Parser_IgnoresModuleDirectivesInsideCommentsAndCodeBlocks|FullyQualifiedName~JazorVueAnalysisService_AnalyzeJazor_IgnoresLegacyImportDirectivesInsideCommentsAndCodeStrings|FullyQualifiedName~JazorLaneService_GetSemanticTokens_IgnoresFakeModuleDirectivesInsideCommentsAndCodeBlocks|FullyQualifiedName~Jolt_Lsp_DocumentLink_IgnoresFakeModuleDirectivesInsideCodeBlocks' -v minimal`：**4/4 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~BuiltinComponentCodeActionProvider_OffersImportQuickFixThroughLspSession|FullyQualifiedName~BuiltinComponentCodeActionProvider_OffersImportQuickFix_WhenExistingImportOnlyMatchesSourcePath|FullyQualifiedName~BuiltinComponentCodeActionProvider_OffersImportQuickFix_WhenExistingImportOnlyMatchesImportedNameBeforeAlias|FullyQualifiedName~BuiltinComponentCodeActionProvider_DoesNotOfferImportQuickFix_WhenComponentAlreadyImportedByLocalBinding' -v minimal`：**4/4 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-restore --filter 'FullyQualifiedName~Jolt_Lsp_WithDevMode_AfterServingHttpRequest_ShutdownKeepsLspStreamClean|FullyQualifiedName~Jolt_Lsp_WithDevMode_WhenUnsavedVueDidChange_BroadcastsHmrJavaScriptUpdate' -v minimal`：**2/2 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-build --no-restore --filter 'FullyQualifiedName~JazorVueCompilerTests|FullyQualifiedName~JazorVueAnalysisRuntimeTests|FullyQualifiedName~JoltPhase6LspTests|FullyQualifiedName~JoltLspTests|FullyQualifiedName~JoltPhase7ExtensionSecurityAndBuiltinTests|FullyQualifiedName~JoltBuildTests' -v minimal`：**262/262 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-build --no-restore --filter 'FullyQualifiedName~JazorVueCompilerTests|FullyQualifiedName~JoltPhase6LspTests|FullyQualifiedName~JoltLspTests|FullyQualifiedName~JoltLaneRoutingTests|FullyQualifiedName~JoltPhase7ExtensionSecurityAndBuiltinTests|FullyQualifiedName~JoltInProcRoslynTests' -v minimal`：**234/234 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --no-build --no-restore --filter 'FullyQualifiedName~JoltPhase6LspTests|FullyQualifiedName~JoltPhase7ExtensionSecurityAndBuiltinTests' -v minimal`：**100/100 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --filter 'FullyQualifiedName~JoltMarkupComponentBridgeTests' --no-restore --disable-build-servers -v minimal`：**8/8 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --filter 'FullyQualifiedName~JoltMarkupComponentBridgeTests|FullyQualifiedName~JoltPhase6LspTests|FullyQualifiedName~JoltLspTests' --no-restore --disable-build-servers -m:1 -v minimal`：**134/134 通过**
- `dotnet test src/Jolt.Test/Jolt.Test.csproj --filter 'FullyQualifiedName~JoltStdioLspServerTests|FullyQualifiedName~Jolt_Lsp_CancelRequest_WithUnknownId_DoesNotBreakSubsequentRequests' --no-restore --disable-build-servers -m:1 -v minimal`：**7/7 通过**
- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --no-restore --disable-build-servers -m:1 -v minimal`：**2391/2391 通过**

## 近期推进信号（截至 2026-04-21，第四轮收口）

- 最近一周 `jolt` 相关提交持续高频，且覆盖 feature/refactor/test/docs 四类
- 最新提交聚焦在：
  - `@code`/`@functions` 与 `@module`/`@import` 指令定位逻辑提取为独立共享定位器 `JazorCodeDirectiveLocator` + `JazorImportDirectiveLocator`
  - `JazorVueParser` 大幅精简（接入共享定位器），`DocumentRegionClassifier` 显著精简至 265 行
  - `MarkupComponentBridgeService` 扩展支持 script-import bridge（组件引用自动发现与 `@module` 快速修复）
  - 新增 1,236 行测试代码（+40 个测试方法），覆盖组件桥接、扩展安全、Phase6/7 LSP 回归
  - `.jazor` 智能感知阶段对标准 Razor 的直接复用继续收口
  - `--lsp --dev` 下 ASP.NET hosting 日志已被抑制
- 第四轮复评遗留问题已全部闭环：
  - `JazorVueParser` 残余扫描实现已删除，重复代码问题清零
  - `Path.GetDirectoryName()!` 的 6 处风险点已全部改为安全目录解析 helper
  - `RangesEqual` 与 `TryFindCodeDirective` 死代码已移除，`ComponentTagPattern` 已改为共享定义

## 下一步行动

1. **稳态巡检（持续项）**
   - 扩展真实浏览器/CDP 长时压测矩阵
   - 持续补调试可视化与诊断端点

2. **构建持续巩固（持续项）**
   - 继续巩固构建产物一致性（manifest/css/js/source map）
   - 继续观察 Deno worker 隔离 workspace 长时行为

3. **生态拓展（后续项）**
   - 在最小 VS Code 集成骨架基础上补 LanguageClient 传输层与发布链路
   - 继续细化扩展权限与沙箱策略

## 风险与注意项

- 文档与实现迭代速度不一致时，最容易造成阶段误读；应优先维护 repo-level 状态页
- 需持续防止"临时 fallback 重新进入主路径"，保持前置硬化优先
- 构建与 LSP 并行演进时，需持续防止"测试路径与真实路径分叉"
- 需持续区分 compile 与 LSP 的职责边界，避免把编译期定制错误地下沉到智能感知阶段
- 当前结论只覆盖 Windows、本机开发、受控内网调试和正式构建；不覆盖公网服务、多租户或不可信代码场景
