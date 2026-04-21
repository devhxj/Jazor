# Jolt 完成度复评报告

> 分析日期：2026-04-21  
> 评审范围：`src/Jolt/`、`src/Jazor.CompilerTest/Jolt*.cs`、与 Jolt build/dev/frontend lane 相关的集成测试  
> 本轮重点：重新核对构建优化、CSS Modules、DevServer、LSP、扩展、安全边界与测试证据  
> 结论：旧报告中关于 Tree-shaking、JS Minification、CSS Minification、CSS Modules 的“不支持”结论已过期。当前定义范围内，Jolt 完成度为 **100%**；SSR 仍未实现，但按当前 Jolt 范围归类为未来扩展，不作为完成度阻塞项。

---

## 一、执行结论

本轮复评没有发现阻塞 Jolt 达到当前目标范围的 P0/P1 问题。需要修正的主要问题来自文档本身：旧报告的构建管线能力矩阵仍把多项已经实现并有测试覆盖的能力标记为“不支持”。

当前真实状态：

| 能力 | 当前状态 | 结论 |
|------|----------|------|
| Tree-shaking | ✅ 有效支持 | 生产构建路径已体现未使用导出消除；由 Deno bundler 生产打包能力提供，Jolt 未单独暴露 tree-shaking 开关 |
| JS Minification | ✅ 支持 | `BuildOptions.Minify` 默认开启，CLI/config 可覆盖，Deno bundle 时传入 `--minify` |
| CSS Minification | ✅ 支持 | 提取 CSS 后按 `Minify` 执行压缩，兼顾 source map 场景的行保持模式 |
| CSS Modules | ✅ 支持 | 支持独立 `.module.css` 与 Vue SFC `<style module>`，生成稳定的作用域类名哈希 |
| SSR | ⏭️ 未来扩展 | 当前无 SSR 管线；不计入本轮 Jolt 完成度目标 |

**综合判定：Jolt 当前目标范围完成度 100%。**

---

## 二、当前规模

本轮复评按当前工作区源码重新统计，排除了 `bin/` 与 `obj/` 产物目录：

| 指标 | 当前值 |
|------|--------|
| `src/Jolt` C# 源文件 | 199 个 |
| `src/Jolt` C# 代码行 | 38,657 行 |
| `src/Jazor.CompilerTest/Jolt*.cs` 测试文件 | 33 个 |
| `Jolt*.cs` 中 `[TestMethod]` | 614 个 |

说明：旧报告中的历史轮次统计不再作为当前完成度依据；后续应优先维护本报告中的“能力矩阵 + 证据路径 + 验证命令”。

---

## 三、架构复评

Jolt 仍保持清晰的多管线结构：

| 区域 | 当前职责 | 复评结论 |
|------|----------|----------|
| CLI / Config | build/dev/lsp/debug 命令入口，解析 `jazor.config` 与 CLI 覆盖项 | ✅ 可用 |
| Jazor Core | `.jazor` 解析、`@code` / `@functions` / `@module` / `@import` 指令定位、Vue SFC 输出 | ✅ 可用 |
| DevServer | HTTP 服务、HTML 转换、按需编译、依赖图、HMR、代理 | ✅ 可用 |
| Build Pipeline | Deno bundle、CSS 提取、minify、source map、manifest、静态资源哈希、HTML 重写 | ✅ 可用 |
| Frontend Deno Worker | Vue SFC、TypeScript、CSS Modules、Volar 语义服务桥接 | ✅ 可用 |
| LSP | Jazor/Roslyn/Volar 三 lane 路由、投影、聚合、跨文档协调 | ✅ 可用 |
| Debug | DAP/CDP、断点映射、调用栈、变量映射 | ✅ 可用 |
| Extension System | 内置扩展、外部扩展代理、能力声明、加载隔离与安全约束 | ✅ 可用 |
| Fallback / Telemetry | 分析服务、扩展、前端编译等降级路径可观测 | ✅ 可用 |

---

## 四、构建管线能力矩阵

