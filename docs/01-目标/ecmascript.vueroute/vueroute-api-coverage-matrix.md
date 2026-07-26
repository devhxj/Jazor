# ECMAScript.VueRoute API 覆盖矩阵

> Baseline: Vue Router 4 官方运行时 API（`vue-router` 包）  
> Interpretation: “已覆盖”表示已有稳定 C# host binding；“部分覆盖”表示保留主 authoring path，但刻意没有镜像全部 TS 细节；“暂不覆盖”表示当前不进入 `ECMAScript.VueRoute` 设计边界。

## 当前覆盖

| 分类 | Vue Router API | `ECMAScript.VueRoute` | 状态 | 说明 |
|------|----------------|-----------------------|------|------|
| Router creation | `createRouter()` | `VueRoute.CreateRouter(...)` | 已覆盖 | 主入口 |
| History | `createWebHistory()` | `CreateWebHistory()` | 已覆盖 | 支持无参和 `basePath` |
| History | `createWebHashHistory()` | `CreateWebHashHistory()` | 已覆盖 | 支持无参和 `basePath` |
| History | `createMemoryHistory()` | `CreateMemoryHistory()` | 已覆盖 | 支持无参和 `basePath` |
| Composition | `useRouter()` | `UseRouter()` | 已覆盖 | 返回 `Router` |
| Composition | `useRoute()` | `UseRoute()` | 已覆盖 | 返回 `RouteLocationNormalizedLoaded` |
| Composition | `useLink()` | `UseLink(...)` | 已覆盖 | 返回 `UseLinkReturn` |
| Built-in components | `RouterLink` / `RouterView` | `RouterLink` / `RouterView` | 已覆盖 | 作为 Vue3 component surface 暴露 |
| Guards | `onBeforeRouteLeave()` / `onBeforeRouteUpdate()` | `OnBeforeRouteLeave(...)` / `OnBeforeRouteUpdate(...)` | 已覆盖 | 组件内守卫 |
| Router runtime | `push()` / `replace()` / `resolve()` / `beforeEach()` / `beforeResolve()` / `afterEach()` | `Router` 成员 | 已覆盖 | 高频导航与守卫主路径 |
| Route records | 单视图 / 多视图 / redirect | `RouteRecord*` | 已覆盖 | 普通 record lowering |
| Route location | string / path object / relative object | `RouteLocationRaw` | 已覆盖 | union surface |
| Query / Params | query / params object | `LocationQuery*` / `RouteParams*` | 已覆盖 | 高频 payload |
| Navigation failure | `isNavigationFailure()` / failure type | `IsNavigationFailure(...)` / `NavigationFailureType` | 已覆盖 | 主错误路径 |

## 部分覆盖

| 分类 | 官方能力 | 状态 | 说明 |
|------|----------|----------|------|
| Typed routes | `RouteMap` / 类型推导辅助 | 部分覆盖 | 当前优先普通 authoring path，不追 TS 类型系统级精度 |
| Route props | `props` 的全部 TS 细分 | 部分覆盖 | 先覆盖 bool / props object 主路径 |
| Redirect callback typing | 回调上下文的更细类型 | 部分覆盖 | 当前保留主回调语义 |
| Async component typing | 异步 loader 的更细返回类型 | 部分覆盖 | 当前统一在 `RouteComponentLoader` 下收口 |

## 暂不覆盖

| 分类 | 官方能力 | 状态 | 说明 |
|------|----------|----------|------|
| Internal matcher APIs | matcher / internal record graph | 暂不覆盖 | 不属于 public authoring surface |
| Experimental TS-only helpers | 类型级工具与内部 utility types | 暂不覆盖 | 不为 TS-only 类型结构扩张 C# surface |
| File-based routing conventions | 目录约定式路由 | 暂不覆盖 | 属于应用层/工具链工作流，不属于基础绑定 |
| Devtools / app-shell conventions | 应用级调试与宿主协议 | 暂不覆盖 | 不属于 `ECMAScript.VueRoute` |

## 当前结论

`ECMAScript.VueRoute` 已覆盖 Vue Router 4 的第一批高频 authoring surface：

- 创建 router
- 读取当前 route
- 声明常见 route records
- 使用 `RouterLink` / `RouterView`
- 常见导航 API 与守卫

当前不追求成为 Vue Router 全量 TypeScript 声明的逐项镜像，而是优先保证：

1. C# API 可读且可发现
2. 发射结果稳定
3. 打包、测试、统一脚本和包消费链条完整
