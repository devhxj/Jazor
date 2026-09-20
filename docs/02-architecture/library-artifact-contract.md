# 类库与标准前端项目契约

> 适用范围：多项目解决方案、NuGet 类库、最终宿主、ECMAScript 源码与 npm/JSR 绑定。实施状态见[当前状态](../04-roadmap/current-status.md)，迁移步骤见[npm/JSR 绑定与标准 Jazor 项目计划](../04-roadmap/npm-jsr-binding-tree-shaking-plan.md)。

## 核心模型

最终宿主拥有一个标准前端项目根 `jazor/`。Emit 只把 Jazor 的编译结果和绑定声明接入这个项目，Deno、NetPack 与 DenoHost 直接使用项目文件。

**交接边界（已定）**：Jazor 生成 `jazor/` 项目根，然后交棒。HMR、debug、SSR 与 bundle 由 Deno 与 NetPack 负责，不是 Jazor 的职责。

| Jazor 负责 | 交棒后由 Deno / NetPack 负责 |
| --- | --- |
| 写出源码、入口与 `package.json` | 依赖恢复、`node_modules`、`deno.lock` |
| 写出标准 ESM 图（bare specifier、side-effect import） | 解析 bare specifier、CSS 转换与抽取、资源投递 |
| 调用 `deno install` | HMR、debug 服务、SSR 运行、生产打包 |

因此 Jazor **不产出**解析/服务/打包期的中间产物：没有样式清单，没有 `<link>` 注入，没有 CSS 拼接，也不解释依赖内容。它只交付一个标准前端项目。

```text
ECMAScript 源码 + ModuleCatalog + npm/JSR 绑定声明
                         |
                         v
                     Jazor.Emit
                         |
                         v
      jazor/（源码、入口、package.json、lock、node_modules）
                         |
             +-----------+-----------+
             |                       |
             v                       v
          NetPack                 DenoHost
```

## 两类类库

| 类型 | 交付内容 | 进入项目的方式 |
| --- | --- | --- |
| ECMAScript 源码库 | `src/ECMAScript/clr/**` 中的 MJS、source map 与源码资源 | 按相对路径写入 `jazor/clr/**` |
| ECMAScript 绑定库 | 强类型 C# API、npm/JSR identity、标准 ESM specifier、上游证据 | dependency 写入根 `package.json`，生成源码保留 bare import |

`ECMAScriptModule` 和 `Jazor.Generated.ModuleCatalog` 产生的模块也是项目源码，按声明的模块路径写入 `jazor/`。ECMAScript 是源码库；其他 `ECMAScript.*` 绑定库的运行时代码由 npm 或 JSR 恢复。

现有 metadata 的 `embedded-mjs` 值映射到 ECMAScript 源码输入。目标项目将这些文件作为普通源码处理。

## 标准项目布局

```text
jazor/
  package.json
  package-lock.json     # 存在完整 npm lock 时保留
  deno.lock
  node_modules/
  entry.js
  ssr-entry.js          # 启用 SSR 时生成
  clr/
  <module-paths>/
  <local-assets>/
  dist/
```

根 `package.json` 使用标准的 `name`、`private`、`type`、`main`、`exports` 和 `dependencies` 字段。`exports["."]` 指向浏览器入口；存在 SSR 入口时增加 `exports["./ssr"]`。

`package-lock.json` 接入 npm 产生且根依赖与当前项目一致的完整 lock。`deno.lock` 由 Deno 2.9.7 生成，作为 Deno restore、check 与 SSR 的冻结依据。`node_modules` 由 Deno 恢复，NetPack 与 DenoHost 共用。

## 源码契约

```text
src/ECMAScript/clr/**         -> jazor/clr/**
ECMAScriptModule.RelativePath -> jazor/<RelativePath>
```

项目内模块根据 importer 与目标文件的最终位置写出 `./` 或 `../` specifier。源码、source map 和源码直接引用的 CSS、worker、字体、图片或 wasm 保持稳定相对路径。

`ModuleCatalog` 提供模块内容、相对路径、Entries、package imports、source map 与本地资源位置。最终宿主的 Emit 将这些内容写入项目。

## 绑定契约

绑定包向 Emit 提供：

- dependency key；
- registry source：npm 或 JSR；
- 精确版本与可用的 integrity；
- `[ECMAScript]` 使用的标准公开 specifier；
- 声明面给出的导出名与 `[Style]` 样式边（见“绑定声明面”）。

`[ECMAScript("<specifier>")]` 是生成 import 的真源。成员级声明优先于类型级声明。specifier 直接由恢复后的标准项目解析。

