# 类库产物与引用契约

> 适用范围：多项目解决方案、NuGet 类库、最终可执行宿主，以及 Jazor 生成模块和上游 ESM/CSS 资源的交付。
>
> 本文是当前版本的规范性边界。工具资产与 artifact/resource 资产已经按下述规则分层；未列入
> contract 的 provider 扩展不得依赖隐式扫描或传递行为。

## 核心结论

`Jazor` 和 `Jazor.Vue` 是作者工具、分析器、生成器、MSBuild/Emit 和 RazorVue 集成的使用资格。它们遵循“谁使用，谁直接引用”原则：

- 定义 Jazor 模块的项目直接引用 `Jazor`。
- 定义 RazorVue 组件的项目直接引用 `Jazor.Vue`，并同时直接引用 `Jazor`。
- 负责最终 `debug`/`release` Emit 的可执行或 Web 宿主直接引用 `Jazor`；RazorVue 宿主再直接引用 `Jazor.Vue`。
- 只消费上游程序集、没有定义自己的 Jazor/RazorVue 输入的中间类库，不得为了传递上游 catalog 而增加 `Jazor` 或 `Jazor.Vue` 引用。

这里的“不传递”是项目能力和构建行为的边界，不是否认必要的运行时 contract 还原。
`Jazor`/`Jazor.Vue` 作为其他包的运行时依赖时，nuspec 必须使用
`exclude="Build,Analyzers"`，因此只传递编译/运行所需的 contract，不传递 analyzer、generator、
build target 或 Emit 资格。需要使用工具链的项目必须直接引用对应包；类库项目仍可用
`PrivateAssets="all"` 隔离自己的工具引用。

生成模块、运行时模块、上游 ESM/CSS、许可证和 source map 则属于另一条 artifact 依赖图。它们可以有意随类库传递，但必须通过声明的 catalog、provider 或 package manifest 进入图，不能靠包名、程序集名或目录扫描推断。

## 三种依赖

规范中“直接引用”和“传递”分别指不同层次：

| 依赖类别 | 典型内容 | 默认规则 |
| --- | --- | --- |
| 工具/作者依赖 | analyzer、source generator、build target、Emit CLI、Razor hook | 使用项目直接引用；类库打包时不把工具资格无意传给下游 |
| 运行时 contract 依赖 | 公共 API 使用的 `ECMAScript`/Vue contract 程序集、应用运行时 | 如果下游编译或运行确实需要，可以正常传递；不能用 `PrivateAssets` 把必需 contract 隐藏掉 |
| artifact/resource 依赖 | generated catalog、runtime provider、ESM、CSS、许可证、source map | 通过统一 artifact graph 声明并按实际入口闭包传播；最终宿主只物化一次 |

因此，“谁使用谁直接引用”首先约束第一类依赖。它不要求把公共运行时 contract 或应用实际使用的 ESM/CSS 依赖伪装成不存在，也不允许把工具依赖当成资源传播机制。

## 三张图

这三类关系不能混为一谈：

```text
.NET 编译引用图
A（定义模块） -> B（消费 A） -> Console（最终宿主）

Jazor 工具图
A: Jazor 直接引用（通常 PrivateAssets=all）
B: 无 Jazor 工具引用
Console: Jazor 直接引用 + JazorMode=debug/release

Artifact/resource 图
A.dll 的 ModuleCatalog ----------------------┐
组件库的 ArtifactCatalog / RuntimeProvider --+-> Console 的 Jazor.Emit
package manifest 的 selected import closure -┘
```

工具图决定谁可以编译和物化；artifact/resource 图决定最终输出需要哪些文件。一个项目引用了某个程序集，不表示它必须重新编译该程序集，也不表示它可以自动启用该程序集作者使用的全部工具。

## 项目角色与直接引用

