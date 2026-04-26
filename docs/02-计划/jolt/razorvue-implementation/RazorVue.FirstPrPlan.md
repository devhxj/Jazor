# RazorVue 首次 PR 计划

> Status: 历史资料
> Positioning: 已归档的首次 PR 规划切片，用于早期 RazorVue 推进。
> Note: 保留本文档作为规划上下文和排序历史；请使用较新的实施状态文档了解当前进展。

本文档将 RazorVue 实现骨架转化为首次交付计划。

本计划有意收窄范围。
它聚焦于应该首先落地的实现通道，且不会破坏现有的静态模块路径。

相关文档：

- [RazorVue.DecisionSummary.md](../../../01-目标/razorvue/design/RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](../../../01-目标/razorvue/design/RazorVue.Design.md)
- [RazorVue.HardRules.md](../../../01-目标/razorvue/design/RazorVue.HardRules.md)
- [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)

## 1. 交付目标

首次 PR 系列只需证明以下三点：

1. 仓库能够识别 RazorVue 入口
2. RazorVue 诊断不会破坏当前的静态模块路径
3. 实现有一个稳定的生长起点

它不应尝试渲染树提取或 Vue 代码生成。

## 2. PR 策略

首次交付通道应拆分为三个小型 PR。

### PR1. 基础类型和分析器模式拆分

目标：

- 引入最小类型系统和诊断外壳
- 将静态模块分析与 RazorVue 分析拆分开来

### PR2. 入口分类和描述符外壳

目标：

- 证明 RazorVue 组件能被发现和分类
- 引入描述符/快照占位符，暂不进行 lowering

### PR3. 发射过渡外壳

目标：

- 添加面向宿主的载体占位符，使后续 Vue 产物有真正的目的地

## 3. PR1 范围

PR1 应仅包含以下生产文件。

### 3.1 `src/Jazor.Razor`

- `JazorComponent.cs`

### 3.2 `src/Jazor.RazorVue`

- `VueComponent.cs`
 
### 3.3 `src/Jazor.Compiler`
- `RazorVue/RazorVueCompilationSymbols.cs`
- `RazorVue/RazorVueEntryKind.cs`

### 3.4 `src/Jazor.Analyzer`

- `RazorVue/RazorVueDiagnosticDescriptors.cs`
- `RazorVue/RazorVueKnownSymbols.cs`
- `RazorVue/RazorVueEntryAnalyzer.cs`
- `RazorVue/RazorVueMisuseAnalyzer.cs`

### 3.5 预计需要修改的现有文件

- `src/Jazor.Razor/Jazor.Razor.csproj`
- `src/Jazor.RazorVue/Jazor.RazorVue.csproj`
- `src/Jazor.RazorVue.Analysis/Jazor.RazorVue.Analysis.csproj`
- `src/Jazor.Analyzer/Analyzer.cs`
- `src/Jazor.Analyzer/AnalyzerReleases.Unshipped.md`
- 可选 `Jazor.slnx`

## 4. PR1 非目标

PR1 不得包含：

- `BuildRenderTree` 提取
- `VueComponentDescriptorFactory`
- lowering 模型
- `DenoHost` 集成变更
- 依赖 `.razor` fixture 的端到端测试

如果 PR1 开始引入这些内容，说明它已经过大了。

## 5. PR1 具体任务

### 5.1 添加基础组件类型

创建：

- `JazorComponent : ComponentBase`
- `VueComponent : JazorComponent`

规则：

- `JazorComponent` 位于 `src/Jazor.Razor`
- `VueComponent` 位于 `src/Jazor.RazorVue`
- `JazorComponent` 保持精简
- `VueComponent` 在 PR1 中可以是几乎空的结构
- 第一阶段的辅助 API 不需要完整实现

### 5.2 拆分分析器模式

重构 `Analyzer.cs`，使现有 ECMAScript 分析器路径不会自动接管 RazorVue 类。

要求行为：

- `[ECMAScriptModule] static class` 继续走遗留规则路径
- `[ECMAScriptModule]` + `JazorComponent` 后代走 RazorVue 规则路径
- 直接 `ComponentBase` 入口产生 RazorVue 诊断

### 5.3 为 RazorVue 专项检查启用生成代码分析

PR1 不需要完整的生成 Razor 提取，
但分析器层面必须停止结构性阻塞该未来路径。

要求结果：

- 在 RazorVue 需要的地方启用生成代码分析
- 实现结构上保证未来基于生成代码的发现不需要再次重写分析器

### 5.4 添加首批诊断集

PR1 应仅包含以下 ID：

