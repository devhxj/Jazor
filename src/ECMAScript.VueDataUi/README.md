# ECMAScript.VueDataUi

> `vue-data-ui` 3.23.4 的 RazorVue 强类型图表 binding。每个 Razor component 指向单独的 upstream ESM entry，因此不会因为使用一个图表而导入整个 chart bundle。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.14.0" />
  <PackageReference Include="Jazor.Vue" Version="0.14.0" PrivateAssets="all" />
  <PackageReference Include="ECMAScript.VueDataUi" Version="0.14.0" />
</ItemGroup>
```

`Jazor` 与所有 `ECMAScript.*` package 应使用同一版本。`Jazor.Vue` 是 Razor SDK 项目的显式 RazorVue opt-in；图表包的 `buildTransitive` target 只负责注册本地 manifest。应用无需再安装 npm package、配置 CDN 或手工复制 `style.css`。

## Razor 使用

将图表放进有确定高度的容器。`responsive: true` 会读取容器尺寸，只有宽度没有高度时上游 SVG/chart viewport 没有稳定的可用尺寸。

```razor
@using ECMAScript.VueDataUi

<section style="height: 280px">
    <VueUiDonut Dataset="@revenue" Config="@donutConfig" />
</section>

@code {
    private readonly VueUiDonutDatasetItem[] revenue =
    [
        new() { Name = "Subscription", Values = [68], Color = "#0f766e" },
        new() { Name = "Usage", Values = [32], Color = "#2563eb" }
    ];

    private readonly VueUiDonutConfig donutConfig = new()
    {
        Responsive = true,
        Theme = VueDataUiTheme.Light,
        UseCssAnimation = true
    };
}
```

Dataset 与稳定 config 字段都有具体 C# 类型。对于 upstream 仍在演进的 nested option，可从 `VueDataUiConfig` / `VueDataUiDatasetItem` 派生强类型 record，或使用其 `VueDictionary<VueValue>` collection initializer；不要退回到 `object` / `object[]`。

`VueUiCandlestick` 的上游 dataset row 是位置数组，而 C# tuple 在 Jazor 中故意保留为命名 object。请通过 `VueUiCandlestickData.Ohlc(timestamp, open, high, low, close, volume)` 创建每一行，它会直接 lower 为所需的六项 JavaScript array。`VueUiTableSparkline` 的 `Config` 依照上游 contract 为 required Razor parameter。

## 按需运行时

- `VueUiDonut` emits `import { VueUiDonut } from "vue-data-ui/vue-ui-donut"`。
- Emit 会从这个 entry 递归物化相对 ESM closure；无关的 chart chunk 不会被复制到应用输出。
- 需要导出能力的图表会使用本包 manifest 中本地的 `jspdf` entry。`jspdf` 和 `vue-data-ui` license 随所需运行时一同交付。
- package root `vue-data-ui` 是聚合入口，binding 故意不使用它。

## 当前组件目录

`VueUiXy`、`VueUiDonut`、`VueUiGauge`、`VueUiVerticalBar`、`VueUiHorizontalBar`、`VueUiStackbar`、`VueUiStackline`、`VueUiSparkline`、`VueUiSparkbar`、`VueUiSparkHistogram`、`VueUiRadar`、`VueUiWaffle`、`VueUiTreemap`、`VueUiHeatmap`、`VueUiScatter`、`VueUiFunnel`、`VueUiWordCloud`、`VueUiKpi`、`VueUiTable`、`VueUiTableHeatmap`、`VueUiTableSparkline`、`VueUiQuickChart`、`VueUiCandlestick`、`VueUiDumbbell`、`VueUiBullet`。

常用 dataset/config 已被强类型建模，包含 XY coordinate/series、table cell、gauge range、stack、spark、scatter、word cloud、quick chart、candlestick 和 bullet 数据形状。未列入目录的上游 visual component 尚未成为公开 binding surface；不要直接用 root bundle 绕过该边界。

## 验证与示例

- [Vue Data UI dashboard sample](../../samples/ECMAScript.VueDataUi.Dashboard/README.md)
- `dotnet run --file scripts/csharp/test-dotnet.cs -- --project dataui`
- `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs`

## 相关文档

- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [示例总览](../../docs/03-guides/examples.md)
- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
