# CLR Module Artifact Provider 统一路线图

> 状态：已完成。本文记录已经落地的最终契约、实施结果和验收门禁。
>
> 基线：`v0.25.0` 已发布实现。实现名称、文件位置和包布局以后续演进为准；
> `System/*.js` 的模块路径、内容语义和导入 ABI 则是必须保持的兼容边界。

## 决策摘要

CLR module 应该和组件库使用同一套 **逻辑 artifact/provider 契约**。这能让
`Jazor.Emit` 只处理一张 artifact graph，统一依赖闭包、冲突、哈希、import-map 和
版本诊断。

这不等于把 CLR module 强制改造成组件库的物理布局（例如 `manifest.json + dist/`）。
CLR module 由 `Jazor.CLR` 的源码和生成器拥有，继续以内嵌程序集资源或生成的内联内容
交付都可以；消费者只看标准 provider 条目。物理载体应由包边界和部署需求决定，不能
再由某个程序集专名决定语义。

目标形态：

```text
Jazor.CLR source --generator--> CLR runtime provider --\
RazorVue/component source --------> adapter provider -----> unified artifact graph
package manifest ----------------> package provider ----/          |
                                                                  v
                                                              Jazor.Emit
```

统一 provider 不改变“谁使用谁直接引用”的工具资格规则：中间类库不会因为携带
provider 而自动获得 analyzer、generator、build target 或 Emit 资格。provider 是
artifact/resource 依赖，工具引用仍由项目直接声明并由 `PrivateAssets` 等 NuGet 资产
边界控制。

## 为什么现在要统一

历史实现曾把相同的运行时目标拆成多条读取路径；当前版本已经一次性切换到统一 provider 契约：

| 载体 | 当前形状 | 读取端行为 | 主要问题 |
| --- | --- | --- | --- |
| CLR runtime | `Jazor.Artifacts.RuntimeProviderCatalog`，条目内含 `Content`、`Hash`、`Dependencies` | `CatalogReader` 按标准 provider contract 读取 | CLR runtime 与其他 provider 共享 schema、路径、哈希和闭包校验 |
| RazorVue runtime | `Jazor.Artifacts.RuntimeProviderCatalog`，条目引用嵌入资源并声明 `Dependencies` | 结构化读取 `SchemaVersion`、`ProviderId`、模块和 import-map | 与 CLR inline provider 走同一归一化路径，资源读取后得到等价 `ModuleRecord` |
| package ESM/CSS | package `manifest.json` + `dist`/license closure | `LibraryMaterializer` 按选中的 logical import 物化 | 与程序集 provider 的冲突、版本和依赖诊断没有完全共用 |

相关代码基线：

- [`ClrRuntimeCatalogEmitter`](../../src/Jazor.Compiler.Generator/ClrRuntimeCatalogEmitter.cs) 扫描 `Jazor.CLR/module` 并生成标准 `Jazor.Artifacts.RuntimeProviderCatalog`；
- [`CatalogReader`](../../src/Jazor.Emit/CatalogReader.cs) 统一读取普通 catalog、artifact catalog 和 runtime provider；
- [`ModuleCollector`](../../src/Jazor.Emit/ModuleCollector.cs) 已按真实 import 入口保留 provider，并跟随声明的依赖闭包；
- [`ArtifactRuntimeProviderCatalog`](../../src/Jazor.RazorVue/Runtime/ArtifactRuntimeProviderCatalog.cs) 是当前标准 provider 的参考实现；
- [`Jazor.targets`](../../src/Jazor/buildTransitive/Jazor.targets) 通过 `JazorArtifactProviderAssembly` 接收 adapter provider。

分裂协议带来的风险不是当前模块语义错误，而是长期维护和发布边界不稳定：

1. 新 provider 必须修改 `Jazor.Emit` 的类型名特判，容易把程序集名误当成协议；
2. inline CLR 内容和 embedded component 资源的哈希、换行、source map 校验可能漂移；
3. `A -> B -> Console`、本地源码和 NuGet consumer 的 provider 发现路径不一致；
4. 旧 catalog 与新 provider 同时存在时，重复路径和内容冲突可能在不同入口表现不同；
5. package manifest、provider import-map 和模块依赖无法形成统一、可解释的诊断。

