# RazorVue Razor IR 迁移计划

> Status: 活跃计划
> Positioning: RazorVue 模板语义前端从 `BuildRenderTree` / `IOperation` 过渡到 SDK Razor SG 内存 IR / Jazor 中立 Razor IR DTO 的专门执行计划。
> Note: 本计划只负责 template frontend 迁移，不默认重写现有 descriptor、setup/lifecycle lowering、artifact identity 或宿主交接主链。

本文档将 RazorVue 的模板前端迁移拆成可执行阶段。

它不把 “改用 Razor IR” 视为一次性替换。
它要求先证明接线、语义等价和分层边界，再推动默认前端切换。

相关文档：

- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.RazorSg.MainlineIrInjection.DecisionRecord.md](./RazorVue.RazorSg.MainlineIrInjection.DecisionRecord.md)
- [RazorVue.RazorSg.TailInjection.Guidance.md](./RazorVue.RazorSg.TailInjection.Guidance.md)
- [zazzy-whistling-lemon.md](../../../03-完成/razorvue/zazzy-whistling-lemon.md)

## 1. 迁移目标

本迁移只在以下目标上达成一致：

1. 不再把 `BuildRenderTree` 恢复视为 RazorVue 模板语义前端的长期路线
2. 使用 SDK Razor SG 已持有的内存 IR，并在生产侧投影到 Jazor 自有中立 IR DTO，获取更直接的模板结构信息
3. 保留现有 RazorVue 下游主链：
   - descriptor 提取
   - lifecycle / setup logic lowering
   - source-origin
   - identity / HMR boundary
   - catalog / emit / host handoff
4. 在 parity 证明前，不删除旧前端

## 2. 非目标

本迁移默认不做以下事情：

1. 不把 `@code` 文本直接拼进 `<script setup>`
2. 不把 `Jazor.Analyzer` 变成脱离当前 SDK Razor SG 主线的“自建 RazorCompile/classic phase 宿主”
3. 不因为引入 IR 就同步删除 canonical / SFC / artifact lowering
4. 不在第一步就承诺覆盖所有 Razor 语法形状
5. 不把“不确定如何映射”的节点静默降级成看起来合理的模板字符串

## 3. 前置条件

在以下条件成立之前，不要开始真正的默认前端切换：

1. 当前 `BuildRenderTree` 前端已覆盖仓库主测试面中的已支持子集
2. 现有 RazorVue SFC / artifact 路径具备稳定测试基线
3. 有明确的 Razor SG tail 注入与 HostOutput 锚点可以承载官方内存 IR 获取
4. 团队接受 template frontend 替换优先于全链路重写

如果这些前提不成立，IR 迁移将变成边做边猜。

## 4. 阶段 1. 宿主接线证明

目标：

- 证明当前仓库内可以稳定取得 SDK Razor SG 已持有的 Razor IR 内存结果，并能投影为 Jazor 中立 DTO

任务：

- 选择实际 owning project：
  - `Jazor.Analyzer` 只负责 SG tail hook、Roslyn output node 扫描和 object shape 传递
  - `Jazor.RazorVue` 负责反射读取官方对象并投影到中立 IR DTO
  - 不把 Razor Compiler 强引用扩散到生产项目
- 建立最小实验接线：
  - 输入 RazorVue 组件文档或文本
  - 返回 `RazorVueRazorSourceGeneratorDocument`
  - 能拿到对应的 IR/document 边界
- 记录需要的 SDK object shape、IL 指纹和版本对齐策略
- 记录是否需要 internal API 访问，以及爆炸半径控制在哪一层
- 先以独立测试项目证明当前 SDK 实际 API 面，而不是预设历史/猜测 API 名称

验收：

- 存在一个可重复运行的入口，可以为指定 RazorVue 组件返回 `RazorVueRazorSourceGeneratorDocument`
- 文档明确“谁拥有 Razor SG hook”和“谁拥有中立 IR 投影”
- 若需要 internal/private 访问，访问点数量和 owning layer 是显式的
- 已记录当前 SDK 暴露的文档节点入口名；不要假设一定存在 `GetDocumentIntermediateNode()`

验证：

- 测试宿主可以创建真实 SDK `RazorCodeDocument` / `RazorCSharpDocument`，生产 bridge 可以把它们按 object shape 投影为中立 DTO
- 对至少一个真实 RazorVue fixture 返回非空 IR/document

