# preview.8 开发者反馈验收

版本：`1.0.0-preview.8`（2026-10-09）。来源是 Zero/Jero 全站迁移反馈 `doc/jazor/JAZOR-FEEDBACK.md` 及同目录 `gaps.md`（G1–G34），反馈文件保留在消费者仓库。已有复现证据的框架问题已落实为支持切片或明确诊断，完整主线、质量与本地发布包消费者门禁均已通过；公开上传由官方 tag workflow 执行。

## 验收范围

| 反馈 | 实现结果 | 验收证据 |
| --- | --- | --- |
| G13 | 组件 candidate、Vue marker、module 声明边界诊断；缺模块路由组件报错 | `RazorSgFeedbackCorrectnessTests` 覆盖作者位置、稳定排序、全部缺失契约和正常 external binding 豁免 |
| G33/G34 | wrapper 默认 props 保留；`Class`/映射 `CssClass` 冲突在生成期提示 | 常量默认值、显式覆盖、splat precedence 与冲突诊断；未覆盖的动态 initializer 仍拒绝并给出替代建议 |
| G27/G28 | code-behind Vue lifecycle import 保留；条件分支使用稳定 Fragment 锚点 | 真实 Vue/happy-dom 覆盖 constructor/OnInitialized hook 注册、mount/unmount/remount、每次挂载 90 次 loading/empty/data 转换及资源清理 |
| G5/G7/G8 | Select、Table、Dropdown、消息/通知/确认/Prompt 强类型交互 | `RazorSgOfficialElementPlusTypedInteractionTests` 覆盖泛型 payload、slot、selection/selectable、command、服务调用和 DOM 交互；具体 stub 边界见下文 |
| G29 | CLR 将 `InputFileChangeEventArgs` 映射到 `EventRef` 与原生 `FileCount`；`NativeFileEventExtensions` 提供 input/drop `Files` | `FileList`/`FileRef` 保持 WebIDL；compiler 和官方 SG/Deno 回归覆盖选取、清空、多文件、drop、multipart、下载 URL 和取消 |
| DOM events/payload | 12 种 DOM 事件的共有属性与精确 `NativeEvent`；CLR DataTransfer、DataTransferItem、TouchPoint、ElementReference 原生投影 | CLR metadata、compiler 发射及官方 SG/runtime 回归覆盖原对象身份、继承属性、单次求值、nullability、原生集合和 CLR DTO 冲突边界 |
| G1 | 前端宿主可接入 `IServiceCollection` / 真正的 `IApplicationBuilder` | `JazorFrontendBuilderTests` 覆盖 HTTP body/query/header/response、双向 WebSocket、PathBase 和 Release 静态资源 |
| G32 | 显式声明的子路径复用闭包中唯一包身份 | npm/JSR/alias、version/integrity、未声明与身份歧义诊断；Base64 完整性摘要按大小写精确比较 |
| G14 | 在程序集身份去重前选择目标 RID 兼容资产 | RID graph、foreign asset 过滤、冲突路径/RID/选择原因 |
| Publish | 默认只交付可运行的 `dist/**` 与可选 `ssr/**`，source map opt-in | SDK fixture 与真实包消费者验证 bundle/chunks/SSR runtime、排除 node_modules/源码、map 默认值与 RID 转发 |
| G2/G3 | 项目体可以设置默认 `JazorMode`；生成目录不吞作者目录 | MSBuild fixture 覆盖 import 顺序、自定义/相对目录、作者 `.cs`/`.razor` 输入和 publish closure |
| DOM/G12 | live `DOMTokenList`、`IWindow.LiveLocation`、显式 `Number` 到 `long`/`ulong` | compiler 和 WebIDL 生成器回归；`Count` 迁移至 `Length`/`GetItem`，大于 `2^53` 的标识保持字符串/BigInt |
| G11 | 结构化 plain object 的 BigInt 使用调用方十进制字符串 replacer | long/ulong、嵌套值、普通 number、null、数组与全局原型不变；不模拟 local runtime class 的 CLR DTO 序列化 |
| G31/G8e | 官方 SG 语法、增量诊断与 extern null 判断按复现边界处理 | `@:`/`<text>`、插值、同行标签、裸文本诊断、extern DOM null comparison；增量回归覆盖 checksum、编辑后作者位置和 source map |

## 最终验证

2026-10-09 对最终实现完成以下验证：

