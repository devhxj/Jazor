using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// vue-i18n 入口；提供 i18n 实例创建与组件内 Composer 访问。
/// vue-i18n entry; provides i18n instance creation and in-component Composer access.
/// </summary>
/// <remarks>
/// 作者入口为 <c>vue-i18n</c>；<c>@intlify/core-base</c>、<c>@intlify/shared</c> 与
/// <c>@intlify/message-compiler</c> 由 manifest 资源闭包提供，不在 C# 侧单独绑定。
/// 首期未绑定 <c>Translation</c>/<c>NumberFormat</c>/<c>DatetimeFormat</c>/<c>I18nT</c>/<c>I18nN</c>/<c>I18nD</c>
/// 组件与 <c>vTDirective</c> 指令。
/// The author entry is <c>vue-i18n</c>; <c>@intlify/core-base</c>, <c>@intlify/shared</c>, and
/// <c>@intlify/message-compiler</c> are supplied by the manifest resource closure rather than
/// separate C# bindings. The first slice does not bind the
/// <c>Translation</c>/<c>NumberFormat</c>/<c>DatetimeFormat</c>/<c>I18nT</c>/<c>I18nN</c>/<c>I18nD</c>
/// components or the <c>vTDirective</c> directive.
/// </remarks>
[ECMAScript("vue-i18n")]
[Description("@#")]
public static partial class VueI18n
{
}
