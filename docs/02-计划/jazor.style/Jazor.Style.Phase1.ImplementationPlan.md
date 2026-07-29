# Jazor.Style 第一阶段实施计划

> 状态：第一阶段已完成；验证证据见 [`docs/03-完成/jazor.style/status.md`](../../03-完成/jazor.style/status.md)
> 更新：2026-07-28
> 结论：技术路线可行，建议以独立的 `Jazor.Style` 包实施
> 范围：原生 C# CSS-in-JS 运行时，参考 Goober 的最小核心，不封装或携带 Goober JavaScript 实现

## 1. 执行摘要

`Jazor.Style` 的第一阶段目标是：

**让 Jazor 用户以结构化 C# 定义样式，在运行时生成确定的类名和 CSS，并完成去重、注入与提取。**

建议的产品边界如下：

```text
C# CssRule / CssDeclarations
    -> Jazor.Compiler 普通 C# 语义降低
    -> Jazor.Style/runtime.mjs
    -> 规范化序列化
    -> 稳定内容哈希
    -> 类名 / keyframes 名称
    -> 内存样式表
    -> 浏览器 <style> 注入
```

第一阶段实施应遵守五项核心决策：

1. `Jazor.Style` 是独立、显式引用的 NuGet 包，`Jazor` 不反向依赖它。
2. 实现为普通 `[ECMAScriptModule]` C# 代码，不向 `Jazor.Compiler` 或 RazorVue 增加 CSS 专用降低。
3. 标准 CSS 属性从现有 WebIDL/Webref 清单生成；运行时只遍历实际初始化的字段，不携带属性元数据表。
4. 首版使用运行时 `<style>` 注入；生产 `Bundle` 内包含运行时样式逻辑，不生成独立 `.css` 文件。
5. 返回值是普通 `string`，Vue 可直接通过 `VueClassValue` 消费；`Jazor.Style` 不依赖 Vue。

## 2. 评估基线

### 2.1 已确认的现有能力

| 能力 | 当前证据 | 对方案的意义 |
| --- | --- | --- |
| record 结构化降低 | `SemanticWalkerCreationTest` 已覆盖 record 对象字面量、继承成员和静态 `null` 省略 | `CssRule` 与 `CssDeclarations` 可在调用点降低为纯 JavaScript 对象 |
| 特殊属性名 | `VisitObjectCreation_RecordSpecialPropertyNames_AreQuotedWhenRequired` 已验证 `data-user-id` 等带连字符键 | 生成属性可直接映射到 `background-color` 等最终 CSS 名称 |
| 动态自有键读取 | `ECMAScript.Object.Keys`、`Reflect.Get` 和 ECMAScript record proxy 索引器均有现有降低 | 序列化器可只遍历实际存在的声明 |
| DOM 注入 | `Global.Document`、`CreateElement`、`AppendChild`、`TextContent`、`SetAttribute` 已有宿主绑定 | 不需要新的 DOM 封装或手写 JavaScript 桥接 |
| 静态 ECMAScript 模块 | `ESGenerator` 已将静态 `[ECMAScriptModule]` 类生成 `.mjs` 和 catalog | CSS 核心可作为独立运行时模块输出 |
| Vue `class` 绑定 | `VueClassValue` 已接受 `string` | 不需要 `Jazor.Style.Vue` 适配层 |

2026-07-28 已在 `.NET 11 Preview 6` 工具链上执行以下聚焦验证：

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "Name=VisitObjectCreation_RecordSpecialPropertyNames_AreQuotedWhenRequired|Name=Visit_Reference_ActualEcmascriptBindings_EmitRealisticBrowserShapes|Name=Visit_Invocation_ObjectStaticMethod_Keys|Name=Visit_Reference_ReflectGet_UsesJsMemberName|Name=Visit_Reference_StaticField" --no-restore
```

结果：5 项通过，0 项失败。

### 2.2 可行性结论

当前编译器和宿主绑定已具备首版所需的基础语义。因此，主路线上不应修改：

- `Jazor.Compiler` 语法或宿主分派；
- `Jazor.RazorVue` Hook 或 render-function 构建器；
- `Jazor.Emit` 的 catalog 协议；
- `VueClassValue` 的公共合同。

实施前仍需要一个小型 G0 证明，验证“继承 record + 字符串索引器 + 无 DOM 环境检测 + 引用程序集 catalog 物化”的端到端组合，而不是重新证明每个已知子能力。

## 3. 产品定位与依赖边界

### 3.1 项目定位

| 项目 | 职责 |
| --- | --- |
| `Jazor.Style` | CSS 模型、规范化序列化、确定性哈希、缓存、样式表注入与提取 |
| `ECMAScript` | `Object`、`Map`、`Set`、DOM 和 CSSOM 宿主绑定 |
| `Jazor.Compiler` | 按现有规则编译普通 C# 语义 |
| `Jazor.Emit` | 物化 `Jazor.Style/runtime.mjs` 并纳入生产 `Bundle` |
| `ECMAScript.Vue3` | 在需要时将返回的字符串作为 `VueClassValue` 消费 |

依赖方向必须保持为：

```text
Jazor.Style -> ECMAScript
Jazor.Style NuGet -> Jazor NuGet

