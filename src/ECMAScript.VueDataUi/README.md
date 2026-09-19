# ECMAScript.VueDataUi

> `vue-data-ui` 3.23.4 的 RazorVue 强类型 binding，完整覆盖上游 71 个公开 `vue-ui-*` entry。每个 Razor component 指向单独的 upstream ESM entry，因此不会因为使用一个图表而导入整个 chart bundle。

本包属于 JS resource library：`manifest.json` 与 `inventory.json` 记录锁定的 `vue-data-ui`
npm package、组件入口、全局样式和依赖元数据；Emit 生成标准 `jazor/package.json`，Deno 将
运行时恢复到 `jazor/node_modules`。C# 程序集只提供映射和 RazorVue authoring contract。
消费方生成的组件模块进入消费程序集的 `Jazor.Generated.ModuleCatalog`。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="1.0.0-preview.3" />
  <PackageReference Include="Jazor.Vue" Version="1.0.0-preview.3" PrivateAssets="all" />
  <PackageReference Include="ECMAScript.VueDataUi" Version="1.0.0-preview.3" />
</ItemGroup>
```

`Jazor` 与所有 `ECMAScript.*` package 应使用同一版本。`Jazor.Vue` 是 Razor SDK 项目的显式 RazorVue opt-in；图表包的 `buildTransitive` target 只负责注册资源 manifest locator。应用无需再安装 npm package、配置 CDN 或手工复制 `style.css`。

## Razor 使用

> 下列 `Vd*` 示例适用于 1.0.0-preview.3 及后续版本；从 preview.2 或更早版本升级时需迁移旧前缀，详见 [CHANGELOG](../../CHANGELOG.md)。

组件和配套公开类型统一使用 `Vd` 前缀，例如 `VdDonut`、`VdDonutConfig`、`VdDonutDatasetItem` 和 `VdTheme`。
从旧版迁移时，将 Razor 标签与 C# 类型引用中的 `VueUi*` 改为 `Vd*`，公共基础类型中的 `VueDataUi*` 也改为 `Vd*`
（例如 `VueDataUiConfig` → `VdConfig`、`VueDataUiCellValue` → `VdCellValue`）。包名与命名空间仍为 `ECMAScript.VueDataUi`；
npm 导入路径和 `VueUi*` 导出名保持上游契约，无需修改 JavaScript 依赖。

将图表放进有确定高度的容器。`responsive: true` 会读取容器尺寸，只有宽度没有高度时上游 SVG/chart viewport 没有稳定的可用尺寸。

```razor
@using ECMAScript.VueDataUi

<section style="height: 280px">
    <VdDonut Dataset="@revenue" Config="@donutConfig" />
</section>

