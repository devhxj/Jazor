# 类库资源与 Emit 物化一次性统一计划

> 状态：已完成一次性切换并通过当前全仓验证。本文保留冻结后的资源 carrier、类库引用传递与
> Emit 物化契约，以及实施完成证据；不设计兼容期、双读路径、降级格式或第三种类库形态。
> 文中的旧符号只用于已移除实现的盘点，不能被当作新的产品名或公开契约。

## 实施完成证据

| 工作包 | 状态 | 当前证据 |
| --- | --- | --- |
| U0-U2 | 完成 | 所有 11 个 JS resource manifest 使用 schema 2；`src/ECMAScript` 和 `src/Jazor.Vue` 以 `manifest.json + dist/**` 交付；纯 Jazor/RazorVue 只生成 `Jazor.Generated.ModuleCatalog` |
| U3 | 完成 | Emit 只读取 `ModuleCatalog` 和 manifest；旧 provider/artifact/source-map 顶层 reader 已移除，模块、source map 和 assets 均从所属 carrier 关联读取 |
| U4 | 完成 | `Jazor.targets` 只在最终 `Exe`/`WinExe` 调用 Emit；源码与 NuGet 的 A -> B -> Console 回归均证明工具不传递、资源闭包传递 |
| U5 | 完成 | Debug、Release、SSR、HMR 共用发现/闭包和 staging 原子提交；Emit 回归覆盖模块、bundle、SSR、source map、import map 与失败路径 |
| U6 | 完成 | 没有兼容 reader、目录 fallback 或第三种 carrier；`dotnet run --file scripts/csharp/test-dotnet.cs` 当前通过 |

## 1. 先固定结论

类库只有两种形式。这里的“形式”指类库如何携带 JavaScript 资源，不指 C# API、框架集成或
Debug/Release 输出模式。

| 类库形式 | 适用对象 | 唯一资源 carrier | 谁负责产生 JavaScript |
| --- | --- | --- | --- |
| **JS resource library** | Vue、Vuetify、Pinia、`src/ECMAScript` 以及已有外部 `.mjs/.js` 的绑定库 | 包内 `manifest.json + dist/**` | 上游/资源库已经提供；C# 只提供映射和 authoring contract |
| **纯 Jazor library** | 开发者用 Jazor 编写、需要由 Jazor 编译的类库 | 生成的 `Jazor.Generated.ModuleCatalog`，即 `ECMAScriptCode` | `Jazor.Compiler` 从 C# 生成模块内容并写入 catalog |

二者地位相同，都可以被 `Jazor.Emit` 发现、解析、裁剪和最终物化；二者不互相伪装，也不
互相转换成另一种类库形式。`ModuleCatalog` 不是旧实现或过渡格式，`manifest.json + dist`
也不是纯 Jazor 的替代 carrier。类库形式由资源的真实来源决定，不由引用者、输出 profile 或
是否使用 RazorVue 决定。

### 1.1 名称和边界

- `ModuleCatalog` 是固定名称；`ECMAScriptCode` 只是描述其“由 C# 携带 ECMAScript 源码”的
  角色，不是另一个类型名，也不是第三种 carrier。文档和代码都以 `ModuleCatalog` 为类型名。
- `ModuleCatalog` 是 manifest-like 的 C# 内部 carrier。它至少携带生成模块的 id、路径、
  源代码、hash、模块依赖和 package import；与该模块对应的 source map 可以作为同一记录的
  关联数据。HMR snapshot/update metadata、bundle 和 SSR runner 不属于类库 carrier，必须留在
  输出层。
- `src/ECMAScript` 是 JS resource library。它最终必须是 `manifest.json + dist/**`；不能因为
  analyzer/source generator 只能生成 C#，就把它误分类为纯 Jazor library。分析阶段生成的
  C# 只能是构建步骤，最终包仍是资源包。
