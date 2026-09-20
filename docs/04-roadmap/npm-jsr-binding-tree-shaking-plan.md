# 绑定库 npm/JSR 与标准项目 Tree Shaking 计划

> 状态：进行中（P0-5）。本计划以标准前端项目为运行、检查和打包单位，定义绑定库来源、Emit 适配边界和细粒度 tree shaking 验收方式。

## 设计原则

1. `jazor/` 是 Emit 生成的普通前端项目根。Deno、NetPack 和 SSR 直接消费同一个项目根。
2. Emit 负责把 Jazor 模块产物和绑定入口接入标准项目；包解析、exports、条件、sideEffects、peer 依赖和 tree shaking 由 Deno、NetPack 与上游 package 机制完成。
3. 绑定库的运行时来源为 npm 和 JSR。绑定库维护强类型 C# API、入口映射、版本、integrity、资源 metadata 和上游证据。
4. `embedded-mjs` 表示源码库 carrier。ECMAScript 的 `src/ECMAScript/clr/**` 是源码，Emit 将实际使用的文件写入 `jazor/clr/**`；Emit 生成的其他 ECMAScript 模块写入项目源码目录。
5. 运行时项目只保留标准源码、入口、package.json、锁文件和 node_modules；Emit 适配层保持短小、稳定、可审计。

## 目标结果

- 一个组件或函数作为入口时，NetPack 从真实 ESM 图裁剪到可达导出、组件、方法和资源。
- 组件内部依赖、共享 helper、Vue peer/optional 依赖、条件导出和初始化模块按上游 package 语义保留。
- CSS、worker、字体、图片、wasm 和 static 文件由入口资源边进入项目和 release 输出。
- Deno 2.9.7 在 Emit 的 MSBuild 阶段恢复 `node_modules` 并生成 `deno.lock`；SSR 与 NetPack 复用该结果。
- 相同输入、SDK、Deno 和 NetPack 版本产生稳定的项目文件、lock、入口、bundle、CSS 清单和 source map。

## 来源模型

### 绑定库：npm 与 JSR

绑定库是 `ECMAScript.*` 中为外部 JavaScript 包提供强类型 C# API 的库。运行时来源和项目接入方式如下：

| 来源 | 绑定声明 | Emit 接入 | 标准工具消费 |
| --- | --- | --- | --- |
| npm | package name、精确版本、integrity、exports 子路径、named/default export、peer/optional 约束和资源边 | 写入 `jazor/package.json` 的精确 dependency，保留 authored specifier | Deno 恢复上游 package，NetPack 按 package.json 解析 |
| JSR | jsr specifier、精确版本或 integrity、入口/export、条件和资源边 | 写入 Deno 可恢复的 dependency，记录 authored specifier 与 canonical identity 的映射 | Deno 与 NetPack 使用同一 `node_modules` 和同一入口 |

绑定 package 的 manifest、inventory、许可证和 fingerprint 描述 contract。上游 package 的源码、exports、sideEffects 和内部依赖由恢复结果提供。

### 源码库：embedded-mjs

`embedded-mjs` 是源码库交付方式。源码 metadata 记录 carrier 路径、模块 hash、相对依赖、生成目标和资源边。

```text
src/ECMAScript/clr/**        -> jazor/clr/**
Emit 生成的 ECMAScript 模块 -> jazor/<声明的源码目录>/**
```

源码模块使用项目内相对 import，并可使用 npm/JSR bare package import。源码 carrier 与绑定 package 在同一项目图中通过标准 ESM 语义连接。

## 标准项目契约

```text
jazor/
  package.json          # Emit 生成：项目类型、依赖、入口和 exports
  package-lock.json     # 完整时作为 npm 生态兼容输入保留
  deno.lock             # Deno 2.9.7 restore 生成的冻结锁
  node_modules/         # Deno restore 恢复，SSR 与 NetPack 共用
  entry.mjs             # Emit 生成的可见应用入口
  clr/                  # ECMAScript 源码 carrier
  <generated-sources>/  # Emit 生成的其他 ECMAScript 模块
  <assets>/             # 由资源边选中的 CSS、worker 和 static 文件
```

