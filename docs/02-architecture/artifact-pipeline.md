# 产物管线

> 适用范围：`Jazor.Compiler` 的生成 carrier、JS resource package、`Jazor.Emit` 物化、source
> map、Netpack bundle、SSR 和 HMR。类库 carrier 的稳定定义见 [类库资源与 Emit 物化一次性统一计划](../04-roadmap/artifact-graph-stabilization-plan.md)。

## 核心边界

Jazor 的类库资源只有两种输入：

| 输入 | 物理形态 | 语义 |
| --- | --- | --- |
| JS resource library | `manifest.json + dist/**` | 已存在的 `.mjs/.js`、CSS、许可证和其他静态资源；C# 只做 binding/authoring contract |
| 纯 Jazor library | 程序集内 `Jazor.Generated.ModuleCatalog`（`ECMAScriptCode`） | C# 经 `Jazor.Compiler` lowering 后携带的生成模块源码和依赖 |

二者是并列的一等输入。`ModuleCatalog` 不是旧实现，`manifest.json + dist` 也不是生成
Jazor 模块的替代品。Emit 可以在内存中把二者归一化为统一资源记录，但该记录不写回 producer、
不作为公开 carrier，也不增加第三种类库类型。

`Jazor.Emit` 不参与 C# 或 Razor 语义降低。它只负责读取两种输入、验证清单、解析显式依赖
闭包、执行冲突检查，并将选定结果物化为最终 JavaScript 输出。

## 职责分层

| 输出或阶段 | 所属组件 | 说明 |
| --- | --- | --- |
| Roslyn `IOperation`、ESTree、模块文本 | `Jazor.Compiler` / `SemanticWalker` / `AstConverter` | 负责 C# 语义、导入收集、source origin 和确定性模块内容 |
| `Jazor.Generated.ModuleCatalog` | `Jazor.Compiler`、RazorVue generator | 纯 Jazor 编译结果的程序集内 carrier；不写最终输出目录 |
| `manifest.json + dist/**` | 各 JS resource library | 已有 JavaScript 及其资源的包 carrier；不经 C# lowering |
| 资源读取、归一化、依赖闭包和冲突校验 | `Jazor.Emit` | 只读取两种 carrier，使用同一套规则 |
| `.mjs/.js/.map/.css`、输出 manifest/import map | `Jazor.Emit` | 已验证资源闭包的物化结果 |
| Release bundle | `Jazor.Emit` + Netpack | 只消费已选闭包；是否 bundle 不改变资源依赖语义 |
| SSR 模块图和 runner | `Jazor.Emit` + ASP.NET Core/DenoHost | 使用同一 carrier 闭包，额外选择显式 SSR 入口 |
| HMR snapshot/update | `Jazor.Emit` + Dev integration | 输出 profile 的更新数据，不是类库 carrier |

## 固定数据流

```text
Jazor.Generated.ModuleCatalog --------┐
                                      ├─ carrier reader
manifest.json + dist/** -------------┘
                                      v
                         in-memory resource records
                                      v
                        roots + explicit dependency closure
                                      v
                            validate / dedupe / conflict
                                      v
                           materialize selected profile
```

### 发现入口

- 程序集闭包只发现精确名称 `Jazor.Generated.ModuleCatalog`。生成模块、source map、HMR
  metadata 和附属资源必须由同一入口关联读取。
- JS resource library 由 MSBuild/NuGet 传递的 manifest locator 定位；locator 只是文件位置，
  不携带 analyzer、generator 或 Emit 资格。
- 不读取 provider 专名 catalog，不按程序集名、CLR 类型名、`System/` 前缀或目录内容推断
  资源，不从已物化输出反向发现 producer。
- 同一输入重复发现时按稳定 identity 去重；内容、路径、owner、类型、hash 或依赖不一致时，
  在写出前返回冲突。

### 资源条目和依赖

JS resource manifest 使用固定 `schemaVersion` 和 `libraryId/version`。资源条目 `type` 只
描述资源语义：`module`、`source-map`、`style`、`license`、`static`。`imports[*]` 是 module
入口；相对模块依赖与 package 依赖分开记录；`requires` 只表示 library 版本约束。

纯 Jazor `ModuleCatalog` 的 module 记录至少包含 `AssemblyName`、`TypeName`、`Id`、
`RelativePath`、`Content`、`Hash`、相对依赖和 package imports。Emit 使用这些声明建立闭包，
不解析 JavaScript 文本猜依赖。

闭包规则：