## 目标契约

### Provider 层

标准 provider 的发现和读取不依赖程序集名称。最低契约如下；具体公共/内部可见性
在 U0 冻结后再决定，不能由某个 adapter 私自扩展：

| 字段/入口 | 约束 |
| --- | --- |
| `SchemaVersion` | 整数；不兼容变更递增版本，不受支持的版本必须显式失败，不静默降级 |
| `ProviderId` | 稳定、区分大小写的逻辑身份；CLR 固定为 `jazor.clr`，RazorVue 继续为 `jazor.vue` |
| provider version/要求 | 用于发现版本冲突和兼容范围；若首版不纳入运行时字段，至少保留 manifest/package 层的诊断入口 |
| `GetModules()` | 返回 provider 自有 module descriptor；顺序不构成语义，读取后按规范排序 |
| `GetAssets()` | 可选的静态文件、source map、license 或其他随 provider 激活的 artifact |
| `GetImportMapEntries()` | 可选的 import-map contribution，只在 provider 被保留时生效 |
| provider dependencies | 只有真实跨 provider 依赖才声明；不能用包名或程序集名推断 |

### Module 层

每个 module descriptor 至少表达：

- 稳定 `Id` 和相对 `RelativePath`；路径统一使用 `/`，不得为空、绝对路径或逃逸
  输出根目录的 `..`；
- 一种内容来源：`InlineContent` 或 `EmbeddedResource`，读取后得到同一份 UTF-8、
  `LF` (`\n`) 规范化文本；不得同时提供两个来源；
- 内容哈希。推荐固定为规范化 UTF-8 文本的 lowercase SHA-256 64 字符十六进制值，
  并在 U0 记录现有哈希兼容规则；读取端应验证声明哈希而不是盲信它；
- module-level `Dependencies`，每一项都是 provider 图中的相对 module path；缺失
  依赖必须指出 provider、module 和 dependency path 并失败；
- 可选 source map（路径、内容、哈希必须成组出现）、package import roots、HMR
  metadata 和 asset linkage。

`ModuleRecord` 是 Emit 的内部归一化结果。inline 与 embedded 两种载体读完后必须
产生等价的 `ModuleRecord`，包括内容、哈希、路径、依赖、source map 和 provider
identity；不能让物理载体泄漏到后续冲突或物化逻辑。

### 激活与闭包

provider module 不是“发现即全量输出”。只有应用生成模块的真实 import 入口命中
provider module 时才激活，随后沿 provider 明确声明的 module dependency 闭包保留。
没有 CLR import 时必须输出零 CLR module；Emit 不通过 `System/` 前缀、程序集名、
资源文件名或 package restore 图猜测依赖。

首阶段可以保留 `ModuleCollector` 当前的入口探测方式；若未来需要更强的无文本探测，
应新增 compiler-owned import-edge metadata，不能在 Emit 中引入一个只为 CLR 编写的
JavaScript 解析器。

### 冲突与确定性

- 同一 provider、同一 module identity 的内容不一致时失败；
- 不同 provider 或不同 provenance 占用同一路径时，内容/元数据不完全一致则失败；
- 相同内容的重复 provider 条目只保留一份，并记录可诊断的 provenance；
- module、asset、import-map 和错误列表按稳定的 ordinal 规则排序；
- provider schema、module path、dependency path、哈希和 source map 的验证在读取端
  完成，不能推迟到写文件后才发现。

## 不变边界

本路线只改变 artifact 的发现和交付协议，不扩大 CLR 或 compiler 能力：

1. `System/*.js` 路径、导出名和导入 ABI 保持不变；
2. CLR mapping、carrier 推断、白名单 key 和 `SemanticWalker` lowering 不迁移到
   Emit，也不因 provider 统一而增加 fallback；
3. `Jazor.CLR` 仍是 CLR mapping/runtime 源码的唯一所有者，生成器继续从这些源码
   产生最终模块；
4. `Jazor.Emit` 只消费归一化 artifact graph，不重新解析 CLR 源码或重编译 JavaScript；
5. `Jazor`、`Jazor.Vue` 的直接引用和工具资产隔离规则保持不变；provider 传播不能
   反向授予下游工具资格；
