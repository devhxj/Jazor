# RazorVue 快速开始

这条路径从一个普通的 Razor 页面开始，最终得到 Vue render-function 模块。页面作者只需要
使用标准 Razor、`ComponentBase` 和强类型组件 contract；不需要阅读 `.razor.g.cs`、手写
`RenderTreeBuilder` 或拼接 JavaScript。

## 1. 建立页面

先运行仓库里的 [RazorVue.Authoring](../../samples/RazorVue.Authoring/README.md) sample，或从它的
[TaskBoard.razor](../../samples/RazorVue.Authoring/TaskBoard.razor) 和
[TaskBoard.razor.cs](../../samples/RazorVue.Authoring/TaskBoard.razor.cs) 开始复制。下面的片段
保持已验证 sample 的作者面；`TaskDraft`、`TaskRow` 和 `TaskTable` 的完整定义也在该 sample
中。TDesign 的泛型组件由 official Razor Source Generator 正常绑定：

```razor
@page "/tasks"
@using ECMAScript.TDesign

<TForm FormData="TaskDraft"
       Data="@Draft"
       Rules="@Rules"
       OnSubmit="@SubmitForm"
       OnReset="@ResetForm"
       OnValidate="@ValidateForm">
    <TFormItem LabelValue="Title" Name="title">
        <TInput T="string"
                Placeholder="Write the next task"
                @bind-Value="Draft.Title"
                @bind-Value:event="OnChange" />
    </TFormItem>
    <TFormItem LabelValue="Owner" Name="owner">
        <TInput T="string"
                Placeholder="Team or person"
                @bind-Value="Draft.Owner"
                @bind-Value:event="OnChange" />
    </TFormItem>
    <TButton Type="@TButtonTypeValue.Reset" Variant="@TButtonVariantValue.Text">Reset</TButton>
    <TButton Type="@TButtonTypeValue.Submit" Theme="@TButtonThemeValue.Primary">Save from form</TButton>
</TForm>
```

```csharp
private TFormRules<TaskDraft> Rules { get; } = new()
{
    ["title"] =
    [
        new TFormRule { Required = true, Message = "Add a title before saving." }
    ],
    ["owner"] =
    [
        new TFormRule { Required = true, Message = "Add an owner before saving." }
    ]
};

private void ValidateForm(TValidateResultContext<TaskDraft> context)
{
    ValidationMessage = context.FirstError ?? "Form is valid.";
    if (context.FirstError is not null)
        StatusMessage = "Fix the highlighted fields before saving.";
}

private void ResetForm(TFormResetEventContext<TaskDraft> context)
{
    Draft = NewDraft();
    ValidationMessage = "Form reset. Title and owner are required.";
    StatusMessage = "Draft reset.";
}

private async Task SubmitForm(TSubmitContext<TaskDraft> context)
    => await SaveDraftAsync();
```

`TForm<T>`、`TFormItem`、`TInput<T>`、`TButton`、typed `EventCallback` 和 `@bind` 都是公开
contract。字段规则、`OnValidate`、`OnReset`、`OnSubmit` 和 `TSubmitContext<T>` 让页面可以
表达字段错误、重置、提交中状态以及异步失败后的保留输入；组件库负责控件显示，页面或 endpoint
负责业务事实。

## 2. 绑定和状态

组件参数保持具体类型。常见的字段写法是 `@bind-Value` 加显式事件名：

```razor
<TInput T="string"
        Placeholder="Team or person"
        @bind-Value="Draft.Owner"
        @bind-Value:event="OnChange" />
```

提交处理器可以用 `Saving` 防止重复提交，并在 `try/finally` 中恢复状态；不要把模型改成
`object`，也不要把错误转换成字符串 JavaScript 调度。需要从服务端获取数据时，注入强类型的
browser client，让 endpoint 返回明确的 DTO。完整的 `SaveDraftAsync`、状态字段和生命周期代码
见 sample 的 [TaskBoard.razor.cs](../../samples/RazorVue.Authoring/TaskBoard.razor.cs)。

## 3. 路由和导航

`@page`、`@layout`、带约束的 route parameter 和 `[SupplyParameterFromQuery]` 会进入生成的
route catalog。应用自己的 host 负责把 catalog 条目渲染为页面和 layout：

```csharp
var uri = Navigation.BaseUri + "tasks?view=compact";
Navigation.NavigateTo(
    uri,
    new Microsoft.AspNetCore.Components.NavigationOptions
    {
        ReplaceHistoryEntry = true,
        HistoryEntryState = "authoring-replace"
    });
```

同一 base URI 的内部导航支持 `pushState`、`replaceState`、query/hash、history state、
`LocationChanged` 订阅和 `LocationChanging` 的取消/注销子集。`LocationChanged` 是普通 CLR
事件，页面在初始化时订阅，在 `Dispose` 中注销：

```csharp
protected override void OnInitialized()
    => Navigation.LocationChanged += OnLocationChanged;

private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    => CurrentUri = args.Location;

public void Dispose()
    => Navigation.LocationChanged -= OnLocationChanged;
```

这里的 route host 是应用自有 framing，不是 Microsoft `Router`、`RouteView`、`LayoutView` 或
`NavLink` 的兼容实现。外部 URI、`forceLoad`、server circuit、SSR/prerender route identity、
以及 `popstate`/`hashchange` 的可取消拦截仍不在声明内。

## 4. 诊断和替代

先修正 Razor SDK/Roslyn 的 `RZ****`/`CS****` 诊断；随后查看 RazorVue 的作者源诊断。常见边界
会在 `.razor` 或 `.razor.cs` 位置报告稳定的 `JAZORVCA***` ID，并带 HelpLink：

| 需要的能力 | 使用方式 |
| --- | --- |
| 数据库、请求上下文、Identity manager | server endpoint + 强类型 browser client；对应 `JAZORVCA001`/`002` |
| 没有 browser adapter 的 Blazor host service | 注册 typed adapter 或移到 endpoint；`JAZORVCA007` |
| `ParameterView` 枚举、`TryGetValue`、`ToDictionary` | 使用声明的 typed `[Parameter]` 属性；`JAZORVCA003`-`005` |
| `PersistentComponentState`、`[PersistentState]`、`[SupplyParameterFromForm]` | 使用显式版本化 bootstrap/endpoint DTO；`JAZORVCA011` |
| Microsoft 内置 UI、`IJSRuntime` 字符串互操作 | 使用 TDesign/Vuetify/Element Plus 或 typed ECMAScript/WebIDL contract；不要添加页面 bridge |

诊断阶段失败时不会生成部分 `ModuleCatalog`、模块或 bundle。正常的 Razor 写法不会因为 RazorVue
私有语法而出现额外 warning。

## 5. 验证

从仓库根目录运行 sample 的隔离构建和浏览器门禁：

```text
dotnet run --file samples/RazorVue.Authoring/build-local.cs -- --configuration Release --work-root .tmp/authoring-local-build
dotnet run --file samples/RazorVue.Authoring/verify-smoke.cs -- --skip-build --work-root .tmp/authoring-local-build --package-output .tmp/nupkg-sample/RazorVue.Authoring
```

第二条命令会检查 source authoring、official Razor SG 生成物、Debug module/source map、Release
package consumer、资源闭包和 HTTP-origin 浏览器 journey。没有 Edge/Chrome/Chromium 时可以加
`--skip-browser`，但这只能验证静态产物，不能替代浏览器证据。

更细的边界和诊断 ID 见 [RazorVue 作者指南](./razorvue-authoring.md)；样例中的完整页面见
[RazorVue.Authoring README](../../samples/RazorVue.Authoring/README.md)。
