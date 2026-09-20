# npm/JSR 绑定与标准 Jazor 项目计划

> 状态：进行中（P0-5）。目标是让 `jazor/` 成为 Deno 2.9.7、NetPack 和 DenoHost 可以直接消费的标准前端项目，并在标准 ESM 图上完成细粒度 tree shaking。

## 设计结论

1. `jazor/` 是唯一项目根。源码、入口、`package.json`、锁文件、`node_modules` 和构建输出都在该目录。
2. ECMAScript 自有 MJS 是项目源码。`src/ECMAScript/clr/**` 写入 `jazor/clr/**`，其他 `ECMAScriptModule` 按声明路径写入项目。
3. ECMAScript 绑定库只绑定 npm 或 JSR 包，交付强类型 C# API、精确 package identity 和标准 ESM specifier。
4. Emit 在 MSBuild 阶段生成并恢复项目。恢复是项目生成的一部分，与 SSR 是否启用无关。
5. Deno 负责依赖恢复、`node_modules`、`deno.lock` 和入口检查；NetPack 负责浏览器构建；DenoHost 负责 SSR。三者直接使用同一项目。

## 标准项目结果

```text
jazor/
  package.json          # Emit 生成：精确依赖与公开入口
  package-lock.json     # 存在完整 npm lock 时保留
  deno.lock             # Deno 2.9.7 生成并冻结
  node_modules/         # Deno 恢复，NetPack 与 DenoHost 共用
  entry.mjs             # 浏览器入口
  ssr-entry.mjs         # 启用 SSR 时生成
  clr/                  # ECMAScript CLR 源码
  <module-paths>/       # 其他生成源码与 source map
  <local-assets>/       # 项目源码直接引用的本地资源
  dist/                 # NetPack Release 输出
```

根 `package.json` 使用标准字段：`name`、`private`、`type`、`main`、`exports` 和 `dependencies`。`exports["."]` 指向 `entry.mjs`；存在 SSR 入口时增加 `exports["./ssr"]`。

`package-lock.json` 只接入 npm 生成且与当前 `package.json` 一致的完整 lock。`deno.lock` 由 Deno 生成，是 Deno check、SSR 和后续确定性恢复的冻结依据。

## 两类输入

### ECMAScript 源码

ECMAScript 自有 MJS 进入项目后就是普通源码：

```text
src/ECMAScript/clr/**        -> jazor/clr/**
ECMAScriptModule 生成结果    -> jazor/<module-path>/**
```

源码模块使用相对 import。source map 以及源码直接引用的 CSS、worker、字体、图片和 wasm 按最终相对路径写入项目。

现有 metadata 中的 `embedded-mjs` 对应这种源码 carrier。目标模型将它归入 ECMAScript 源码输入。

### ECMAScript 绑定

绑定库的构建声明保持精简：

- C# 强类型 API；
- `[ECMAScript]` 上的标准 ESM specifier、导入形式和 export name；
- npm/JSR dependency key、精确版本和可用的 integrity；
- 上游快照、许可证与 inventory；
- 上游要求调用方显式导入时，与绑定入口关联的 CSS side-effect specifier。

绑定包交付 package identity 与入口声明，运行时代码由 npm/JSR 恢复。恢复后的上游 `package.json`、`exports`、conditions、`sideEffects`、dependencies 和 peer dependencies 是包解析与 tree shaking 的标准依据。

## 模块与包映射

`[ECMAScript("<specifier>")]` 保存生成源码最终写出的 import specifier。它直接使用 Deno 与 NetPack 可以解析的公开入口：

```csharp
[ECMAScript("tdesign-vue-next/es/button/index.mjs", Transform.Component, "Button")]
public sealed class TButton : IUIComponent
{
}

[ECMAScript("date-fns/addDays")]
public static class AddDays
{
}
```

Emit 从 bare specifier 取得 dependency key，再从绑定声明取得精确 identity：

| 输入 | `package.json` | 生成源码 |
| --- | --- | --- |
| npm binding | dependency key 对应精确 npm 版本 | 保持公开 bare specifier |
| JSR binding | dependency key 对应精确 `jsr:` value | 保持该 key 下的公开 specifier |
| 项目源码 | 无 package dependency | 根据文件位置写 `./` 或 `../` import |