根 `package.json` 使用标准字段：`private`、`type: module`、稳定项目名称、npm/JSR `dependencies`、`main` 或 `exports["."]` 指向 `./entry.mjs`。启用 SSR 时增加显式 SSR entry。项目构建诊断保留在现有应用 manifest 和 Emit 输出证据中，运行时解析依赖标准 package 字段。

`package-lock.json` 作为 npm 生态兼容输入，在能够表达完整 npm 图时保留或生成；`deno.lock` 由 Deno restore 生成并作为冻结依据。Deno 配置由 Deno 侧按项目需要管理。

## 模块与包映射

C# 的 `[ECMAScript("specifier")]` 是绑定入口的 authored specifier 真源。成员级路径优先于宿主类型级路径；`Transform` 表达导入或组件 lowering，`ExportName` 表达 named/default export。

```csharp
[ECMAScript("tdesign-vue-next/button/Button", Transform.Component, "Button")]
public sealed class TButton : IUIComponent
{
}

[ECMAScript("date-fns/addDays")]
public static class AddDays
{
}
```

Emit 保持 npm authored bare specifier；metadata 校验 package identity、exports 子路径和 export name。源码库逻辑路径在写出阶段按最终 importer 位置生成 `./` 或 `../` 相对 import。

## Emit 适配契约

Emit 是标准项目的适配器和事务边界。MSBuild 调用一次 Emit 完成项目写出、依赖恢复、检查和交付；SSR 运行阶段复用结果。

### 输入

- 根程序集、参与 Emit 的程序集、源码根、输出根和 build profile。
- `ModuleCatalog` 中的模块内容、Entries、模块依赖、package imports、source map 和资源边。
- npm/JSR binding metadata：package identity、版本、integrity、入口、peer/optional 选择和 CSS/worker/static 边。
- ECMAScript 源码 carrier metadata：源码路径、模块 hash、相对依赖和生成目标。
- Deno 2.9.7 可执行文件路径和 restore/check 选项。

### 写出与恢复顺序

1. 在输出同卷创建 staging 项目根，按 profile 收集实际模块 roots、package roots 和资源 roots。
2. 写出应用生成模块、source map、`src/ECMAScript/clr/**` 的实际闭包和其他源码模块。
3. 根据最终文件位置把项目内逻辑 import 归一化为 `./` 或 `../`；npm/JSR authored specifier 保持 bare package 形式。
4. 写出应用和 ECMAScript 源码 carrier 自身拥有的 CSS、worker、static、字体、图片、wasm 和 license 文件，并保留入口关联与 hash；npm/JSR 包内资源由包自身的 ESM import、exports 和 NetPack 资源图处理。
5. 生成可见 `entry.mjs`，按应用 Entries 连接实际模块；启用 SSR 时生成显式 SSR entry。
6. 生成根 `package.json`，写入 `type`、`main/exports`、npm/JSR 精确 dependencies 和 profile entries。
7. 根据 `package.json` 与现有 `deno.lock` 的 identity 选择一次标准恢复：首次生成或依赖 identity 变化时使用 `--frozen=false`；已有一致的 `deno.lock` 时使用 `deno ci`。

```text
# 首次生成或依赖 identity 变化
deno install --package-json --node-modules-dir=auto --frozen=false

# 已有一致的 deno.lock
deno ci --node-modules-dir=manual
```

8. 对根 entry、profile entry 和 SSR entry 执行离线 frozen check：

```text
deno check --node-modules-dir=manual --no-remote --frozen-lockfile entry.mjs
```

9. 将同一 staging 项目根交给 NetPack 与 SSR；NetPack 从 `entry.mjs`、`package.json`、`node_modules` 和源码树构图，SSR 使用同一 `node_modules` 与 `deno.lock`。
10. 校验项目文件、lock、entry、Deno check 和 NetPack 输出后，以同卷原子替换提交 `jazor/`。

### Emit 的稳定边界

- Emit 负责项目文件、入口、依赖声明、源码路径和完整性校验。
- Deno 负责 npm/JSR restore、node_modules 和 deno.lock。
- NetPack 负责 exports、条件、sideEffects、ESM/CSS tree shaking 和 release bundle。
- DenoHost 负责使用已恢复项目执行 SSR。
- 应用 manifest 负责记录生成模块和本地资源清单，作为 Emit 与标准工具之间的输入证据；项目运行时依赖 `package.json`、包的 `exports` 和 NetPack 资源图。