| 来源 | `package.json` | 生成源码 |
| --- | --- | --- |
| npm | dependency key 对应精确 npm 版本 | 保持公开 bare specifier |
| JSR | dependency key 对应精确 `jsr:` value | 保持该 key 下的公开 specifier |
| 项目源码 | 无 dependency | 写出相对 specifier |

绑定选择上游公开的细粒度 ESM 入口。例如 TDesign Button 使用 `tdesign-vue-next/es/button/index.mjs`，该入口继续引用组件实现、共享 helper、Vue 和样式。上游正式 API 只有根入口时，绑定使用根入口及其完整运行时闭包。

绑定 metadata 连接 C# contract 与 dependency identity，并保存版本、integrity、许可证和上游快照。恢复后的上游 package 定义 exports、conditions、`sideEffects`、传递依赖、worker 与静态资源。

## 绑定声明面

绑定库的运行时图由 C# 声明面表达。

> 本节的规则均已确认。

### 特性

三个特性职责互不重叠（已定）：

| 特性 | 参数 | 职责 | 状态 |
| --- | --- | --- | --- |
| `[ECMAScript("specifier")]` | import specifier | **绑定导入库**：从外部 npm/JSR 包导入 | 已定 |
| `[ECMAScript]` | 无 | 环境/宿主契约，不绑定外部模块 | 已定 |
| `[ECMAScriptModule("path")]` | 输出路径 | **标注生成模块**：该类型被发射为某模块 | 已定 |
| `[Style("specifier")]` | 样式 specifier | **绑定 CSS 导入** | 已定 |
| `[ECMAScriptName("name")]` / `[Description("@#name")]` | 名称 | 输出名，同时作为模块导出名 | 已定 |

`[ECMAScript("specifier")]` 与 `[ECMAScriptModule("path")]` 是**相反方向**的两件事：前者是"从哪个外部库导入"（消费），后者是"这个类型生成到哪个模块"（产出）。两者可以同时出现在一个类型上，互不冲突，各自解析。

**声明面同项目（已定）**：`[Style]` 与 `[ECMAScript]` 位于同一个程序集 `ECMAScript.Contract`（`netstandard2.0`、零依赖），命名空间同为 `ECMAScript`。绑定库经 `ECMAScript` 的程序集引用传递可见两者。声明面特性集中在一处，不分散到实现程序集，避免绑定库为用一个特性而引入实现依赖。

`[ECMAScriptName]` / `[Description("@#...")]` 的名字机制**位置不变**，仍位于 `ECMAScript` 程序集（`src/ECMAScript/attribute/`）。它是既有实现的一部分，本次规则不为它增加迁移工作量。

注：`ECMAScript.Contract` 内现有特性命名空间并不统一——`ECMAScriptAttribute` 是 `ECMAScript`，`EmitsAttribute`/`JazorAttribute`/`PropsAttribute` 是 `ECMAScript.Contract`。`[Style]` 与 `[ECMAScript]` 同命名空间，以保证两个配对使用的声明面特性写法一致。

`[ECMAScript]` 最多一个参数。**不存在 `Transform`，也不存在组件专用参数**：值绑定与组件绑定使用同一条声明。成员级声明优先于类型级声明。（已定）

没有独立的 `[Export]` 特性：导出名由名字机制给出。（已定）

**不区分开发/生产 profile（已定）**：声明面每个特性只写一个 specifier，不提供 dev/prod 成对字段。差异由上游 package 的 `exports` conditions 解析决定——`PackageExportsResolver` 已按 conditions 工作。这与交接边界一致：Jazor 不做 profile 判断，只写出标准 ESM 图，profile 选择属于消费侧。绑定 metadata 中成对的 `development*`/`production*` 字段随之整体退役。

### 名字与导出名

名字解析是三态规则（已定）：

| 写法 | 结果 |
| --- | --- |
| 无名字特性 | 使用符号名 |
| `[ECMAScriptName("X")]` / `[Description("@#X")]` | 使用显式名 `X` |
| `[ECMAScriptName("")]` / `[Description("@#")]` | **stop**：不参与命名，也不参与外层宿主的名称拼接 |

`ECMAScriptName` 优先于 `Description("@#...")`。`default` 导出写成 `[ECMAScriptName("default")]`，没有独立的 `default` 回退规则。

### 组件

组件身份由约定判定：类型派生自 `Microsoft.AspNetCore.Components.ComponentBase` 且实现 Vue marker。声明面上组件与普通绑定没有区别，不需要组件标记。（已定）

### 样式边

`[Style]` 是**绑定 CSS 导入**，声明该绑定入口需要的样式 side-effect specifier（已定）：