成员级 `[ECMAScript]` 优先于类型级声明；`Transform` 决定普通导入或组件导入；`ExportName` 决定 named/default export。

绑定优先选择上游公开的最细 ESM 入口。上游只提供正式根入口时直接使用根入口；VueDraggable 等高耦合库由该入口保留完整运行时闭包。

同一个 dependency key 只能对应一个精确 identity。生成源码出现 package import 而绑定声明缺失，或多个绑定为同一 key 声明不同 identity 时，Emit 在恢复前报告冲突。

## Emit 必须完成的工作

Emit 是项目生成器和失败原子性边界，按以下顺序执行：

1. **收集入口**：从最终宿主和 `ModuleCatalog` 取得浏览器 Entries、可选 SSR Entries、生成模块、source map、package imports 与本地资源。
2. **建立 staging 项目**：在同卷目录建立与最终 `jazor/` 相同的结构，并沿用仍与 `package.json` 一致的锁文件。staging 只用于原子提交。
3. **写入源码**：把 `clr/**`、其他源码 carrier、生成的 `ECMAScriptModule`、source map 和源码资源写到最终相对位置。
4. **写入标准 import**：项目内模块使用相对 specifier；npm/JSR 绑定保留 `[ECMAScript]` 的 bare specifier；必要样式以普通 side-effect import 写入入口。
5. **生成可见入口**：`entry.mjs` 连接实际浏览器 Entries；启用 SSR 时生成 `ssr-entry.mjs`。这些文件是 Deno、NetPack 和 DenoHost 共用的真实入口。
6. **生成 `package.json`**：从实际 package imports 合并 dependency key 与精确 identity，并写入标准入口字段。
7. **恢复与冻结**：在 staging 项目根调用 Deno 2.9.7，生成或验证 `deno.lock` 与 `node_modules`。
8. **检查入口**：使用本地 `node_modules`、`--no-remote` 和 frozen lock 检查所有生成入口。
9. **调用消费者**：Release 让 NetPack 从 `entry.mjs` 构建 `dist/`；SSR 运行时让 DenoHost 从同一项目加载 `ssr-entry.mjs`。
10. **提交项目**：源码、项目文件、lock、恢复结果和构建结果全部有效后，原子替换最终 `jazor/`。

首次恢复或 dependency identity 变化：

```text
deno install --package-json --node-modules-dir=auto --frozen=false
```

锁文件一致后的确定性恢复：

```text
deno ci
```

入口检查：

```text
deno check --node-modules-dir=manual --no-remote --frozen-lockfile entry.mjs
```

Deno 2.9.7 的 `ci` 命令按 lock 执行 frozen install。Emit 从 DenoHost runtime package 取得同版本可执行文件，并沿用项目已经建立的 `node_modules` 模式。

## 标准构建行为

NetPack 的工作根、解析根和入口都来自 `jazor/`：

```text
entry.mjs
  -> 项目源码
  -> npm/JSR 公开入口
  -> 包内组件、函数和共享 helper
  -> dependencies / peer dependencies
  -> side-effect module
  -> CSS / worker / font / image / wasm
```

NetPack 使用上游 package 的 `exports`、conditions 和 `sideEffects`，并执行 ESM/CSS tree shaking。组件内部 import 自然保留其实现、共享依赖、Vue peer、初始化顺序和必要样式；未从入口到达的 export、组件和资源由标准构建图裁剪。

上游入口已经 import CSS 时直接跟随该边。上游要求调用方显式引入 CSS 时，绑定声明把样式 specifier 关联到组件入口，Emit 写出普通 side-effect import。worker、字体、图片和 wasm 通过包内 import 或 `new URL(..., import.meta.url)` 进入构建图。

DenoHost 的工作目录是 `jazor/`，使用 Emit 已恢复的 `node_modules` 和 `deno.lock`。依赖恢复属于 Emit 流程；SSR 只消费结果。Debug、Release、HMR 和 SSR 因而观察同一份源码与 package identity。

