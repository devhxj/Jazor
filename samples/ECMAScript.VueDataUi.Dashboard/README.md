# ECMAScript.VueDataUi.Dashboard

> 独立 `ECMAScript.VueDataUi` NuGet consumer sample。它用官方 Razor Source Generator 编译 Donut、Gauge 和 Sparkline，而不是通过 project reference 绕过 package assets。

## 内容

- `RevenueDashboard.razor` 将三个 `VueUi*` chart 放到固定高度容器中，并以 `Responsive = true` 使用 upstream resize 行为。
- `RevenueDashboard.razor.cs` 只使用 `ECMAScript.VueDataUi` 的 typed dataset/config；没有 `object` 或 root `vue-data-ui` import。
- `build-local.cs` 先在隔离目录打包 `Jazor`、`Jazor.Vue` 和 `ECMAScript.VueDataUi`，随后以 package consumer 方式构建并检查生成的图表 import、style 和 selected ESM entries。

## 构建

在仓库根目录执行：

```bash
dotnet run --file samples/ECMAScript.VueDataUi.Dashboard/build-local.cs
```

临时 nupkg、NuGet cache 与生成 artifact 都在 `.tmp/`，不会写入 sample 的 tracked 源文件。

## 相关文档

- [ECMAScript.VueDataUi](../../src/ECMAScript.VueDataUi/README.md)
- [示例总览](../../docs/03-guides/examples.md)
