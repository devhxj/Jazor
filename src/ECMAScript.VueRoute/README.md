# ECMAScript.VueRoute

`ECMAScript.VueRoute` 是参照 `ECMAScript.Vue3` 风格建立的独立外部库项目，负责把 `vue-router` 4 的高频运行时 API 映射成可在 C# / Jazor authoring 中直接使用的宿主绑定。

## Install

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.26" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.1.26" />
</ItemGroup>
```

`ECMAScript.VueRoute` 作为独立前端库发布，不再由 `Jazor` 主包默认捆绑。

## Current Scope

- `createRouter()` / `createWebHistory()` / `createWebHashHistory()` / `createMemoryHistory()`
- `useRouter()` / `useRoute()` / `useLink()`
- 官方公开注入 key：`routerKey` / `routeLocationKey` / `routerViewLocationKey` / `matchedRouteKey` / `viewDepthKey`
- `RouterLink` / `RouterView`
- 路由记录、路由位置、基础查询参数/路由参数对象
- 常用导航 API：`push` / `replace` / `resolve` / `beforeEach` / `beforeResolve` / `afterEach`

## Boundary

- 该项目只做通用宿主映射，不向 compiler 增加 `vue-router` 专用特判。
- 绑定命名默认保持 Vue Router 官方 API 词根，只做 C# 大小写投影。
- 首版优先覆盖高频 authoring surface；更细的 generic 精度和长尾 API 可以后续按需补齐。

## Authoring Rules

- 公开 API 优先用 C# 类型系统表达约束，不用 `object` / `object?` 兜底去模拟 JavaScript `any`。
- JavaScript 侧的类型擦除不是放宽 C# API 的理由；只有 C# 本身确实表达不了时，才补显式 helper。
- 对普通标量、对象字面量、数组字面量，优先直接写目标宿主类型，让现有隐式转换承担投影。
- 对 lambda、delegate、接口类型值，优先使用库里显式提供的 `From(...)` 或强类型 `Add(...)` overload，而不是依赖 Roslyn 去“猜”一层 delegate/union 转换。

## Authoring Patterns

下面这些场景应该优先使用显式 helper：

- 路由组件 authoring

```csharp
new RouteRecordSingleView
{
    Path = "/users",
    Component = RawRouteComponent.From(component)
};

new RouteRecordSingleView
{
    Path = "/lazy",
    Component = RawRouteComponent.From(() => Promise<ECMAScript.Vue3.IVueComponent>.Resolve(component))
};
```

- 命名视图组件字典

```csharp
var components = new RawRouteComponents
{
    { "default", component },
    { "sidebar", () => Promise<ECMAScript.Vue3.IVueComponent>.Resolve(component) }
};
```

- `beforeEnter` / 导航守卫

```csharp
BeforeEnter = RouteRecordBeforeEnter.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => true);

var guards = new RouteNavigationGuard[]
{
    (RouteLocationNormalized to, RouteLocationNormalizedLoaded from) => true
};
BeforeEnter = RouteRecordBeforeEnter.From(guards);
```

- legacy `next(...)` callback 分支

这是兼容旧式 Vue Router authoring 的保留入口，不是当前推荐模式。
在 C# 层会收到弃用警告；生产代码优先使用 return-based guard：

- 放行：`return true;`
- 中断：`return false;`
- 重定向：`return new RouteLocationAsPath { ... };`
- 抛错：`return new Error(...);`

```csharp
next(NavigationGuardNextArgument.From((Vue3.VueComponentPublicInstance instance) =>
{
    _ = instance;
}));
```

`next(vm => ...)` 只对 `beforeRouteEnter` 风格组件守卫真正有意义；当前 `ECMAScript.VueRoute` 并不把它作为推荐 authoring surface。

- `props` / `redirect` / `meta` callback 值

```csharp
RouteRecordPropsResolver propsResolver = (RouteLocationNormalized to) => new UserProps();
RouteRedirectCallback redirectCallback = (RouteLocation to, RouteLocationNormalizedLoaded from) => "/login";

Props = propsResolver;
Redirect = redirectCallback;

var meta = new RouteMeta
{
    { "onEnter", () => { } }
};
meta["onLeave"] = RouteMetaValue.From(() => { });
```

- `useLink()` / maybe-ref authoring

```csharp
var toReadonly = ToRef(() => new RouteLocationAsRelative
{
    Name = "users"
});
var replaceReadonly = Computed(() => true);

