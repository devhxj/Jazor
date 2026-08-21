# RazorVue Blazor CLR 类型支持计划

> 状态：规划中。基线为已发布的 `v0.19.0`；只有已合并并通过对应门禁的能力才能标记为 Support。
>
> 定位：这是 [RazorVue 开发者体验完善路线图](./razorvue-developer-experience.md) 中浏览器运行时类型与服务的专项实施计划。它不试图把整个 ASP.NET Core/Blazor runtime 映射到 JavaScript。

## 1. 目标与范围

RazorVue 的目标是让标准 Blazor 作者面在浏览器中保持可观察行为，而不是复制 server renderer、circuit 或完整 CLR 对象模型。本计划只覆盖普通组件 C# 逻辑确实需要消费、且能够在浏览器中建立稳定 carrier 与行为合同的类型：

- 导航拦截所需的 `ValueTask`、`LocationChangingContext` 和关联注销/取消协议；
- DOM 事件参数对象：原生 DOM event 作为 carrier，`Jazor.CLR` 负责 Blazor 成员投影；
- `ElementReference` 的浏览器操作；
- 受控的 JS interop 对象与回调协议；
- 浏览器认证状态；
- 表单编辑、验证与文件输入所需的值对象。

以下内容不因名称属于 Blazor 就进入 `Jazor.CLR`：`ComponentBase`、`EventCallback`、`RenderFragment`、`ParameterView`、`RenderTreeBuilder`、renderer/circuit 基础设施和标准组件组合协议。它们分别属于 `Jazor.RazorVue` 的 current-component lowering、render emitter 或 runtime adapter。

### 1.1 当前基线

| 范围 | `v0.19.0` 状态 | 本计划中的位置 |
| --- | --- | --- |
| `NavigationManager` 基础导航、`LocationChangedEventArgs`、`NotFoundEventArgs`、URL-backed `System.Uri` | 已支持 | 作为导航拦截切片的基础，不重复实现 |
| `System.Threading.Tasks.Task`、`Task<TResult>` | 已支持，以 Promise 为 carrier | P0/S3/S5 复用既有 async 基础；本计划不因新切片扩张其成员面 |
| `NavigationManager.RegisterLocationChangingHandler(...)`、`LocationChangingContext` | 未支持 | P0：导航拦截 |
| 非泛型 `System.Threading.Tasks.ValueTask` | 未支持 | P0：导航拦截的异步 carrier 前置条件 |
| `ChangeEventArgs`、`MouseEventArgs`、`KeyboardEventArgs`、`FocusEventArgs` | Razor SG 可绑定 handler，Vue 可传递原生 event；尚无已发布的 CLR member projection | P1：核心事件 |
| `@ref` capture | 已由 render emitter 处理 | P1：只补受控的元素操作，不把 capture 再做成普通 CLR renderer API |
| `IJSRuntime`、`IJSObjectReference`、`AuthenticationStateProvider` 的默认可执行宿主 | 未支持 | P2：宿主协议，不是单独 Alias |
| `EditContext`、`FieldIdentifier`、`ValidationMessageStore`、`InputFile` | 基础 `EditForm`/常见 input adapter 已存在，但完整编辑和验证语义未支持 | P3：表单垂直切片 |

本表只以已发布、已合并的代码和测试为依据。开发中的本地改动、白名单草稿或尚未通过 browser/package 验证的实现不改变上述状态。

### 1.2 支持等级与运行 profile

本计划沿用 M5 的支持等级：

| 等级 | 含义 |
| --- | --- |
| Direct Support | 标准 C# API 直接映射，浏览器行为已被证明。 |
| Compatibility Adapter | 作者源码不变，RazorVue runtime 吸收浏览器和 Blazor 的实现差异。 |
| Guided Adaptation | 无法保真，但存在明确、强类型的浏览器替代。 |
| Reject | 无稳定浏览器语义或会破坏确定性，必须在使用点失败。 |

每个切片必须分别标明适用 profile：

| Profile | 规则 |
| --- | --- |
| Browser interactive | 本计划的主目标；必须有真实浏览器回归。 |
| SSR/prerender + hydration | 仅在切片显式完成 payload、一次性副作用和 hydration 时序证明后支持。 |
| Interactive Server / server-hosted reference | 只作为 Blazor 行为 oracle 和 API 迁移参考，不扩大 RazorVue 浏览器支持范围。 |
| Static/non-interactive render | 仅支持没有交互副作用的页面输出；事件和交互指令必须有明确诊断或对应的 render-mode adapter，不能静默输出看似可点击但没有处理器的 DOM。 |

## 2. 所有权与实现约束

```text
标准 Razor/C# 作者代码
  -> official Razor Source Generator
  -> Jazor.RazorVue: 识别 Vue/DOM/组件边界，建立 runtime bridge
  -> Jazor.Compiler / SemanticWalker: C# 调用、成员、类型、导入和失败裁决
  -> Jazor.CLR: 已声明 CLR 成员到浏览器 carrier/helper 的映射
  -> Jazor.Emit: .mjs、source map、manifest、bundle 物化
```

| 层 | 本计划中的责任 | 禁止的做法 |
| --- | --- | --- |
| `Jazor.CLR` | CLR 类型/成员的 `Alias`、`Inline`、`Import` 或 `Compile` 声明；新增 runtime helper 以 C# 写在 `[ECMAScriptModule]` 模块中并由现有管道编译；DOM event 的 Blazor 成员投影 | 用 `object` 或长 Inline 模板伪造复杂 host 协议，或为领域状态机新增手写 `.mjs` |
| `Jazor.Compiler` / `SemanticWalker` | 所有 C# 表达式、调用、成员访问、导入收集和使用点失败 | 对未映射外部成员静默发射原始 JavaScript |
| `Jazor.RazorVue` | Vue listener 的原生 event 传递、`@ref` 生命周期、Vue `provide`/`inject`、组件/路由/表单 framing | 为每种 `EventArgs` 手工构造 payload，用手拼 JS 替代 C# 成员/函数语义 lowering，或把导航、认证、表单状态机新增到既有 hand-written runtime `.mjs` |
| `Jazor.Emit` | 产物和 runtime closure 物化 | 在 RazorVue 中直接写入文件或绕过 manifest |

