# RazorVue Razor SG 主线 IR 注入决策记录

> Status: 活跃决策记录
> Date: 2026-05-08
> Scope: RazorVue 正式上线前，Razor 组件如何在当前 SDK Razor Source Generator 主线上稳定产出可消费的 IR carrier。

本文档只记录已经确认的事实、被否定的路线、当前锁定的实现方向和下一步执行顺序。

它的目的不是展开所有设计讨论，而是防止后续实现重新回到已经被证明错误或被明确禁止的路线。

相关文档：

- [RazorVue.RazorIrMigrationPlan.md](./RazorVue.RazorIrMigrationPlan.md)
- [RazorVue.RazorSg.TailInjection.Guidance.md](./RazorVue.RazorSg.TailInjection.Guidance.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [docs/03-完成/razorvue/completion-analysis.md](../../../03-完成/razorvue/completion-analysis.md)

## 1. 当前目标

当前 RazorVue 线路的核心阻塞不再是 packaging，也不再是消费侧模板前端。

当前唯一主问题是：

1. 在当前 SDK Razor Source Generator 主线上，为 Razor 组件正式产出 Razor IR carrier。
2. 让 `Jazor.Analyzer` / `RazorVueGenerator` 在正式构建中只消费 carrier，而不是重新猜测 Razor 文档来源。
3. 保持 Razor 组件始终走 IR 语义，不回退到错误路线。

## 2. 已锁定的硬约束

以下约束已经明确，后续实现不得违反：

1. Razor 组件必须走 IR 解析。
2. 注入点是 Roslyn / Razor IR 渠道，不是文件系统回读渠道。
3. `path` 最多只能作为 identity metadata，不能变成主输入契约。
4. 手写 `.cs` / 手写 `IComponent` / source-authored `BuildRenderTree` 可以作为显式 fallback。
5. Razor 组件没有 carrier 时必须报错，不能静默弥合。

## 3. 明确禁止的路线

以下路线已经被否定，后续不得再回到这些方向：

1. 读取 `.razor` 原文作为实现主线。
2. 使用 `AdditionalTextsProvider` / `AdditionalFiles` 回读 `.razor` 作为补偿主线。
3. 基于 `path` 显式回读 `.razor` 后再 `RazorProjectEngine.Process(...)`。
4. Razor 组件缺原文时 fallback 到 Roslyn 生成的 `BuildRenderTree`。
5. 切回 classic Razor codegen / `SdkRazorGenerate` / `ResolvedRazorExtension` 主线。
6. 把 `ProvideRazorExtensionInitializerAttribute` / 旧 initializer 模型当成正式接入方案。

这些路线的问题是同一个：它们都绕开了当前 SDK Razor SG 主线，而不是在主线上完成正式接入。

## 4. 已完成且不能回滚的改动

当前消费侧已经切到 carrier-only 模型，以下改动属于正确方向，不应回退：

1. 新增 Razor IR carrier 模型：
   - `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrCarrier.cs`
   - `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrCarrierMetadata.cs`
   - `Jazor.RazorVue.Runtime.RazorVueRazorIrCarrierAttribute`
2. `RazorVueSemanticSnapshot` 去掉旧 path/import-path 主输入。
3. `RazorVueCompilationContext` 去掉 `RazorVueRazorDocumentSet` 主输入以及主文档查找逻辑。
4. `RazorVueRazorDocumentSemanticFrontend` 收紧为 carrier-only。
5. 删除 `src/Jazor.RazorVue/RazorSdk/RazorVueRazorDocumentLocator.cs`。
6. `RazorVueRazorCodeDocumentProvider` 只认 `snapshot.RazorIrCarrier`。
7. `RazorVuePreferredTemplateFrontend` 改成：
   - 有 carrier 则走 Razor IR frontend。
   - 无 carrier 且 `BuildRenderTree` 为源码手写时允许 fallback。
   - 无 carrier 且属于 Razor 生成组件时明确报错。
8. `src/Jazor.Analyzer/RazorVue/Generation/RazorVueGenerator.cs` 已去掉对 `AdditionalTextsProvider` 的主依赖。

## 5. 已验证通过的聚焦测试

以下 focused tests 已通过，证明“消费侧切 carrier-only”当前是稳定的：

```powershell
dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorVueRazorCodeDocumentProviderTests|FullyQualifiedName~RazorVueTemplateFrontendParityTests" -v minimal
```

结果：13/13 通过。

```powershell
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~RazorVuePipelineTests|FullyQualifiedName~RazorVueSfcArtifactFactoryTests|FullyQualifiedName~RazorVueDescriptorExtractionTests|FullyQualifiedName~ESGeneratorTests" -v minimal
```

结果：406/406 通过。

这说明当前问题主要不在消费侧，而在正式 carrier 注入链。

## 6. SDK 主线调查结论

当前仓库 `global.json` 锁定 SDK `10.0.203`。

围绕这一版本已确认的事实如下：

1. `Microsoft.NET.Sdk.Razor` 在 `net6+` 默认启用 `UseRazorSourceGenerator=true`。
2. 当前 SDK 主线是 Razor Source Generator，不是 classic RazorCompile。
3. SDK 会把 `.razor` 作为 `AdditionalFiles` 接入，并通过 analyzer config 传递：
   - `build_property.RazorLangVersion`
   - `build_property.RootNamespace`
   - `build_property.SupportLocalizedComponentNames`
   - `build_property.GenerateRazorMetadataSourceChecksumAttributes`
   - `build_property.MSBuildProjectDirectory`
   - `build_metadata.AdditionalFiles.TargetPath`
   - `build_metadata.AdditionalFiles.CssScope`
4. `ResolvedRazorExtension` 只对 classic codegen 目标链有意义，不是当前 SG 主线的正式入口。

## 7. Razor SG / Roslyn 探针结论

### 7.1 SDK Razor SG 可见 API 面

对 `Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator` 的探针已确认存在这些关键方法：

1. `Initialize(IncrementalGeneratorInitializationContext)`
2. `GetDeclarationProjectEngine(...)`
3. `GetStaticTagHelperFeature(Compilation)`
4. `GetGenerationProjectEngine(...)`
5. `ComputeRazorSourceGeneratorOptions(...)`
6. `ComputeProjectItems(...)`

这说明“在我们自己的 generator / test harness 内主动驱动 SDK Razor SG”是现实方向。

### 7.2 Roslyn HostOutput 通道形状

对 Roslyn API 的探针已确认：

1. `IncrementalGeneratorInitializationContext` 有公开 `RegisterHostOutput(...)`
2. `HostOutputProductionContext` 有 `AddOutput(string, object)`
3. `GeneratorRunResult` 有 `HostOutputs : ImmutableDictionary<string, object>`

这说明 Razor SG 的确可以通过 HostOutput 发布内部结果。

### 7.3 SDK internal 类型事实

对 `10.0.203` SDK 二进制的探针已确认：

1. `RazorGeneratorResult` 是 internal。
2. `SourceGeneratorRazorCodeDocument` 是 internal。
3. `RazorSourceGenerationOptions` 是 internal。
4. `SourceGeneratorProjectItem` 是 internal。

这意味着正式实现不能把这些 SDK internal 类型当成长期公开契约直接硬耦合。

更合理的原则是：

1. 主线使用公开 Roslyn driver / generator 输入输出能力。
2. 对少量不可见 internal 点收口在很小的 bridge 层。
3. 如确有必要，可评估 `UnsafeAccessor` 或旧平台下的 IL weaving，但它们只能是窄桥接手段，不能成为主架构。

## 8. 本轮新增确认事实

本轮 focused validation 已把“能不能在当前 SDK Razor SG 主线上拿到可消费结果”从猜测推进到已证实事实。

### 8.1 `AdditionalFiles.TargetPath` 的真实契约

当前 SDK Razor source generator 对 `build_metadata.AdditionalFiles.TargetPath` 的处理不是“直接相对路径字符串”，而是：

1. 先读取 analyzer config metadata
2. 再按 Base64 解码
3. 最后按 UTF-8 还原成 Razor 相对路径

这意味着：

1. 任何手工驱动 Razor SG 的 harness / bridge 都必须按 UTF-8 Base64 写入 `TargetPath`
2. 不能再把之前的纯文本 `Pages/Counter.razor` 当成正确输入
3. 之前遇到的 `FormatException` 已被定位为输入契约错误，而不是 Razor SG 主线不可行

### 8.2 公开 `RazorSourceGenerator` 单轮运行已证实可同时产出 generated source 与 `HostOutputs`

`src/Jazor.RazorVue.RazorIr.Test/RazorSourceGeneratorHostOutputTests.cs` 已证明：

1. 手工构造 `Compilation + AdditionalText + analyzer config metadata`
2. 设置 `build_property.EnableRazorHostOutputs=true`
3. 单次运行公开 `Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator`

可以同时得到：

1. 正常 Razor generated source
2. `GeneratorRunResult.HostOutputs`
3. 其中 key/value 中包含 internal `RazorGeneratorResult`

这条事实非常关键，因为它证明：

1. 当前 SDK 主线本身已经持有我们需要的 Razor 结果
2. 问题不再是“SDK Razor SG 能不能产出结果”
3. 真正问题只剩“如何在正式 analyzer/generator 主线上同轮、单次、合法地接进去”

### 8.3 `RazorGeneratorResult` 可桥接回 `RazorCodeDocument`

`src/Jazor.RazorVue.RazorIr.Test/RazorSourceGeneratorCarrierBridgeTests.cs` 已证明：

1. `HostOutputs` 中的 internal `RazorGeneratorResult`
2. 可以通过受控反射 bridge 调用 `GetCodeDocument(string physicalPath)`
3. 按 Razor 组件 physical path 取回对应 `RazorCodeDocument`

这意味着当前正式注入路线已经具备最关键的桥：

1. 不需要读 `.razor` 原文回建文档
2. 不需要回到 classic Razor codegen
3. 可以直接站在当前 SDK Razor SG 已生成的 code document / IR 结果上继续产出 carrier

### 8.4 不能依赖 `SuppressRazorSourceGenerator=true` 让手工实例化的 Razor SG 自停机

本轮曾尝试验证“外层 companion generator 手工驱动 Razor SG 时，是否可以靠 `build_property.SuppressRazorSourceGenerator=true` 避免重复执行官方 Razor SG”。

结论是否定的：

1. 手工实例化运行 `RazorSourceGenerator` 时，它不会因为这个 property 自动停机
2. 因此不能把“companion generator 外层再跑一次 Razor SG，同时希望官方 SG 被自抑制”当成正式生产架构

这进一步收紧了最终方向：

1. companion / double-run 最多只能作为验证手段
2. 最终生产实现必须回到单次处理，而不是依赖自抑制假设

### 8.5 旧 `Jazor.RazorVue.RazorExtension` 路线在当前 SDK API 面下已不成立

对 `src/Jazor.RazorVue.RazorExtension/` 的再验证表明：

1. `ProvideRazorExtensionInitializerAttribute`
2. `RazorExtensionInitializer`
3. `IntermediateNodePassBase.ExecuteCore(...)`
4. `IRazorTargetExtensionFeature.TargetExtensions`

这些旧 API/扩展点在当前 SDK 10.0.203 对齐的引用面下已不匹配。

这不是“还差一点修补”的问题，而是当前 SDK SG 主线的正式入口已经不是这条路。

因此：

1. 旧 initializer/classic extension 不再作为主线候选
2. 不要再把实现资源投入到 `src/Jazor.RazorVue.RazorExtension/`

## 9. 当前锁定的正式实现方向

经过 SDK Razor SG 源码阅读、Roslyn 同轮可见性 focused test、HostOutput focused test 以及实际 bridge 验证，当前正式实现方向已经锁定为：

**官方 Razor SG 原样运行，通过受控 IL 尾部注入复用官方增量数据流，额外注册 RazorVue carrier source output。**

详细执行指导见 [RazorVue.RazorSg.TailInjection.Guidance.md](./RazorVue.RazorSg.TailInjection.Guidance.md)。

### 9.1 最终目标不变

最终目标仍然是：

1. 在 `Jazor.Analyzer` analyzer/generator 载体内完成 RazorVue 正式 SG 接入。
2. 它必须建立在当前 SDK Razor SG 主线上，而不是 classic / path 回读 / 原文重建。
3. 它最终必须符合单次处理，不接受“正式生产仍双跑 Razor SG”。
4. 官方 `.razor.g.cs` 生成路径必须保持原样。

### 9.2 已排除的生产架构

以下方案可以作为 spike / 探针存在，但不得作为生产主线：

1. companion generator 同轮读取官方 Razor SG 新生成的 source / partial / attribute。
2. 外层 generator 以相同 Roslyn 输入 nested run 一次 SDK `RazorSourceGenerator`。
3. wrapper / fork 替换官方 Razor SG 后再产出 carrier。
4. 只依赖 `HostOutput` 回调产出 carrier source。

排除原因：

1. 同轮 generator 之间不能看到彼此新增的 partial / attribute。
2. nested run 会重复执行 Razor SG，且不能靠 `SuppressRazorSourceGenerator=true` 让手工实例化的 SDK Razor SG 自停机。
3. wrapper / fork 更容易引入用户项目 Razor 编译主线和 analyzer assembly 版本冲突风险。
4. `HostOutput` 是宿主输出，不是 compilation source output，不能把 carrier partial 放进 final compilation。

### 9.3 锁定方案形态

正式方案形态如下：

1. 通过受控 IL 尾部注入 patch `RazorSourceGenerator.Initialize(...)`。
2. 保留官方 `RegisterImplementationSourceOutput(...)` 产 `.razor.g.cs` 的逻辑。
3. 保留官方 `RegisterHostOutput(...)` 发布 `RazorGeneratorResult` 的逻辑。
4. 在官方最终 Razor/C# document 增量数据流形成后，新增一个并列 RazorVue source output。
5. RazorVue source output 复用同一条官方数据流，生成 RazorVue IR carrier partial。
6. carrier partial 进入同一轮 final compilation，供 RazorVue carrier-only 消费侧使用。

另一个现在已经由 focused test 锁定的关键点是安装时序：

1. `Jazor.Analyzer` 作为 analyzer 载荷进入编译器进程时，模块初始化器可以早于 `Microsoft.CodeAnalysis.Razor.Compiler` 程序集装载执行。
2. 我们自己的 generator `Initialize(...)` 已经晚于 Razor compiler assembly 装载。
3. 因此真正的 patch 安装入口必须落在 analyzer assembly 装载期，而不是落在 `RazorVueGenerator.Initialize(...)`。
4. 这进一步排除了“等我们的 generator 开始跑时再动态挂钩官方 Razor SG”的变体。

其中 `HostOutput` 的角色是：

1. 证明官方 Razor SG 末端拥有完整 `RazorGeneratorResult` / `RazorCodeDocument`。
2. 作为定位末端数据流和验证 bridge 的锚点。
3. 不是 carrier partial 的源码产出通道。

carrier partial 必须通过 source output 的 `AddSource(...)` 进入 compilation。

### 9.4 实现硬边界

无论哪种结果，正式实现都不允许：

1. 回读 `.razor`
2. classic codegen
3. Razor 组件 fallback 到 Razor 生成的 `BuildRenderTree`
4. 生产环境双跑 SDK Razor SG
5. 改写官方 `.razor.g.cs`
6. 改写官方 Razor parser / engine passes / tag helper discovery

### 9.5 注入失败策略

由于该方案依赖 SDK internal / IL shape，必须显式 fail-fast：

1. 绑定 `RazorSourceGenerator.Initialize(...)` IL 指纹与 declared method surface。
2. assembly path / version / MVID 只作为测试和排查观测信息，不作为正式兼容门。
3. 校验失败时，如果 RazorVue 已启用，则报告明确诊断并停止 carrier 生成。
4. RazorVue 未启用时，不注入、不影响普通 Razor 项目。
5. SDK 升级必须先更新指纹和 focused tests。

失败时禁止自动退回 `.razor` 原文回读、`BuildRenderTree` 反推、classic codegen 或 production nested run。

## 10. 当前不建议采用的路线

即使技术上能做，也不建议把以下方式作为当前主方案：

1. 直接在 `Jazor.RazorVue` 重新自建一套“手工 `RazorProjectEngine.Process(...)` + 原文输入”的正式生产链。
2. 通过路径或 `AdditionalText.GetText()` 自己重组一套“伪 SG”主线。
3. wrapper / fork 替换官方 Razor SG 主线。
4. production nested run SDK `RazorSourceGenerator`。
5. 只在 `HostOutput` 回调内尝试产 carrier source。

这些路线最多只能作为测试探针或失败对照，不得作为 RazorVue 正式上线实现。

`UnsafeAccessor` / `ILAccess.Fody` / IL patching 的定位已经收窄：

1. 可以作为受控 IL 尾部注入的实现手段。
2. 不能用于绕过 Roslyn 调度模型。
3. 不能把官方 Razor SG 替换为 Jazor 私有 wrapper。

## 11. 仍待实现的技术点

架构方向已锁定，后续还需要完成以下实现工作：

1. Razor SG `Initialize(...)` IL shape 探针与指纹校验。
2. 最小 RazorVue carrier source output delegate。
3. 注入后同轮 `.razor.g.cs` 与 carrier partial 产出验证。
4. `RazorGeneratorResult` / `RazorCodeDocument` bridge 的最小 internal 访问面收口。
5. RazorVue 启用 / 未启用行为分流。
6. `Jazor` NuGet analyzer/generator 载体中的依赖打包与加载验证。
7. SDK 指纹不匹配时的明确 diagnostic。

## 12. 接下来执行顺序

后续实现按以下顺序推进，不跳步：

### 第一步：完成 IL shape 探针

目标：

1. 读取当前 SDK Razor compiler assembly。
2. 定位 `RazorSourceGenerator.Initialize(...)`。
3. 输出 `Initialize(...)` IL 指纹与 declared method surface，并把 assembly path / version / MVID 保留为观测信息。
4. 证明当前 SDK 下可以定位最终 Razor/C# document 增量数据流。

### 第二步：最小尾部注入验证

目标：

1. 通过 IL 尾部注入新增并列 source output。
2. 官方 `.razor.g.cs` 仍正常产出。
3. RazorVue carrier partial 同轮产出并进入 final compilation。
4. RazorVue 未启用时 no-op。

### 第三步：carrier 数据面补齐

目标：

1. 从 `RazorCodeDocument` / IR 提取组件 identity。
2. 补 imports、document boundary、source-origin 以及模板前端所需结构。
3. 不支持 Razor IR shape 产出显式 diagnostic。

### 第四步：端到端回归与打包补齐

目标：

1. 补 `src/Jazor/Jazor.csproj` analyzer 依赖打包缺口。
2. 补 RazorVue focused suites 与 NuGet 场景验证。
3. 覆盖 SDK 指纹不匹配 fail-fast。
4. 再收口到 emit / SDK 集成切片。

## 13. 当前工作区注意事项

当前仓库存在较多无关脏改动，后续实现应只聚焦：

1. `src/Jazor.Analyzer`
2. `src/Jazor.RazorVue`
3. `src/Jazor.RazorVue.RazorIr.Test`
4. `src/Jazor.RazorVue.Test`

以下方向当前不要继续投入：

1. `src/Jazor.RazorVue.RazorExtension/`
2. Jolt 主线
3. 无关 Wiki / Vue3 / Pinia / VueRoute 脏改动

## 14. 一句话结论

RazorVue 当前已经把消费侧改对了，且本轮已证实 SDK Razor SG 单轮能够同时产出 generated source 与 `HostOutputs -> RazorGeneratorResult -> RazorCodeDocument`；经过源码阅读和实际测试，当前最优解已经锁定为：HostOutput 用作 Razor SG 末端 IR 结果锚点，受控 IL 尾部注入新增并列 source output，用同一条官方 Razor SG 数据流生成 RazorVue carrier。
