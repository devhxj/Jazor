# Jazor.Emit

`Jazor.Emit` 是宿主构建边界的唯一物化器。它不参与 C# 或 Razor 语义 lowering，只读取
已确定的类库 carrier 和 package metadata，并把选定的资源闭包写入最终 `JazorDir`：

| carrier | 内容 | 来源 |
| --- | --- | --- |
| JS resource library | package metadata + ESM/CSS/worker/static 资源；资源来自 npm、JSR 或明确声明的 embedded carrier | Vue、Vuetify、Pinia、`ECMAScript` 等已有 JavaScript 资源包 |
| 纯 Jazor library | 程序集内 `Jazor.Generated.ModuleCatalog`（ECMAScriptCode） | `Jazor.Compiler`/RazorVue 从 C# 生成的模块 |

这两种 carrier 是并列的一等输入。Emit 可以在内存中把它们归一化为资源记录和 package graph
来做去重、依赖闭包和冲突校验，并为本次 profile 生成标准 `jazor/package.json`、`package-lock.json`
以及 embedded package 投影；Emit 在 MSBuild staging 中调用 Deno 生成 `deno.lock` 并恢复 `node_modules`。

## 唯一调用边界

MSBuild 只收集：

- 根程序集和引用程序集路径；
- JS resource 包传递下来的 package metadata locator；
- `JazorMode`、`JazorDir`、source root 和 SSR 选项。

随后每个构建 profile 只调用一次默认 Emit 入口。Emit 在目标目录同卷 staging 中完成：

1. 读取所有 `Jazor.Generated.ModuleCatalog`；
2. 读取并校验显式 package metadata，收集 npm/JSR identity 与 embedded carrier；
3. 按模块/package/resource 声明解析依赖闭包、版本、路径和 hash；
4. 生成模块、source map、资源、package project、import map 和应用 manifest；
5. Release 时在同一请求中运行 Netpack bundle，SSR 时生成 `ssr/` profile；
6. 所有检查成功后整体提交到 `JazorDir`。

任何步骤失败、取消或冲突，都不会替换上一份完整输出。staging 目录是 Emit 的私有实现
细节，不是类库格式、MSBuild item 或可被下一次构建消费的产物。

## CLI

默认入口同时覆盖 Debug、Release 和可选 SSR：

```text
dotnet Jazor.Emit.dll \
  --root <root.dll> \
  --assembly <reference.dll> \
  --out <jazor-dir> \
  --write-manifest <jazor-dir>/jazor-manifest.json \
  --mode debug|release \
  --source-root <project-root> \
  --ssr true|false \
  --library-manifest <package>/manifest.json
```

`--assembly` 和 `--library-manifest` 可以重复。路径、版本、资源类型和依赖由 package metadata
显式声明；Emit 使用 metadata 指向的 carrier 和 package identity 建立 graph，并保持
相对模块、CSS、worker、static 和 license 资源的 owner 关系。第三方运行时代码保持在恢复的
`node_modules`，只有 embedded carrier 的选中文件进入本地 package projection。

`toolchain` 和 `manifest materialize` 不再是构建入口。Netpack、资源 materializer 和
import-map writer 仍作为 Emit 内部实现参与同一事务，不能被 MSBuild 分段调用，也不能产生
公开 intermediate carrier。

## 输出

Debug 输出生成模块、source map、资源 vendor、`jazor-manifest.json`、`importmap.json`、
`ssr-importmap.json` 和资源 `manifest.json`。Release 在同一目录增加 bundle 及其 source map；
启用 `--ssr` 时在 `ssr/` 下生成独立的 SSR 模块图和资源闭包。输出文件属于宿主 profile，
不改变上游类库 carrier 的形式。

## 验证

```text
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --no-restore
```