- 已移除实现的承载归属如下。表中的旧符号只允许出现在迁移清单、测试迁移说明和历史记录中，
  不能继续作为 Emit 读取入口、包资产或新的抽象名：

  | 现有实现位置/符号 | 实际内容 | 目标归属 | 处理要求 |
  | --- | --- | --- | --- |
  | 已删除的 `src/ECMAScript/Catalog.g.cs` 中的 `RuntimeProviderCatalog` | `Jazor.CLR` lowering 后的 `System/**` JS | `src/ECMAScript/manifest.json + dist/**`（JS resource library） | 生成器直接写资源包；C# 生成文件不再作为运行时读取入口 |
  | 已删除的 `src/Jazor.RazorVue/Runtime/ArtifactRuntimeProviderCatalog.cs` | RazorVue 预制 helper `.mjs` 及 import-map 贡献 | 拥有这些 helper 的 JS resource package 的 `manifest.json + dist/**` | 每个文件声明 module/依赖；不借 provider 名称传递 |
  | 已删除的 RazorVue `ArtifactCatalog` | Razor SG 之后由 Jazor 生成的组件模块、map 和资源元数据 | `Jazor.Generated.ModuleCatalog`（纯 Jazor library） | 模块内容并入现有 `ModuleCatalog`，不只删除类型 |
  | 已删除的 `ModuleSourceMapCatalog` | 纯 Jazor module 的 source map | 对应 `ModuleCatalog` module 的关联记录 | source map 由同一 catalog entry 关联读取 |
  | `RazorVue.RouteCatalog`、`RazorSourceTextCatalog` 等 | 编译/分析阶段 metadata | 编译层内部 | 不得被 Emit 发现，也不得转成类库资源 |
- `RazorVue.RouteCatalog`、`RazorSourceTextCatalog` 等只供编译/分析使用的 C# 元数据不是类库
  资源 carrier，也不能被 Emit 当作第三种类库。它们不进入资源发现闭包。

### 1.2 什么属于输出层

Emit 物化后得到的 `.mjs/.js/.map/.css`、输出 `jazor-manifest.json`、浏览器 import map、
Release bundle、SSR runner/文件和 HMR snapshot 都属于最终 JavaScript 输出层。它们只消费
两种类库 carrier 的归一化结果，不能反过来作为资源发现输入，也不新增 library type、catalog
或 provider。输出层可以有内部 plan/record，但这些对象不是类库 carrier，不写回程序集或
NuGet 包。

## 2. 稳定资源契约

### 2.1 JS resource library 的包契约

每个资源类库的包根固定包含：

```text
<library-package-root>/
  manifest.json
  dist/
    ...
```

`manifest.json` 描述已有资源和资源间的显式关系；`dist` 是被描述的实际字节。现有
`imports/requires/styles/files` 结构继续作为基础结构，资源条目的语义用稳定的 `type` 精确
描述。`type` 描述“这一个资源条目是什么”，不描述类库形式、生成器、CLR、RazorVue、provider
或输出 profile。

本次一次性切换将 manifest 的 `schemaVersion` 提升为 `2`，所有资源包同时重写，Emit 不保留
`schemaVersion: 1` reader。字段形状保持稳定，只把资源条目显式类型化：`imports[*]` 增加
`type: "module"`；module 的相对依赖使用 `developmentModuleDependencies`/
`productionModuleDependencies`，package 依赖继续使用现有的 `developmentDependencies`/
`productionDependencies`；`styles`、根 `files` 和 entry `files` 使用 `{ type, path, hash }`
记录。这样 package 版本约束（`requires`）与资源条目不会混在一起。

首批允许的 `type`：

| `type` | 语义 | 必需信息 |
| --- | --- | --- |
| `module` | 可执行 ESM/JavaScript 模块 | logical specifier 或 module id、实际路径、hash、对应 profile 路径、显式依赖 |
| `source-map` | 某个 module 的 source map | 关联 module id、实际路径、hash；不能孤立存在 |
| `style` | CSS 等样式资源 | 实际路径、hash |
| `license` | 许可证/通知文件 | 实际路径、hash |
| `static` | 其他随包静态文件 | 实际路径、hash |

现有字段与 `type` 的关系必须固定：`imports[*]` 记录必须显式带 `type: "module"`，其
`files` 记录 module 的附属资源；`styles` 中的记录必须带 `type: "style"`；根 `files` 中的
许可证记录带 `type: "license"`，其他文件带 `type: "static"`。source map 作为关联的
`type: "source-map"` 记录指向唯一 module id。为保持结构稳定，`requires` 仍只表达 library
版本约束，不伪装成资源条目；需要增加资源语义时在同一条目结构中增加新的 `type` 和该类型的
必需字段，不另建 carrier。不得用 `js-resource`、`modulecatalog`、`clr`、`runtime-provider`
等类库/载体名称充当类型。