所有类型必须以完整垂直切片交付。一个类型仅有白名单 key、一个空对象 Alias，或仅能通过 Razor 编译，都不构成 Support。

每个切片还必须遵守以下不变量：

1. 保持求值顺序、副作用次数、异常传播和 async 完成时机；不能以“生成的 JS 更短”为由改变行为。
2. 浏览器 carrier 是实现细节，不可把它误宣称为完整 CLR runtime identity；无法可靠判定的 `is`/`as`/`typeof` 必须显式失败。
3. 不引入任意字符串执行、开放 `object` 参数、动态 import 或服务器 API fallback。
4. CLR whitelist 源变更后必须重新运行 `Jazor.Compiler.Generator`，并提交生成的 `WhiteList.cs.Generate.cs`。
5. 新能力改变消费者可使用的 API 面，应按 [发版与版本规则](../03-guides/release-and-versioning.md) 进入 `MINOR`，而不是 PATCH。
6. 实现路径按以下顺序选择并记录原因：C# 类型系统与既有 WebIDL binding、`[Jazor]` 声明和 whitelist、短 `Alias`/`Inline`、C# 编写的 `[ECMAScriptModule]` `Import` helper，最后才是确有 AST 级语义需要的 compiler `Compile`。不能跳过前一层而直接新增 runtime glue。
7. 本计划新增的领域 runtime 行为一律以 C# 写入 `Jazor.CLR` 模块，再由现有管道编译为产物；不得新增 hand-written `.mjs`。现有 RazorVue `.mjs` 只保留 Vue 生命周期、渲染 framing 和到 C# 模块入口的薄转发，不承载新增状态机或成员语义。
8. 内部 runtime 对象布局遵循 [CLR Runtime Object 布局调整计划](./clr-runtime-object-layout-plan.md)：匿名 object、structural record、原生 browser carrier、closure 和 `WeakMap` 是默认实现；只有已证明需要未知 `object` nominal type identity 的场景才能保留命名 runtime class，且不得以 `__jazorType` 或任何平行 tag 协议补回身份。

## 3. P0：导航拦截与异步 carrier

### 3.1 交付目标

让组件可使用标准 `NavigationManager.RegisterLocationChangingHandler(Func<LocationChangingContext, ValueTask>)` 阻止或观察内部导航，并得到与浏览器 history 交互一致的注销和异步行为。

| 类型/API | 目标支持面 | 明确边界 |
| --- | --- | --- |
| `System.Threading.Tasks.ValueTask` | 无参构造、`CompletedTask`、由 `Task` 包装、`AsTask`、`Preserve`、`await` 所需 awaiter/configure 路径 | 不承诺精确 `Task`/`ValueTask` runtime 类型识别、相等性、`IValueTaskSource` 池化协议或所有状态查询成员 |
| `System.Threading.Tasks.ValueTask<T>` | 不属于本 P0 的必做项；只在有已批准的强类型返回 API 时单独设计 | 不能因未来 JS interop 需要而先做无约束泛型 Promise Alias |
| `LocationChangingContext` | `TargetLocation`、`HistoryEntryState`、`IsNavigationIntercepted`、`CancellationToken`、`PreventNavigation()` | 不能只构造普通对象后遗漏导航提交点读取取消结果，也不能把公开的 token getter 留成未映射成员 |
| `CancellationToken`、`CancellationTokenSource`、`CancellationTokenRegistration` | `LocationChangingContext.CancellationToken` 的可观察取消、注册和注销；快速重复导航、back/forward replay 需要时作为同一协议闭环交付 | 不把 token 仅映射为布尔字段；注册、注销和一次性取消必须构成闭环 |
| 返回的 `IDisposable` | `Dispose()` 取消当前 handler 注册，且重复 dispose 不造成额外副作用 | 不把 handler 注册伪装成 field-like event |

### 3.2 必须先确认的参考行为

实现前为下列问题建立标准 Blazor reference fixture；未知行为不能由当前 Vue runtime 猜测：

- 多个 handler 的调用顺序、同步副作用顺序、异步完成顺序和异常传播；
- 任一 handler 调用 `PreventNavigation()` 后，剩余 handler 与最终导航提交的行为；
- handler 在执行期间触发新导航时，旧 context 的取消与最终 URL；
- 被后续导航取代的 context 何时触发 `CancellationToken`，以及已注册回调与 handler completion 的先后顺序；
- `NavigateTo`、`NavLink`、浏览器 back/forward、hash/history state、外部 URI 与 `forceLoad` 的差异；
- 注册句柄 dispose 后的行为、组件 unmount 后是否仍保留 handler；
- `IsNavigationIntercepted` 和 `HistoryEntryState` 的实际值来源。

### 3.3 实施顺序

