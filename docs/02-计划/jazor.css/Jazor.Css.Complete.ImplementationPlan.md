# Jazor.Css 完整实现计划

> 状态：已完成
> 更新：2026-07-29
> 基线：`Jazor.Css` 第一阶段运行时与 `v0.1.31` 包
> 目标：在不引入 CSS 专用编译分支的前提下，完成可用于组件样式、Shadow DOM 与隔离渲染的正式产品合同

## 1. 实施结论

`Jazor.Css` 继续采用结构化 C# 模型与普通 ECMAScript 模块。完整实现不是复制浏览器 CSS 解析器，也不是把 Goober API 翻译成 C#；它要补齐运行时 CSS-in-JS 在实际应用中不可缺少的作用域、现代规则和 hydration 能力，同时保持第一阶段 API 与输出格式稳定。

正式链路为：

```text
CssRule / CssAtRule
    -> Jazor.Compiler 常规 IOperation 降低
    -> Jazor.Css/runtime.mjs
    -> CssContext registry
    -> document / ShadowRoot / detached snapshot
    -> debug module 或 release Bundle
```

## 2. 不变量

以下边界在完整实现中保持不变：

1. `Jazor.Css` 仍是独立、显式引用的 NuGet 包，并精确依赖同版本 `Jazor`。
2. 不增加 CSS 专用 Hook、analyzer 例外、compiler intrinsic、RazorVue lowering 或 MSBuild 属性。
3. 标准属性继续从 Webref inventory 确定性生成；CSS 值继续使用开放的 `string?` 合同。
4. `Css.Class` 继续返回普通 `string`，现有类规则、关键帧、全局规则与提取 API 保持兼容。
5. `jazor-css:v1` 哈希与 DOM 条目帧继续作为持久合同；本次扩展不得改变既有规则名称。
6. release 输出继续是包含运行时逻辑的 Bundle，不新增独立 CSS 构建管线。

## 3. 完整能力

### 3.1 现代嵌套规则

`CssChildKind` 在 `Selector`、`Media`、`Supports` 之外增加：

| Kind | 合同 |
| --- | --- |
| `Container` | 输出 `@container <prelude>`，prelude 必填 |
| `Layer` | 输出命名或匿名 `@layer` block |
| `Scope` | 输出带或不带范围的 `@scope` block |
| `StartingStyle` | 输出无 prelude 的 `@starting-style` block |

所有分组规则递归保留当前 selector 和 `Children` 顺序。`StartingStyle` 不接受 prelude；其他规则按各自合同检查空值和结构分隔符。

### 3.2 声明型 at-rule

新增 `CssAtRule` 与 `Css.AtRule`：

```csharp
public sealed record CssAtRule(
    string Name,
    CssDeclarations Declarations,
    string? Prelude = null,
    CssAtRule[]? Children = null);
```

该模型表达 `@font-face`、`@property`、`@counter-style`、`@page` 和 page-margin 等声明 block。at-rule 名按 ASCII 不区分大小写语义规范化为小写，prelude 保留作者内容并检查会破坏 block 的结构分隔符。

`@charset`、`@import`、`@namespace` 等 statement at-rule 不进入运行时 API。它们要求样式表头部顺序、外部资源获取或文档级解析状态，不适合增量 registry。

### 3.3 独立上下文

新增由 `Css.CreateContext` 创建的 `CssContext`。上下文持有独立的：

- canonical/name 双向索引；
- ID/body 碰撞索引；
- 注册顺序与 CSS 正文；
- StyleId、nonce、DOM 目标和接管状态。

默认 API 继续使用模块默认上下文。显式上下文使用 `ClassIn`、`KeyframesIn`、`GlobalIn`、`AtRuleIn`、`ExtractFrom` 与 `SnapshotFrom`，使 registry 归属在调用点可见，且避免 ECMAScript 模块导出重载冲突。

相同规则在不同上下文中必须生成相同名称，但不得共享内存条目。指向同一 DOM 目标与 StyleId 的上下文通过所有权帧接管同一 style 节点，这是显式的持久化共享，而不是静态 cache 泄漏。

### 3.4 DOM 目标

`CssOptions.Target` 接受 `DocumentFragment`，主要用于 `ShadowRoot`。目标为空时使用 `document.head`；目标非空时在该 fragment 内按 StyleId 查找、创建和接管 style。

