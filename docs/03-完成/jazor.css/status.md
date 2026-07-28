# Jazor.Css 完成状态

> 记录日期：2026-07-29
> 范围：公共 API、运行时、现代规则、隔离上下文、DOM 目标、服务端渲染快照、构建集成与 NuGet 发布边界

## 结论

`Jazor.Css` 的完整运行时合同已经实现。该模块继续作为独立的显式引用包，复用 Jazor 的常规编译、物化与打包链路；本次扩展未增加 CSS 专用 Hook、编译器分支、RazorVue 降低逻辑或 MSBuild 配置。

既有 `jazor-css:v1` 名称与 DOM 条目帧保持稳定。原有类规则、关键帧、全局规则、提取和默认 DOM 注入均保持兼容；新增能力以独立上下文和结构化模型扩展，不改变既有调用语义。

## 已完成能力

| 领域 | 当前合同 | 验证证据 |
| --- | --- | --- |
| 独立包 | `Jazor.Css` 精确依赖同版本 `Jazor`，不安装 CSS 专用 targets | Release pack 与本地 NuGet 消费 |
| 属性目录 | 从 Webref inventory 确定性生成 705 个标准属性 | 生成脚本 `--check` 与反射测试 |
| 基础规则 | class、keyframes、global、fallback、`!important`、自定义属性 | Deno 运行时回归 |
| 选择器 | `&`、后代组合、列表笛卡尔积、引号、转义、函数与属性选择器 | selector 扫描回归 |
| 分组规则 | `@media`、`@supports`、`@container`、`@layer`、`@scope`、`@starting-style` | 递归与顺序回归 |
| 声明型规则 | `@font-face`、`@property`、`@counter-style`、`@page` 及递归声明 block | `CssAtRule` Deno 与编译器回归 |
| 确定性 | 属性排序、顺序敏感内容、领域隔离、固定哈希向量、碰撞检查 | `Jazor.Css.Test` |
| 独立上下文 | 相同内容同名、注册表隔离、显式 context API | detached context 回归 |
| DOM 目标 | `document.head` 与 `DocumentFragment`/`ShadowRoot` 单 style 所有权 | DOM 模拟与真实浏览器验证 |
| CSP 与接管 | nonce、所有权头、UTF-16 长度帧、模块重载接管 | Deno 与浏览器验证 |
| 提取与水合 | 纯 CSS、StyleId、nonce、可接管 hydration 文本 | snapshot 及无重复水合验证 |
| 编译器 | 新 API 继续使用常规 import 与结构化 record 降低 | 编译器集成测试 |
| RazorVue | `Css.Class` 返回值继续作为普通字符串进入 `class` prop | RazorVue `JazorCss` 回归 |
| Emit | runtime catalog、source map、debug 物化 | `Jazor.EmitTest` |
| Bundle | release 包含实际使用入口，无未解析 runtime import | 本地 NuGet release 构建 |

## 公共入口

默认上下文保留：

```text
Css.Class
Css.Keyframes
Css.Global
Css.AtRule
Css.Extract
Css.Snapshot
Css.Configure
```

显式上下文使用：

```text
Css.CreateContext
Css.ClassIn
Css.KeyframesIn
Css.GlobalIn
Css.AtRuleIn
Css.ExtractFrom
Css.SnapshotFrom
```

`Detached=true` 提供无 DOM、请求级隔离的注册表；`Target=shadowRoot` 将样式节点限定在指定 `DocumentFragment`。两者互斥，避免一个上下文同时声明内存隔离与 DOM 所有权。

## 可复验命令

```powershell
dotnet run --file scripts/csharp/generate-jazor-css-properties.cs -- --check
dotnet run --file scripts/csharp/test-dotnet.cs -- --project css
dotnet run --file scripts/csharp/test-dotnet.cs -- --project css-browser
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter JazorCss
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter JazorCss
dotnet build Jazor.slnx -v minimal /m:1 /p:BuildInParallel=false
dotnet pack src/Jazor.Css/Jazor.Css.csproj -c Release
```

截至本状态页更新时：

- 属性生成门禁通过，目录包含 705 个属性；
- `Jazor.Css.Test` 共 15 项通过；
- 真实浏览器验证通过 computed style、nonce、单节点所有权、模块重载、Shadow DOM 与 snapshot 水合；
- RazorVue 聚焦用例 1 项通过；
- Emit/Catalog/NuGet/debug/release 聚焦用例 6 项通过；
- `Jazor.slnx` 构建为 0 警告、0 错误；
- Release 包内容与同版本精确依赖通过检查。

## 稳定边界

- release Bundle 保留运行时注册逻辑，不生成独立 `.css` 文件。
- CSS 值按作者内容保留；运行时只验证确定性序列化所需的结构，不实现完整 CSS 值语法检查。
- 高基数连续值应使用内联样式或 CSS 自定义属性，运行时不自动回收已注册规则。
- `@charset`、`@import` 和 `@namespace` 等 statement at-rule 不提供动态注册入口。
- 不提供 `styled(Component)`、原始 CSS 解析、PostCSS、autoprefixer、CSS Modules 或组件库适配层。

这些内容是正式职责边界，不是隐藏配置、兼容回退或待补实现。