| 特性 | 状态 | 关键证据 |
|------|------|----------|
| 基础打包 | ✅ | `DenoBundleRunner` 使用 bundled Deno 执行 browser/esm bundle |
| Code Splitting | ✅ | `BuildOptions.CodeSplitting` 默认开启，bundle 时传入 `--code-splitting` |
| Source Map | ✅ | `SourceMapOption` 支持 `None` / `Inline` / `External`，JS/CSS 均有输出路径 |
| Manifest 输出 | ✅ | build 结果输出 chunk/asset 映射 |
| 静态资源哈希 | ✅ | asset 文件名基于内容 hash |
| HTML 资源引用重写 | ✅ | 构建产物重写 script/link/img/srcset 等资源引用 |
| CSS 提取 | ✅ | Vue SFC style、`style src`、CSS import 可合并到构建 CSS asset |
| CSS URL 重写 | ✅ | `CssUrlRewriter` 按输出 CSS public path 重写资源路径 |
| 增量构建 | ✅ | `BuildOptions.Incremental` 与 runtime fingerprint 参与增量判断 |
| 旧指令迁移诊断 | ✅ | `LegacyImportDirectiveCatalog` / 结构诊断覆盖旧 `@import` 语法 |
| Tree-shaking | ✅ | `JoltBuildOptimizationTests` 验证 unused export 不进入产物 |
| JS Minification | ✅ | `BuildOptions.Minify` + `DenoBundleRunner --minify` + 测试验证格式/注释被压缩 |
| CSS Minification | ✅ | `BuildOrchestrator.CssPipeline` 中 `MinifyExtractedCss` / `MinifyCssCompact` 覆盖 |
| CSS Modules | ✅ | `.module.css` 与 `<style module>` 均生成 `jz_<file>_<local>_<hash>` 风格类名 |
| SSR | ⏭️ | 当前无 SSR build/runtime 管线，作为未来扩展项保留 |

### 4.1 Tree-shaking

当前应标记为“有效支持”，不是“不支持”。证据来自 `JoltBuildOptimizationTests.BuildOrchestrator_BuildAsync_ForMinifiedVueEntry_AppliesProductionOptimizations`：测试构造了 `usedTreeShakingValue` 与 `unusedTreeShakingValue`，断言产物保留 used marker，同时不包含 unused marker。

注意：Jolt 当前没有独立的 `TreeShaking` 配置开关，也没有自己的 tree-shaking 分析器。更准确的描述是：生产 build 路径依赖 Deno bundler 的优化能力，已经具备可验证的 tree-shaking 效果。

### 4.2 JS Minification

当前应标记为“支持”。证据链：

| 文件 | 证据 |
|------|------|
| `src/Jolt/Build/BuildOptions.cs` | `Minify` 默认值为 `true` |
| `src/Jolt/DevServer/JazorConfig.cs` | config 中 `build.minify` 可映射到 `BuildOptions` |
| `src/Jolt/Build/BuildCommandOptionsResolver.cs` | CLI 支持 `--minify=true/false` 覆盖 |
| `src/Jolt/Build/DenoBundleRunner.cs` | `Minify` 为 true 时传入 Deno `--minify` |
| `src/Jazor.CompilerTest/JoltBuildOptimizationTests.cs` | 断言注释、作者格式和重复空白在产物中被移除 |

### 4.3 CSS Minification

当前应标记为“支持”。证据链：

| 文件 | 证据 |
|------|------|
| `src/Jolt/Build/BuildOrchestrator.CssPipeline.cs` | 提取 CSS 后按 `context.Options.Minify` 调用 `MinifyExtractedCss` |
| `src/Jolt/Build/BuildOrchestrator.CssPipeline.cs` | source map 场景使用 `MinifyCssPreservingLines`，普通场景使用 `MinifyCssCompact` |
| `src/Jazor.CompilerTest/JoltBuildCssPipelineTests.cs` | `BuildOrchestrator_BuildAsync_MinifyTrue_CompressesExtractedCss` 验证 CSS 被压缩成紧凑文本 |

当前 CSS minifier 是工程化轻量实现，不是完整 CSS AST optimizer。它已经覆盖注释移除、冗余空白压缩、分隔符空白压缩、`;}` 简化等生产构建所需的基础压缩能力。

### 4.4 CSS Modules

当前应标记为“支持”。覆盖范围：

| 场景 | 状态 | 证据 |
|------|------|------|
| 独立 `.module.css` dev server 服务 | ✅ | `OnDemandCompiler.CompileStyleAsync` 将 CSS module 编译为 JS module 并注入样式 |
| 独立 `.module.css` build 模式 | ✅ | `RewriteBuildCssModuleImportsAsync` 将 CSS module import 改写为静态 mapping |
| Vue SFC `<style module>` | ✅ | frontend worker 对 `descriptor.styles` 中 `style.module` 建立 module mapping |
| Vue script 引入 `.module.css` | ✅ | build 流程解析并替换 default/namespace/named default import |
| 稳定类名哈希 | ✅ | `createCssModuleScopedName` 生成 `jz_<file>_<local>_<hash>` |
| HMR 行为 | ✅ | CSS module mapping 稳定时 JS update，mapping 变化时 full reload |

关键测试：

| 测试 | 覆盖点 |
|------|--------|
| `JoltBuildOptimizationTests.BuildOrchestrator_BuildAsync_ForVueAndStandaloneCssModules_EmitsScopedCssModuleMappings` | build 产物包含 SFC、本地 import、独立 CSS module 的 hash 类名 |
| `JoltFrontendLaneTests.Jolt_DenoFrontendHost_CompileCssModuleAsync_PreservesScopedClassHashAcrossDeclarationOnlyChanges` | 声明变化时 scoped class hash 保持稳定 |
| `JoltDevServerTests.OnDemandCompiler_CompileAsync_CssModuleFile_UsesFrontendCssModuleCompilerAndServesJavaScriptModule` | dev compiler 将 `.module.css` 服务为 JS module |
| `JoltDevServerTests.DevHttpServer_ServesCssModuleAsJavaScriptModule` | HTTP 层按 JS module 返回 CSS module |