上下文必须保存实际 owner document，后续追加文本不得重新依赖可能已经变化的全局 `document`。不同 ShadowRoot 可使用相同 StyleId 而互不冲突。

### 3.5 隔离提取与 hydration

`CssOptions.Detached=true` 禁止 DOM 访问，并与 `Target` 互斥。每个请求或渲染任务可创建独立 detached context，避免共享默认 registry。

`CssSnapshot` 同时提供：

| 字段 | 用途 |
| --- | --- |
| `StyleId` | hydration style 的 DOM ID |
| `Nonce` | CSP nonce |
| `CssText` | 无内部标记的纯 CSS |
| `HydrationText` | 带所有权头和长度帧的可接管 style 文本 |

浏览器 context 使用相同 StyleId 与 nonce 时，必须从 `HydrationText` 恢复条目索引；相同内容不得重写、重复追加或改变注册顺序。

## 4. 工作分解

| 阶段 | 工作 | 验收 |
| --- | --- | --- |
| A | 扩展公共模型与稳定导出 | 旧 API 无签名变化；新模型从独立包可见 |
| B | 将静态 registry 收敛为默认 `CssContext` | 既有固定哈希向量和 CSS 文本保持不变 |
| C | 实现现代 group at-rule 与声明型 at-rule | Deno 覆盖递归、顺序、空 prelude 与非法输入 |
| D | 实现 detached、ShadowRoot 和 snapshot | 隔离 registry、目标接管、hydration 均通过真实执行 |
| E | 穿透 compiler、RazorVue、Emit、Bundle、NuGet | 不新增专用分支；debug/release 真实消费成功 |
| F | 更新包 README、目标、状态与发布说明 | 公共合同、边界、命令和证据一致 |

## 5. 测试矩阵

| 层级 | 必须覆盖的场景 |
| --- | --- |
| 公共模型 | 新类型可从打包程序集编译消费；内部 context 状态不进入作者 API |
| 规范化 | 既有哈希固定向量不变；at-rule 名统一小写；顺序语义稳定 |
| Deno | context 隔离、现代嵌套、递归 at-rule、snapshot、错误传播 |
| DOM 模拟 | 自定义 target 创建与接管、nonce、所有权冲突、owner document 稳定 |
| 真实浏览器 | ShadowRoot computed style、单 style、HMR 接管、SSR hydration |
| 编译器 | 所有新 API 只产生普通 module import 与结构化 record，不增加 special case |
| RazorVue | 类名继续作为普通字符串进入 `class` prop |
| Emit | runtime catalog 与 source map 包含新导出，debug 物化完整 |
| Bundle | release 无未解析 import，新入口经 tree-shaking 后按实际使用保留 |
| NuGet | 独立包可编译 context、at-rule 与 snapshot 消费代码 |

## 6. 稳定非目标

以下项目不属于 `Jazor.Css` 完整运行时合同：

- `styled(Component)`、Vue wrapper 或组件库适配层；
- 原始 CSS block、标签模板或 CSS 文本解析器；
- 构建时静态提取、独立 `.css`、CSS Modules、PostCSS 与 autoprefixer；
- 完整 CSS Typed OM 值类型系统；
- statement at-rule 的动态注册；
- 自动引用计数、规则回收或 LRU；
- CSS 专用 source map；
- 新增 `JazorCss*` 配置项。

这些边界用于保持模块职责清晰，不视为隐藏的未完成实现。

## 7. 完成定义

只有满足以下条件，完整实现才可标记完成：

1. 第一阶段 11 项运行时回归全部保持通过，固定名称与正文无变化。
2. 新 context、at-rule、target 与 snapshot 场景均有 Deno 回归。
3. 真实浏览器验证 ShadowRoot 样式生效，并验证 snapshot hydration 不重复写入。
4. 编译器集成确认新 API 仍走常规 import 与 record lowering。
5. RazorVue、CatalogReader、debug、release Bundle 和本地 NuGet 消费全部通过。
6. 属性生成 `--check`、pack 内容和精确版本依赖通过。
7. README、目标、完成状态和发布说明使用同一正式合同。
8. 相关改动提交、推送并发布；发布包可由公开源重新消费验证。
