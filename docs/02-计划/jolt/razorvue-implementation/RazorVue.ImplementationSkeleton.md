# RazorVue 实现骨架

> Status: 活跃计划
> Positioning: RazorVue 第一阶段实现的仓库级骨架。
> Note: 这是一个用于建立所有权、类型和着陆顺序的阶段产物，不代表完整管线已经实现。

本文档将 RazorVue 设计映射为具体的仓库级实现切片。

它不重新定义架构。
它的存在是为了在编码开始之前回答四个实际问题：

1. 哪个项目拥有每个阶段
2. 哪些文件和类型应该首先存在
3. 哪些诊断应该首先引入
4. 哪些测试应该首先编写

相关文档：

- [RazorVue.DecisionSummary.md](../../../01-目标/razorvue/design/RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](../../../01-目标/razorvue/design/RazorVue.Design.md)
- [RazorVue.ComponentDescriptorSpec.md](../../../01-目标/razorvue/design/RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](../../../01-目标/razorvue/design/RazorVue.DenoHostContract.md)
- [RazorVue.HardRules.md](../../../01-目标/razorvue/design/RazorVue.HardRules.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.RazorIrMigrationPlan.md](./RazorVue.RazorIrMigrationPlan.md)

## 1. 项目所有权

第一阶段应按以下方式在现有项目之间划分所有权。

### 1.1 `src/Jazor.Analyzer`

拥有：

- RazorVue 入口发现诊断
- RazorVue 误用诊断
- 生成代码分析启用
- RazorVue 相关类型的编译符号缓存

不拥有：

- 渲染树提取
- Razor SDK / `RazorCodeDocument` / IR 宿主所有权
- Vue lowering
- 面向宿主的 catalog/物化

### 1.2 `src/Jazor.Common`

拥有：

- 中性的 RazorVue 符号/上下文模型
- 语义快照提取
- 组件描述符提取
- 逻辑提取
- 过渡模板前端提取
- Vue lowering
- RazorVue catalog/物化模型

不拥有：

- `ComponentBase` 派生的编写基类
- 最终的宿主依赖解析
- 打包

### 1.3 `src/Jazor.RazorVue`

拥有：

- `VueComponent`
- Vue 优先的编写表面
- 仅 net10 的依赖边界，用于面向 Vue 的入口类型

不拥有：

- `JazorComponent`
- 静态模块 lowering
- 已由 RazorVue 核心实现层拥有的通用基础设施

### 1.4 `src/Jazor.Razor`

拥有：

- `JazorComponent`
- 面向 Razor/AspNetCore 的前端组件入口基板
- 仅 net10 的依赖边界，用于 Razor 入口类型

不拥有：

- Vue 优先的编写 API
- 静态模块 lowering

### 1.5 RazorVue 专用 Razor SDK/toolset 宿主层

拥有：

- Razor SDK / toolset 对齐的 `RazorCodeDocument` 获取能力
- 对 Razor compiler / source mappings / toolset 版本漂移的宿主隔离
- 需要 SDK 私有访问时的最小爆炸半径

不拥有：

- RazorVue descriptor / lifecycle / setup / artifact 语义
- 最终的 RazorVue generator catalog 输出

### 1.6 `src/Jazor.RazorVue.Analysis`

拥有：

- RazorVue 生成器/分析器面向的入口
- RazorVue 路由的 Razor 特定生成代码发现
- 从 `[ECMAScriptModule]` Razor 组件到编译器拥有的 lowering/产物模型的接线

不拥有：

- `JazorComponent`
- `VueComponent`
- 已由 RazorVue 核心实现层拥有的通用核心契约
- 直接拥有 Razor SDK phase / `RazorCodeDocument` 生命周期

### 1.7 `src/Jazor.Emit`

拥有：

- 已发射的编译器拥有载体的 catalog 读取
- 清单持久化格式更新
- `ModuleCatalog` 加 `RazorVueCatalog` 的过渡支持
- `DenoHost` 交接

### 1.8 `src/Jazor.RazorVue.Test`

拥有：

- 发现/诊断测试
- 提取测试
- lowering 测试
- 产物/catalog 测试
- Razor IR/template frontend parity 测试

第一阶段的大部分 RazorVue 验证应保留在现有 RazorVue 测试面中，而不是把 Razor SDK/frontend 迁移测试挪到无关测试项目。

## 2. 建议的目录结构

第一阶段应避免将 RazorVue 类型分散到不相关的文件中。

建议的 RazorVue 核心侧布局：

