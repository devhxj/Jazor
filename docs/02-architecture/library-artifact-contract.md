# 类库资源与引用契约

> 适用范围：多项目解决方案、NuGet 类库、最终可执行宿主，以及 Jazor 生成模块和外部 ESM/CSS 资源的交付。本文是当前稳定契约；交付状态与验证门槛见[当前状态](../04-roadmap/current-status.md)。

## 核心结论

类库输入分为两类，Emit 将它们统一投影到标准依赖项目：

| 形式 | 典型项目 | 输入 carrier | C# 的作用 |
| --- | --- | --- | --- |
| JS resource library | `ECMAScript`、Vue、Vuetify、Pinia 等已有上游 `.mjs/.js` 的绑定库 | 版本化 package metadata（当前由 `manifest.json`、`inventory.json` 和资源树承载），可按入口解析的 ESM/CSS/worker/static 资源 | 映射上游模块，提供强类型 authoring contract |
| 纯 Jazor library | 开发者编写 Jazor/C# 并由 Jazor 编译的类库 | `Jazor.Generated.ModuleCatalog`（`ECMAScriptCode`） | 被 lowering 的源码和生成模块依赖 |

`ModuleCatalog` 与绑定 package metadata 都是 Emit 的一等输入。`src/ECMAScript/clr/**` 是仓库自有 ECMAScript 源码 carrier；`src/Jazor.Vue/dist/**` 是独立 Vue runtime bridge；其他映射库以 npm/JSR package identity 为运行时来源，随包携带的本地 MJS 通过 manifest 明确声明 embedded carrier（例如 VuIcons 的 `runtime/vu-icons/**`）。最终宿主由 Emit 生成标准 `jazor/package.json`，并按来源把 npm、JSR 和 embedded mjs 组织成同一份 package graph。Emit 在 MSBuild staging 中调用 DenoHost runtime 生成 `deno.lock`、恢复 `node_modules`，NetPack 和 SSR 复用这份工作根。

绑定入口由 C# 的 `[ECMAScript("<specifier>")]` 声明。该字符串是最终 ESM import specifier，必须与 package metadata 的入口 key 一致，并作为逻辑 root 参与解析。物理资源路径、输出布局和完整性信息由 metadata 管理；包名、版本、来源（`npm`、`jsr`、`embedded-mjs`）、integrity、`exports`、`sideEffects`、peer dependency 以及 CSS/worker/static 资源共同描述 package identity，Emit 依据这些信息解析入口闭包。

绑定资源来源遵循统一的 package identity：

| 来源 | 资源提供方 | Emit 的 package 投影 |
| --- | --- | --- |
| `npm` | 外部 npm package registry | 根 `package.json` 的精确 dependency；DenoHost 恢复到 `jazor/node_modules` |
| `jsr` | 外部 JSR registry 或经过校验的本地 JSR adapter | JSR npm 兼容 identity 或标准本地 adapter；NetPack 与 Deno 解析同一入口和字节 |
| `embedded-mjs` | 明确声明的本地 ESM、CSS、worker 和 static carrier（`src/ECMAScript/clr/**` 是核心源码载体） | `packages/<name>` 与 `node_modules/<name>` 下的标准本地 package，带 `package.json`、`exports` 和资源闭包 |

`embedded-mjs` 适用于需要随绑定交付的自有模块。metadata 将逻辑 specifier 映射到声明的 carrier 路径，并记录模块哈希、相对 import、共享依赖和资源边；Emit 只物化实际 roots 可达的文件，同时把同一绑定声明的 npm/JSR 依赖接入 package graph。作者代码和最终 bundle 使用 metadata 声明的逻辑入口。

`ArtifactCatalog`、`RuntimeProviderCatalog` 和类似 provider/descriptor 名称不属于本契约。若历史实现中存在这些符号，其承载内容迁移到 `ModuleCatalog`、binding package metadata 或编译期 metadata；最终输出统一使用标准 package project 和本次 profile 的资源闭包。

## 直接引用和资源传递

“谁使用，谁直接引用”只约束工具和作者能力，不阻断运行时资源传递：

