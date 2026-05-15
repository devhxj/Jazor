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
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        out ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
    {
        var descriptor = snapshot.Descriptor;
        var renderExpression = expressionEmitter.EmitFragment(renderTree);
        var requiresAttributeMergeHelper = RazorVueAttributeMergeHelper.ContainsInvocation(renderExpression);
        var propDefaultBindings = CollectPropDefaultBindings(snapshot, descriptor, expressionEmitter);
        var setupBodyBuilder = new StringBuilder();
        setupBodyBuilder.AppendLine("  setup(__jazorRawProps, { emit, slots, expose, attrs }) {");
        AppendPropsBinding(setupBodyBuilder, propDefaultBindings);
        if (RazorVueForLoopLoweringSupport.ContainsForLoop(renderTree))
            RazorVueForLoopLoweringSupport.AppendForRangeHelper(setupBodyBuilder, "    ");
        if (requiresAttributeMergeHelper)
            RazorVueAttributeMergeHelper.AppendHelper(setupBodyBuilder, "    ");
        AppendLifecycleLowering(setupBodyBuilder, snapshot);
        AppendSetupLogicLowering(setupBodyBuilder, snapshot, expressionEmitter);
        setupBodyBuilder.Append("    return () => ").Append(renderExpression).AppendLine(";");
        setupBodyBuilder.AppendLine("  }");

        compilerImports = expressionEmitter.FlushCompilerImports();

        var builder = new StringBuilder();
        AppendVueImports(builder, snapshot, resolvedComponents, compilerImports);
        builder.AppendLine();
        builder.AppendLine("export default defineComponent({");
        builder.Append("  name: \"").Append(descriptor.Name).AppendLine("\",");
        builder.Append("  props: ").Append(FormatStringArray(descriptor.Props.Select(static prop => prop.Name))).AppendLine(",");
        builder.Append("  emits: ").Append(FormatStringArray(descriptor.Emits.Select(static emit => emit.Name))).AppendLine(",");
        builder.Append(setupBodyBuilder);
        builder.AppendLine("});");
        return builder.ToString();
    }

    private static void AppendVueImports(
        StringBuilder builder,
        RazorVueSemanticSnapshot snapshot,
        ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
    {
        var vueImports = new List<string> { "defineComponent", "h" };
        vueImports.AddRange(RazorVueSetupAndLifecycleLoweringSupport.CollectVueRuntimeImports(snapshot));

        builder.Append("import { ")
            .Append(string.Join(", ", vueImports.Distinct(StringComparer.Ordinal)))
            .AppendLine(" } from \"vue\";");
        RazorVueCompilerImportFormatter.AppendImportStatements(builder, compilerImports);
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

    private static ImmutableArray<PropDefaultBinding> CollectPropDefaultBindings(
        RazorVueSemanticSnapshot snapshot,
        VueComponentDescriptor descriptor,
        RazorVueExpressionEmitter expressionEmitter)
    {
        if (descriptor.Props.IsDefaultOrEmpty)
            return ImmutableArray<PropDefaultBinding>.Empty;

        var builder = ImmutableArray.CreateBuilder<PropDefaultBinding>();
        foreach (var prop in descriptor.Props)
        {
            if (prop.DefaultSource != VuePropDefaultSource.PropertyInitializer ||
                string.IsNullOrWhiteSpace(prop.DefaultExpression))
                continue;

            var expression = LowerDefaultExpression(snapshot, expressionEmitter, prop);
            builder.Add(new PropDefaultBinding(prop.Name, expression));
        }

        return builder.ToImmutable();
    }

    private static void AppendPropsBinding(
        StringBuilder builder,
        ImmutableArray<PropDefaultBinding> propDefaultBindings)
    {
        if (propDefaultBindings.IsDefaultOrEmpty)
        {
            builder.AppendLine("    const props = __jazorRawProps;");
            return;
        }

        AppendPropDefaultProxy(builder, propDefaultBindings);
    }

    private static void AppendPropDefaultProxy(
        StringBuilder builder,
        ImmutableArray<PropDefaultBinding> propDefaultBindings)
    {
        builder.AppendLine("    const __jazorPropDefaultCache = Object.create(null);");
        builder.AppendLine("    const props = new Proxy(__jazorRawProps, {");
        builder.AppendLine("      get(target, key, receiver) {");
        builder.AppendLine("        if (typeof key === \"string\") {");
        foreach (var binding in propDefaultBindings)
        {
            builder.Append("          if (key === ")
                .Append(ToJavaScriptString(binding.PropName))
                .AppendLine(") {");
            builder.AppendLine("            const value = Reflect.get(target, key, receiver);");
            builder.AppendLine("            if (value !== undefined) return value;");
            builder.AppendLine("            if (Object.prototype.hasOwnProperty.call(__jazorPropDefaultCache, key)) return __jazorPropDefaultCache[key];");
            builder.Append("            const defaultValue = ")
                .Append(binding.ExpressionText)
                .AppendLine(";");
            builder.AppendLine("            __jazorPropDefaultCache[key] = defaultValue;");
            builder.AppendLine("            return defaultValue;");
            builder.AppendLine("          }");
        }

        builder.AppendLine("        }");
        builder.AppendLine("        return Reflect.get(target, key, receiver);");
        builder.AppendLine("      }");
        builder.AppendLine("    });");
    }

    private static string LowerDefaultExpression(
        RazorVueSemanticSnapshot snapshot,
        RazorVueExpressionEmitter expressionEmitter,
        VuePropDescriptor prop)
    {
        if (prop.DefaultSource != VuePropDefaultSource.PropertyInitializer ||
            string.IsNullOrWhiteSpace(prop.DefaultExpression))
            throw new InvalidOperationException($"Prop '{prop.PublicName}' does not declare a default expression.");

        var propertySymbol = snapshot.ComponentSymbol
            .GetMembers(prop.PublicName)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(static candidate => !candidate.IsStatic);
        if (propertySymbol is null ||
            propertySymbol.DeclaringSyntaxReferences.Length == 0)
        {
            return prop.DefaultExpression!;
        }

        foreach (var reference in propertySymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration ||
                declaration.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = snapshot.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (!RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declaration.Initializer.Value,
                    out var operation))
            {
                continue;
            }

            return expressionEmitter.EmitSetupExpression(operation!);
        }

        return prop.DefaultExpression!;
    }

    private sealed record PropDefaultBinding(string PropName, string ExpressionText);

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