1. 在 `Jazor.CLR` 以 Promise carrier 实现非泛型 `ValueTask` 的最小可观察面，并为不可保真的身份/比较成员保留 `Op.Discard`。
2. 以 `Jazor.CLR` 的 C# `[ECMAScriptModule]` 定义 `LocationChangingContext` 成员和 `PreventNavigation()` helper；复杂队列、取消和 commit 决策使用该模块的 `Import`，不压缩进 Inline，也不新建手写 `.mjs`。
3. 在 `System/RuntimeModule.js` 增加 C# 编写、仅供 CLR module 内部使用的 lifetime/subscription primitive：subscription 以 `{ dispose }` closure 表示，lifetime 以 opaque `object` 为 `WeakMap<object, LifetimeState>` key，并以强类型 WebIDL `EventTarget` 注册和移除 browser listener。它拥有可幂等释放的 cleanup stack，不产生 whitelist key，也不保存 URL、route、Vue component、VNode、slot 或业务状态；它只解决“谁拥有 listener、何时一次性释放”的跨切片生命周期问题。不得为此新建 `JRuntimeLifetime` 或 `JBrowserSubscription` nominal class。
4. `NavigationManagerModule` 组合该 primitive 形成 C# 编写的 navigation runtime host：host 是 opaque `object`，私有状态置于 module-owned `WeakMap`，它创建 `NavigationManager` carrier，订阅/释放 `popstate` 与 `hashchange`，并以一个显式 refresh callback 向宿主报告已提交的位置变化。注册、注销、handler dispatch、取消、commit 和 replay 仍由同一导航状态机拥有；browser `History` 操作优先复用 WebIDL binding。认证、表单等后续切片只能复用 lifetime 的所有权/释放约定，不能复用或改写 navigation state。
5. `blazor-routing.mjs` 对 S1 仅创建/释放 host、将其 `NavigationManager` 放入 `provide`，并保留 Vue 组件的路由渲染与 `NavLink` 意图调用；`popstate`/`hashchange` 的 browser 订阅完全归 `NavigationRuntimeHost`，不得在 `.mjs` 再注册或转发。既有路由解析和 `NavLink` framing 保持其现有归属，但不得在此新增 dispatch、取消、commit 或 replay 状态机；无法取消的 browser event 必须按 reference fixture 选择 replay/restore 或明确 Guided Adaptation，不能假装已经拦截。
6. 只有 browser、Release package 和适用 SSR/hydration 行为一致后，才把 API 从 Discard 改为 Support。

### 3.4 验收

- CLR metadata 与 runtime 场景：默认/已完成/失败 `ValueTask`、`LocationChangingContext.CancellationToken` 的取消/注册/注销、handler 注册/注销、`PreventNavigation`、异常和重复 dispose。
- compiler emission：直接调用、`await`、返回 `ValueTask` 的 lambda、`IDisposable.Dispose()`；所有 import 名和 alias 稳定。
- Razor SG/browser：组件注册 handler 后完成允许导航、阻止导航、back/forward、组件卸载和快速连续导航。
- 实现归属：跨切片 listener owner/cleanup 位于 C# `RuntimeModule` 的 opaque object/closure/`WeakMap` lifetime primitive，新增导航状态、dispatch、取消、commit/replay 位于 C# `NavigationManagerModule` 的 opaque host 和私有 state；host 必须能证明 mount 仅订阅一次、unmount 通过同一 lifetime 释放全部 browser listener。RazorVue runtime 的变更至多是创建/释放 host、`provide` 和直接转发到模块入口，不能新增 hand-written `.mjs` 状态机。
- Release package：runtime module 进入真实 consumer 的 closure，且未使用该切片的应用不会被无条件物化。

## 4. P1：核心 DOM 事件参数

### 4.1 CLR-first：原生 carrier，不造 EventArgs payload

默认路径不在 RazorVue 重新组装 Blazor event object。Vue listener 本来就以真实 DOM event 调用 handler，而 `EventCallback.Factory.Create<T>` 的当前 lowering 已把 callback 变成编译后的 C# handler。因此首批事件的运行路径应保持为：

```text
Vue onClick/onKeydown
  -> native DOM Event
  -> compiler-lowered C# handler(event)
  -> Jazor.CLR member mapping
  -> event.clientX / event.key / event.type ...
```

`T` 仍由 Razor SG/Roslyn 用于 C# 绑定；JavaScript 调用点只传一次真实 event。除 `ChangeEventArgs` 的单点捕获外，这样不需要通用的 `RenderEmitter` 事件类型 descriptor 表、不需要 per-event listener wrapper，也不需要把 DOM object 复制成一个 PascalCase payload。

每个 Blazor `EventArgs` 类型声明为 `Op.Alias(..., "Object")`，其只读成员由 `Jazor.CLR` adapter 映射到 WebIDL event 的 camelCase 字段。adapter receiver 应使用已有的 `ECMAScript` WebIDL 类型，例如：

```csharp
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.MouseEventArgs", "Object")]
public static class MouseEventArgsModule
{
    [Jazor(Op.Alias,
        "Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientX.get",
        "clientX")]
    public extern static double ClientX(MouseEvent instance);
}
```

这里的 `MouseEvent` 仅是 CLR adapter 的实现签名，carrier 仍是浏览器给 Vue 的原生对象。现有 carrier inference 只会把 `Jazor.CLR` 内部类视为 runtime value carrier；不能为了让 generator 推断 carrier 而额外包一层 `JMouseEvent`，因为真实 DOM event 不会是该包装类的实例。

因此第一版明确保持以下边界：构造器和 setter 为 `Op.Discard`，`is`、`as`、`typeof(EventArgsType)` 也不提供 runtime identity。事件参数是传入 handler 的只读投影，不是可由作者构造、修改或进行 CLR 身份判断的 POCO。未来只有出现具体作者场景并有 reference fixture 时，才评估以 CLR sidecar 实现某个可观察写入语义；不预先建立通用 overlay/proxy。

