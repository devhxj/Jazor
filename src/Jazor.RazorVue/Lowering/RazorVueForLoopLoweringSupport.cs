using System.Text;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Lowering;

internal static class RazorVueForLoopLoweringSupport
{
    public static bool ContainsForLoop(RazorVueRenderFragment fragment)
    {
        if (fragment.Children.IsDefaultOrEmpty)
            return false;

        foreach (var child in fragment.Children)
        {
            switch (child)
            {
                case RazorVueForNode:
                    return true;
                case RazorVueElementNode element when ContainsForLoop(element.Children):
                    return true;
                case RazorVueComponentNode component:
                    if (ContainsForLoop(component.Children))
                        return true;
                    foreach (var slotTemplate in component.SlotTemplates)
                    {
                        if (ContainsForLoop(slotTemplate.Children))
                            return true;
                    }
                    break;
                case RazorVueConditionalNode conditional when ContainsForLoop(conditional.WhenTrue) || ContainsForLoop(conditional.WhenFalse):
                    return true;
                case RazorVueForEachNode loop when ContainsForLoop(loop.Body):
                    return true;
            }
        }

        return false;
    }

    public static bool ContainsForLoop(RazorVueCanonicalTemplateFragment fragment)
    {
        foreach (var child in fragment.Children)
        {
            switch (child)
            {
                case RazorVueCanonicalForNode:
                    return true;
                case RazorVueCanonicalElementNode element when ContainsForLoop(element.Children):
                    return true;
                case RazorVueCanonicalComponentNode component:
                    if (ContainsForLoop(component.Children))
                        return true;
                    foreach (var slot in component.Slots)
                    {
                        if (ContainsForLoop(slot.Children))
                            return true;
                    }
                    break;
                case RazorVueCanonicalConditionalNode conditional when ContainsForLoop(conditional.WhenTrue) || ContainsForLoop(conditional.WhenFalse):
                    return true;
                case RazorVueCanonicalForEachNode loop when ContainsForLoop(loop.Body):
                    return true;
            }
        }

        return false;
    }

    public static void AppendForRangeHelper(StringBuilder builder, string indent)
    {
        builder.Append(indent).AppendLine("const __jazorVueForRange = (start, limit, conditionOperator, stepOperator, stepValue) => {");
        builder.Append(indent).AppendLine("  const values = [];");
        builder.Append(indent).AppendLine("  const resolvedStep = stepValue ?? 1;");
        builder.Append(indent).AppendLine("  for (let current = start; ; ) {");
        builder.Append(indent).AppendLine("    const shouldContinue = conditionOperator === \"<\" ? current < limit");
        builder.Append(indent).AppendLine("      : conditionOperator === \"<=\" ? current <= limit");
        builder.Append(indent).AppendLine("      : conditionOperator === \">\" ? current > limit");
        builder.Append(indent).AppendLine("      : conditionOperator === \">=\" ? current >= limit");
        builder.Append(indent).AppendLine("      : (() => { throw new Error(\"RazorVue for-loop helper received an unsupported condition operator.\"); })();");
        builder.Append(indent).AppendLine("    if (!shouldContinue) {");
        builder.Append(indent).AppendLine("      break;");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    const stepDelta = stepOperator === \"++\" ? 1");
        builder.Append(indent).AppendLine("      : stepOperator === \"--\" ? -1");
        builder.Append(indent).AppendLine("      : stepOperator === \"+=\" ? resolvedStep");
        builder.Append(indent).AppendLine("      : stepOperator === \"-=\" ? -resolvedStep");
        builder.Append(indent).AppendLine("      : (() => { throw new Error(\"RazorVue for-loop helper received an unsupported step operator.\"); })();");
        builder.Append(indent).AppendLine("    if (typeof stepDelta !== \"number\" || !Number.isFinite(stepDelta) || stepDelta === 0) {");
        builder.Append(indent).AppendLine("      throw new Error(\"RazorVue for-loop helper requires a finite non-zero effective step value.\");");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    const expectsPositiveProgress = conditionOperator === \"<\" || conditionOperator === \"<=\";");
        builder.Append(indent).AppendLine("    if ((expectsPositiveProgress && stepDelta < 0) || (!expectsPositiveProgress && stepDelta > 0)) {");
        builder.Append(indent).AppendLine("      throw new Error(\"RazorVue for-loop helper detected a step direction that moves away from the loop limit.\");");
        builder.Append(indent).AppendLine("    }");
        builder.Append(indent).AppendLine("    values.push(current);");
        builder.Append(indent).AppendLine("    current += stepDelta;");
        builder.Append(indent).AppendLine("  }");
        builder.Append(indent).AppendLine("  return values;");
        builder.Append(indent).AppendLine("};");
    }

    public static string GetForConditionOperator(RazorVueForConditionKind conditionKind)
        => conditionKind switch
        {
            RazorVueForConditionKind.LessThan => "<",
            RazorVueForConditionKind.LessThanOrEqual => "<=",
            RazorVueForConditionKind.GreaterThan => ">",
            RazorVueForConditionKind.GreaterThanOrEqual => ">=",
            _ => throw new NotSupportedException($"Unsupported RazorVue for condition kind '{conditionKind}'.")
        };

    public static string GetForStepOperator(RazorVueForStepKind stepKind)
        => stepKind switch
        {
            RazorVueForStepKind.Increment => "++",
            RazorVueForStepKind.Decrement => "--",
            RazorVueForStepKind.AddAssign => "+=",
            RazorVueForStepKind.SubtractAssign => "-=",
            _ => throw new NotSupportedException($"Unsupported RazorVue for step kind '{stepKind}'.")
        };
}
