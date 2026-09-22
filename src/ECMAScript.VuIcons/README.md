# ECMAScript.VuIcons（已废弃）

> 此 binding 已停止维护，不属于当前 Jazor 生态层的受支持组件库。新项目请使用
> `ECMAScript.Lucide`，它直接绑定 `lucide-vue-next` 的公开 ESM 入口并支持按需 tree shaking。
> 本页仅保留旧项目迁移所需的历史说明，不再新增 API、生成器输出或运行时 carrier。

> `vu-icons` 1.5.4 的 RazorVue 强类型 binding，完整覆盖上游 1,821 个 Vue 3 `Vu*` wrapper。已知图标走单图标 ESM entry；仅运行时选择名称时才加载完整 icon catalog。

本包属于 JS resource library：上游 raw Vue wrapper 不能直接作为浏览器 ESM 入口，因此包内
由生成器维护明确的 `embedded-mjs` carrier（`manifest.json + runtime/vu-icons/**`）；许可证等
附属文件由 manifest 显式声明。C# 程序集只提供映射和 RazorVue authoring contract。消费方生成的图标组件模块进入消费程序集的
`Jazor.Generated.ModuleCatalog`。

## 旧项目迁移

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="1.0.0-preview.3" />
  <PackageReference Include="Jazor.Vue" Version="1.0.0-preview.3" PrivateAssets="all" />
  <PackageReference Include="ECMAScript.VuIcons" Version="1.0.0-preview.3" />
</ItemGroup>
```

不要在新项目中添加此包。已有项目应迁移到 `ECMAScript.Lucide`；VuIcons 的生成器、测试 lane 和资源 carrier 不再是当前产品契约的一部分。

## 历史用法（仅供迁移参考）

已知图标使用生成的静态 component。`Size` 接受 number 或 string，`Color`、`ClassName` 与 `Spin` 对应上游 wrapper props。

```razor
@using ECMAScript.VuIcons

<button class="toolbar-action" type="button">
    <VuSearch Size="18" Color="#0f766e" ClassName="toolbar-icon" />
    Search
</button>
```

图标名称在运行时改变时，使用 `VuIcon`。`VuIconName` 是闭合 enum，避免把无效的上游 token 写成 string。

```razor
@using ECMAScript.VuIcons

<VuIcon Name="@currentIcon" Size="24" Color="#2563eb" Class="status-icon" Spin="@isLoading" />

@code {
    private VuIconName currentIcon = VuIconName.Activity;
    private bool isLoading;
}
```

`Name` 是唯一且必填的 Razor 参数。上游的 `Icon` alias 不在 binding 中公开：Razor 无法在保持 compile-time required contract 的同时表达“`Name` 或 `Icon` 二选一”。

## 历史运行时模型

- 静态 `<VuSearch />` emits `import { VuSearch } from "vu-icons/VuSearch";`。Emit 只物化该 SVG module、共享 renderer、样式和许可证，不复制其余 1,820 个图标或 `icons-data.js`。逻辑入口由 manifest 的 package export 映射到 `runtime/vu-icons/components/VuSearch.mjs`。
- 动态 `<VuIcon Name="@currentIcon" />` emits `import { VuIcon } from "vu-icons";`。由于名称在运行时才能确定，manifest 的 package entry 闭包会物化 `runtime/vu-icons/index.mjs`、共享 runtime 和完整 `icons-data.js` catalog。这是动态选择的必要成本。
- 两条路径均使用浏览器可执行的本地 `.mjs` bridge，而不是上游 raw `.vue` SFC；`Jazor.Emit` 无需额外 SFC compiler。

应优先使用静态 `Vu*` component。只有图标名称确实来自运行时状态、配置或服务端数据时，才使用动态 `VuIcon`。

## 历史生成流程

从 npm tarball 解压 `vu-icons` 后，使用其 package 根目录重新生成绑定：

```bash
dotnet run --file scripts/csharp/generate-vu-icons.cs -- --source .tmp/vu-icons/package --output src/ECMAScript.VuIcons
```

生成器从 upstream `icons.json`、`icons-data.js`、`web-types.json` 和 Vue 3 wrapper source 同时验证 component/name/data/documentation 的一一对应关系，并更新 C# descriptor、`VuIconName` 与本地 browser bridge。生成的每个静态组件摘要和公共 prop 摘要保留 `web-types.json` 原文；按需加载说明位于 `<remarks>`。更新后运行：

旧版本上游 `vu-icons` 为 MIT License；该说明只用于既有项目的许可证核对。