`MouseEventArgs`、`KeyboardEventArgs` 和 `FocusEventArgs` 的 DOM-origin callback 路径是 Direct Support，包含标准 DOM attribute 和把同一个 native event 原样向上转发的组件 adapter。`ChangeEventArgs` 因为必须在事件时刻保存 value，属于 Compatibility Adapter；它不改变另外三类的直接映射性质。普通组件 `EventCallback<T>.InvokeAsync(...)` 可以携带任意自定义值；当它使用 `new MouseEventArgs(...)`、成员初始化或其他合成 event object 时，不能由 native DOM carrier 自动实现，首版在构造/调用使用点拒绝。需要合成参数的组件必须作为单独的 component-emits 切片，显式定义其 CLR creator/carrier 与生命周期，不能借用 DOM 映射悄悄放行。

### 4.2 第一组类型与映射面

| 类型 | 支持等级 | 原生 DOM carrier | 首批 CLR getter alias | RazorVue 工作 |
| --- | --- | --- | --- | --- |
| `Microsoft.AspNetCore.Components.Web.MouseEventArgs` | Direct Support | `MouseEvent` | `Detail`、`ScreenX/Y`、`ClientX/Y`、`OffsetX/Y`、`PageX/Y`、`MovementX/Y`、`Button`、`Buttons`、`CtrlKey`、`ShiftKey`、`AltKey`、`MetaKey`、`Type` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.Web.KeyboardEventArgs` | Direct Support | `KeyboardEvent` | `Key`、`Code`、`Location`、`Repeat`、`CtrlKey`、`ShiftKey`、`AltKey`、`MetaKey`、`Type`、`IsComposing` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.Web.FocusEventArgs` | Direct Support | `FocusEvent` | `Type` | 无 wrapper，原样传 event。 |
| `Microsoft.AspNetCore.Components.ChangeEventArgs` | Compatibility Adapter | `Event` | `Value`，通过 CLR helper 读取已捕获的 change value | 只在 typed `ChangeEventArgs` handler 上调用一次 capture helper；见下一节。 |

数值在 JavaScript 中统一是 `Number`，因此 `int`/`long`/`float`/`double` 的不同不会要求 payload 转换；C# 的静态签名和 Razor SG 继续负责作者侧类型检查。首批 read surface 覆盖这些类型的全部公开实例 getter。未列出的 setter、构造器和 runtime identity 不是遗漏，而是显式不支持的语义边界。

### 4.3 `ChangeEventArgs.Value`：唯一需要事件边界捕获的核心例外

原生 `Event` 没有顶层 `value`，而且 `event.target.value` 在 async handler 恢复前可能已经被用户后续输入改变。仅把 `Value.get` 映射成一次延迟的 `event.target.value` 读取会失去 Blazor 的事件时刻语义。

这里保留一个极小、CLR-owned 的 bridge，而不是构造 `ChangeEventArgs` payload：

```text
onChange: event => handler(captureChangeEvent(event))
                         |  returns the same native Event
                         |  stores the event-time value in a WeakMap

ChangeEventArgs.Value.get -> getChangeEventValue(event)
```

`captureChangeEvent` 与 `getChangeEventValue` 位于 C# 编写的 `Jazor.CLR` event `[ECMAScriptModule]`；实现复用 `WeakMap` 模式和 `HTMLInputElement`/`HTMLSelectElement` 等 WebIDL receiver，在 C# 控制流中完成输入、checkbox 与 select 的值塑形。RazorVue 只根据 Roslyn 已绑定的 `EventCallback<ChangeEventArgs>` 保留这一次调用，不了解字段形状，也不复制 object 或新增 `.mjs` helper。这是唯一一个类型定向的 listener 钩子，不是可扩展为通用 descriptor 表的协议。首批捕获规则必须用 Blazor reference fixture 固化：普通 input/textarea/select 为 string、checkbox 为 bool、`select[multiple]` 为 string array；file input 不借用此通道，进入后续 `InputFileChangeEventArgs`/`IBrowserFile` 切片。`@bind` 的直接赋值路径继续使用已有 value/checked 提取，不因支持 typed change handler 而创建 EventArgs carrier。

### 4.4 实施与验收

1. 在 `Jazor.CLR` 增加每个类型的 `Op.Alias(..., "Object")` 和 getter adapter；`MouseEvent`、`KeyboardEvent`、`FocusEvent` 等采用现有 WebIDL receiver 类型，所有 constructor/setter 明确留为 `Op.Discard`。
2. 重新运行 `Jazor.Compiler.Generator`，并在 `Jazor.CLR.Test` 断言 type alias、getter key、Op/path，以及这些 Object alias 不获得伪造的 runtime carrier。
3. 只为 `ChangeEventArgs` 增加 C# 编写的 CLR `Import` helper 与 RazorVue 的一次 capture 调用；不得引入泛化 event descriptor、payload class、每种事件各自的 listener wrapper 或 hand-written `.mjs` event helper。
4. 在 `Jazor.CompilerTest` 覆盖 C# property access 的 emission、未支持 setter/constructor/identity 的稳定失败，以及 import alias 的稳定性。
5. 在 official Razor SG/browser fixture 覆盖 method group、lambda、async handler、原样转发 native event 的组件 `EventCallback<T>`、`preventDefault`、`stopPropagation`、`@bind` 与 typed `@onchange` 共存。浏览器测试必须证明 async continuation 读取到的是触发时的 `ChangeEventArgs.Value`，而不是之后修改的 DOM value；合成 `new EventArgs` 路径必须稳定失败。capture 调用插入后，source map 仍须指向作者 handler，而不是 CLR helper 或 listener bridge 内部。

## 5. P1：元素引用与焦点

`@ref` capture 已是 render emitter 的职责：VNode 的 ref callback 在元素创建/更新/卸载时把真实 DOM element 写入当前组件 state。它不需要也不应重新变成 RenderTree 或 renderer CLR 模块。

