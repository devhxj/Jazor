# 1.0 公共 API 基线清单

本文是冻结审查的人工可读基线。它覆盖 1.0 最容易被应用直接引用的宿主、SSR、开发 reload 和 `Jazor.Admin` 类型；编译器与生态绑定的完整签名仍以发布候选程序集的 API 兼容性工具结果为准。机器快照会排除 Razor 生成的下划线前缀辅助类型，例如 `_Imports`。

## 包与命名空间

| 包 | 稳定程序集/命名空间 |
| --- | --- |
| `Jazor` | `Jazor`, `Jazor.AspNetCore`, `Jazor.AspNetCore.Dev` |
| `Jazor.Vue` | `Jazor.RazorVue`, `ECMAScript.Vue*` |
| `Jazor.Admin` | `Jazor.Admin` |

## `Jazor.AspNetCore`

### 扩展入口

- `JazorWebApplication.CreateBuilder(string[] args, CallerFilePath)`
- `JazorExtensions.UseJazorHost`
- `JazorExtensions.UseJazorSecurityHeaders`
- `JazorExtensions.UseJazorStaticFiles`
- `JazorExtensions.UseJazorArtifacts`
- `JazorExtensions.UseJazorAssets`
- `JazorExtensions.UseJazorSpaFallback`
- `JazorSsrExtensions.AddJazorSsr`
- `JazorSsrExtensions.UseJazorSsr`

### 配置与协议类型

- `JazorHostOptions`
- `JazorSecurityHeaderOptions`
- `JazorAssetOptions`
- `JazorArtifactOptions`
- `JazorSpaFallbackOptions`
- `JazorSsrOptions`
- `IJazorSsrRenderer`
- `JazorSsrProvider`
- `JazorAuthenticationState`
- `JazorAuthenticationStatus`
- `JazorAuthenticationEnvelope`
- `JazorSsrRequest`
- `JazorSsrStateEnvelope`
- `JazorSsrRenderResult`

## `Jazor.AspNetCore.Dev`

- `JazorFrontendExtensions.AddJazorFrontend(WebApplicationBuilder, Action<JazorFrontendOptions>?)`
- `JazorFrontendExtensions.AddJazorFrontend(IServiceCollection, Action<JazorFrontendOptions>?)`
- `JazorFrontendExtensions.UseJazorPathBase(WebApplication)` / `UseJazorPathBase(IApplicationBuilder)`
- `JazorFrontendExtensions.UseJazorFrontend(WebApplication, Action<JazorHostOptions>?)` / `UseJazorFrontend(IApplicationBuilder, Action<JazorHostOptions>?)`
- `JazorFrontendOptions`
- `JazorViteServerOptions`
- `JazorFrontendUrls`
- `JazorReloadExtensions.AddJazorReload`
- `JazorReloadExtensions.UseJazorReload`
- `JazorReloadOptions`
- `JazorHmrMapping`

统一 frontend API 在 Development 通过 DenoHost 管理 Vite 生命周期并代理 HTTP/HMR，在其他环境托管 Release 产物。`UseJazorReload` 是独立的低层 reload/HMR 能力，在非 Development 环境保持 no-op。

## `Jazor.Vue` 原生事件与载荷扩展

`Jazor.Vue` 通过 `ECMAScript.Vue` 程序集交付 `ECMAScript.NativeFileEventExtensions`。引入 `ECMAScript` 命名空间后，既有 Blazor 事件类型可直接访问以下 extension 属性：

| C# 事件类型 | 扩展属性 | 返回类型 |
| --- | --- | --- |
| `InputFileChangeEventArgs` | `Files` | `FileList` |
| `DragEventArgs` | `Files` | `FileList?` |

输入变更事件的 `Files` 对应原生 `target.files`，拖放事件的 `Files` 对应 `dataTransfer?.files`；应在事件回调中、首次 `await` 前读取。`FileList`、`FileRef` 仍由 WebIDL 定义；这些 extension 只在已有 C# 事件类型上追加映射，不改变浏览器接口的生成归属。CLR 文件事件 adapter 是编译映射实现，不增加独立作者 API，也不引入 `IBrowserFile` / CLR Stream 模拟。

`NativeDomEventExtensions` 同时覆盖 `ChangeEventArgs`、`InputFileChangeEventArgs`、`MouseEventArgs`、`KeyboardEventArgs`、`FocusEventArgs`、`PointerEventArgs`、`WheelEventArgs`、`DragEventArgs`、`ClipboardEventArgs`、`TouchEventArgs`、`ErrorEventArgs`、`ProgressEventArgs`。每种事件提供精确返回类型的 `NativeEvent`，并增加缺少的共同 `Target`、`CurrentTarget`、`EventPhase`、`Bubbles`、`Cancelable`、`DefaultPrevented`、`Composed`、`IsTrusted`、`TimeStamp` 属性。鼠标派生家族复用基类 extension；`RelatedTarget`、`ClipboardData`、pointer 压力/角度与 UI `View` 等保持 WebIDL 类型和 nullability。

`NativeBrowserPayloadExtensions` 提供以下同类投影：