校验规则：

1. `schemaVersion`、`libraryId`、`version`、路径、`type` 和该类型必需字段必须存在且可验证。
2. 所有路径只能位于 manifest 所属包根，统一为 `/`、不允许 `..`、绝对路径或 URL；实际文件
   读取后的字节必须与 `hash` 一致。
3. development/production 选择、入口依赖和 `requires` 必须显式声明；Emit 不解析 JavaScript
   文本猜依赖，也不因目录存在就全量复制。
4. 同一 `libraryId + version + logical identity + type` 只能对应一个内容和依赖集合；同一
   输出路径被不同 identity 占用时直接失败，不能 last-write-wins。
5. 未知 `type`、缺少关联 module、缺文件、错误 hash、非法路径、循环或无法满足的依赖约束，
   都在写目标目录前失败。

### 2.2 纯 Jazor library 的 ModuleCatalog 契约

`Jazor.Compiler` 生成的唯一顶层类型是：

```text
Jazor.Generated.ModuleCatalog
```

它存在于类库/应用程序集内，原因是 analyzer/source generator 的标准输出能力是添加 C#。
它不是运行时 CLR catalog，也不是 Emit 输出目录中的文件。当前已有的模块字段必须保留：

| 字段 | 用途 |
| --- | --- |
| `AssemblyName`、`TypeName`、`Id` | owner、源码符号和稳定逻辑身份 |
| `RelativePath` | 模块在最终图中的相对路径 |
| `Content`、`Hash` | 已完成 lowering 的 JavaScript 字节和完整性校验 |
| `PackageImports` | 模块对 JS resource library 入口的显式引用 |
| `Dependencies` | 生成模块之间的相对依赖；不能由 Emit 重新猜测 |
| source-map/HMR/附属资源关联字段 | 只在需要时出现，并与对应 module 一起校验 |

source map 可以在实现内部以字段或嵌套记录表达，但必须由同一个 `ModuleCatalog` 入口读取；
不能继续新增 `ModuleSourceMapCatalog`、`ArtifactCatalog` 等并列顶层 carrier。RazorVue 生成的
组件模块若是 C# 经 Jazor 编译得到，也使用这个 `ModuleCatalog`；Vue framing 仍由
`Jazor.RazorVue` 负责，但不改变 carrier 名称。

### 2.3 Emit 内部归一化记录不是第三种形式

Emit 可以在内存中把两个 carrier 映射为同一套内部资源记录，以便做去重、依赖闭包和冲突校验。
该记录：

- 不写回程序集、包或输出目录；
- 不作为公开 API、MSBuild carrier 或新的 catalog 名称；
- 必须保留 owner、logical identity、`type`、路径、字节、hash、依赖和来源信息；
- 完成预检后才进入物化阶段。

因此“统一”指统一收集和物化算法，不是把两种类库强行做成同一种输入文件。

## 3. 引用、依赖和传递

必须把四类关系分开，否则会把工具传递误当成资源传递：

| 关系 | 约束 |
| --- | --- |
| C# authoring/reference | 决定类型和映射是否能编译；按普通项目/NuGet 引用解析 |
| Jazor tooling reference | `Jazor`、`Jazor.Vue`、Analyzer、Generator、Emit 等只对直接使用它的项目启用；普通程序集引用不自动取得工具资格 |
| JS resource dependency | 来自资源 manifest 的入口、`requires` 和 profile-specific dependencies；随包依赖传递 |
| generated module dependency | 来自 `ModuleCatalog` 的 module/import 记录；随程序集/包传递，按闭包选取 |

工具引用不传递，资源依赖要传递。中间类库不重新编译、复制或物化上游资源；只有最终宿主
选择 root 并调用一次 Emit。

### 3.1 A -> B -> Console 的固定行为

