# Jazor.Admin

> 定位：面向 Razor-to-Vue 应用的管理壳契约与原生组件库。

`Jazor.Admin` 是库项目，提供可复用的应用框架、导航模型和 RazorVue 管理壳组件。`samples/JazorAdmin` 是消费该库的生产级管理参考应用；示例中的 TDesign 组合、业务页面、认证和部署策略不构成此包的公共 API。

本包是纯 Jazor 类库：RazorVue 组件由 Jazor 编译到程序集内的
`Jazor.Generated.ModuleCatalog`（`ECMAScriptCode`）。它不把组件生成结果伪装成外部资源包；
最终宿主通过 Emit 与所声明的 JS resource package manifest 一起按依赖闭包物化。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.2" />
  <PackageReference Include="Jazor.Vue" Version="0.26.2" PrivateAssets="all" />
  <PackageReference Include="Jazor.Admin" Version="0.26.2" />
</ItemGroup>
```

`Jazor.Vue` 显式启用 Razor-to-Vue 编译路径；`Jazor.Admin` 的传递依赖提供 `ECMAScript.Style` 和 `ECMAScript.VueRoute` 的基础契约。所有 Jazor 包应使用相同版本。

## 职责

- 提供导航、面包屑、页面操作、布局模式和应用级显示状态等强类型模型。
- 提供 `ApplicationFrame`、`AdminLayout`、`SidebarMenu`、`HeaderBar`、`PageContainer` 和 `AdminBreadcrumb` 等原生 RazorVue 管理壳组件。
- `SidebarMenu` 支持通过 `IconTemplate`（`RenderFragment<AdminNavItem>`）注入应用侧图标渲染；未提供时仅输出带 `data-icon` 的占位 span，不绑定任何第三方图标实现。
- `AdminLayout` 在窄视口（≤760px）下将侧栏切换为 overlay 抽屉：backdrop 点击与导航选中后关闭，桌面视口维持原折叠列契约。
- 通过 `IVueContainerComponent` 与 `IVueContainerImplementation<TContainer>` 支持应用在保持公共容器契约的前提下替换具体实现。
- 以 `Href` 表示普通链接，以 `RouteTarget` 表示强类型 Vue Router 导航；路由目标优先于普通链接。

## 边界

- 本包只拥有应用框架与组件契约，不提供表格、表单、通知、鉴权页面或具体业务功能。
- 主题、语言、灰度等状态由应用控制；本包不持久化用户偏好，也不规定本地化策略。
- UI 库适配属于应用或专用 binding 包的职责。`Jazor.Admin` 不泄漏第三方组件库的 props。
- 替换容器实现时必须同时满足 props、events 和 slots 契约；组件选择由程序集级 `[VueInject]` 完成。

## 验证

```bash
dotnet build src/Jazor.Admin/Jazor.Admin.csproj
dotnet run --file samples/JazorAdmin/verify-smoke.cs -- --configuration Release
```

第二个命令验证示例应用对库、Razor SG 产物和容器替换的真实消费路径。

## 相关文档

- [JazorAdmin 示例](../../samples/JazorAdmin/README.md)
- [管理壳架构](../../docs/02-architecture/admin-shell.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
