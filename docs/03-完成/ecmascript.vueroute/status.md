# ECMAScript.VueRoute 状态

> 更新时间：2026-05-07

## 结论

`ECMAScript.VueRoute` 已从“高频 API 绑定测试通过但缺少真实 consumer 运行验证”的状态，推进到**可进入生产候选（production candidate）的高频路线绑定线**。

当前可以对外做出的生产声明是：

- Vue Router 4 高频 authoring surface 已具备独立项目、独立测试、emit/package 验证、真实 Vite consumer smoke。
- 默认 `RouterView` 消费路径、`RouterLink`、内存路由、导航守卫、redirect、route props、`useRoute()`、`useRouter()`、`useLink()`、注入键和 `loadRouteLocation()` 已有代码级与运行级证据。
- 当前仍不应宣称“完整覆盖 Vue Router 全量类型系统和所有长尾运行时 authoring 形态”。

## 已验证能力

- 独立项目构建通过：
  - `src/ECMAScript.VueRoute/ECMAScript.VueRoute.csproj`
- 独立测试工程通过：
  - `src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj`
  - 本轮结果：`94/94`
- 本地打包/emit/bundle 验证已通过：
  - `src/Jazor.EmitTest/SdkIntegrationTests.cs`
  - 已覆盖 Vue Router import、route object lowering、injection/reactive authoring、最终 bundle 产物
- Jolt bundling 路径已补齐 npm 依赖解析配置并有测试验证：
  - `src/Jolt/Build/DenoBuildImportMapGenerator.cs`
  - `src/Jolt/Build/DenoBundleRunner.cs`
  - `src/Jolt.Test/JoltBuildTests.cs`
- 真实 consumer smoke 已补齐：
  - `samples/ECMAScript.VueRoute.MemorySmoke/`
  - 已验证本地 `Jazor` package pack + sample host rebuild
  - 已验证本地 host 生成模块可被 Vite + Vue + Vue Router 真实消费
  - 已验证 Vite production build
  - 已验证 Vitest runtime/DOM
  - 已验证隔离 generated output consumer path，不依赖已跟踪 sample 产物

## 当前生产声明边界

当前生产声明覆盖以下高频路径：

- `createRouter()`、`createMemoryHistory()`、`createWebHistory()`、`createWebHashHistory()`
- `useRouter()`、`useRoute()`、`useLink()`
- `RouterLink`
- 默认 `RouterView` 消费路径
- route record authoring
- redirect authoring
- route props resolver
- `beforeEach` / `beforeResolve` / `afterEach`
- component-level `OnBeforeRouteLeave()` / `OnBeforeRouteUpdate()`
- typed injection keys：
  - `RouterKey`
  - `RouteLocationKey`
  - `RouterViewLocationKey`
  - `MatchedRouteKey`
  - `ViewDepthKey`
- `LoadRouteLocation(...)`

## 未纳入当前生产声明的边界

以下内容不在本轮 production candidate 声明内：

- Vue Router TypeScript 全量类型系统的逐项镜像覆盖
- typed routes 等更细粒度高级类型玩法
- 长尾 matcher / scroll / alias / advanced guard 组合的真实 consumer 全覆盖
- `RouterView` scoped-slot 的独立真实 consumer runtime 声明

说明：

- 本轮 sample 验证最终采用默认 `RouterView` 消费路径作为生产 smoke 主线。
- `RouterView` scoped-slot 的 compiler/boundary surface 已有测试，但尚未单独作为真实 consumer runtime 完成状态写入本页结论。

## 证据

本轮已通过的验证命令：

```powershell
dotnet build src/ECMAScript.VueRoute/ECMAScript.VueRoute.csproj -c Release -v minimal
```

```powershell
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj -v minimal
```

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~Build_LocalJazorPackage_WithVueRouteAuthoring_EmitsVueRouterImportsAndRouteObjects|FullyQualifiedName~Build_LocalJazorPackage_WithVueRouteInjectionAndReactiveAuthoring_EmitsTypedVueRouterContracts|FullyQualifiedName~Build_LocalJazorPackage_WithVueRouteReactiveAuthoring_BundlesThroughBundledDeno_AndResolvesVuePackages" -v minimal
```

```powershell
pwsh .\samples\ECMAScript.VueRoute.MemorySmoke\verify-smoke.ps1 -Configuration Debug
```

sample smoke 覆盖的运行时断言包括：

- 本地 pack `Jazor` 后，sample host 能从新鲜 `.nupkg` rebuild 并产出隔离 generated modules
- generated router module 存在并包含 `vue-router` runtime import
- generated host app 可被 Vite build
- runtime navigation scenario 可完成：
  - push
  - replace
  - redirect
  - blocked guard
- DOM consumer 可完成：
  - 初始 home route 渲染
  - detail route 切换
  - `useLink()` 导航
  - component guard 可观察
  - injection / matched route / route props 可观察

## 当前判断

如果目标是“Vue Router 高频业务 authoring 在 Jazor 外部库路线下能否按生产标准继续推进”，当前答案是**可以**。

如果目标是“`ECMAScript.VueRoute` 是否已经覆盖 Vue Router 4 的完整高级 authoring/runtime 全量面”，当前答案仍然是**不能这样宣称**。