```text
src/Jazor.Common/
  RazorVue/
    RazorVueCompilationSymbols.cs
    RazorVueCompilationContext.cs
    RazorVueComponentCandidate.cs
    RazorVuePipeline.cs
    Discovery/
      RazorVueEntryClassifier.cs
    Descriptor/
      VueComponentDescriptorFactory.cs
      VueComponentRegistryBuilder.cs
    Logic/
      RazorVueLogicExtractor.cs
    RenderTree/
      RazorVueRenderTreeExtractor.cs
      RazorVueRenderTreeBuilderPatterns.cs
    IrExtraction/
      RazorVueRazorDocumentProvider.cs
      RazorVueIrTemplateExtractor.cs
      RazorVueIrParityComparer.cs
    Lowering/
      RazorVueLoweringContext.cs
      VueComponentLowerer.cs
      VueRenderFunctionEmitter.cs
    Artifacts/
      RazorVueSemanticSnapshot.cs
      VueCompiledArtifact.cs
      RazorVueCatalog.cs
      RazorVueManifest.cs
      RazorVueSourceOrigin.cs

src/Jazor.RazorVue/
  VueComponent.cs

src/Jazor.Razor/
  JazorComponent.cs

src/Jazor.RazorVue.Analysis/
  RazorVue/Generation/RazorVueGenerator.cs
```

建议的分析器侧布局：

```text
src/Jazor.Analyzer/
  RazorVue/
    RazorVueKnownSymbols.cs
    RazorVueEntryAnalyzer.cs
    RazorVueMisuseAnalyzer.cs
    RazorVueDiagnosticDescriptors.cs
```

建议的发射侧布局：

```text
src/Jazor.Emit/
  RazorVueCatalogReader.cs
  RazorVueManifestModel.cs
```

第一阶段不要求第一天就有上面每个文件。
它确实要求它们所代表的所有权边界。

## 3. 首批具体类型

以下类型是值得首先定义的最小实现骨架。

### 3.1 编译器上下文

```csharp
public sealed record RazorVueCompilationSymbols(
    INamedTypeSymbol ECMAScriptModuleAttribute,
    INamedTypeSymbol JazorComponent,
    INamedTypeSymbol VueComponent,
    INamedTypeSymbol ComponentBase,
    INamedTypeSymbol ParameterAttribute);
```

```csharp
public sealed record RazorVueCompilationContext(
    Compilation Compilation,
    RazorVueCompilationSymbols Symbols);
```

### 3.2 入口分类

```csharp
public enum RazorVueEntryKind
{
    None,
    StaticModule,
    RazorVueComponent,
    Invalid
}
```

```csharp
public sealed record RazorVueComponentCandidate(
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol? BuildRenderTreeMethod,
    RazorVueEntryKind EntryKind);
```

### 3.3 快照和产物类型

```csharp
public sealed record RazorVueSemanticSnapshot(
    INamedTypeSymbol ComponentSymbol,
    VueComponentDescriptor Descriptor,
    RazorVueLogicModel Logic,
    RazorRenderTreeNode RenderTree,
    ImmutableArray<RazorVueSourceOrigin> Origins);
```

```csharp
public sealed record VueCompiledArtifact(
    string ComponentName,
    string RelativeModulePath,
    string ModuleCode,
    ImmutableArray<string> Imports,
    ImmutableArray<string> Styles,
    VueArtifactIdentity Identity,
    VueRuntimeHints Hints);
```

```csharp
public sealed record RazorVueCatalog(
    string AssemblyName,
    ImmutableArray<VueCompiledArtifact> Artifacts);
```

## 4. 首批编译器入口表面

第一阶段不应从修改当前 `AstConverter` 或 `SemanticWalker` 开始。

建议的首个编译器入口类型：

```csharp
public sealed class RazorVuePipeline
{
    public RazorVueCatalog Execute(RazorVueCompilationContext context);
}
```

建议的首个内部调用顺序：

1. `RazorVueEntryClassifier`
2. `VueComponentDescriptorFactory`
3. `RazorVueLogicExtractor`
4. `RazorRenderTreeExtractor`
5. `VueComponentLowerer`
6. `VueRenderFunctionEmitter`
7. catalog/物化

这将新路径与当前静态模块路径保持分离。
当模板前端迁移到 `RazorCodeDocument` / Razor IR 时，也应首先替换 `RenderTree/IrExtraction` 层，而不是直接重写全部 lowering 和 artifact 结构。

## 5. 分析器拆分

当前分析器是一个大型规则表面。
第一阶段 RazorVue 不应在该形状中添加更多不相关的逻辑。

建议的拆分：

### 5.1 `RazorVueKnownSymbols`

职责：

- 缓存 `ECMAScriptModuleAttribute`
- 缓存 `JazorComponent`
- 缓存 `VueComponent`
- 缓存 `ComponentBase`
- 缓存诊断使用的常见 Razor/Blazor 符号

### 5.2 `RazorVueEntryAnalyzer`

职责：

- 识别 `[ECMAScriptModule]` Razor 组件候选
- 验证 `JazorComponent` 继承
- 区分静态模块入口和 RazorVue 入口

### 5.3 `RazorVueMisuseAnalyzer`

职责：

- 诊断 `StateHasChanged`
- 诊断 `ShouldRender`
- 诊断 `SetParametersAsync`
- 诊断应在 lowering 之前失败的歧义倾向无效模式

## 6. 诊断计划

第一阶段应预留专用诊断范围，而非复用现有通用分析器错误。

建议的 ID：