6. 不把 CLR module 简单复制成每个应用的 `dist` 全量目录，也不让中间类库写入最终
   宿主的 `jazor/` 目录。

## 实施阶段

### U0：契约冻结与物理边界决策

**责任**：`Jazor.Common`/`Jazor.Emit`、`Jazor.CLR`、RazorVue adapter、Packaging。

工作项：

- 建立当前 provider schema-v1 的正式字段表和兼容矩阵，冻结 provider/module
  identity、路径规范、哈希算法、换行规范、source map、asset、import-map 和依赖语义；
  若 inline source、source map 或 version 要求无法以向后兼容方式加入，则在此阶段
  冻结新 schema version，不把差异藏在可选字段解释中；
- 首个版本同时支持 inline 与 embedded resource 两种 module source；两者属于同一
  schema，由 reader 归一化，不为 CLR 保留第二套 catalog 协议；
- 选择 CLR provider 的最终物理载体：可先在现有程序集内生成标准 provider，以降低
  迁移风险；最终可迁入专用 CLR artifact/provider assembly（名称和包边界在此阶段
  冻结）。不得把“是否嵌入”写成 Emit 的业务判断；
- provider version/要求暂由 package/manifest 版本边界承载；schema 不兼容时直接失败，不提供旧载体过渡期；
- 用最小 synthetic provider fixture 固化合法、缺失依赖、重复路径、冲突哈希、旧/新
  载体等契约测试。

**出口条件**：契约文档、schema fixture、版本/冲突诊断样例评审通过；没有依赖
程序集名称的未决字段；确认不需要修改 CLR lowering 或公共作者 API。

### U1：Emit 通用读取与归一化

**责任**：`Jazor.Emit`。

工作项：

- 将 `CatalogReader` 的 runtime provider 读取抽成与 provider 来源无关的归一化路径；
  inline content、embedded resource 和现有 artifact catalog 都映射到同一
  `ModuleRecord`/asset/import-map 模型；
- 统一 schema、路径、哈希、source map、依赖和 import-map 校验，错误消息包含
  provider id、assembly/package provenance、module id/path；
- 让 `ModuleCollector` 继续以真实入口裁剪 provider，并对 provider dependency closure
  采用同一套缺失依赖、重复路径和确定性排序规则；
- 不读取旧 catalog 类型；旧包必须升级到包含标准 provider catalog 的版本，Emit 不提供双读或 fallback；
- 不在本阶段改变写出的 `System/*.js` 内容、路径或导入别名。

**出口条件**：synthetic inline/embedded provider 产生完全等价的 `ModuleRecord`；
旧 CLR catalog 不属于支持输入，标准 provider 在同一输入下产生确定的模块闭包和 manifest；schema 错误、
路径冲突和缺失依赖均显式失败。

### U2：CLR 生成器迁移到标准 provider

**责任**：`Jazor.Compiler.Generator`、`Jazor.CLR`；生成输出由现有生成流程维护。

工作项：

- 将 `ClrRuntimeCatalogEmitter` 的输出改为标准 `Jazor.Artifacts.RuntimeProviderCatalog`
  形状，固定 `ProviderId = "jazor.clr"`，用标准 `Dependencies` 表达编译器收集的
  CLR module dependency closure；
- 首选先复用现有已生成 module 文本和 hash，验证迁移只改变 catalog 外壳；必要时
  再把文本从 inline source 迁入嵌入资源；两步不能同时改变 CLR module 内容；
- 保持 `System/*.js` 相对路径和依赖闭包；生成器仍从 Roslyn/`SemanticWalker` 结果
  收集依赖，不让 Emit 通过解析 JavaScript 猜图；
- 重新生成并提交应提交的 generated source，禁止手改生成输出来绕过 schema；
- 为 CLR provider 增加 provider metadata、所有 module 的 hash/path/dependency
  一致性断言，并保留现有 CLR 行为测试。

**出口条件**：标准 provider 可以独立被 `CatalogReader` 读取；迁移前后模块文本、
hash、路径、导入 ABI 和按入口裁剪的闭包一致；没有 CLR import 时仍为零 CLR module；
CLR whitelist/compiler/Razor SG 现有门禁不变。

