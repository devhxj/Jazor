# Jazor.Css 目标与边界

`Jazor.Css` 为 Jazor 应用提供结构化、确定且框架无关的 CSS-in-JS 能力。作者使用普通 C# API 描述样式，运行时以标准 ECMAScript 模块完成规范化、命名、注册、DOM 注入与提取；编译器、RazorVue 和 Emit 不承担 CSS 专用语义。

## 产品定位

该模块解决四个相互关联的问题：

1. C# 作者需要带成员提示的标准 CSS 属性，同时保留自定义属性、回退声明、at-rule 描述符和未来 CSS 值的开放性；
2. 相同样式内容需要在调用点、独立注册表和模块重载之间获得稳定名称，并避免重复注入；
3. 组件样式需要结构化表达选择器、条件规则、容器、层、作用域、起始样式和声明型 at-rule；
4. 浏览器、Shadow DOM 与隔离渲染需要共享同一份所有权、提取和水合合同。

正式链路如下：

```text
结构化 C# CssRule / CssAtRule
    -> Jazor.Compiler 常规 IOperation 降低
    -> Jazor.Css/runtime.mjs
    -> CssContext 注册表
    -> document / ShadowRoot / detached 快照
```

## 包边界

- `Jazor.Css` 是独立的显式 opt-in NuGet 包，并精确依赖同版本 `Jazor`。
- `Jazor` 不反向依赖 `Jazor.Css`；仅引用核心包不会获得 CSS API 或 runtime catalog。
- `Jazor.Css` 不依赖 Vue、RazorVue、ASP.NET Core 或组件库。
- 引用包不会主动注入样式；只有执行注册 API 才会产生规则。
- 包不安装 Razor Hook，不扫描组件，也不提供 CSS 专用 MSBuild props/targets。

## 公共合同

### 默认上下文

| API | 合同 |
| --- | --- |
| `Css.Class(CssRule)` | 返回稳定类名，并在首次出现时注册 CSS |
| `Css.Keyframes(params CssFrame[])` | 返回稳定动画名并注册关键帧 |
| `Css.Global(string, CssRule)` | 注册全局 selector 规则 |
| `Css.AtRule(CssAtRule)` | 注册结构化声明块 at-rule |
| `Css.Extract()` | 按注册顺序返回纯 CSS，不清空注册表 |
| `Css.Snapshot()` | 返回纯 CSS 与可接管水合文本 |
| `Css.Configure(CssOptions)` | 在首次注册前配置默认 DOM 目标、StyleId 与 nonce |

### 显式上下文

`Css.CreateContext` 创建独立注册表。对应操作使用 `ClassIn`、`KeyframesIn`、`GlobalIn`、`AtRuleIn`、`ExtractFrom` 和 `SnapshotFrom`，使样式归属在调用点明确。

相同内容在不同上下文中获得相同名称，但内存条目互不共享。指向同一 DOM 目标和 StyleId 的上下文可通过所有权帧接管同一样式节点，这是显式 DOM 持久化，而不是静态状态泄漏。

## 样式模型

标准属性从 Webref inventory 确定性生成，值类型保持为 `string?`，以容纳 `var(...)`、`calc(...)`、自定义函数和持续演进的 CSS 语法。

- `Additional` 表达重复声明、回退链与 `!important`，保留作者顺序。
- `Children` 表达选择器、`@media`、`@supports`、`@container`、`@layer`、`@scope` 和 `@starting-style` 的有序嵌套。
- `CssAtRule` 表达 `@font-face`、`@property`、`@counter-style`、`@page` 和递归页边距规则等声明块。
- 字符串索引器用于自定义属性、尚未进入清单的属性和不属于 `CSSStyleDeclaration` 的 descriptor。

API 不接受 `object`、动态字典或原始 CSS block。结构化输入是 C# 作者体验、确定性和错误传播的共同边界。

## 确定性与所有权

- 命名属性按最终 CSS 名做 ordinal 排序，C# 初始化顺序不影响名称。
- `Additional`、`Children`、关键帧和嵌套 at-rule 的顺序属于级联语义，必须进入规范内容与哈希。
- 类、关键帧、全局规则和 at-rule 使用独立名称领域。
- 名称使用版本化双 32 位状态哈希，并维护内容到名称、名称到内容的双向索引；冲突明确失败。
- 浏览器端每个“上下文/目标/StyleId”组合只管理一个带所有权头的 `<style>`，条目使用 UTF-16 长度定界帧。
- 模块重载或水合后，新上下文从 DOM 恢复条目；相同 ID 与正文不重复注入，不同正文不得复用 ID。

## DOM、隔离与 Hydration

默认上下文使用 `document.head`。`CssOptions.Target` 接受 `DocumentFragment`，主要用于 `ShadowRoot`；不同目标可使用相同 StyleId 而互不干扰。

`Detached=true` 创建无 DOM 上下文，并与 `Target` 互斥。每个请求或渲染任务可使用独立 detached 上下文，避免默认注册表跨请求共享。

`CssSnapshot` 提供：

- `CssText`：无内部标记的纯 CSS；
- `HydrationText`：带所有权头和长度帧的样式文本；
- `StyleId` 与 `Nonce`：浏览器接管所需的节点元数据。

浏览器使用相同 StyleId 与 nonce 创建上下文后，应无重写地接管快照对应的样式节点。

## 输出合同

`Jazor.Css` 完全复用 `JazorMode`：

| 模式 | 行为 |
| --- | --- |
| `none` | 不物化前端产物 |
| `debug` | 输出 runtime `.mjs`、source map 与 manifest 条目 |
| `release` | 将 runtime 与调用模块纳入生产 Bundle |

release 不生成独立 `.css`，也不执行构建期提取、PostCSS、autoprefixer 或 CSS Modules 转换。

## 稳定非目标

- 不封装或携带 Goober JavaScript；
- 不提供 `styled(Component)`、Vue wrapper 或组件库适配层；
- 不解析原始 CSS block、标签模板或任意 CSS 文本；
- 不增加 compiler intrinsic、analyzer 特例或 RazorVue lowering 分支；
- 不动态注册 `@charset`、`@import`、`@namespace` 等 statement at-rule；
- 不提供自动规则回收、引用计数或 LRU；
- 不新增 `JazorCss*` 构建配置。

这些项目属于明确的职责边界，不是隐藏配置或待补 fallback。

实施和验收见[完整实现计划](../../02-计划/jazor.css/Jazor.Css.Complete.ImplementationPlan.md)，当前证据见[完成状态](../../03-完成/jazor.css/status.md)。
