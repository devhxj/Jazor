# 产物管线

> 适用范围：`Jazor.Compiler` 的生成 carrier、JS resource package、`Jazor.Emit` 物化、source map、Netpack bundle、SSR 和 HMR。类库 carrier 的稳定定义由本文和[类库资源与引用契约](./library-artifact-contract.md)共同给出；交付状态见[当前状态](../04-roadmap/current-status.md)。

## 核心边界

Jazor 的类库资源只有两种输入：

| 输入 | 物理形态 | 语义 |
| --- | --- | --- |
| JS resource library | package metadata（`manifest.json`/`inventory.json`） | npm/JSR package identity、入口、exports、sideEffects、CSS/worker/static 边；C# 只做 binding/authoring contract |
| 纯 Jazor library | 程序集内 `Jazor.Generated.ModuleCatalog`（`ECMAScriptCode`） | C# 经 `Jazor.Compiler` lowering 后携带的生成模块源码和依赖 |

二者并列，都是 Emit 的一等输入。`ModuleCatalog` 承载生成 Jazor 模块，package metadata 承载外部包身份和明确声明的 embedded carrier。Emit 在内存中归一化二者，得到统一的 package graph；该记录归属本次物化过程。

`Jazor.Emit` 不参与 C# 或 Razor 语义降低。它只负责读取两种输入、验证清单、解析显式依赖闭包、执行冲突检查，并物化选定结果，产出最终 JavaScript 输出。

## 职责分层

| 输出或阶段 | 所属组件 | 说明 |
| --- | --- | --- |
| Roslyn `IOperation`、ESTree、模块文本 | `Jazor.Compiler` / `SemanticWalker` / `AstConverter` | 负责 C# 语义、导入收集、source origin 和确定性模块内容 |
| `Jazor.Generated.ModuleCatalog` | `Jazor.Compiler`、RazorVue generator | 纯 Jazor 编译结果的程序集内 carrier；最终宿主负责输出目录 |
| package metadata | 各 JS resource library | 外部 npm/JSR identity，或明确声明的 embedded JavaScript 及其资源；不经 C# lowering |
| 资源读取、归一化、依赖闭包和冲突校验 | `Jazor.Emit` | 只读取两种 carrier，使用同一套规则 |
| `.mjs/.js/.map/.css`、输出 manifest/import map | `Jazor.Emit` | 已验证资源闭包的物化结果 |
| Release bundle | `Jazor.Emit` + Netpack | 消费已选闭包；bundle 维持资源依赖语义 |
| SSR 模块图和 runner | `Jazor.Emit` + ASP.NET Core/DenoHost | 使用同一 carrier 闭包，额外选择显式 SSR 入口 |
| HMR snapshot/update | `Jazor.Emit` + Dev integration | 输出 profile 的更新数据，归属宿主输出层 |

## 固定数据流

