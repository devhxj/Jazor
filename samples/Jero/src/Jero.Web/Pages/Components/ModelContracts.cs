using System;
using System.ComponentModel;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.Components;

/// <summary>将编辑器的值更新回调映射到 Vue 的 <c>onUpdate:modelValue</c>，类型参数取编辑器实际发出的值类型。</summary>
public sealed record ModelUpdate<T>([property: Description("@#onUpdate:modelValue")] Action<T> Update) : VueProps;
