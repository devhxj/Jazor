# ECMAScript.VueRoute

`ECMAScript.VueRoute` 是参照 `ECMAScript.Vue3` 风格建立的独立外部库项目，负责把 `vue-router` 4 的高频运行时 API 映射成可在 C# / Jazor authoring 中直接使用的宿主绑定。

## Current Scope

- `createRouter()` / `createWebHistory()` / `createWebHashHistory()` / `createMemoryHistory()`
- `useRouter()` / `useRoute()` / `useLink()`
- `RouterLink` / `RouterView`
- 路由记录、路由位置、基础查询参数/路由参数对象
- 常用导航 API：`push` / `replace` / `resolve` / `beforeEach` / `beforeResolve` / `afterEach`

## Boundary

- 该项目只做通用宿主映射，不向 compiler 增加 `vue-router` 专用特判。
- 绑定命名默认保持 Vue Router 官方 API 词根，只做 C# 大小写投影。
- 首版优先覆盖高频 authoring surface；更细的 generic 精度和长尾 API 可以后续按需补齐。
