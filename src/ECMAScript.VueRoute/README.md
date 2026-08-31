# ECMAScript.VueRoute

> 定位：Vue Router 4 的独立强类型 C# binding 与 Razor-to-Vue authoring 接口。

本包属于 JS resource library：Vue Router 的已有 runtime ESM 位于包内
`manifest.json + dist/**`，许可证等附属文件由 manifest 显式声明；C# 程序集只提供映射和
authoring contract。消费方编写的 RazorVue 组件生成到消费程序集的
`Jazor.Generated.ModuleCatalog`。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.0" />
  <PackageReference Include="Jazor.Vue" Version="0.26.0" PrivateAssets="all" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.26.0" />
</ItemGroup>
```

`ECMAScript.VueRoute` 是独立生态包，不由 `Jazor` 主包默认携带。Razor 组件项目仍需按需引用 `Jazor.Vue`。

## 当前支持范围

- router 与 history：`createRouter()`、`createWebHistory()`、`createWebHashHistory()`、`createMemoryHistory()`。
- composition API：`useRouter()`、`useRoute()`、`useLink()` 与公开 injection key。
- 组件：`RouterLink`、`RouterView` 及 `VueRouterLink` Razor authoring proxy。
- 路由记录、`RouteLocationRaw`、query/params、`push`、`replace`、`resolve` 与常用导航 guard。

## authoring 规则

- 使用具体 `RouteLocationRaw`、路由记录和 guard contract，不使用 `object` / `object?` 模拟 JavaScript `any`。
- 普通标量、对象字面量和数组字面量优先直接赋给目标 host 类型。
- lambda、delegate、接口值无法由 C# 直接投影时，使用提供的强类型 `From(...)` 或 `Add(...)` overload，而不是依赖多跳隐式转换。
- return-based navigation guard 是推荐写法；legacy `next(...)` 仅保留兼容入口。

## 常用模式

```csharp
new RouteRecordSingleView
{
    Path = "/users",
    Component = RawRouteComponent.From(component)
};

BeforeEnter = RouteRecordBeforeEnter.From(
    (RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => true);
```

命名视图、lazy component、`props`、`redirect`、`meta` 和 `useLink()` 的完整类型以源码中的对应 union / helper contract 为准。

## 边界

本项目只提供通用 `vue-router` host mapping，不向 compiler 增加 Router 专用 lowering。binding 命名默认沿用 Vue Router API 词根，只有 C# 无法表达的 runtime 名称才使用显式映射。

## 验证

```bash
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj
```

## 相关文档

- [测试项目](../ECMAScript.VueRoute.Test/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