### U3：包布局与 MSBuild provider 注册

**责任**：`Jazor`/Packaging、`Jazor.CLR` provider 所有者、`Jazor.Vue` adapter。

工作项：

- 明确 provider assembly、contract assembly、analyzer 和 build target 的包归属；
  不依赖 `ECMAScript.dll` 的偶然名称，也不依赖某个 analyzer 文件名才能发现 CLR
  provider；
- 为包消费者提供显式、稳定的 `JazorArtifactProviderAssembly` 注册；Debug、Release
  和 SSR 使用同一收集边界，并对重复路径做一次性去重；
- 验证 `Jazor.CLR` provider 的运行时依赖能随最终宿主到达，但不会把中间类库的
  analyzer/generator/Emit 工具资格隐式传给下游；
- 明确 `A -> B -> Console` 的责任：A 产生并携带自己的 catalog/artifact，B 只消费
  A 时不重新编译，Console 直接引用 Jazor 并只执行一次 Emit；
- 对源码引用、本地 NuGet 包、隔离 package consumer 和不同 `PrivateAssets`/
  `buildTransitive` 组合建立同一套发现矩阵。

**出口条件**：上述四种消费方式都能发现且只加载一次 CLR provider；A 的真实 CLR
import 能到达 Console，未引用模块不被复制；工具资产隔离和公共 contract 依赖均有
可观察测试证据。

### U4：一次性切换并删除 CLR 特判

**责任**：`Jazor.Emit`、生成器、Packaging、测试与文档维护者。

工作项：

- 一次性删除旧 catalog 类型探测、`isClrRuntimeCatalog` 分支和 `jazor.clr` 硬编码
  推导；provider id 只来自标准 catalog 自身；
- 删除旧生成 catalog 形状、旧测试夹具和仅服务 CLR 特判的 helper；不保留双读或
  fallback。旧包不再被新 Emit 当作 artifact provider；
- 在架构文档和发布说明中记录这是一次不兼容的载体切换，包消费者必须 lockstep 升级。

**出口条件**：Emit 对 CLR、RazorVue 和未来 provider 走同一读取主线；源码中不存在
按程序集名识别 CLR provider 的业务分支；旧载体的失败信息可操作，且不产生重复输出。

### U5：统一 artifact graph 与长期护栏

**责任**：`Jazor.Emit`/Packaging，协同各 adapter 和 package owner。

工作项：

- 让 generated module catalog、runtime provider、component artifact catalog 和
  package manifest 都进入同一内部 artifact graph；物理 manifest、程序集资源和内联
  内容只是 graph node 的不同存储；
- 统一模块、asset、import-map、provider version、缺失 dependency 和路径冲突的
  诊断代码与确定性排序；
- 为新 provider 提供 schema fixture/consumer test 模板，要求接入者声明入口、闭包、
  资产和版本，不允许通过包名或目录扫描补全；
- 在发布门禁中比较 debug/release/SSR 的 graph，确保只因目标输出模式发生预期物化
  差异，而不是 provider 发现差异。

**出口条件**：新增 provider 不需要修改 `Jazor.Emit` 的 provider 专名分支；graph
  快照在源码、NuGet 和隔离 consumer 中稳定；未使用 entry 的模块和资产不会进入输出。

## 测试与验收门禁

### Provider/Emit 单元与集成

- 标准 inline provider 与 embedded-resource provider 读取后生成等价 `ModuleRecord`；
- schema 版本、空/绝对/逃逸路径、错误哈希、半缺 source map、重复 module id 和
  provider id 冲突都给出显式、可定位诊断；
- 缺失 module dependency 失败时包含 provider、importer 和 dependency path；依赖
  闭包排序稳定，循环依赖不会重复物化或死循环；
- 同一路径相同内容只输出一份，不同内容稳定失败；import-map 只保留激活 provider
  的贡献，冲突 specifier 稳定失败；
- 标准 inline 与 embedded provider 在同一输入下输出相同路径、内容、hash、dependency
  closure 和 manifest 语义；旧 catalog 不属于支持输入。

### CLR 与 compiler 回归