| API | 计划 | 边界 |
| --- | --- | --- |
| `ElementReference` | 将由 `@ref` 捕获得到的真实 DOM element 视为内部 carrier | 不支持用 `new ElementReference(...)` 伪造浏览器节点，也不承诺 `Id`/`Context` 的 server renderer 身份。 |
| `ElementReferenceExtensions.FocusAsync(ElementReference)` | 优先以短 `Inline` 调用 WebIDL `HTMLElement.Focus()`，并返回已完成的 `ValueTask`/Promise carrier | 仅处理由 `@ref` 捕获的真实 DOM element；不伪造 server renderer 身份。 |
| `ElementReferenceExtensions.FocusAsync(ElementReference, bool preventScroll)` | 优先以短 `Inline` 调用 `HTMLElement.Focus(FocusOptions)`，其中 `FocusOptions.PreventScroll` 由标准 bool overload 提供 | 不通过宽松 options `object` 替代标准 bool overload。 |

这两个 extension 成员首先走 `SemanticWalker -> Inline`：调用既有 WebIDL `HTMLElement.Focus(FocusOptions?)` binding，并复用已有 `ValueTask`/Promise carrier。只有出现短模板无法保持的可观察协议时，才升级为 C# 编写的 `[ECMAScriptModule]` `Import` helper；不为两个 overload 预先建立 runtime module。DOM node 生命周期和 Vue ref framing 仍由 RazorVue 处理。`scrollIntoView`、selection、measurement 等非标准 `ElementReference` API 应走已有或新增的强类型 WebIDL binding，不应借此把任意 DOM 方法塞进 CLR 模块。

验收覆盖同一元素重新渲染、条件卸载、组件 unmount、`OnAfterRenderAsync` 调用时机、`preventScroll`、短 Inline 的 `ValueTask` emission；若因已证明的复杂行为升级为 `Import`，再验证其 Release bundle closure。

## 6. P2：扩展 DOM 事件族

扩展事件沿用同一条 CLR-first 原则：Vue 继续传入原生 event，`Jazor.CLR` 将 Blazor property getter 映射到 native carrier。listener 层不得组装 payload 或为每个类型另建 normalizer；若 live browser object 必须转换为 CLR 值契约，只能由 CLR property helper 在该属性首次访问时完成，不能把物化前移到事件 listener。

| 类型组 | 原生 carrier / 依赖 | 交付要求 |
| --- | --- | --- |
| `PointerEventArgs` | `PointerEvent`，继承 `MouseEventArgs` getter slice | 以 getter alias 增加 pointer id、尺寸、压力、倾角、pointer type、primary。 |
| `WheelEventArgs` | `WheelEvent`，继承 `MouseEventArgs` getter slice | 以 getter alias 增加 `DeltaX`、`DeltaY`、`DeltaZ`、`DeltaMode`。 |
| `DragEventArgs`、`DataTransfer`、`DataTransferItem` | `DragEvent`、`DataTransfer` | `DragEventArgs.DataTransfer` 先映射为 native carrier；其 Blazor surface 单独建 CLR adapter，不能把不可用 DOM 方法或 File 对象伪装成普通 POCO。 |
| `ClipboardEventArgs` | `ClipboardEvent` | `Type` 可直接 alias；clipboard data 的权限/用户手势限制由 browser carrier 保持。 |
| `TouchEventArgs`、`TouchPoint` | `TouchEvent`、不可变 `TouchList`、`Touch` | `Touch` 以 CLR getter alias 投影为 `TouchPoint`；集合成员优先在属性首次访问时以短 `Inline` 的 `Array.from(...)` 转为 `TouchPoint[]`。`TouchList` 不可变，因此惰性转换仍读取同一事件值；不在 listener 时刻 materialize，也不预先新增 helper module。 |
| `ErrorEventArgs`、`ProgressEventArgs` | `ErrorEvent`、`ProgressEvent` | 公开成员都有稳定 native 来源时采用 getter alias；否则按成员拒绝。 |

当前 .NET 11 Blazor public reference surface 中没有独立的 `InputEventArgs` 或 `CompositionEventArgs` 类型。本计划不为不存在的 Blazor 契约创建自定义 CLR 类型；输入/组合事件先使用已存在的 `ChangeEventArgs` 或在未来 framework API 出现后重新评估。

扩展事件只在有真实 RazorVue 消费场景时推进。每个类型组独立成为一个 MINOR 能力切片，不因共享 WebIDL carrier 或 CLR helper 而自动获得 Support。

## 7. P2：受控 JS interop

### 7.1 为什么不能只添加接口 Alias

`IJSRuntime` 与 `IJSObjectReference` 的核心问题是“哪个 identifier 可以调用、它的参数/返回值如何编组、该模块由谁提供”，而不是接口在 JavaScript 中叫什么。当前组件注入机制能够按类型生成 Vue `inject(...)`，但内建 runtime 只有 `NavigationManager` provider；没有可执行的默认 JS interop 或认证 provider。因此单独给接口加 `Object` Alias 会生成无法解析的服务或开放任意动态执行。

### 7.2 目标合同

| 类型/API | 目标支持面 | 前置条件 |
| --- | --- | --- |
| `IJSRuntime` / `JSRuntimeExtensions` | 静态 whitelist/module contract 中声明的 `InvokeVoidAsync`、`InvokeAsync<TResult>` | identifier 为编译期可确定的 entry；参数和结果使用已声明的强类型投影。 |
| `IJSRuntime.ImportModuleAsync(...)` / `[ModuleImport]` | 获取静态 module specifier 对应的强类型 `IJSObjectReference` | module specifier 必须在编译期可确定并进入 manifest；不能把运行时字符串变成动态 import 逃生口。 |
| `IJSObjectReference` | 已声明模块/object 的受控 invocation 和 async dispose | import/module closure 进入 manifest，object lifetime 可追踪。 |
| `IJSInProcessRuntime` / `IJSInProcessObjectReference` | 仅对应静态 contract 中实际同步 browser binding 的调用 | 不把异步 Promise 假装同步返回。 |
| `DotNetObjectReference<T>`、`JSInvokableAttribute` | 已发现且可静态绑定的回调 entry | 不扫描程序集反射，不接受任意字符串回调。 |
| `ValueTask<TResult>` | 仅为已经批准的强类型 interop 返回路径新增 | 先定义 Promise carrier、类型投影与失败的 runtime identity 规则。 |