当前仓库已验证的最小事实：

- 已将 RazorVue 生产 bridge 收束到 `src/Jazor.RazorVue/RazorSdk/`
  - 它不强引用 Razor Compiler
  - 它通过 `RazorVueReflectedRazorIrReader` 读取官方 Razor SG 内存对象 shape
  - 它输出 `RazorVueRazorSourceGeneratorDocument` / `RazorVueRazorIrNode` / `RazorVueRazorSourceSpan` 等 Jazor 自有 DTO
- 可以在独立测试项目中直接引用 Razor compiler 二进制并创建 `RazorCodeDocument`
- 当前 SDK 暴露的是 `GetDocumentNode()` / `GetRequiredDocumentNode()`，而不是文档里早期假设的 `GetDocumentIntermediateNode()`
- 这说明后续迁移实现必须以“当前真实 API 面”为准，而不是照搬旧资料或其他宿主中的猜测命名
- 独立测试项目现在还可以解析出与仓库 `global.json` / 当前测试构建实际一致的 Razor SDK toolset，并验证运行时加载的 `Microsoft.CodeAnalysis.Razor.Compiler.dll` 与该 SDK source-generator 二进制哈希一致
- 这意味着后续关于“当前独立 spike 看到的 Razor 行为”可以明确归因到仓库当前锁定 SDK（当前为 `10.0.203`），而不是机器上更高版本/预览版 SDK 漂移
- 在当前最小独立宿主中，基础 markup、正文插值、属性插值以及混合 markup 的 `if` / `foreach` 形状都可以通过 document node 树稳定盘点
- 但当前最小独立宿主下，控制流仍主要表现为穿插于节点树中的 `CSharpCodeIntermediateNode` token 片段，而不是已经整理好的专门条件/循环中间模型
- 更重要的是，当前 spike 仍以 `tagHelpers: null` 运行；大写标签目前只被观察为 `MarkupElementIntermediateNode`，这还不能证明“组件标签 / child content / slot 语义”已经进入可消费的 component-aware IR 路径
- 当前最小独立宿主确实加载了 component 相关 Razor pass 和 `TagHelperDiscoveryService`
- 进一步的独立探针显示：当前最小独立宿主里 `TagHelperDiscoveryService` 虽然存在，但其 `_producerFactories` 集合实际为空
- 这说明当前问题不能再理解成“service 对外类型存在就等于 discovery 能工作”；更底层的 producer factory 注入根本没有在当前最小 host 中完成
- 更进一步的独立探针显示：无论输入是当前编译单元内定义的 component / classic tag helper，还是先编译成 metadata reference 再挂到 host compilation，`TryGetDiscoverer(...)` 都返回 `False`
- 这说明当前阻塞并不是“discoverer 已建立但产出为零”，而是独立 spike 尚未具备让 Razor discovery service 构造 discoverer 的上层发现上下文
- 最新独立探针进一步证明：当测试宿主显式按官方 Razor producer factory 清单向 `RazorProjectEngineBuilder.Features` 注入
  - `DefaultTagHelperProducer+Factory`
  - `BindTagHelperProducer+Factory`
  - `ComponentTagHelperProducer+Factory`
  - `EventHandlerTagHelperProducer+Factory`
  - `KeyTagHelperProducer+Factory`
  - `RefTagHelperProducer+Factory`
  - `SplatTagHelperProducer+Factory`
  - `FormNameTagHelperProducer+Factory`
  - `RenderModeTagHelperProducer+Factory`
  之后，`TryGetDiscoverer(...)` 会从 `False` 变为 `True`
- 这把根因进一步收敛为：当前最小独立 host 并不是“不可能做 component-aware discovery”，而是没有走到 Razor 官方用于装配 producer factory 的初始化切片
- 基于 Razor 官方源码的进一步校准已经确认：
  - `Microsoft.CodeAnalysis.Razor.CompilerFeatures.Register(builder)` 负责注册 component/bind/event/ref/key/splat 等 producer factories
  - `Microsoft.AspNetCore.Mvc.Razor.Extensions.RazorExtensions.Register(builder)` 负责补上 `DefaultTagHelperProducer+Factory` 以及 MVC 相关扩展
  - Razor 官方 source generator 宿主实际走的是 `CompilerFeatures.Register(builder) + RazorExtensions.Register(builder)` 组合