ECMAScript.Vue3 -- optional consumer --> Jazor.Style string result
```

`Jazor.Style` 不引用 `ECMAScript.Vue3`、`Jazor.RazorVue`、`Jazor.AspNetCore` 或任何第三方组件库。

仓库内项目引用与发布依赖应分开表达：

| 场景 | 引用 |
| --- | --- |
| 源码构建 | 引用 `ECMAScript.csproj`；将 `Jazor.Compiler` 和 `Jazor.Analyzer` 作为私有 analyzer/source-generator 工具引用 |
| NuGet 消费 | `Jazor.Style` 仅声明对同版本 `Jazor` 包的依赖，不向消费者暴露仓库内工程引用 |

### 3.2 启用方式

- 仅引用 `Jazor` 时，不包含 `Jazor.Style` 公共 API。
- 显式引用 `Jazor.Style` 后，才可使用 `Css.Class` 等 API。
- 引用包本身不会注入样式；只有执行 `Css.Class`、`Css.Global` 或 `Css.Keyframes` 才会注册 CSS。
- `Jazor.Style` 不安装额外 Hook，也不启动 Razor 扫描；消费者仅获得已编译的公共类型和模块 catalog。
- 不新增任何 `JazorStyle*` MSBuild 属性。输出仍只由现有 `JazorMode`、`JazorDir` 和 `JazorTool` 控制。

## 4. Goober 机制的取舍

[Goober](https://github.com/cristianbote/goober) 的核心价值不在 `styled(...)` 封装，而在一组很小且边界清楚的机制：输入编译、递归规则序列化、内容哈希、单样式表更新和提取。`Jazor.Style` 只借鉴这些机制，不复制 JavaScript API 形状。

| 能力 | Goober 常见形态 | `Jazor.Style` 决策 | 理由 |
| --- | --- | --- | --- |
| 声明输入 | 对象、数组、函数和标签模板 | 结构化 record | 保留 C# 属性名检查和 Razor/编译器可见合同 |
| CSS 文本解析 | `astish` 和正则解析 | 首版不接收原始样式块 | 避免在 C# 运行时再造一个不完整 CSS 解析器 |
| 嵌套规则 | 递归对象键 | `CssChild` 有序列表 | 明确区分 selector、`@media` 和 `@supports` |
| 类名 | 32 位内容哈希 | 双 32 位稳定状态 | 保持同步、小巧，同时显著降低碰撞概率 |
| 去重 | 内存 cache + CSS 文本查找 | 内存索引 + DOM 条目标记 | 保持常规运行与 HMR 重载幂等 |
| 样式目标 | 默认 `<style>`，可替换 target | 首版仅使用文档 `<head>` 中的单 `<style>` | 避免在首版引入 Shadow DOM/多样式表生命周期 |
| CSP | 隐式全局 nonce | `CssOptions.Nonce` | 配置来源明确，不依赖未声明全局变量 |
| 前缀 | 可注入 prefixer | 不自动加前缀 | 不让回调改变规范化哈希；必要前缀由开发者显式声明 |
| `styled(Component)` | 框架适配 API | 不纳入首版 | `Jazor.Style` 应保持框架无关 |
| 提取 | 返回当前样式文本 | 提供非破坏性 `Css.Extract()` | 用于无 DOM 执行、测试和诊断，不暗示完整 SSR 协议 |

本方案要求独立实现序列化、哈希与注入逻辑，不引入 Goober 运行时依赖。如实施中实际改编了第三方源码，必须同步核对许可证并更新仓库 `NOTICE.txt`；仅借鉴机制时不复制原实现。

## 5. 第一阶段公共 API

### 5.1 核心入口

首版公共命令表面固定为：

```csharp
namespace Jazor.Style;

[ECMAScriptModule("Jazor.Style/runtime.mjs")]
public static class Css
{
    public static string Class(CssRule rule);

    public static string Keyframes(params CssFrame[] frames);