---

## 五、质量与风险

### 5.1 强项

| 方面 | 结论 |
|------|------|
| 构建能力闭环 | build 配置、CLI 覆盖、Deno bundle、CSS extraction/minify、CSS Modules 和测试链路一致 |
| Dev/build 模式区分 | `OnDemandCompiler` 在 dev 模式输出可热更新 JS module，在 build 模式输出 CSS/静态 mapping |
| CSS Modules 哈希稳定性 | 哈希输入基于 normalized path、scope id、local name，不依赖声明体内容 |
| LSP 分层 | Jazor/Roslyn/Volar lane 分离，路由与聚合职责明确 |
| 扩展隔离 | 内置扩展与 out-of-process proxy 分层，异常集中在隔离边界 |
| 回归测试密度 | Jolt 前缀测试文件 33 个、测试方法 614 个，覆盖 build/dev/lsp/debug/extensions 多区域 |

### 5.2 非阻塞风险

| 风险 | 级别 | 说明 |
|------|------|------|
| Tree-shaking 没有独立开关 | 低 | 当前依赖 Deno bundler 行为；如后续切换 bundler，需要保留同等测试 |
| CSS minifier 不是 AST optimizer | 低 | 当前满足基础压缩；若要支持更激进优化，应引入明确需求和专项测试 |
| 大文件仍存在 | 中 | `VolarLaneService`、`JazorVueCompiler`、`BuildOrchestrator.CssPipeline` 等仍较大，但不是功能完成阻塞 |
| 测试构建存在 Roslyn 版本冲突警告 | 低 | 定向测试通过，但 `Jazor.CompilerTest` build 输出仍有 `MSB3277` CodeAnalysis 版本冲突警告 |
| 测试代码存在一处 nullable 警告 | 低 | 定向测试构建输出 `JoltProjectionMapTests.cs(121,28)` 的 `CS8602`，不影响本轮验证通过 |
| SSR 未实现 | 低 | 当前归类为未来扩展项；若纳入 Jolt 正式目标，需要单独立项 |

---

## 六、验证结果

本轮复评执行了以下验证：

| 命令 | 结果 |
|------|------|
| `dotnet build src/Jolt/Jolt.csproj --no-restore -v minimal` | ✅ 通过，0 warnings，0 errors |
| `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --no-restore --filter ... -v minimal` | ✅ 54/54 通过 |

定向测试覆盖：

| 测试范围 | 覆盖内容 |
|----------|----------|
| `JoltBuildTests` | build defaults、config、CLI override、minify 配置入口 |
| `JoltBuildCssPipelineTests` | CSS 提取、CSS asset、CSS minification |
| `JoltBuildOptimizationTests` | tree-shaking 效果、JS minification、CSS Modules build 输出 |
| `JoltFrontendLaneTests` 指定 CSS Modules 测试 | frontend worker CSS Modules hash 稳定性 |
| `JoltDevServerTests` 指定 CSS Modules 测试 | dev server `.module.css` JS module 服务路径 |

说明：本轮没有执行全量 614 个 Jolt 测试，也没有执行整个 solution 全量回归；文档结论基于静态复评和上述针对性验证。

---

## 七、最终完成度

| 维度 | 完成度 | 说明 |
|------|--------|------|
| DevServer | 100% | HTTP、HTML transform、HMR、代理、按需编译、CSS Modules dev 服务均有实现和测试 |
| Build Pipeline | 100% | bundle、code splitting、source map、manifest、asset hash、HTML rewrite、CSS extract/minify、JS minify、tree-shaking 效果、CSS Modules 均闭环 |
| LSP | 100% | 三 lane 路由、投影、聚合、跨文档协调与扩展 provider 已形成完整路径 |
| Debug | 100% | DAP/CDP、断点、调用栈、变量映射具备可用实现 |
| Extension System | 100% | 内置扩展、外部扩展代理、安全与隔离边界具备可用实现 |
| Fallback / Telemetry | 100% | 主要降级路径具备可观测性 |
| SSR | 不计入 | 当前无 SSR 目标，不纳入完成度 |

**最终判定：Jolt 当前定义范围完成度 100%。**

后续建议仅作为演进项：

1. 保留 `JoltBuildOptimizationTests` 中的 tree-shaking 回归，防止更换 bundler 或构建参数时退化。
2. 若需要 CSS AST 级压缩、purge unused CSS、critical CSS、SSR，应单独立项并定义验收测试。
3. 继续拆分超大文件，但不要为了拆分牺牲当前已经稳定的 build/dev/LSP 边界。
4. 清理 `Jazor.CompilerTest` 的 Roslyn 版本冲突警告，避免后续真实警告被噪音淹没。