1. roots 来自应用自身生成模块、显式选择的 package specifier、SSR/HMR 入口和用户声明的
   静态入口。纯 consumer host 若 root assembly 没有自有 catalog module，则其显式提供的
   `ModuleCatalog` modules 作为 consumer roots；仅传递 manifest locator 不会自动形成 root。
2. ModuleCatalog 的相对 dependency 指向同一生成 owner 的 module；package import 指向
   JS resource manifest 的唯一 entry。
3. manifest entry 按当前 profile 选择 development/production 路径，沿显式 module/package
   dependencies 和 `requires` 继续解析。
4. 闭包之外的 module、map、style、license、static 文件不物化；引用程序集不等于全量复制
   其 `dist`。
5. 缺失入口/文件/依赖、版本不满足、路径越界、错误 hash、重复 identity 或输出路径冲突，
   都在目标目录写入前失败。

## 输出 profile

| Profile | 输入选择 | 输出 |
| --- | --- | --- |
| `browser-debug` | 应用 roots + development resource entries | 独立 module、source map、输出 manifest/import map |
| `browser-release` | 应用 roots + production entries | 生产模块和 Netpack bundle/source map |
| `ssr-debug` | browser roots + 显式 SSR runner/Vue server entries | 可诊断 SSR module graph、runner、SSR import map |
| `ssr-release` | SSR roots + production entries | 发布 SSR graph、runner、hydration 所需资源 |
| `hmr-debug` | 与 browser-debug 同一次收集的 roots/closure | 完整当前 module snapshot 和 HMR update metadata |

所有 profile 共享同一 carrier 发现、identity、依赖和冲突规则；profile 只改变选中的入口和
最终投影。输出的 `jazor-manifest.json`、browser/SSR import map、SSR 文件和 HMR envelope
不再作为下一次构建的 producer 输入。

## 物化和失败原子性

Emit 先在目标同卷 staging 中写出完整 profile，再验证每个文件的字节、hash、相对路径和
owner。成功后原子替换最终输出目录；失败、取消、并发冲突或进程中断不替换上一份有效输出，
也不留下新旧混合文件。

Debug、Release、SSR 和 HMR 都使用同一发现和闭包规则；HMR snapshot、SSR worker 状态和 bundle
只是宿主输出层数据。它们不能改变类库 carrier、资源 `type` 或依赖闭包，也不能把输出 manifest
变成资源包 manifest。

## 包和项目引用

- 定义纯 Jazor module 的项目直接引用 Jazor，生成自己的 `ModuleCatalog`；中间类库不会重编译
  或物化上游 catalog。
- JS resource package 的 manifest locator 和 package dependency 可以传递；真实 `dist`
  只由拥有该 manifest 的包提供，最终宿主按闭包一次物化。
- `Jazor`、`Jazor.Vue`、Analyzer、Generator 和 Emit 是工具资格，谁直接使用谁直接引用；
  普通程序集引用不自动传递工具资格。
- NuGet 的 target 只承载明确的工具边界；Jazor 的 `build/Jazor.targets` 负责直接 tooling，
  `buildTransitive/Jazor.Resources.targets` 只添加 manifest locator。Jazor.Vue 只有
  `buildTransitive/Jazor.Vue.targets`，它始终传递 manifest locator，并只在当前项目直接声明
  `PackageReference Include="Jazor.Vue"` 时注册 RazorVue analyzer。analyzer 依赖位于
  `tools/net11.0/analyzers/`，不得使用自动 `analyzers/dotnet/cs` 资产。
- 定义 Jazor module 或 RazorVue 组件的类库以 `PrivateAssets="all"` 直接引用对应工具包；最终
  `Exe`/`WinExe` 宿主直接引用 `Jazor` 后才获得 Emit。
- 源码 ProjectReference 和 NuGet PackageReference 必须产生相同的 carrier 发现、版本选择、
  依赖闭包、去重、冲突诊断和输出字节。

## 禁止项

- 禁止新增或读取第三种类库 carrier，禁止用 provider/descriptor/catalog 专名表示资源形式。
- 禁止 `ArtifactCatalog`、`RuntimeProviderCatalog` 或并列 source-map catalog 成为 Emit 入口；
  它们承载的数据必须归入 ModuleCatalog、manifest/dist 或纯编译期 metadata 的正确 owner。
- 禁止目录扫描、源码 `bin` fallback、隐式全量物化、last-write-wins 和 JavaScript 文本猜图。
- 禁止中间类库执行 Emit、改写上游资源或把工具资产隐式传给下游。

具体一次性实施顺序和门禁见 [类库资源与 Emit 物化一次性统一计划](../04-roadmap/artifact-graph-stabilization-plan.md)。
