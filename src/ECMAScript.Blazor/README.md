# ECMAScript.Blazor

`ECMAScript.Blazor` 是 Blazor framework 类型到原生 browser carrier 的类型映射扩展库。它只包含 `[Jazor]` mapping declarations 和用于保持声明可编译的 adapter signatures，不拥有 Razor renderer，也不定义独立的 JavaScript runtime module。

当前首批映射把 `MouseEventArgs`、`KeyboardEventArgs` 和 `FocusEventArgs` 的只读 getter 投影到同一个 DOM callback 传入的 `MouseEvent`、`KeyboardEvent` 和 `FocusEvent`，并把 `ChangeEventArgs` 投影到 `JazorEvent`。setter、构造器和合成 `EventArgs` payload 不在此切片内。`ChangeEventArgs.Value` 由 `Jazor.CLR` helper 在 listener 边界一次性捕获，并以 `WeakMap` 保存事件时刻的 string、bool 或 string array 值。

该程序集不单独发布给应用安装：它作为 `Jazor.Vue` NuGet 的 `lib/net11.0` payload 交付；`Jazor` 核心包不包含该程序集，也不因此增加 `Microsoft.AspNetCore.App` framework reference。当前第一方 mapping 仍由 compiler generator 静态合并，独立程序集的交付边界不应误写成已经具备任意 NuGet provider 的动态发现能力。

映射源修改后运行：

```bash
dotnet run --project src/Jazor.Compiler.Generator/Jazor.Compiler.Generator.csproj
dotnet build src/ECMAScript.Blazor/ECMAScript.Blazor.csproj
```
