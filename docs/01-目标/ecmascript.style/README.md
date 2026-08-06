# ECMAScript.Style 目标与边界

`ECMAScript.Style` 是 ECMAScript 生态中的独立样式模块，为使用 C# 编写的 ECMAScript 程序提供强类型、确定且与 UI 框架无关的 CSS-in-JS 能力。开发者使用 C# 类型系统描述属性和值；生成后的标准 ECMAScript 模块负责规则规范化、稳定命名、注册、DOM 注入、提取与水合。Jazor 仅为其提供通用编译、物化与打包基础设施。

## 产品目标

该模块围绕四项长期目标设计：

1. **可靠编写**：以 Webref 语法数据生成属性目录，以 C# 原生 union 和名义值区分长度、百分比、颜色、时间、角度、轨道等值域，在编译期阻止明显的跨域赋值。
2. **开放演进**：通过 `raw(...)` 显式承载尚未建模的标准、实验性或自定义语法，不以封闭枚举阻碍 CSS 演进，也不将所有属性退化为任意字符串。
3. **确定输出**：相同规则在不同调用点、上下文和模块重载后得到相同名称；顺序敏感内容严格保留作者顺序，碰撞明确失败。
4. **统一运行**：浏览器、Shadow DOM、服务端提取和水合共享同一注册与所有权协议，不建立 CSS 专用编译或构建支线。

正式链路如下：

```text
CssRule / CssAtRule / typed CSS values
    -> Jazor.Compiler 常规 IOperation 降低
    -> style.mjs
    -> CssContext 注册表
    -> document / ShadowRoot / detached snapshot
    -> debug 模块或 release Bundle
```

## 包边界

- `ECMAScript.Style` 是独立、显式引用的 NuGet 包，并精确依赖同版本 `Jazor`。
- `Jazor` 不反向依赖 `ECMAScript.Style`；仅引用核心包不会获得 Style API 或模块目录项。
- 包不依赖 Vue、RazorVue、ASP.NET Core 或任何组件库。
- 引用包只使 API 与模块目录可用；只有执行注册方法才会创建规则或修改 DOM。
- 包不安装 Razor Hook，不扫描组件，不提供 CSS 专用 props、targets 或用户配置项。

## 公共模型

### 门面

唯一公共门面为小写静态类 `css`。所有方法和预定义值采用 lower camel case，可按需静态导入：

```csharp
using ECMAScript.Style;
using static ECMAScript.Style.css;

var className = style(new CssRule
{
    Display = inlineFlex,
    Gap = rem(0.5),
    Width = percent(100) - rem(2)
});
```

`css.style(...)` 与静态导入后的 `style(...)` 完全等价。包不注入全局静态 using，是否省略 `css.` 由消费项目决定。

### 类型化值域

标准属性由锁定的 `@webref/css@6.12.7` 语法快照与 WebIDL inventory 共同生成。生成器将属性映射到具体值域；复杂语法使用开放的 `CssValue` union，而非 `string?`。

主要构件包括：

- 名义值：`CssLength`、`CssPercentage`、`CssLengthPercentage`、`CssAngle`、`CssTime`、`CssColor`、`CssTransform`、`CssTrack` 等；
- 属性值域：`CssLengthPercentageValue`、`CssColorValue`、`CssTimeValue`、`CssDisplayValue`、`CssTrackValue` 等原生 union；
- 预定义关键字：`auto`、`none`、`normal`、`flex`、`grid`、`inlineFlex`、`relative`、`transparent` 等；
- 工厂函数：单位、颜色、变量、网格、变换、字符串、URL 和数学组合；
- 逃生口：`raw(string)` 显式接纳未来语法或尚未覆盖的复合值。

混合长度百分比运算返回独立的 `CssLengthPercentage`。因此 `Width = percent(100) - rem(2)` 合法，而纯长度属性不会错误接受该值。

### 规则模型

- `CssDeclarations` 承载命名属性、动态属性和有序补充声明。
- `CssRule` 在声明基础上增加有序子规则。
- `CssFrame` 表达关键帧。
- `CssAtRule` 表达声明块 at-rule 及递归子规则。
- `CssContext` 隔离名称索引、正文、顺序、DOM 目标与水合状态。
- `CssSnapshot` 同时提供纯 CSS 与可接管的水合文本。

`style(...)` 与 `keyframes(...)` 返回普通 `string`，可直接进入 ECMAScript 模块、Vue props 或 RazorVue `class` 属性，无需适配层。

## 确定性合同

- 命名属性按最终 CSS 名进行 ordinal 排序，C# 初始化顺序不影响名称。
- `Additional`、`Children`、关键帧和嵌套 at-rule 的顺序属于级联语义，必须进入规范内容与哈希。
- 类、关键帧、全局规则与 at-rule 使用独立名称领域。
- `ecmascript-style:v1`、`ecs-*`、`ecs-k-*` 与默认 StyleId `ecmascript-style` 是稳定协议值。
- 内容到名称、名称到内容采用双向索引；哈希碰撞或 DOM 所有权冲突明确失败。
- DOM 条目以 UTF-16 长度帧记录，避免 Unicode 内容使重载或水合边界漂移。

## 上下文与水合

默认上下文面向 `document.head`。`configure(...)` 只能在首次默认注册前设置 StyleId、CSP nonce 或目标。

`context(...)` 创建独立注册表：

- `Target=shadowRoot` 将所有权限定在指定 `DocumentFragment`；
- `Detached=true` 禁止 DOM 访问，适用于请求级提取；
- 两者互斥，避免同一上下文同时声明 DOM 所有权与无 DOM 隔离。

默认与显式上下文共用同一方法名，通过重载区分：`style`、`keyframes`、`global`、`atRule`、`extract` 与 `snapshot`。相同内容在不同上下文中得到相同名称，但内存条目互不共享。

`CssSnapshot.CssText` 是纯 CSS；`HydrationText` 包含所有权头与条目帧。浏览器使用相同目标、StyleId 和 nonce 创建上下文后，可原样接管已有样式节点，不重复写入规则。

## 输出合同

`ECMAScript.Style` 复用且仅复用三项标准 Jazor 配置：

| 配置 | 默认值 | Style 行为 |
| --- | --- | --- |
| `JazorMode` | `none` | `none` 不输出；`debug` 物化模块；`release` 生成 Bundle |
| `JazorDir` | `$(MSBuildProjectDirectory)\wwwroot\jazor\` | debug 与 release 的统一输出根目录 |
| `JazorTool` | `Deno` | 仅在 `release` 下选择 Deno 或 Netpack |

debug 入口固定为 `style.mjs`；release 将该运行时与消费模块纳入 `bundle.js`。模块不生成独立 `.css`，也不执行 PostCSS、autoprefixer 或 CSS Modules 转换。

## 明确非目标

- 不封装或携带 Goober 等 JavaScript CSS-in-JS 库；
- 不提供 `styled(Component)`、Vue wrapper 或组件库适配层；
- 不解析原始 CSS block、标签模板或任意 CSS 文本；
- 不增加 Style 专用 compiler intrinsic、analyzer 例外或 RazorVue lowering；
- 不动态注册 `@charset`、`@import`、`@namespace` 等 statement at-rule；
- 不提供自动规则回收、引用计数或 LRU；
- 不增加任何 Style 专用构建配置。

实施步骤见[实现计划](../../02-计划/ecmascript.style/ECMAScript.Style.ImplementationPlan.md)，验收结果见[完成状态](../../03-完成/ecmascript.style/status.md)。