| 场景 | A | B | Console 最终行为 |
| --- | --- | --- | --- |
| 外部资源链 | A 包含 `manifest.json + dist` | B 的 manifest/包依赖显式声明 A 的资源入口 | Console 解析 B 的入口和 `requires`，收集 A 的 manifest/dist，闭包内每个资源只物化一次 |
| 纯 Jazor 链 | A 程序集包含 A 的 `ModuleCatalog` | B 只消费 A 时不重编译 A；B 自己若写 Jazor 才直接引用 Jazor 并生成 B 自己的 catalog | Console 收集 B、A 以及应用自身 catalog，沿 module/package imports 一次物化 |
| 混合链 | A 可以是任一形式 | B 不改写 A 的 carrier；自己的资源仍使用自己的形式 | Console 同时读取两种标准 carrier，统一校验后输出一个资源闭包 |

具体规则：

1. B 引用 A 的 C# 程序集，只表示 B 可以使用 A 的 public API；不表示 B 获得 Jazor
   analyzer/generator，也不表示 B 重新生成 A 的 JavaScript。
2. B 若需要编写纯 Jazor 源码，B 必须直接引用 Jazor；B 生成的 `ModuleCatalog` 只含 B 的
   modules。A 的 catalog 作为显式依赖保留。
3. JS resource library 的 C# binding 可以被多个类库引用，但真实 `.mjs/.js` 只由拥有
   manifest/dist 的包提供；依赖包只传递 manifest 定位和版本约束，不复制上游 `dist`。
4. Console 必须能从源码项目引用和 NuGet 项目引用得到相同的 catalog/manifest 定位、版本
   解析、依赖闭包、去重和冲突诊断；不能以“程序集存在”推断全量资源。
5. `Jazor`、`Jazor.Vue` 等工具包不通过普通传递引用自动注入。最终宿主直接引用并配置
   自己需要的工具/Emit；运行时资源依赖仍按上面的显式规则传递。
6. NuGet 的 `build/` 是唯一的直接 tooling activation 边界；`Jazor` 的
   `buildTransitive/Jazor.Resources.targets` 与 `Jazor.Vue` 的 `buildTransitive/Jazor.Vue.targets`
   只传递 manifest locator。analyzer 依赖位于 `tools/net11.0/analyzers/`，不能使用自动
   `analyzers/dotnet/cs` 资产绕过该边界。

## 4. 统一 Emit 主线

```text
ModuleCatalog（纯 Jazor） -------┐
                                  ├─ 统一内存资源记录（实现细节）
manifest.json + dist（JS resource）┘
                                  |
                     roots + explicit dependencies
                                  v
                       validate -> select closure
                                  v
                  materialize one output profile at a time
```

### 4.1 发现阶段

- 从最终宿主提供的程序集闭包中只发现 `Jazor.Generated.ModuleCatalog`；不读取
  `ArtifactCatalog`、`RuntimeProviderCatalog` 或 provider 专名类型。
- 从 MSBuild/NuGet 传递的 manifest locator 读取 JS resource library；locator 只是文件定位
  信息，不是第二种 carrier，也不携带 analyzer/generator 资格。
- 读取 manifest 后验证所有声明的 `dist` 文件、`type`、hash、入口和依赖；不扫描包目录补全
  清单，不解析 JS 文本补全图。
- 发现顺序、去重顺序、冲突诊断顺序都必须稳定；相同 identity/相同字节只保留一份，不同
  owner、路径、类型、字节或依赖声明必须失败。

### 4.2 依赖闭包

1. roots 来自应用自身生成模块、显式选择的 resource specifier、SSR/HMR 明确入口和用户
   配置的静态入口。若最终宿主是纯 consumer、root assembly 没有自己的 ModuleCatalog
   module，则宿主显式提供的 catalog modules 作为 consumer roots；仅有 manifest locator
   而没有 module/package root 时，不得凭 locator 自行产生资源 roots。
2. ModuleCatalog module 的相对依赖沿 `Dependencies` 走；bare package import 沿
   `PackageImports` 找到对应 JS resource manifest entry。
3. JS resource entry 沿当前 profile 的 dependencies 和 `requires` 走；`requires` 必须
   解析到唯一可用版本，版本冲突或依赖缺失直接失败。
4. 依赖闭包之外的模块、source map、style、license、static 文件不进入输出；不能因为某个
   程序集被引用就复制其全部 `dist`。
5. 闭包完成后再次检查每个 module import 的目标存在且 owner/version 唯一，防止“清单可读但
   浏览器运行时 undefined”。

### 4.3 物化阶段

