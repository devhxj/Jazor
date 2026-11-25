using System;
using System.ComponentModel;
using static ECMAScript.Vue;

namespace Jero.Web.App.Components;

// ValueFormat and the supplied options determine what a particular editor emits.
// Vue's colon-delimited listener name cannot be spelled as a C# property.
public sealed record ModelUpdate<T>([property: Description("@#onUpdate:modelValue")] Action<T> Update) : VueProps;
