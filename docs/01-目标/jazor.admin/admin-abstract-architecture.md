# Jazor.Admin 架构边界

## 结论

`Jazor.Admin` 是 UI 库中立的后台框架层；`samples/JazorAdmin` 是使用 TDesign Starter 的正式参考管理产品。两者通过稳定 contract 协作，不存在“参考产品使用第三方组件库就污染框架边界”的矛盾。

## 分层

### Jazor.Admin

只拥有后台结构语义和可替换容器：

- `AdminLayoutMode`、导航树、面包屑和页面动作模型
- Layout、Header、Sidebar、PageContainer 的公开容器 contract
- 与特定组件库无关的 slot：Logo、HeaderActions、UserRegion、Extra、导航模板
- 自有原生实现作为默认实现

公开 API 不出现 `TLayout`、`TMenu`、`TButton`、`El*` 或其他第三方组件类型。

### 组件库绑定

`ECMAScript.TDesign`、`ECMAScript.Vuetify` 和类似包仅提供各自组件的强类型 authoring surface 与包内静态资源。它们不拥有后台产品语义。

### JazorAdmin

JazorAdmin 在应用层把 `Jazor.Admin` contract 投影到 TDesign Vue Next Starter：

- 使用 Starter 的布局和页面模板；
- 增加产品定义的一级 IconBar；
- 组合 ASP.NET Core Identity、OpenIddict、Quartz 和业务 API；
- 用于验证 Jazor 的完整生产路径。

这种组合不得回流到 `Jazor.Admin` 的 public API。若将来需要另一个组件库的参考产品，应另建 sample 或应用项目，复用同一套中立 contract。

## 注入边界

公开壳层通过 `IVueContainerComponent`、`IVueContainerImplementation<TContainer>` 与 `[VueInject]` 选择编译期实现。应用层可以提供 TDesign 或其他实现；框架不维护第二套 adapter 协议，也不从组件库 props 推导产品 contract。

## 当前规则

- JazorAdmin 是正式参考产品，不是历史 spike。
- 组件库专属组合只驻留 sample 或应用层。
- `Jazor.Admin` 仅在存在真正可跨组件库复用的语义时演进。
- 页面结构以产品当前模板为准；不记录已淘汰协议作为新实现规则。
