using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// <c>useI18n()</c> 的作用域；字符串值与 vue-i18n <c>useI18n</c> 的 <c>useScope</c> 值域一致。
/// The scope of <c>useI18n()</c>; values mirror the vue-i18n <c>useScope</c> domain.
/// </summary>
[String]
public enum VueI18nScope
{
    /// <summary>使用全局作用域的 locale 与消息。Use the global scope's locale and messages.</summary>
    [Description("@#global")]
    Global,

    /// <summary>为当前组件创建本地作用域。Create a local scope for the current component.</summary>
    [Description("@#local")]
    Local,

    /// <summary>继承父级作用域。Inherit the parent scope.</summary>
    [Description("@#parent")]
    Parent
}

/// <summary>
/// <c>createI18n()</c> 的选项对象。
/// Options for <c>createI18n()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueI18nCreateOptions
{
    /// <summary>当前 locale，例如 <c>en-US</c>。The current locale, for example <c>en-US</c>.</summary>
    [Description("@#locale")]
    public string? Locale { get; init; }

    /// <summary>回退 locale。The fallback locale.</summary>
    [Description("@#fallbackLocale")]
    public string? FallbackLocale { get; init; }

    /// <summary>按 locale 索引的消息树。The message tree keyed by locale.</summary>
    [Description("@#messages")]
    public Vue.VueDictionary? Messages { get; init; }

    /// <summary>按 locale 索引的日期时间格式。The datetime formats keyed by locale.</summary>
    [Description("@#datetimeFormats")]
    public Vue.VueDictionary? DateTimeFormats { get; init; }

    /// <summary>按 locale 索引的数字格式。The number formats keyed by locale.</summary>
    [Description("@#numberFormats")]
    public Vue.VueDictionary? NumberFormats { get; init; }

    /// <summary>是否使用 legacy 模式；Jazor 绑定始终要求组合式 API，请保持 false。Whether to use legacy mode; the Jazor binding always requires the composition API, so keep it false.</summary>
    [Description("@#legacy")]
    public bool? Legacy { get; init; }

    /// <summary>是否把全局属性与函数注入组件实例。Whether to inject global properties and functions into component instances.</summary>
    [Description("@#globalInjection")]
    public bool? GlobalInjection { get; init; }

    /// <summary>是否允许在 legacy 模式下使用组合式 API。Whether composition API usage is allowed in legacy mode.</summary>
    [Description("@#allowComposition")]
    public bool? AllowComposition { get; init; }

    /// <summary>缺失翻译时是否输出警告。Whether to warn on missing translations.</summary>
    [Description("@#missingWarn")]
    public bool? MissingWarn { get; init; }

    /// <summary>回退到 fallback locale 时是否输出警告。Whether to warn when falling back to the fallback locale.</summary>
    [Description("@#fallbackWarn")]
    public bool? FallbackWarn { get; init; }

    /// <summary>是否回退到根（全局）作用域。Whether to fall back to the root (global) scope.</summary>
    [Description("@#fallbackRoot")]
    public bool? FallbackRoot { get; init; }

    /// <summary>是否允许在消息中使用 HTML。Whether HTML is allowed inside messages.</summary>
    [Description("@#warnHtmlMessage")]
    public bool? WarnHtmlMessage { get; init; }
}

/// <summary>
/// <c>useI18n()</c> 的选项对象。
/// Options for <c>useI18n()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueI18nUseOptions
{
    /// <summary>当前 locale。The current locale.</summary>
    [Description("@#locale")]
    public string? Locale { get; init; }

    /// <summary>回退 locale。The fallback locale.</summary>
    [Description("@#fallbackLocale")]
    public string? FallbackLocale { get; init; }

    /// <summary>本地作用域的消息树。The message tree for the local scope.</summary>
    [Description("@#messages")]
    public Vue.VueDictionary? Messages { get; init; }

    /// <summary>本地作用域的日期时间格式。The datetime formats for the local scope.</summary>
    [Description("@#datetimeFormats")]
    public Vue.VueDictionary? DateTimeFormats { get; init; }

    /// <summary>本地作用域的数字格式。The number formats for the local scope.</summary>
    [Description("@#numberFormats")]
    public Vue.VueDictionary? NumberFormats { get; init; }

    /// <summary>Composer 作用域。The Composer scope.</summary>
    [Description("@#useScope")]
    public VueI18nScope? UseScope { get; init; }

    /// <summary>是否继承根作用域的 locale。Whether to inherit the root scope locale.</summary>
    [Description("@#inheritLocale")]
    public bool? InheritLocale { get; init; }

    /// <summary>缺失翻译时是否输出警告。Whether to warn on missing translations.</summary>
    [Description("@#missingWarn")]
    public bool? MissingWarn { get; init; }

    /// <summary>回退到 fallback locale 时是否输出警告。Whether to warn when falling back to the fallback locale.</summary>
    [Description("@#fallbackWarn")]
    public bool? FallbackWarn { get; init; }
}

