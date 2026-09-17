using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// date-fns 入口；以纯函数导入提供解析、格式化、加减、比较、区间与 locale 能力。
/// date-fns entry; provides parsing, formatting, arithmetic, comparison, interval, and locale
/// capabilities as pure-function imports.
/// </summary>
/// <remarks>
/// 上游为 date-fns 4 的扁平 ESM 布局；所有日期参数使用 JavaScript <see cref="Date"/> 宿主类型。
/// 首期切片未绑定 <c>context</c> 构造函数（TZDate 扩展点）与 <c>fp</c> 函数式入口。
/// The upstream is the flat ESM layout of date-fns 4; all date arguments use the JavaScript
/// <see cref="Date"/> host type. The first slice does not bind the <c>context</c> constructor
/// (the TZDate extension seam) or the <c>fp</c> functional entry.
/// </remarks>
[ECMAScript("date-fns")]
[Description("@#")]
public static partial class DateFns
{
}