| 项目角色 | 直接 `Jazor` | 直接 `Jazor.Vue` | `JazorMode` | 责任 |
| --- | --- | --- | --- | --- |
| ECMAScript 模块类库 | 必须 | 不需要 | `none` | 编译自己的 `[ECMAScriptModule]` 输入并随定义程序集携带 catalog |
| RazorVue 组件类库 | 必须 | 必须 | `none` | 编译自己的 `.razor`/Razor SG 输入并随定义程序集携带组件 artifact |
| Vue 绑定/组件封装类库 | 按是否生成自己的模块决定 | 公共 API 使用 Vue 类型时必须 | `none` | 声明自己的 binding 和 package artifact 入口，不负责最终 Emit |
| 只消费 A 的中间类库 | 不因 A 而增加 | 不因 A 而增加 | `none` | 正常引用 A，不重新编译 A，不负责最终 Emit |
| 既消费又定义模块的中间类库 | 必须 | 按是否有 RazorVue 输入决定 | `none` | 只为自己的输入启用对应工具 |
| 核心最终宿主 | 必须 | 不需要 | `debug` 或 `release` | 收集引用程序集 catalog 并执行 Emit |
| RazorVue 最终宿主 | 必须 | 必须 | `debug` 或 `release` | 收集组件、runtime provider 和 package asset closure 并执行 Emit |

类库中的工具引用默认应隔离，并按项目角色选择：

```xml
<!-- 只定义普通 ECMAScript 模块的类库；这是工具资产隔离目标的示意。 -->
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.25.0" PrivateAssets="all" />
</ItemGroup>

<!-- 定义 RazorVue 组件的类库；这是工具资产隔离目标的示意。 -->
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.25.0" PrivateAssets="all" />
  <PackageReference Include="Jazor.Vue" Version="0.25.0" PrivateAssets="all" />
</ItemGroup>
```

最终宿主的 `Jazor`/`Jazor.Vue` 是宿主自己的直接选择。`PrivateAssets="all"` 不得用于隐藏一个确实属于公共 API 或最终 artifact 图的运行时/绑定依赖；这类依赖应作为正常包依赖发布，或者由后续拆分后的 runtime contract 包明确承载。当前 `Jazor.Vue` 同时包含 authoring payload 和若干运行时/绑定 payload，类库封装时必须按实际公共 API 和资源需求判断，不能机械套用一个标志。

上面的示意与包边界配合使用：手工 nuspec 对 `Jazor`/`Jazor.Vue` 依赖统一声明
`exclude="Build,Analyzers"`，让公共 contract 正常到达下游，同时阻断工具资产；定义自己
输入的类库再用 `PrivateAssets="all"` 隔离直接工具引用。不能只依赖项目模板中的
`PrivateAssets`，因为它不会修改手工 nuspec。

`PrivateAssets` 只控制项目引用的资产是否流入下游项目，不会修改手工编写的 nuspec。包作者不能一边在项目中设置 `PrivateAssets="all"`，一边在 nuspec 中重新列出 `Jazor`/`Jazor.Vue`，然后宣称工具依赖已经被隔离。

## A -> B -> Console

### 推荐结构

```text
A
  定义 Jazor 模块
  直接引用 Jazor（类库工具资产私有）
  JazorMode=none

B
  只引用 A
  不引用 Jazor/Jazor.Vue
  JazorMode=none

Console
  引用 B（从而得到 A 的普通程序集依赖）
  直接引用 Jazor
  JazorMode=debug 或 release
```

编译和物化顺序是：

1. A 的编译器/生成器在 A 自己的编译中生成 `Jazor.Generated.ModuleCatalog`；A.dll 是该 catalog 的所有者。
2. B 只把 A 当作普通 .NET 引用，不会因为 A 的 catalog 而再次编译 A，也不会自动获得 A 的 analyzer 规则。
3. Console 的 `Jazor.targets` 将根程序集和 `ReferenceCopyLocalPaths` 中的程序集交给 `Jazor.Emit`。只要 A.dll 被正常复制到宿主输出，Emit 就能读取 A 的 catalog；CLI 场景也可以通过显式 `--assembly` 提供它。
4. Emit 合并 Console、B、A 以及显式 provider 的 catalog，按模块 ID/相对路径去重并检查内容冲突，然后写出宿主自己的 `jazor/` 输出。

如果 B 也声明了自己的 `[ECMAScriptModule]` 或其他 Jazor 输入，B 必须像 A 一样直接引用 `Jazor`；如果 B 定义 RazorVue 组件，则还必须直接引用 `Jazor.Vue`。引用 A 本身不是工具资格的来源。

### 资源传播

A 的生成模块不应复制成 B 的源文件，也不应由 Console 重新编译。A 的 package 若需要让消费者看到公共 ECMAScript 类型或其运行时资源，必须分别声明相应的正常运行时/绑定依赖；不能把 `Jazor` 工具包当作资源传递开关。

