using System;

namespace Jazor.RazorVue.Runtime;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RazorVueRazorIrCarrierAttribute : Attribute
{
    public RazorVueRazorIrCarrierAttribute(
        string documentPath,
        string importsJson,
        string documentText)
    {
        DocumentPath = documentPath ?? throw new ArgumentNullException(nameof(documentPath));
        ImportsJson = importsJson ?? throw new ArgumentNullException(nameof(importsJson));
        DocumentText = documentText ?? throw new ArgumentNullException(nameof(documentText));
    }

    public string DocumentPath { get; }

    public string ImportsJson { get; }

    public string DocumentText { get; }
}