- `Jazor.CLR.Test` 继续验证所有 module 的 identity、hash、可解析性、内部 import 和
  named export；
- `Jazor.EmitTest` 验证无 CLR import、单入口精确闭包、多入口合并、缺失依赖、重复
  provider、source map/manifest 和旧/新载体等价；
- `Jazor.CompilerTest` 验证 authored C# 仍绑定原有 `System/*.js` helper；本路线不得
  通过 raw JavaScript 或 Emit 特判改变 lowering；
- Razor SG 组件和 CLR mapping 回归保持通过，证明 `jazor.vue` 与 `jazor.clr` 在同一
  provider collector 中不会互相污染。

### Package consumer 与宿主

至少覆盖以下矩阵：

| 场景 | 必须证明 |
| --- | --- |
| `A -> B -> Console` 源码项目 | A 的 catalog/provider 可由 Console 发现；B 不重复编译；宿主只 Emit 一次 |
| 同一拓扑的本地 NuGet 包 | provider assembly、catalog、contract 和 build target 的包边界与源码一致 |
| 隔离 package consumer | 不依赖仓库默认 `bin/` 或机器上的旧 DLL；provider 只从当前包恢复 |
| Debug/Release/SSR | 入口和依赖闭包一致，只有目标物化格式不同 |
| 无 CLR 使用的应用 | 输出不包含任何 `System/*.js` CLR module |
| 单一/多个 CLR 入口 | 只保留精确依赖闭包，重复 provider 不重复写文件 |

建议入口：

```bash
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
dotnet test src/Jazor.CLR.Test/Jazor.CLR.Test.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit
```

完成 U3 以后还必须运行仓库现有的本地打包、Windows SPA/SSR 和 isolated package
consumer 门禁；仅程序集反射测试不能证明 NuGet 传递关系正常。所有阶段提交前执行
`git diff --check`，并对新增/修改的 Markdown 做本地链接检查。

## 风险、回滚与版本

| 风险 | 处理 |
| --- | --- |
| provider assembly 迁移造成运行时依赖缺失 | provider 注册、包布局和 isolated consumer 同步验收；失败时回滚整组包版本，不回滚 reader 到旧载体 |
| inline 与 embedded 内容换行/编码不同导致 hash 漂移 | U0 固定 UTF-8/换行规范；迁移前后做内容和 hash 对照，禁止以“语义相同”掩盖 ABI 变化 |
| 同一路径由多个 provider 提供 | 按 provider/module/path 比较完整内容和元数据；不一致直接失败，不按加载顺序取胜 |
| `PrivateAssets`/`buildTransitive` 让 provider 不可发现或工具泄漏 | 用源码、NuGet、隔离 consumer 矩阵验证；把 provider 注册作为中性 artifact item，不把工具资产当资源传播开关 |
| 新 provider 继续添加 Emit 专名分支 | U5 增加 synthetic provider contract 测试和 code review 护栏；provider identity 必须来自 catalog |

schema 的不兼容变化必须递增 schema version，并在发布说明中给出包升级顺序。CLR
provider 的迁移本身若只改变内部载体，可保持现有模块 ABI；若改变外部 provider/package
契约，应按仓库版本规则进入对应 `MINOR`/`PATCH` 评审，不在计划中预先承诺版本号。

## 非目标

- 不把 CLR module 变成组件库的 Vue component，也不让 `Jazor.CLR` 依赖 RazorVue；
- 不强制所有 provider 使用 `manifest.json + dist/`，也不把程序集内嵌资源改成应用级全量
  文件复制；
- 不通过扫描 JavaScript、`node_modules`、程序集名称或包名推断依赖；
- 不扩大 CLR whitelist、carrier、JS interop 或 RazorVue lowering 支持面；
- 不让中间类库在自己的构建中物化最终宿主 artifact；
- 不读取旧 CLR catalog；新版本只接受标准 provider contract。

## 关联文档

- [类库产物与引用契约](../02-architecture/library-artifact-contract.md)
- [产物管线](../02-architecture/artifact-pipeline.md)
- [RazorVue 开发者体验与资源交付](../02-architecture/razor-to-vue.md)
- [安装与配置](../03-guides/installation-and-configuration.md)