- 定义纯 Jazor module 的项目直接引用 `Jazor`；生成自己的 `ModuleCatalog`。
- 定义 RazorVue 组件的项目直接引用 `Jazor` 和 `Jazor.Vue`；生成的组件 module 仍进入 `ModuleCatalog`。
- 只消费上游程序集的中间类库通过普通引用传递 C# contract、package metadata locator 和资源依赖描述；每个资源 producer 继续由自己的 package metadata 提供内容与完整性信息。
- 最终 Console/Web 宿主直接引用并配置所需的 Jazor/Emit 工具，收集直接和传递的 catalog、package metadata 与实际 import roots，生成一次标准 package project。
- `npm`、`jsr` 和 `embedded-mjs` 依赖进入同一 package graph；选中的 module、CSS、worker、static 和 license 资源按 root 闭包物化，未进入闭包的资源保持在 producer 包内。

### NuGet 资产边界

`build/` 与 `buildTransitive/` 按以下职责划分：

| 包 | 直接 `build/` 资产 | 可传递 `buildTransitive/` 资产 |
| --- | --- | --- |
| `Jazor` | `Jazor.props`、`Jazor.targets`、compiler/analyzer 依赖闭包和 Emit 调用 | 仅 `Jazor.Resources.targets`，提供 `ECMAScript` 的 manifest locator |
| `Jazor.Vue` | 唯一的 `buildTransitive/Jazor.Vue.targets` 在当前项目直接声明 `Jazor.Vue` 时注册 RazorVue analyzer | 同一 target 始终传递 Vue/Vue runtime manifest locator |

Jazor analyzer DLL 位于 package 的 `tools/net11.0/analyzers/`，由直接引用时导入的 `build/` target 注册给 Roslyn。普通程序集与 NuGet 依赖以资源传递方式参与闭包。

定义 module/组件的类库对 `Jazor`、`Jazor.Vue` 使用 `PrivateAssets="all"`，使该直接工具引用不成为库消费者的工具依赖；最终宿主需要 Emit 时自行直接引用 `Jazor`，不使用该隔离设置。资源 manifest locator 和资源包依赖不受此规则影响。

### 三类关系

| 关系 | 作用 | 是否传递 |
| --- | --- | --- |
| C# authoring/reference | 类型、binding 和普通 API 编译 | 按普通 ProjectReference/PackageReference |
| Jazor tooling reference | analyzer、generator、Razor hook、Emit | 不隐式传递；使用者直接引用 |
| JS/generated resource dependency | package entry、ModuleCatalog module、map/style/license、worker/static | 按显式依赖和最终 root 闭包传递 |

工具资格不因程序集存在而出现；资源依赖不因工具资格而自动全量复制。

## A -> B -> Console

### 纯 Jazor 链

```text
A：直接引用 Jazor，生成 A 的 ModuleCatalog
B：普通引用 A；不重编译 A，不执行 Emit
Console：直接引用 Emit，收集 A、B 和应用 catalog，物化一次
```

B 通过程序集依赖消费 A 的 catalog。B 编写纯 Jazor 源码时直接引用 Jazor，并生成 B 自己的 `ModuleCatalog`；Console 收集 A 与 B 的 catalog。

### JS resource 链

```text
A：包内 package metadata + ESM/CSS/worker/static 资源
B：在 package dependency 中声明 A 的入口、版本和来源
Console：收集实际 roots，解析 B -> A 的 package graph，生成 jazor/package.json 并物化一次
```

B 通过 A 的 metadata 传递 package identity、入口和资源边；A 的资源仍由 A 的 producer 提供。Emit 将 graph 投影为标准 `package.json`、可选的完整 `package-lock.json`、embedded package，并在 MSBuild staging 中调用 Deno 生成 `deno.lock` 和恢复 `node_modules`。

### 混合链

A、B 可以分别采用两种形式。每个 producer 保持自己的 carrier；Console 同时读取 `ModuleCatalog` 和 package metadata，以显式 module/package/resource dependency 建立一份闭包，相同 identity 只物化一次。

## 资源 identity 和 `type`

Emit 的内部记录包含 owner、package identity、library/version、logical id/specifier、来源、`type`、相对路径、内容/hash、profile 路径、source map 关联和显式依赖。持久化输入使用 `ModuleCatalog` 或 binding package metadata；最终 profile 以标准 package project 形式输出。

`type` 只描述资源语义：

| `type` | 说明 |
| --- | --- |
| `module` | 可执行 ESM/JavaScript |
| `source-map` | 唯一关联 module 的 map |
| `style` | CSS |
| `license` | 许可证/通知 |
| `static` | 其他静态文件 |

