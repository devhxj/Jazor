# 产物管线

> 适用范围：C# 编译模块、ECMAScript 源码、npm/JSR 绑定、Jazor.Emit、Deno、NetPack、SSR 与 HMR。标准项目契约见[类库与标准前端项目契约](./library-artifact-contract.md)。

## 一个项目根

最终宿主的前端产物统一位于 `jazor/`：

```text
ModuleCatalog + ECMAScript 源码 + binding declarations
                         |
                         v
                     Jazor.Emit
                         |
                         v
             jazor/（源码、入口、package.json）
                         |
             +-----------+-----------+
             |                       |
             v                       v
       Deno restore/check       NetPack / DenoHost
```

Emit 是 MSBuild 阶段的项目适配器。它把 Jazor carrier 转换为标准项目文件，再调用标准工具。项目形成后，包解析、条件选择、模块可达性和资源裁剪遵循 Deno、npm/JSR package 与 NetPack 的规则。

## 输入与所有权

| 输入 | 产生方 | Emit 的处理 | 项目结果 |
| --- | --- | --- | --- |
| `Jazor.Generated.ModuleCatalog` | `Jazor.Compiler` / RazorVue | 读取生成模块、Entries、source map、相对依赖与 package imports | 生成的 `.mjs`、`.map` 与相对 import |
| ECMAScript 源码 carrier | ECMAScript 源码库 | 按声明路径写入源码树 | `clr/**` 与其他项目源码 |
| binding declaration | `ECMAScript.*` binding | 收集 dependency key、版本、integrity、specifier 与显式样式入口 | 根 `package.json` 的 `dependencies` 与源码 bare import |
| 应用本地资源 | 最终宿主 / `ModuleCatalog` | 按源码 import 或 URL 的目标路径写出 | 项目内 CSS、worker、字体、图片和 wasm |

现有 metadata 中的 `embedded-mjs` 对应 ECMAScript 源码 carrier，写入项目源码树。npm/JSR binding 的运行时代码由依赖恢复提供。`manifest.json`、inventory 和 source map 保存声明与诊断证据，项目运行时解析使用标准 `package.json` 和源码 import。

## 固定数据流

1. **收集**：最终宿主提供根程序集、参与程序集、应用 Entries、`ModuleCatalog`、源码 carrier 和 binding declarations。
2. **准备 staging**：Emit 在输出同卷创建 staging 项目根，保持与最终项目相同的路径结构并复用一致的锁文件。
3. **写源码**：写入 `clr/**`、其他 ECMAScript 源码、编译生成模块、source map 与本地资源。
4. **归一化 import**：项目内模块按最终文件位置写 `./` 或 `../`；npm/JSR import 保留绑定声明中的 bare specifier。
5. **生成入口**：生成 `entry.mjs`；存在 SSR 时生成 `ssr-entry.mjs`。每个入口只连接应用实际 roots。
6. **生成项目声明**：合并实际 package imports，写根 `package.json` 的标准字段、`dependencies` 和 `exports`。
7. **恢复与检查**：调用 Deno 2.9.7 生成或验证 `deno.lock` 和 `node_modules`，再以 frozen lock 检查可见入口。
8. **消费与提交**：Release 交给 NetPack 构建 `dist/`；SSR 交给 DenoHost 使用同一项目；所有检查通过后原子替换目标 `jazor/`。

## 源码与入口路径

源码 carrier 的路径在项目内保持稳定：

```text
src/ECMAScript/clr/**         -> jazor/clr/**
ECMAScriptModule.RelativePath -> jazor/<RelativePath>
```

`ModuleCatalog` 中的相对依赖在写出时根据 importer 的最终路径计算。package import 直接由 Deno 恢复的 `node_modules` 解析。入口导出保持 C# binding 声明的 named/default 形式。

应用可以拥有浏览器、SSR 和 HMR 等可见入口；这些入口共享一棵项目源码与 package 依赖图。Debug 输出的 `jazor-manifest.json` 和 source map 用于诊断与开发服务，package resolution 始终读取根 `package.json`。

## Deno 项目恢复

Deno 版本固定为 2.9.7，由 `DenoHost.Runtime` 包提供可执行文件。Emit 根据根项目的 dependency identity 选择标准命令。

首次生成或依赖 identity 变化：

```text
deno install --package-json --node-modules-dir=auto --frozen=false
```

锁文件与 `package.json` 对齐：

```text
deno ci
```

恢复后的入口检查：

```text
deno check --node-modules-dir=manual --no-remote --frozen-lockfile entry.mjs
```

启用 SSR 时对 `ssr-entry.mjs` 追加相同的 frozen check。`deno.lock` 是 Deno 的冻结依据；完整 npm `package-lock.json` 作为生态兼容输入随项目保留，并在生成时验证根依赖一致。

## 标准消费者

### NetPack

NetPack 接收项目根、可见 entry、构建模式与输出目录，并沿普通 ESM 图读取：

```text
entry.mjs
  -> 项目源码
  -> npm/JSR 公开入口
  -> 包内实现与共享 helper
  -> peer / optional dependency
  -> side-effect module
  -> CSS / worker / static asset
```

NetPack 使用上游 `package.json` 的 `exports`、conditions 和 `sideEffects`。公开子路径与 named export 提供细粒度入口；组件内部 import、共享模块和必要副作用在可达图中保持，未到达的导出与资源由构建器裁剪。

### DenoHost

DenoHost 以 `jazor/` 为工作目录，加载 `ssr-entry.mjs`，复用 Emit 已恢复的 `node_modules` 与 `deno.lock`。SSR 与浏览器构建使用同一 package identity、源码路径和条件选择。

### HMR

HMR 读取当前项目的源码入口、source map 和变更文件。更新消息引用项目内稳定路径；依赖恢复和包解析继续由同一 `package.json`、`node_modules` 与 lock 提供。

## 事务与确定性

Emit 在同卷 staging 中完成写入、恢复、检查和构建。成功后以目录替换提交；发生失败、取消或构建错误时保留上一份完整 `jazor/`。相同源码、SDK、Deno、NetPack、依赖 identity 和构建选项产生稳定的源码路径、入口、项目声明、lock 与 bundle 元数据。

项目内重复模块按稳定路径和内容 hash 去重。dependency key、版本或入口冲突在 Deno restore 前报告；缺失源码、无法解析的相对 import、Deno frozen check 失败或 NetPack 失败会阻止提交。

## 配置与交付

最终宿主配置 `JazorMode` 与 `JazorDir`，Emit 在 MSBuild 中运行：

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)/jazor/</JazorDir>
</PropertyGroup>
```

- `debug`：保留项目源码、source map、诊断 manifest 和 import map，供开发与 HMR 使用。
- `release`：从同一项目根调用 NetPack，输出 `dist/` 与 bundle metafile。
- `JazorSSR=true`：在同一次项目生成与恢复中增加 `ssr-entry.mjs`，供 DenoHost 直接消费。

## 验收证据

- 生成的 `jazor/` 可由 Deno 2.9.7 在 `--no-remote` 与 frozen lock 下检查。
- `package.json` 的每个 dependency key 都对应一个精确 binding identity，源码中的每个 bare import 都能解析。
- NetPack metafile 展示入口可达的 JS、CSS、worker 和 static 资源；组件内部依赖保持完整，共享模块只出现一个实例。
- DenoHost SSR 使用同一 `node_modules` 与 `deno.lock`，浏览器与 SSR 的 package identity 一致。
- 重复 MSBuild 生成得到稳定项目文件、路径、source map 和 bundle 输出。
