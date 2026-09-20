# 类库与标准前端项目契约

> 适用范围：多项目解决方案、NuGet 类库、最终宿主、ECMAScript 源码与 npm/JSR 绑定。实施状态见[当前状态](../04-roadmap/current-status.md)，迁移步骤见[npm/JSR 绑定与标准 Jazor 项目计划](../04-roadmap/npm-jsr-binding-tree-shaking-plan.md)。

## 核心模型

最终宿主拥有一个标准前端项目根 `jazor/`。Emit 只把 Jazor 的编译结果和绑定声明接入这个项目，Deno、NetPack 与 DenoHost 直接使用项目文件。

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
  entry.mjs
  ssr-entry.mjs         # 启用 SSR 时生成
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
- named/default export 与 `Transform`；
- 上游要求调用方显式导入时的 CSS side-effect specifier。

`[ECMAScript("<specifier>")]` 是生成 import 的真源。成员级声明优先于类型级声明。specifier 直接由恢复后的标准项目解析。

| 来源 | `package.json` | 生成源码 |
| --- | --- | --- |
| npm | dependency key 对应精确 npm 版本 | 保持公开 bare specifier |
| JSR | dependency key 对应精确 `jsr:` value | 保持该 key 下的公开 specifier |
| 项目源码 | 无 dependency | 写出相对 specifier |

绑定选择上游公开的细粒度 ESM 入口。例如 TDesign Button 使用 `tdesign-vue-next/es/button/index.mjs`，该入口继续引用组件实现、共享 helper、Vue 和样式。上游正式 API 只有根入口时，绑定使用根入口及其完整运行时闭包。

绑定 metadata 连接 C# contract 与 dependency identity，并保存版本、integrity、许可证和上游快照。恢复后的上游 package 定义 exports、conditions、`sideEffects`、传递依赖、worker 与静态资源。

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
| `Jazor.Emit` | 写源码与入口，生成 `package.json`，调用 Deno，提交标准项目 |
| Deno 2.9.7 | restore、`node_modules`、`deno.lock` 与入口检查 |
| NetPack | package resolution、conditions、`sideEffects`、ESM/CSS/asset tree shaking |
| DenoHost | 从已恢复项目执行 SSR |

Emit 在项目边界完成两种转换：源码 carrier 变成项目文件，绑定 declaration 变成 `dependencies` 与 bare import。随后使用标准项目语义完成恢复、构建和运行。

## Emit 事务

1. 收集应用 Entries、生成模块、ECMAScript 源码、package imports 和本地资源。
2. 在同卷 staging 根按最终路径写出源码、source map 与本地资源。
3. 生成 `entry.mjs`、可选 `ssr-entry.mjs` 和根 `package.json`。
4. 调用 Deno 2.9.7 恢复依赖、生成或验证 `deno.lock`，并 frozen check 所有入口。
5. Release 模式让 NetPack 从同一项目根生成 `dist/`。
6. 所有步骤成功后原子提交整个 `jazor/`。

首次生成或 dependency identity 变化时使用 `deno install --package-json --node-modules-dir=auto --frozen=false`；lock 一致时使用 `deno ci`；入口检查使用 `deno check --node-modules-dir=manual --no-remote --frozen-lockfile`。

## 一致性与验收

- 同一 dependency key 对应一个精确 package identity。
- 每个 bare import 都能由根 `package.json` 和恢复后的 package 解析。
- 每个项目内 import 都能按最终相对路径解析。
- package 内部 import、peer dependency、初始化和 `sideEffects` 由上游 package 保持。
- Debug、Release、HMR 和 SSR 使用同一项目根、源码树、dependency identity 与恢复结果。
- 项目生成、恢复、检查或构建失败时，最终 `jazor/` 保持上一份完整结果。
- Deno 2.9.7 能在 `jazor/` 直接 restore、check 和 run。
- NetPack 能从 `jazor/entry.mjs` 解析 `node_modules` 并输出 `jazor/dist/`。
- DenoHost 能从同一项目根加载 `ssr-entry.mjs`。
- 单组件或单函数 consumer 只保留入口可达的 ESM 与资源，同时保留组件内部依赖和必要副作用。