`type` 使用资源语义名称。metadata 的入口记录使用 `module`，package dependency 表达来源和版本约束；未来资源语义在同一条目结构中增加相应 `type` 和必需字段。

同一 `library/version/logical identity/type` 必须有相同字节、路径、hash 和依赖；相同输出路径被不同 identity 占用时失败。路径越界、缺文件、错误 hash、未知 type、缺依赖和版本冲突都在物化前失败。

## 项目职责

| 项目/层 | 负责内容 | 交付重点 |
| --- | --- | --- |
| `Jazor.Compiler` | `IOperation -> ESTree`、导入、source origin、纯 Jazor `ModuleCatalog` | 交付稳定的逻辑 module roots |
| `Jazor.CLR` | CLR mapping、白名单声明、helper 源码和 carrier 选择 | 提供可追踪的 CLR runtime 映射 |
| `ECMAScript` | ECMAScript/Web API C# contract，以及 package metadata、入口和资源包 | 提供绑定 contract 与 package carrier |
| `Jazor.RazorVue` | official Razor SG binding、Vue framing 和组件 module 生成 | 交付 Vue render module 与资源 roots |
| `Jazor.Emit` | 两类 carrier 读取、实际 roots、package graph、闭包、校验、package project、Deno restore 和 profile 输出 | 在 MSBuild staging 中完成确定性物化 |
| `DenoHost` | 提供 Emit 与 SSR 使用的 Deno runtime、frozen lock 执行能力和 worker 运行时 | 执行已生成 package graph 的离线检查与 SSR |
| `Jazor.AspNetCore` | 静态资源、SSR、hydration 和运行时宿主，调用 Emit 生成的 frozen profile | 复用统一的 `jazor/node_modules` |

编译期 `RouteCatalog`、source text registry 等服务于分析和生成；资源 carrier 与 Emit resource closure 使用本文定义的类库输入。

## Package 和输出边界

- JS resource package 由 package metadata 和可按入口解析的 ESM 资源组成；资源来自外部 npm/JSR package，或来自明确声明的 embedded-mjs carrier；style、license、worker 和 static 文件由 metadata 明确声明。历史映射库 `dist/**` 不属于当前 carrier 契约。
- 纯 Jazor package 通过程序集携带 `Jazor.Generated.ModuleCatalog`；下游使用其已生成的 module 内容，并把 module root 合并进 package graph。
- Emit 为本次选中的闭包生成标准 `jazor/` 工作根：

  ```text
  jazor/
    package.json          # 精确依赖、exports 和项目类型
    package-lock.json     # 完整 npm 输入可用时生成的兼容 lock（可选）
    deno.lock             # Emit 调用 DenoHost runtime 生成的冻结依赖图
    node_modules/         # Emit 调用 DenoHost runtime 恢复，NetPack/SSR 共用
  ```

- `jazor-manifest.json`、import map、bundle、SSR runner、HMR snapshot 是最终输出层的 profile 投影，描述本次选中的闭包。
- Debug、Release、SSR、HMR 共用 discovery、identity、依赖和冲突规则，只改变 profile 的入口、条件和物化方式。
- Emit、DenoHost 使用同卷 staging 和原子提交；验证成功后 profile 一次性切换，运行时继续提供上一份有效输出直到新 profile 完成。

## 失败与验收

必须覆盖源码 ProjectReference 与 NuGet PackageReference 的 A -> B -> Console 矩阵：纯 Jazor、JS resource、混合依赖、重复 identity、路径冲突、缺失依赖和版本冲突。验收还必须证明：

1. 直接引用 Jazor/Jazor.Vue 的项目获得对应工具资格，普通引用继续传递 C# contract 与 package metadata。
2. 上游 ModuleCatalog 与 binding package metadata 保持 producer ownership，Emit 按 identity 和 hash 读取资源并生成 profile。
3. 最终宿主物化 root 闭包；module、map、style、license、worker 与 static 均由所选资源入口和显式资源边确定。
4. Debug/Release/bundle/SSR/HMR 的实际输出均可运行，source map、import map、hydration 和 HMR 行为正常，NetPack 与 SSR 复用 `jazor/node_modules`。
5. 同一输入重复构建 package、lock 投影、清单、bundle 和 source map 字节稳定；清单/hash/路径/依赖错误在 profile 提交前给出确定诊断。

一次性迁移的过程材料已归入历史记录；当前实现以本文定义的两类 carrier 和验收边界为准。
