# ECMAScript.VueRoute 状态

> 状态：当前状态快照
> 对应项目：`src/ECMAScript.VueRoute/`

## 结论

`ECMAScript.VueRoute` 提供 Vue Router 4 的类型化 C# authoring contract。当前定位是独立的 Vue 生态绑定包，依赖 Jazor 的 ECMAScript 编译和 Emit 能力，不承担 Razor Hook 或开发服务器职责。

当前已形成生产候选的高频 API 覆盖包括：

- `createRouter`、memory/history/hash history；
- `useRouter`、`useRoute`、`useLink`；
- `RouterLink`、默认 `RouterView`；
- route record、redirect、route props；
- `beforeEach`、`beforeResolve`、`afterEach`；
- 组件级 `OnBeforeRouteLeave`、`OnBeforeRouteUpdate`；
- `RouterKey`、`RouteLocationKey` 等类型化 injection key；
- `LoadRouteLocation`。

## 生产声明边界

当前不宣称完整覆盖 Vue Router 全部 TypeScript 类型和所有长尾运行时组合。以下内容仍需独立验证：

- typed routes 等高级类型玩法；
- 复杂 matcher、scroll、alias 和 guard 组合；
- `RouterView` scoped-slot 的独立真实 consumer runtime 场景。

编译器测试、Emit 测试和真实 consumer smoke 的结论应分别记录，不能用单一代理层测试替代完整运行验证。

## 验证入口

```text
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

详细覆盖矩阵见 [Vue Router API 覆盖矩阵](../../01-目标/ecmascript.vueroute/vueroute-api-coverage-matrix.md)。