```text
Jazor.Generated.ModuleCatalog --------┐
                                      ├─ carrier reader
package metadata ---------------------┘
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

- 程序集闭包只发现精确名称 `Jazor.Generated.ModuleCatalog`。生成模块、source map、HMR metadata 和附属资源必须由同一入口关联读取。
- JS resource library 由 MSBuild/NuGet 传递的 manifest locator 定位；locator 只是文件位置，不携带 analyzer、generator 或 Emit 资格。
- 发现过程不读取 provider 专名 catalog，不按程序集名、CLR 类型名、`System/` 前缀或目录内容推断资源，也不从已物化输出反向发现 producer。
- 同一输入重复发现时按稳定 identity 去重；内容、路径、owner、类型、hash 或依赖不一致时，在写出前返回冲突。

### 资源条目和依赖

JS resource manifest 使用固定 `schemaVersion` 和 `libraryId/version`。资源条目 `type` 只描述资源语义：`module`、`source-map`、`style`、`license`、`static`。`imports[*]` 是 module 入口；相对模块依赖与 package 依赖分开记录；`requires` 只表示 library 版本约束。

纯 Jazor `ModuleCatalog` 的 module 记录至少包含 `AssemblyName`、`TypeName`、`Id`、`RelativePath`、`Content`、`Hash`、相对依赖和 package imports。Emit 使用这些声明建立闭包，不解析 JavaScript 文本猜依赖。

闭包规则：

1. roots 来自应用自身生成模块、显式选择的 package specifier、SSR/HMR 入口和用户声明的静态入口。纯 consumer host 通过显式提供的 `ModuleCatalog` modules 声明 consumer roots；manifest locator 提供资源位置。
2. ModuleCatalog 的相对 dependency 指向同一生成 owner 的 module；package import 指向 JS resource manifest 的唯一 entry。
3. manifest entry 按当前 profile 选择 development/production 路径，沿显式 module/package dependencies 和 `requires` 继续解析。
4. 闭包之外的 module、map、style、license、static 文件不物化；引用程序集只贡献其声明的 roots 和 package metadata。
5. 缺失入口/文件/依赖、版本不满足、路径越界、错误 hash、重复 identity 或输出路径冲突，都在目标目录写入前失败。

## 输出 profile

| Profile | 输入选择 | 输出 |
| --- | --- | --- |
| `browser-debug` | 应用 roots + development resource entries | 独立 module、source map、输出 manifest/import map |
| `browser-release` | 应用 roots + production entries | 生产模块和 Netpack bundle/source map |
| `ssr-debug` | browser roots + 显式 SSR runner/Vue server entries | 可诊断 SSR module graph、runner、SSR import map |
| `ssr-release` | SSR roots + production entries | 发布 SSR graph、runner、hydration 所需资源 |
| `hmr-debug` | 与 browser-debug 同一次收集的 roots/closure | 完整当前 module snapshot 和 HMR update metadata |

所有 profile 共享同一 carrier 发现、identity、依赖和冲突规则；profile 选择入口并形成最终投影。`jazor-manifest.json`、browser/SSR import map、SSR 文件和 HMR envelope 归属本次构建的输出层。

## 物化和失败原子性

Emit 先在目标同卷 staging 中写出完整 profile，再验证每个文件的字节、hash、相对路径和 owner。成功后原子替换最终输出目录；失败、取消、并发冲突或进程中断时保留上一份有效输出。

Debug、Release、SSR 和 HMR 共用发现和闭包规则；HMR snapshot、SSR worker 状态和 bundle 归属宿主输出层。类库 carrier、资源 `type` 和依赖闭包由输入契约确定，输出 manifest 描述选中的资源闭包。

## 包和项目引用

- 定义纯 Jazor module 的项目直接引用 Jazor，生成自己的 `ModuleCatalog`；中间类库传递上游 catalog。
- JS resource package 的 manifest locator 和 package dependency 可以传递；外部 npm/JSR runtime 由 package manager 恢复，embedded-mjs runtime 由明确声明的本地 carrier 提供，最终宿主按闭包一次物化。
- `Jazor`、`Jazor.Vue`、Analyzer、Generator 和 Emit 是工具资格，谁直接使用谁直接引用；普通程序集引用不自动传递工具资格。
- NuGet 的 target 承载明确的工具边界；Jazor 的 `build/Jazor.targets` 负责直接 tooling，`buildTransitive/Jazor.Resources.targets` 传递 manifest locator。`buildTransitive/Jazor.Vue.targets` 始终传递 manifest locator，并在当前项目直接声明 `PackageReference Include="Jazor.Vue"` 时注册 RazorVue analyzer。analyzer 依赖位于 `tools/net11.0/analyzers/`，由直接引用的 target 注册给 Roslyn。
- 定义 Jazor module 或 RazorVue 组件的类库以 `PrivateAssets="all"` 直接引用对应工具包；最终 `Exe`/`WinExe` 宿主直接引用 `Jazor` 后才获得 Emit。
- 源码 ProjectReference 和 NuGet PackageReference 必须产生相同的 carrier 发现、版本选择、依赖闭包、去重、冲突诊断和输出字节。

## 输入约束

- 类库资源以 `ModuleCatalog` 或 package metadata 进入 Emit；provider、descriptor 与 catalog 专名属于各自的 metadata owner。
- `ArtifactCatalog`、`RuntimeProviderCatalog` 和 source-map catalog 的数据分别归属 `ModuleCatalog`、package metadata 或纯编译期 metadata。
- 发现以声明的 carrier、identity 和显式依赖为依据；Emit 以所选闭包进行物化。
- 中间类库传递资源 carrier；最终宿主执行 Emit 并物化输出。

一次性实施过程已归入历史记录；当前变更应遵守本文的固定数据流、失败原子性和验证边界。
