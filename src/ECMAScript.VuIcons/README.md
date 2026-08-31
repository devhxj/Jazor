# ECMAScript.VuIcons

> `vu-icons` 1.5.4 的 RazorVue 强类型 binding，完整覆盖上游 1,821 个 Vue 3 `Vu*` wrapper。已知图标走单图标 ESM entry；仅运行时选择名称时才加载完整 icon catalog。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.0" />
  <PackageReference Include="Jazor.Vue" Version="0.26.0" PrivateAssets="all" />
  <PackageReference Include="ECMAScript.VuIcons" Version="0.26.0" />
</ItemGroup>
```

`Jazor` 与所有 `ECMAScript.*` package 应使用同一版本。包的 `buildTransitive` target 只注册资源 manifest locator；应用不需要 npm、CDN 或手工复制 CSS。

## Razor 使用

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

## 按需运行时

- 静态 `<VuSearch />` emits `import { VuSearch } from "vu-icons/VuSearch";`。Emit 只物化该 SVG module、共享 renderer、样式和许可证，不复制其余 1,820 个图标或 `icons-data.js`。
- 动态 `<VuIcon Name="@currentIcon" />` emits `import { VuIcon } from "vu-icons";`。由于名称在运行时才能确定，manifest 的 package entry 闭包会物化完整 `icons-data.js` catalog。这是动态选择的必要成本。
- 两条路径均使用浏览器可执行的本地 `.mjs` bridge，而不是上游 raw `.vue` SFC；`Jazor.Emit` 无需额外 SFC compiler。

应优先使用静态 `Vu*` component。只有图标名称确实来自运行时状态、配置或服务端数据时，才使用动态 `VuIcon`。

## 更新上游图标集

从 npm tarball 解压 `vu-icons` 后，使用其 package 根目录重新生成绑定：

```bash
dotnet run --file scripts/csharp/generate-vu-icons.cs -- --source .tmp/vu-icons/package --output src/ECMAScript.VuIcons
```

生成器从 upstream `icons.json`、`icons-data.js` 和 Vue 3 wrapper source 同时验证 component/name/data 的一一对应关系，并更新 C# descriptor、`VuIconName` 与本地 browser bridge。更新后运行：

```bash
dotnet run --file scripts/csharp/test-dotnet.cs -- --project vu-icons
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter RazorSgVuIconsLibraryComponentTests
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter Materialize_ProductionVuIcons
```

上游 `vu-icons` 为 MIT License；发布包随 runtime artifact 保留其 license 文本。
