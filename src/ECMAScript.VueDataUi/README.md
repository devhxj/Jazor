# ECMAScript.VueDataUi

> `vue-data-ui` 3.23.4 的 RazorVue 强类型 binding，完整覆盖上游 71 个公开 `vue-ui-*` entry。每个 Razor component 指向单独的 upstream ESM entry，因此不会因为使用一个图表而导入整个 chart bundle。

本包属于 JS resource library：`vue-data-ui` 的已有 ESM 和 CSS 位于包内
`manifest.json + dist/**`，许可证等附属文件由 manifest 显式声明；C# 程序集只提供映射和
RazorVue authoring contract。消费方生成的组件模块进入消费程序集的
`Jazor.Generated.ModuleCatalog`。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.2" />
  <PackageReference Include="Jazor.Vue" Version="0.26.2" PrivateAssets="all" />
  <PackageReference Include="ECMAScript.VueDataUi" Version="0.26.2" />
</ItemGroup>
```

`Jazor` 与所有 `ECMAScript.*` package 应使用同一版本。`Jazor.Vue` 是 Razor SDK 项目的显式 RazorVue opt-in；图表包的 `buildTransitive` target 只负责注册资源 manifest locator。应用无需再安装 npm package、配置 CDN 或手工复制 `style.css`。

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

`VueUiCandlestick` 的上游 dataset row 是位置数组，而 C# tuple 在 Jazor 中故意保留为命名 object。请通过 `VueUiCandlestickData.Ohlc(timestamp, open, high, low, close, volume)` 创建每一行，它会直接 lower 为所需的六项 JavaScript array。相同原因下，`VueUiAgePyramidData.Row(year, rank, left, right)` 和 `VueUiFlowData.Link(from, to, value)` 分别产生上游要求的 4/3 项 array。`VueUiTableSparkline` 的 `Config` 依照上游 contract 为 required Razor parameter。

## 按需运行时

- `VueUiDonut` emits `import { VueUiDonut } from "vue-data-ui/vue-ui-donut"`。
- Emit 会从这个 entry 递归物化相对 ESM closure；无关的 chart chunk 不会被复制到应用输出。
- 需要导出能力的图表会通过本包 manifest 的 package dependency 选择本地、无 bare import 的 `jspdf` browser ESM entry。该 entry 已包含 jsPDF 的浏览器依赖闭包；`jspdf`、其 bundled notices 与 `vue-data-ui` license 随所需运行时一同交付。
- package root `vue-data-ui` 是聚合入口，binding 故意不使用它。

## 完整组件目录

当前包与 `dist/components/vue-ui-*.js` 一一对应，共 71 个公开 Razor component：

- 基础与 Cartesian：`VueUiXy`、`VueUiXyCanvas`、`VueUiVerticalBar`、`VueUiHorizontalBar`、`VueUi3dBar`、`VueUiBump`、`VueUiCandlestick`、`VueUiDumbbell`、`VueUiHeatmap`、`VueUiHistoryPlot`、`VueUiRidgeline`、`VueUiScatter`、`VueUiSparkline`、`VueUiSparkTrend`、`VueUiStackbar`、`VueUiStackline`、`VueUiStripPlot`。
- 比例、层级与关系：`VueUiBullet`、`VueUiChestnut`、`VueUiChord`、`VueUiCirclePack`、`VueUiDonut`、`VueUiDonutEvolution`、`VueUiFunnel`、`VueUiGalaxy`、`VueUiGauge`、`VueUiNestedDonuts`、`VueUiOnion`、`VueUiRings`、`VueUiTreemap`、`VueUiWaffle`、`VueUiWheel`、`VueUiWordCloud`。
- 专项图表：`VueUiAgePyramid`、`VueUiDag`、`VueUiFlow`、`VueUiGeo`、`VueUiHill`、`VueUiMoodRadar`、`VueUiMolecule`、`VueUiParallelCoordinatePlot`、`VueUiQuadrant`、`VueUiRadar`、`VueUiRelationCircle`、`VueUiWorld`。
- 指标、表格与 compact visual：`VueUiCarouselTable`、`VueUiDigits`、`VueUiGizmo`、`VueUiKpi`、`VueUiQuickChart`、`VueUiRating`、`VueUiSmiley`、`VueUiSparkgauge`、`VueUiSparkbar`、`VueUiSparkHistogram`、`VueUiSparkStackbar`、`VueUiTable`、`VueUiTableHeatmap`、`VueUiTableSparkline`、`VueUiThermometer`、`VueUiTiremarks`。
- Layout、overlay 与 SVG utility：`VueUiAccordion`、`VueUiAnnotator`、`VueUiCursor`、`VueUiDashboard`、`VueUiIcon`、`VueUiMiniLoader`、`VueUiPattern`、`VueUiPatternSeed`、`VueUiSkeleton`、`VueUiTimer`。

每个 chart 都有专用 dataset/config 根类型；稳定字段使用具体 C# properties，图表仍在迭代的 nested options 使用 `VueDataUiConfig` / `VueDataUiDatasetItem` 的 `VueDictionary<VueValue>` 扩展。`VueUiAnnotator`、`VueUiGeo` 和 `VueUiWorld` 是上游明确允许省略 dataset 的少数例外；其余 dataset 均保持 Razor required parameter。不要使用 package root `vue-data-ui` 绕过按需加载边界。

## 验证与示例

- [Vue Data UI dashboard sample](../../samples/ECMAScript.VueDataUi.Dashboard/README.md)
- `dotnet run --file scripts/csharp/test-dotnet.cs -- --project dataui`
- `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs`

## 相关文档

- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [示例总览](../../docs/03-guides/examples.md)
- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
