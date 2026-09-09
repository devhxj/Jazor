# Jazor.Admin

> 定位：面向 Razor-to-Vue 应用的管理壳契约与原生组件库。

`Jazor.Admin` 是库项目，提供可复用的应用框架、导航模型和 RazorVue 管理壳组件。`samples/JazorAdmin` 是消费该库的生产级管理参考应用；示例中的 TDesign 组合、业务页面、认证和部署策略不构成此包的公共 API。

本包是纯 Jazor 类库：RazorVue 组件由 Jazor 编译到程序集内的
`Jazor.Generated.ModuleCatalog`（`ECMAScriptCode`）。它不把组件生成结果伪装成外部资源包；
最终宿主通过 Emit 与所声明的 JS resource package manifest 一起按依赖闭包物化。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.34.0" />
  <PackageReference Include="Jazor.Vue" Version="0.34.0" PrivateAssets="all" />
  <PackageReference Include="Jazor.Admin" Version="0.34.0" />
</ItemGroup>
```

`Jazor.Vue` 显式启用 Razor-to-Vue 编译路径；`Jazor.Admin` 的传递依赖提供 `ECMAScript.Style` 和 `ECMAScript.VueRoute` 的基础契约。所有 Jazor 包应使用相同版本。

## 职责

- 提供导航、面包屑、页面操作、布局模式和应用级显示状态等强类型模型。
- 提供 `JFrame`、`JLayout`、`JSidebar`、`JHeader`、`JPage` 和 `JBreadcrumb` 等原生 RazorVue 管理壳组件。
- `JSidebar` 支持通过 `IconTemplate`（`RenderFragment<AdminNavItem>`）注入应用侧图标渲染；未提供时仅输出带 `data-icon` 的占位 span，不绑定任何第三方图标实现。
- `JSidebar` 和 `JLayout` 为导航 landmark 提供可本地化的 `NavigationLabel`，分支切换按钮提供 `ExpandLabel` / `CollapseLabel`，并在当前路由项上输出 `aria-current="page"`。
- `JLayout` 在窄视口（≤760px）下将侧栏切换为 overlay 抽屉：backdrop 点击与导航选中后关闭，桌面视口维持原折叠列契约。
- 通过 `IVueContainerComponent` 与 `IVueContainerImplementation<TContainer>` 支持应用在保持公共容器契约的前提下替换具体实现。
- 以 `Href` 表示普通链接，以 `RouteTarget` 表示强类型 Vue Router 导航；路由目标优先于普通链接。
- 空导航目录会安全地产生空路由记录；`AdminRouteCatalog.Resolve` 在没有候选路由时返回带 fallback key 的定义，不会因访问 `routes[0]` 抛出异常。

## 组件实现约定

- 组件的静态 HTML 结构优先放在 `.razor` 文件中，参数、派生状态和事件处理放在同名 `.razor.cs` 文件中。
- `JHeader`、`JFrame`、`JBreadcrumb`、`JAction` 和 `JSidebar` 的外层结构已经遵循这一约定。
- 递归菜单项、复杂 `RenderFragment` 插槽和 RazorVue direct render 尚未支持的形状可以保留在 `.razor.cs`；迁移必须以生成诊断和最终 `.mjs` 链接结果通过为准。
- 不要为了追求模板化而引入中间 JavaScript 标记协议；组件应直接生成最终 Vue render-function 结构。

当前文件化边界如下：

| 组件 | 模板文件 | C# 文件保留内容 |
|------|----------|----------------|
| `JFrame`、`JHeader`、`JBreadcrumb`、`JAction` | 完整静态结构 | 参数、派生状态与生命周期 |
| `JPage` | 完整页面结构 | 标题/操作过滤、区域状态与插槽判定 |
| `JSidebar` | 导航壳结构 | 递归菜单项、路由目标解析与展开状态 |
| `JLayout` | 暂无独立模板 | 多插槽组合、动态导航注入、移动抽屉事件协议 |

`JLayout` 的渲染树同时承载桌面列布局和移动 overlay 抽屉，并需要在同一轮渲染中稳定快照多个插槽；待 RazorVue 支持等价的插槽表达后再继续拆分。

## 命名约定

- `Jazor.Admin` 的公共组件统一使用简短的 `J` 前缀，例如 `JLayout`、`JSidebar` 和 `JPage`，以便在同时使用 TDesign、Vuetify 或其他组件库时避免标签名冲突。
- 本次版本是破坏性命名迁移：旧的 `AdminLayout`、`SidebarMenu`、`PageContainer` 等组件名称不再导出，调用方必须使用新的 `JLayout`、`JSidebar`、`JPage` 等名称。
- `J` 前缀只用于公共组件类型；`AdminNavItem`、`AdminRouteDefinition` 等模型和内部 helper 保持现有命名，避免无意义的缩写扩散。

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
