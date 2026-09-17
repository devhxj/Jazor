# ECMAScript.VueUse

VueUse 15 的 C# 组合式 API 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游版本、`manifest.json`（schema 2）、`dist/` 资源、许可证与 inventory。

Strongly typed C# composition-API bindings for VueUse 15, shipped as a Jazor JS resource library with a locked upstream version, package-local `manifest.json` (schema 2), `dist/` runtime assets, licenses, and inventory.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| npm 包 | `@vueuse/core`（并 vendor `@vueuse/shared`） |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 许可证 | MIT（`licenses/*-LICENSE`） |
| 入口 | `@vueuse/core`（自包含 bundle）、`@vueuse/shared`（基础工具） |
| peer 依赖 | `vue`（由 `ECMAScript.Vue` 资源库提供） |

`@vueuse/core` 会再导出 `@vueuse/shared` 的成员，因此两个逻辑 import 都进入资源闭包。

## 首期范围 First slice

按策展清单绑定高频 composable：浏览器与元素（`useMouse`、`useWindowSize`、`useElementBounding`、`useIntersectionObserver`、`useMediaQuery`、`usePreferredDark`、`useOnline`、`useNetwork`、`useBattery`、`useFps` 等）、存储与设备（`useStorage`、`useLocalStorage`、`useClipboard`、`useDark`、`useColorMode`、`useTitle`、`useFavicon`、`useFullscreen`）、时间（`useNow`、`useTimestamp`、`useTimeAgo`、`useIntervalFn`、`useTimeoutFn`、`useRafFn`）、事件（`onClickOutside`、`onKeyDown`、`onKeyStroke`、`onKeyUp`、`onLongPress`、`useEventListener`）与状态/工具（`useToggle`、`usePrevious`、`useDebounceFn`、`useThrottleFn`、`createEventHook`、`useAsyncState`、`computedAsync`、`computedEager`、`isClient`）。

The first slice binds a curated list of high-frequency composables across browser/element, storage/device, time, events, and state/utility groups.

未绑定的上游导出（`@vueuse/core` 暴露 300+ 成员）按需在 `Api/VueUse.Api.cs` 追加，无需改动资源闭包。

## SSR 边界 SSR boundary

VueUse 的浏览器类 composable 依赖 `window`/`document`：`useMouse`、`useWindowSize`、`useWindowScroll`、`useElementSize`、`useElementBounding`、`useIntersectionObserver`、`useResizeObserver`、`useDocumentVisibility`、`useWindowFocus`、`useActiveElement`、`useMediaQuery`、`usePreferred*`、`useBattery`、`useDevicePixelRatio`、`useFps`、`useIdle`、`usePageLeave`、`useFullscreen` 等必须在浏览器生命周期内使用；`isClient` 用于在 SSR 中分支。`useStorage` 系列在无 `localStorage` 环境会退化到内存实现，需按上游语义确认使用边界。

## 使用示例 Authoring

```csharp
using static ECMAScript.VueUse;

var mouse = UseMouse();
var size = UseElementSize(elementRef);
var dark = UseDark();
var clipboard = UseClipboard();

// 读取：mouse.X.Value；写入：dark.Value = true
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-vueuse.cs -- --version <upstream-version>
dotnet run --file scripts/csharp/generate-vueuse.cs -- --source .tmp/p3/node_modules --version <upstream-version>
```

生成器校验 registry integrity 与版本一致，并按入口文件裸 import 推导闭包边；上游引入未收录包引用时显式失败。

## 测试 Tests

`src/ECMAScript.VueUse.Test` 覆盖 import/manifest/inventory 元数据、vendored 哈希、上游导出 drift 与编译器 emission；`Jazor.EmitTest` 覆盖真实 materialization 闭包。