| 门禁 | 结果 |
| --- | --- |
| Release solution build | 0 warning、0 error |
| 完整主线 | Compiler `10728/10728`、CLR `5092/5092`、RazorVue SG `5035/5035`、Emit `221/221`；Style 与全部主线生态 lane 通过 |
| Compiler coverage | 行 `99.41%`（17702/17807）、分支 `97.02%`（6895/7107） |
| RazorVue coverage | 行 `97.53%`（14623/14993）、分支 `94.01%`（6282/6682） |
| Vue binding coverage | 目标公共契约审计 `100%`，对应测试通过 |
| 公共 API | `76564` 条（9017 个顶层声明、67547 个成员）；Release 程序集与更新后基线比较新增 `0`、删除 `0` |
| 绑定与 XML 文档 | 五项生成检查和 inventory fingerprint 无漂移；26 个程序集 XML 检查通过 |
| diagnostics / typed bootstrap | 作者位置与机器协议、typed consumer 均通过 |
| 参考应用矩阵 | RazorVue.Authoring 与 JazorAdmin 的 source/package/Release Chrome smoke 通过 |
| 本地 NuGet 包 | 23 个 lockstep 包及 package shape 验证通过 |
| Windows SPA consumer | 真实 Chrome、`/docs` PathBase、发布资源和交互通过 |
| Windows SSR consumer | 独立发布目录 Deno SSR bundle、HTML、`/todo` 资源与真实 Chrome hydration 通过 |

相对反馈实现前的 API 基线，累计新增 `445` 个去重签名、改变 `14` 个去重签名；DOMTokenList 和 TableColumn scoped slot 的迁移见[API 冻结审查](../03-guides/public-api-freeze.md)与 [CHANGELOG](../../CHANGELOG.md)。

完整复现入口：

```bash
dotnet run --file scripts/csharp/verify-release-candidate.cs -- --tag v1.0.0-preview.8
```

各质量门禁与消费者命令见[当前状态](./current-status.md)。候选入口归档日志、TRX、覆盖率、包与最终报告；公开发布由 [Publish NuGet From Ref](../../.github/workflows/nuget-publish-ref.yml) 在指定 tag 上执行质量与包消费者验证后上传。所有包版本来自 tag，本目标不在项目文件写版本。

## 范围边界

DOM 扩展由 `ECMAScript.Vue` 程序集提供，作者导入 `ECMAScript`；原生接口继续由 WebIDL 拥有。事件表面覆盖 Change、InputFileChange、Mouse、Keyboard、Focus、Touch、Clipboard、Error、Progress、Pointer、Wheel、Drag。现代共有属性直接映射原生属性，`NativeEvent`、`NativeDataTransfer`、`NativeItem`、`NativeTouch`、`NativeElement` 都投影原对象。这些契约用于 DOM 来源 callback，不扩展到路由、验证等合成事件；`CurrentTarget` 和临时 clipboard/drag payload 应在 callback 中、首次 await 前读取。

CLR `DataTransfer.Files`/`Items` 仍不支持，使用 `NativeFiles: FileList`、`NativeItems: DataTransferItemList`；CLR `DataTransferItem` 支持 Alias 和 `Kind`/`Type` getter，setter 与构造仍拒绝。Touch 原有数组 getter 的 `Array.from` 行为保留，原生集合使用 `NativeTouches`、`NativeTargetTouches`、`NativeChangedTouches`。独立名称避免实例 DTO 成员优先于 extension 的 C# 绑定冲突。文件入口不引入 `IBrowserFile` / CLR Stream 模拟。

新增 DOM 回归使用真实 Vue 3.5.42 与 Happy DOM 20.10.6。Happy DOM 的 FileList/TouchList 和部分 event initializer 不完整，夹具补齐对应字段和列表形状；这些用例证明 DOM 仿真行为。Windows SPA/SSR 与参考应用的真实 Chrome 门禁证明各自消费者场景，不据此宣称每个新增 DOM 事件都已完成真实浏览器 smoke。Element Plus typed interaction 使用真实组件验证选项、行选择与 scoped slot，command/sort/service 成功路径使用 stub；服务取消和完整 CSS/browser smoke 仍遵循[绑定 README](../../src/ECMAScript.ElementPlus/README.md)的范围。

G4/G9 的依赖闭包与程序集副本修复保持。G15–G18、G20–G26、G30 属于应用、后端或浏览器环境，不增加框架兼容层；embedded-mjs 的 `@scope` 与过期 checksum 问题仍需具体复现。G6 的 `Headers.Get` 已有 WebIDL 绑定；G10 使用现有 named export object + extern member 习语，见[包绑定指南](../03-guides/js-resource-binding.md)，不新增 SmCrypto 包。G19 的同源 iframe 使用现有 `SecurityHeaders.XFrameOptions = "SAMEORIGIN"`；富文本选择与错误工作簿响应由应用契约决定。P3-A/B/C 与 Monaco 未完成的浏览器/真实 RazorVue consumer 边界保持。

## 评审补缺

以下五个问题均由回归复现、修复，并纳入通过的完整门禁：

- Inline getter 空条件访问：`read()?.Files` 及 `read()?.NativeEvent.Type/ComposedPath()` 对整链做 nullish guard，接收者只求值一次。
- Progress CLR long carrier：`Loaded`/`Total` getter 转为 BigInt，真实 ProgressEvent 的加减运算正确；`NativeEvent` 保留 WebIDL Number。
- 外部组件默认值覆盖：先判断无条件显式覆盖，再验证和翻译仍然生效的 initializer。
- Development SSR：优先当前源码图，Production 优先自足 bundle。
- 子路径包完整性：Base64 摘要精确比较，大小写冲突在输出写入前报告。

评审修复未新增作者 API，CLR whitelist 已按源码重新生成。