## 实施顺序

### A. 固定标准项目 fixture

- 生成包含一个 npm binding、一个 JSR binding、`clr/**` 和一个生成模块的最小项目。
- 固定 dependency key、精确 identity、标准 specifier、入口、lock 与恢复后的 `node_modules` 形态。
- 用组件内部依赖、显式 CSS import、worker 和静态资源证明标准工具链行为。

### B. 收敛 Emit

- 将 `LibraryPackageWriter` 收敛为根 `package.json` writer。
- 将源码 carrier 直接写入项目源码目录。
- 让 Deno restore/check、NetPack 和 DenoHost 始终使用同一项目根。
- 用 EmitTest 固定 identity 冲突、lock 更新、离线检查、构建失败和原子提交。

### C. 收敛 NetPack 与 SSR

- NetPack 从 `jazor/entry.mjs` 构图，并把 `jazor/` 作为 package resolution root。
- DenoHost 从 `jazor/ssr-entry.mjs` 运行，复用项目 lock 与 `node_modules`。
- 让生产路径只保留项目根、可见入口和标准 package resolution。

### D. 迁移绑定库

- 枚举所有 `src/ECMAScript.*/manifest.json`，把绑定运行时来源统一为 npm/JSR。
- TDesign、Element Plus、Vuetify 验证公开组件入口、内部依赖、Vue peer 和组件 CSS。
- VueUse、VueI18n、VeeValidate、DateFns、Pinia、VueRoute 和 VueQuery 验证 named export 与函数级裁剪。
- VueDataUi、FilePond、Monaco、图标和编辑器类绑定验证全局样式、worker、字体、图片和 wasm。
- VueDraggable 等高耦合库验证正式根入口的完整运行时闭包。
- ECMAScript 的 `clr/**` 与所有 `ECMAScriptModule` 生成结果按项目源码路径交付。

### E. 固定交付证据

- 更新架构、绑定指南、consumer fixture、质量门禁和 CHANGELOG。
- 保存生成的 `package.json`、`deno.lock`、Deno 检查结果与 NetPack metafile。
- 让源码 ProjectReference 与 NuGet consumer 通过同一套浏览器和 SSR 验收。

## 验收

| 场景 | 必须证明 |
| --- | --- |
| 标准项目 | 源码、入口、`package.json`、lock、`node_modules` 与 `dist/` 位于同一 `jazor/` 根 |
| npm/JSR | dependency key、精确 identity、生成 import 与恢复后的公开入口一致 |
| ECMAScript 源码 | `clr/**` 与其他生成模块通过相对 import 被 Deno 和 NetPack 直接解析 |
| Emit | 项目生成、restore/ci、frozen check、NetPack 调用和原子提交具有回归证据 |
| JavaScript | 单组件或单函数入口只保留可达 export/module，共享模块保持单一实例 |
| 组件内部依赖 | 组件实现、共享 helper、Vue peer、初始化模块和必要副作用保持运行语义 |
| CSS | 已用组件样式和必要全局样式进入输出，未到达组件的独立样式不进入输出 |
| worker/static | worker、字体、图片和 wasm 由标准 import/URL 图解析并输出 |
| SSR | DenoHost 直接使用 Emit 已恢复的项目、`node_modules` 与 frozen lock |
| 确定性 | 相同源码、工具版本和 dependency identity 产生稳定项目文件、lock 与构建结果 |

## 完成定义

- `jazor/` 可由 Deno 2.9.7 直接 restore、check 和 run，并可由 NetPack 直接打包。
- 所有外部 ECMAScript 绑定通过 npm/JSR dependency 与公开 ESM specifier 接入。
- ECMAScript 的 `clr/**`、生成模块和其他自有 MJS 作为普通项目源码交付。
- Emit、NetPack 与 DenoHost 共享一个项目根、一张标准 ESM 图和一次依赖恢复结果。
- 仓库 fixture 证明未使用的方法、组件及其独立 CSS 被裁剪，同时保留组件内部依赖与必要副作用。
- 架构、指南、测试门禁和能力状态使用同一标准项目契约。
