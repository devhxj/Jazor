# npm/JSR 绑定与标准 Jazor 项目计划

> 状态：进行中（P0-5）。目标是让 `jazor/` 成为 Deno 2.9.7、NetPack 和 DenoHost 可以直接消费的标准前端项目，并在标准 ESM 图上完成细粒度 tree shaking。

## 设计结论

1. `jazor/` 是唯一项目根。源码、入口、`package.json`、锁文件、`node_modules` 和构建输出都在该目录。
2. ECMAScript 自有 MJS 是项目源码。`src/ECMAScript/clr/**` 写入 `jazor/clr/**`，其他 `ECMAScriptModule` 按声明路径写入项目。
3. ECMAScript 绑定库只绑定 npm 或 JSR 包，交付强类型 C# API、精确 package identity 和标准 ESM specifier。绑定库经声明面表达运行时图：`[ECMAScript("<specifier>")]` 给出 import，导出名由 `[ECMAScriptName]` / `[Description("@#...")]` 给出，样式边由可重复的 `[Style]` 声明。Emit 只汇总已声明结果，不解析上游包内部结构。worker 与 static 资源经包内模块边交付（如 Monaco 的 worker 是 `imports` 条目），不构成独立资源类型；license 由绑定包的 NuGet `licenses/**` 交付，不参与运行时图。
4. Emit 在 MSBuild 阶段生成并恢复项目。恢复是项目生成的一部分，与 SSR 是否启用无关。
5. Deno 负责依赖恢复、`node_modules`、`deno.lock` 和入口检查；NetPack 负责浏览器构建；DenoHost 负责 SSR。三者直接使用同一项目。
6. **Emit 的职责边界**：生成 `jazor/` 项目、把 ECMAScript 自有源码按声明路径写进项目树、调用 `deno install` 装入 `ECMAScript.*` 绑定的 npm/JSR 依赖，然后交棒。绑定包内部的模块引用关系不在 Emit：它由绑定库经声明面固化为生成模块里的合法 import specifier，Emit 只汇总结果，不解析、不推断、不重建上游包内部结构。

## 标准项目结果

```text
jazor/
  package.json          # Emit 生成：精确依赖与公开入口
  package-lock.json     # 存在完整 npm lock 时保留
  deno.lock             # Deno 2.9.7 生成并冻结
  node_modules/         # Deno 恢复，NetPack 与 DenoHost 共用
  entry.js              # 浏览器入口
  ssr-entry.js          # 启用 SSR 时生成
  clr/                  # ECMAScript CLR 源码
  <module-paths>/       # 其他生成源码与 source map
  <local-assets>/       # 项目源码直接引用的本地资源
  dist/                 # NetPack Release 输出
```

根 `package.json` 使用标准字段：`name`、`private`、`type`、`main`、`exports` 和 `dependencies`。`exports["."]` 指向 `entry.js`；存在 SSR 入口时增加 `exports["./ssr"]`。

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
- `[ECMAScript]` 声明的标准 ESM specifier；
- 名字机制（`[ECMAScriptName]` / `[Description("@#...")]`）给出的输出名与导出名；
- `[Style]` 声明的样式 side-effect specifier；
- npm/JSR dependency key、精确版本和可用的 integrity；
- 上游快照、许可证与 inventory。

绑定包交付 package identity 与入口声明，运行时代码由 npm/JSR 恢复。恢复后的上游 `package.json`、`exports`、conditions、`sideEffects`、dependencies 和 peer dependencies 是包解析与 tree shaking 的标准依据。

## 模块与包映射

`[ECMAScript("<specifier>")]` 保存生成源码最终写出的 import specifier。它直接使用 Deno 与 NetPack 可以解析的公开入口：