- 当前独立测试项目已把这条官方注册路径接入并验证：
  - 仅 `CompilerFeatures.Register(builder)` 即可让 `TryGetDiscoverer(...)` 为 component compilation 返回 `True`
  - 在同一路径下，`GetTagHelpers(...)` 与 `GetTagHelpersForCompilation(...)` 都已经可以返回 probe component `Probe.Components.CounterCard` 的 descriptor
  - 走完整的 source-generator 对齐路径 `CompilerFeatures.Register(builder) + RazorExtensions.Register(builder)` 时，除了 component descriptors，还能返回 probe classic tag helper `Probe.TagHelpers.DemoCardTagHelper` 的 descriptor
- 这意味着“`ProcessDesignTime` + 手搓 `Compilation` + 直接 discovery”本身并不是根本错误；先前失败的真正原因是独立宿主缺少 Razor 官方的初始化注册切片
- 因此下一阶段的重点已经从“还能不能发现 descriptor”切换为“RazorVue 生产实现到底放在哪个 Razor SDK 对齐宿主层，以及如何在保留 `BuildRenderTree` 正常生成的同时并行产出 Vue SFC catalog”
- 本轮进一步通过 `RazorSourceGeneratorHostOutputTests` 与 `RazorSourceGeneratorCarrierBridgeTests` 证实：
  - 公开 `Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator` 在单次 driver run 中即可同时产出标准 Razor generated source 与 `HostOutputs`
  - `HostOutputs` 中存在 internal `RazorGeneratorResult`
  - 可通过受控 bridge 从 `RazorGeneratorResult.GetCodeDocument(string physicalPath)` 取回对应 `RazorCodeDocument`
- 本轮还确认：
  - `build_metadata.AdditionalFiles.TargetPath` 的真实输入契约是 UTF-8 Base64 后的 Razor 相对路径，而不是纯文本路径
  - 不能依赖 `SuppressRazorSourceGenerator=true` 让手工实例化运行的 SDK Razor SG 自停机
  - 因此 “外层 companion generator 再跑一次 Razor SG + 依赖 SDK 自抑制” 不能作为最终生产架构，只能作为验证手段
- 基于源码阅读和实际测试，当前 RazorVue 正式 SG 接入方案已经锁定为受控 IL 尾部注入：
  - HostOutput 只作为 Razor SG 末端 IR 结果锚点和验证点
  - RazorVue SFC catalog/artifact 必须通过并列 source output 的 `AddSource(...)` 进入 compilation
  - 官方 `.razor.g.cs` 生成、官方 HostOutput 发布、Razor parser / engine passes / tag helper discovery 均保持原样
  - RazorVue 未启用时不注入；目标 SDK IL shape 不匹配时 fail-fast，不退回 `.razor` 回读或 `BuildRenderTree` 反推
- 本轮对旧 `src/Jazor.RazorVue.RazorExtension/` 的校验也表明：
  - `ProvideRazorExtensionInitializerAttribute` / `RazorExtensionInitializer` 等旧 classic extension API 面在当前 SDK 10.0.203 引用下已不成立
  - 这条路线不再属于正式接入候选
  - 该项目已删除，避免保留 Razor Compiler 强引用和错误入口
- 本轮新增真实外部编译时序探针也已确认：
  - analyzer assembly 的模块初始化器可以早于 `Microsoft.CodeAnalysis.Razor.Compiler` 程序集装载事件执行
  - 我们自己的 generator `Initialize(...)` 观察到 Razor compiler assembly 时已经是已装载状态
  - 因此若要在官方 Razor SG 初始化前安装 hook，必须使用 analyzer assembly 装载期入口，而不是 generator `Initialize(...)`
- 本轮外部构建进一步确认：
  - `Jazor.Analyzer` 不应强引用或打包 `Microsoft.CodeAnalysis.Razor.Compiler.dll`
  - `Jazor.RazorVue` 生产代码同样不应强引用或打包 `Microsoft.CodeAnalysis.Razor.Compiler.dll`
  - 官方 Razor SG tail 输出中的 `RazorCodeDocument` / `RazorCSharpDocument` 在 analyzer load context 内可能出现 full name 相同但强转失败的类型身份隔离
  - Analyzer 侧必须按 object/tuple shape 读取官方输出，`Jazor.RazorVue` 必须继续按 object shape 反射投影到中立 IR DTO
  - 生产路径不强转 `RazorCodeDocument`，不重建本地 `RazorCodeDocument`，不读取磁盘 `.razor`，不调用 `AdditionalText.GetText()` 补偿，也不 production nested run Razor SG