@code {
    private readonly VdDonutDatasetItem[] revenue =
    [
        new() { Name = "Subscription", Values = [68], Color = "#0f766e" },
        new() { Name = "Usage", Values = [32], Color = "#2563eb" }
    ];

    private readonly VdDonutConfig donutConfig = new()
    {
        Responsive = true,
        Theme = VdTheme.Light,
        UseCssAnimation = true
    };
}
```

Dataset 与稳定 config 字段都有具体 C# 类型。对于 upstream 仍在演进的 nested option，可从 `VdConfig` / `VdDatasetItem` 派生强类型 record，或使用其 `VueDictionary<VueValue>` collection initializer；不要退回到 `object` / `object[]`。

`VdCandlestick` 的上游 dataset row 是位置数组，而 C# tuple 在 Jazor 中故意保留为命名 object。请通过 `VdCandlestickData.Ohlc(timestamp, open, high, low, close, volume)` 创建每一行，它会直接 lower 为所需的六项 JavaScript array。相同原因下，`VdAgePyramidData.Row(year, rank, left, right)` 和 `VdFlowData.Link(from, to, value)` 分别产生上游要求的 4/3 项 array。`VdTableSparkline` 的 `Config` 依照上游 contract 为 required Razor parameter。

## 按需运行时

- `VdDonut` emits `import { VueUiDonut } from "vue-data-ui/vue-ui-donut"`。
- Emit 会从这个 entry 递归物化相对 ESM closure；无关的 chart chunk 不会被复制到应用输出。
- 需要导出能力的图表会通过本包 manifest 的 package dependency 选择本地、无 bare import 的 `jspdf` browser ESM entry。该 entry 已包含 jsPDF 的浏览器依赖闭包；`jspdf`、其 bundled notices 与 `vue-data-ui` license 随所需运行时一同交付。
- package root `vue-data-ui` 是聚合入口，binding 故意不使用它。

## 完整组件目录

文档来源需要区分：`vue-data-ui` 3.23.4 npm 包没有 JetBrains `web-types.json`，组件 `.d.ts` 的 `Props` 类型也没有逐组件 JSDoc；因此本包不会把本地概括说明伪装成上游原始注释。组件类型和说明以锁定 npm tarball 中的类型声明、`README.md` 与上游文档为依据；当前 C# XML 注释只在上游有对应文字时保留原文，其余组件说明属于 binding contract 说明。后续若上游发布结构化组件文档，应将该快照接入生成器并补齐逐 prop 原文。

当前绑定与 npm package 的 `vue-data-ui/vue-ui-*` 入口一一对应，共 71 个公开 Razor component：

- 基础与 Cartesian：`VdXy`、`VdXyCanvas`、`VdVerticalBar`、`VdHorizontalBar`、`Vd3dBar`、`VdBump`、`VdCandlestick`、`VdDumbbell`、`VdHeatmap`、`VdHistoryPlot`、`VdRidgeline`、`VdScatter`、`VdSparkline`、`VdSparkTrend`、`VdStackbar`、`VdStackline`、`VdStripPlot`。
- 比例、层级与关系：`VdBullet`、`VdChestnut`、`VdChord`、`VdCirclePack`、`VdDonut`、`VdDonutEvolution`、`VdFunnel`、`VdGalaxy`、`VdGauge`、`VdNestedDonuts`、`VdOnion`、`VdRings`、`VdTreemap`、`VdWaffle`、`VdWheel`、`VdWordCloud`。
- 专项图表：`VdAgePyramid`、`VdDag`、`VdFlow`、`VdGeo`、`VdHill`、`VdMoodRadar`、`VdMolecule`、`VdParallelCoordinatePlot`、`VdQuadrant`、`VdRadar`、`VdRelationCircle`、`VdWorld`。
- 指标、表格与 compact visual：`VdCarouselTable`、`VdDigits`、`VdGizmo`、`VdKpi`、`VdQuickChart`、`VdRating`、`VdSmiley`、`VdSparkgauge`、`VdSparkbar`、`VdSparkHistogram`、`VdSparkStackbar`、`VdTable`、`VdTableHeatmap`、`VdTableSparkline`、`VdThermometer`、`VdTiremarks`。
- Layout、overlay 与 SVG utility：`VdAccordion`、`VdAnnotator`、`VdCursor`、`VdDashboard`、`VdIcon`、`VdMiniLoader`、`VdPattern`、`VdPatternSeed`、`VdSkeleton`、`VdTimer`。

每个 chart 都有专用 dataset/config 根类型；稳定字段使用具体 C# properties，图表仍在迭代的 nested options 使用 `VdConfig` / `VdDatasetItem` 的 `VueDictionary<VueValue>` 扩展。`VdAnnotator`、`VdGeo` 和 `VdWorld` 是上游明确允许省略 dataset 的少数例外；其余 dataset 均保持 Razor required parameter。不要使用 package root `vue-data-ui` 绕过按需加载边界。

## 验证与示例

- [Vue Data UI dashboard sample](../../samples/ECMAScript.VueDataUi.Dashboard/README.md)
- `dotnet run --file scripts/csharp/test-dotnet.cs -- --project dataui`
- `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs`

## 相关文档

- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [示例总览](../../docs/03-guides/examples.md)
- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
