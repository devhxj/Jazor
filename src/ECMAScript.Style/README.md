# ECMAScript.Style

> 定位：面向 Jazor ECMAScript 模块的强类型、确定性 CSS-in-JS binding。

`ECMAScript.Style` 将结构化 C# 值转换为标准 `style.mjs` runtime 模块。它保持普通 ECMAScript import、Razor-to-Vue 互操作、基于内容的稳定命名、隔离 registry、Shadow DOM 所有权、SSR snapshot、CSP nonce 与幂等 hydration。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Style" Version="0.23.0" />
</ItemGroup>
```

该包依赖同版本 `Jazor` 进行编译和产物发射，但不安装 Razor hook、CSS 专用 MSBuild target、Vue adapter 或 compiler 分支。只有调用 `css` API 才会注册样式。

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
    padding = important(px(8) | px(12))
});
```

CSS authoring DSL uses `lower_snake_case`: generated declaration properties, `css` facade members, tokens, and structural members such as `additional` and `children` all follow CSS-oriented spelling. CLR data/configuration models remain PascalCase, including `CssRule`, `CssDeclarations`, `CssAtRule`, `CssShadow`, `CssChild`, and `CssOptions`. The C# spelling changes do not change the generated CSS or the `style.mjs` JavaScript export ABI.

CSS authoring DSL 使用 `lower_snake_case`：生成的声明属性、`css` facade 成员、token，以及 `additional`、`children` 等结构成员都采用面向 CSS 的拼写。CLR 数据/配置模型仍保持 PascalCase，包括 `CssRule`、`CssDeclarations`、`CssAtRule`、`CssShadow`、`CssChild` 与 `CssOptions`。C# 拼写变化不会改变生成的 CSS 或 `style.mjs` 的 JavaScript 导出 ABI。

The public API uses typed domains for lengths, colors, time, display, selectors, and at-rules. Use an existing typed value directly; reserve `raw(...)` for CSS grammar that is not yet modeled and whose semantics the caller deliberately owns. `px(8) | px(12)` is a two-side padding shorthand, while `px(1) | solid` remains a typed border shorthand.

公开 API 使用类型化长度、颜色、时间、display、selector 与 at-rule 值域。已存在的强类型值应直接使用；`raw(...)` 只用于尚未建模且调用方明确承担语义的 CSS 语法。`px(8) | px(12)` 表示双边 padding 简写，而 `px(1) | solid` 仍表示强类型 border 简写。

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