/// <summary>
/// <c>t()</c> 的命名选项对象；覆盖 named 插值、复数与 locale 等参数。
/// The named options object for <c>t()</c>; it covers named interpolation, pluralization, and locale overrides.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueI18nTranslateOptions
{
    /// <summary>命名插值参数。The named interpolation parameters.</summary>
    [Description("@#named")]
    public Vue.VueDictionary? Named { get; init; }

    /// <summary>复数形式计数。The pluralization count.</summary>
    [Description("@#plural")]
    public Number? Plural { get; init; }

    /// <summary>本次翻译使用的 locale 覆盖值。The locale override for this translation.</summary>
    [Description("@#locale")]
    public string? Locale { get; init; }

    /// <summary>默认消息。The default message.</summary>
    [Description("@#default")]
    public string? Default { get; init; }

    /// <summary>本次翻译是否抑制缺失警告。Whether to suppress the missing warning for this translation.</summary>
    [Description("@#missingWarn")]
    public bool? MissingWarn { get; init; }

    /// <summary>本次翻译是否抑制回退警告。Whether to suppress the fallback warning for this translation.</summary>
    [Description("@#fallbackWarn")]
    public bool? FallbackWarn { get; init; }
}

/// <summary>
/// 由 <c>createI18n()</c> 创建的 i18n 根实例；同一对象既是运行时根，也是 Vue 插件安装目标。
/// The i18n root instance created by <c>createI18n()</c>; the same object is both the runtime root
/// and a Vue plugin install target.
/// </summary>
public abstract record VueI18nInstance : Vue.VuePlugin
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueI18nInstance()
    {
    }

    /// <summary>实例模式：<c>composition</c> 或 <c>legacy</c>。The instance mode: <c>composition</c> or <c>legacy</c>.</summary>
    [Description("@#mode")]
    public extern string Mode { get; }

    /// <summary>全局 Composer。The global Composer.</summary>
    [Description("@#global")]
    public extern VueI18nComposer Global { get; }

    /// <summary>释放实例并清理已挂载的 Composer。Disposes the instance and cleans up mounted Composers.</summary>
    [Description("@#dispose")]
    public extern void Dispose();
}