```csharp
[ECMAScript("tdesign-vue-next/es/button/index.mjs")]
[ECMAScriptName("Button")]
[Style("tdesign-vue-next/es/button/style/index.css")]
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

成员级 `[ECMAScript]` 优先于类型级声明；导出名由名字机制给出；组件身份由 `ComponentBase` + Vue marker 约定判定；样式边由 `[Style]` 声明。绑定库经这些声明把运行时模块图固定为公开入口，Emit 读取声明结果而不重新解析绑定包的内部模块结构。

绑定优先选择上游公开的最细 ESM 入口。上游只提供正式根入口时直接使用根入口；VueDraggable 等高耦合库由该入口保留完整运行时闭包。

同一个 dependency key 只能对应一个精确 identity。生成源码出现 package import 而绑定声明缺失，或多个绑定为同一 key 声明不同 identity 时，Emit 在恢复前报告冲突。

## Emit 必须完成的工作

Emit 是项目生成器，按以下顺序执行；失败显式传播，不做整目录快照或回滚：

1. **收集入口**：从最终宿主和 `ModuleCatalog` 取得浏览器 Entries、可选 SSR Entries、生成模块、source map、package imports 与本地资源。
2. **原地生成项目**：Emit 直接在最终 `jazor/` 写入源码、入口、`package.json` 与锁文件；单文件用临时文件加 rename，避免出现半写文件，过期文件按清单差异删除。写入就地生效：中断或失败时项目可能处于未收敛状态，下一次构建按同一规则收敛，不保留整目录快照。

依赖层（`package.json`、`deno.lock`、`node_modules`）只在 dependency identity 变化时更新；常见重建只重写源码层（`clr/**`、生成模块、入口、source map），并对一致的锁文件做 frozen 校验。`node_modules` 是可从锁文件重建的派生缓存，由 Deno 从全局缓存恢复。

3. **写入源码**：把 `clr/**`、其他源码 carrier、生成的 `ECMAScriptModule`、source map 和源码资源写到最终相对位置。
4. **写入标准 import**：项目内模块使用相对 specifier；npm/JSR 绑定保留编译器已固化的 `[ECMAScript]` bare specifier；`[Style]` 声明以普通 side-effect import 写入。Emit 只收集这两个已声明的结果。
5. **生成可见入口**：`entry.js` 连接实际浏览器 Entries；启用 SSR 时生成 `ssr-entry.js`。这些文件是 Deno、NetPack 和 DenoHost 共用的真实入口。
6. **生成 `package.json`**：从实际 package imports 合并 dependency key 与精确 identity，并写入标准入口字段。
7. **恢复与冻结**：在项目根调用 Deno 2.9.7，生成或验证 `deno.lock` 与 `node_modules`。
8. **检查入口**：使用本地 `node_modules`、`--no-remote` 和 frozen lock 检查所有生成入口。
9. **调用消费者**：Release 让 NetPack 从 `entry.js` 构建 `dist/`；SSR 运行时让 DenoHost 从同一项目加载 `ssr-entry.js`。
10. **交付结果**：源码、项目文件、lock、恢复结果与构建结果就地生效；任一步失败即返回错误，由下一次构建收敛。

首次恢复或 dependency identity 变化：

```text
deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=false
```

锁文件一致后的确定性恢复：

```text
deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=true
```

入口检查：

```text
deno check --node-modules-dir=manual --no-remote --no-config --frozen-lockfile entry.js
```

Deno 2.9.7 的 `--frozen=true` 按 lock 执行确定性恢复。`--node-modules-dir=manual` 与 `--node-modules-linker=hoisted` 在首次恢复、确定性恢复与入口检查三处保持一致，避免 `node_modules` 布局漂移；`--no-config` 使检查只使用生成项目的 `package.json`。Emit 从 DenoHost runtime package 取得同版本可执行文件。

## 标准构建行为

NetPack 的工作根、解析根和入口都来自 `jazor/`：

```text
entry.js
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

## 实施进度

已落地的切片（按提交顺序）：

| 切片 | 内容 | 提交 |
| --- | --- | --- |
| D-0 声明面收口 | 删除 `Transform` 与三参数构造；组件身份改约定判定；导出名走名字机制；新增 `[Style]`（`ECMAScript.Contract`） | `d5777106` |
| D-1 绑定迁移 | 2294 处组件声明改单参数形态，232 处补 `[ECMAScriptName]`，1 处 `default` 显式化 | `d5777106` |
| B1 就地写入 | `ModuleWriter` 逐文件 temp+rename；退役 `OutputTransaction`、`EmitOptions.Clean`、`ImportMapWriter` 事务、`DirectoryTransaction`、`--clean true` | `3b8ea74b`、`3c2830bb` |
| 样式边 | `[Style]` 在使用点发射为 side-effect import，顺序即层叠顺序 | `16babeb4` |
| D3 载体相对化 | 载体内部 import 全改相对 specifier；`clr/` 前缀纳入声明路径，使声明路径即项目路径 | `9715ec80`、`fba111da` |
| B2 物化退役 | embedded carrier 按声明路径写入源码树；退役 `packages/` 投影、合成 `file:` 本地包与合成 `package-lock.json`；`LibraryPackageWriter` 收敛为根 `package.json` writer | `fba111da` |
| D3 收尾 | carrier 逻辑键单列通道（`CarrierImportKeys`）：carrier 写出的 specifier 已是相对的，逻辑键无法从生成文本反推，供 `--library-manifest` 选择使用 | `907af628` |
| D-2 绑定 specifier 收敛 | `[ECMAScript]` 改为上游公开入口；manifest 键收敛为同一 specifier 并按「模块 × 导出名」归并；生成器 `--check` 门禁改为按上游 `exports` 校验可解析性 | `b278bb57`、`d7f4327a` |
| B3 单根收敛 | `ToolchainRequest` 四根 → `ProjectRoot`；CLI `--artifacts/--source-root/--out-root` → `--root`；bundle 固定 `jazor/dist/`；宿主探测同步 | `aae07532` |
| E 交付证据 | CHANGELOG 逐项记录破坏性变更与迁移路径 | `6d4b0973` |

当前测试基线：

| 套件 | 结果 |
| --- | --- |
| `Jazor.CompilerTest` | 10714 / 10714（全绿；D-2 前为 10713 / 10714） |
| `Jazor.RazorVue.Sg.Test` | 5010 / 5010（全绿；D-2 前为 5004 / 5010） |
| `Jazor.EmitTest` | 224 / 224（全绿；D3/B2 切片开始时为 9 个失败） |
| `Jazor.CLR.Test` | 4963 / 5089（126 个既有失败；改动前基线为 127） |
| 生成器门禁 | `vuetify --check`、`elementplus --check`、`tdesign components --check` 全部通过 |

### 剩余：阶段 C（NetPack 与 SSR 收敛）

阶段 C 需要先落地**两个尚不存在的产物**，因此与前面的切片分开实施：

- **`entry.js` / `ssr-entry.js` 的发射**。当前 Emit 不产出可见入口，NetPack 靠合成 `__jazor_entry__/<stem>.mjs`（按根程序集的模块列表 `export *`）充当入口，SSR 则由宿主在运行根写 `@jazor/ssr-runner.mjs`。C 阶段要把入口变成 Emit 的正式产物：浏览器 `entry.js`、SSR `ssr-entry.js`，二者都是普通项目源码，可被 Deno 与 NetPack 直接解析。
- **NetPack 直接以项目根构图**。入口存在后，`__jazor_netpack_bundle__` 临时工作区（含 `CopyMaterializedLibraryFiles`/`CopyMaterializedAssets`/`CopyLibraryPublishAssetsToOutput`/`CopyStaticAssetsToOutput` 的逐文件搬运）与 `WriteBundleCssAsync`（样式拼接、`ResolveExternalStylesheetPaths`）一并退役：资源由标准 import/URL 图进入打包，样式由打包器处理。

这两项是「Emit 生成项目并交棒」契约的最后一段，完成后 `jazor/` 才真正只由项目根、可见入口与标准 package resolution 构成。B3 已把消费入口的根收敛到位，因此 C 阶段不再需要触碰 `ToolchainRequest`。

仍未开始：仅剩阶段 C（见下节）。

### D-2 已落地：绑定 specifier 收敛到上游公开入口

`D-1` 只统一了声明形态；`D-2` 把 `[ECMAScript]` 的 **specifier 取值**从 Jazor 自造简写改为上游公开入口，并让 manifest 键与声明值同源。

**收敛前的形态**：声明写简写，真值另存在 `manifest.json` 的 `imports.*.production` 里，两者靠绑定私有的别名通道连接。这些简写在上游并不存在：

| 绑定 | 收敛前 | 收敛后（上游公开入口） |
| --- | --- | --- |
| TDesign | `tdesign-vue-next/button/Button`（119 条） | `tdesign-vue-next/es/button/index.mjs` |
| ElementPlus | `element-plus/affix/ElAffix`（112 条） | `element-plus/es/components/affix/index.mjs` |
| Vuetify | `vuetify/components/VCardActions`（32 条） | `vuetify/components/VCard` 等真实模块 |
| Monaco | `monaco-editor`（1 条） | `monaco-editor/editor/editor.api.js` |

逐条按上游包验证的三个反例：

- `tdesign-vue-next/button/Button`：`es/button/` 下只有 `index.mjs`、`button.mjs`、`props.mjs` 与 `style/`；`tdesign-vue-next@1.20.7` 无 `exports` 字段，按文件布局字面解析，该 specifier 直接失败。
- `vuetify/components/VCardActions`：`vuetify@4.2.1` 的 `exports["./components/*"] → ./lib/components/*/index.js` 只对真实目录成立，而 `lib/components/` 下没有 `VCardActions` 目录（它在 `VCard/` 内）。
- `element-plus/affix/ElAffix`：该形态不对应任何真实文件；`element-plus@2.14.5` 的 `exports` 只有 `"./es/*.mjs"` 通配与 `"./*"` 兜底。

**两处结构性问题一并解决**：

1. **Vuetify 子组件共享父目录是系统性现象**：`VCard` 目录导出 6 个组件，`VGrid` 目录导出 4 个。因此收敛规则精确表述为**同一「模块 × 导出名」对必须一致**；一个模块导出多个组件是上游正常形态。`manifest.json` 的 119→73（TDesign）、112→81（ElementPlus）、123→109（Vuetify）归并即由此而来。
2. **labs→stable 迁移**：`VCalendar` 等 9 个组件已从 `vuetify/labs/components/` 迁到 `vuetify/components/`，labs 路径在当前版本解析失败。声明的 specifier 必须跟随上游可解析路径；catalog 分组用的 family 由 `contracts.json` 提供，不再从 specifier 反推。

**门禁**：`VuetifyCatalogGenerator` 新增 `ValidateImportSpecifiers`，逐个把声明 specifier 按固定版本上游 `exports`（精确键 / 单层通配 / 兜底）解析成真实文件，解析失败即构建错误——不再依赖绑定私有别名表。`update-vue-binding-inputs` 输出同形结果并显式校验 `development`/`production` 一致（成对 profile 字段按 `D-1` 计划退役）。

**收尾**：`f323af59` 曾误删 8 个 ElementPlus 快照元数据（4 个 `.d.ts` + 4 个 `.mjs.map`），导致 `elementplus --check` 无法运行；已从固定的 `node_modules` 恢复并逐一比对字节一致。

## 实施顺序

### A. 固定标准项目 fixture

- 生成包含一个 npm binding、一个 JSR binding、`clr/**` 和一个生成模块的最小项目。
- 固定 dependency key、精确 identity、标准 specifier、入口、lock 与恢复后的 `node_modules` 形态。
- 用组件内部依赖、显式 CSS import、worker 和静态资源证明标准工具链行为。

### B. 收敛 Emit

- 将 `LibraryPackageWriter` 收敛为根 `package.json` writer。
- 将源码 carrier 直接写入项目源码目录。
- 让 Deno restore/check、NetPack 和 DenoHost 始终使用同一项目根。
- 退役 `LibraryMaterializer` 的资源物化路径：vendor 树与按 entry 复制的样式不再由 Emit 复制；样式、worker 与 static 遵循标准 JS 项目范式，经包内 import 或 `new URL(..., import.meta.url)` 进入 NetPack 可达图；license 不参与构建图，由绑定包的 NuGet `licenses/**` 与输出层声明交付，门禁继续断言。ECMAScript 自有 `clr/**`、其他生成模块和源码本地资源仍按项目相对路径写入。样式的下游处理不属于 Jazor/Emit（见契约文档“样式边”）。
- 将绑定 `manifest.json` 降级为声明与诊断证据，不再作为 Emit 的物化输入；生成器与门禁继续使用 package identity、specifier、hash、inventory、许可证和上游快照做一致性检查。
- 用 EmitTest 固定 `LibraryMaterializer` 退役、identity 冲突、lock 更新、离线检查、构建失败传播与增量收敛；绑定门禁继续验证 hash/inventory，标准项目测试验证上游 exports、sideEffects 以及 JS、CSS、worker 和静态资源的可达性。

#### B 退役清单（实现映射）

处置语义：**退役** = 删除该类型/成员；**改写** = 职责保留、实现方式替换；**保留** = 本次不动。行号对应当前工作树，指向类型/成员声明行。

##### B1 整目录事务与 staging（第一刀）

| 文件 | 类型 / 成员 | 处置 | 替代 |
| --- | --- | --- | --- |
| `EmitPipeline.cs` | `OutputTransaction`（`CreateAsync:607`、`CommitAsync:629`、`DisposeAsync:674`、`CopyDirectory:712`、`TryMoveDirectory:694`） | 退役 | 直接写入 `JazorDir`；不再建 `.jazor-output-*` / `.jazor-output-backup-*` |
| `EmitPipeline.cs` | `OutputTransaction.CreateAsync` 的 `clean=false` 分支（`:624`） | 退役 | 增量重建不再递归复制上一份输出；依赖清单差异收敛 |
| `EmitPipeline.cs` | SSR 额外物化根 `.jazor-ssr-materialization-*`（`:96`）与 `finally` 清理（`:196`） | 退役 | SSR 闭包与浏览器闭包同根，不再需要隔离的临时包工作区 |
| `EmitPipeline.cs` | `MergeDirectory:211` | 退役 | 仅 `packages/` 物化被删除后无合并对象；源码 carrier 就地写入 |
| `ModuleWriter.cs` | `MaterializationTransaction`（`.jazor-emit-*`、`backup/NNNNNNNN`、`committed` 回滚，`:410`–`:537`） | 退役 | 逐文件 temp+rename 覆盖；失败显式返回 |
| `ImportMapWriter.cs` | `CommitAsync:405`（`.jazor-importmap-*` + `.jazor-importmap-backup-*` + 回滚） | 改写 | 就地 temp+rename 写 `importmap.json` / `ssr-importmap.json` / `manifest.json` |
| `LibraryMaterializer.cs` | `MaterializationPlan.Commit:1101`（`.jazor-library-*`、按根目录备份与回滚、legacy 根 `vendor`/`ecmascript`/`embedded` 清理 `:1112`） | 退役 | 物化本身退役；legacy 根的一次性清理改为普通过期目录清理 |
| `DirectoryTransaction.cs` | 整个文件（`Move:14`） | 退役 | 无目录移动调用方 |
| `EmitOptions.cs` | `Clean`（字段 `:9`、默认 `true` `:25`、`--clean` `:56`） | 退役 | 增量收敛由清单差异决定；`Jazor.targets:106`、`:129` 同步去掉 `--clean true` |

退役后应用项目目录不再出现 `.jazor-output-*`、`.jazor-output-backup-*`、`.jazor-emit-*`、`.jazor-library-*`、`.jazor-library-backup-*`、`.jazor-importmap-*`、`.jazor-ssr-materialization-*` 这些兄弟目录。它们今天全部落在 `JazorDir` 的父目录（即应用项目目录）里，而 `Jazor.targets:25-30` 的默认项排除只覆盖 `$(JazorDir)**`，因此这些临时目录会暴露给 SDK 默认 glob 与 `dotnet watch`。

##### B2 资源物化与包投影

| 文件 | 类型 / 成员 | 处置 | 替代 |
| --- | --- | --- | --- |
| `LibraryMaterializer.cs` | `Materialize:22`、`MaterializationPlan`（`Add`/`AddStyle`/`AddEntryClosure`）、`LibraryPublishAsset:1237`、`MaterializedModuleOwner:1232`、`ImportEntry:1249` | 退役 | Deno restore 的 `node_modules` + NetPack 的 `exports`/`conditions`/`sideEffects` 图 |
| `LibraryMaterializer.cs` | `AddExternalStyle:1064`、`AddExternalStylesheet:1078`、`IsStylesheetSpecifier:1089`、`ImportEntry.GetStyleImports:1287`、`GetStylesheetImports:1290`、`GetStyles:1284` | 退役 | 两套分类集合合并为一条 side-effect import 边，由编译器写入生成模块；绑定不做形态判断，下游处理见契约文档“样式边” |
| `LibraryMaterializer.cs` | `LibraryManifest.Load`/`LoadMetadata`/`LoadCore`/`ValidateAllFiles:1478`/`ManifestFile:1242`/`LibraryPackageReference:1209` | 收窄 | 只作声明与诊断证据：package identity、specifier、hash、inventory 断言；文件字节校验退役 |
| `LibraryMaterializer.cs` | `LibraryAssets` 的 `MaterializedPaths`/`PublishAssets`/`PackageProjections`（`:1193`） | 退役 | 无物化文件可枚举，无包投影可生成 |
| `LibraryMaterializer.cs` | `LibraryAssets` 的 `StylePaths`/`ExternalStyleModuleImports`/`ExternalStylesheetPaths` | 退役 | 只剩一条 side-effect import 边（由 `[Style]` 在使用点发射），不再需要形态分类 |
| `LibraryMaterializer.cs` | `ImportPaths`/`BrowserImportPaths`、`Specifier` 解析与 `Provider` 匹配（`SelectImports:332`、`CreateImportIndex:274`） | 保留 | 仍用于把 bare specifier 归类为 npm/JSR dependency 与冲突检测；embedded 改用相对路径后 `BrowserImportPaths` 退役 |

**embedded 源码 carrier 必须离开 `packages/` 投影。** 这是“Emit 抽出源码”当前被挡住的唯一位置：`clr/**`、`vu-icons`、`jazor-vue-runtime` 三个 carrier 全部被强制写进合成的本地包根，而不是项目源码树。

| 文件 | 类型 / 成员 | 处置 | 替代 |
| --- | --- | --- | --- |
| `LibraryMaterializer.cs` | `MaterializationPlan.GetPackageRootRelativePath:969`（`"packages/" + name` 硬前缀） | 改写 | 返回 carrier 声明的项目源码路径：`clr/**` → `clr/**`，`vu-icons` → `runtime/vu-icons/**`，`jazor-vue-runtime` → `dist/**` |
| `LibraryMaterializer.cs` | `MaterializationPlan.GetPackageFilePath:991`（对 core source 剥 `clr/` 再套 `packages/<System 或 Microsoft>/`） | 退役 | 路径直接来自声明，不再有前缀剥换 |
| `LibraryMaterializer.cs` | `LibraryManifest.GetPackageNameForPath:1388`、`IsCoreSource:1382`、`IsEmbedded:1385`、`GetPackageReference:1365` 的 embedded 分支 | 改写 | 保留“这是 Jazor 自有源码、不是 npm/JSR 包”的判定；删除由它推导合成包名的用途 |
| `LibraryMaterializer.cs` | `MaterializationPlan.AddImport:1015` 的 embedded 调用（用改写后的 `packages/...` 路径作为模块 import 目标） | 改写 | 用项目源码相对路径作为 import 目标，与普通项目模块同一条路径规则 |
| `LibraryMaterializer.cs` | `GetPackageExportName:608`、`LibraryPackageProjection`、`Plan.ModuleOwners:126` 的投影循环 | 退役 | 源码不是包，没有 `exports` 需要合成 |

三个 carrier 的现状规模（`imports` 条目数）：`ecmascript` 78 条（`clr/**`）、`vu-icons` 1822 条、`jazor-vue-runtime` 4 条，合计 1826 条源码条目今天都落在 `jazor/packages/**` 下。

| 文件 | 类型 / 成员 | 处置 | 替代 |
| --- | --- | --- | --- |
| `LibraryPackageWriter.cs` | `WriteEmbeddedPackage:146`、`WritePackageManifest:169`、`CopyDirectory:185`、`GetEmbeddedPackagePath:163` | 退役 | ECMAScript 源码 carrier 直接写项目源码目录；绑定只交付 npm/JSR identity，不再有 `packages/<name>` 本地包 |
| `LibraryPackageWriter.cs` | `WritePackageLock:208`、`LooksLikeConcreteVersion:276`、`CanWriteCompleteNpmLock:105` 的合成锁分支 | 退役 | `deno.lock` 由 Deno 生成；`package-lock.json` 仅在存在与 `package.json` 一致的完整 npm lock 时保留 |
| `LibraryPackageWriter.cs` | `WritePackageProject:16` | 收敛为唯一职责 | 仅写根 `package.json`：标准字段 + `dependencies` + `jazor.packages` identity 记录；补 `main`、`exports["."]`、SSR 时的 `exports["./ssr"]` |
| `LibraryPackageWriter.cs` | `WriteJson:199` | 改写 | 走单文件 temp+rename |
| `ManifestModel.cs` | `ModuleEntry:635`、`AssetEntry:648`、`Save:129` | 收窄为诊断证据 | 条目内容不变，但不再驱动资源物化或 publish-asset 判定；`Save` 走 temp+rename |

##### B3 消费入口收敛

**已落地（本切片）**：`ToolchainRequest` 的四根契约（`ArtifactRoot`/`SourceRoot`/`OutputRoot`/`PackageRoot`）收敛为单一 `ProjectRoot`，CLI 参数从 `--artifacts --source-root --out-root` 收敛为 `--root`，`BundleOutputPath` 固定在 `<projectRoot>/dist/bundle.js`。生产路径上这四个根始终指向同一个 `jazor/`，拆开只让调用方承担一致性责任。`BundleOptions` 同步收敛（`InputDirectory`/`SourceRoot`/`PackageRoot` → `ProjectRoot`）。

区分「调用方已恢复依赖」的判据改为 `MaterializedLibraries is null`：Emit 先在自己的项目根完成 restore 再传物化结果，直接调用 Toolchain 的调用方没有恢复过，需要在临时工作区补一次同样的 restore。

仍待完成（阶段 C 一并处理）：

| 文件 | 类型 / 成员 | 处置 | 替代 |
| --- | --- | --- | --- |
| `NetpackBundler.cs` | bundle 工作区 `__jazor_netpack_bundle__`、合成入口 `__jazor_entry__`、`__jazor_netpack_output__` 与 `finally` 清理 | 退役 | 以 `jazor/` 为解析根、`entry.js` 为入口调用 NetPack，输出 `jazor/dist/` |
| `NetpackBundler.cs` | `CopyMaterializedLibraryFiles`、`CopyMaterializedAssets`、`CopyLibraryPublishAssetsToOutput`、`CopyStaticAssetsToOutput` | 退役 | 资源由标准 import/URL 图进入打包，不再由 Emit/NetPack 逐文件搬运 |
| `NetpackBundler.cs` | `WriteBundleCssAsync`、外部样式解析（`ResolveExternalStylesheetPaths`、`ResolveExternalStyleFile`） | 退役 | 样式是入口的 side-effect import，由打包器处理；Jazor 不拼接 CSS |
| `NetpackBundler.cs` | `PrepareBundledRouteRuntime`、`RewriteModuleImports`、`SelectNetpackOutputs`、`WriteOutputs` | 保留 | 输出选择、source map 重命名与 import 重写仍是 bundle 交付契约 |
| `DenoPackageRestorer.cs` | `RestoreAndCheckAsync`、`CheckAsync`、`RunAsync` | 保留 | argv 一致；工作根已是最终 `jazor/` |
| `CatalogReader.cs` | `CatalogReader`、`CatalogAssetRecord`、`CatalogReadResult` | 保留 | `ModuleCatalog` 读取与源码 carrier 写出 |
| `EmitPipeline.cs` | `ValidateOptions`、`EnsureManifestIsOwnedByOutput`、`GetReservedOutputPaths`、`GetSafePath` | 保留 | 所有权、路径越界与输出冲突校验继续有效 |
| `EmitPipeline.cs` | `RemoveBrowserRawProjection:475`、`DeleteOutputFile:500` | 改写 | 统一为基于 `jazor-manifest.json` 的清单差异清理，含 Release 投影裁剪 |
| `ModuleWriter.cs` | `PrepareModules:57`、`PreparedModule:387`、`Equivalent:302`、`BuildManifest:208`、`ValidateManifestCollision:294` | 保留 | 纯校验与清单；hash/identity/依赖校验不变 |
| `ModuleWriter.cs` | `BuildDesiredFiles:231`、`DesiredFile:404`、`FindStaleFiles:267`、`WriteResult:578` | 保留 | 过期文件就地删除 + `Written`/`Skipped`/`Deleted` 诊断，作为增量收敛的唯一机制 |
| `ImportMapWriter.cs` | import map 组装与 `node_modules`/exports 解析（`AddRestoredPackageImports:196`、`PackageExportsResolver`） | 保留 | 浏览器解析 bare specifier 仍需；移除 styles 数组输出 |
| `ImportMapWriter.cs` | `manifest.json` 的 styles 数组、`SsrArtifactLocator.ReadStylePaths`、宿主与 SSR 的 `<link>` 循环 | 退役 | 样式由标准工具链处理，不需要样式清单与 `<link>` 注入 |
| `Jazor.Emit.csproj` + `Jazor.Vue.nuspec` | `tooling\**\*` 复制规则（`Jazor.Emit.csproj:13`）与 `tooling/vue/compiler-sfc.esm-browser.js`（约 1.76 MB） | 迁移（必须与 `Jazor.Vue` 一起定） | 该载荷由 `Jazor.Vue.nuspec:41-44` 交叉引用，打进 `Jazor.Vue` 包的 `tools/net11.0/tooling/vue/`；核心 `jazor` 包在 `Jazor.csproj:144` 排除了它。仓库内没有任何代码按该文件名加载它，所以它是投递给消费者的 opt-in 载荷，而非死文件。处置要么把文件移入 `Jazor.Vue` 自己的源目录（消除跨项目引用），要么随 SFC 能力一并退役——不能只改 `Jazor.Emit` |

##### B4 测试与 fixture 影响（必须与实现同步改）

| 测试 / fixture | 现在钉住的行为 | 目标 |
| --- | --- | --- |
| `LibraryMaterializerTests`（整类，49 个 `[TestMethod]`） | `packages/vue/...` 物化路径、`vendor/` 不生成、import map 目标前缀 `/jazor/packages/` | 退役或改写为 package identity/specifier 断言 |
| `JazorSsrHostingTests.SsrHostWorkspace.CreateArtifactRootAsync:638` | 手工拼装 artifact root：`Materialize` + `WritePackageProject` + `RestoreAndCheckAsync` + `ImportMapWriter.WriteAsync`，并在 `manifest.json` 写 `"styles":["/jazor/vendor/test.css"]` | 改为按项目路径组装：源码 + `entry.js`/`ssr-entry.js` + `package.json` + Deno restore；样式断言改为断言生成的 side-effect import |
| `EmitPipelineAssetContractTests.ExecuteAsync_RejectsAssetPathThatConflictsWithGeneratedModule:61` | `clean: true` 失败后输出根不存在（staging-only） | 保留"写入前校验失败不产生输出"；补"写入中失败显式返回、下一次构建收敛" |
| `EmitPipelineAssetContractTests.ExecuteAsync_ValidatesAssetSourceHashBeforeReusingExistingOutput:14` | `clean: false` 走增量复制种子路径 | 改为断言既有输出未被破坏，不再依赖 staging 种子 |
| `StaticModuleSourceMapTests.ModuleWriter_Write_*`（`:189`、`:243`、`:309`） | `Written` 计数、同名 map 过期删除、清单规范化 | 断言不变；计数改为不依赖 backup 计算 |
| `SdkIntegrationTests.Build_LocalPackages_WithExternalRazorSgConsumer_CleanBuildsVueRenderArtifactsByteForByte:3344` | 删根重建后逐字节一致 | 保留为确定性门禁 |
| `SdkIntegrationTests.Build_LocalPackages_WithExternalRazorSgConsumer_CleanEmitRemovesDeletedComponentArtifacts:3415` | 删除组件后模块与 map 从输出消失 | 保留为清单差异收敛门禁 |
| `JazorAspNetCoreReloadTests.WriteHmrManifestAsync:636` | 注释称 "Emit publishes manifests from a transaction directory"，用临时文件 + `File.Move` 重试模拟原子替换 | 改为就地单文件 temp+rename；删除原子替换措辞 |
| `ToolchainTests.TryParse_*:14`、`BuildAsync_MissingArtifactRoot_ReturnsTypedContractDiagnostic:748` | 四根 CLI（`--artifacts`/`--out-root`/`--source-root`） | 收敛为单项目根 |
| `LegacyRazorVueContractRetirementTests.SdkTargets_InvokeSingleEmitContractForDebugAndRelease:70` | 钉住 targets 的 flag 列表 | 随 `--clean` 移除同步更新 |
| `SampleGeneratedArtifactLayoutTests.CheckedInSampleManifests_UsePortableRootIdentityAndExistingArtifacts:16` | 钉住三个已入库 `samples/**/jazor/` 根 | 重新生成或将测试改指新布局 |

##### 为什么退役而不是继续维护（现状证据）

1. 宿主在注册时对 artifact root 建立并长期持有目录句柄（`JazorExtensions.cs:193-196`），且要求首次构建早于注册（`:152-155`）；整目录替换会让该挂载与探针失效，必须重启宿主。
2. HMR 在 artifact root 上创建递归 `FileSystemWatcher`（`ReloadService.cs:257-274`），路径在启动时解析一次（`:185-193`）；目录替换会改变目录身份，Windows 保留旧句柄、inotify 跟随旧 inode。
3. `SsrRenderer` 已经就地写入运行根（`@jazor/ssr-runner.js`，`SsrRenderer.cs:175-180`）并以 artifact root 作为 Deno worker 的 `WorkingDirectory`（`:342-362`）；就地写入是消费侧已经假定的语义。
4. `clean=false` 会把整个输出根递归复制到兄弟目录（`EmitPipeline.CopyDirectory:712`），其中包含 `node_modules`；随后该副本被删除并重新恢复。
5. 回滚路径没有测试强制中途失败来验证，只有写入前校验失败与 `clean:false` 种子路径被覆盖，因此事务的失败收敛是未验证代码。

已由交接边界消解：解析 bare specifier、样式消费、非模块资源投递都发生在 Jazor 交棒之后，由 Deno 与 NetPack 按标准 ESM 处理。Jazor 只写出标准 ESM 图，不产出 `importmap.json`、样式清单或 `<link>` 注入。embedded 源码改用相对路径（源码是项目源码，不是包）。

声明面规则已全部定下，无遗留待决项。两项已决规则记录如下：

- **同一模块多处声明强制一致（已定）**：同一 specifier 由多处声明时，specifier、导出名与样式边必须一致；**不一致是构建错误**。ElementPlus 的成员级（`ElementPlusComponentExports.cs`）与类型级（`ElementPlus.Components.generated.cs`）声明在迁移后事实相同，正好作为迁移验收。
- **Jazor 自有产物统一 `.js`（已定）**：根 `package.json` 声明 `"type": "module"`，项目内 `.js` 已是 ESM。生成模块、`entry.js`/`ssr-entry.js`、`ssr-runner.js`、source map 与项目内相对 import 一律 `.js`；**上游 specifier 逐字保留**（如 `element-plus/es/components/affix/style/css.mjs`）。当前实现硬编码 `.mjs`（`ESGenerator.cs:398`），迁移时统一；测试中约 1950 处 `.mjs` 引用多为 fixture 路径字符串，随之更新。

### C. 收敛 NetPack 与 SSR

- NetPack 从 `jazor/entry.js` 构图，并把 `jazor/` 作为 package resolution root；输出固定写入 `jazor/dist/`，移除独立临时 bundle 工作区。
- DenoHost 从 `jazor/ssr-entry.js` 运行，复用项目 lock 与 `node_modules`。
- 让生产路径只保留项目根、可见入口和标准 package resolution。

### D. 迁移绑定库

标准是[类库与标准前端项目契约 · 绑定声明面](../02-architecture/library-artifact-contract.md)。声明面（`[ECMAScript]` 0/1 参数、名字机制、`[Style]`）先定，然后一次性迁移，不逐个站点修。

#### D-0 声明面收口（迁移的前置）

声明面规则已全部确认，以下按已定规则实施。

- **删除 `Transform` 枚举**与 `ECMAScriptAttribute.Validate` 的三条组合规则；`[ECMAScript]` 只保留一个 import 参数。（已定）
- **组件身份改为约定判定**：`ComponentBase` 派生 + 实现 Vue marker。`Util.cs` 中按 `Transform.Component` 跳过的逻辑改为按约定判定；`ComponentSymbolPolicy` 的符号来源从 `Jazor.RazorVue` 下沉或在 `Jazor.Compiler` 按元数据名等价实现。（已定）
- **组件导出名改走名字机制**：删除 `RenderEmitter` 的 `ExportName ?? "default"` 回退；`default` 写成 `[ECMAScriptName("default")]`。（已定）
- **`[ECMAScriptName("")]` 与 `[Description("@#")]` 统一为 stop**：已完成——`Util.GetSymbolNameConfig` 中空 `ECMAScriptName` 由 `JsNameConfig.None` 改为 `Stop`，两种写法现在都产生名称解析边界（`HasNameResolutionBoundary` 为 true），测试期望同步更新（`UtilBoundaryScenarioTests.BlankExplicitNameSuppressesDescription`）。**仍待收口**：`GetStringEnumLiteralText` 直接读特性参数、绕过统一解析，string enum 成员的 stop 取值语义（空值 vs 符号名 vs 报错）未定。
- **`[Style]` 新增**：特性名、可重复性、顺序语义均已定（可重复；声明顺序即导入顺序与 CSS 层叠顺序）。形态判别已退役；归属规则与物化位置已定（见契约文档“样式边”）。
- **生成器物化必须字段透明**：`VuetifyCatalogGenerator` 等处不得硬编码旧参数形态。（已定）
- **声明是唯一真相、manifest 降为派生证据**：`manifest.json` 的样式相关字段随形态判别退役；license 由 NuGet `licenses/**` 交付，不参与运行时图。（已定）

#### D-1 绑定迁移（一次性）

| 项 | 范围 |
| --- | --- |
| 删除 `Transform.Component` 参数 | 2260 处类型级声明：VuIcons 1822、TDesign 136、Vuetify 114、ElementPlus 111、VueDataUi 71、VueRoute 2、WangEditor 2、FilePond 1、VueDraggable 1 |
| 补 `[ECMAScriptName]`（导出名 ≠ 类型名） | 214 处，例如 `ElVirtualizedSelect → ElSelectV2`、`TAffix → Affix`、`VueRouterLink → RouterLink`、`VueDraggableList → VueDraggable` |
| 补 `[ECMAScriptName("default")]` | 1 处（`VueFilePond`） |
| 免写（导出名 == 类型名） | 2046 处，靠符号名回退 |
| 成对 profile 字段整体退役 | `imports.*` 的 `development*` / `production*` 成对字段全部退役（`development`/`production`、`developmentDependencies`/`productionDependencies`、`developmentStyleImports`、`developmentStylesheetImports` 及其 production 对应项）。声明面每个特性只写一个 specifier；profile 差异由上游 `exports` conditions 解析 |
| 形态判别整体退役 | `IsStylesheetSpecifier` 的 `.css`/`.sass`/`.less`/`.styl` 判断退役；不再把样式边分为“JS 模块”与“样式表”两类，只保留一条 side-effect import 边 |

其余迁移步骤：

- 枚举所有 `src/ECMAScript.*/manifest.json` 作为迁移清单与上游证据，校验其 package identity 与入口声明；绑定运行时代码来源从包内 `dist` 切换为 npm/JSR 恢复。
- 运行时自选资源（Monaco worker、图标库样式等）迁移到标准 JS 资源入口：上游公开 `new URL(..., import.meta.url)` / CSS import 时跟随该边，上游缺失时由源码 carrier 提供这类标准入口；不再保留 Jazor 专属 worker/static 特例。
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
| Emit | 项目生成、确定性 restore、frozen check、NetPack 调用、失败传播与增量收敛具有回归证据 |
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
