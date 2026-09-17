# ECMAScript.FloatingUi

Floating UI 的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游多包版本、`manifest.json`（schema 2）、`dist/` 资源、许可证与 inventory。

Strongly typed C# bindings for Floating UI, shipped as a Jazor JS resource library with locked upstream package versions, package-local `manifest.json` (schema 2), `dist/` runtime assets, licenses, and inventory.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| 作者入口 | `@floating-ui/vue` |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 闭包包 | `@floating-ui/vue`、`@floating-ui/dom`、`@floating-ui/core`、`@floating-ui/utils` |
| 许可证 | MIT（`licenses/*-LICENSE`） |
| peer 依赖 | `vue`（由 `ECMAScript.Vue` 资源库提供） |

各包版本分别锁定并记入 `inventory.json`；`manifest.json` 的 `version` 是作者入口包 `@floating-ui/vue` 的版本。

## 首期范围 First slice

- `useFloating`：响应式坐标、摆放状态、中间件数据、`floatingStyles` 与手动 `update`
- 中间件工厂：`offset`（数值或分轴）、`shift`、`flip`、`size`、`arrow`、`autoPlacement`、`hide`、`inline`、`limitShift`
- 生命周期与计算：`autoUpdate`（返回解绑函数）、`computePosition`（Promise 结果）

未绑定：`platform`、`detectOverflow`、`getOverflowAncestors`，以及需要 `MiddlewareState` 的自定义中间件。需要时按需扩充 `Api/FloatingUi.Api.cs`。

Not bound yet: `platform`, `detectOverflow`, `getOverflowAncestors`, and custom middleware that requires `MiddlewareState`.

## SSR 边界 SSR boundary

Floating UI 依赖浏览器 DOM。`useFloating`、`autoUpdate`、`computePosition` 与全部中间件必须在浏览器生命周期内使用（挂载后、卸载前）；SSR 期间不得直接调用，也不提供服务器端等价实现。

Floating UI depends on the browser DOM. `useFloating`, `autoUpdate`, `computePosition`, and all middleware must run within the browser lifecycle (after mount, before unmount); they must not be called during SSR, and no server-side equivalent is provided.

## 使用示例 Authoring

```csharp
using static ECMAScript.FloatingUi;
using static ECMAScript.Vue;

var reference = UseTemplateRef<Element>("reference");
var floating = UseTemplateRef<Element>("floating");

var position = UseFloating(reference, floating, new FloatingUseOptions
{
    Placement = FloatingPlacement.BottomStart,
    Middleware = [Offset(8), Flip(), Shift()]
});

// position.FloatingStyles 直接绑定到浮层的 :style
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-floating-ui.cs -- --version <author-entry-version>
dotnet run --file scripts/csharp/generate-floating-ui.cs -- --source .tmp/p3/node_modules --version <author-entry-version>
```

生成器校验上游 integrity、按入口文件的裸 import 推导闭包边，并在上游引入本表未收录的包引用时显式失败。

## 测试 Tests

`src/ECMAScript.FloatingUi.Test` 覆盖 import/manifest/inventory 元数据、vendored 哈希、上游导出 drift 与编译器 emission；`Jazor.EmitTest` 覆盖真实 materialization 闭包。
