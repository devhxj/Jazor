# ECMAScript.Style 完成状态

> 记录日期：2026-07-29
>
> 范围：公共 API、强类型值域、运行时、上下文、DOM、水合、编译、物化、Bundle 与 NuGet

## 结论

`ECMAScript.Style` 的产品合同已经实现。该模块作为 ECMAScript 生态中的独立 opt-in 包，复用 Jazor 的通用编译、模块目录、物化与打包链路；没有引入 CSS 专用 Hook、RazorVue lowering、分析器例外或 MSBuild 配置。

公共入口统一为小写静态类 `css`。消费方既可使用 `css.style(...)`、`css.px(...)`，也可通过 `using static ECMAScript.Style.css;` 直接调用 `style(...)`、`px(...)`。模型类型继续采用 `CssRule`、`CssContext`、`CssOptions` 等标准 PascalCase 名称。

debug 运行时入口统一为 `style.mjs`。内部 `ecmascript-style:v1`、`ecs-*`、`ecs-k-*`、默认 StyleId 与 DOM 条目帧保持稳定，因此模块路径调整不会改变既有样式名称或 hydration 内容。

## 已完成能力

| 领域 | 当前合同 | 证据 |
| --- | --- | --- |
| 独立包 | `ECMAScript.Style` 精确依赖同版本 `Jazor`，不安装 CSS targets | pack 与本地 NuGet 消费 |
| 公共门面 | 唯一 `css` 门面，lower camel case，支持静态导入 | 反射与编译回归 |
| 属性目录 | 由 inventory 与 `@webref/css@6.12.7` 生成 705 个属性 | 生成器 `--check` |
| 强类型值域 | 原生 union、名义 token、关键字、单位、变量、颜色、网格与变换 | 反射、编译和 Deno 回归 |
| 类型准确性 | 跨域与隐式字符串赋值失败；混合长度百分比不进入纯长度属性 | Roslyn 诊断回归 |
| 开放语法 | `raw(...)` 显式承载未来或未建模语法 | 编译回归 |
| 基础规则 | style、keyframes、global、fallback、`!important`、自定义属性 | Deno 运行时回归 |
| 现代嵌套 | selector、media、supports、container、layer、scope、starting-style | 递归与顺序回归 |
| 声明型规则 | font-face、property、counter-style、page 与递归声明 block | `CssAtRule` 回归 |
| 确定性 | 属性排序、顺序敏感内容、领域隔离、固定 hash、碰撞失败 | `ECMAScript.Style.Test` |
| 独立上下文 | 相同内容同名、注册表隔离、同名方法重载 | detached context 回归 |
| DOM 目标 | document 与 DocumentFragment/ShadowRoot 单节点所有权 | DOM 模拟与真实浏览器 |
| CSP 与接管 | nonce、所有权头、UTF-16 长度帧、HMR 接管 | Deno 与浏览器 |
| 提取与水合 | 纯 CSS、StyleId、nonce、可接管 hydration 文本 | snapshot 与无重复水合 |
| Compiler | 用户运算符消费通用 `[ECMAScriptInline]` | Compiler 专用回归 |
| RazorVue | 普通导入 `style.mjs`，类名仍为普通字符串 | RazorVue 聚焦回归 |
| Emit | runtime catalog、source map、manifest、debug 根入口物化 | Catalog 与 SDK 集成回归 |
| Bundle | release 包含实际使用入口，无未解析 runtime import | 本地 NuGet release 构建 |

## 公共入口

默认上下文与显式上下文使用统一方法名：

```text
style(rule)                  style(context, rule)
keyframes(frames)            keyframes(context, frames)
global(selector, rule)       global(context, selector, rule)
atRule(rule)                 atRule(context, rule)
extract()                    extract(context)
snapshot()                   snapshot(context)
context(options)
configure(options)
```

常用值 API：

```text
px rem em percent deg rad ms seconds
color hex rgb rgba hsl hsla
var varOr ident keyword raw
fr minMax fitContent repeat
translate translateX translateY rotate scale transform
min max clamp
```

## 输出与配置

`ECMAScript.Style` 不增加任何构建属性，仅使用：

```xml
<PropertyGroup>
  <JazorMode>none</JazorMode> <!-- none | debug | release -->
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
</PropertyGroup>
```

- `none`：不物化前端产物；
- `debug`：输出 `style.mjs`、source map、消费模块与 manifest；
- `release`：输出 `bundle.js` 与 `bundle.js.map`，不保留中间模块。

## 可复验命令

```text
dotnet run --file scripts/csharp/generate-ecmascript-style-properties.cs -- --check
dotnet test src/ECMAScript.Style.Test/ECMAScript.Style.Test.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter Convert_EcmascriptInlineOperator
dotnet run --file scripts/csharp/verify-ecmascript-style-browser.cs
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter EcmaScriptStyle
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter EcmaScriptStyle
dotnet build Jazor.slnx -v minimal /m:1 /p:BuildInParallel=false
```

当前聚焦证据：

- 属性生成门禁通过，目录包含 705 个属性；
- `ECMAScript.Style.Test` 22 项通过；
- Compiler 内联运算符回归 1 项通过；
- RazorVue 聚焦回归 1 项通过；
- Emit catalog 回归 2 项通过；
- 本地 NuGet 包内容与 debug/release 消费回归 2 项通过；
- 完整解决方案构建通过，结果为 0 警告、0 错误；
- 仓库统一测试入口通过 6,541 项 MSTest 与 35 项 render-context 测试；
- 真实浏览器验证通过 computed style、nonce、单节点所有权、HMR、Shadow DOM 与 snapshot hydration；
- 发布后公开源消费结果将在 NuGet 发布完成后确认。

## 稳定边界

- release Bundle 保留运行时注册逻辑，不生成独立 `.css`。
- `raw(...)` 是值语法的显式开放边界；运行时不复制完整浏览器 CSS parser。
- 高基数连续值应使用内联样式或 CSS 自定义属性，运行时不自动回收已注册规则。
- statement at-rule、组件包装、PostCSS、autoprefixer、CSS Modules 与框架适配不属于当前合同。

这些内容是正式职责边界，不是隐藏配置、兼容回退或历史线路。