最终宿主只负责一次物化。类库不得在自己的 build 中写入最终宿主的 `jazor/` 目录，也不得把临时生成目录作为包内容让下游再次扫描。

## 统一 artifact 机制

对外应只有一张逻辑 artifact graph。其 provider/manifest 条目至少能表达：

- provider identity、schema version 和版本要求；
- generated module、embedded runtime module、relative path、内容哈希和 source map；
- package import root、development/production 依赖和相对 ESM closure；
- styles、静态文件、许可证和其他随 entry 激活的文件；
- import-map contribution 与 provider 间依赖；
- 相同路径冲突、缺失依赖和版本不兼容的确定性诊断。

消费者只依赖这张图，不依赖 provider 的物理实现。当前可以继续使用不同载体，但其语义必须映射到同一张图：

| 当前载体 | 所有者 | 在统一图中的含义 |
| --- | --- | --- |
| `Jazor.Generated.ModuleCatalog` | `Jazor.Compiler` | 普通类库生成模块 |
| `Jazor.Generated.ArtifactCatalog` | `Jazor.RazorVue` 等 adapter | 组件 artifact、source map、资产和 package imports |
| `Jazor.Artifacts.RuntimeProviderCatalog` | adapter/runtime provider | 嵌入 runtime 模块、相对依赖和 import-map |
| `Jazor.Artifacts.RuntimeProviderCatalog` | `Jazor.CLR`/`ECMAScript` 运行时 | CLR runtime 模块及其精确依赖闭包 |
| package `manifest.json` + `JazorLibraryManifest` | 绑定/组件包 | ESM/CSS entry、模式依赖和文件 closure |

组件封装库如果依赖另一个组件库的 ESM/CSS，必须在 artifact 图中声明该依赖。不能因为 C# 项目能编译，或因为某个包在 restore 图中出现，就假定最终 host 会复制它的资源；也不能要求每个 host 手工重复列出 wrapper 的全部上游 manifest。

## 当前实现

当前 `v0.25.0` 已具备以下基础：

- `Jazor.Emit` 结构化读取普通 module catalog、RazorVue artifact catalog 和 runtime provider catalog；
- `ModuleCollector` 以真实入口跟随 provider/CLR 的声明依赖，并检查路径、内容和 provider 冲突；
- `Jazor.targets` 从根程序集、`ReferenceCopyLocalPaths` 和显式 provider 收集程序集；
- `LibraryMaterializer` 按生成模块的 `PackageImports` 选择 manifest entry，并物化声明的 ESM closure、styles 和许可证。

已完成的 package 边界包括：

- 所有组件/绑定包对 `Jazor`/`Jazor.Vue` 的依赖均排除 `Build,Analyzers`；公共 contract 和运行时程序集仍按正常依赖还原；
- 需要 analyzer、generator、build target 或 Emit 的最终项目显式直接引用 `Jazor`，RazorVue 项目显式直接引用 `Jazor.Vue`；
- 组件包自身的 `buildTransitive` 只注册 package manifest，负责 artifact/resource 传播，不授予下游工具资格；
- `A -> B -> Console` 中 A 的 catalog/provider 由 Console 的一次 Emit 收集，B 不重复编译或物化；
- runtime provider 的模块和资产都在真实入口闭包激活后进入输出，未引用 provider 不会污染 manifest。

## 验收清单

新增或拆分类库时，至少验证：

- 定义输入的项目直接引用所需工具；只消费上游的项目没有无理由的工具引用；
- 类库默认不写宿主 artifact，最终 host 只执行一次 Emit；
- 类库的 generated catalog 能在最终 host 的引用复制集合中被发现；
- package ESM/CSS/许可证通过声明的 artifact dependency 到达 host，未使用 entry 不被复制；
- `Jazor`/`Jazor.Vue` 的 analyzer、generator、build target 不因普通类库依赖意外启用；
- 缺失 catalog、provider、manifest entry、closure 文件或冲突路径都在构建/Emit 时显式失败。

相关实现说明：[产物管线](./artifact-pipeline.md)、[Razor-to-Vue](./razor-to-vue.md)、[安装与配置](../03-guides/installation-and-configuration.md)。统一 CLR module/provider 的实施顺序见[CLR Module Artifact Provider 统一路线图](../04-roadmap/clr-artifact-provider-unification.md)。
