# ECMAScript.VueRoute 平衡式目标设计

## 定位

`ECMAScript.VueRoute` 是独立于 `ECMAScript` 平台内核、`ECMAScript.Vue3` 视图库线、`ECMAScript.Pinia` 状态管理线之外的路由 authoring surface。  
它的职责不是在 compiler 中加入 Vue Router 名字特判，也不是重建 Vue Router 的 TypeScript 类型系统，而是把 `vue-router` 4 的高频运行时能力暴露成稳定、可测试、可打包的 C# host binding。

## 目标

1. 保持与 Vue Router 官方运行时 API 的直接映射关系，让 C# 表面能对应 `vue-router` 的真实 authoring path。
2. 保持依赖方向清晰：`ECMAScript.VueRoute` 依赖 `ECMAScript.Vue3`，但不把路由语义反向污染回 `ECMAScript` 核心层。
3. 对 compiler 保持“普通外部库”姿态：通过 `[ECMAScript("npm:vue-router@4")]`、`[Description("@#...")]`、`[ECMAScriptInline(...)]` 和普通 record/object lowering 完成映射。
4. 保持 public API 不暴露 `object`，优先使用 `VueProps`、union struct、delegate、`VueReadonlyRef<T>`、`RouteLocation*` 等桥接类型承接 JS 的 unknown-like 形态。
5. 让测试和工程边界独立：`VueRoute` 的结构、导入、打包、脚本接线和 compiler-boundary 回归应在 `ECMAScript.VueRoute.Test` 中完成，而不是继续落在 `Jazor.CompilerTest`。

## 非目标

- 不在 compiler 中新增 `VueRoute`、`Router`、`useRoute` 之类的关键字或特判。
- 不追求镜像 Vue Router 全部 TypeScript utility types、内部 matcher 细节、历史实现细节。
- 不把 RazorVue、应用级导航约定、文件路由约定或其他应用工具链协议塞进 `ECMAScript.VueRoute`。
- 不为低频长尾 API 提前补满全部覆盖；优先主运行时和高频 authoring path。

## 推荐边界

### 1. 模块导入边界

- `VueRoute` 模块入口当前固定映射到 `npm:vue-router@4`。
- import-map、bundler alias、无版本裸导入是否切换，不在 compiler 中硬编码，由 `ECMAScript.VueRoute` 自身文档和测试约束。
- `VueRoute.cs` 只保留模块入口标记和共享委托/枚举声明，不承载静态 API 实现。

### 2. 静态 API 边界

- 高优先级运行时 API 直接映射：`createRouter`、`createWebHistory`、`createWebHashHistory`、`createMemoryHistory`、`useRouter`、`useRoute`、`useLink`。
- 组件入口直接暴露为静态属性：`RouterLink`、`RouterView`。
- 导航守卫优先覆盖 `onBeforeRouteLeave` / `onBeforeRouteUpdate`、`beforeEach` / `beforeResolve` / `afterEach` 这类主 authoring path。

### 3. 路由对象边界

- 位置对象、记录对象、query/params、props/options 优先用 record + `[Description("@#...")]` 做普通对象 lowering。
- JS 端 union surface 优先用 `readonly struct + implicit operator` 建模，例如 `RouteLocationRaw`、`RouteRecordRaw`、`RouteRedirectOption`。
- 不为 TS-only 精细类型差异牺牲 C# 可读性；先保证主 authoring path 可表达、可发射、可测试。

### 4. 组件/loader 边界

- `RouteComponent` 同时承接同步组件和异步 loader。
- 对接口组件路径使用 `ECMAScriptInline("__arg1")` 这类普通 host contract 处理，不为隐式转换失败引入编译器魔法。
- `RouterLink` / `RouterView` 仍作为 Vue3 component contract 暴露，而不是单独建运行时包装层。

### 5. 编译与测试边界

- `Jazor.Compiler` 只需要把 `ECMAScript.VueRoute` 当作普通外部 host binding 消费。
- `VueRoute` 自己的结构合同、反射合同、打包接线、共享脚本接线、compiler-boundary 行为由 `ECMAScript.VueRoute.Test` 锁定。
- 包消费端到端回归由 `Jazor.EmitTest` 负责，证明 `Jazor` nupkg 中的 `ECMAScript.VueRoute` 可以被真实 restore/build/emit。

## 设计取舍

### 独立测试工程优于继续堆进编译器测试

`VueRoute` 的确依赖 compiler 完成 import/lowering，但它的所有权属于外部库表面，不属于 compiler 主语义线。  
因此结构、脚本、打包、反射、消费回归都应独立到 `ECMAScript.VueRoute.Test`，避免 `Jazor.CompilerTest` 继续膨胀成外部库杂糅集。

### 普通 record/object lowering 优于 router 专用 lowering

Vue Router 里的大多数 authoring payload，本质上都是普通对象：`RouterOptions`、`RouteLocationAsPath`、`RouteRecord*`。  
当前设计优先复用通用 lowering 规则，而不是引入“路由专用对象发射协议”。

### 高频路径优先优于追求 TS 类型全量对齐

当前绑定优先覆盖：

- 创建 router
- 读取当前 route
- 声明 route records
- 使用 `RouterLink` / `RouterView`
- 常见导航 API 与守卫

而不是先去对齐全部内部 matcher、typed route map、实验性类型工具。

## 后续补齐方向

- 更完整的 Vue Router API 覆盖矩阵
- 是否切换 `npm:vue-router@4` 到更统一的导入约定
- 更多导航失败、meta、命名视图、重定向回调边界测试
- 文档与 Wiki 中的正式使用示例
