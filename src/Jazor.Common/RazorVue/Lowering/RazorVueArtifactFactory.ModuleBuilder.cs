using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueArtifactFactory
{
    private static string BuildModuleCode(
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment renderTree,
        RazorVueExpressionEmitter expressionEmitter,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var descriptor = snapshot.Descriptor;
        var builder = new StringBuilder();
        AppendVueImports(builder, snapshot, resolvedComponents);
        var renderExpression = expressionEmitter.EmitFragment(renderTree);
        builder.AppendLine();
        builder.AppendLine("export default defineComponent({");
        builder.Append("  name: \"").Append(descriptor.Name).AppendLine("\",");
        builder.Append("  props: ").Append(FormatStringArray(descriptor.Props.Select(static prop => prop.Name))).AppendLine(",");
        builder.Append("  emits: ").Append(FormatStringArray(descriptor.Emits.Select(static emit => emit.Name))).AppendLine(",");
        builder.AppendLine("  setup(props, { emit, slots, expose, attrs }) {");
        AppendLifecycleLowering(builder, snapshot);
        AppendSetupLogicLowering(builder, snapshot, expressionEmitter);
        builder.Append("    return () => ").Append(renderExpression).AppendLine(";");
        builder.AppendLine("  }");
        builder.AppendLine("});");
        return builder.ToString();
    }

    private static void AppendVueImports(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
    {
        var vueImports = new List<string> { "defineComponent", "h" };
        vueImports.AddRange(RazorVueSetupAndLifecycleLoweringSupport.CollectVueRuntimeImports(snapshot));

        builder.Append("import { ")
            .Append(string.Join(", ", vueImports.Distinct(StringComparer.Ordinal)))
            .AppendLine(" } from \"vue\";");
        AppendComponentImports(builder, resolvedComponents);
    }

    private static void AppendLifecycleLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot)
        => RazorVueSetupAndLifecycleLoweringSupport.AppendLifecycleLowering(builder, snapshot, "    ");

    private static void AppendSetupLogicLowering(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter)
        => RazorVueSetupAndLifecycleLoweringSupport.AppendSetupLogicLowering(
            builder,
            snapshot,
            expressionEmitter,
            ImmutableArray<VueLogicFieldDescriptor>.Empty,
            ImmutableArray<VueLogicMethodDescriptor>.Empty,
            "    ");

    private static string DescribeSetupFieldShape(IFieldSymbol field)
    {
        if (field.DeclaringSyntaxReferences.Length == 0)
            return "unsupported";

        var syntax = field.DeclaringSyntaxReferences[0].GetSyntax();
        if (syntax is not VariableDeclaratorSyntax declarator || declarator.Initializer is null)
            return "unsupported";

        return declarator.Initializer.Value.ToString();
    }

    private static string DescribeSetupMethodShape(IMethodSymbol method)
    {
        if (method.DeclaringSyntaxReferences.Length == 0)
            return "unsupported";

        var syntax = method.DeclaringSyntaxReferences[0].GetSyntax();
        if (syntax is not MethodDeclarationSyntax methodSyntax)
            return "unsupported";

        if (methodSyntax.ExpressionBody is not null)
            return methodSyntax.ExpressionBody.Expression.ToString();

        if (methodSyntax.Body?.Statements.Count == 1 &&
            methodSyntax.Body.Statements[0] is ReturnStatementSyntax returnStatement &&
            returnStatement.Expression is not null)
        {
            return returnStatement.Expression.ToString();
        }

        return "unsupported";
    }

    private static string FormatStringArray(IEnumerable<string> values)
        => "[" + string.Join(", ", values.Select(ToJavaScriptString)) + "]";
}