- `JAZORVUE001` 无效的 RazorVue 入口继承
- `JAZORVUE002` 不允许直接使用 `ComponentBase` 入口
- `JAZORVUE003` RazorVue 提取的生成代码分析不可用
- `JAZORVUE004` `StateHasChanged` 不属于 RazorVue 语义
- `JAZORVUE005` `ShouldRender` 不属于 RazorVue 语义
- `JAZORVUE006` `SetParametersAsync` 不属于 RazorVue 语义
- `JAZORVUE007` 歧义组件名称
- `JAZORVUE008` 内部组件名称冲突
- `JAZORVUE009` 未知组件 prop
- `JAZORVUE010` 不支持的第一阶段 RazorVue 语法形状

第一阶段应保持初始集合较小。
在第一个循环闭合之前不要设计 30 个诊断。

## 7. 首批测试文件

建议在 `src/Jazor.RazorVue.Test/` 中的首批测试文件：

- `RazorVueAnalyzerTests.cs`
- `RazorVueDescriptorExtractionTests.cs`
- `RazorVueLogicExtractionTests.cs`
- `RazorVueRenderTreeExtractionTests.cs`
- `RazorVueIrTemplateExtractionTests.cs`
- `RazorVueTemplateFrontendParityTests.cs`
- `RazorVueLoweringTests.cs`
- `RazorVueArtifactEmissionTests.cs`
- `RazorVuePipelineTests.cs`

建议的首批测试名称：

- `RazorVue_Entry_ValidVueComponent_IsDiscovered`
- `RazorVue_Entry_ComponentBaseOnly_ReportsDiagnostic`
- `RazorVue_Entry_StaticModule_RemainsOnLegacyPath`
- `RazorVue_Descriptor_ParameterProperty_BecomesProp`
- `RazorVue_Descriptor_EventCallback_BecomesEmit`
- `RazorVue_Descriptor_ChildContent_BecomesDefaultSlot`
- `RazorVue_Resolution_AmbiguousShortName_ReportsDiagnostic`
- `RazorVue_Resolution_FullyQualifiedComponent_ResolvesSuccessfully`
- `RazorVue_RenderTree_OpenElementAddContent_ProducesElementNode`
- `RazorVue_RenderTree_IfBlock_ProducesConditionalNode`
- `RazorVue_RenderTree_Foreach_ProducesLoopNode`
- `RazorVue_RazorIr_ElementTree_ProducesEquivalentTemplateNodeShape`
- `RazorVue_RazorIr_IfForeach_ProducesEquivalentStructuredNodes`
- `RazorVue_TemplateFrontends_BuildRenderTreeAndIr_AgreeOnSupportedSubset`
- `RazorVue_Lowering_HtmlElement_LowersToHCall`
- `RazorVue_Lowering_ComponentNode_LowersToComponentHCall`
- `RazorVue_Artifact_IdentityHashes_AreSplit`
- `RazorVue_Host_Manifest_CanBeMaterialized`

## 8. 首次 PR 顺序

第一阶段不应作为一个巨大的 PR 落地。

建议的顺序：

### PR1. 基础和诊断外壳

包含：

- `JazorComponent`
- `VueComponent`
- 分析器模式拆分
- RazorVue 诊断外壳

### PR2. 描述符提取

包含：

- 描述符模型
- 描述符提取
- 描述符测试

### PR3. 过渡模板前端提取

包含：

- 最小渲染树模型
- 构建器模式提取器
- 渲染树测试

### PR4. Razor IR template frontend spike / parity

包含：

- `RazorCodeDocument` 获取宿主接线
- IR 到模板中间模型提取
- parity 测试
- 不改变现有 descriptor / setup / lifecycle / artifact 主链

### PR5. Lowering 和产物发射

包含：

- 快照/产物模型
- lowering
- 产物测试

### PR6. 宿主过渡

包含：

- `RazorVueCatalog` 消费
- 清单演进
- `DenoHost` 交接

## 9. 骨架的显式非目标

此骨架不应从以下内容开始：

- Vuetify 特定的 lowering
- Router/Pinia 集成
- `.vue` SFC 生成
- “直接从 IR 拼接最终 SFC 文本并同步删除现有 lowering 链”的一次性大切换
- SSR/hydration 规划
- 别名限定的组件标签语法
- 广泛的 Razor 语法支持

## 10. 实现启动门

真正的实现仅在以下条件全部满足时才应开始：

1. 团队接受提议的项目/文件所有权
2. 团队接受首批诊断 ID 范围
3. 团队接受完全限定的组件名称作为第一阶段唯一的歧义规避方式
4. 团队接受 `RazorSourceMap -> GeneratedSyntaxLocation -> GeneratedFallback` 来源层级
5. 团队接受 RazorVue 与当前 `ModuleCatalog` 路径共存的过渡计划
6. 团队接受 `RazorCodeDocument` / Razor IR 是长期模板语义前端，而 `BuildRenderTree` 提取只是过渡实现

## 11. 结论

本文档的目标不是预测每个未来的文件。

它是确保第一阶段从以下内容开始：

- 具体的所有权
- 稳定的阶段边界
- 显式的诊断
- 显式的测试
- 保持当前编译器路径正常工作的实现顺序
