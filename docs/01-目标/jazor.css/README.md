# Jazor.Css 目标与边界

`Jazor.Css` 为 Jazor 应用提供结构化、确定且框架无关的 CSS-in-JS 能力。它以普通 C# API 表达样式，以标准 ECMAScript 模块承载运行时，不要求编译器、RazorVue 或 Emit 增加 CSS 专用协议。

## 产品定位

该模块解决三个相互关联的问题：

1. C# 作者需要具有成员提示的标准 CSS 属性，同时保留自定义属性、fallback 和未来 CSS 值的开放性；
2. 相同样式内容需要在不同调用点和模块重载后获得稳定名称，并避免重复注入；
3. 样式运行时必须自然进入现有 debug 物化和 release Bundle，而不引入第二套构建配置。

正式链路如下：

```text
结构化 C# CssRule
    -> Jazor.Compiler 常规 IOperation 降低
    -> Jazor.Css/runtime.mjs
    -> 内存 registry
    -> 浏览器单一 <style> 或无 DOM 提取
```

## 包边界

- `Jazor.Css` 是独立的显式 opt-in NuGet 包，并精确依赖同版本 `Jazor`。
- `Jazor` 不反向依赖 `Jazor.Css`；仅引用核心包不会获得 CSS API 或运行时 catalog。
- `Jazor.Css` 不依赖 Vue、RazorVue、ASP.NET Core 或组件库。
- 引用包不会主动注入样式。只有执行 `Css.Class`、`Css.Keyframes` 或 `Css.Global` 才会注册规则。
- 包不安装 Razor Hook，不扫描组件，也不提供 CSS 专用 MSBuild props/targets。

## 公共合同

| API | 合同 |
| --- | --- |
| `Css.Class(CssRule)` | 规范化规则，返回稳定类名，并在首次出现时注册 CSS |
| `Css.Keyframes(params CssFrame[])` | 生成稳定动画名并注册关键帧 |
| `Css.Global(string, CssRule)` | 注册全局 selector 规则，按完整内容去重 |
| `Css.Extract()` | 按注册顺序返回 CSS 正文，不清空 registry |
| `Css.Configure(CssOptions)` | 在首次注册前设置样式节点 ID 和 CSP nonce |

标准属性从 Webref inventory 确定性生成，值类型保持为 `string?`，以容纳 `var(...)`、`calc(...)`、自定义函数和持续演进的 CSS 语法。`Additional` 专门表达重复声明、fallback 与 `!important`；`Children` 表达 selector、`@media` 和 `@supports` 的有序嵌套。

## 确定性与所有权

- 命名属性按最终 CSS 名进行 ordinal 排序，C# 初始化顺序不影响名称。
- `Additional` 与 `Children` 的顺序属于级联语义，必须进入规范内容与哈希。
- 名称使用版本化的双 32 位状态哈希，并维护内容到名称、名称到内容的双向索引；发现冲突时明确失败。
- 浏览器端只管理一个带所有权头的 `<style>`，条目使用 UTF-16 长度定界帧。
- 模块重载后，新实例从 DOM 接管已有条目；相同 ID 与正文不重复注入，不同正文不得复用 ID。

## 输出合同

`Jazor.Css` 完全复用 `JazorMode`：

| 模式 | 行为 |
| --- | --- |
| `none` | 不物化任何前端产物 |
| `debug` | 输出 runtime `.mjs`、source map 与 manifest 条目 |
| `release` | 将运行时和调用模块纳入现有生产 Bundle |

第一阶段仍是运行时 CSS-in-JS。release 不生成独立 `.css`，也不执行构建期提取、PostCSS、autoprefixer 或 CSS Modules 转换。

## 非目标

- 不封装或携带 Goober JavaScript 实现；
- 不提供 `styled(Component)` 或组件库适配层；
- 不解析任意原始 CSS block；
- 不增加编译器 intrinsic、analyzer 特例或 RazorVue lowering 分支；
- 不承诺多请求 SSR 隔离、流式注入或 hydration 样式协议；
- 不提供样式引用计数、自动回收或运行时 LRU。

实施细节与验收矩阵见 [第一阶段实施计划](../../02-计划/jazor.css/Jazor.Css.Phase1.ImplementationPlan.md)，当前验证状态见 [第一阶段完成状态](../../03-完成/jazor.css/status.md)。
