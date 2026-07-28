# Jazor.Css 第一阶段完成状态

> 记录日期：2026-07-28
> 范围：`Jazor.Css` 第一阶段公共 API、运行时、包、Emit/RazorVue 集成与浏览器行为

## 结论

第一阶段计划所定义的核心合同已经实现。`Jazor.Css` 作为独立 opt-in 包发布，复用 Jazor 的常规编译与输出链路；标准属性生成、规则规范化、确定性命名、无 DOM 提取、DOM 注入、CSP nonce、HMR 接管、debug 物化和 release Bundle 均有自动化证据。

主路未增加 CSS 专用编译器分支、RazorVue 适配或 MSBuild 配置。

## 已完成范围

| 领域 | 当前状态 | 主要证据 |
| --- | --- | --- |
| 包与 API | 独立 `Jazor.Css` 包，精确依赖同版本 `Jazor` | nupkg 内容与本地包消费测试 |
| 属性生成 | 从 `webidl.inventory.json` 生成 705 个属性 | 生成脚本 `--check` 与反射映射测试 |
| 规则生成 | 声明、fallback、selector、media、supports、keyframes、global | Deno 运行时回归 |
| 确定性 | 排序规范化、固定哈希向量、内容去重与碰撞检查 | `Jazor.Css.Test` |
| 环境行为 | 无 DOM registry、非破坏性提取、单 style、nonce、长度帧与接管 | Deno DOM 模拟与真实浏览器 smoke |
| Emit | catalog/source map 读取，debug 物化，release Bundle | `Jazor.EmitTest` 的 `JazorCss` 用例 |
| RazorVue | `Css.Class` 返回值直接进入 Vue `class` prop | `Jazor.RazorVue.Sg.Test` 的 `JazorCss` 用例 |
| 发布流程 | 解决方案、测试入口、发布脚本与 NuGet workflow 已纳入 | 项目与脚本配置审查 |

## 可复验命令

```powershell
dotnet run --file scripts/csharp/generate-jazor-css-properties.cs -- --check
dotnet run --file scripts/csharp/test-dotnet.cs -- --project css
dotnet run --file scripts/csharp/test-dotnet.cs -- --project css-browser
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter JazorCss
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter JazorCss
dotnet pack src/Jazor.Css/Jazor.Css.csproj -c Release
```

真实浏览器用例加载生成后的 `Jazor.Css/runtime.mjs`，验证背景色与 display 的 computed style、nonce、唯一 style 节点、Unicode 长度帧，以及查询参数重载后的幂等接管。

## 当前边界

- release Bundle 保留运行时注入逻辑，不生成独立 CSS 文件。
- `Css.Extract()` 是进程内非破坏性提取，不提供请求级 registry 隔离。
- CSS 值按作者内容保留，不执行不完整的值语法验证或安全清洗。
- 动态高基数值会持续增加 registry；连续值应使用 inline style 或 CSS 自定义属性。
- 首版只覆盖 selector、`@media` 和 `@supports` 子规则，不接受任意 at-rule。

上述边界属于第一阶段明确的产品范围，不是隐藏配置或 fallback 路径。