- 本轮生产引用收口进一步确认：
  - `RazorVueReflectedRazorIrReaderTests` 覆盖官方对象到中立 IR 的投影，包括 static markup 无 source mappings、generated span `FilePath` nullable、token base-type 识别
  - `ProductionRazorCompilerReferenceTests` 覆盖 `Jazor.Analyzer` / `Jazor.RazorVue` / `Jazor` 包装项目不得引用或打包 Razor Compiler / Razor Utilities Shared
  - `dotnet pack src/Jazor/Jazor.csproj -c Release -v minimal` 已成功，生成包的 analyzer/lib payload 未包含 Razor Compiler / Razor Utilities Shared
- 本轮 fail-fast 边界进一步收口：
  - `RazorSourceGeneratorInitializeHookInstaller` 在 Harmony patch 官方 Razor SG 前先执行 `Initialize(...)` IL hash 与 declared method surface 校验
  - assembly path / version / MVID 只保留为观测信息，不作为正式兼容门
  - unsupported SDK shape 会在 patch 前被拒绝，并通过 bootstrap trace 记录 failure
  - tail output 启用且当前 compilation 存在 RazorVue component candidate 时，如果读不懂官方 output shape、没有收到 Razor SG document 或只收到 suppressed document，必须报告 `JAZORVGA020`
  - tail output 启用但当前 compilation 没有 RazorVue component candidate 时必须 no-op，避免普通 Razor 或未使用 RazorVue 组件的项目被误报
  - 普通 `RazorVueGenerator` 在 integration 启用后不再尝试自己产出 catalog/artifact；它只负责对 bootstrap patch failed、tail 未注册等未接管状态给出 `JAZORVGA019` / `JAZORVGA018`
- 当前聚焦验证已通过：
  - `RazorSourceGeneratorBootstrapPatchTests` 3/3 通过，证明官方 `.razor.g.cs` 保持生成，RazorVue tail output 同轮生成 SFC catalog/artifact，禁用 integration 时 no-op
  - HostOutput / Bridge / Compatibility 相关 focused tests 5/5 通过
  - `RazorSourceGeneratorTailOutputTests` 4/4 通过，证明 enabled tail output 在有 RazorVue candidate 时缺失输入不静默成功，在无 candidate 时不误报
  - `ESGeneratorTests` 新增 3 个 integration bootstrap 诊断回归，证明 tail 未注册不静默成功、patch failed 不被误判为 not-active、tail 已注册后普通 generator 不误报
  - `Jazor.Analyzer` build 0 warning / 0 error，输出目录不再包含 `Microsoft.CodeAnalysis.Razor.Compiler.dll`
  - `Jazor.RazorVue` build 0 warning / 0 error，生产输出不依赖 Razor Compiler

## 5. 阶段 2. IR 节点盘点与支持边界

目标：

- 明确已支持 RazorVue 子集在 Razor IR 中实际长什么样

任务：

- 为以下形状建立样例与节点盘点：
  - element
  - component
  - text / markup
  - interpolation
  - attribute / parameter
  - `if`
  - `foreach`
  - child content / slot
- 记录每种形状对应的 IR 节点和必要上下文
- 区分三类情况：
  - 可直接结构化映射
  - 可经受控适配映射
  - 当前不支持，必须显式失败

验收：

- 已支持子集有明确的 IR 节点对照表
- 不支持形状有显式记录，不再依赖“到时候看 IR 长什么样”

验证：

- 至少有一组文档化 fixture 与节点盘点输出对应

当前已确认的阶段 2 事实：