## 组件内部依赖与资源闭包

```text
binding entry
  -> package exports target
  -> component implementation / shared helper
  -> peer and optional dependency
  -> package side effect
  -> CSS / worker / static edge

source entry
  -> relative source module
  -> CLR/generated helper
  -> local resource edge
```

同一模块在多个入口闭包中保持一个稳定 identity。VueDraggable 等耦合度高的底层库按其 npm/JSR package 的完整运行时闭包验收；组件内部 helper 和共享 Vue runtime 随真实边保留。

资源 metadata 记录 module、style、worker、static、license 五类边，以及 sideEffects 对 CSS import、全局注册、polyfill 和初始化模块的影响。

## 当前实现接入点

1. `LibraryMaterializer`：将 ECMAScript 源码写入 `clr/**` 和声明的项目源码目录；绑定库提供 npm/JSR roots 与资源 metadata。
2. `LibraryPackageWriter`：生成根 `package.json`；完整 npm 图存在时保留标准 `package-lock.json` 输入，并记录 dependency identity。
3. `EmitPipeline`：以一个 staging 项目根串联源码写出、入口生成、一次 Deno restore/check、NetPack/SSR 消费和原子提交。
4. `NetpackBundler`：接收可见 entry 与同一 `PackageRoot`，直接调用标准 package resolution 和 tree shaking。
5. Route runtime 与 route catalog：作为普通项目源码或明确 package entry 写入项目树，通过标准相对/bare import 连接。
6. 路径与 identity 校验：验证 Emit 生成的相对 import、package identity 和项目文件可解析，运行时解析继续使用标准 package 机制。

## 分阶段计划

### 阶段 A：来源与项目 fixture

- 固定 npm/JSR binding metadata、ECMAScript 源码 carrier metadata、entry 和 package.json schema。
- 建立 npm、JSR、CLR source、组件内部依赖、CSS 和 worker fixture。
- 固化 JSR dependency key、canonical identity、authored specifier 和项目内相对 import。

退出条件：fixture 生成普通 `jazor/` 项目，Deno 与 NetPack 直接读取同一入口和依赖图。

### 阶段 B：Emit 标准适配

- 完成源码写出、项目内 import 归一化、`entry.mjs`、`package.json` 和 lock 输入。
- 在 MSBuild 的 Emit 阶段选择一次 restore 或 ci，生成 `deno.lock`，随后执行 frozen check；SSR 复用恢复结果。
- 为失败诊断、完整性校验、项目路径和原子提交建立 EmitTest 回归。

退出条件：生成的 `jazor/` 可由 Deno 2.9.7 check/run，项目文件可直接交给 NetPack。

### 阶段 C：标准项目构建

- 让 NetPack 直接消费 `jazor/entry.mjs`、`package.json`、`node_modules` 和源码目录。
- 验证 exports、sideEffects、组件内部依赖、共享依赖、CSS、worker、static 和 metafile。
- 保持 release 输出与项目输入边界清晰。

退出条件：Deno、NetPack 和 SSR 使用同一个 `jazor/` 项目图与恢复结果。

### 阶段 D：绑定库逐包迁移

1. 纯 ESM 函数/composable 包，验证 named export 与共享依赖。
2. TDesign、Element Plus、Vuetify 等组件包，验证组件级入口、组件内部依赖、Vue peer 和 CSS 边。
3. VueDataUi、图表、编辑器、上传和图标包，验证全局 CSS、worker、字体、图片、wasm 与动态资源。
4. VueDraggable 等耦合底层库，按 npm/JSR package 的完整闭包验收运行时语义。
5. 新增 binding 统一采用 npm/JSR identity，上游 ESM、exports 和 sideEffects 变化进入 metadata fingerprint。

ECMAScript 源码库单独验证 `clr/**`、生成模块、相对 import 和源码资源进入标准项目。

### 阶段 E：契约、文档与发布同步