- 可重复；**声明顺序即导入顺序，也是 CSS 层叠顺序**。
- 只声明 specifier。绑定不做形态判断，也不从扩展名推断是 JS 模块还是样式表。
- 归属：包的基础样式在包入口声明一次；入口自有样式在该入口声明。
- 物化：编译器在**使用点**把声明的 specifier 发射为生成模块里的普通 side-effect import。

`[Style]` 与 `[ECMAScript]` 的区别是"要不要绑定名字"：`[ECMAScript]` 产生一个有名字的导入绑定，`[Style]` 只产生一条纯副作用边，不引入任何绑定的标识符。

**依附于入口（已定）**：`[Style]` 挂在 `[ECMAScript("specifier")]` 声明的入口上，"入口"由 `[ECMAScript]` 唯一确定。只声明 `[Style]` 而没有对应 `[ECMAScript]` 入口时是**构建错误**，不允许样式边悬空。

**处理边界（已定）**：Jazor 与 Emit 的职责到生成模块里的普通 side-effect import 为止。

```js
import "element-plus/es/components/affix/style/css.mjs";
```

此后就是标准 ESM 图：依赖恢复由 Deno 负责，打包与 CSS 抽取由 NetPack 负责，开发期模块请求由 dev server 负责。**Jazor 不产出样式清单、不注入 `<link>`、不拼接 CSS，也不解释样式内容**——与任何标准 JS 项目一致。上游模块内部继续 import CSS（如 `css.mjs` 内再 import `theme-chalk/el-affix.css`）属于上游自己的模块图，由标准工具链沿边处理，不需要 Jazor 参与。

### specifier 形态规则

**已定** 扩展名规则分两类，判据是"谁拥有这个文件"：

| 类别 | 形态 | 例子 |
| --- | --- | --- |
| 包 specifier（bare，含深层路径） | **逐字使用上游公开的 specifier**，不增删扩展名 | exports 键：`vuetify/styles`；深层路径：`element-plus/dist/index.css`、`element-plus/es/components/affix/style/css.mjs` |
| Jazor 自有产物 | 统一 **`.js`** | 生成模块、`entry.js`、`ssr-entry.js`、source map；项目内相对 import 如 `./components/showcase-landing.js` |

根 `package.json` 声明 `"type": "module"`，因此项目内 `.js` 已经是 ESM，`.mjs` 是重复表达。上游 specifier 的扩展名属于上游契约，一律不碰——上游文件叫 `.mjs` 就写 `.mjs`（见下）。

包 specifier 不能统一加扩展名。以 `vuetify@4.2.1` 为例，`exports` 同时有显式键和兜底通配：

| 写法 | 命中的 exports 规则 | 结果 |
| --- | --- | --- |
| `vuetify/styles` | 精确键 `"./styles"` | 映射到 `./lib/styles/main.css`（**上游声明的真实样式入口**） |
| `vuetify/styles.css` | 兜底通配 `"./*"` | 按字面解析为包根的 `./styles.css`，上游不发布该文件 |

所以加扩展名不是被 exports 拒绝，而是**丢掉了上游的显式映射**，退化成对不存在文件的字面查找。绑定必须使用上游声明的 specifier 才能拿到真实的样式入口。

扩展名的有无也不携带形态信息：`vuetify/styles` 无扩展名却映射到 CSS，`.../style/css.mjs` 有扩展名却是 JS 模块。

深层路径可解析的前提是上游 exports 允许它。绑定使用 `element-plus/es/components/affix/style/css.mjs` 这类深层 specifier 时，依赖的是上游 exports 的通配或兜底规则：`element-plus@2.14.5` 提供 `"./es/*.mjs"` 通配与 `"./*"` 兜底，`tdesign-vue-next@1.20.7` 则没有 `exports` 字段（任意路径按文件布局解析）。若上游改成封闭 exports，这些深层 specifier 会被阻止——因此绑定选择的 specifier 必须由恢复后的 package 实际验证可解析，不能只凭路径存在。

当前三个主要绑定的 specifier 已用上游快照的 exports 规则逐一验证：ElementPlus 446 条、TDesign 470 条、Vuetify 448 条，合计 1364 条全部命中精确键、通配或开放的 exports，无未解析项。

### 同一模块多处声明

**已定** 同一个上游模块可以由多处声明（不同创作面各有用途），但模块级事实必须一致：specifier、导出名、样式边。**不一致是构建错误**，不允许静默分叉。

### 编译器物化

**已定** 声明由编译器在使用点物化：

- 模块边 → 生成模块中的 `import`；
- `[Style]` → 生成模块中的 side-effect `import`。

候选取向是编译器在符号层解析（键无损），Emit 只汇总生成结果，不反射绑定程序集，也不解析上游包内部结构。

