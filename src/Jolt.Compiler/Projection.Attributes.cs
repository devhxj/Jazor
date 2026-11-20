using Jolt.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal sealed partial class Projection
{
    private void EmitAttributeSplat(JoltAttribute attribute)
    {
        if (attribute.Value is not [JoltExpression expression] || string.IsNullOrWhiteSpace(expression.Code))
        {
            Report("JOLT1030", attribute.NameSpan, "@attributes requires a C# attribute bag expression.");
            return;
        }
        // Open bags have no statically known keys. EmitProperties still checks
        // required component props against the explicit attributes.
        string method = IntrinsicPrefix + Intrinsics.Count;
        int start = _code.Length;
        Call(new(IntrinsicKind.Attributes, expression.CodeSpan), "object", "global::ECMAScript.Vue.VueProps bag");
        _methods.Add($"private static object {method}(global::ECMAScript.Vue.VueAttributeBag bag) => throw new global::System.NotImplementedException();\n");
        Copy(expression.CodeSpan);
        _anchors.Add((TextSpan.FromBounds(start, _code.Length), expression.CodeSpan));
        Write("), ");
    }
}