- 同步 `library-artifact-contract.md`、`artifact-pipeline.md`、`js-resource-binding.md`、`current-status.md` 和 CHANGELOG。
- 更新 NuGet 与源码 ProjectReference consumer，使其发现并验证标准 `jazor/` 项目文件。
- 保存来源、版本、entry、lock、资源闭包和 bundle metafile 的可复现证据。

## 验收矩阵

| 场景 | 必须证明 |
| --- | --- |
| npm binding | 精确版本、integrity、exports、peer 选择和 node_modules 入口一致 |
| JSR binding | authored specifier、dependency key、版本、入口和冻结字节在 Deno/NetPack 中一致 |
| ECMAScript 源码库 | `src/ECMAScript/clr/**` 到 `jazor/clr/**` 的实际闭包复制完整，生成模块和相对 import 可解析 |
| 标准项目 | package.json、entry、源码、node_modules 和 lock 位于同一 `jazor/` 根，可由标准工具直接消费 |
| Emit 适配 | roots、源码写出、路径归一化、入口、package.json、restore、check 和提交均有测试证据 |
| Tree shaking | 只导入一个组件/函数时，未使用导出、组件和方法不进入 bundle，共享依赖只保留一份 |
| 组件依赖 | 组件内部模块、共享 helper、Vue peer 和必要 side effect 在裁剪后保持运行语义 |
| CSS | 已使用组件和必要基础 CSS 存在，未选入口的 CSS 不进入输出；全局 stylesheet 按上游声明保留 |
| worker/static | worker、字体、图片和 wasm 可解析且只随可达资源边输出 |
| Deno restore | Emit 按 lock 状态选择一次 install 或 ci，生成 `node_modules` 与 `deno.lock`，随后在 no-remote 下通过 frozen check |
| SSR | 直接复用 Emit 已恢复的 jazor/node_modules 和 frozen lock |
| NetPack Web | 从 jazor/entry.mjs 解析 exports/sideEffects，并生成可运行、稳定的 Web bundle |
| 条件导出 | browser、deno、development、production 条件在对应 profile 中确定选择 |
| 确定性 | 相同 SDK、Deno、NetPack 和输入下项目文件、lock、manifest、bundle 与 source map 字节稳定 |

验证覆盖绑定专属测试、Jazor.EmitTest package/closure/materialization 回归、MSBuild Emit restore/check/SSR 回归、NetPack fixture、真实 RazorVue consumer 和适用 Chromium smoke。自动化继续使用 `scripts/csharp/` 单文件 C# 入口。

## 实施证据

- npm/JSR binding 保存 package identity、上游版本、specifier、exports、sideEffects、资源边和 fingerprint。
- ECMAScript 源码库保存 carrier 路径、模块 hash、生成目标和相对依赖证据。
- Emit 保存 `package.json`、entry catalog、`deno.lock`、restore/check 摘要、项目源码清单和本地资源清单。
- NetPack 保存入口、解析条件、tree shaking metafile、bundle/source map 和 CSS 清单。
- 失败信息包含来源、specifier、entry、依赖 owner、profile 和 Emit 阶段。

## Definition of Done

- 所有纳入范围的 ECMAScript 绑定库都以 npm/JSR identity 声明精确依赖和公开入口；运行时包由标准依赖恢复提供。
- ECMAScript 源码库的 `clr/**` 与生成模块由 Emit 写入标准 `jazor/` 项目，并通过项目内相对 import 运行。
- Emit 完成标准项目适配：写出源码与入口、生成 package.json、执行 Deno 2.9.7 restore/check、提交 deno.lock 和 node_modules，并把同一项目根交给 NetPack/SSR。
- NetPack 以 `jazor/` 为工作根消费同一 node_modules，按上游 exports/sideEffects 完成细粒度 JS/CSS tree shaking；组件内部和共享依赖语义保持一致。
- SSR 直接复用 Emit 已恢复的项目根；浏览器、SSR、源码 ProjectReference 和 NuGet consumer 通过验收矩阵。
- 架构文档、作者指南、测试门禁、发布脚本、CHANGELOG 和 current-status.md 同步记录标准项目契约。

后续按阶段验收，把 npm/JSR 绑定和 ECMAScript 源码项目逐步转入 Support 能力。