### 生成器

**已定** 生成的编目、registry 与 catalog 从声明派生，并且必须字段透明：声明面新增字段时自动透传。生成物不是第二处声明点。

### 声明与派生证据的关系

**已定** 声明面是运行时图的唯一真相；`manifest.json`、inventory、许可证与 hash 是派生证据，由声明生成、由门禁校验，不作为构建输入。`manifest.json` 中样式相关的字段随形态判别一起退役。license 由绑定包的 NuGet `licenses/**` 交付，不参与运行时图。

## 引用传递

```text
A：提供 ModuleCatalog，或提供 npm/JSR binding declaration
B：通过 ProjectReference/PackageReference 使用 A
最终宿主：收集实际 Entries 和 package imports，生成一次 jazor/ 项目
```

工具资格由直接引用的 `Jazor`、`Jazor.Vue` 和 Emit targets 提供。源码 carrier、`ModuleCatalog` 与 binding declaration 随普通项目引用传递；最终 `Exe`、`WinExe` 或 Web 宿主直接引用 `Jazor`，配置并拥有 `JazorDir`。

| 包 | 直接资产 | 传递资产 |
| --- | --- | --- |
| `Jazor` | props/targets、compiler、analyzer、Emit 与 DenoHost runtime 接线 | ECMAScript 源码 locator 与 binding declaration locator |
| `Jazor.Vue` | RazorVue analyzer、generator 与项目源码 | Vue 相关源码和 binding locator |
| `ECMAScript.*` binding | C# assembly、XML 文档、binding metadata、inventory、许可证 | npm/JSR identity 与标准入口 |
| `ECMAScript` 源码库 | C# assembly、源码 metadata | MJS 源码、source map 与本地资源 |

源码 ProjectReference 与 NuGet PackageReference 对同一类库产生相同的 dependency identity、生成 import、源码相对路径和项目结果。

## 职责边界

| 层 | 责任 |
| --- | --- |
| `Jazor.Compiler` | C# 语义、import 收集、source origin、source map 与 `ModuleCatalog` |
| `ECMAScript` 源码库 | 提供可写入项目的 MJS 源码与本地资源 |
| `ECMAScript.*` binding | 提供强类型 API、标准 specifier 和精确 npm/JSR identity |
| `Jazor.Emit` | 生成 `jazor/` 项目、写出源码与入口、生成 `package.json`、调用 Deno install 恢复绑定依赖 |
| Deno 2.9.7 | restore、`node_modules`、`deno.lock` 与入口检查 |
| NetPack | package resolution、conditions、`sideEffects`、ESM/CSS/asset tree shaking |
| DenoHost | 从已恢复项目执行 SSR |

Emit 在项目边界完成两种转换：源码 carrier 变成项目文件，绑定 declaration 变成 `dependencies` 与 bare import。随后使用标准项目语义完成恢复、构建和运行。

## Emit 写入

1. 收集应用 Entries、生成模块、ECMAScript 源码、package imports 和本地资源。
2. 在最终 `jazor/` 按最终路径写出源码、source map 与本地资源；单文件使用临时文件加 rename。
3. 生成 `entry.js`、可选 `ssr-entry.js` 和根 `package.json`。
4. 调用 Deno 2.9.7 恢复依赖、生成或验证 `deno.lock`，并 frozen check 所有入口。
5. Release 模式让 NetPack 从同一项目根生成 `dist/`。
6. 各步骤就地生效；失败显式返回，由下一次构建收敛。

首次生成或 dependency identity 变化时使用 `deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=false`；lock 一致时使用 `deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=true`；入口检查使用 `deno check --node-modules-dir=manual --no-remote --no-config --frozen-lockfile`。

## 一致性与验收

- 同一 dependency key 对应一个精确 package identity。
- 每个 bare import 都能由根 `package.json` 和恢复后的 package 解析。
- 每个项目内 import 都能按最终相对路径解析。
- package 内部 import、peer dependency、初始化和 `sideEffects` 由上游 package 保持。
- Debug、Release、HMR 和 SSR 使用同一项目根、源码树、dependency identity 与恢复结果。
- 项目生成、恢复、检查或构建失败时显式返回错误，不做整目录回滚；下一次构建按同一规则收敛。
- Deno 2.9.7 能在 `jazor/` 直接 restore、check 和 run。
- NetPack 能从 `jazor/entry.js` 解析 `node_modules` 并输出 `jazor/dist/`。
- DenoHost 能从同一项目根加载 `ssr-entry.js`。
- 单组件或单函数 consumer 只保留入口可达的 ESM 与资源，同时保留组件内部依赖和必要副作用。
