# ECMAScript.Style

> 定位：面向 Jazor ECMAScript 模块的强类型、确定性 CSS-in-JS binding。

`ECMAScript.Style` 将结构化 C# 值转换为标准 `style.mjs` runtime 模块。它保持普通 ECMAScript import、Razor-to-Vue 互操作、基于内容的稳定命名、隔离 registry、Shadow DOM 所有权、SSR snapshot、CSP nonce 与幂等 hydration。

本包是纯 Jazor 类库：`style.mjs` 由 Jazor 编译并写入程序集内的
`Jazor.Generated.ModuleCatalog`（`ECMAScriptCode`），不携带外部 `manifest.json + dist/**`
资源。编写或组合 Jazor 模块的项目应直接引用 `Jazor`；RazorVue 项目还应直接引用
`Jazor.Vue`。最终宿主只通过一次 Emit 物化选中的 catalog 依赖闭包。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.40.0" />
  <PackageReference Include="ECMAScript.Style" Version="0.40.0" />
</ItemGroup>
```

该包依赖同版本 `Jazor` 的 contract；编译器、analyzer 与 Emit 工具不会通过传递依赖安装。它不安装 Razor hook、CSS 专用 MSBuild target、Vue adapter 或 compiler 分支。可复用类库通常将直接的 `Jazor` 工具引用设置为 `PrivateAssets="all"`；最终宿主需要 Emit 时应自行直接引用 `Jazor`。只有调用 `css` API 才会注册样式。

## 编写方式

通过小写静态类 `css` 创建规则与 class name：

```csharp
using ECMAScript.Style;
using static ECMAScript.Style.css;

var actionClass = style(new CssRule
{
    display = inline_flex,
    gap = rem(0.5),
    align_items = keyword("center"),
    padding = important(px(8) | px(12) | px(16) | px(20))
});
```

CSS authoring DSL uses `lower_snake_case`: generated declaration properties, `css` facade members, tokens, and structural members such as `additional` and `children` all follow CSS-oriented spelling. CLR data/configuration models remain PascalCase, including `CssRule`, `CssDeclarations`, `CssAtRule`, `CssShadow`, `CssChild`, and `CssOptions`. The C# spelling changes do not change the generated CSS or the `style.mjs` JavaScript export ABI.

CSS authoring DSL 使用 `lower_snake_case`：生成的声明属性、`css` facade 成员、token，以及 `additional`、`children` 等结构成员都采用面向 CSS 的拼写。CLR 数据/配置模型仍保持 PascalCase，包括 `CssRule`、`CssDeclarations`、`CssAtRule`、`CssShadow`、`CssChild` 与 `CssOptions`。C# 拼写变化不会改变生成的 CSS 或 `style.mjs` 的 JavaScript 导出 ABI。

The public API uses typed domains for lengths, colors, time, display, selectors, and at-rules. Use an existing typed value directly; reserve `raw(...)` for CSS grammar that is not yet modeled and whose semantics the caller deliberately owns. `px(8) | px(12) | px(16) | px(20)` is a two-side padding shorthand, while `px(1) | solid` remains a typed border shorthand.

公开 API 使用类型化长度、颜色、时间、display、selector 与 at-rule 值域。已存在的强类型值应直接使用；`raw(...)` 只用于尚未建模且调用方明确承担语义的 CSS 语法。`px(8) | px(12) | px(16) | px(20)` 表示一至四值 padding 简写，而 `px(1) | solid` 仍表示强类型 border 简写。其它简写优先使用 `margin(...)`、`gap(...)`、`radius(...)` 等命名工厂，因为它们能在 C# 中保留各自的值域。

现代尺寸与锚点定位同样使用专用值域，不需要把 `anchor-size()` 或 `calc-size()` 写回原始字符串：

```csharp
var cardAnchor = anchor_name("--card");

