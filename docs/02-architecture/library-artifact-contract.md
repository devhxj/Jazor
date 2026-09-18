# 类库资源与引用契约

> 适用范围：多项目解决方案、NuGet 类库、最终可执行宿主，以及 Jazor 生成模块和外部 ESM/CSS 资源的交付。本文是当前稳定契约；交付状态与验证门槛见[当前状态](../04-roadmap/current-status.md)。

## 核心结论

类库携带 JavaScript 资源只有两种形式：

| 形式 | 典型项目 | 携带方式 | C# 的作用 |
| --- | --- | --- | --- |
| JS resource library | `ECMAScript`、Vue、Vuetify、Pinia 等已有上游 `.mjs/.js` 的绑定库 | `manifest.json +` 可按入口解析的 ESM 资源 | 映射上游模块，提供强类型 authoring contract |
| 纯 Jazor library | 开发者编写 Jazor/C# 并由 Jazor 编译的类库 | `Jazor.Generated.ModuleCatalog`（`ECMAScriptCode`） | 被 lowering 的源码和生成模块依赖 |

两种形式都是 Emit 的一等输入。`ModuleCatalog` 是固定的 generated C# carrier，属于当前正式实现；`manifest.json +` 绑定包携带的 ESM 资源是已有 JS 资源的 package carrier，与前者职责并列。Emit 可以在内存中统一处理二者，但不产生第三种类库形式。

绑定入口由 C# 的 `[ECMAScript("<specifier>")]` 声明。该字符串是最终 ESM import specifier，必须与资源 manifest 的 `imports` key 一致，并作为逻辑入口参与解析。物理资源路径、输出布局和完整性信息由 package metadata 管理；包名、版本、来源、integrity、`exports`、peer dependency 以及 CSS/worker/static 资源共同描述 package metadata，Emit 依据这些信息解析入口闭包。

`ArtifactCatalog`、`RuntimeProviderCatalog` 和类似 provider/descriptor 名称不属于本契约。若历史实现中存在这些符号，其承载内容必须迁移到上述两种 carrier 或编译期 metadata，再删除旧读取入口。

## 直接引用和资源传递

“谁使用，谁直接引用”只约束工具和作者能力，不阻断运行时资源传递：

- 定义纯 Jazor module 的项目直接引用 `Jazor`；生成自己的 `ModuleCatalog`。
- 定义 RazorVue 组件的项目直接引用 `Jazor` 和 `Jazor.Vue`；生成的组件 module 仍进入 `ModuleCatalog`。
- 只消费上游程序集的中间类库不因普通引用自动获得 analyzer、generator 或 Emit 资格，也不重新生成、复制或物化上游资源。
- 最终 Console/Web 宿主直接引用并配置所需的 Jazor/Emit 工具，收集直接和传递的 catalog 与 manifest，执行一次物化。
- JS resource package 的 manifest locator、package dependency 和版本约束可以传递；真实 `dist` 只由拥有 manifest 的包提供。

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
| JS/generated resource dependency | manifest entry、ModuleCatalog module、map/style/license | 按显式依赖和最终 root 闭包传递 |

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
A：包内 manifest.json + dist/**
B：只在 manifest/package dependency 中声明 A 的入口和版本
Console：选中 B 的入口，解析 B -> A 的 manifest closure，物化一次
```

B 通过 A 的 manifest 传递 `dist`，并维持 manifest 与 `ModuleCatalog` 的原有 carrier 身份。Emit 预检在缺失 manifest、版本条件或入口冲突时报告失败。

### 混合链

A、B 可以分别采用两种形式。每个 producer 保持自己的 carrier；Console 同时读取 `ModuleCatalog` 和 `manifest.json + dist`，以显式 module/package dependency 建立一份闭包，相同 identity 只物化一次。

## 资源 identity 和 `type`

Emit 的内部记录包含 owner、library/version、logical id/specifier、`type`、相对路径、内容/hash、profile 路径、source map 关联和显式依赖。持久化 carrier 使用 `ModuleCatalog` 或 manifest/dist。

`type` 只描述资源语义：

| `type` | 说明 |
| --- | --- |
| `module` | 可执行 ESM/JavaScript |
| `source-map` | 唯一关联 module 的 map |
| `style` | CSS |
| `license` | 许可证/通知 |
| `static` | 其他静态文件 |

`type` 使用资源语义名称。manifest 的 `imports[*]` 使用 `module`，`requires` 表达 library 版本约束；未来资源语义在同一条目结构中增加相应 `type` 和必需字段。

同一 `library/version/logical identity/type` 必须有相同字节、路径、hash 和依赖；相同输出路径被不同 identity 占用时失败。路径越界、缺文件、错误 hash、未知 type、缺依赖和版本冲突都在物化前失败。

## 项目职责

| 项目/层 | 负责内容 | 不负责的事项 |
| --- | --- | --- |
| `Jazor.Compiler` | `IOperation -> ESTree`、导入、source origin、纯 Jazor `ModuleCatalog` | 读取资源包或写最终宿主文件 |
| `Jazor.CLR` | CLR mapping、白名单声明、helper 源码和 carrier 选择 | 另建 runtime provider/catalog |
| `ECMAScript` | ECMAScript/Web API C# contract，以及 `manifest.json + dist` 资源包 | 交付 JS resource；不生成纯 Jazor `ModuleCatalog` |
| `Jazor.RazorVue` | official Razor SG binding、Vue framing 和组件 module 生成 | 维护第二个资源 catalog 或手写 C# lowering |
| `Jazor.Emit` | 两类 carrier 读取、闭包、校验、物化和 profile 输出 | 决定 C# 语义或资源隐式依赖 |
| `Jazor.AspNetCore` | 静态资源、SSR、hydration 和运行时宿主 | 替代 Emit 的 producer discovery |

编译期 `RouteCatalog`、source text registry 等服务于分析和生成；资源 carrier 与 Emit resource closure 使用本文定义的类库输入。

## Package 和输出边界

- JS resource package 的包根是 `manifest.json +` 可按入口解析的 ESM 资源；style、license、worker 和 static 文件由 manifest 明确声明。
- 纯 Jazor package 通过程序集携带 `Jazor.Generated.ModuleCatalog`；下游使用其已生成的 module 内容。
- `jazor-manifest.json`、import map、bundle、SSR runner、HMR snapshot 是最终输出层的投影，描述本次选中的闭包。
- Debug、Release、SSR、HMR 共用 discovery、identity、依赖和冲突规则，只改变 profile 的入口和物化方式。
- Emit 使用同卷 staging 和原子提交；失败、取消、并发冲突或重启时保留完整有效输出。

## 失败与验收

必须覆盖源码 ProjectReference 与 NuGet PackageReference 的 A -> B -> Console 矩阵：纯 Jazor、JS resource、混合依赖、重复 identity、路径冲突、缺失依赖和版本冲突。验收还必须证明：

1. 只有直接引用 Jazor/Jazor.Vue 的项目获得工具资格。
2. 上游 ModuleCatalog/manifest/dist 不被中间类库重编译、重写或提前物化。
3. 最终宿主物化 root 闭包；module、map、style、license 与 static 均由所选资源入口确定。
4. Debug/Release/bundle/SSR/HMR 的实际输出均可运行，source map、import map、hydration 和 HMR 行为正常。
5. 同一输入重复构建字节稳定；所有清单/hash/路径/依赖错误在写目标目录前明确失败。

一次性迁移的过程材料已归入历史记录；当前实现以本文定义的两类 carrier 和验收边界为准。
