# 用 Jolt 编写 Vue 组件

Jolt 是用 C# 编写 Vue 3 单文件组件的工具。一份 `.jazor` 文件对应一个 ES 模块，默认导出 Vue 组件。HTML 和组件标签直接写在顶层。`@code` 的实例成员由每个组件实例独立持有，静态方法和字段进入 Jazor 静态模块类。`public static` 方法和字段以及公开常量按 Jazor 的模块规则生成具名导出。

编译链路和指令的实际产物见[语法审计](../02-architecture/jolt-authoring-comparison.md#现状审计)。后续拟支持的写法与示例见[目标特性](../02-architecture/jolt-authoring-comparison.md#目标特性与写法)。

## 安装并运行

Jolt CLI 使用 .NET 11，发布包覆盖 Windows 和 glibc Linux 的 x64 与 ARM64。在克隆的仓库根目录执行以下命令。

```sh
dotnet tool install --global Jazor.Jolt --version 2.7.0
jolt dev samples/Jolt.Counter --port 15658
```

已有 CLI 时，请用 `dotnet tool update --global Jazor.Jolt --version 2.7.0`。打开终端打印的网址，点击 **Increment**。子组件通过 `Action<int>` 回调通知父组件，父组件更新计数并控制 Reka UI 组件的打开状态。 [父组件](../../samples/Jolt.Counter/App.jazor) · [子组件](../../samples/Jolt.Counter/CounterButton.jazor)

| 命令 | 结果 |
| --- | --- |
| `jolt dev <project>` | 启动 Vite 并监听作者文件。默认使用 `http://127.0.0.1:15658/`，可用 `--port <端口>` 覆盖。宿主自启时可传 `--stop-on-stdin-close` 并持有标准输入的写端，关闭写端会让 Jolt 停止 Vite 并退出。 |
| `jolt build <project>` | 在 `.jolt/` 生成组件模块、源码映射与模块清单。可用 `--output <目录>` 指定输出。 |
| `jolt bundle <project>` | 用项目的 Vite 配置生成生产资源。计数器示例输出到 `dist/`。 |
| `jolt bundle <project> --ssr <server-entry>` | 从同一次组件编译生成 `dist/client` 和 `dist/server`。服务端入口相对于项目根目录解析。 |
| `jolt library <project>` | 生成可安装的 ESM 组件包、TypeScript 声明和样式，默认输出到项目的 `dist/`。支持 `--output <目录>`。 |
| `jolt lsp` | 通过标准输入输出启动编辑器使用的语言服务。 |

从仓库源码运行时，将 `jolt` 替换为 `dotnet run --project src/Jolt.Cli --`。 [示例与浏览器测试](../../samples/Jolt.Counter/README.md)

## 配置编辑器

在发布页下载与 CLI 同一发行版的扩展，将示例的 `samples/Jolt.Counter` 作为文件夹打开，再打开 `App.jazor`。 [2.7.0 下载](https://github.com/devhxj/Jazor/releases/tag/v2.7.0)

| 编辑器 | 安装 |
| --- | --- |
| VS Code | 选择 **Extensions: Install from VSIX**，安装 `jolt-2.7.0.vsix`。 [VS Code 配置](../../editors/jolt/README.md) |
| Visual Studio 2026 18.12 及以上（Windows x64） | 关闭 Visual Studio，再用 VSIX Installer 打开 `Jolt.VisualStudio.vsix`。 [Visual Studio 配置](../../editors/jolt-visual-studio/README.md) |

在 `@code` 内输入成员名可检查 C# 补全，在 `CollapsibleRoot` 上执行转到定义可检查 npm 声明导航。首次还原 npm 依赖时，本地 C# 与本地组件仍可补全和导航，npm 类型就绪后编辑器会刷新相关诊断和语义高亮。

## 直接使用 npm 组件

项目用 `package.json` 和 `pnpm-lock.yaml` 声明 Vue 及组件包。以下组件通过 `@using Npm.RekaUi` 让该包导出的名字进入当前文档。构建时从包的公开声明检查 `Open`，运行时从 `reka-ui` 导入 `CollapsibleRoot`。类型检查器由 Jolt 提供，消费者无需安装组件专用 C# 类库。

```razor
@using Npm.RekaUi

<button @onclick="@Toggle">切换</button>
<CollapsibleRoot Open="@open" />

@code {
    private bool open;
    private void Toggle() => open = !open;
}
```

命名约定区分包、子路径和导出。例如 `@scope/ui-kit/button` 的显式公开子路径对应 `Npm.Scope.UiKit.Button`。组件的具名导出沿用导出名，默认组件用路径末段的 PascalCase 名称。编译和编辑器读取已还原包的公开声明，运行时使用对应的 ESM 导出。同项目的 `.vue` 文件遵循本地目录命名空间规则。 [npm 元数据与范围](../../src/Jolt.Npm/README.md) · [混编样例](../../samples/Jolt.Mixed/App.vue)

### 在现有 Vue 应用里迁移一个列表页

以下写法需要当前源码构建的 Jolt CLI。宿主保留原有路由和 API 客户端，Jolt 页面接收查询结果和回调。按钮可以直接引用无参方法。分页事件保留 Vue 声明中的名称，Roslyn 会按声明校验回调的参数。

```razor
@using Npm.ElementPlus

<ElCard Header="站内信" class="zero-list-card">
    <ElButton @onclick="Refresh">刷新</ElButton>
    <ElPagination CurrentPage="@Page" Total="@Total" PageSize="20"
                  PageSizes="@(new double[] { 20, 50 })"
                  @oncurrent-change="ChangePage" />
</ElCard>

@code {
    [Parameter] public int Page { get; set; }
    [Parameter] public int Total { get; set; }
    [Parameter] public Action Refresh { get; set; } = default!;
    [Parameter] public Action<double> ChangePage { get; set; } = default!;
}
```

这里的 `@oncurrent-change` 生成 `onCurrentChange`。Element Plus 在决定是否展示分页控件时会检查该监听器。需要读取点击位置时，`ElButton` 的处理器可以接收 `ECMAScript.MouseEvent`。组件上的 `class` 和 `style` 接受 Vue 对应的属性类型。[事件与渲染回归](../../src/Jolt.Compiler.Test/ProjectCompilerTests.cs)

当宿主和 Jolt 都消费同一数据传输类型时，将它放在共享的普通 C# 文件中。在组件项目的 `package.json` 里通过 `"jolt": { "csharpSources": ["Contracts/Notice.cs"] }` 列出源码，让 .NET 宿主项目也编译该文件。构建和编辑器按同一份定义检查字段。在本地 SFC 中可以声明 `rows: readonly Readonly<{ id: string; title: string }>[]`，在 Jolt 中直接传入 `public sealed record Notice(string id, string title)` 的数组。投影按字段的 JavaScript 名称、是否必有值和标量类型核对，额外字段可留给业务代码使用，渲染时传递原数组。[共享源码测试](../../src/Jolt.Compiler.Test/SharedCSharpSourcesTests.cs) · [npm 与 SFC 契约验收](../../src/Jolt.Npm/verify.cs)

`csharpSources` 也接受相对于 `package.json` 所在目录的 glob。比如用 `["**/*Contracts.cs"]` 匹配根目录和各级子目录中的契约文件，或用 `["Contracts/*.cs"]` 只匹配 `Contracts` 目录中的 C# 文件。可以混用路径与 glob，重复匹配的文件只编译一次。显式路径缺失时报错，glob 没有命中时得到空集合。glob 跳过隐藏项、文件和目录链接，以及 `bin`、`obj`、`dist` 和 `node_modules`。匹配的未保存编辑以及新建文件同样参与编辑器检查。

表格列的插槽若在上游声明里只公开 `any`，应用可以在一个本地 SFC 内声明实际的行类型，对外只公开只读行数组和带类型的详情事件。Jero 的站内信列表在这层完成列渲染，再将点中的消息 ID 传回宿主的已读接口。[组件运行验收](../../scripts/csharp/verify-zero-jolt.cs)

### 直接调用 npm 函数

Jolt CLI 提供 Vue、Pinia、Vue Router、Pinia Testing 和 Vue Devtools 的手写 C# 类型。项目在 `package.json` 中声明实际调用的 npm 库。例如测试组件可以直接调用 `ECMAScript.Pinia.SetActivePinia(ECMAScript.PiniaTesting.CreateTestingPinia())`。普通 `.cs` 项目通过 NuGet 引用对应绑定。其他应用库使用下文的 npm 投影。 [包选择](./installation-and-configuration.md#选择包)

在项目的 `package.json` 中声明依赖。下例使用 `es-toolkit` 1.53.2 的公开子路径 `es-toolkit/string`。导入 `Exports` 类型并起一个 C# 别名，就可以在标记或 `@code` 中调用函数。多个组件共用的别名可以放在目录的 `_Imports.jazor`。

```razor
@using Text = Npm.EsToolkit.String.Exports

<output>@Text.CamelCase(Title)</output>

@code {
    [Parameter] public string Title { get; set; } = "Order total";
}
```

页面显示 `orderTotal`。Jolt 从锁定版本的声明得到 `string → string`，生成模块从原子路径导入 `camelCase`。写成 `Text.CamelCase(42)` 时，CLI 和编辑器在实参处报告 C# 类型错误。编辑器可从方法跳到已还原包中的声明。普通具名值采用 PascalCase，默认值使用 `Text.Default`，标量常量可作为只读属性读取。 [真实依赖及版本](../../samples/Jolt.Counter/package.json) · [导出类型与限制](../../src/Jolt.Npm/README.md)

同一模块中的函数引用同一个顶层输入声明时，兼容的投影可以共用一个 C# 类型。比如 `DateFns.Add` 和 `DateFns.Sub` 都接收 `DateFns.Duration`。对于已具现的泛型接口，Jolt 还会核对类型实参，能识别同一接口的本地类型别名。已有代码如果显式引用按方法生成的输入类型名，可以改用共用类型名，或写成由参数推断类型的 `new() { ... }`。

例如，项目声明 `date-fns` 后，可以把自动投影的结果直接传给同一个包里的另一个函数。下面的日期使用 JavaScript 月份编号，`0` 表示一月。

```razor
@using Dates = Npm.DateFns.Exports
<output>@IsLater()</output>

@code {
    private bool IsLater() {
        var start = new ECMAScript.Date(2026, 0, 15);
        var end = Dates.AddDays(start, 1);
        return Dates.IsBefore(start, end);
    }
}
```

`end` 的 C# 类型是 `ECMAScript.Date`，可以继续调用 `end.GetDate()`。把天数改为 `true` 会得到实参类型错误。[真实包运行示例](../../scripts/csharp/fixtures/jolt-npm-runtime/App.jazor)

date-fns 4.4.0 还可以用日期端点枚举区间，或在日期、毫秒时间戳和字符串组成的数组中查找最早日期。下面沿用上例的 `Dates`、`start` 和 `end`。

```csharp
var days = Dates.EachDayOfInterval(new() { Start = start, End = end }, new() { Step = 2 });
var values = new Dates.DateOrNumberOrString[] { days[0], 1700000000000d };
var earliest = Dates.Min(values);
var latest = Dates.Max(values);
```

`Step = 2` 表示每隔一天取一个日期，省略选项时逐日枚举。`Min` 和 `Max` 的上游声明都接收可变数组，元素允许三种类型。显式声明联合元素类型后，C# 会检查每次写入，生成的 JavaScript 使用普通数组。

在 `FormatISO` 的选项中，两组字面量分别对应不同的 C# 枚举。下面的调用返回 `20260115`。将 `Dates.ISOStringRepresentation.Date` 赋给 `Format` 会在 C# 编译时得到类型错误。[ISO 格式运行示例](../../scripts/csharp/fixtures/jolt-npm-runtime/App.jazor)

```csharp
var text = Dates.FormatISO(start, new() {
    Format = Dates.ISOStringFormat.Basic,
    Representation = Dates.ISOStringRepresentation.Date,
});
```

同一个区间还可用于判断日期和计算时长。把两个导出一起投影后，`FormatDuration` 可以直接接收 `IntervalToDuration` 返回的对象。这条调用也适用于通过构建取得 npm 投影的普通 C# 类库。[跨程序集运行示例](../../scripts/csharp/verify-jolt-public-projection.cs)

```csharp
var interval = new Dates.Interval { Start = start, End = end };
var includesEnd = Dates.IsWithinInterval(end, interval);
var elapsed = Dates.FormatDuration(Dates.IntervalToDuration(interval));
```

在使用 Vue I18n 组合式 API 的应用中，通过 `@using I18n = Npm.VueI18n.Exports` 引入包别名。在组件的 `[Setup]` 内，`var composer = I18n.UseI18n();` 取得已安装的 Composer。`composer.T("greeting")` 返回当前语言的消息，给 `composer.Locale.Value` 赋值后即可用同一个 Composer 读取新语言的消息。 [带插件注册的运行示例](../../scripts/csharp/verify-jolt-composable-runtime.cs)

若从插件实例读取 `Global`，创建时就可以确定它的类型。下面两个方法都调用上游的 `createI18n`，生成的选项对象分别带有 `legacy: false` 和 `legacy: true`。模式由运行时变量决定时可用 `I18n.CreateI18n(new() { Legacy = useLegacy, ... })`，此时仅公开两种结果都支持的成员。[两种模式的翻译与释放](../../scripts/csharp/fixtures/jolt-binding-projection/I18nPlugin.jazor)

```csharp
var composition = I18n.CreateI18nWithLegacyFalse(new() { Locale = "fr" });
composition.Global.Locale.Value = "en";
var legacy = I18n.CreateI18nWithLegacyTrue(new() { Locale = "en" });
legacy.Global.Locale = "fr";
```

使用动态消息目录时，`GetLocaleMessage` 返回可传给另一语言的 `VueProps`。输入可以是嵌套字典或应用自己的 `VueProps` 记录。`SetLocaleMessage` 替换整棵树，`MergeLocaleMessage` 保留未被修改的兄弟节点。获取缺少的语言时，锁定的 Vue I18n 返回空对象。[消息树及跨语言回传](../../scripts/csharp/fixtures/jolt-binding-projection/I18nPlugin.jazor)

```csharp
var composer = composition.Global;
composer.MergeLocaleMessage("fr", new Vue.VueDictionary {
    ["menu"] = new Vue.VueDictionary<string> { ["save"] = "Enregistrer" },
});
composer.SetLocaleMessage("fr-CA", composer.GetLocaleMessage("fr"));
```

检查某个 locale 是否有资源时，用 `composer.Messages.Value[locale]`。缺席读为 `null`。命中时取回原消息对象，可以直接再传给 `composer.SetLocaleMessage("fr-CA", messages)`。`GetLocaleMessage(locale)` 对缺席的 locale 返回空对象。[真实包用例](../../scripts/csharp/fixtures/jolt-binding-projection/I18nPlugin.jazor)

日期与数字格式也能按语言读取。`composer.DatetimeFormats.Value["en"]` 和 `composer.NumberFormats.Value["en"]` 分别读取相应格式表，缺席返回 `null`。要替换配置，可用 `SetDateTimeFormat` 和 `SetNumberFormat`。例如下面的组合式 Composer 依次设置回退语言、按语言选择的回退表和关闭回退。Legacy 模式直接给 `global.FallbackLocale` 赋同样的值。[格式表与实际回退查找](../../scripts/csharp/fixtures/jolt-binding-projection/I18nPlugin.jazor)

```csharp
composer.FallbackLocale.Value = new string[] { "fr", "en" };
composer.FallbackLocale.Value = new Vue.VueDictionary<string[]> { ["de"] = ["en"] };
string[]? fromGerman = composer.FallbackLocale.Value.Get("de");
composer.FallbackLocale.Value = ECMAScript.False.Value;
```

手上已经有原始消息文本时，可以用 `composer.Rt("Resolved {'message'}")` 按 Vue I18n 的消息语法求值，得到 `"Resolved message"`。按资源键查找仍使用 `composer.T("checkout.receipt")`。[独立 DLL 与 SSR 用例](../../scripts/csharp/verify-jolt-i18n-plugin.cs)

需要原始资源时，`composer.Tm("menu").GetOwn("save")?.AsString` 会读取嵌套的文本。动态键和自定义解析器可能返回数值、对象或函数，`AsString` 只在实际值为字符串时取值。缺键时 Vue I18n 返回的空对象也通过同一入口读取。[动态资源用例](../../scripts/csharp/fixtures/jolt-binding-projection/I18nPlugin.jazor)

如果 npm 工厂的公开声明返回 `Composer<typeof messages>`，资源字面量也能参与类型检查。例如法语资源含有 `welcome: 'Bonjour {guest}'`，英语资源还提供 `checkout.title`，Jolt 会生成下面的调用。法语缺少标题时，启用英语回退的 Composer 仍从英语表取值。更改消息的参数名后，重新生成的 C# 会在仍使用旧属性名的调用处报错。[组件示例](../../scripts/csharp/fixtures/jolt-binding-projection/StaticI18n.jazor) · [独立类库及 SSR](../../scripts/csharp/verify-jolt-i18n-plugin.cs)

```csharp
var composer = I18n.OpenComposer(true);
var greeting = composer.TWelcome(new() { Guest = "Ada" });
var title = composer.TCheckoutTitle();
var item = composer.TIndexed("a");                        // 'Item {0}' → "Item a"
var quantity = composer.TPlural(2d);                     // 'None | One | Many' → "Many"
var owner = composer.TNamedPlural(new() { Guest = "Ada" }, 2d);
var enabled = composer.Messages.Value.En?.Config.Enabled;
```

同一个 Composer 也可以按语言写回资源。下面的西班牙语标题在包中声明为 `'Hola {guest}' | 'Buenas {guest}'`。`SpanishTitle` 是生成枚举的 C# 别名，两种文案都能使用同一个翻译方法。

```csharp
composer.Locale.Value = I18n.StringChoiceOfEnOrEsOrFrOrPt.Es;
composer.SetLocaleMessageEs(new() {
    Panel = new() { Title = SpanishTitle.HolaGuest, Caption = "Reemplazo",
        Enabled = ECMAScript.False.Value, Score = SpanishScore.Value0 }
});
composer.MergeLocaleMessageEs(new() { Panel = new() { Title = SpanishTitle.BuenasGuest } });
var text = composer.TPanelTitle(new() { Guest = "Ada" }); // "Buenas Ada"
```

完整替换时，漏填必填的 `Caption` 会得到 C# 编译错误。合并时，省略它会保留已有文字。如果某个嵌套对象在资源声明中可缺席，首次提供这个对象时要填齐其必填字段。[完整组件及类型别名](../../scripts/csharp/fixtures/jolt-binding-projection/StaticI18n.jazor)

VueUse 的异步操作可以用任意 JavaScript 值拒绝。对 `UseAsyncState` 返回的 `state`，用 `var reason = state.Error.Value;` 读当前原因，`reason?.AsError?.Message` 读 Error 的消息，`reason?.AsString` 读字符串原因。`AsNumber` 和 `AsBoolean` 保留 `0` 与 `false`。[异步状态用例](../../scripts/csharp/fixtures/jolt-binding-projection/AsyncVueRefs.jazor)

Vue Query 的异步查询可以返回项目自己的记录。例如在 `[Setup]` 中创建查询后，`query.Data.Value?.Label` 会检查记录字段，首次加载前允许为空。

```csharp
// @using Query = Npm.Tanstack.VueQuery.Exports
var query = Query.UseQuery(new Query.UseQueryAsyncOptionsInput<QueryData> {
    QueryKey = ["fixture", 0, false],
    QueryFn = () => ECMAScript.Promise<QueryData>.Resolve(new QueryData("fetched"))
});
```

上例的 `QueryData` 声明为 `record QueryData(string Label)`。查询、失效和重取都能使用混合键。例如 `record PageFilter(double Page) : ECMAScript.Vue.VueProps` 可以作为 `QueryKey = ["orders", 0, false, new PageFilter(3)]` 的第四项。此前传入 `ECMAScript.Array<string>` 的代码可改用 `QueryKey = ["orders"]`。在组件挂载或服务端渲染前，C# 可以直接在应用实例上调用 `app.Use(Query.VueQueryPlugin)`。[查询与提交](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor) · [插件注册与运行断言](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-query.mjs)

普通查询键由应用约定载荷类型。读取时显式写出类型实参，未命中会得到 `null`。字符串、数值、布尔值和继承 `VueProps` 的记录可以直接写入。普通 C# 记录也可使用带类型的更新回调，按需读取旧值。返回 `null` 会跳过此次更新并保留原缓存内容。[组件用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor) · [普通 C# 类库消费](../../scripts/csharp/verify-jolt-public-projection.cs)

```csharp
var client = Query.UseQueryClient();
client.SetQueryData(["count", 1], 0d);
client.SetQueryData<double>(["count", 1], previous => (previous ?? 0d) + 1d);
double? count = client.GetQueryData<double>(["count", 1]);
var pending = client.FetchQuery<QueryData>(new() {
    QueryKey = ["orders", 1],
    QueryFn = () => ECMAScript.Promise<QueryData>.Resolve(new QueryData("ready")),
});
client.RemoveQueries(new() { QueryKey = ["count", 1], Exact = true });
```

当 mutation 接收数字并返回布尔值时，使用 `UseMutationBooleanValueOptionsInput<double>`。原先在 `UseMutationBooleanOptionsInput<double>` 中传入值类型的调用可以迁移到这个入口。`mutation.Variables.Value` 为 `double?`，`mutation.Data.Value` 为 `bool?`。首次调用前，两者均可读为 `null`。调用 `MutateAsync(0)` 后可以分别观察到 `0` 与 `false`。数值查询仍可用 `UseQueryNumberAsyncOptionsInput`。[标量结果与实参验证](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor)

在无限查询中，页记录和游标分别参与推导。下面的页类型是应用定义的 `QueryData`，数值游标在 `QueryFn` 和 `GetNextPageParam` 中都是 `double`。最后一页返回 `null` 停止翻页。和普通查询一样，Vue Query 的无限查询键可以组合字符串、数字、布尔值和 `VueProps` 记录。[SSR 与客户端分页用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor)

```csharp
var feed = Query.UseInfiniteQuery(new Query.UseInfiniteQueryNumberOptionsInput<QueryData> {
    QueryKey = ["feed", 0, false, new PageFilter(3)],
    InitialPageParam = 0,
    QueryFn = ctx => ECMAScript.Promise<QueryData>.Resolve(
        new QueryData(ctx.PageParam == 0 ? "first" : "second")),
    GetNextPageParam = (page, pages, cursor, cursors) => cursor == 0 ? 1 : null,
});
```

加载后可通过 `feed.Data.Value!.Pages[0].Label` 读取记录，通过 `feed.HasNextPage.Value` 判断能否再调用 `FetchNextPage()`。锁定的 Vue Query 5.103.1 把默认结果的 `pageParams` 声明为 `unknown[]`。回调里的 `cursors` 有元素类型，公开结果只生成声明可证明类型的 `Pages`。[页序列与真实缓存断言](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-query.mjs)

如果需要在 C# 结果里使用游标，可以在 `UseInfiniteQueryNumberSelectedOptionsInput<QueryData, SelectedPage>` 设置 `Select = data => new SelectedPage(data.Pages[0].Label, data.PageParams[0])`。这里 `SelectedPage` 是作者定义的 `record SelectedPage(string Label, double Cursor)`。选项的 `Select` 为必填项，同时决定 `Data.Value` 的类型。查询尚未取得数据时，该值可为空。[组件用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor)

VeeValidate 的带类型表单可以分别声明输入记录和提交记录，两者继承 `ECMAScript.Vue.VueProps`。`UseForm<RegistrationInput, RegistrationOutput>` 的解析器通过 `values.Get(row => row.Address)` 读取当前字段，在每个分支都填写结果记录和错误数组。成功处理器直接读取转换后的 `output.AddressLength`，失败处理器用 `invalid.Values.Get(row => row.Address)` 和 `invalid.Errors[nameof(RegistrationInput.Address)]` 取得原值与错误。

只需要初值时，使用单泛型表单。即使初值包含所有字段，VeeValidate 默认仍会在最后一个同名字段卸载时移除其值。从旧版生成的绑定迁移时，把实时输入和 schema 解析回调中的 `values.Address` 改为 `values.Get(row => row.Address)`，数值和布尔字段使用 `GetValue`。可用 `Vee.UseFormErrors()` 读取注入表单的首条错误。[完整表单用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor)

```csharp
public sealed record Input(double Count, bool Enabled) : ECMAScript.Vue.VueProps;

// 在 [Setup] 中
var form = Vee.UseForm<Input>(new() { InitialValues = new(0, false) });
double? count = form.Values.GetValue(row => row.Count);
bool? enabled = form.Values.GetValue(row => row.Enabled);
```

读取 VeeValidate 当前字段时可以保留标量的可空类型。调用方提供与表单字段匹配的类型参数。在 `[Setup]` 中写 `double? quantity = Vee.UseFieldValue<double>("quantity").Value;`，缺少这个字段时读到 `null`，字段已有的零会读成 `0`。布尔字段同样保留 `false`。[字段读取用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor)

需要展示同一字段的所有错误时，可从创建或注入的泛型表单读取 `form.ErrorBag.Value["email"]`。同一字段的两条消息按原顺序返回，缺席的字段读为 `null`。只显示首条错误时用 `form.Errors.Value["email"]`。[创建的表单](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor) · [注入的表单](../../scripts/csharp/fixtures/jolt-npm-runtime/FormScope.jazor)

在上面的 `Input` 表单中可以用选择器批量设置错误。下面的调用为数量写入两条消息，并清除启用状态的旧错误。其他字段保留原有错误。若把 `Count` 改成 DTO 上不存在的属性，生成程序集的消费者会收到 C# 编译错误。[独立 DLL 的正反例](../../scripts/csharp/fixtures/jolt-binding-projection/OverloadOperationConsumer.cs)

```csharp
form.SetErrors(new() {
    { row => row.Count, new string[] { "数量过低", "请重新输入" } },
    { row => row.Enabled, null },
});
```

创建字段时，路径、规则和初值可以分别提供。下面的数量字段在首次校验时可以为空。调用 `SetValue(0, false)` 写入零，第二个实参让 VeeValidate 跳过这次写入的自动校验。若创建时同时提供校验器和必填初值，校验器直接接收字段类型，记录字段可以直接访问其属性。[独立类库中的字段调用](../../scripts/csharp/verify-jolt-public-projection.cs) · [记录与异步校验器](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor)

```csharp
var quantity = Vee.UseField<double>("quantity", value => value is null ? "请填写数量" : true);
quantity.SetValue(0, false);
var enabled = Vee.UseField<bool>("enabled", options: new() { InitialValue = false });
```

在 `[Setup]` 中调用 `Vee.UseForm<Input>()` 可以直接创建有类型的空表单，无需配置初值和 schema。它会提供给子组件，尚未写入的数值字段可通过 `form.Values.GetValue(row => row.Count)` 读为 `null`。给字段写入 `0` 后，同一个读取会得到 `0`。[独立 C# 类库用例](../../scripts/csharp/fixtures/jolt-binding-projection/CorrelatedConsumer.cs)

空表单的成功提交回调仍可能收到缺少字段的记录。假设 `Input` 包含 `double Count` 和 `bool Enabled`，下面的处理能区分 `Enabled` 缺席和显式写入的 `false`。批量写入时，`SetValues` 接收输入 DTO，实际可省略哪些属性由 DTO 的构造方式决定。[普通 C# 消费与执行](../../scripts/csharp/fixtures/jolt-binding-projection/CorrelatedConsumer.cs)

```csharp
var form = Vee.UseForm<Input>();
bool? submitted = null;
var submit = form.HandleSubmit(values => { submitted = values.GetValue(row => row.Enabled); });
form.SetValues(new Input(0, false), false);
var pending = submit(); // pending 完成后，submitted 为 false
```

若父表单的 schema 把 `Input.Address` 转为 `Output.SubmittedAddress`，子组件用 `Vee.UseFormContext<Input, Output>()` 声明这份约定。上下文存在时，可在 `HandleSubmit` 的成功回调中用 `values.Get(row => row.SubmittedAddress)` 读结果。失败回调的 `Values` 和 `Errors` 仍对应输入。带完整初值和 schema 创建的表单可在成功回调里直接读 `values.SubmittedAddress`。[跨组件提交与转换](../../scripts/csharp/fixtures/jolt-npm-runtime/FormScope.jazor)

子组件可以在 `[Setup]` 中用 `UseFormContext<Input>()` 获取父表单。这里的 `Input` 是和父表单约定的输入记录。独立挂载子组件时，上下文可为 `null`。即使在表单内，字段也可能尚未注册，因此数值通过 `GetValue` 读为可空值。写入和校验使用同一个属性选择器。[父子组件挂载用例](../../scripts/csharp/fixtures/jolt-npm-runtime/FormScope.jazor)

```csharp
var form = Vee.UseFormContext<Input>();
if (form is null) return;
double? count = form.Values.GetValue(row => row.Count);
form.SetFieldValue(row => row.Count, 2d, false);
var validation = form.ValidateField(row => row.Count);
```

只需通用表单状态时，可以省略类型参数。例如 `var state = Vee.UseFormContext();` 后用 `state?.SubmitCount.Value` 读取提交次数，或通过 `state?.Errors.Value["email"]` 读取首条错误。这个入口在缺少父表单时也返回 `null`。[父表单存在与缺席的两种挂载](../../scripts/csharp/fixtures/jolt-npm-runtime/FormScope.jazor)

已知表单记录结构时，可以通过属性选择器读取嵌套值。以下 `FormSnapshot` 的 `Details` 是可空记录。选择器中的 `!` 只处理 C# 的可空提示，生成的 JavaScript 在任一段缺失时返回 `null`。每段按该记录属性的实际 JavaScript 名称读取，因此重命名过的字段也适用。[表单记录与读取代码](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor)

```csharp
var values = Vee.UseFormValues<FormSnapshot>().Value;
string? email = values.Get(row => row.Details!.EmailAddress);
double? count = values.GetValue(row => row.Details!.Count);
bool? enabled = values.GetValue(row => row.Details!.Enabled);
```

带类型的表单实例可以写 `form.SetFieldValue(row => row.Details!.EmailAddress, "updated", false)`。当两段属性的 JavaScript 名分别为 `contact`、`address` 时，发射的调用传入 `"contact.address"`。值的类型随叶属性确定，数值字段可直接写入 `0`。给显式指定为 `string` 的字段传 `null` 遵循 Roslyn 的可空告警规则。选择器由连续的记录数据属性组成，每段 JavaScript 名为非空，且不含 `.`、`[` 或 `]`。锁定的 VeeValidate 4.15.1 对数组元素的写入使用下面的字段模型。[普通类库用例](../../scripts/csharp/fixtures/jolt-binding-projection/CorrelatedConsumer.cs)

对于 `Contacts` 数组，可以在带类型的表单或注入的表单上下文中用 `DefineField` 同时取出字段模型和输入事件。假设 `Count` 的类型是 `double`，字段尚未创建时 `previous` 为 `null`，已写入的 `0` 仍是 `0`。下标可用整数常量，编译后直接传入路径字符串。

```csharp
var (count, props) = form.DefineField(row => row.Contacts![0].Count);
double? previous = count.Value?.Value;
count.WriteValue = 4d;
props.Value.OnBlur();
```

`form` 来自 `UseFormContext<Input>()` 时，应先判断上下文是否存在。创建的表单和注入的上下文都能用 `form.ValidateField(row => row.Contacts![0].Count)` 校验同一条路径。静态配置可以传 `new() { Label = "数量", ValidateOnBlur = false }`。需要根据字段状态生成自定义属性时，在组件或类库中声明 `public sealed record FieldProps(double Count) : Vue.VueProps;`，然后用下面的重载。[真实表单用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor)

```csharp
var (model, attributes) = form.DefineField<double, FieldProps>(row => row.Contacts![0].Count,
    new() { Props = state => new FieldProps(state.Value?.Value ?? 0) });
model.WriteValue = 4;
double current = attributes.Value.Count;
```

回调产生的字段可以覆盖基础事件名。这个重载按 `FieldProps` 的实际类型读取合并后的对象。省略 `Props` 的重载保留 `OnBlur()` 等基础事件。[覆盖同名字段的 DLL 用例](../../scripts/csharp/fixtures/jolt-binding-projection/CorrelatedConsumer.cs)

校验时机也可以随字段状态变化。下面的输入框在用户首次离开后才开始随模型更新触发校验。函数式配置在每次读取时可以按当前状态返回新的配置。[字段事件与校验用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor)

```csharp
var (count, events) = form.DefineField(row => row.Count,
    state => new() { ValidateOnBlur = false, ValidateOnModelUpdate = state.Touched });
```

### 普通 C# 类库的公开投影

普通 `.csproj` 直接引用Jazor 2.7.0，在 `package.json` 声明 `date-fns` 并提供 `pnpm-lock.yaml` 后，即可在 `.cs` 文件中写 `using Dates = Npm.DateFns.Exports;`。项目加载时，生成器先提供包根的 `Exports` 类型。首次输入别名时就有可绑定的类型，输入 `Dates.` 可从实际导出索引补全 `AddDays` 等成员。

对于项目打开时已有的依赖，VS Code C# 语言服务的 `Balanced` 模式可立即解析未保存的别名。首次调用尚未投影的 `Dates.AddDays` 时，成员处可能出现 `CS0117`。保存该文件即可获得方法签名。如果第二个参数传了 `true`，诊断会指向这个实参并报告 `CS1503`。

项目打开后才加入 `date-fns` 时，更新 `package.json` 和锁文件即可继续编辑。如果类库原先没有 npm 清单，可以在 `.csproj` 所在目录首次创建这两个文件。两种情况都已在真实的 VS Code C# 语言服务上验证。在 `Balanced` 和 `Automatic` 下，都可以在未保存的 C# 文件中从 `Npm.` 选到 `DateFns`，再从 `Npm.DateFns.` 选到 `Exports`。写好别名后，`Dates.` 会列出 `AddDays` 等实际导出。

如果先写了 `using` 再加包，`Automatic` 会自行清除导入错误，`Balanced` 下保存当前 C# 文件一次即可刷新绑定和方法签名。移除依赖后，两个模式也在各自的刷新时机撤去包名和成员补全，并在旧 `using` 处报告缺失的命名空间。VS Code 的模式由 `dotnet.server.sourceGeneratorExecution` 控制。[候选包编辑器验证](../../scripts/csharp/verify-jazor-npm-editor.cs)

子项目可以使用父目录的 `package.json`。如果在子项目目录创建自己的清单和锁文件，构建和 Automatic 编辑器随即使用子项目声明的依赖。删除这两份文件后，项目又能使用父清单中的包。已验证这个切换过程中的补全、调用签名及原 C# 文件的未保存编辑。[嵌套工作区验收](../../scripts/csharp/verify-jazor-npm-editor.cs)

开发本地 `link:` 包时，修改已有的 `.d.ts` 会刷新普通 C# 的签名。如果将入口改为重导出一份新建的声明文件，C# 会读到新的签名；要让它继续感知这份新文件自己的后续修改，需要让项目重新求值，使该文件进入 Roslyn 的设计时输入。已验证保存一次实际改动的 `.csproj` 后，原来未保存的 C# 调用立即得到更新的签名。[普通 C# 编辑器验收](../../scripts/csharp/verify-jazor-npm-editor.cs)

在 `.jazor` 编辑器中，链接包可以位于项目根目录之外。将包入口改为重导出另一份声明后，打开的组件会按新签名报错，F12 跟随仍能绑定的调用定位到新文件。组件也可以通过一个本地 `.vue` 间接依赖链接包的 prop 类型。在 `jolt dev` 中只改动这个 prop 的声明就会重新检查组件；修复后原页面继续运行。[编辑器与前端会话验收](../04-roadmap/jazor-jolt-migration.md#5-统一-vue-开发会话保留通用模块运行能力)

如果 npm 函数声明为 `mapItems<T, U>(items: readonly T[], select: (item: T) => U): readonly U[]`，可直接传入普通 C# 数组。下例的 `items` 是 `CatalogItem[]`，`Name` 为字符串。第一次调用推断出字符串元素，第二次将返回的只读数组直接交回 npm，推断出数值元素。两次调用都保留传入数组的 JavaScript 身份。[组件与独立类库消费用例](../../scripts/csharp/verify-jolt-public-projection.cs)

```csharp
var items = new[] { new CatalogItem("Notebook", 6), new CatalogItem("Pen", 2) };
var names = Fixture.MapItems(items, item => item.Name);
var lengths = Fixture.MapItems(names, name => (double)name.Length);
```

函数直接接收异步回调并返回相同的 `Promise<T>` 时，C# 可以从回调推导应用类型。比如 es-toolkit 1.53.2 的两个公开子路径可以这样组合。普通类库消费者已实际执行此调用，若将 `row.Title` 改为不存在的属性，Roslyn 会在消费项目报告 `CS1061`。[独立消费项目](../../scripts/csharp/verify-jolt-public-projection.cs)

```csharp
using Retries = Npm.EsToolkit.Function.Exports;
using Timers = Npm.EsToolkit.Promise.Exports;

[ECMAScript.ECMAScriptModule("jobs.mjs")]
public static class Jobs
{
    private sealed record Row(string Title);

    public static ECMAScript.IPromise<string> LoadTitle() =>
        Timers.WithTimeout(() => Retries.Retry(
            () => ECMAScript.Promise<Row>.Resolve(new Row("loaded")), 2), 1000)
            .Then(row => row.Title);
}
```

普通 C# 项目使用 .NET 11、C# preview 和 Jazor SDK。在项目中声明 npm 依赖并锁定 `pnpm-lock.yaml`，然后在 `.cs` 中通过 `using Npm…` 引入入口，运行 `dotnet build`。使用 Vue ref 的投影需要引用 `ECMAScript.Vue`。

```csharp
using Dates = Npm.DateFns.Exports;

using Locales = Npm.DateFns.Locale.Exports;

var duration = new Dates.Duration { Days = 2 };
string label = Dates.FormatDuration(duration);
string french = Dates.FormatDistance(new ECMAScript.Date(2026, 0, 17),
    new ECMAScript.Date(2026, 0, 15), new() { Locale = Locales.Fr }); // 2 jours
```

调用由 Jazor 编译为 ESM，执行环境需能解析相应的 npm 包。在独立消费者验收中，生成的模块调用真实 date-fns 后得到 `2 days`。[独立构建和运行用例](../../scripts/csharp/verify-jolt-public-projection.cs)

#### 在桌面 Visual Studio 验收普通 C# 编辑反馈

使用临时解决方案和当前候选 Jazor 包。项目目标为 `net11.0`，语言版本为 `preview`，直接引用该包，并在 `package.json` 和 `pnpm-lock.yaml` 中锁定 `date-fns`。先只保存空的 `Calendar.cs`，打开解决方案并等待 NuGet 还原和项目加载完成。以下按 C# 源生成器的 Automatic 模式操作。桌面 VS 缺省使用 Balanced，输入新调用后先保存一次，再检查成员签名与 F12。

1. 输入 `using Dates = Npm.`，按 Ctrl+Space 查看 `DateFns`。继续输入 `Npm.DateFns.` 查看 `Exports`，完成 `using Dates = Npm.DateFns.Exports;`。
2. 在类内写一个调用 `Dates.AddDays(new ECMAScript.Date(2024, 0, 31), 2)` 的方法。输入 `Dates.` 时检查 `AddDays`，再检查调用签名。把 `2` 改为 `true`，检查 `CS1503` 是否出现在该参数；改回 `2`，检查诊断清除。用另一个编辑器或终端读取 `Calendar.cs`，确认磁盘上仍是原来的空文件。
3. 在已绑定的 `AddDays` 上按 F12，核对生成 C# 中的方法签名。要打开本机原声明，桌面 Visual Studio 安装或更新现有 `Jolt.VisualStudio.vsix` 后，使用悬停中带文件名和行号的 `Open addDays.d.ts at line 48`。VS Code 使用悬停中的 `Open TypeScript declaration`。`.jazor` 中的 F12 直接定位到原始声明。[Visual Studio 安装说明](../../editors/jolt-visual-studio/README.md)
4. 若要验证外部声明刷新，增加一个用 pnpm `link:` 指向的本地包，让入口重导出另一份 `.d.ts` 中的函数。在打开的 C# 文件中调用该导出并保留未保存状态。单独修改被重导出的声明，将参数从 `string` 改为 `bool`，检查签名、诊断与快速信息链接是否刷新。

候选 SDK `.12` 已在独立桌面配置验证 Automatic 的未保存补全与成员 F12，以及 Balanced 保存后的参数帮助和成员 F12。[迁移验收记录](../04-roadmap/jazor-jolt-migration.md) · [独立 Roslyn LSP 验收](../../scripts/csharp/verify-jazor-npm-editor.cs)

#### 在多个 DLL 之间传递同一条 npm 记录

当一个类库返回 npm 记录，另一个类库在公开方法中接收这个记录时，让它们引用**同一个定义该类型的程序集**。可以沿用已有的普通 C# 类库。如果所有者只负责共享类型，可以像下面的 `CellContracts` 一样，用 `nameof` 选出公开签名所需的 npm 导出。项目引用 Jazor，并在 npm 清单中声明相应的包。生成器从包的 TypeScript 声明推导完整签名。

```csharp
using Wire = Npm.WireCells.Exports;

internal static class CellContracts
{
    internal const string Factory = nameof(Wire.MakeCell);
    internal const string Label = nameof(Wire.ReadLabel);
}
```

两个业务类库各自通过普通 `ProjectReference` 引用该项目。A 可以用 `Wire.MakeCell` 创建记录，B 可以在强类型方法中用 `Wire.ReadLabel` 接收它。最终消费者只需引用 A 和 B，即可写 `SharedCellProducerB.Label(SharedCellProducerA.Create("hello"))`。验收检查了三份 DLL 的类型引用都指向 `CellContracts`，并执行了生成的 ESM。契约项目只交付 .NET 声明，实际调用所在的业务模块会直接从 `wire-cells` 导入函数。[契约声明](../../scripts/csharp/fixtures/jazor-npm-library/CellContracts.cs) · [跨 DLL 构建及运行验收](../../scripts/csharp/verify-jazor-npm-library.cs)

如果两份已编译的 DLL 分别定义了自己的 `Cell<T>`，把 A 的结果直接传给要求 B 自有类型的方法会报 `CS1503`。迁移时将两边都要用的导出选入同一契约项目，让两个业务库引用它再重新构建。契约项目中只选了部分导出时，使用同一 `Exports` 的下游项目要调用其他导出，也需要先在所有者中选入相应成员。

### 在 Vue 应用中使用普通 Jazor 类库的模块

Web 宿主也可以直接编译应用目录中的 C# 浏览器模块。例如将 `JazorDir` 设为 `App/.jazor/modules`，让运行 `jolt bundle App` 的 MSBuild 目标在宿主构建后显式依赖 `JazorDebug`。保持 `JazorMode` 为空时，这个依赖会生成普通 ES 模块，发布目标只需收集 Jolt 生成的 `App/dist`。应用目录中的 `package.json` 与 `pnpm-lock.yaml` 同时用于 C# 的 npm 投影和 Jolt 打包。将该目录下的业务 `.cs` 列入宿主的 `Compile`，排除依赖、缓存与生成目录。

如果解决方案已有可执行 .NET 宿主，宿主可以用普通 `ProjectReference` 引用类库，由 Jazor Emit 把宿主及其引用模块交付到前端目录。以下示例假定 `Host/`、`Library/` 和 `frontend/` 是同级目录，宿主和类库均直接引用 Jazor。宿主项目使用已有的设置：

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)/../frontend/jazor/</JazorDir>
</PropertyGroup>
<ItemGroup>
  <ProjectReference Include="../Library/Library.csproj" />
</ItemGroup>
```

类库中的 `Greeting.Read()` 导出到 `messages/greeting.mjs`。宿主的 `ui-message.mjs` 模块在 C# 中调用它。运行 `dotnet build Host/Host.csproj` 后，Vue SFC 即可通过普通 ESM 导入使用宿主模块；Jolt 组件通过 prop 接收结果：

```vue
<script setup>
import Message from './Message.jazor';
import { Read } from './jazor/ui-message.mjs';
</script>
<template><Message :Text="Read()" /></template>
```

开发时运行 `jolt dev frontend`。在另一个终端运行 `dotnet watch --project Host/Host.csproj --no-hot-reload run`。保存 `Library/Greeting.cs` 后，.NET 会重建引用和宿主，Emit 更新 `frontend/jazor/` 中的模块，已打开的页面随 Vite 更新。准备分发时，先构建宿主交付 ESM，再运行 `jolt bundle frontend`，由 Jolt 从作者的入口生成 `frontend/dist/`。示例把 `JazorDir` 指向单独的子目录，因为 Emit 会在其中写自己的入口、包清单和锁文件。[项目引用的自动重建与页面状态验收](../../scripts/csharp/verify-jolt-mixed.cs)

换成 `PackageReference` 时，宿主按同一流程交付 DLL 中的模块。如果模块使用了投影的 npm 包，例如 `date-fns`，请在 Vue 应用的 `package.json` 和锁文件中声明应用要运行的版本。这样，从应用直接导入的 `addDays` 与经过 C# 类库导出的 `addDays` 在开发和生产中都是同一个函数；独立 Jazor 宿主则可以使用生产者锁定的版本。[同一 NuGet 的两种宿主验收](../../scripts/csharp/verify-jazor-module-handoff.cs)

### 复用构建生成的投影

Jazor SDK、`jolt build` 和语言服务使用用户级投影缓存。Windows 下的根目录是 `%LOCALAPPDATA%/Jazor/cache`。两个项目通过相同规则解析出相同的 npm 契约时，可以复用已有签名和生成的 C#。缓存命中后，编辑器从当前项目的安装位置打开声明。

| 位置 | 内容与用途 |
| --- | --- |
| 用户缓存下的 `npm-metadata/v1/` | npm 导出的签名和解析输入。可重定位的查询在启动 checker 前复核。项目内的 `.jazor/cache/npm-metadata/` 保存需要本地路径的查询。 |
| 用户缓存下的 `projections/v1/<摘要>/` | 普通类库使用 `Bindings.g.cs`。Jolt 组件使用 `Projection.g.cs` 和 `symbols.json`，跳转时按声明编号找到当前的 TypeScript 文件和位置。 |
| 工程的 `obj/<配置>/<TFM>/<工程名>.npm.g.cs` | 本次 C# 编译实际使用的投影，可以在这里检查某个调用生成了哪些重载。 |

交换模块或导出的顺序会命中同一份公开投影。空行或普通注释只移动声明时，生成的 C# 和符号表仍可复用，编辑器按新行号跳转。修改已投影成员的 JSDoc 时，生成的 XML 文档和悬停说明会随之更新。修改参数类型或把原先共用的类型拆成两个独立声明时，构建会重新检查调用。生成文件可以由下一次构建恢复。[跨进程构建、输出恢复与编辑器跳转验收](../../scripts/csharp/verify-jolt-projection-cache.cs)

多个项目共用业务 API 时，在类库中声明公共 C# 门面，消费项目通过 `ProjectReference` 引用该类库。[跨程序集运行用例](../../scripts/csharp/verify-jolt-public-projection.cs)

如果两个包共享泛型记录，消费方通常可以让 C# 推断生成的类型。例如一个包导出 `createEnvelope<T>(value: T): Envelope<T>`，另一个包导入同一声明并导出 `forwardEnvelope<T>(value: Envelope<T>): Envelope<T>`。在同一个项目中导入两个模块后，普通 C# 和组件都能这样写。

```csharp
var entry = Fixture.CreateEnvelope("initial");
var forwarded = Transfer.ForwardEnvelope(entry);
forwarded.Payload = "updated";
string label = entry.Payload;       // "updated"，两个变量引用同一个 JS 对象
double version = forwarded.Version; // 对应 readonly version: number
```

类似地，若 `selectMode(label, enabled, count)` 的结果依赖 `enabled` 的布尔字面量，可以写 `Transfer.SelectModeWithEnabledTrue("catalog", 4).Path` 读取详情，或写 `double count = Transfer.SelectModeWithEnabledFalse("catalog", 4)` 取得数值。两个方法在原参数位置传入各自的布尔值。[两种入口的完整消费者](../../scripts/csharp/verify-jolt-public-projection.cs)

当上游声明 `enabled?: B` 并将 `B` 默认为 `false` 时，`Transfer.SelectOptional()` 保留省略实参的调用形式，`Transfer.SelectOptionalWithEnabledFalse()` 显式传 `false`。二者都返回 `double`，上游仍可用 `arguments.length` 区分它们。递归泛型也可以跨包传递，例如子节点仍然携带原来的 `string` 类型。

```csharp
var root = Fixture.BuildTree("root");
var child = Transfer.ForwardTree(root).Children[0];
child.Value = "leaf";
string label = root.Children[0].Value; // "leaf"
```

如果返回记录声明了 `value?: T`，可以用 `HasValue` 判断是否提供了该字段。下面的写入能通过先前获得的引用读回。显式写入的零会保存在原对象上。

```csharp
var draft = Transfer.ForwardDraft<double>(new() { Seed = 5 });
double initial = draft.HasValue ? draft.Value : draft.Seed; // 5
var forwarded = Transfer.ForwardDraft(draft);
forwarded.Value = 0;
double current = draft.Value; // 0
```

上游若写作 `value?: T | null`，可使用 `new() { Seed = 5d, Value = null }` 传入显式空值，此时 `HasValue` 为真。字段存在时，先取 `var value = draft.Value`，再用 `value is null` 判断是否为空，非空时 `value.Value` 给出原始 `T`。[投影规则](../02-architecture/npm-projection-rules.md#对象上的可选泛型字段) · [独立类库示例](../../scripts/csharp/verify-jolt-public-projection.cs)

### 在每个实例中使用 VueUse

对需要 Vue 实例上下文的组合式函数，在 `@code` 中声明一个带 `[Setup]` 的同步、无参、返回 `void` 的私有实例方法。Vue 建立 props、slots 后调用它一次，随后开始渲染。下面的组件每次挂载都创建自己的计数器。

```razor
@using VueUse = Npm.Vueuse.Core.Exports

<button @onclick="Increment">@counter.Count.Value</button>

@code {
    [Parameter] public double Initial { get; set; }
    private VueUse.UseCounterResult counter = default!;

    [Setup]
    private void Configure() => counter = VueUse.UseCounter(Initial);

    private void Increment() => counter.Inc();
}
```

`UseCounterResult` 来自 `@vueuse/core` 14.4.0 的返回类型。`Count.Value` 可读取且参与响应式追踪，按该版本的声明应通过 `Inc()`、`Dec()` 或 `Set(value)` 更新。同步 setup 中注册的 Vue watcher 归组件实例所有，卸载即停止。 [可运行组件](../../samples/Jolt.Counter/.fixtures/NpmCounter.jazor) · [双实例、卸载与重新挂载验证](../../samples/Jolt.Counter/verify-browser.cs)

`UseStorage` 的第三个参数可以指定存储对象，第四个参数配置写入行为。在 `[Setup]` 中用具名实参 `options` 可以选择浏览器默认存储。下面的数值存储首次挂载时读到 `1`，存储中仍没有这个键。写入 `0` 后重新挂载会读回 `0`。给 `Value` 赋 `null` 会清除存储，同步读取能观察到这个空值，因此读取类型为 `double?`。[可运行存储组件](../../scripts/csharp/fixtures/jolt-npm-runtime/Storage.jazor)

```csharp
var saved = VueUse.UseStorage("jolt-storage-options", 1d,
    options: new() { WriteDefaults = false });
saved.Value = null;
double? afterClear = saved.Value;
```

在组件的 `[Setup]` 中使用 `UseVModel` 时，传入 Vue 的响应式 props 和一个属性选择器。如果 props 的记录类型将 `SomeProperty` 映射到 JS 的 `amount`，下面的调用会订阅 `amount`。父组件监听 `update:amount` 并更新 prop 后，模板中的 `model.Value` 会显示新值。数值属性决定了 `Value` 的 C# 类型，给它赋布尔值会在编译时报错。[可运行组件](../../scripts/csharp/fixtures/jolt-npm-runtime/Model.jazor) · [挂载与事件断言](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-model.mjs)

```csharp
var model = VueUse.UseVModel(props, x => x.SomeProperty);
double current = model.Value;
model.Value = 0;
```

要自己处理更新事件，可在选择器后直接传入回调。VueUse 声明了事件名和可变数量的参数，生成的 `args` 类型是 `ECMAScript.UnknownValue?[]`。例如，在收到 `update:amount` 时只接受数值更新，可以这样写。[组件示例](../../scripts/csharp/fixtures/jolt-npm-runtime/Model.jazor)

```csharp
var model = VueUse.UseVModel(props, x => x.SomeProperty, (name, args) =>
{
    if (name == "update:amount" && args.Length == 1 && args[0]?.AsNumber is double amount)
        SaveAmount(amount);
});
```

同一个选择器也决定选项的类型。例如输入框允许数值为零，同时忽略负数提交时，可以这样绑定。父组件没有提供值时，组件读到 `0`。给 `amount.Value` 写入负数时，父组件收不到更新事件。[挂载用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Model.jazor)

```csharp
var amount = VueUse.UseVModel(props, x => x.SomeProperty, new()
{
    DefaultValue = 0d,
    ShouldEmit = value => value >= 0,
});
```

若要通过 Vue watcher 同步本地值，调用 `UseVModelWithPassiveTrue(props, x => x.SomeProperty, new() { DefaultValue = 0d })`。该写法支持 `string`、`bool` 和 `double` 属性。赋值后可在下一次 Vue 更新周期观察父组件的变化。[双向同步断言](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-model.mjs)

若 prop 是非泛型密封记录或一维、交错数组，可以用 `UseVModelWithPassiveTrueRecord`。例如 `Counts` 声明为 `IVueRef<double>[]` 时，下面的两种赋值分别修改原 ref 和替换数组元素。设置 `Deep = true` 后，父组件也能收到内部修改的更新事件。[组件和记录定义](../../scripts/csharp/fixtures/jolt-npm-runtime/Model.jazor)

```csharp
var draft = VueUse.UseVModelWithPassiveTrueRecord(props, x => x.Refs, new() { Deep = true });
draft.Value.Rows[0].Counts[0].Value = 0;
draft.Value.Rows[0].Counts[0] = ECMAScript.Vue.Ref(9d);
```

如果 `Count` 是记录本身的 `IVueRef<double>` 属性，使用记录视图。选择器保留 C# 属性类型并发射实际的 JavaScript 键。直接属性支持 `IVueRef<double>`、`IVueRef<bool>` 和 `IVueRef<string>`。每个链接跟随创建它时的对象。父组件替换整条记录后，通过 `model.Value` 取得当前对象再选择属性。[深代理、浅代理与原始对象的挂载用例](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-model.mjs)

```csharp
var model = VueUse.UseVModelWithPassiveTrueRecordView(props, x => x.View, new() { Deep = true });
var count = ECMAScript.Vue.ToRef(model.Value, x => x.Count);
count.Value = 0;
```

嵌套记录用 `model.Value.Select(x => x.Nested)` 选择当前对象，再把结果交给 `Vue.ToRef`。两层选择器各自使用 DTO 上的 JavaScript 别名。在 npm 声明已给出具体输入和返回记录的情况下，生成的 `Value` 直接提供解包后的属性。若 setter 还接受原始 DTO，可通过 `WriteValue` 传入带 ref 的新记录。[类型化的读写用例](../../scripts/csharp/fixtures/jolt-binding-projection/ConcreteVueRefs.jazor)

`UseVModelWithCloneCallback` 的克隆回调通过选项 `CloneValue` 传入。选择数值属性时，`CloneValue = value => value + 1` 的参数与返回值都是 `double`。父组件的值通过 computed getter 时执行回调。[挂载用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Model.jazor)

`UseUrlSearchParams()` 返回响应式的 URL 参数。单个值读为字符串，重复值读为数组，缺席的键读为 `null`。用 `Set` 写入一个或多个值，用 `Delete` 移除该键。下面的模式匹配取出第一个值。[完整组件](../../scripts/csharp/fixtures/jolt-npm-runtime/UrlParams.jazor)

```csharp
var query = VueUse.UseUrlSearchParams();
query.Set("tag", new[] { "vue", "jolt" });
var firstTag = query["tag"] switch
{
    string text => text,
    string[] tags when tags.Length > 0 => tags[0],
    _ => "",
};
query.Delete("tag");
```

剪贴板的 `Copy` 接受字符串，也接受返回 `Promise<string>` 的 lambda。若文本需要异步获取，可直接传入取值函数。组件能从 `CopyPending.Value` 读取进行中状态，完成后从 `Text.Value` 和 `Copied.Value` 读取结果。[组件与调用](../../scripts/csharp/fixtures/jolt-npm-runtime/Clipboard.jazor)

```csharp
var clipboard = VueUse.UseClipboard();
clipboard.Copy("jolt");
clipboard.Copy(() => ECMAScript.Promise<string>.Resolve("vue"));
```

事件钩子的类型参数代表一次触发传递的载荷。下例中，监听器一次收到完整的字符串数组。订阅返回的句柄可以单独解除监听，也可以保留同一个委托传给 `hook.Off(listener)`。调用 `hook.Clear()` 会清空该钩子的订阅。[真实事件钩子与退订用例](../../scripts/csharp/fixtures/jolt-npm-runtime/EventHook.jazor)

```csharp
var hook = VueUse.CreateEventHook<string[]>();
string selected = "";
var registration = hook.On(items => { selected = items[0]; });
hook.Trigger(["vue", "jolt"]);
registration.Off();
```

读取鼠标来源时，`UseMouse().SourceType.Value` 与 `UseMouseInElement().SourceType.Value` 使用同一可空枚举，事件发生前可读为 `null`。网络状态的 `UseNetwork().EffectiveType.Value` 也是可空枚举，只供读取。宿主缺少连接信息时，可以直接用 `is null` 判断。[挂载组件](../../scripts/csharp/fixtures/jolt-npm-runtime/Events.jazor) · [事件与缺席状态断言](../../scripts/csharp/fixtures/jolt-npm-runtime/verify-events.mjs)

## 从 JavaScript 导入组件和静态成员

将以下代码保存为项目根目录的 `Counter.jazor`。点击按钮会更新当前组件实例的 `Count`。

```razor
<button @onclick="@Increment">@Count</button>

@code {
    public const int Initial = 3;
    public static int Step = 2;

    private int Count = Initial;

    public static int Advance(int value) => value + Step;
    private void Increment() => Count = Advance(Count);
}
```

执行 `jolt build .` 后，项目根目录的 JavaScript 入口可同时导入组件和静态成员。

```javascript
import Counter, { Advance, Initial, Step } from './.jolt/Counter.mjs';
```

`Count` 随组件实例创建。`Step` 是模块级状态，`Advance` 使用该模块的当前值。

## 组件参数、插槽和重构

### 表单、节点身份和事件

从本仓库构建 CLI 后，下面的组件可以让 Reka UI 的开关与原生勾选框共同编辑一个布尔值。

```razor
@using Npm.RekaUi

<section>
    <input @bind="filter" @bind:event="oninput" />
    <label><input type="checkbox" @bind="open" /> 展开</label>
    <CollapsibleRoot @bind-Open="open">
        <CollapsibleTrigger>详情</CollapsibleTrigger>
        <CollapsibleContent>@filter</CollapsibleContent>
    </CollapsibleRoot>
</section>

@code {
    private string filter = "";
    private bool open;
}
```

文本框和 `textarea` 默认在 `change` 时写回字符串。`oninput` 随输入更新，输入法组合期间由 Vue 推迟到提交后写回。checkbox 使用 `bool`。命名模型从 npm 声明配对同名 prop 与单参数 `update:名字` 事件，目前支持同类型的 `boolean`、`string` 或 `number`。绑定目标须是组件可以写入的状态。

循环中的 `<li @key="row.Id">` 和带 `@key` 的组件使用 Vue 节点身份。DOM 事件支持无参处理器，以及接收 WebIDL 事件的同步或 `Task` 方法，例如 `@onkeydown="HandleKey"` 可接收 `ECMAScript.KeyboardEvent`。用 `@onsubmit:preventDefault` 取消默认行为，用 `@onclick:stopPropagation="enabled"` 在事件发生时读取布尔条件。npm 组件的 `@onupdate:open` 仍按包声明的载荷调用。 [可编辑列表示例](../../samples/Jolt.Counter/.fixtures/EditableList.jazor)

| 原生控件 | 绑定类型 | 更新时机 |
| --- | --- | --- |
| 文本输入、`textarea` | `string` | 默认 `change`，可指定 `oninput` |
| checkbox | `bool` | 勾选变化 |
| radio、单选 `select` | `string` | 选择变化。显式选项值也使用字符串 |
| `<select multiple>` | `string[]` | 选择变化。多选由静态 HTML 属性决定 |
| `<input type="number">` | `double?` | 默认 `change`，可指定 `oninput`。清空或无效输入写回 `null` |

希望自己决定是否接受输入时，成对使用 `:get` 与 `:set`。普通绑定完成赋值后要运行逻辑时，使用 `:after`。这两种形式分别支持同步方法或返回 `Task` 的方法，也可用于组件的命名模型，如 `@bind-Value:get` / `@bind-Value:set`。文本和数字控件在同步 setter 返回时重新读取 getter，拒绝输入后立即恢复显示。异步提交的后续更新及选择类控件的回显遵循 Vue 响应式刷新。 [原生表单样例](../../samples/Jolt.Counter/.fixtures/NativeForm.jazor) · [绑定与交互样例](../../samples/Jolt.Counter/.fixtures/TableDemo.jazor)

```razor
<input @bind:get="Name" @bind:set="SetName" @bind:event="oninput" />
<input @bind="Note" @bind:after="SaveNote" />
<output>@LastSaved</output>

@code {
    private string Name = "";
    private string Note = "";
    private string LastSaved = "";
    private void SetName(string value) => Name = value.TrimStart();
    private void SaveNote() => LastSaved = Note;
}
```

### 本地组件、引用和属性透传

在子组件上声明 `[Parameter] public string Value { get; set; }` 和 `[Parameter] public Action<string>? ValueChanged { get; set; }`，并在编辑时调用 `ValueChanged?.Invoke(next)`。父组件可用 `<TableEditor @bind-Value="name" />` 接收更新。生成的组件公开 Vue 的 `value` / `update:value`，已有显式传 `Value` 和 `ValueChanged` 的调用仍走原回调。 [子组件](../../samples/Jolt.Counter/.fixtures/TableEditor.jazor)

用 `<input @ref="input" />` 捕获单个元素，在组件上声明可写的 `ECMAScript.HTMLInputElement?` 字段或属性。给本地组件的公共实例方法加 `[Expose]`，父组件即可用生成的 `TableEditorRef?` 调用 `editor?.Focus()`。挂载完成后引用可用，移除目标时 Vue 清空引用。当前只支持单个 DOM 或本地组件目标，循环中的集合引用和 npm 组件实例引用留待单独实现。 [焦点和卸载样例](../../samples/Jolt.Counter/.fixtures/TableDemo.jazor)

透传一组属性可写 `<button class="base" @attributes="attrs" title="保存">保存</button>`。例如在 `@using static ECMAScript.Vue` 下将 `attrs` 声明为 `VueDictionary<string>`。属性和展开表达式按书写顺序各求值一次，普通重名键以后者为准，class、style 和监听器交给 Vue 的 `mergeProps`。调用具有必填参数的组件时仍需显式给出该参数，便于 Roslyn 在编译时检查。 [属性展开回归](../../src/Jolt.Compiler.Test/AttributesDirectiveTests.cs)

### Vue 内置组件

`<Teleport>` 和 `<Transition>` 可直接用于 `.jazor` 模板。当前作用域内同名的本地组件优先。

```razor
<Teleport To="#overlays" Disabled="@ShowInline">
    <aside>通知</aside>
</Teleport>

<Transition Name="fade" Mode="out-in">
    @if (ShowNotice) {
        <p>通知已开启</p>
    }
</Transition>

@code {
    private bool ShowInline;
    private bool ShowNotice = true;
}
```

`Teleport` 的 `To` 必填，接受 CSS 选择器或能隐式转换为 `Vue.VueTeleportTarget` 的表达式。`Disabled` 接受布尔值，启用时子节点留在原处。SSR 宿主需要在目标容器内放入服务器返回的 Teleport 内容，客户端才能水合该部分。

`Transition` 接受 `Name` 和可选的 `Mode`。`Mode` 支持 `in-out`、`out-in` 或 `Vue.VueTransitionMode` 表达式。直接子节点或 `<template #default>` 构成默认插槽，每次渲染可以提供一个元素或组件，`@if` 的各个分支分别检查。

### 参数与插槽

组件用 `[Parameter]` 声明参数和委托，用 `[EditorRequired]` 标记调用者必须提供的参数。默认插槽使用 `<slot />`，具名插槽使用 `<slot name="header" />`，调用方用 `<template #header>…</template>` 填充内容。`<template>` 在这里是具名插槽写法，文件顶层可以直接书写标记。C# 区域支持 `@name`、`@(expression)`、`@if` 和 `@foreach`。

编辑器用相同的项目加载器、组件声明和 C# 投影提供补全、悬停、签名帮助、定义导航和诊断。键入标签名时，可以选择同项目其他命名空间的本地组件，接受建议后会插入相应的 `@using`，随后可以补全该组件的类型化参数。npm 定义可跳转至声明文件。 [VS Code 扩展](../../editors/jolt/README.md) · [Visual Studio 扩展](../../editors/jolt-visual-studio/README.md)

编辑器提供查找引用、改名、格式化和快捷修复。跨文件改名按 Roslyn 符号和组件绑定更新 C#、参数、委托和具名插槽。支持文件操作的编辑器还可以从标签改名组件及其同目录文件。格式化调整 C# 和标签内的属性缩进，保留渲染文本与换行风格。未知的本地组件可以用快捷修复添加 `@using`，缺少必填参数时可以匹配当前组件的同名同类型字段或可读属性。

### 泛型组件

本地组件支持单个 `@typeparam T`。调用方提供类型明确的数组或 `IReadOnlyList<T>` 参数时，Roslyn 推断 `T`，再用它检查委托和插槽载荷。调用方也可以显式写 `T="string"`。组件运行时使用同一份 Vue 模块。 [本地泛型组件样例](../../samples/Jolt.Counter/.fixtures/List.jazor)

```razor
<List Items="@Names" Select="@OnPick">
    <template #item context="row"><p>@row.value.Length</p></template>
</List>

@code {
    private string[] Names = ["One", "Four"];
    private void OnPick(string name) { }
}
```

npm 消费已验证 Reka UI 2.11.0 的 `ListboxVirtualizer`。它从 `Options` 推断元素类型，并将该类型用于 `TextContent` 的参数和默认插槽的 `option`。当前支持公开 Vue 可调用组件的单个泛型参数和必填数组输入，支持范围还取决于声明可映射的原始类型约束。 [npm 消费样例](../../samples/Jolt.Counter/.fixtures/App.jazor)

### 泛型表格、内联模板与目录导入

本地组件可分别声明多个 `@typeparam` 及其 `where` 约束。调用点可显式给出全部参数，也可让 Roslyn 从输入值与委托推断。下例中 `Items` 决定行类型，`GetStatus` 决定状态类型，`Row Context="row"` 中能直接访问带类型的 `row.value` 和 `row.status`。传递的类型参数在运行时擦除。普通 `T` 上的 `default(T)` 仍受核心编译器的约束，可证明引用类型时才能降为 `null`。 [表格声明](../../samples/Jolt.Counter/.fixtures/TypedTable.jazor) · [多泛型检查](../../src/Jolt.Compiler.Test/GenericComponentTests.cs)

```razor
<TypedTable Items="@Names" RowKey="@Length" GetStatus="@Length">
    <Row Context="row">
        @{ var label = row.value; }
        <td>@label</td><td>@row.status</td>
    </Row>
</TypedTable>

@code {
    private string[] Names = ["One", "Four"];
    private int Length(string name) => name.Length;
}
```

用 `@for`、`@foreach` 和 `@switch` 组织标记，支持模式守卫、`break` 和 `continue`。循环变量、case 变量和 `@{ … }` 中的局部声明遵循投影后的 C# 作用域。组件声明了 `row` 出口，就可以用 `<Row Context="row">` 填充，也可以保留 `<template #row context="row">`。默认出口可写 `<ChildContent>`。同名出口在不同分支需提供相同的字段及类型。 [控制流样例](../../samples/Jolt.Counter/.fixtures/TableDemo.jazor)

内联模板适合在多个位置复用同一段标记。通过 `RenderFragment<T>` 传给子组件的 `[Parameter]` 时，调用点检查类型。无参片段使用 `RenderFragment` 和 `() => @<…>`。每次调用产生新的 Vue 节点，并读取闭包中捕获的变量。 [内联模板回归](../../src/Jolt.Compiler.Test/InlineFragmentTests.cs)

```razor
@{ var suffix = "!"; RenderFragment<string> label = name => @<strong>@(name + suffix)</strong>; }
<p>@label("One")</p>
<p>@label("Four")</p>
```

项目根或子目录的 `_Imports.jazor` 接受 `@using`，包括 `@using Npm.RekaUi`。从根向内层依次导入，最后应用组件自己的指令。共享文件只影响所在目录及后代，不生成组件模块。编辑器使用未保存的导入，错误定位到实际声明文件。 [示例共享导入](../../samples/Jolt.Counter/.fixtures/_Imports.jazor)

### 组件样式

在文件顶层写无属性的 `<style>` 以随组件模块交付全局 CSS，或用 `<style scoped>` 限定选择器作用域。两种块按源码顺序交付，嵌套的样式元素仍按普通标记发射。CSS 可直接写 `@media`，从 RazorVue 迁移的 `@@media` 也会在字符串和注释之外还原。Jolt 为组件模块分配稳定的 scope ID，`jolt dev` 和 `jolt bundle` 复用 Vue 的样式转换。已验证同名选择器隔离、父组件对其子组件根节点的样式、`:slotted()` 和样式热更新。当前作者入口支持原生 CSS 样式块。 [浏览器验收样例](../../samples/Jolt.Counter/README.md)

## Vue 项目集成

### 在一个 Vite 项目中混用 .vue 和 .jazor

从仓库根目录运行 `dotnet run --project src/Jolt.Cli -- dev samples/Jolt.Mixed`。样例中的 `App.vue` 持有一个字符串，通过 `v-model:text` 调用 Jolt 组件。Jolt 组件再调用同目录的原生 Vue 输入组件。

```vue
<script setup lang="ts">
import { ref } from 'vue';
import MixedEditor from './editors/MixedEditor.jazor';
const name = ref('First draft');
</script>

<template>
    <MixedEditor :Limit="24" v-model:text="name" />
</template>
```

在 `MixedEditor.jazor` 内，`<NameInput Limit="@Limit" @bind-Text:get="Text" @bind-Text:set="Save" />` 调用 `NameInput.vue`。Jolt 读取 Vue 组件的公开声明，识别 `limit: number` 和 `defineModel<string>('text')`，校验传入值与回写委托。跨目录使用 `@using <组件命名空间>`，规则与本地 Jolt 组件一致。 [Jolt 调用方](../../samples/Jolt.Mixed/editors/MixedEditor.jazor) · [Vue 输入组件](../../samples/Jolt.Mixed/editors/NameInput.vue)

项目声明 `vue`、`vite` 和 `@vitejs/plugin-vue`，用 `jolt dev` 或 `jolt bundle` 运行。标准 Vue 与 TypeScript 编辑工具通过生成的声明识别源码导入。首次运行 `jolt build` 或 `jolt dev` 后，在项目的 `tsconfig.json` 中使用下面两项配置。`jolt dev` 在编辑时更新 `.jolt/` 下的声明和映射，Vue Language Tools 可检查 SFC 和 TSX 调用方。 [完整 tsconfig](../../samples/Jolt.Mixed/tsconfig.json) · [TSX 源码调用](../../samples/Jolt.Mixed/probe.tsx)

```json
{
    "compilerOptions": {
        "moduleResolution": "Bundler",
        "allowArbitraryExtensions": true,
        "rootDirs": [".", "./.jolt"]
    }
}
```

### 通过 Vue Router 使用 `@page` 和 `@layout`

安装 `vue-router` 并在应用的 `package.json` 中设置 `"jolt": { "router": true }`。从构建目录导入 `routes`，将它传给应用已有的 `createRouter`，也可以和手写路由合用。

```ts
import { routes } from './.jolt/_routes.mjs'

const router = createRouter({ history, routes })
```

```razor
@page "/drafts/{id:int}"
@layout DraftLayout
<h1>Draft @Id</h1>

@code {
    [Parameter] public int Id { get; set; }
}
```

`@page` 接受绝对路径、独立的 `{name}` 字符串段和 `{name:int}` 整数段。参数名按大小写不敏感的方式匹配公开的 `[Parameter]` 属性，属性类型分别为 `string` 和 `int`。整数超出 Int32 范围会在向页面传递路由参数时报告错误。路由仍由应用创建并安装，因此客户端和 SSR 可以使用同一份路由记录。

`@layout` 选择一个本地、非泛型 `.jazor` 组件。布局须提供无作用域参数的默认 `<slot />`，且不能声明必填参数或路由指令。可以用完整组件名消除同名歧义。

### 将组件库交付给普通 Vue 或 TSX 项目

让库的 `package.json` 提供包名、版本和 Vue 版本范围，执行 `jolt library <项目目录>`。产物包含 npm 清单、以 Vue 为 peer dependency 的 ESM、公开类型及转换后的组件 CSS。例如，本仓库的 `Meter` 可从包根具名导入，也可从独立子路径默认导入。不同目录有同名组件时按子路径选择。 [组件源码](../../scripts/csharp/fixtures/jolt-library/library/Meter.jazor) · [示例库清单](../../scripts/csharp/fixtures/jolt-library/library/package.json)

```ts
import { Meter } from '@jazor/library-meter-fixture';
```

在 SFC 中用 `<Meter v-model:value="value">`，用 `<template #reading="{ amount }">` 读取数值，模板引用只公开 `[Expose]` 声明的方法。TSX 可写 `value={value.value}`、`onUpdate:value={next => value.value = next}` 并传入带类型的插槽。包的 `exports` 会将 Vue 与 TypeScript 工具引向同一份组件声明。 [SFC 消费者](../../scripts/csharp/fixtures/jolt-library/consumer/src/App.vue) · [TSX 消费者](../../scripts/csharp/fixtures/jolt-library/consumer/src/JsxMeter.tsx)

### 使用 ASP.NET Core 托管混合应用的 SSR

从仓库根目录构建并启动示例。打开 `http://127.0.0.1:5168/editor?name=Alice`，查看页面源代码即可看到请求名称、计数和已渲染的 Teleport。点击计数按钮后，浏览器接管原来的 DOM，再用 Vue Router 切换页面。 [示例与发布步骤](../../samples/Jolt.Mixed.Ssr/README.md)

```sh
dotnet run --project src/Jolt.Cli -- bundle samples/Jolt.Mixed.Ssr/frontend --ssr entry-server.ts
dotnet run --project samples/Jolt.Mixed.Ssr -- --urls http://127.0.0.1:5168
```

**构建与部署。** `index.html` 提供 Vue 挂载节点、状态和 Teleport 目标的位置。Vite 生成带哈希的脚本和 CSS，两个入口都成功后替换项目的 `dist/`。样例的 ASP.NET Core 项目将整个目录随 `dotnet publish` 交付。部署新产物后重启宿主。如果应用位于子路径，Vite 的 `base`、Vue Router 的 history base 和 ASP.NET Core 的 `PathBase` 应一致。 [文档模板](../../samples/Jolt.Mixed.Ssr/frontend/index.html) · [发布配置](../../samples/Jolt.Mixed.Ssr/Jolt.Mixed.Ssr.csproj)

**每请求渲染。** 在宿主中注册 `AddJoltSsr("frontend/dist")`，通过 `UseJoltSsr` 的请求工厂提供 props 和 provider。服务端入口导出 `render({ url, state })`，为该请求创建 Vue app、memory history 和 Pinia。等待路由和预取完成，将最终 store 快照写回 `state.props`，并返回根 HTML 和 Vue 的 Teleport 片段。宿主把结果和序列化状态填入同一份构建模板。 [ASP.NET Core 入口](../../samples/Jolt.Mixed.Ssr/Program.cs) · [服务端入口](../../samples/Jolt.Mixed.Ssr/frontend/entry-server.ts)

**客户端接管。** 从 `#__jazor_ssr_state` 读取状态，以 web history 调用同一个应用工厂，恢复 Pinia 和 provider。等待 `router.isReady()` 后用 `createSSRApp` 创建的应用挂载。示例中的 `.jazor` 组件从 `[Setup]` 用 `Vue.Inject<string>` 读取请求值。 [共享工厂](../../samples/Jolt.Mixed.Ssr/frontend/workspace.ts) · [客户端入口](../../samples/Jolt.Mixed.Ssr/frontend/entry-client.ts) · [类型化注入](../../samples/Jolt.Mixed.Ssr/frontend/RequestBadge.jazor)

当页面需要从一组已知的本地组件中选择一个，可写 `<component is="@(showDetails ? Details : Summary)" Caption="@caption" />`。每个候选的参数和插槽都要通过检查，选中分支才创建 VNode。候选为当前作用域中可见的 `.jazor` 或 `.vue` 组件。 [动态组件回归](../../src/Jolt.Compiler.Test/DynamicComponentTests.cs)

## 验证与边界

运行 `dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt` 检查语法与编译回归。npm 类型、编辑器协议和浏览器分别有独立验证，可直接检查包升级后的行为、未保存编辑和生产资源。 [npm 验证](../../src/Jolt.Npm/verify.cs) · [编辑器协议验证](../../src/Jolt.LanguageServer/verify.cs) · [浏览器验证](../../samples/Jolt.Counter/README.md)

SSR 有独立的并发请求与浏览器验收，覆盖预取失败后的恢复、接管原 DOM、首次点击、返回导航和组件清理。 [SSR 验收脚本](../../scripts/csharp/verify-jolt-ssr.cs) · [已发布能力](../04-roadmap/current-status.md)