var popoverClass = style(new CssRule
{
    anchor_name = cardAnchor,
    position_anchor = cardAnchor,
    width = calc_size(min_content, size + rem(1)),
    top = anchor(cardAnchor, anchor_bottom, rem(0.5)),
    margin_top = anchor_size(cardAnchor, anchor_block)
});
```

`CssSizingValue`、`CssAnchorPositionValue`、`CssInsetValue` 与锚点声明值域彼此独立，因此宽度、定位边、简写和锚点名称不会因更新 WebRef grammar 而退化为通用 `CssValue`。

## 开发者工作流

为组件创建样式时，先构造规则，再把 `style(...)` 返回的 class name 传给宿主元素：

```csharp
var buttonClass = style(new CssRule
{
    background_color = var("--brand-color"),
    color = color("white"),
    padding = px(8) | px(16)
});

return new ButtonModel { ClassName = buttonClass };
```

`var("--brand-color")` 是 `var(--brand-color)` 的专用入口；需要后备值时使用 `var_or("--brand-color", color("white"))`，或使用等价的 `var("--brand-color", color("white"))`。不要用 `keyword("var(...)")`，因为 `var()` 是函数值而不是 keyword。

同一 context 中内容完全相同的规则会复用 class name；不要为了“避免重复”在业务层维护另一份全局缓存。需要独立的页面、租户或测试边界时，使用 `context(...)`，并将 context 显式传给 `styleIn`、`extractFrom` 和 `snapshotFrom`。

`configure(...)` 只用于默认 context，并且必须在第一次调用 `style`、`global`、`keyframes` 或 `at_rule` 之前执行。服务端渲染建议使用 `context(new CssOptions { Detached = true })` 收集快照；浏览器端再将快照交给 hydration 流程，避免服务端探测 DOM。

## 性能与可预测性

- 规则名称由序列化内容决定，与注册顺序和进程实例无关，适合 SSR 与客户端复用。
- 声明会按 CSS 属性名排序，保证输出稳定；`additional` 和嵌套 `children` 保留作者顺序，因此只在确实需要重复声明或尚未建模的语法时使用。
- 高频路径应复用 `CssContext`，避免每次渲染都创建新 context；context 不应跨请求共享可变的 SSR 状态。
- `extract`、`snapshot` 是读取操作，不会清空 registry。重复调用不会重复注入 style 元素。
- 生产构建应使用 `JazorMode=release`，由 Emit/Netpack 处理模块闭包和 bundle；开发模式适合 source map 和调试，不代表最终资源大小。

## 常见错误

- `StyleId cannot be empty`：`StyleId` 只能是非空字符串；省略它会使用 `ecmascript-style`。
- `Configure must be called before...`：默认 context 已经注册过规则，应改用新的 `context(...)`，不要尝试重新定位已活动的 context。
- `A detached CSS context cannot have a DOM target`：分离 context 不拥有 DOM，删除 `Target` 或关闭 `Detached`。
- `style element ... is not owned`：目标节点中已有同名但非 ECMAScript.Style 管理的 `<style>`，请更换 `StyleId`。

这些错误表示配置或 CSS grammar 与当前契约不一致；不要用 `raw(...)` 绕过已存在的类型化属性。只有规范尚未建模时，才将 `raw(...)` 作为局部、明确的兼容桥接。

## 确定性与 hydration

规则名称从内容稳定推导，注册顺序、嵌套规则与 keyframe 输出保持可预测。`document`、`ShadowRoot`、detached 提取和 hydration 共享同一 runtime contract；SSR 只传递应用明确拥有的 snapshot 和 nonce 信息，不隐式建立全局状态。

## 产物与验证

`JazorMode=debug` 会随应用产物物化 `style.mjs`；release 由 `Jazor.Emit` 与 Netpack 处理。运行相关回归：

```bash
dotnet test src/ECMAScript.Style.Test/ECMAScript.Style.Test.csproj
```

## 边界

- 本包不负责 Razor Source Generator、Vue 组件 lowering 或 CSS 文件管理。
- 运行时样式值必须保持已声明的 C# domain，不以 `object?` 作为公开 catch-all。
- 具体 CSS 属性与生成输入以源码、WebIDL inventory 和测试为准，README 不维护逐项属性清单。

## 相关文档

- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)

