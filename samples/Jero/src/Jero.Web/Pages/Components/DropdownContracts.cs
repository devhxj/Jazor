using System;
using System.ComponentModel;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.Components;

/// <summary>导出菜单约定用字符串标识命令，处理器接收 Element Plus 回调的第一个参数。</summary>
public sealed record ExportMenuCommand([property: Description("@#onCommand")] Action<string> Command) : VueProps;