| 已有 C# 类型 | 新增原生入口 | 契约 |
| --- | --- | --- |
| `DataTransfer` | `NativeDataTransfer`、`NativeFiles`、`NativeItems` | `ECMAScript.DataTransfer`、`FileList`、`DataTransferItemList` |
| `DataTransferItem` | `NativeItem` | WebIDL `DataTransferItem`；CLR `Kind`/`Type` getter 也已映射 |
| `TouchPoint` | `NativeTouch`、`Target`、半径/压力/角度属性 | WebIDL `Touch`；`Target` 为 `EventTarget` |
| `ElementReference` | `NativeElement` | WebIDL `HTMLElement` |

`NativeDomEventExtensions` 还为 `ChangeEventArgs`/`InputFileChangeEventArgs` 增加 `Type`，为 `TouchEventArgs` 增加 `NativeTouches`、`NativeTargetTouches`、`NativeChangedTouches`；它们返回原生 `TouchList`，CLR 三个数组属性保留复制行为。

`DataTransfer` 的 CLR `Files:string[]` 和 `Items:DataTransferItem[]` 与原生列表不同，extension 不能覆盖实例成员，故使用 `NativeFiles`/`NativeItems`。原生方法及旧属性通过精确的 `NativeEvent`/载荷投影访问。`System.EventArgs`、导航和验证等合成参数不具有 DOM carrier，不增加通用原生映射。

## `Jazor.Admin`

- `AdminRouteDefinition`、`AdminRouteCatalog`
- `AdminLayoutState`
- `AdminLayoutMode`、`AdminThemeMode`
- `AdminPageActionKind`、`AdminPageAction`
- `AdminBreadcrumbItem`
- `AdminNavItem`、`AdminNavItemsCollectionBuilder`

`Jazor.Admin` 的页面、表单、表格和认证业务模型仍属于应用层，不进入公共壳 API 基线。

## ECMAScript 绑定命名

ECMAScript 浏览器绑定保留浏览器标准运行时名称；当标准名称与 C# 常用类型冲突时使用 `Ref` 后缀。当前冻结前迁移表如下：

| 旧名称 | 新名称 |
| --- | --- |
| `JazorFile` | `FileRef` |
| `JazorDocument` | `DocumentRef` |
| `JazorWindow` | `WindowRef` |
| `JazorHistory` | `HistoryRef` |
| `JazorEvent` | `EventRef` |
| `JazorLocation` | `LocationRef` |
| `JazorPropertyKey` | `PropertyKeyRef` |
| `JazorPropertyDescriptor` | `PropertyDescriptorRef` |

这些名称只影响 C# 作者侧 API；生成的 JavaScript/WebIDL ABI 名称保持浏览器标准名称不变。

## preview.8 绑定契约修正

浏览器标准接口仍由 WebIDL 生成；`extension` 只给已有 C# 类型追加宿主映射。`IWindow.LiveLocation` 是此类扩展，返回原生 `LocationRef`；`WindowRef.Location` 继续是 WebIDL 属性，`IWindow` / `WindowProxy` 的既有 `Location` 契约继续保留。

以下 14 个属性从错误的 `List<string>` 修正为 WebIDL `DOMTokenList`：

| 类型 | 属性 |
| --- | --- |
| `Element` | `ClassList`, `Part` |
| `HTMLAnchorElement`, `HTMLAreaElement`, `HTMLFormElement`, `SVGAElement` | `RelList` |
| `HTMLFencedFrameElement`, `HTMLIFrameElement` | `Sandbox` |
| `HTMLLinkElement` | `Blocking`, `RelList`, `Sizes` |
| `HTMLOutputElement` | `HtmlFor` |
| `HTMLScriptElement`, `HTMLStyleElement` | `Blocking` |

迁移时把显式 `List<string>` 变量改为 `DOMTokenList` 或 `var`，把 `.Count` 改为 `.Length`，按索引读取改为 `.GetItem(index)`；添加、删除和切换 token 使用 `.Add(...)`、`.Remove(...)`、`.Toggle(...)`。这些操作直接映射浏览器的 live token list，不再模拟 CLR 列表。

`Number` 新增到 `long` / `ulong` 的显式转换；它们进入 BigInt 值域，不能恢复源 Number 已丢失的整数精度。精确 ID 的 JSON 策略见 [Browser Interop](./browser-interop.md)。

Element Plus 新增 `ElTypedSelect<TValue>`、`ElTypedOption<TValue>`、`ElTypedTable<TRow>`、`ElTypedTableColumn<TRow>`、`ElTypedDropdown<TCommand>`、`ElTypedDropdownItem<TCommand>`，保留原组件的 runtime export/import。反馈服务通过 `ElMessage.Service`、`ElMessageBox.Service`、`ElNotification.Service` 暴露强类型选项、返回 handle 和 promise。

`ElTableColumn.ChildContent` 现在显式接收 `ElTableSlotContext`；typed column 接收 `ElTableSlotContext<TRow>`。直接给原 `ChildContent` 赋 `RenderFragment` 的 C# 代码需改为接收 context 的 `RenderFragment<ElTableSlotContext>`。Razor 中用 `Context="cell"` 访问 `cell.Row`、`cell.Column` 和 `cell.Index`，使默认单元格 slot 的 payload 与 Element Plus 一致。

## 冻结规则

本清单的新增、删除或重命名同步更新 [1.0 公共 API 冻结审查](./public-api-freeze.md)、测试和 `CHANGELOG.md`。发布候选从构建后的程序集生成机器可比较的签名快照，其差异作为 1.0 放行证据；机器兼容性检查提供正式比对结果。