### 7.3 实施步骤

1. 将所谓 registry 定义为**编译期** typed module contract：宿主或绑定包以 `[Jazor]`/whitelist 声明 import specifier、`ImportModuleAsync` 的模块入口、可调用成员、强类型参数/结果投影和同步/异步属性。它是编译与 emit metadata，不得生成运行时 JS `Map`、字符串 lookup 或通用 dispatcher。
2. 对 const identifier，`SemanticWalker` 的 `Compile` 在调用点解析该 contract，收集直接 `ImportSpecifier`，再由 `Jazor.Emit` 将实际模块纳入 manifest/closure。`IJSRuntime` 实例只作为复用现有 `provide`/`inject` 的注入 facade，不拥有成员查找。
3. `ImportModuleAsync` 仅接受静态 specifier。若引入 `[ModuleImport]`，它必须是同一强类型 contract 内的静态 C# module declaration，并复用既有 import collection；当前没有该 attribute pipeline 时不得在计划中假定它已存在。动态 import 继续拒绝。
4. 在 `Jazor.CLR` 映射已批准的接口成员和受控模块获取入口；object lifetime、dispose 或 callback dispatch 若不能用短 Inline 保真，使用 C# 编写的 `[ECMAScriptModule]` `Import` helper 或必要的 `Compile` 协议，不使用泛型 `object[]` fallback 或 hand-written `.mjs` glue。
5. 在 authored-source analyzer 对未知 identifier、动态 identifier、未声明返回类型和 server-only interop 位置给出稳定诊断与强类型替代方向。
6. 最后再考虑 `DotNetObjectReference` 和 `JSInvokable`；它们需要 callback lifetime、实例释放和非反射发现协议，不能由 `InvokeAsync` 自动推导。

### 7.4 验收

- const identifier 必须在编译时解析为唯一的 whitelist/module contract entry 和直接 import；未知或动态调用没有 runtime `undefined`，也不留下运行时 registry lookup。
- 参数、返回值、异常、取消、静态模块获取、object dispose 和 module cache 有真实 browser 回归。
- 同步接口只在同步 registry entry 上可用；所有 Promise 路径在 C# 侧保持 `Task`/`ValueTask` 语义。
- Release package 只物化被 registry 使用的 module closure；SSR/hydration 另有明确 profile 证明。

## 8. P2：认证状态

认证不是把 `AuthenticationStateProvider` 映射为一个 JavaScript object 就完成。它需要浏览器可验证的状态来源、刷新通知、SSR handoff 以及与真实 endpoint 授权分离的契约。provider 的状态、订阅、刷新和 claims carrier 应以 C# 写在 `Jazor.CLR` 模块；RazorVue 只复用已有 `provide`/`inject`、cascade 和 component render framing，SSR payload 契约归 `Jazor.Emit`，不得另造 hand-written `.mjs` 认证状态协议。

| 类型/API | 目标支持面 | 边界 |
| --- | --- | --- |
| `AuthenticationStateProvider` | `GetAuthenticationStateAsync()` 与状态变更通知 | provider 必须由 host 注册；没有默认隐式 identity 服务。 |
| `AuthenticationState` | `User` 的最小可观察身份 carrier | 不宣称完整服务器 `ClaimsPrincipal` runtime 身份。 |
| `ClaimsPrincipal`、`ClaimsIdentity`、`Claim` | 仅为已批准的角色/claim 查询提供受控 carrier/member slice | 不引入任意 claims transformation、服务器 ticket 或安全决策 fallback。 |
| `CascadingAuthenticationState` | 作为认证组件 adapter，将 provider 的 `Task<AuthenticationState>` 以标准 cascading contract 提供给后代组件 | 不属于 CLR module；订阅、刷新和 unmount 必须与 `AuthenticationStateProvider` 使用同一生命周期协议。 |
| `AuthorizeView` / `AuthorizeRouteView` | 作为 RazorVue 组件 adapter 消费认证 state | 它们不是 CLR module；UI 隐藏本身不构成 endpoint 授权。 |

实施顺序：先确定 host 提供的 C# auth descriptor 来源和版本化 refresh 方式，再设计 claims carrier，然后在 CLR module 实现 provider/event，接入 `CascadingAuthenticationState`，最后增加 `AuthorizeView`/route adapter。两个组件 adapter 仅消费同一 lifecycle contract，不各自发明 JS state protocol。SSR profile 必须明确 payload 何时生成、何时失效、hydration 后是否重取；没有该协议时只支持 Browser interactive 或维持 Guided Adaptation。

验收至少包括 anonymous/authenticated 切换、role/claim 分支、`CascadingAuthenticationState` 向后代的初始值与刷新传播、provider refresh、组件 unmount、token/descriptor 过期后的可观察行为，以及 endpoint 授权不因 UI adapter 而被错误宣称为已覆盖。

## 9. P3：表单、验证与文件输入

基础 `EditForm`、`InputText`、`InputTextArea`、`InputCheckbox`、`InputNumber`、`InputDate` 和 `InputSelect` adapter 已解决部分 DOM 输入，但这不等于已实现 Blazor 表单状态机。本阶段必须以完整编辑上下文为单位推进。

