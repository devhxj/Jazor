namespace ECMAScript.TDesign;

/// <summary>
/// Export surface generated from the frozen TDesign runtime catalog.
/// </summary>
/// <remarks>
/// 这里声明的是聚合入口，供各成员作默认值；成员自身的 <c>[ECMAScript]</c> 会覆盖它。
/// 取值必须是上游公开入口（<c>es/index.mjs</c>），不是 Jazor 自造的简写。
/// </remarks>
[ECMAScript("tdesign-vue-next/es/index.mjs")]
public static partial class TComponents
{
}
