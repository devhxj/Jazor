using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// vue-i18n 入口；提供 i18n 实例创建与组件内 Composer 访问。
/// vue-i18n entry; provides i18n instance creation and in-component Composer access.
/// </summary>
/// <remarks>
/// 作者入口为 <c>vue-i18n</c>，资源闭包使用上游 <c>browser</c> 条件的 <c>esm-browser</c> 构建
/// （已内联 <c>@intlify/*</c>）；bundler 构建在模块顶层引用 <c>process.env</c>，浏览器不可用。
/// 首期未绑定 <c>Translation</c>/<c>NumberFormat</c>/<c>DatetimeFormat</c>/<c>I18nT</c>/<c>I18nN</c>/<c>I18nD</c>
/// 组件与 <c>vTDirective</c> 指令。
/// The author entry is <c>vue-i18n</c>, and the resource closure vendors the upstream
/// <c>browser</c>-condition <c>esm-browser</c> build (which inlines <c>@intlify/*</c>); the bundler
/// build references <c>process.env</c> at module top level and cannot load in a browser. The first
/// slice does not bind the
/// <c>Translation</c>/<c>NumberFormat</c>/<c>DatetimeFormat</c>/<c>I18nT</c>/<c>I18nN</c>/<c>I18nD</c>
/// components or the <c>vTDirective</c> directive.
/// </remarks>
[ECMAScript("vue-i18n")]
[Description("@#")]
public static partial class VueI18n
{
}