var link = UseLink(new UseLinkOptions
{
    To = toReadonly,
    Replace = replaceReadonly
});
```

对 `VueReadonlyRef<T>`，当前已提供直接赋值能力，因为 C# 本身可以表达这类类源类型转换。

```csharp
var toRef = Ref(new RouteLocationAsRelative
{
    Name = "users"
});
var replaceRef = Ref(true);

var link = UseLink(new UseLinkOptions
{
    To = RouteLocationRawMaybeRef.From(toRef),
    Replace = RouteBooleanMaybeRef.From(replaceRef)
});
```

对 `IVueRef<T>`，仍应显式使用 `From(...)`。原因不是运行时差异，而是 C# 不允许以接口类型作为用户定义转换源，因此这里不能靠隐式转换消除样板。

同样的规则也适用于其他 union / wrapper authoring：

- 如果你已经有一个强类型委托变量、类实例变量、或可直接命中的 union 分支值，优先直接赋值。
- 如果你写的是 lambda / delegate 字面量，而 Roslyn 不能直接把它绑定到目标 nullable union 或 wrapper 属性，再使用显式 `From(...)`。
- 对 `history.state` 这类递归值面，优先直接写 `string?[]` / `bool?[]` / `Number?[]` / `HistoryState?[]` 等强类型数组；不需要手工把每个元素先包成 `HistoryStateValue`。

```csharp
RouteRecordPropsResolver propsResolver = (RouteLocationNormalized to) => new UserProps();
RouteRedirectCallback redirectCallback = (RouteLocation to, RouteLocationNormalizedLoaded from) => "/home";
RouterScrollBehavior scrollBehavior = (RouteLocationNormalized to, RouteLocationNormalizedLoaded from, ScrollPositionNormalized? savedPosition) => false;

var route = new RouteRecordSingleView
{
    Path = "/users",
    Component = component,
    Props = propsResolver
};

var redirect = new RouteRecordRedirect
{
    Path = "/legacy",
    Redirect = redirectCallback
};

var options = new RouterOptions
{
    History = history,
    Routes = [route, redirect],
    ScrollBehavior = scrollBehavior
};

var state = new HistoryState
{
    ["tags"] = new string?[] { "users", null, "detail" },
    ["flags"] = new bool?[] { true, false, null },
    ["steps"] = new Number?[] { (Number)1, null, (Number)2 },
    ["trail"] = new HistoryState?[]
    {
        new HistoryState { ["kind"] = "root" },
        null
    }
};

history.Push("/users");
history.Replace("/users/7");
history.Push("/users/7", state);
```

不要把 `From(...)` 当成默认习惯用法。只有在 C# 不能直接绑定字面量 authoring 形式时，才把它当作语言边界补位。

## Injection Keys

Vue Router 官方公开的注入 key 当前已经以强类型形式暴露：

- `VueRoute.RouterKey`
- `VueRoute.RouteLocationKey`
- `VueRoute.RouterViewLocationKey`
- `VueRoute.MatchedRouteKey`
- `VueRoute.ViewDepthKey`

其中 `ViewDepthKey` 对应的值面不是弱化成 `object` 或裸 `Symbol`，而是建模为专门的 `RouterViewDepthValue`：

- 普通数字可直接赋值
- `IVueRef<TNumber>` 因 C# 接口源转换限制，使用显式 `RouterViewDepthValue.From(...)`

```csharp
Vue3.Provide(VueRoute.ViewDepthKey, 1);

var depth = Ref(1);
Vue3.Provide(VueRoute.ViewDepthKey, RouterViewDepthValue.From(depth));
```

## Collection Initializer Preference

当 map 值来自接口类型或 lambda 时，优先使用集合初始化器而不是索引器赋值：

```csharp
var namedProps = new RouteNamedProps
{
    { "default", true },
    { "sidebar", new SidebarProps() },
    { "footer", (RouteLocationNormalized to) => new FooterProps() }
};
```

原因不是运行时限制，而是 C# 对“lambda/interface -> union/indexer setter”的静态转换能力有限。集合初始化器可以通过显式 `Add(...)` overload 保持强类型 authoring，而无需退回 `object` 或 compiler 特判。