物化只接收已经校验的内部记录：

- Debug：保留模块、source map、输出 manifest 和 import map；
- Release：保留选中模块、资源和 bundle 所需记录；是否 bundle 不改变依赖闭包；
- SSR：使用同一闭包，额外选择 server runner、Vue server renderer 等明确入口；
- HMR：使用同一次物化的完整模块快照和 source origin；更新消息不是新的 library carrier；
- style/license/static：按 `type` 和显式 owner 路径复制；不参与 module identity；
- 任何预检失败、取消、并发冲突或写入失败都使用 staging，不能留下半套宿主输出。

输出侧的 `jazor-manifest.json`、import map、SSR 文件和 HMR envelope 只描述本次物化结果，
不得被下一次构建当成 producer manifest 读取。

## 5. 一次性实施工作包

### U0：冻结契约和现状盘点（完成）

- 固定本文两类 library 形式、`ModuleCatalog` 顶层名称、JS resource `manifest.json +
  dist` 包根、`type` 枚举和冲突规则。
- 记录历史实现曾把 module、source map、HMR、assets 和依赖分散到哪些载体，并逐项标注
  其最终 owner；`ModuleSourceMapCatalog`、`Jazor.Generated.ArtifactCatalog`、
  `Jazor.Artifacts.RuntimeProviderCatalog` 及 RazorVue provider helper 仅作为退役清单，
  不是当前产物或 Emit 读取入口。
- 为每个现有条目标注最终 owner：纯 Jazor 生成内容进入其程序集 ModuleCatalog；已有/手写
  JS 资源进入拥有它的 JS resource package；编译期 route/source metadata 留在编译层。

### U1：收敛纯 Jazor 生成 carrier（完成）

- 修改 `Jazor.Compiler/ESGenerator`：在同一个 `Jazor.Generated.ModuleCatalog` 中保留现有
  module 内容、hash、package imports，并承接相对 dependencies、source-map 和必要的 HMR/
  附属数据。
- 修改 RazorVue 最终 compilation 输出，使生成的组件模块进入同一 `ModuleCatalog`；保留
  Vue framing、诊断、source origin 和 deterministic ordering。
- 删除 `ArtifactCatalog` 和 `ModuleSourceMapCatalog` 作为 Emit 入口；删除的是重复顶层
  carrier，不是它们携带的内容。
- 纯 Jazor library 的程序集引用/打包测试必须证明 catalog 字节在 A -> B -> Console 链上
  不被重新编译或改写。

### U2：收敛 JS resource library（完成）

- 将 `src/ECMAScript/Catalog.g.cs` 当前承载的每一个 CLR runtime module 逐项写入
  `src/ECMAScript/dist/System/**`，并在 `src/ECMAScript/manifest.json` 中声明 `module`、
  source map、依赖和 hash；不能全量 runtime 化或遗漏任何导出/import/ABI。
- 现有外部 binding 包继续使用各自 `manifest.json + dist/**`；`ECMAScript` 与 Vue/Vuetify/
  Pinia 等资源库采用同一资源契约，不另造 CLR/provider manifest。
- `Jazor.RazorVue` 中已经存在的预制 runtime `.mjs` 若不是 C# 经 Jazor 生成，也必须归档到
  其拥有的 JS resource package 的 manifest/dist；不得继续使用 provider catalog 携带。
- 资源生成先写同卷 staging，校验完整清单、字节和 hash 后再一次性替换；失败不产生新旧混合
  的 `manifest/dist`。

### U3：改造 Emit 发现和闭包（完成）

- `CatalogReader` 只保留 `Jazor.Generated.ModuleCatalog` 读取；删除 `ArtifactCatalog`、
  `RuntimeProviderCatalog` 和 provider 专名分支。
- `ModuleCollector`、`LibraryMaterializer`、`ModuleWriter` 共用同一套 identity/type/path/
  hash/dependency 校验；允许不同输入 carrier，禁止不同语义的隐式转换。
- 由显式 module/package dependency 取代当前按 provider、路径前缀或 JavaScript 文本猜图；
  保留现有 CLR module closure DCE，但入口必须来自实际编译模块依赖。
- package/source 引用、版本约束、重复资源和错误码在 Debug、Release、SSR、HMR 中保持相同。