- `JAZORVUE001` 无效的 RazorVue 入口继承
- `JAZORVUE002` 不允许直接使用 `ComponentBase` 入口
- `JAZORVUE004` `StateHasChanged` 不属于 RazorVue 语义
- `JAZORVUE005` `ShouldRender` 不属于 RazorVue 语义
- `JAZORVUE006` `SetParametersAsync` 不属于 RazorVue 语义

不要在 PR1 中添加歧义或描述符诊断。

## 6. PR1 测试计划

PR1 应添加一个新测试文件：

- `src/Jazor.CompilerTest/RazorVueAnalyzerTests.cs`

建议的首批测试：

- `RazorVue_Entry_ValidVueComponent_IsAccepted`
- `RazorVue_Entry_ComponentBaseOnly_ReportsJAZORVUE002`
- `RazorVue_Entry_StaticModule_RemainsOnLegacyPath`
- `RazorVue_Misuse_StateHasChanged_ReportsJAZORVUE004`
- `RazorVue_Misuse_ShouldRender_ReportsJAZORVUE005`
- `RazorVue_Misuse_SetParametersAsync_ReportsJAZORVUE006`

建议的 fixture 风格：

- 先使用内联 C# 编译测试
- 除非绝对必要，不要在 PR1 测试中引入 `.razor` 文件

## 7. PR1 验收门

PR1 仅在以下条件全部满足时才算完成：

1. `JazorComponent` 和 `VueComponent` 能够编译
2. 现有静态模块分析器行为仍然正常工作
3. RazorVue 入口诊断通过专用 ID 报告
4. 误用诊断使用专用 RazorVue ID
5. `RazorVueAnalyzerTests.cs` 覆盖入口拆分和误用外壳

## 8. PR2 范围

PR2 只能在 PR1 全部通过后开始。

需要引入的生产文件：

- `src/Jazor.RazorVue.Analysis/RazorVue/RazorVueCompilationContext.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/RazorVueComponentCandidate.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/VueComponentDescriptor.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/VuePropDescriptor.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/VueEmitDescriptor.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/VueSlotDescriptor.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/RazorVueSemanticSnapshot.cs`

PR2 仍应在渲染树提取之前停止。

PR2 测试文件：

- `RazorVueDescriptorExtractionTests.cs`

PR2 证明：

- 描述符外壳存在
- 入口候选可以成为编译器拥有的快照

## 9. PR3 范围

PR3 应仅引入宿主过渡外壳，而非最终 lowering。

需要引入的生产文件：

- `src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/VueCompiledArtifact.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/RazorVueCatalog.cs`
- `src/Jazor.Emit/RazorVueCatalogReader.cs`
- `src/Jazor.Emit/RazorVueManifestModel.cs`

PR3 证明：

- 新载体在 `Jazor.Emit` 中有了归属
- 与当前 `ModuleCatalog` 的过渡是显式的

## 10. 建议的命令级验证

PR1 验证目标：

```powershell
$env:DOTNET_CLI_HOME='D:\repository\own\jazor\Jazor\.dotnet'; $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; dotnet test 'src/Jazor.CompilerTest/Jazor.CompilerTest.csproj' --filter 'FullyQualifiedName~RazorVueAnalyzerTests' -v minimal
```

PR2 验证目标：

```powershell
$env:DOTNET_CLI_HOME='D:\repository\own\jazor\Jazor\.dotnet'; $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; dotnet test 'src/Jazor.CompilerTest/Jazor.CompilerTest.csproj' --filter 'FullyQualifiedName~RazorVueAnalyzerTests|FullyQualifiedName~RazorVueDescriptorExtractionTests' -v minimal
```

PR3 验证目标：

```powershell
$env:DOTNET_CLI_HOME='D:\repository\own\jazor\Jazor\.dotnet'; $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; dotnet test 'src/Jazor.CompilerTest/Jazor.CompilerTest.csproj' --filter 'FullyQualifiedName~RazorVue' -v minimal
```

## 11. 首次 PR 系列的评审清单

首次系列中的每个 PR 都应对照以下问题进行评审：

1. 是否保留了静态模块路径
2. 是否将 RazorVue 类型保持在独立的实现通道上
3. 是否避免了过早触及渲染树/lowering 工作
4. 是否仅添加了该 PR 所需的诊断
5. 是否比之前留下了更清晰的下一步着陆点

## 12. 结论

首次 PR 系列应证明 RazorVue 可以安全地进入仓库。

它不应试图证明整个编译器。
正确的首个成果是：

- 入口拆分
- 专用诊断
- 显式类型骨架
- 显式宿主过渡通道