- 普通元素在当前树中可见为 `MarkupElementIntermediateNode`
- 正文 `@expr` 当前可见为 `CSharpExpressionIntermediateNode`
- 属性值中的 `@expr` 当前可见为 `CSharpExpressionAttributeValueIntermediateNode`
- `if` / `foreach` 在当前最小宿主里至少保留为与 markup 交错的 `CSharpCodeIntermediateNode` 片段，这说明后续映射需要处理“代码 token 包裹结构化 markup”的组合形态
- 当前还不能把“组件节点”“默认 slot”“命名 slot”“typed child content”纳入已盘点完成的事实表；要先补一个 component-aware host，把 tag helper / component discovery 接线接上，再继续这些形状的盘点
- 当前“component-aware host” 的阻塞已经缩小到发现上下文层，而不是 `RazorCodeDocument` 基础接线层：
  - component passes 已存在
  - discovery service 已存在
  - 当前锁定 SDK 的 Razor compiler 二进制已被独立测试宿主实际加载
  - 但 `_producerFactories` 当前为空
  - `TryGetDiscoverer(...)` 目前直接返回 `False`
  - 而显式注入官方 producer factories 后，`TryGetDiscoverer(...)` 可以变成 `True`
  - 因此独立 spike 缺少的是“官方 producer factory 装配所在的 Razor SDK 初始化切片”，而不只是某个 descriptor 输入细节
- 但当前迁移已不应再把“独立 `RazorProjectEngine.Process(...)` 重建主文档”当成正式上线目标；
  - 它更适合用于 IR 形状盘点和辅助验证
  - 正式生产主线需要继续收敛到当前 SDK Razor source generator 已持有的内存 IR 结果和 Jazor 中立 IR DTO 上

## 6. 阶段 3. IR 到模板中间模型映射

目标：

- 用 Razor IR 替换旧 template frontend，而不是直接生成最终 SFC 文本

任务：

- 选择迁移目标：
  - 复用现有 `RazorVueRenderFragment` / canonical template 输入形状
  - 或定义新模板中间模型，并提供显式适配层
- 为以下形状建立映射：
  - HTML element
  - component
  - text / interpolation
  - attribute / parameter
  - conditional
  - foreach
  - slot / child content
- 显式处理 source-origin 采集
- 对无法可靠映射的节点抛出结构化失败，而不是直接拼模板文本

验收：

- 新前端输出可以被当前 canonical / SFC / artifact lowering 消费
- 下游不需要为前端迁移重写 setup/lifecycle/descriptor 主链
- source-origin 不因为前端迁移丢失

验证：

- 针对每种已支持节点形状有单元测试
- 新前端能驱动至少一个真实组件走完当前 template lowering 主链

## 7. 阶段 4. parity 测试

目标：

- 证明新旧前端在已支持子集上语义等价

任务：

- 建立新旧前端双跑测试
- 至少比较：
  - 模板节点形状
  - `if` / `foreach` 结构
  - 组件解析名与 slot 结构
  - template source-origin
  - 最终 template 片段或 artifact hash 稳定性
- 对差异做分类：
  - 等价但文本不同
  - 真实语义偏差
  - 当前不支持形状

验收：

- 已支持子集的语义差异是显式可见的
- 不存在“默认切换后再看哪里坏了”的盲切

验证：

- 有专门的 parity 测试类
- parity 报告能指出具体组件/节点形状差异

## 8. 阶段 5. 默认切换

目标：

- 在验证通过后，将 Razor IR 前端设为默认模板前端

任务：

- 引入受控切换方式：
  - feature flag
  - 配置
  - 或默认新前端 + 仅限手写 `BuildRenderTree` 的显式 fallback
- 将主 pipeline 默认指向新前端
- 不保留“IR 失败就静默回退旧前端”的保守策略
- 明确唯一允许回退的边界：
  - 当前组件没有可绑定的 Razor 文档
  - 且 `BuildRenderTree` 被判定为源码手写 authoring，而不是 Razor 生成产物
- 其余 Razor 组件一律要求 SDK Razor SG document / 中立 IR 投影成功；失败直接报错

验收：

- 默认构建路径不再依赖 `BuildRenderTree` 恢复模板结构
- 切换策略是显式的，不依赖隐式异常吞掉后换路

验证：

- RazorVue 主测试面在默认新前端下通过
- 有针对“手写 `BuildRenderTree` 才允许 fallback”和“Razor 组件缺文档直接报错”的测试

当前已落地的阶段 5 边界收敛：

