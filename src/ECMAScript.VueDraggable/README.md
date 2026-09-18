# ECMAScript.VueDraggable

vue-draggable-plus 的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游版本、`manifest.json`（schema 2）、`dist/` 资源、许可证与 inventory。

Strongly typed C# bindings for vue-draggable-plus, shipped as a Jazor JS resource library with a locked upstream version, package-local `manifest.json` (schema 2), `dist/` runtime assets, license, and inventory.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| npm 包 | `vue-draggable-plus` |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 许可证 | MIT（`licenses/vue-draggable-plus-LICENSE`） |
| 入口 | `vue-draggable-plus`（组件、composable、指令和 Sortable 生命周期共享一个 ESM 运行时） |
| peer 依赖 | `vue`（由 `ECMAScript.Vue` 资源库提供） |

## 首期范围 First slice

- 组合式函数：`useDraggable(element, list, options)`，返回 `start`/`pause`/`resume`/`destroy`
- 组件代理：`VueDraggableList`（`ModelValue`、`Tag`、`Options`、默认插槽与透传属性）
- 选项：SortableJS 常用集（`Animation`、`ChosenClass`、`GhostClass`、`Group`、`Handle`、`Filter`、`Direction`、`Disabled`、`Sort` 等）与 12 个 `on*` 事件回调

`VueDraggableList<TItem>` 按 `Transform.Component` 封装上游 `VueDraggable` 组件，`VueDraggable.UseDraggable` 映射上游 `useDraggable` composable。两者使用同一个 `vue-draggable-plus` 入口；该库的拖拽核心、组件生命周期、指令和 composable 共享状态，因此完整运行时闭包是组件绑定的稳定交付单元。

未绑定：`vDraggable` 指令（Razor 指令编排尚未建模）、`closest`/`save`/`toArray`/`option` 等 Sortable 实例方法。需要时按需扩充。

## SSR 边界 SSR boundary

拖拽依赖真实 DOM 与指针事件。`useDraggable`、`VueDraggableList` 与全部事件回调必须在浏览器生命周期内使用；SSR 期间不得创建 Sortable 实例，也不提供服务器端等价实现。

## 使用示例 Authoring

```razor
<VueDraggableList ModelValue="@items" Options="@new VueDraggableOptions { Animation = 150 }">
    <div data-key="@context">@context</div>
</VueDraggableList>
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-vue-draggable.cs -- --version <upstream-version>
dotnet run --file scripts/csharp/generate-vue-draggable.cs -- --source .tmp/p3c --version <upstream-version>
```

生成器在 vendor 时执行**浏览器安全闸门**：任何在模块顶层引用 `process.env` 的产物都会导致生成失败（这类 bundler 变体在浏览器导入即崩溃）。

## 测试 Tests

`src/ECMAScript.VueDraggable.Test` 覆盖 manifest/inventory 元数据、vendored 哈希、上游导出 drift、组件代理契约与编译器 emission。
