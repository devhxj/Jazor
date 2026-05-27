using Jazor.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueAnalyzerTests
{
    [TestMethod]
    public async Task RazorVue_Entry_ValidVueComponent_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE001", "JAZORVUE002", "JAZORVUE004", "JAZORVUE005", "JAZORVUE006", "JAZORVUE007", "JAZORVUE008", "JAZORVUE009", "JAZORVUE010", "JAZORVUE011", "JAZORVUE012", "JAZORVUE013", "JAZORVUE014", "JAZORVUE015", "JAZORVUE016", "JAZOR001");
    }

    [TestMethod]
    public async Task RazorVue_Entry_ComponentBaseOnly_ReportsJAZORVUE002()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase
            {
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE002");
        AssertNoDiagnostic(diagnostics, "JAZOR001");
    }

    [TestMethod]
    public async Task RazorVue_Entry_StaticModule_RemainsOnLegacyPath()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public static class InvalidModule
            {
                public static Version Value = new();
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZOR001");
        AssertNoDiagnostic(diagnostics, "JAZORVUE001", "JAZORVUE002", "JAZORVUE004", "JAZORVUE005", "JAZORVUE006", "JAZORVUE007", "JAZORVUE008", "JAZORVUE009", "JAZORVUE010", "JAZORVUE011", "JAZORVUE012", "JAZORVUE013", "JAZORVUE014", "JAZORVUE015", "JAZORVUE016");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_StateHasChanged_ReportsJAZORVUE004()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                public void Trigger()
                {
                    StateHasChanged();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE004");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DynamicShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    return Value > 0;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LocalPrefixedShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    var threshold = Value + 1;
                    return threshold > 2;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LocalMutationShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    var props = Value;
                    props++;
                    props += 2;
                    return props > 3;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PropertyMutationShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    Value++;
                    return Value > 0;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PropertyMutationInsideReturnShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    return Value++ > 0;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PropertyMutationInsideLocalMutationValueShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    var props = 0;
                    props = Value++;
                    return props > 0;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_IfElseShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    if (Value < 0)
                    {
                        return false;
                    }

                    return Value > 0;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_IfElseShouldRenderWithBranchLocals_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    if (Value < 0)
                    {
                        var props = Value + 1;
                        return props < 0;
                    }
                    else
                    {
                        var props = Value + 2;
                        return props > 2;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_IfElseShouldRenderWithLocalMutation_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    if (Value > 0)
                    {
                        var props = Value;
                        props--;
                        return props > 0;
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LocalFunctionWithLocalMutationShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    bool IsReady(int value)
                    {
                        var next = value;
                        next++;
                        return next > 2;
                    }

                    return IsReady(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        case 0:
                            return false;
                        case 1:
                            return true;
                        default:
                            return Value > 1;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRenderWithCaseLocalMutation_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        case 0:
                            return false;
                        default:
                            var props = Value;
                            props++;
                            return props > 1;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRenderWithoutDefaultAndTrailingReturn_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        case 0:
                            return false;
                    }

                    return Value > 0;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRenderWithLoopCase_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        default:
                            while (Value > 0)
                            {
                                return true;
                            }

                            return false;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRenderWithPropertyMutation_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        default:
                            Value++;
                            return Value > 0;
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRenderWithGuardedCase_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        case 0 when Value > 1:
                            return false;
                        default:
                            return true;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRenderWithNonExhaustiveGuardedCase_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        case 0 when Value > 1:
                            return false;
                    }

                    return true;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRenderWithGuardedCaseBreakAndTrailingReturn_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        case 0 when Value > 1:
                            break;
                    }

                    return true;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SwitchShouldRenderWithGuardedDeclarationPattern_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public object? Value { get; set; }

                protected override bool ShouldRender()
                {
                    switch (Value)
                    {
                        case int props when props > 1:
                            return props < 10;
                    }

                    return true;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_NestedSwitchShouldRenderWithGuardedCase_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    if (Value >= 0)
                    {
                        switch (Value)
                        {
                            case 0 when Value > 1:
                                return false;
                            default:
                                return true;
                        }
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BlockNestedSwitchShouldRenderWithGuardedCase_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    {
                        switch (Value)
                        {
                            case 0 when Value > 1:
                                return false;
                        }
                    }

                    return true;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LoopShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    while (Value > 0)
                    {
                        return true;
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ForLoopShouldRenderWithBreakAndContinue_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    var seen = 0;
                    for (var index = 0; index < Value; index++)
                    {
                        if (index == 1)
                        {
                            continue;
                        }

                        seen += index;
                        if (seen > 2)
                        {
                            break;
                        }
                    }

                    return seen > 0;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DoWhileLoopShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    var next = Value;
                    do
                    {
                        next--;
                    }
                    while (next > 1);

                    return next == 1;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ForEachLoopShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int[] Values { get; set; } = [];

                protected override bool ShouldRender()
                {
                    foreach (var value in Values)
                    {
                        if (value > 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_NestedForEachLoopShouldRenderWithBreakAndContinue_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int[][] Values { get; set; } = [];

                protected override bool ShouldRender()
                {
                    foreach (var row in Values)
                    {
                        foreach (var value in row)
                        {
                            if (value < 0)
                            {
                                continue;
                            }

                            if (value > 3)
                            {
                                break;
                            }

                            return true;
                        }
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ForEachLoopShouldRenderWithCollectionMutation_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int[] Values { get; set; } = [];

                protected override bool ShouldRender()
                {
                    foreach (var value in Values = [])
                    {
                        if (value > 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LoopShouldRenderWithPropertyMutation_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    while (Value > 0)
                    {
                        Value--;
                    }

                    return true;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ForLoopShouldRenderWithHeaderPropertyMutation_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    for (Value = 0; Value < 3; Value++)
                    {
                        if (Value > 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_TryCatchFinallyShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    var current = Value;
                    try
                    {
                        if (current < 0)
                        {
                            return false;
                        }

                        return current > 0;
                    }
                    catch (Exception error) when (Value > -10)
                    {
                        var fallback = Value + 1;
                        return fallback > 1;
                    }
                    finally
                    {
                        current++;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ThrowBranchShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    if (Value < 0)
                    {
                        throw new InvalidOperationException("negative");
                    }

                    return Value > 0;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ThrowOnlyShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                protected override bool ShouldRender()
                {
                    throw new InvalidOperationException("unsupported");
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_TryCatchShouldRenderWithReservedCatchLocal_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    try
                    {
                        return Value > 0;
                    }
                    catch (Exception props)
                    {
                        return props is not null && Value > 1;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_TryFinallyShouldRenderWithPropertyMutation_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    try
                    {
                        return Value > 0;
                    }
                    finally
                    {
                        Value++;
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_TryFinallyShouldRenderWithTerminalThrow_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    try
                    {
                        return Value > 0;
                    }
                    finally
                    {
                        throw new InvalidOperationException("terminal");
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PatternLocalShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public object? Value { get; set; }

                protected override bool ShouldRender()
                {
                    if (Value is int props)
                    {
                        return props > 0;
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ExpressionPatternLocalShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public object? Value { get; set; }

                protected override bool ShouldRender()
                {
                    return Value is int props && props > 0;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_OutDeclarationShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Value { get; set; }

                protected override bool ShouldRender()
                {
                    if (int.TryParse(Value, out var props))
                    {
                        return props > 0;
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_RecursivePatternDeclaredLocalShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public object? Value { get; set; }

                protected override bool ShouldRender()
                {
                    if (Value is int { } props)
                    {
                        return props > 0;
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ListPatternDeclaredLocalShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int[] Values { get; set; } = [];

                protected override bool ShouldRender()
                {
                    if (Values is [1, ..] props)
                    {
                        return props.Length > 0;
                    }

                    return false;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LocalLambdaShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    Func<int, bool> ready = value => value > 0;
                    return ready(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PropertyMutationInsideLocalLambdaShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    Func<int, bool> ready = value =>
                    {
                        Value++;
                        return value > 0;
                    };

                    return ready(Value);
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PropertyMutationInsideLocalFunctionShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    bool IsReady()
                    {
                        Value++;
                        return Value > 0;
                    }

                    return IsReady();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_MethodGroupDelegateLocalShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    bool IsReady(int value) => value > 0;
                    Func<int, bool> ready = IsReady;
                    return ready(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_CurrentComponentMethodGroupDelegateLocalShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private bool IsReady(int value)
                {
                    var next = value + 1;
                    return next > 1;
                }

                protected override bool ShouldRender()
                {
                    Func<int, bool> ready = IsReady;
                    return ready(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PropertyMutationInsideMethodGroupDelegateShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                private bool IsReady(int value)
                {
                    Value++;
                    return value > 0;
                }

                protected override bool ShouldRender()
                {
                    Func<int, bool> ready = IsReady;
                    return ready(Value);
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DelegateValueUseShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    Func<int, bool> ready = value => value > 0;
                    return ready is not null;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ConstantTrueShouldRender_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                protected override bool ShouldRender()
                {
                    return true;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ComponentBaseShouldRenderPassThrough_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                protected override bool ShouldRender()
                {
                    return base.ShouldRender();
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PassThroughShouldRenderToSupportedBase_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            public abstract class ShouldRenderBaseComponent : ComponentBase, IVueComponent
            {
                protected override bool ShouldRender()
                {
                    return true;
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ShouldRenderBaseComponent
            {
                protected override bool ShouldRender()
                {
                    return base.ShouldRender();
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PassThroughShouldRenderToDynamicBase_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            public abstract class ShouldRenderBaseComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override bool ShouldRender()
                {
                    return Value > 0;
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ShouldRenderBaseComponent
            {
                protected override bool ShouldRender()
                {
                    return base.ShouldRender();
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseOnlySetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                public override Task SetParametersAsync(ParameterView parameters)
                {
                    return base.SetParametersAsync(parameters);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ExpressionBodiedNoOpSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                public override Task SetParametersAsync(ParameterView parameters)
                    => Task.CompletedTask;
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DefaultTaskOnInitializedAsync_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                protected override Task OnInitializedAsync()
                    => default;
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DefaultValueTaskDisposeAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                public ValueTask DisposeAsync()
                    => default;
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DeclarationInitializedPropertyLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<string> ValueChanged { get; set; }

                private string Prefix { get; } = "Count: ";

                protected override void OnParametersSet()
                {
                    ValueChanged.InvokeAsync(Prefix);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SourceStableLocalLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<string> ValueChanged { get; set; }

                private string Prefix => "Count: ";

                protected override void OnParametersSet()
                {
                    var label = Prefix + Value;
                    ValueChanged.InvokeAsync(label + "!");
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LocalFunctionLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<string> ValueChanged { get; set; }

                protected override void OnParametersSet()
                {
                    string FormatLabel(int value) => "Count: " + value;
                    ValueChanged.InvokeAsync(FormatLabel(Value));
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_CallableLocalLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                protected override void OnParametersSet()
                {
                    Func<int, int> increment = static value => value + 1;
                    ValueChanged.InvokeAsync(increment(Value));
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LocalNamedFirstRenderOnInitializedPayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidInitializedComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                protected override void OnInitialized()
                {
                    var firstRender = 1;
                    ValueChanged.InvokeAsync(firstRender);
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidInitializedAsyncComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                protected override Task OnInitializedAsync()
                {
                    var firstRender = 1;
                    return ValueChanged.InvokeAsync(firstRender);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_HelperCallWithOptionalParamsAndNamedLifecyclePayloads_AreAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidOptionalComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<string> LabelChanged { get; set; }

                private static string FormatLabel(int value, string prefix = "Count: ")
                    => prefix + value;

                protected override void OnParametersSet()
                {
                    LabelChanged.InvokeAsync(FormatLabel(Value));
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidParamsComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> CountChanged { get; set; }

                private static int CountValues(params int[] values)
                    => values.Length;

                protected override void OnParametersSet()
                {
                    CountChanged.InvokeAsync(CountValues(Value, 42));
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidNamedComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<string> LabelChanged { get; set; }

                private static string FormatNamed(int value, string prefix, string suffix)
                    => prefix + value + suffix;

                protected override void OnParametersSet()
                {
                    LabelChanged.InvokeAsync(FormatNamed(suffix: "!", value: Value, prefix: "Count: "));
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DeclarationInitializedFieldLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<int> ReadyChanged { get; set; }

                private readonly int _readyCode = 7;

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    return ReadyChanged.InvokeAsync(_readyCode);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_HelperCallLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<string> ValueChanged { get; set; }

                private string Prefix { get; } = "Count: ";

                private string FormatLabel()
                    => Prefix;

                protected override void OnInitialized()
                {
                    ValueChanged.InvokeAsync(FormatLabel());
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_AsyncHelperCallLifecyclePayload_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<string> ValueChanged { get; set; }

                private Task<string> FormatLabelAsync()
                    => Task.FromResult("Count: ");

                protected override void OnInitialized()
                {
                    ValueChanged.InvokeAsync(FormatLabelAsync().Result);
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LocalAliasFirstRenderLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    var alias = firstRender;
                    return ReadyChanged.InvokeAsync(alias);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_LocalFunctionCapturedFirstRenderLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    bool NormalizeReady() => firstRender;
                    return ReadyChanged.InvokeAsync(NormalizeReady());
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_CoalescedFirstRenderLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    bool? alias = firstRender;
                    return ReadyChanged.InvokeAsync(alias ?? false);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PatternFirstRenderLifecyclePayloads_AreAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    return ReadyChanged.InvokeAsync(
                        firstRender is true or false &&
                        firstRender is not false &&
                        firstRender is bool);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DeclarationPatternFirstRenderLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    return ReadyChanged.InvokeAsync(firstRender is bool ready && ready);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ArrayIndexerFirstRenderLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    var readyStates = new[] { false, firstRender };
                    return ReadyChanged.InvokeAsync(readyStates[1]);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ArrayPatternFirstRenderLifecyclePayload_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    var readyStates = new[] { false, firstRender };
                    var payload = readyStates is [_, var ready] ? ready : false;
                    return ReadyChanged.InvokeAsync(payload);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenConditionalEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    if (Value > 0)
                    {
                        await ValueChanged.InvokeAsync(Value);
                        await ReadyChanged.InvokeAsync(true);
                    }
                    else
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenLocalConditionEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    if (ready)
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    else
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenSwitchEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    switch (Value)
                    {
                        case 0:
                            await ReadyChanged.InvokeAsync(false);
                            break;
                        case 1:
                        case 2:
                            await ValueChanged.InvokeAsync(Value);
                            await ReadyChanged.InvokeAsync(true);
                            break;
                        default:
                            await ReadyChanged.InvokeAsync(Value > 0);
                            break;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryFinallyEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenGuardReturnEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    if (!ready)
                    {
                        return;
                    }

                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenIfReturnElseEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    if (!ready)
                    {
                        return;
                    }
                    else
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenIfEmitElseReturnSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    if (ready)
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenSourceStableLocalEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<string> ValueChanged { get; set; }

                private string Prefix => "Count: ";

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var label = Prefix + Value;
                    await ValueChanged.InvokeAsync(label + "!");
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SetParametersAsyncEmitWithoutBasePassThrough_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SetParametersAsyncBaseThenReturnWithUnreachableEmit_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    return;
                    await ValueChanged.InvokeAsync(Value + 1);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTerminalGuardReturnSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    if (!ready)
                    {
                        return;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTerminalGuardReturnWithSideEffectConditionSetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                public bool IsReady()
                {
                    Value++;
                    return Value > 0;
                }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    if (!IsReady())
                    {
                        return;
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenLocalFunctionEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    int Increment(int value) => value + 1;
                    await ValueChanged.InvokeAsync(Increment(Value));
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenCallableLocalEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    Func<int, int> increment = static value => value + 1;
                    await ValueChanged.InvokeAsync(increment(Value));
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_NonTrivialSetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    Value++;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenConditionalMutationSetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    if (Value > 0)
                    {
                        Value++;
                    }
                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchEmptyTryRecoverySetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                    }
                    catch (Exception)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenCatchAllEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenEmptyCatchSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenCompletedTaskCatchSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception)
                    {
                        await Task.CompletedTask;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryFinallyEmptyTryCleanupSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryFinallyCompletedTaskTryCleanupSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                        await Task.CompletedTask;
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenUnusedCatchVariableEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception error)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenCatchFilterEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception) when (ready)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchFinallyEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchFinallyEmptyFinallySetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                    finally
                    {
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchFinallyCompletedTaskFinallySetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                    finally
                    {
                        await Task.CompletedTask;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchFinallyEmptyTryCatchCleanupSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                    }
                    catch (Exception)
                    {
                        await Task.CompletedTask;
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchFinallyUnusedCatchVariableEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception error)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryFinallyGuardReturnEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                        if (!ready)
                        {
                            return;
                        }

                        await ValueChanged.InvokeAsync(Value);
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryFinallyBareReturnSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        return;
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(Value > 0);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchFinallyGuardReturnEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                        if (!ready)
                        {
                            return;
                        }

                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchFilterFinallyGuardReturnEmitSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    var ready = Value > 0;
                    try
                    {
                        if (!ready)
                        {
                            return;
                        }

                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception) when (ready)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                    finally
                    {
                        await ReadyChanged.InvokeAsync(ready);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenCatchVariableSetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception error)
                    {
                        await ReadyChanged.InvokeAsync(error is not null);
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenCatchVariableFilterSetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    catch (Exception error) when (error is not null)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenFinallyMutationSetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                    finally
                    {
                        Value++;
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchFinallyBareReturnSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        return;
                    }
                    catch (Exception)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                    finally
                    {
                        await ValueChanged.InvokeAsync(Value);
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenTryCatchBareReturnWithoutFinallySetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<bool> ReadyChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    try
                    {
                        return;
                    }
                    catch (Exception)
                    {
                        await ReadyChanged.InvokeAsync(false);
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenDoubleReturnIfElseSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    if (Value <= 0)
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }

                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseThenEmitThenDoubleReturnIfElseSetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    await ValueChanged.InvokeAsync(Value);
                    if (Value <= 0)
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SupportedSetParametersAsyncBaseThenEmit_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PassThroughSetParametersAsyncToSupportedBaseEmit_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            public abstract class SetParametersAsyncBaseComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    await ValueChanged.InvokeAsync(Value);
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : SetParametersAsyncBaseComponent
            {
                public override Task SetParametersAsync(ParameterView parameters)
                {
                    return base.SetParametersAsync(parameters);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_SetParametersAsyncWithBaseEmitAndDerivedEmit_DoesNotReportJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            public abstract class SetParametersAsyncBaseComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    await ValueChanged.InvokeAsync(Value);
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : SetParametersAsyncBaseComponent
            {
                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_PassThroughSetParametersAsyncToExternalOverrideWithoutSource_ReportsJAZORVUE006()
    {
        var baseCompilation = CSharpCompilation.Create(
            assemblyName: "External.SetParametersAsync.Base",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(
                """
                using System.Threading.Tasks;
                using ECMAScript.VueContract;
                using Microsoft.AspNetCore.Components;

                public abstract class ExternalSetParametersAsyncBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    public override async Task SetParametersAsync(ParameterView parameters)
                    {
                        await base.SetParametersAsync(parameters);
                        Value++;
                    }
                }
                """),
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new System.IO.MemoryStream();
        var emitResult = baseCompilation.Emit(image);
        Assert.IsTrue(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static x => x.ToString())));

        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ExternalSetParametersAsyncBase
            {
                public override Task SetParametersAsync(ParameterView parameters)
                {
                    return base.SetParametersAsync(parameters);
                }
            }
            """,
            MetadataReference.CreateFromImage(image.ToArray()));

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_UnknownLibraryParameter_ReportsJAZORVUE007()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VBtn>(0);
                    builder.AddAttribute(1, "DefinitelyMissing", "#");
                    builder.CloseComponent();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE007");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DuplicateLibraryMappedParameter_ReportsJAZORVUE007()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VSelect>(0);
                    builder.AddAttribute(1, nameof(VSelect.ModelValue), "admin");
                    builder.AddAttribute(2, nameof(VSelect.SelectedValue), VuetifySelectModelValue.From("user"));
                    builder.CloseComponent();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE007");
        var diagnostic = diagnostics.Single(static diagnostic => diagnostic.Id == "JAZORVUE007");
        StringAssert.Contains(diagnostic.GetMessage(), "ModelValue");
        StringAssert.Contains(diagnostic.GetMessage(), "SelectedValue");
        StringAssert.Contains(diagnostic.GetMessage(), "modelValue");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_InvalidLibraryBindTarget_ReportsJAZORVUE008()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                public string? Text { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VBtn>(0);
                    builder.AddAttribute(1, nameof(VBtn.Text), Text);
                    builder.AddAttribute(2, "TextChanged", EventCallback.Factory.Create<string?>(this, HandleTextChanged));
                    builder.CloseComponent();
                }

                private void HandleTextChanged(string? value)
                {
                    Text = value;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE008");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ValidLibraryParameter_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Label { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VTextField>(0);
                    builder.AddAttribute(1, nameof(VTextField.Label), Label);
                    builder.CloseComponent();
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE007", "JAZORVUE008");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ValidLibraryBindTarget_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Value { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VTextField>(0);
                    builder.AddAttribute(1, nameof(VTextField.ModelValue), Value);
                    builder.AddAttribute(2, nameof(VTextField.ModelValueChanged), EventCallback.Factory.Create<string?>(this, HandleModelValueChanged));
                    builder.CloseComponent();
                }

                private void HandleModelValueChanged(string? value)
                {
                    Value = value;
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE007", "JAZORVUE008");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_UnknownLibrarySlot_ReportsJAZORVUE009()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public RenderFragment? ChildContent { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VDivider>(0);
                    builder.AddContent(1, ChildContent);
                    builder.CloseComponent();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE009");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_TypedLibrarySlotContextMisuse_ReportsJAZORVUE010()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VDialog>(0);
                    builder.AddAttribute(1, nameof(VDialog.Activator), "not-callable");
                    builder.CloseComponent();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE010");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DuplicateLibrarySlotAssignment_ReportsJAZORVUE011()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VDialog>(0);
                    builder.AddAttribute(1, nameof(VDialog.Activator), Activator);
                    builder.AddAttribute(2, nameof(VDialog.Activator), Activator);
                    builder.CloseComponent();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE011");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_MissingSlotValue_ReportsJAZORVUE015()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/parent-card")]
                public class ParentCard : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ChildCard>(0);
                        builder.AddAttribute(1, "Header");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE015");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ValidTypedLibrarySlot_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : ComponentBase, IVueComponent
            {
                [Parameter]
                public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VDialog>(0);
                    builder.AddAttribute(1, nameof(VDialog.ActivatorTarget), VuetifyDialogActivatorTarget.Parent());
                    builder.AddAttribute(2, nameof(VDialog.Activator), Activator);
                    builder.CloseComponent();
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE007", "JAZORVUE009", "JAZORVUE010", "JAZORVUE011", "JAZORVUE015");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_NonCallableTypedUserSlot_ReportsJAZORVUE010()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/child")]
                public class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/host")]
                public class Host : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddAttribute(1, "ItemTemplate", "not-callable");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE010");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_DefaultSlotForwardingToNamedUserSlot_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public class Panel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/page")]
                public class Page : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Panel>(0);
                        builder.AddAttribute(1, "Header", ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE009", "JAZORVUE010", "JAZORVUE011", "JAZORVUE015");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_InvalidLibraryComponentDeclaration_ReportsJAZORVUE012()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            public sealed class InvalidLibraryComponent : ComponentBase, IVueLibraryComponent
            {
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE012");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_InvalidLibraryStyleDependencyDeclaration_ReportsJAZORVUE013()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [VueLibraryComponent("demo/components", "DemoPanel")]
            [VueLibraryStyle("demo/styles")]
            [VueLibraryStyle(" demo/styles ")]
            public sealed class InvalidLibraryComponent : ComponentBase, IVueLibraryComponent
            {
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE013");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_InvalidLibraryPluginRequirementDeclaration_ReportsJAZORVUE014()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [VueLibraryComponent("demo/components", "DemoPanel")]
            [VueLibraryPluginRequirement("demo-host")]
            [VueLibraryPluginRequirement(" demo-host ")]
            public sealed class InvalidLibraryComponent : ComponentBase, IVueLibraryComponent
            {
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE014");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_InvalidComponentCaptureUnmatchedValuesDeclaration_ReportsJAZORVUE016()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Collections.Generic;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/panel")]
                public sealed class InvalidPanel : ComponentBase, IVueComponent
                {
                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyList<string>? AdditionalAttributes { get; set; }
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE016");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ValidLibraryMetadata_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [VueLibraryComponent("demo/components", "DemoPanel")]
            [VueLibraryStyle("demo/styles")]
            [VueLibraryPluginRequirement("demo-host")]
            public sealed class ValidLibraryComponent : ComponentBase, IVueLibraryComponent
            {
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE012", "JAZORVUE013", "JAZORVUE014", "JAZORVUE015", "JAZORVUE016");
    }

    [TestMethod]
    public async Task RazorVue_ContainerInject_DuplicateRegistrations_ReportJAZORVUE018_WithoutUsageSite()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]
            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.VuetifyNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                }

                [VueLibraryComponent("vuetify/components", "VNavigationDrawer")]
                public sealed class VuetifyNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                }
            }
            """);

        AssertHasDiagnosticContaining(diagnostics, "JAZORVUE018", "duplicate implementations");
    }

    [TestMethod]
    public async Task RazorVue_ContainerInject_MissingCompatibleImplementationProp_ReportJAZORVUE018_WithoutUsageSite()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueProp(nameof(Title), Name = "title")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }
            """);

        AssertHasDiagnosticContaining(diagnostics, "JAZORVUE018", "missing compatible prop");
        AssertHasDiagnosticContaining(diagnostics, "JAZORVUE018", "Subtitle");
    }

    [TestMethod]
    public async Task RazorVue_ContainerInject_MismatchedImplementationContract_ReportJAZORVUE018_WithoutUsageSite()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.WrongNavShell))]

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./containers/secondary-shell")]
                public sealed class SecondaryShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                public sealed class WrongNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.SecondaryShell>
                {
                }
            }
            """);

        AssertHasDiagnosticContaining(diagnostics, "JAZORVUE018", "declares container contract");
        AssertHasDiagnosticContaining(diagnostics, "JAZORVUE018", "Demo.Containers.SecondaryShell");
    }

    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source, params MetadataReference[] additionalReferences)
    {
        var compilation = CreateCompilation(source, additionalReferences);
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new Jazor.Analyzer.Analyzer(),
            new RazorVueEntryAnalyzer(),
            new RazorVueMisuseAnalyzer(),
            new RazorVueAuthoringAnalyzer());

        var compileErrors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, compileErrors.Length, string.Join(Environment.NewLine, compileErrors.Select(static x => x.ToString())));

        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
    }

    private static CSharpCompilation CreateCompilation(string source, params MetadataReference[] additionalReferences)
    {
        var references = RazorVueMetadataReferences.Create(additionalReferences);

        return CSharpCompilation.Create(
            assemblyName: "RazorVue.Analyzer.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string id)
        => Assert.IsTrue(
            diagnostics.Any(diagnostic => diagnostic.Id == id),
            $"Expected diagnostic {id}, actual: {string.Join(Environment.NewLine, diagnostics.Select(static x => x.ToString()))}");

    private static void AssertHasDiagnosticContaining(IEnumerable<Diagnostic> diagnostics, string id, string fragment)
        => Assert.IsTrue(
            diagnostics.Any(diagnostic =>
                diagnostic.Id == id &&
                diagnostic.GetMessage().Contains(fragment, StringComparison.Ordinal)),
            $"Expected diagnostic {id} containing '{fragment}', actual: {string.Join(Environment.NewLine, diagnostics.Select(static x => x.ToString()))}");

    private static void AssertNoDiagnostic(IEnumerable<Diagnostic> diagnostics, params string[] ids)
    {
        var unexpected = diagnostics
            .Where(diagnostic => ids.Contains(diagnostic.Id, StringComparer.Ordinal))
            .ToArray();

        Assert.AreEqual(0, unexpected.Length, string.Join(Environment.NewLine, unexpected.Select(static x => x.ToString())));
    }
}