- generator 默认链现在显式使用 Razor SG document 输入的 `RazorVueRazorDocumentSemanticFrontend`
- 该前端负责把 Roslyn component candidate 绑定到 `.razor` / `_Imports.razor` 路径
- `Jazor.Common` 中的 `DefaultRazorSemanticFrontend` 已退回 Roslyn-only 语义前端，不再隐式承担 Razor 文档定位策略
- 这使 `Jazor.Common` 保持“通用编排 + 显式依赖注入”，而 Razor SDK 文档绑定继续留在 `src/Jazor.RazorVue/RazorSdk/`
- 当前默认切换前新增的架构门已经通过 focused test 锁定：
  - 一个 generator 生成的 source 不能被同轮另一个 generator 的 `CompilationProvider` / symbol 查询看到
  - 第二轮 compilation 才能观察上一轮 generator 输出
  - 因此正式生产实现不能采用“一个 generator 产中间 source，另一个 generator 同轮读取该 source”的串联结构
- 正式接入已经收敛为受控 IL 尾部注入：
  - 复用官方 Razor SG 的最终 document 增量数据流
  - 新增并列 source output 产出 RazorVue SFC catalog/artifact
  - 保持官方 `.razor.g.cs` 与 HostOutput 行为原样
  - 生产侧通过反射中立 IR 投影消费官方结果，不引用 Razor Compiler

## 9. 阶段 6. 旧前端清理

目标：

- 在默认路线稳定后清理过渡实现

任务：

- 统计旧 `BuildRenderTree` 前端的剩余依赖点
- 仅在以下条件全部满足后才删除旧前端：
  - parity 已证明
  - 主测试面稳定
  - source-origin / diagnostics / identity 未退化
  - 下游无旧前端特有耦合
- 删除或 obsolete 旧前端类型，并更新文档

验收：

- 仓库默认路线不再以 `BuildRenderTree` 为模板语义前端
- 清理发生在验证之后，而不是设计预期阶段

验证：

- 删除旧前端后，主 RazorVue 测试面仍通过
- 相关文档不再把 `BuildRenderTree` 提取写成长期架构

## 10. 推荐的 PR 顺序

### PR1. Razor SG 主线桥接事实固化

包含：

- HostOutput focused tests
- `RazorGeneratorResult -> RazorCodeDocument` bridge 测试
- 决策与迁移文档更新
- 不触及主 pipeline 默认路径

### PR2. Razor SG 尾部注入 shape 探针

包含：

- `RazorSourceGenerator.Initialize(...)` IL shape 探针
- `Initialize(...)` IL 指纹和 declared method surface 兼容门
- 指纹不匹配 fail-fast diagnostic 设计
- RazorVue 未启用 no-op 行为设计

### PR3. 最小 RazorVue source output 注入

包含：

- 受控 IL 尾部注入新增并列 source output
- 官方 `.razor.g.cs` parity 验证
- 最小 RazorVue SFC artifact/catalog 同轮进入 final compilation
- package / analyzer 装载最小验证

### PR4. IR 节点盘点与最小映射

包含：

- IR 节点样例
- 最小 template 中间模型映射
- 单元测试

### PR5. parity 测试框架

包含：

- 新旧前端双跑
- 差异报告
- 已支持子集基线

### PR6. 默认前端切换

包含：

- 切换策略
- 仅限手写 `BuildRenderTree` 的 fallback
- 主测试面验证

### PR7. 旧前端清理

包含：

- obsolete / 删除旧提取器
- 文档更新
- 最终回归验证

## 11. 测试策略

建议的测试分层：

1. SDK Razor SG document / 中立 IR 投影测试
2. IR 节点映射测试
3. template source-origin 测试
4. 新旧前端 parity 测试
5. 默认切换测试
6. 旧前端清理后的回归测试
7. Razor SG tail injection 指纹与 fail-fast 测试

建议的早期测试名称：

