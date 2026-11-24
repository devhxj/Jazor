# Jazor.Emit

Jazor.Emit 将 C# 编译器写入程序集的 `Jazor.Generated.ModuleCatalog` 交付给 JavaScript 项目。模块内容、相对路径、依赖和 source map 来自 catalog。运行时源码和静态资源来自显式声明的资源清单。

## 类库与宿主

普通 C# 类库构建时将 ESM 和 source map 放入 DLL 的 `ModuleCatalog`。最终可执行项目或 Web 宿主直接引用 Jazor，由 MSBuild 根据宿主的 `JazorMode` 调用 Emit。以下配置在宿主的默认目录 `jazor/` 写出开发项目。

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
</PropertyGroup>
```

项目留空或显式设置 `none` 时，MSBuild 跳过 Emit。`JazorDir` 只指定输出位置。`debug` 写出模块、map、资源、标准入口及 `package.json`，并在进程内恢复依赖。`release` 还调用 Vite 生成生产资源。启用 Emit 的宿主在发布时生成生产输出。

## 直接调用与 Jolt 构建

直接调用 Emit CLI 时，省略 `--mode` 默认执行开发构建。`debug` 和 `development` 选择开发构建，`release` 和 `production` 选择生产构建。用 `--root`、`--out` 和 `--write-manifest` 分别指定根程序集、前端输出目录和增量状态文件。

用 `--assembly` 指定可供解析依赖的程序集，`--module-assembly` 可添加额外的模块根。`--source-root` 用于定位 catalog 引用的本地资源。`--write-manifest` 指向生成目录以外的构建状态文件。内容不变的模块和 map 保留时间戳。

`--library-manifest` 提供运行时资源声明，`--browser-entry` 指定浏览器入口，`--ssr true` 启用服务端渲染。Windows 上 MSBuild 按构建机 RID 选择 `CETCompat=false` 的 Emit apphost。[工具链指南](../../docs/03-guides/toolchain-migration.md) · [宿主项目配置](../../docs/03-guides/installation-and-configuration.md)

Jolt 应用使用 `jolt bundle <project>` 从自己的入口、锁文件和 Vite 配置打包。引用 `ECMAScript.Vue` 的 C# 项目由该包向编译器传递适合 `reactive` 的字段存储策略。

## 验证

```sh
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --no-restore
```