### U4：工具和资源引用传递（完成）

- 为每个资源 package 提供 manifest locator 的明确 MSBuild/NuGet 资产；locator 可传递，工具
  资格不可传递。
- `Jazor` 的 `build/Jazor.props`、`build/Jazor.targets` 只在直接 PackageReference 时导入；
  `buildTransitive/Jazor.Resources.targets` 只含 manifest locator。Jazor.Vue 只有一个
  `buildTransitive/Jazor.Vue.targets`：它始终传递 manifest locator，仅在当前项目直接声明
  `PackageReference Include="Jazor.Vue"` 时注册 analyzer。间接 Jazor/Jazor.Vue NuGet dependency
  回归证明 analyzer/generator/Emit 不会因普通类库依赖激活；资源 package 自己的 manifest
  locator 继续按其显式 package dependency 传递。
- 直接引用 Jazor 的项目才能生成自己的 ModuleCatalog；只消费上游 catalog 的类库不重新运行
  generator；最终宿主直接引用 Emit 并收集整个程序集/资源闭包。
- 增加源码项目和 NuGet 项目的 A -> B -> Console 矩阵，覆盖纯 Jazor、JS resource、混合链和
  多版本冲突。

### U5：输出 profile 和失败原子性（完成）

- Debug、Release/bundle、SSR、HMR 都只消费同一份闭包结果，各自只改变物化投影。
- 保留 source map、import map、style/license、SSR runner、HMR generation 和错误定位；不让
  profile 反向改变 library type 或依赖规则。
- staging、取消、进程重启、并发写入和重复构建都必须留下上一份完整有效输出或空目标，不得
  留下部分文件。

### U6：一次切换和清理（完成）

- U1-U5 的 producer、package targets、Emit、测试和文档同一版本切换；这是破坏性的一次切换，
  不发布旧/新双格式，不保留兼容 reader、旧 catalog 转换器或目录 fallback。
- 只有在新的 manifest/dist、ModuleCatalog、引用传递、所有 profile 和失败门禁通过后，才删除
  旧 generated catalog 文件和 provider 代码；删除动作与同一提交中的消费端切换绑定。

## 6. 验收门禁

### Carrier 和资源完整性

- 每个 JS resource package 都有可复现的 `manifest.json + dist/**`；schema、条目排序、路径、
  hash、依赖和版本稳定。
- 每个纯 Jazor library/应用只产生 `Jazor.Generated.ModuleCatalog`；生成模块、source map、
  HMR/附属元数据都能从该入口读取。
- `src/ECMAScript` 的 `System/**` 资源与原 `Catalog.g.cs` 逐模块比对：文件数、路径、导出、
  内部 import、依赖、字节和 hash 全部一致。
- 不存在被 Emit 读取的 `ArtifactCatalog`、`RuntimeProviderCatalog` 或并列 source-map catalog；
  编译期 route/source catalog 不会被当作资源输入。

### 引用和闭包

- A -> B -> Console 的源码/NuGet 两条链得到相同的资源集合、版本选择、去重和冲突结果。
- 只有 manifest locator、没有任何 module/package root 时不输出 `System/**`；纯 consumer
  host 的 catalog modules 作为显式 roots 后，只输出这些 roots 的声明依赖闭包。
- 未使用的 external entry、SSR/devtools/style/license 不会因程序集引用被全量复制。
- 缺失 manifest、入口、文件、依赖、错误 hash、非法路径、重复 identity 或路径冲突，都在写
  目标目录前失败并给出 owner/specifier/path。

### 功能 profile

- `dotnet build Jazor.slnx` 通过；`Jazor.CompilerTest`、`Jazor.CLR.Test`、`Jazor.EmitTest`、
  Razor SG 集成测试和相关 package consumer 全部通过。
- Debug 的模块/source map/import map、Release bundle、SSR runner/hydration、HMR snapshot 和
  style/license/static 输出都能实际运行；不只检查 JSON 或生成文本。
- 同一输入重复构建 byte-for-byte deterministic；失败、取消、并发、重启后没有半套输出。

当前门禁已通过：类库 carrier 只有 `manifest.json + dist` 与
`Jazor.Generated.ModuleCatalog/ECMAScriptCode`，输出 profile 只有对这两种 carrier 的物化投影。