| 类型/API | 目标支持面 | 非目标/依赖 |
| --- | --- | --- |
| `EditContext` | model、字段变更、validation state、`Validate()`、已证明的事件订阅 | 不能仅靠 model 上可选的 `validate()` 方法声称等价。 |
| `FieldIdentifier` | model identity + field name 的稳定键 | `Expression<Func<T>>` 的解析必须由编译期/已知 binding descriptor 完成，不做浏览器反射。 |
| `ValidationMessageStore` | add/clear/查询并通知 validation state | 消息归属、字段级与全局消息、重复/清理顺序必须有 reference fixture。 |
| `InputBase<T>` 与 `InputText`、`InputTextArea`、`InputCheckbox`、`InputNumber`、`InputDate`、`InputSelect`、`InputRadioGroup`、`InputRadio` | 用同一个 edit-context contract 更新 field 和 parse error | nullable、enum、date/culture、nested field 和自定义派生类要逐项声明。 |
| `ValidationMessage`、`ValidationSummary`、`DataAnnotationsValidator` | RazorVue component adapter | DataAnnotations 需要编译期 descriptor 或明确验证器合同，不能扫描整个程序集。 |
| `InputFileChangeEventArgs`、`IBrowserFile` | 基于 browser `File`/`Blob` 的文件元数据与读取流协议 | 先完成 `InputFile` adapter 和 File lifetime；不把文件内容无界复制为普通 object。 |

`EditContext` 与 `ValidationMessageStore` 的状态机、订阅和通知以 C# 写在 `Jazor.CLR` module；`FieldIdentifier` 的 expression 只采用编译期或已知 binding descriptor，DataAnnotations 只采用由 C# attribute 形成的编译期 descriptor。RazorVue 只承担 component/cascade/render framing，不得新增 hand-written `.mjs` 表单状态机、反射扫描或运行时 JS 验证表。

`FormName`、`AntiforgeryToken`、`[SupplyParameterFromForm]` 和 enhanced form post 需要 SSR/endpoint host contract，不属于浏览器 CLR module 的自然延伸；在该 contract 未完成前保持 Guided Adaptation 或 Reject。

表单验收必须同时覆盖：提交/取消、field changed、同步和异步 validation、parse failure、nested model、nullable/enum/date、清除消息、重复提交、组件卸载以及 server error 的显式交接。只验证表单标签能提交或输入值能写回不足以标记 Support。

## 10. 明确不进入本计划的类型

| 类型/领域 | 归属或处理方式 |
| --- | --- |
| `ComponentBase`、`IComponent`、`IHandleEvent`、`RenderHandle`、`Renderer`、`RenderTreeBuilder`、内部 RenderTree API | RazorVue component/runtime protocol；renderer/server 基础设施保持不支持。 |
| `EventCallback`、`RenderFragment`、`ParameterView`、component reference、slot | RazorVue lowering；只在已定义的 current-component/slot adapter 入口通过 compiler。 |
| `Router`、`RouteView`、`LayoutView`、`NavLink`、`NavigationLock`、`FocusOnNavigate`、`PageTitle`、`HeadContent`、`HeadOutlet` | 组件或 router adapter；不应因其参数使用 Blazor 类型而整体搬入 CLR。 |
| `AuthorizeView`、`AuthorizeRouteView`、`CascadingAuthenticationState` | 认证状态的消费者，属于 §8 的组件 adapter；不因可接受 `AuthenticationState` 而变成 CLR module。 |
| `Virtualize`、`QuickGrid`、`SectionContent`、`SectionOutlet`、`StreamRendering` | 独立的浏览器渲染/性能/SSR 项目，不能用占位 CLR types 冒充支持。 |
| `HttpClient`、`IHttpClientFactory` | 明确归 `Jazor.RazorVue` 的 browser endpoint-client / application-service adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；没有已声明 endpoint contract 时为 Guided Adaptation 或 Reject，不能映射服务器 `HttpClient` 或隐式 credential 行为。 |
| `IStringLocalizer`、`IStringLocalizer<T>`、资源本地化 | 明确归 localization + SSR state-handoff adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；在 culture、resource payload、fallback 与 hydration 未证明前为 Guided Adaptation 或 Reject。 |
| `ILogger`、`ILogger<T>` | 明确归浏览器 diagnostics / host logging adapter 与 authored-source diagnostic 线，不属于本 CLR 计划；不能把浏览器调用误宣称为服务器 logger，未注册 adapter 的注入或调用必须得到明确诊断。 |
| `HttpContext`、circuit/server service、`PersistentComponentState`、protected browser storage、数据库/Identity 管理服务 | server/SSR host 边界；没有浏览器等价 runtime。 |
| 反射、动态 Type、任意 JS text execution | 维持 Reject；无法通过“通用 object 映射”进入浏览器。 |

## 11. 实施顺序与依赖