- `RazorVue_RazorDocumentProvider_ComponentDocument_CreatesCodeDocument`
- `RazorVue_RazorIr_ElementNode_MapsToTemplateElement`
- `RazorVue_RazorIr_ComponentNode_MapsToTemplateComponent`
- `RazorVue_RazorIr_IfForeach_MapToStructuredTemplateNodes`
- `RazorVue_TemplateFrontends_BuildRenderTreeAndIr_AgreeOnSupportedSubset`
- `RazorVue_TemplateFrontend_DefaultsToRazorIr_WhenParityGatePasses`
- `RazorVue_TemplateFrontend_FallsBackOnlyForHandwrittenBuildRenderTree`
- `RazorVue_TemplateFrontend_RazorGeneratedComponentWithoutBoundDocument_FailsFast`
- `RazorVue_Generator_SameDriverRun_DoesNotSeeCarrierPartialFromAnotherGenerator`
- `RazorVue_RazorSourceGenerator_HostOutput_ExposesCodeDocument_ForPhysicalPath`
- `RazorVue_RazorSourceGenerator_TailInjection_AddsRazorVueSourceOutput`
- `RazorVue_RazorSourceGenerator_TailInjection_NoOps_WhenRazorVueDisabled`
- `RazorVue_RazorSourceGenerator_TailInjection_FailsFast_WhenSdkShapeMismatches`

## 12. 风险与缓解

| 风险 | 影响 | 缓解 |
|------|------|------|
| Razor SDK/toolset 接线不稳定 | 高 | 将接线隔离在专门宿主层，不把私有访问扩散到核心主链 |
| Roslyn 同轮 generator 不可见导致串联 generator 架构失效 | 高 | 已通过 focused test 锁定；正式路线改为受控 IL 尾部注入新增并列 source output |
| Razor SG IL shape 随 SDK 升级变化 | 高 | 绑定 `Initialize(...)` IL 指纹和 declared method surface；assembly path/version/MVID 仅作观测，不匹配时 RazorVue 启用场景 fail-fast |
| Analyzer 与官方 Razor SG load context 类型身份隔离 | 高 | Analyzer 与 RazorVue 生产代码均不引用/打包 Razor Compiler；按 object shape 读取官方输出；投影到 Jazor 中立 IR DTO |
| Razor Compiler 被重新引入生产项目 | 高 | `ProductionRazorCompilerReferenceTests` 锁定 Analyzer/RazorVue/Jazor 包装项目不得引用或打包 Razor Compiler / Razor Utilities Shared；旧 RazorExtension 项目已删除 |
| Tail output 静默丢失 catalog/artifact | 高 | enabled + RazorVue candidate 场景下输入不可读、无 Razor SG document、suppressed-only、bridge failed 均报告 `JAZORVGA020`；无 candidate 场景 no-op 防误报 |
| IR 形状与预期不一致 | 高 | 先做节点盘点和样例归档，再做正式映射 |
| 新前端破坏 setup/lifecycle/source-origin | 高 | 迁移只替换 template frontend，保留下游主链，强制 parity 测试 |
| 切换后回归难定位 | 中 | 建双跑 parity 报告，并把 fallback 严格限制为手写 `BuildRenderTree` authoring |
| 过早删除旧前端 | 中 | 把清理延后到默认切换稳定之后 |

## 13. 完成门

Razor IR 模板前端迁移仅在以下条件全部满足时才算完成：

1. SDK Razor SG document / 中立 IR 投影路径稳定
2. IR 到模板中间模型的已支持映射完成
3. 新旧前端 parity 在主支持子集上已证明
4. 默认前端已切换到 Razor IR
5. source-origin / diagnostics / identity 未退化
6. 旧 `BuildRenderTree` 前端已删除或降为非默认且有明确清理计划

## 14. 结论

使用 SDK Razor SG 内存 IR / Jazor 中立 IR DTO 代替 `BuildRenderTree` 是必要方向。

正确吸收当前补充方案的方式是：

1. 接受 SDK Razor SG 内存 IR / Jazor 中立 IR DTO 取代 `BuildRenderTree` 作为长期 template frontend
2. 接受“通过当前 SDK Razor SG 已持有的内存 IR 结果接入，而不是自建 classic / path 回读主线”
3. 接受“受控 IL 尾部注入 + 并列 source output”作为当前源码阅读和实际测试后的正式 SG 接入方案
4. 不接受“直接从 IR 拼最终 SFC 文本，并把 `@code` 文本直接并入 `<script setup>`，同时绕过现有 descriptor / lifecycle / setup / artifact 主链”的一步到位切法

因此，正确的做法不是直接跳到“从 IR 拼 SFC 文本并删除旧链”，
而是把 template frontend 迁移当成一个有 Razor SG 尾部注入、Razor IR document 输入、结构映射、parity 验证和默认切换门的独立工程任务。
