using System.ComponentModel;

namespace ECMAScript;

public static partial class VueI18n
{
    /// <summary>
    /// 创建 i18n 根实例；返回值可直接作为 Vue 插件安装，并通过 <c>Global</c> 暴露全局 Composer。
    /// Creates the i18n root instance; the returned value can be installed as a Vue plugin and
    /// exposes the global Composer through <c>Global</c>.
    /// </summary>
    /// <param name="options">创建选项。The creation options.</param>
    [Description("@#createI18n")]
    public extern static VueI18nInstance CreateI18n(VueI18nCreateOptions options);

    /// <summary>
    /// 在当前组件中获取 Composer；<c>useScope</c> 决定使用全局、本地还是父级作用域。
    /// Gets the Composer for the current component; <c>useScope</c> selects the global, local, or parent scope.
    /// </summary>
    /// <param name="options">使用选项；为空时返回全局 Composer。The usage options; the global Composer is returned when omitted.</param>
    [Description("@#useI18n")]
    public extern static VueI18nComposer UseI18n(VueI18nUseOptions? options = null);
}
