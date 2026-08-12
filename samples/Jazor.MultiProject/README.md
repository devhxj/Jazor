# Jazor.MultiProject

> 定位：多项目 C# -> ECMAScript 模块发现与输出模式的最小示例。

该示例展示推荐的 SDK 布局：共享契约项目声明模块，功能项目引用共享契约，最终 host 统一发射自身及其引用项目中标记为 `[ECMAScriptModule]` 的模块。

## 结构

- `Sample.Contracts`：共享模块库。
- `Sample.Features`：声明 ECMAScript module 并引用共享契约的类库。
- `Sample.Host`：最终应用 host，负责生成模块和选择输出模式。

## 构建

已配置发布包源时，直接构建 host：

```bash
dotnet build samples/Jazor.MultiProject/Sample.Host/Sample.Host.csproj
```

生成模块位于 `Sample.Host/jazor/`；Web 宿主可将其挂载到浏览器 `/jazor/*`，发布时复制到 `<publish>/jazor/`。

使用当前仓库的本地包进行验证：

```bash
dotnet run --file samples/Jazor.MultiProject/build-local.cs
```

## 输出模式

`JazorMode=debug` 输出可检查的模块；`JazorMode=release` 通过 Netpack 生成浏览器 bundle。构建 release bundle：

```bash
dotnet run --file samples/Jazor.MultiProject/build-local.cs -- --bundle
```

本示例的 bundle 位于 `Sample.Host/jazor/bundle.js`，并导出 host 的 `boot` 入口。

## 相关文档

- [快速开始](../../docs/03-guides/quick-start.md)
- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