| 顺序 | 可独立发布的能力切片 | 主要依赖 | 完成后允许宣称的支持 |
| --- | --- | --- | --- |
| S0 | API ledger、reference fixtures、diagnostic ownership | 在 `src/Jazor.RazorVue.Sg.Test/RazorVueUsageScenarioCatalog.cs` 新增 `BlazorClr` family/area 与 `RazorVueBlazorClrCapabilityLedger`（这是新增分区，不是现有 M5 row）。每个类型级 row 必须回链到唯一的 `RazorVueM5CapabilityLedger` owner，只细化类型/成员/carrier/profile，不能与父 row 产生冲突的支持状态；同时记录实现路径（WebIDL receiver、`Alias`/`Inline`、C# `Import` module 或 `Compile`）及其选择理由，不能把 hand-written `.mjs` 作为领域行为实现路径。同步更新 `RazorVueSemanticMatrixInventoryTests` 的 catalog count/coverage 断言，并新增 `RazorVueBlazorClrCapabilityLedgerTests` 断言 owner、profile、carrier、实现路径与 fixture 完整性。标准 Blazor reference fixture 新增于同项目的 `RazorSgBlazorClrReferenceFixtureTests`，RazorVue browser/runtime fixture 新增于 `RazorSgBlazorClrRuntimeTests` | 只有计划，不改变 API 状态 |
| S1 | 导航拦截：`ValueTask` + `LocationChangingContext` + 注册句柄 | `NavigationManager` 基础 runtime、可能的取消协议 | 受限的内部导航拦截 |
| S2 | 核心事件：Change/Mouse/Keyboard/Focus | WebIDL carrier、CLR member adapters；仅 `ChangeEventArgs` 需要一次性 value capture | 强类型高频 DOM handler |
| S3 | `ElementReference.FocusAsync` | `@ref` lifecycle、`ValueTask` carrier、WebIDL `HTMLElement.Focus`/`FocusOptions` | 受控元素焦点 |
| S4 | Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress 事件组 | S2、WebIDL/File carrier（按组） | 已完成组的强类型 handler |
| S5 | 编译期 typed module contract、`IJSRuntime` 首批 invocation 与静态模块获取 entry | S1 的 async carrier、host injection、现有 import collection、manifest closure | 已声明 identifier/module 的 interop |
| S6 | C# auth state/provider、`CascadingAuthenticationState` 与 `AuthorizeView` adapter | S5 的 host/provider 模式、auth descriptor contract | 浏览器 UI 认证状态 |
| S7 | C# `EditContext`/验证，随后 `InputFile` | S2、S5 或强类型 File binding、表单 descriptor | 已声明的表单/验证/文件行为 |

没有日历式发版目标。每个切片在标准语义 fixture、browser、package 及适用 profile 全部通过后，才进入下一次 MINOR；没有通过时保持计划状态或转为 Guided Adaptation/Reject。

## 12. 统一验收与发布清单

任一类型切片进入 Support 前，至少完成下列证据链：

1. **API ledger**：记录目标 framework 版本、类型/成员、profile、support level、carrier、依赖、明确排除项、实现路径及其选择理由和对应测试名。实现路径必须标明 WebIDL receiver、`Alias`/`Inline`、C# `Import` module 或 `Compile`；ledger 唯一落点为 `src/Jazor.RazorVue.Sg.Test/RazorVueUsageScenarioCatalog.cs` 的新增 `BlazorClr` family/area 和 `RazorVueBlazorClrCapabilityLedger`；每行必须回链一个 `RazorVueM5CapabilityLedger` owner，作为其成员级细化而非第二套状态。新增条目必须同步更新 `RazorVueSemanticMatrixInventoryTests` 的 catalog count/coverage 断言，并通过 `RazorVueBlazorClrCapabilityLedgerTests`。
2. **CLR metadata/runtime**：在 `src/Jazor.CLR.Test` 断言 type alias、member `Op`、module path 和 helper 行为。新增 runtime helper 必须能回链到 C# `Jazor.CLR` source，而非 hand-written `.mjs`；变更 whitelist 源后运行 `dotnet run --project src/Jazor.Compiler.Generator/Jazor.Compiler.Generator.csproj`。
3. **编译器 emission**：在 `src/Jazor.CompilerTest` 覆盖直接调用、成员访问、异常路径、async/await、interface/extension dispatch（存在时）和稳定 import。
4. **official Razor SG 集成**：在 `src/Jazor.RazorVue.Sg.Test` 使用真实 `.razor` 作者写法，验证 generated C# binding、RazorVue lowering 和 mapped diagnostic。标准 Blazor 行为 oracle 固定由 `RazorSgBlazorClrReferenceFixtureTests` 承载，RazorVue runtime/browser 语义由 `RazorSgBlazorClrRuntimeTests` 承载，避免把 reference 结果误当成浏览器 Support。
5. **真实浏览器**：验证 DOM、history、事件、生命周期、Promise/异常、unmount 和交互结果；不得只断言生成 `.mjs` 文本。若 RazorVue runtime 有改动，审查其仅为 framing/薄转发，新增领域状态和成员语义必须仍在 C# CLR module。
6. **交付**：至少确认 debug/release artifact；涉及 runtime import 的切片还要确认 isolated package consumer 的 closure。支持 SSR/hydration 时另行覆盖一次性副作用与状态 handoff。
7. **失败体验**：未支持的 member、动态值或 server-only 入口在作者源/实际使用点得到稳定诊断，绝不留下运行时 `undefined` 或部分 artifact。

完成某个切片后，更新 [当前状态](./current-status.md)、作者指南和 CHANGELOG 的面向用户行为描述；已发布版本章节不回写。文档、CLR mapping、RazorVue adapter 和测试必须同一提交评审，避免出现“文档称支持但 runtime 未注册”或“白名单已放行但没有浏览器 carrier”的灰区。

## 13. 决策门

每次新增类型前，维护者必须先回答：

1. 该类型是否有浏览器中的真实 carrier，还是只是在 server renderer 中存在？
2. 作者会在何处创建、接收、调用或比较该值？每个使用点是否可保真？
3. 这是 CLR member mapping、RazorVue bridge、host provider 还是 SSR handoff 问题？是否需要多个层共同完成？
4. 是否已依次尝试 C# 类型系统、既有 WebIDL binding、`[Jazor]`/whitelist 与短 `Alias`/`Inline`，而无需 `object`、动态 string 或额外 fallback？
5. 若前述路径不足，为什么必须使用 C# `[ECMAScriptModule]` `Import` helper 或 compiler `Compile`，且如何保持该逻辑不落入 hand-written `.mjs`？
6. reference fixture 是否已经说明 async、异常、生命周期和取消顺序？
7. 若答案是否定的，最诚实的结果是 Guided Adaptation 还是 Reject？

只有这些问题都有可验证答案时，类型才进入实现；“ASP.NET Core reference assembly 中存在该类型”不是支持它的充分理由。