/// <summary>
/// vue-i18n Composer：locale 切换、消息查找与日期/数字格式化入口。
/// The vue-i18n Composer: locale switching, message lookup, and datetime/number formatting entries.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueI18nComposer
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueI18nComposer()
    {
    }

    /// <summary>Composer 实例标识。The Composer instance identifier.</summary>
    [Description("@#id")]
    public extern Vue.VueReadonlyRef<Number> Id { get; }

    /// <summary>当前 locale（可写引用）。The current locale as a writable ref.</summary>
    [Description("@#locale")]
    public extern Vue.IVueRef<string> Locale { get; }

    /// <summary>回退 locale（可写引用）。The fallback locale as a writable ref.</summary>
    [Description("@#fallbackLocale")]
    public extern Vue.IVueRef<string> FallbackLocale { get; }

    /// <summary>是否继承根作用域的 locale。Whether the root scope locale is inherited.</summary>
    [Description("@#inheritLocale")]
    public extern bool InheritLocale { get; set; }

    /// <summary>可用 locale 列表（按字典序）。The available locales in lexical order.</summary>
    [Description("@#availableLocales")]
    public extern string[] AvailableLocales { get; }

    /// <summary>locale 消息树。The locale message tree.</summary>
    [Description("@#messages")]
    public extern Vue.VueReadonlyRef<Vue.VueDictionary> Messages { get; }

    /// <summary>日期时间格式树。The datetime format tree.</summary>
    [Description("@#datetimeFormats")]
    public extern Vue.VueReadonlyRef<Vue.VueDictionary> DateTimeFormats { get; }

    /// <summary>数字格式树。The number format tree.</summary>
    [Description("@#numberFormats")]
    public extern Vue.VueReadonlyRef<Vue.VueDictionary> NumberFormats { get; }

    /// <summary>是否为全局 Composer。Whether this is the global Composer.</summary>
    [Description("@#isGlobal")]
    public extern Vue.VueReadonlyRef<bool> IsGlobal { get; }

    /// <summary>按 key 翻译消息。Translates a message by key.</summary>
    /// <param name="key">消息 key。The message key.</param>
    [Description("@#t")]
    public extern string T(string key);

    /// <summary>按 key 翻译消息，并按计数选择复数形式。Translates a message by key, selecting the plural form by count.</summary>
    /// <param name="key">消息 key。The message key.</param>
    /// <param name="plural">复数计数。The pluralization count.</param>
    [Description("@#t")]
    public extern string T(string key, Number plural);

    /// <summary>按 key 翻译消息并应用命名插值。Translates a message by key and applies named interpolation.</summary>
    /// <param name="key">消息 key。The message key.</param>
    /// <param name="named">命名插值参数。The named interpolation parameters.</param>
    [Description("@#t")]
    public extern string T(string key, Vue.VueDictionary named);

    /// <summary>按 key 翻译消息，使用完整的选项对象。Translates a message by key using the full options object.</summary>
    /// <param name="key">消息 key。The message key.</param>
    /// <param name="options">翻译选项。The translation options.</param>
    [Description("@#t")]
    public extern string T(string key, VueI18nTranslateOptions options);

    /// <summary>解析当前 locale 下的消息引用（linked message）。Resolves a locale message reference (linked message).</summary>
    /// <param name="message">待解析的消息。The message to resolve.</param>
    [Description("@#rt")]
    public extern string Rt(string message);

    /// <summary>判断某个 key 是否存在可用翻译。Determines whether a key has an available translation.</summary>
    /// <param name="key">消息 key。The message key.</param>
    [Description("@#te")]
    public extern bool Te(string key);

    /// <summary>读取某个 key 下的原始消息对象（含复数分支）。Reads the raw message object for a key, including plural branches.</summary>
    /// <param name="key">消息 key。The message key.</param>
    [Description("@#tm")]
    public extern Vue.VueDictionary Tm(string key);

    /// <summary>格式化日期时间。Formats a datetime value.</summary>
    /// <param name="value">日期时间值。The datetime value.</param>
    /// <param name="format">格式名或格式 key；为空时使用默认格式。The format name or key; the default format is used when omitted.</param>
    /// <param name="locale">locale 覆盖值。The locale override.</param>
    [Description("@#d")]
    public extern string D(Date value, string? format = null, string? locale = null);

    /// <summary>格式化时间戳。Formats a timestamp.</summary>
    /// <param name="value">时间戳（毫秒）。The timestamp in milliseconds.</param>
    /// <param name="format">格式名或格式 key；为空时使用默认格式。The format name or key; the default format is used when omitted.</param>
    /// <param name="locale">locale 覆盖值。The locale override.</param>
    [Description("@#d")]
    public extern string D(Number value, string? format = null, string? locale = null);

    /// <summary>格式化数字。Formats a number.</summary>
    /// <param name="value">数字值。The number value.</param>
    /// <param name="format">格式名或格式 key；为空时使用默认格式。The format name or key; the default format is used when omitted.</param>
    /// <param name="locale">locale 覆盖值。The locale override.</param>
    [Description("@#n")]
    public extern string N(Number value, string? format = null, string? locale = null);

    /// <summary>读取某个 locale 的完整消息树。Reads the whole message tree of a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    [Description("@#getLocaleMessage")]
    public extern Vue.VueDictionary GetLocaleMessage(string locale);

    /// <summary>覆盖某个 locale 的完整消息树。Replaces the whole message tree of a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    /// <param name="messages">新的消息树。The new message tree.</param>
    [Description("@#setLocaleMessage")]
    public extern void SetLocaleMessage(string locale, Vue.VueDictionary messages);

    /// <summary>合并消息到某个 locale。Merges messages into a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    /// <param name="messages">要合并的消息。The messages to merge.</param>
    [Description("@#mergeLocaleMessage")]
    public extern void MergeLocaleMessage(string locale, Vue.VueDictionary messages);

    /// <summary>读取某个 locale 的日期时间格式。Reads the datetime formats of a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    [Description("@#getDateTimeFormat")]
    public extern Vue.VueDictionary GetDateTimeFormat(string locale);

    /// <summary>覆盖某个 locale 的日期时间格式。Replaces the datetime formats of a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    /// <param name="formats">新的格式集合。The new format set.</param>
    [Description("@#setDateTimeFormat")]
    public extern void SetDateTimeFormat(string locale, Vue.VueDictionary formats);

    /// <summary>合并日期时间格式到某个 locale。Merges datetime formats into a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    /// <param name="formats">要合并的格式。The formats to merge.</param>
    [Description("@#mergeDateTimeFormat")]
    public extern void MergeDateTimeFormat(string locale, Vue.VueDictionary formats);

    /// <summary>读取某个 locale 的数字格式。Reads the number formats of a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    [Description("@#getNumberFormat")]
    public extern Vue.VueDictionary GetNumberFormat(string locale);

    /// <summary>覆盖某个 locale 的数字格式。Replaces the number formats of a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    /// <param name="formats">新的格式集合。The new format set.</param>
    [Description("@#setNumberFormat")]
    public extern void SetNumberFormat(string locale, Vue.VueDictionary formats);

    /// <summary>合并数字格式到某个 locale。Merges number formats into a locale.</summary>
    /// <param name="locale">locale 标识。The locale identifier.</param>
    /// <param name="formats">要合并的格式。The formats to merge.</param>
    [Description("@#mergeNumberFormat")]
    public extern void MergeNumberFormat(string locale, Vue.VueDictionary formats);
}