    public static void Global(string selector, CssRule rule);

    public static string Extract();

    public static void Configure(CssOptions options);
}
```

语义约定：

| API | 语义 |
| --- | --- |
| `Css.Class` | 以规范化规则内容生成稳定类名，首次出现时注册 CSS |
| `Css.Keyframes` | 生成稳定动画名称并注册 `@keyframes` |
| `Css.Global` | 为显式 selector 注册全局规则，重复内容自动去重 |
| `Css.Extract` | 非破坏性返回当前样式表文本，不清空缓存或 DOM |
| `Css.Configure` | 在首次样式注册之前设置 `<style>` ID 和 CSP nonce |

`Configure` 在首次注册后再次调用应明确失败，不应隐式迁移已存在的样式表。

### 5.2 声明模型

```csharp
[ECMAScript]
[Description("@#")]
public partial record CssDeclarations
{
    [Description("@#$additional")]
    public CssDeclaration[]? Additional { get; init; }

    public extern string? this[string propertyName] { get; set; }
}

[ECMAScript]
[Description("@#")]
public sealed record CssRule : CssDeclarations
{
    [Description("@#$children")]
    public CssChild[]? Children { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record CssDeclaration(
    string Name,
    string Value,
    bool Important = false);

[String]
public enum CssChildKind
{
    [Description("@#selector")]
    Selector,

    [Description("@#media")]
    Media,

    [Description("@#supports")]
    Supports
}

[ECMAScript]
[Description("@#")]
public sealed record CssChild(
    CssChildKind Kind,
    string Prelude,
    CssRule Rule);

[ECMAScript]
[Description("@#")]
public sealed record CssFrame(
    string Selector,
    CssDeclarations Declarations);

[ECMAScript]
[Description("@#")]
public sealed record CssOptions
{
    public string? StyleId { get; init; }

    public string? Nonce { get; init; }
}
```

该模型的关键取舍是：

- `CssDeclarations` 可直接用于 keyframes，从类型上禁止帧中再嵌套 selector 或条件规则。
- `CssRule` 继承声明并增加有序 `Children`，保留 CSS 级联顺序。
- 标准属性使用生成的命名属性；字符串索引器只作为自定义属性、实验属性和清单尚未覆盖属性的受控出口。
- `Additional` 保留声明顺序，专门承载重复同名属性、fallback 链和运行时计算的属性名。
- 公共表面不出现 `object`、`Dictionary<string, object?>` 或递归动态联合。

### 5.3 属性值类型决策

首版的标准属性类型为 `string?`。这不是弱化为 JavaScript `any`，而是与现有 `CSSStyleDeclaration` 及 CSSOM 的可写属性合同保持一致。

首版不建立不完整的 `CssColor` / `CssLength` / `CssDisplay` 类型家族，原因是：

- CSS 值语法是开放的，需要容纳 `var(...)`、`calc(...)`、自定义函数和未来规范值；
- 局部强类型但不完整的值模型会迫使用户频繁逃逸，并不比 `string` 更可靠；
- 属性名已由 C# 命名属性约束，首版的主要错误面已经收窄。

后续只应根据真实使用频率增加返回 `string` 的单位或颜色帮助器，不应在第一阶段预先复制整套 CSS Typed OM。

### 5.4 作者端示例

```csharp
using Jazor.Style;

[ECMAScriptModule("components/button-styles.mjs")]
public static class ButtonStyles
{
    private static readonly string FadeIn = Css.Keyframes(
        new("from", new CssDeclarations
        {
            Opacity = "0"
        }),
        new("to", new CssDeclarations
        {
            Opacity = "1"
        }));

    public static readonly string Button = Css.Class(new CssRule
    {
        Display = "inline-flex",
        AlignItems = "center",
        Gap = "0.5rem",
        Color = "var(--button-text)",
        Animation = $"{FadeIn} 180ms ease-out",
        ["--button-text"] = "#f8fafc",
        Children =
        [
            new(CssChildKind.Selector, "&:hover", new CssRule
            {
                Transform = "translateY(-1px)"
            }),
            new(CssChildKind.Media, "(prefers-reduced-motion: reduce)", new CssRule
            {
                Animation = "none",
                Transition = "none"
            })
        ]
    });
}
```

Vue/Razor 组件直接消费 `ButtonStyles.Button` 字符串，不需要专用类型转换或组件包装。

## 6. CSS 属性生成方案

### 6.1 数据源

使用已纳入仓库的 `src/ECMAScript/webidl/webidl.inventory.json`。该清单由 `@webref/css` 数据构造 `partial CSSStyleDeclaration`，已是当前 ECMAScript 绑定的规范化来源。

生成器必须使用 `System.Text.Json` 解析结构化清单，不得通过正则表达式扫描已生成 C# 文件。

### 6.2 产物

新增单文件 C# 生成脚本：

```text
scripts/csharp/generate-jazor-style-properties.cs
```

生成并检入：

```text
src/Jazor.Style/CssDeclarations.Properties.g.cs
```

每个属性的形状为：

```csharp
[Description("@#background-color")]
public string? BackgroundColor { get; init; }
```

生成规则：

1. 只采集 `partial CSSStyleDeclaration` 中符合 CSS 属性形状的可写 `CSSOMString` attribute。
2. 以最终 CSS 属性名去重并按 ordinal 顺序输出。
3. `_float` 还原为 `float`；`webkit*`、`moz*`、`ms*` 等已知厂商前缀还原为带前导连字符的 CSS 名称。
4. C# 成员名使用稳定 PascalCase 规则；检查生成成员之间以及与 `Additional`、`Children` 等手写模型成员的冲突，发生冲突时生成失败，不静默覆盖。
5. 生成头记录 inventory schema 和 `webrefCss` 版本，但不写入当前时间、绝对路径或其他非确定数据。
6. 生成文件不手工编辑，变更数据源或命名规则时必须同步更新生成测试。

### 6.3 为什么生成公共属性不会膨胀 JavaScript

`CssDeclarations` 在调用点被降低为结构化对象。只有对象初始化器中实际使用的成员会进入 JavaScript 对象。

运行时序列化器通过 `Object.keys(rule)` 读取实际字段，不保留“所有 CSS 属性”的映射表。因此，大量命名属性主要影响 C# 程序集元数据和编辑期体验，而不是生产 JavaScript 体积。

## 7. 运行时设计

### 7.1 规范化序列化

序列化必须先产生规范内容，再计算名称和生成最终 CSS。不得直接对 record 的 JavaScript 对象做插入顺序依赖的 `stringify` 哈希。

声明序列化顺序：

1. 获取对象自有键；
2. 排除 `$children` 和 `$additional` 保留键；
3. 按 CSS 属性名做 ordinal 排序；
4. 跳过 `null` / `undefined`，保留空字符串和原始值内部空白；
5. 输出 `name:value;`；
6. 按作者给定顺序追加 `Additional`，根据 `Important` 显式追加 `!important`。

如果需要同名属性 fallback，应将该组属性全部写入 `Additional`，避免把命名属性与 fallback 顺序混用。

类规则的规范形式使用内部 root-selector token，不包含尚未生成的类名；keyframes 的规范形式只包含帧选择器和声明，不包含尚未生成的动画名称。完成哈希后，再以最终类名或动画名称渲染 CSS，从而避免名称与哈希输入循环依赖。

### 7.2 selector 组合

`CssChildKind.Selector` 使用下列语义：

- `Prelude` 中出现的 nesting token `&` 替换为当前 selector；
- 没有 `&` 时，按后代 selector 组合，即 `parent + " " + child`；
- selector 列表按顶层逗号分割，嵌套在 `:is(...)`、`:where(...)`、属性 selector 或字符串内的逗号不分割；
- 父子 selector 列表按笛卡尔积展开，保证多 selector 嵌套的含义正确；
- `&` 替换只处理非转义、非字符串且非属性值中的 nesting token。

这需要一个小型、状态明确的 selector 扫描器，而不是用单个正则表达式拆分 selector。扫描器只处理组合所需的引号、转义、圆括号、方括号和顶层逗号，不尝试实现完整 CSS selector 语法验证。

### 7.3 条件规则

`CssChildKind.Media` 和 `CssChildKind.Supports` 保留 `Children` 数组顺序，并分别输出：

```css
@media <prelude> { <current-selector-rules> }
@supports <prelude> { <current-selector-rules> }
```

条件规则可递归包含 selector、`@media` 和 `@supports`。首版不对条件表达式做语义改写，只去除外围空白并检查结构分隔符是否会破坏输出块。

### 7.4 keyframes 与全局规则

`Css.Keyframes` 以 frame 数组顺序作为级联顺序，frame selector 允许 `from`、`to` 和百分比列表。每个 frame 只接受 `CssDeclarations`，不接受嵌套规则。

`Css.Global` 不像 Goober 的单一全局槽位那样替换上一次内容；它按完整内容去重并保留首次注册顺序。动态主题应使用 CSS 自定义属性或主题类，不应隐式替换全局样式。

`Css.Global` 的第一个参数是 selector，不是任意 at-rule 或原始 CSS 入口。`@font-face`、`@import`、`@layer`、`@container` 和 `@page` 均不在首版合同中。

### 7.5 确定性哈希与名称

哈希输入必须包含领域和序列化版本：

```text
jazor-css:v1\0<class|keyframes|global>\0<canonical-css>
```

哈希算法固定为两套并行、更新式不同的 32 位状态，对 UTF-16 code unit 逐个更新：

- 状态 A 使用 FNV-1a 风格的 32 位更新式：初值 `0x811c9dc5`，乘数 `0x01000193`；
- 状态 B 使用 DJB2-XOR 风格的 32 位更新式：初值 `0x00001505`，乘数 `33`；
- 每轮乘法使用 `Math.imul`，每次更新后按无符号 32 位值截断；
- 最终分别以无符号 base36 输出，不依赖区域性或运行进程状态。

每个 code unit `u` 的更新顺序固定为：

```text
A = uint32(imul(A xor u, 0x01000193))
B = uint32(imul(B, 33) xor u)
```

算法、常量、UTF-16 输入单位和输出编码共同构成 `jazor-css:v1` 的持久合同，必须以固定向量测试锁定。后续如需更换算法，必须提升序列化版本，不得在 `v1` 下静默改变既有名称。

名称形状：

```text
class:     jz-<hash-a>-<hash-b>
keyframes: jz-k-<hash-a>-<hash-b>
global id: jz-g-<hash-a>-<hash-b>
```

硬性规则：

- 不得使用 `.NET string.GetHashCode()`；
- 同一规范内容必须始终产生同一名称；
- 属性初始化书写顺序不得改变名称；
- `Children` 和 `Additional` 的顺序属于可观察 CSS 语义，必须进入哈希；
- 注册时仍需要比对 ID 与规范内容；如发生真实碰撞，必须明确失败，不得复用错误样式。

### 7.6 缓存与样式注入

运行时维护：

- 规范内容 -> 名称映射；
- 名称 -> 规范内容映射；
- 按首次注册顺序保存的条目记录，包括 ID 和最终 CSS 正文；当前模块新注册的条目同时关联规范内容。

`Css.Extract()` 只按注册顺序连接 CSS 正文，不暴露下述 DOM 所有权标记和条目帧。内部注释仅用于浏览器样式节点的接管与校验。

浏览器环境中：

1. 使用默认 ID `jazor-css` 或 `CssOptions.StyleId` 查找现有 `<style>`；
2. 不存在时创建元素，写入根所有权标记 `/*jazor-css:v1*/`，按配置设置 nonce，并追加到 `document.head`；
3. 已存在时必须确认它是 `<style>` 且 `TextContent` 以相同根标记开头，否则报告 `StyleId` 冲突，不接管任意同名元素；
4. 已配置 nonce 时，复用元素的 nonce 必须与配置一致；未配置 nonce 时保留现有值，不覆盖；
5. 每个条目使用长度定界帧 `/*jz:v1:<entry-id>:<utf16-length>*/<css-body>`，长度按 CSS 正文的 UTF-16 code unit 计数；
6. 接管现有样式表时按帧头和长度重建条目顺序；格式损坏、重复 ID 对应不同正文或正文越界时明确失败；
7. 新条目使用文本节点追加，不在每次注册时重写整个样式表；
8. 模块内存缓存丢失但 DOM 尚在时，以条目 ID 和正文共同判定：完全一致则跳过，不一致则按哈希碰撞失败。

长度定界使接管逻辑无须按 CSS 语法或下一段注释猜测条目边界；即使声明值或 selector 中出现注释文本，也不会改变后续条目的解析位置。

首版不使用 `CSSStyleSheet.insertRule` 或 `adoptedStyleSheets`：文本节点可一致承载嵌套条件规则、keyframes 和条目标记，也与 nonce 及无 DOM 提取路线保持同一份 CSS 文本契约。

无 DOM 环境中，名称生成、去重和内存样式表照常工作，`Css.Extract()` 返回当前 CSS。这是可测试的无 DOM 能力，不是多请求 SSR 隔离协议。

### 7.7 输入约束

首版只保留数据正确性和输出结构所需的检查：

- selector、条件 prelude、frame selector 和声明名不得为空；
- `Css.Global` 与 selector child 的 selector 不得以 `@` 开头；
- 自定义声明名不得包含会逃出声明的 `:`、`;`、`{`、`}` 或空白；
- selector 扫描中未闭合的引号、圆括号或方括号必须失败；
- CSS value 按作者内容保留，不做不完整的语法校验、自动转义或安全清洗。

CSS value 是样式源片段，不应直接接收未经业务约束的用户输入。首版不因此引入一套不完整的 CSS 安全过滤器。

## 8. 与 Jazor 输出链的关系

### 8.1 `debug`

`JazorMode=debug` 时，`Jazor.Emit` 物化 `Jazor.Style/runtime.mjs`、source map 和 manifest 条目。CSS 仍在运行时注入，不额外生成 `.css` 调试文件。

### 8.2 `release`

`JazorMode=release` 时，运行时模块和调用点进入现有生产 `Bundle`。首版不改造 `Jazor.Emit` 使其执行静态 CSS 提取，不新增 CSS chunk manifest、PostCSS 或 autoprefixer 步骤。

### 8.3 为什么不在首版做构建时提取

构建时提取要求编译器区分静态规则与运行时规则，还需处理：

- 跨模块调用和静态初始化顺序；
- 条件样式与动态值；
- bundle chunk 归属和按需加载；
- CSS source map 与 C# source map 链接；
- SSR/hydration 的样式幂等。

这是独立产品能力，不是运行时版的自然附带功能。在没有明确产品契约之前，不为它预留编译器分支或 MSBuild 配置。

## 9. 分阶段实施 WBS

### G0：端到端可行性门禁

| ID | 任务 | 产物 | 验收 |
| --- | --- | --- | --- |
| G0.1 | 建立最小 `CssDeclarations -> CssRule` 继承 record 探针 | 测试内模块 | 命名属性、字面量索引器和保留键按预期发射 |
| G0.2 | 验证 `Object.keys + sort + indexer/Reflect` | 生成 JavaScript 断言 | 只读取实际设置属性，不输出属性元数据表 |
| G0.3 | 验证无 DOM 环境判定 | Deno 执行探针 | 不引用 `document` 时不抛出 `ReferenceError`，仍可返回类名与 CSS |
| G0.4 | 验证 DOM 样式注入 | 浏览器探针 | `<style>` 创建、nonce、文本写入和复用正确 |
| G0.5 | 验证引用程序集 catalog | debug/release 探针 | `Jazor.Style/runtime.mjs` 能从独立引用程序集进入物化与 Bundle |

G0 退出条件：

- 不存在 CSS 专用编译器分支；
- 不存在手写 JavaScript 运行时文件；
- Deno 与浏览器两个环境均完成最小执行闭环。

如 G0 发现通用 ECMAScript 宿主成员缺失，应补齐对应平台绑定和通用测试；不得为 `Jazor.Style` 新增按类型名匹配的降低特例。

### Phase A：项目、包与公共合同

| ID | 任务 | 产物 | 验收 |
| --- | --- | --- | --- |
| A1 | 新建 `src/Jazor.Style/` | csproj、nuspec、README | `net11.0`/preview 构建成功，包仅依赖 `Jazor` |
| A2 | 定义公共模型 | `CssDeclarations`、`CssRule`、`CssChild`、`CssFrame`、`CssOptions` | C# 使用方式符合本文契约 |
| A3 | 定义运行时入口 | `Css` 静态模块 | catalog 中路径稳定为 `Jazor.Style/runtime.mjs` |
| A4 | 纳入解决方案和发布清单 | `Jazor.slnx`、C# 发布脚本、NuGet workflow | 不新增 PowerShell 自动化，包可独立 pack |

### Phase B：属性生成

| ID | 任务 | 产物 | 验收 |
| --- | --- | --- | --- |
| B1 | 实现单文件 C# 生成器 | `generate-jazor-style-properties.cs` | 仅读取结构化 inventory，输出确定 |
| B2 | 生成公共属性 | `CssDeclarations.Properties.g.cs` | 运行时 CSS 名、C# 名和去重结果正确 |
| B3 | 建立生成回归 | `Jazor.Style.Test` 或聚焦生成器测试 | 代表性标准属性、逻辑属性、`float` 和厂商前缀均覆盖 |
| B4 | 增加重生成一致性门禁 | 校验命令 | 脚本重跑后 `git diff` 为空 |

### Phase C：规范化、哈希与规则生成

| ID | 任务 | 产物 | 验收 |
| --- | --- | --- | --- |
| C1 | 实现声明规范化 | 声明序列化器 | 属性顺序无关、`Additional` 顺序敏感、null 省略 |
| C2 | 实现 selector 扫描与组合 | selector 组合器 | `&`、后代、selector 列表、`:is(...)`、属性 selector 和转义覆盖 |
| C3 | 实现嵌套条件规则 | `media` / `supports` 序列化 | 嵌套顺序和当前 selector 传递正确 |
| C4 | 实现 keyframes 与 global | 对应 CSS 产物 | 名称稳定、条目去重、注册顺序正确 |
| C5 | 实现双状态哈希 | 同步哈希和 base36 编码 | 固定向量锁定 `v1`，跨执行结果稳定，不使用 `GetHashCode()` |
| C6 | 实现内容缓存与碰撞检查 | 双向索引 | 同内容去重，不同内容不得复用同 ID |

### Phase D：注入、提取与环境行为

| ID | 任务 | 产物 | 验收 |
| --- | --- | --- | --- |
| D1 | 实现无 DOM 内存样式表 | 内存 registry | `Class` / `Global` / `Keyframes` / `Extract` 在 Deno 中执行成功 |
| D2 | 实现单 `<style>` 注入 | DOM sheet | 默认 ID、追加顺序和重复调用正确 |
| D3 | 实现 CSP nonce | `CssOptions.Nonce` | 创建与复用样式表时 nonce 一致 |
| D4 | 实现所有权标记与长度定界条目帧 | HMR 幂等机制 | 模块缓存重置后可重建条目；同 ID 异正文明确失败 |
| D5 | 实现非破坏性提取 | `Css.Extract()` | 连续提取结果一致，不影响后续去重 |

### Phase E：集成、文档与发布

| ID | 任务 | 产物 | 验收 |
| --- | --- | --- | --- |
| E1 | 普通 ECMAScript 模块集成 | 非 Vue 测试资产 | 引用 `Jazor.Style` 即可编译并物化模块 |
| E2 | RazorVue 消费集成 | 最小 Razor 组件回归 | 返回字符串直接进入 Vue `class`，无适配代码 |
| E3 | debug/release 集成 | `Jazor.EmitTest` 或包内 smoke | debug 可检查模块，release Bundle 无未解析 import |
| E4 | 浏览器验证 | 单文件 C# 驱动的 browser smoke | 样式实际生效，重复加载不产生重复条目 |
| E5 | 文档收口 | 项目 README、仓库 README、目标/完成文档 | API、边界、输出模式和非目标一致 |
| E6 | NuGet 发布验证 | `.nupkg` 与本地消费工程 | 包依赖、README、catalog 和 source map 完整 |

## 10. 测试矩阵

| 层级 | 必须覆盖的场景 | 建议归属 |
| --- | --- | --- |
| 生成器 | 清单解析、去重、PascalCase、`float`、厂商前缀、冲突失败、重生成稳定 | `Jazor.Style.Test` 或 `ECMAScript.WebIDL.GeneratorTest` |
| 编译器形状 | 继承 record、带连字符键、字面量索引器、`params` frame、跨模块 import | `Jazor.CompilerTest` |
| 规范化 | 属性书写顺序无关、null 省略、空值保留、`Important`、重复 fallback | `Jazor.Style.Test` JavaScript 执行回归 |
| selector | `&:hover`、`> child`、多 selector、`:is(a,b)`、`[data-x="a,b"]`、转义、未闭合输入 | `Jazor.Style.Test` |
| 条件规则 | selector -> media、media -> selector、supports -> media、兄弟顺序 | `Jazor.Style.Test` |
| 名称 | `v1` 固定向量、同内容同名、异内容异名、跨进程稳定、class/keyframes/global 领域隔离 | `Jazor.Style.Test` |
| 无 DOM | 生成类名、去重、全局规则、keyframes、重复 `Extract` | Deno 回归 |
| DOM | `<style>` 创建与复用、所有权冲突、nonce、长度定界条目、条目顺序、HMR 重载、实际 computed style | 浏览器 smoke |
| Vue | Razor 参数/表达式生成的字符串类名进入 render function | `Jazor.RazorVue.Sg.Test` |
| Emit | 引用程序集 catalog、debug 物化、release Bundle、source map、manifest | `Jazor.EmitTest` |
| 包 | pack 内容、依赖组、README、本地 NuGet 消费 | 包消费 smoke |

实施完成时至少执行：

```powershell
dotnet test src/Jazor.Style.Test/Jazor.Style.Test.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "JazorStyle"
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter "JazorStyle"
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "JazorStyle"
dotnet build Jazor.slnx
dotnet pack src/Jazor.Style/Jazor.Style.csproj
```

浏览器验证必须由 `scripts/csharp/` 下的单文件 C# 入口驱动，不新增 `.ps1` 包装脚本。

## 11. 风险与控制

| 风险 | 影响 | 控制方式 |
| --- | --- | --- |
| record/索引器组合在真实引用程序集中出现缺口 | 高 | G0 先跑端到端探针，不在生产 API 完成后才发现 |
| 属性清单与 Webref 升级漂移 | 中 | 结构化数据源、检入生成物、确定性重生成门禁 |
| selector 组合被简单字符串替换破坏 | 高 | 使用有状态扫描器，聚焦覆盖引号、括号、属性 selector 和逗号 |
| 规范化排序破坏 fallback 顺序 | 高 | 唯一属性排序，重复/fallback 全部通过有序 `Additional` 表达 |
| 哈希碰撞复用错误 CSS | 高 | 双 32 位状态 + ID/内容反查，真实碰撞明确失败 |
| HMR 重载丢失内存 cache | 中 | 单 style ID + 稳定条目标记，不只依赖模块静态集合 |
| 高基数动态值使样式表无界增长 | 高 | 文档明确“有限样式状态”契约；连续值使用 inline style 或 CSS 变量；首版不做会破坏存量 DOM 的自动淘汰 |
| CSP 环境拒绝内联样式 | 高 | 显式 `Nonce` 配置与浏览器回归 |
| 用户误认 `release` 会提取 `.css` | 中 | README 明确首版是运行时 CSS-in-JS，Bundle 包含注入逻辑 |
| 多请求 SSR 共享静态 cache | 高 | 不声称首版具备请求隔离；服务器上下文化作为后续独立契约 |

## 12. 第一阶段非目标

下列内容不纳入首版，也不预留空抽象层：

- 封装、携带或运行 Goober JavaScript 包；
- `styled(Component)`、Vue component wrapper 或 Vuetify/其他组件库适配；
- 标签模板、原始 CSS block 解析或任意 JavaScript 函数插值；
- 编译器 CSS 专用 intrinsic、analyzer 特例或 RazorVue lowering 分支；
- 构建时 CSS 提取、CSS Modules、scoped CSS、PostCSS 和 autoprefixer；
- 完整 CSS Typed OM 值类型系统；
- `@font-face`、`@import`、`@layer`、`@container`、`@page` 等其他 at-rule；
- Shadow DOM 注入目标、多样式表、可替换写入接口或实例化 `CssSheet`；
- 多请求 SSR 隔离、流式注入和 hydration 样式协议；
- CSS source map；
- 样式自动回收、引用计数或运行时 LRU；
- 额外 MSBuild 输出配置。

## 13. 完成定义

第一阶段只有在以下条件全部满足时才视为完成：

1. `Jazor.Style` 可作为独立 NuGet 包构建、pack 和本地消费。
2. 标准 CSS 属性可从现有 WebIDL inventory 确定性生成，重生成不产生差异。
3. `Css.Class`、`Css.Keyframes`、`Css.Global`、`Css.Extract` 和 `Css.Configure` 按本文合同完成。
4. 命名对属性初始化顺序不敏感，对 `Children` / `Additional` 的可观察顺序敏感，并能检出哈希碰撞。
5. 无 DOM Deno 回归可生成与提取 CSS；真实浏览器回归可创建/复用 `<style>`、设置 nonce 并使样式实际生效。
6. 相同内容在常规重复调用和 HMR 风格模块重载后均不重复注入。
7. 普通 ECMAScript 模块和 RazorVue 组件均能消费返回的字符串类名，不需要 Vue 适配包。
8. `JazorMode=debug` 可物化运行时模块，`JazorMode=release` 可完成生产 Bundle，不新增 CSS 专用配置。
9. 主路实现不修改 `Jazor.Compiler` 或 RazorVue 的 CSS 专用语义。
10. 项目 README、仓库入口、测试说明和发布说明对首版运行时边界表述一致。

## 14. 实施顺序与合并策略

建议按以下顺序实施，每个切片在通过聚焦验证后独立提交：

1. G0 端到端探针与证据。
2. 项目骨架、公共模型和包元数据。
3. CSS 属性生成脚本、生成物与生成测试。
4. 规范化序列化、selector 组合和条件规则。
5. 哈希、缓存、global 和 keyframes。
6. 无 DOM 提取、DOM 注入、nonce 和 HMR 幂等。
7. Vue、Emit、Bundle、浏览器与 NuGet 消费集成。
8. README、目标文档、完成证据和发布说明收口。

不应把属性生成大文件、核心运行时、Emit 集成和文档改写混成一个不可审查的提交。
