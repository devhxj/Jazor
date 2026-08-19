namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderFailureCaseCatalog
{
    private static void AddBoundaryDiagnosticCases(List<DirectRenderFailureCase> cases)
    {
        AddBoundaryCaseFamily(cases, "unresolved_render_fragment", CreateUnresolvedRenderFragmentCase);
        AddBoundaryCaseFamily(cases, "unsupported_render_fragment_factory", CreateUnsupportedRenderFragmentFactoryCase);
        AddBoundaryCaseFamily(cases, "recursive_render_fragment_method_group", CreateRecursiveRenderFragmentMethodGroupCase);
        AddBoundaryCaseFamily(cases, "unclosed_render_fragment_helper", CreateUnclosedRenderFragmentHelperCase);
        AddBoundaryCaseFamily(cases, "set_attribute_without_preceding_attribute", CreateSetAttributeWithoutPrecedingAttributeCase);
        AddBoundaryCaseFamily(cases, "dynamic_updates_attribute_name", CreateDynamicUpdatesAttributeNameCase);
        AddBoundaryCaseFamily(cases, "dynamic_event_modifier_name", CreateDynamicEventModifierNameCase);
        AddBoundaryCaseFamily(cases, "side_effect_sequence", CreateSideEffectSequenceCase);
        AddBoundaryCaseFamily(cases, "dynamic_bulk_attribute_name", CreateDynamicBulkAttributeNameCase);
        AddBoundaryCaseFamily(cases, "unclosed_render_fragment_method_group", CreateUnclosedRenderFragmentMethodGroupCase);
        AddBoundaryCaseFamily(cases, "unsupported_event_callback_binder", CreateUnsupportedEventCallbackBinderCase);
        AddBoundaryCaseFamily(cases, "unsupported_render_tree_builder_extension", CreateUnsupportedRenderTreeBuilderExtensionCase);
        AddBoundaryCaseFamily(cases, "unresolvable_component_import", CreateUnresolvableComponentImportCase);
        AddBoundaryCaseFamily(cases, "unsupported_helper_invocation_shape", CreateUnsupportedHelperInvocationShapeCase);
        AddBoundaryCaseFamily(cases, "unsupported_event_callback_binder_handler_shape", CreateUnsupportedEventCallbackBinderHandlerShapeCase);
    }

    private static void AddBoundaryCaseFamily(
        List<DirectRenderFailureCase> cases,
        string family,
        BoundaryCaseFactory createCase)
    {
        for (var variant = 0; variant < 4; variant++)
        {
            var marker = "boundary-" + family + "-" + variant.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var spec = createCase(variant, marker);
            cases.Add(new DirectRenderFailureCase(
                family + "_" + variant.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "DirectRenderFailure" + cases.Count.ToString("D3", System.Globalization.CultureInfo.InvariantCulture),
                spec.Body,
                spec.Members,
                spec.ExpectedFailureFragment,
                Scenario: null));
        }
    }

    private static BoundaryCaseSpec CreateUnresolvedRenderFragmentCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.AddContent(0, ExternalRenderFragments.Fragment);",
                "RenderFragment content requires a resolvable inline, local, helper, or component-slot source."),
            1 => new(
                "RenderFragment fragment = ExternalRenderFragments.Fragment; builder.AddContent(0, fragment);",
                "External type 'RazorVue.FailureMatrix.ExternalRenderFragments' is not supported and cannot be used for property access."),
            2 => new(
                "builder.AddContent(0, (RenderFragment)ExternalRenderFragments.Render);",
                "External type 'RazorVue.FailureMatrix.ExternalRenderFragments' is not supported and cannot be used for method reference."),
            _ => new(
                "builder.AddContent(0, ExternalRenderFragments.GenericFragment, \"value\");",
                "AddContent<TValue> requires a resolvable RenderFragment<TValue> source.")
        };

    private static BoundaryCaseSpec CreateUnsupportedRenderFragmentFactoryCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.AddContent(0, CreateFragment());",
                "RenderFragment content requires a resolvable inline, local, helper, or component-slot source.")
            {
                Members = "private RenderFragment CreateFragment() { var value = " + Literal(marker) + "; return child => child.AddContent(0, value); }"
            },
            1 => new(
                "builder.AddContent(0, CreateFragment());",
                "RenderFragment content requires a resolvable inline, local, helper, or component-slot source.")
            {
                Members = "private RenderFragment CreateFragment() { RenderFragment result = child => child.AddContent(0, " + Literal(marker) + "); result = child => child.AddContent(1, " + Literal(marker + "-replacement") + "); return result; }"
            },
            2 => new(
                "builder.AddContent(0, CreateFragment());",
                "RenderFragment content requires a resolvable inline, local, helper, or component-slot source.")
            {
                Members = "[Parameter] public bool Visible { get; set; } private RenderFragment CreateFragment() { if (Visible) return child => child.AddContent(0, " + Literal(marker) + "); return child => child.AddContent(1, " + Literal(marker + "-fallback") + "); }"
            },
            _ => new(
                "builder.AddContent(0, CreateFragment(), \"value\");",
                "AddContent<TValue> requires a resolvable RenderFragment<TValue> source.")
            {
                Members = "private RenderFragment<string> CreateFragment() { var prefix = " + Literal(marker) + "; return value => child => child.AddContent(0, prefix + value); }"
            }
        };

    private static BoundaryCaseSpec CreateRecursiveRenderFragmentMethodGroupCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.AddContent(0, (RenderFragment)RenderSelf);",
                "Recursive RenderFragment method group 'RenderSelf'")
            {
                Members = "private void RenderSelf(RenderTreeBuilder target) { target.AddContent(0, (RenderFragment)RenderSelf); }"
            },
            1 => new(
                "builder.AddContent(0, (RenderFragment)RenderStatic);",
                "Recursive RenderFragment method group 'RenderStatic'")
            {
                Members = "private static void RenderStatic(RenderTreeBuilder target) { target.AddContent(0, (RenderFragment)RenderStatic); }"
            },
            2 => new(
                "builder.AddContent(0, (RenderFragment)RenderFirst);",
                "Recursive RenderFragment method group 'RenderFirst'")
            {
                Members = "private void RenderFirst(RenderTreeBuilder target) { target.AddContent(0, (RenderFragment)RenderSecond); } private void RenderSecond(RenderTreeBuilder target) { target.AddContent(0, (RenderFragment)RenderFirst); }"
            },
            _ => new(
                "builder.AddContent(0, (RenderFragment)RenderExpression);",
                "Recursive RenderFragment method group 'RenderExpression'")
            {
                Members = "private void RenderExpression(RenderTreeBuilder target) => target.AddContent(0, (RenderFragment)RenderExpression);"
            }
        };

    private static BoundaryCaseSpec CreateUnclosedRenderFragmentHelperCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.AddContent(0, CreateElementFragment());",
                "RenderFragment helper 'CreateElementFragment' left unclosed")
            {
                Members = "private RenderFragment CreateElementFragment() => child => { child.OpenElement(0, \"div\"); child.AddContent(1, " + Literal(marker) + "); };"
            },
            1 => new(
                "builder.AddContent(0, CreateComponentFragment());",
                "RenderFragment helper 'CreateComponentFragment' left unclosed")
            {
                Members = "private RenderFragment CreateComponentFragment() { return child => { child.OpenComponent<FailureMatrixChild>(0); child.AddContent(1, " + Literal(marker) + "); }; }"
            },
            2 => new(
                "builder.AddContent(0, CreateScopedFragment(), \"value\");",
                "RenderFragment helper 'CreateScopedFragment' left unclosed")
            {
                Members = "private RenderFragment<string> CreateScopedFragment() => value => child => { child.OpenRegion(0); child.AddContent(1, value + " + Literal(marker) + "); };"
            },
            _ => new(
                "builder.AddContent(0, CreateLocalFragment());",
                "RenderFragment helper 'CreateLocalFragment' left unclosed")
            {
                Members = "private RenderFragment CreateLocalFragment() { return child => { child.OpenElement(0, \"section\"); child.AddContent(1, " + Literal(marker) + "); }; }"
            }
        };

    private static BoundaryCaseSpec CreateSetAttributeWithoutPrecedingAttributeCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.OpenElement(0, \"div\"); builder.SetAttributeValue(1, " + Literal(marker) + "); builder.CloseElement();",
                "SetAttributeValue requires a known preceding attribute"),
            1 => new(
                "builder.OpenElement(0, \"section\"); builder.SetAttributeValue(1, " + Literal(marker) + "); builder.CloseElement();",
                "SetAttributeValue requires a known preceding attribute"),
            2 => new(
                "builder.OpenComponent<FailureMatrixChild>(0); builder.SetAttributeValue(1, " + Literal(marker) + "); builder.CloseComponent();",
                "SetAttributeValue requires a known preceding attribute"),
            _ => new(
                "builder.OpenComponent(0, typeof(FailureMatrixChild)); builder.SetAttributeValue(1, " + Literal(marker) + "); builder.CloseComponent();",
                "SetAttributeValue requires a known preceding attribute")
        };

    private static BoundaryCaseSpec CreateDynamicUpdatesAttributeNameCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.OpenElement(0, \"input\"); builder.SetUpdatesAttributeName(AttributeName); builder.CloseElement();",
                "SetUpdatesAttributeName requires a compile-time attribute name")
            {
                Members = "[Parameter] public string AttributeName { get; set; } = \"value\";"
            },
            1 => new(
                "builder.OpenElement(0, \"input\"); builder.SetUpdatesAttributeName(GetAttributeName()); builder.CloseElement();",
                "SetUpdatesAttributeName requires a compile-time attribute name")
            {
                Members = "private string GetAttributeName() => \"checked\";"
            },
            2 => new(
                "builder.OpenElement(0, \"input\"); builder.SetUpdatesAttributeName(Visible ? \"value\" : \"checked\"); builder.CloseElement();",
                "SetUpdatesAttributeName requires a compile-time attribute name")
            {
                Members = "[Parameter] public bool Visible { get; set; }"
            },
            _ => new(
                "builder.OpenElement(0, \"input\"); builder.SetUpdatesAttributeName(_attributeName); builder.CloseElement();",
                "SetUpdatesAttributeName requires a compile-time attribute name")
            {
                Members = "private readonly string _attributeName = \"value\";"
            }
        };

    private static BoundaryCaseSpec CreateDynamicEventModifierNameCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.OpenElement(0, \"button\"); builder.AddEventPreventDefaultAttribute(1, EventName, true); builder.CloseElement();",
                "Event modifier names must be compile-time strings")
            {
                Members = "[Parameter] public string EventName { get; set; } = \"onclick\";"
            },
            1 => new(
                "builder.OpenElement(0, \"button\"); builder.AddEventStopPropagationAttribute(1, GetEventName(), true); builder.CloseElement();",
                "Event modifier names must be compile-time strings")
            {
                Members = "private static string GetEventName() => \"onkeydown\";"
            },
            2 => new(
                "builder.OpenElement(0, \"button\"); builder.AddEventPreventDefaultAttribute(1, Visible ? \"onclick\" : \"onblur\", true); builder.CloseElement();",
                "Event modifier names must be compile-time strings")
            {
                Members = "[Parameter] public bool Visible { get; set; }"
            },
            _ => new(
                "builder.OpenElement(0, \"button\"); builder.AddEventStopPropagationAttribute(1, _eventName, true); builder.CloseElement();",
                "Event modifier names must be compile-time strings")
            {
                Members = "private readonly string _eventName = \"onkeyup\";"
            }
        };

    private static BoundaryCaseSpec CreateSideEffectSequenceCase(int variant, string marker)
    {
        BoundaryCaseSpec spec = variant switch
        {
            0 => new(
                "builder.AddContent(NextSequence(), " + Literal(marker) + ");",
                "RenderTreeBuilder sequence arguments must be side-effect-free"),
            1 => new(
                "builder.OpenElement(NextSequence(), \"div\"); builder.CloseElement();",
                "RenderTreeBuilder sequence arguments must be side-effect-free"),
            2 => new(
                "builder.OpenElement(0, \"div\"); builder.AddAttribute(NextSequence(), \"data-case\", " + Literal(marker) + "); builder.CloseElement();",
                "RenderTreeBuilder sequence arguments must be side-effect-free"),
            _ => new(
                "builder.OpenComponent<FailureMatrixChild>(NextSequence()); builder.CloseComponent();",
                "RenderTreeBuilder sequence arguments must be side-effect-free")
        };

        return spec with { Members = "private int NextSequence() => 0;" };
    }

    private static BoundaryCaseSpec CreateDynamicBulkAttributeNameCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.OpenElement(0, \"div\"); builder.AddMultipleAttributes(1, new Dictionary<string, object> { [AttributeName] = " + Literal(marker) + " }); builder.CloseElement();",
                "Bulk attribute names must be compile-time strings")
            {
                Members = "[Parameter] public string AttributeName { get; set; } = \"data-case\";"
            },
            1 => new(
                "builder.OpenElement(0, \"div\"); builder.AddMultipleAttributes(1, new Dictionary<string, object> { { GetAttributeName(), " + Literal(marker) + " } }); builder.CloseElement();",
                "Bulk attribute names must be compile-time strings")
            {
                Members = "private static string GetAttributeName() => \"data-case\";"
            },
            2 => new(
                "builder.OpenElement(0, \"div\"); builder.AddMultipleAttributes(1, new Dictionary<string, object> { [Visible ? \"data-visible\" : \"data-hidden\"] = " + Literal(marker) + " }); builder.CloseElement();",
                "Bulk attribute names must be compile-time strings")
            {
                Members = "[Parameter] public bool Visible { get; set; }"
            },
            _ => new(
                "builder.OpenElement(0, \"div\"); builder.AddMultipleAttributes(1, new Dictionary<string, object> { { _attributeName, " + Literal(marker) + " } }); builder.CloseElement();",
                "Bulk attribute names must be compile-time strings")
            {
                Members = "private readonly string _attributeName = \"data-case\";"
            }
        };

    private static BoundaryCaseSpec CreateUnclosedRenderFragmentMethodGroupCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.AddContent(0, (RenderFragment)RenderElement);",
                "RenderFragment method group left unclosed")
            {
                Members = "private void RenderElement(RenderTreeBuilder target) { target.OpenElement(0, \"div\"); target.AddContent(1, " + Literal(marker) + "); }"
            },
            1 => new(
                "builder.AddContent(0, (RenderFragment)RenderComponent);",
                "RenderFragment method group left unclosed")
            {
                Members = "private static void RenderComponent(RenderTreeBuilder target) { target.OpenComponent<FailureMatrixChild>(0); target.AddContent(1, " + Literal(marker) + "); }"
            },
            2 => new(
                "builder.AddContent(0, (RenderFragment)RenderRegion);",
                "RenderFragment method group left unclosed")
            {
                Members = "private void RenderRegion(RenderTreeBuilder target) { target.OpenRegion(0); target.AddContent(1, " + Literal(marker) + "); }"
            },
            _ => new(
                "builder.AddContent(0, (RenderFragment)RenderExpression);",
                "RenderFragment method group left unclosed")
            {
                Members = "private void RenderExpression(RenderTreeBuilder target) => target.OpenElement(0, \"section\");"
            }
        };

    private static BoundaryCaseSpec CreateUnsupportedEventCallbackBinderCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"oninput\", EventCallback.Factory.CreateBinder<string>(this, value => _boundValue = value.Trim(), _boundValue)); builder.CloseElement();",
                "EventCallbackFactory.CreateBinder is supported by RazorVue DOM @bind v1")
            {
                Members = "private string _boundValue = " + Literal(marker) + ";"
            },
            1 => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"oninput\", EventCallback.Factory.CreateBinder<string>(this, value => { _firstBoundValue = value; _secondBoundValue = value; }, _firstBoundValue)); builder.CloseElement();",
                "EventCallbackFactory.CreateBinder is supported by RazorVue DOM @bind v1")
            {
                Members = "private string _firstBoundValue = " + Literal(marker) + "; private string _secondBoundValue = \"\";"
            },
            2 => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"oninput\", EventCallback.Factory.CreateBinder<string>(this, value => Text = value, Text)); builder.CloseElement();",
                "EventCallbackFactory.CreateBinder cannot bind to current-component parameter")
            {
                Members = "[Parameter] public string Text { get; set; } = " + Literal(marker) + ";"
            },
            _ => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"oninput\", EventCallback.Factory.CreateBinder<string>(this, SetBoundValue, _boundValue)); builder.CloseElement();",
                "EventCallbackFactory.CreateBinder is supported by RazorVue DOM @bind v1")
            {
                Members = "private string _boundValue = " + Literal(marker) + "; private void SetBoundValue(string value) { _boundValue = value; }"
            }
        };

    private static BoundaryCaseSpec CreateUnsupportedRenderTreeBuilderExtensionCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.UnsupportedBuilderExtension();",
                "RenderTreeBuilder.UnsupportedBuilderExtension is not supported by direct render operation lowering yet."),
            1 => new(
                "builder.UnsupportedBuilderExtensionWithValue(" + Literal(marker) + ");",
                "RenderTreeBuilder.UnsupportedBuilderExtensionWithValue is not supported by direct render operation lowering yet."),
            2 => new(
                "builder.UnsupportedBuilderExtensionGeneric(" + Literal(marker) + ");",
                "RenderTreeBuilder.UnsupportedBuilderExtensionGeneric is not supported by direct render operation lowering yet."),
            _ => new(
                "builder.UnsupportedBuilderExtensionOptional();",
                "RenderTreeBuilder.UnsupportedBuilderExtensionOptional is not supported by direct render operation lowering yet.")
        };

    private static BoundaryCaseSpec CreateUnresolvableComponentImportCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.OpenComponent<FailureNoImportChild>(0); builder.CloseComponent();",
                "must declare [ECMAScriptModule(\"./path\")] or [VueLibraryComponent(\"package\", \"Export\")]"),
            1 => new(
                "builder.OpenComponent<FailureWhitespaceLibrarySpecifierChild>(0); builder.CloseComponent();",
                "must declare [ECMAScriptModule(\"./path\")] or [VueLibraryComponent(\"package\", \"Export\")]"),
            2 => new(
                "builder.OpenComponent<FailureWhitespaceLibraryExportChild>(0); builder.CloseComponent();",
                "must declare [ECMAScriptModule(\"./path\")] or [VueLibraryComponent(\"package\", \"Export\")]"),
            _ => new(
                "builder.OpenComponent<FailureWhitespaceModuleChild>(0); builder.CloseComponent();",
                "must declare [ECMAScriptModule(\"./path\")] or [VueLibraryComponent(\"package\", \"Export\")]")
        };

    private static BoundaryCaseSpec CreateUnsupportedHelperInvocationShapeCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "builder.OpenElement(0, \"div\"); RenderWithoutArguments(); builder.CloseElement();",
                "RazorVue direct render operation lowering does not support invocation")
            {
                Members = "private static void RenderWithoutArguments() { }"
            },
            1 => new(
                "builder.OpenElement(0, \"div\"); _externalHelper.Render(builder, " + Literal(marker) + "); builder.CloseElement();",
                "RazorVue direct render operation lowering does not support invocation")
            {
                Members = "private readonly ExternalRenderTreeBuilderHelper _externalHelper = new();"
            },
            2 => new(
                "builder.OpenElement(0, \"div\"); RenderWithSecondBuilder(" + Literal(marker) + ", builder); builder.CloseElement();",
                "RazorVue direct render operation lowering does not support invocation")
            {
                Members = "private static void RenderWithSecondBuilder(string value, RenderTreeBuilder target) { target.AddContent(0, value); }"
            },
            _ => new(
                "builder.OpenElement(0, \"div\"); RenderExpression(builder, " + Literal(marker) + "); builder.CloseElement();",
                "RazorVue direct render operation lowering does not support invocation")
            {
                Members = "private static void RenderExpression(RenderTreeBuilder target, string value) => System.Console.WriteLine(value);"
            }
        };

    private static BoundaryCaseSpec CreateUnsupportedEventCallbackBinderHandlerShapeCase(int variant, string marker)
        => variant switch
        {
            0 => new(
                "System.Action<string> handler = value => _boundValue = value.Trim(); builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"oninput\", EventCallback.Factory.CreateBinder<string>(this, handler, _boundValue)); builder.CloseElement();",
                "Handler operation kind: LocalReference.")
            {
                Members = "private string _boundValue = " + Literal(marker) + ";"
            },
            1 => new(
                "object handler = (System.Action<string>)(value => _boundValue = value.Trim()); builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"oninput\", EventCallback.Factory.CreateBinder<string>(this, (System.Action<string>)handler, _boundValue)); builder.CloseElement();",
                "Inner handler operation kind: LocalReference.")
            {
                Members = "private string _boundValue = " + Literal(marker) + ";"
            },
            2 => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"oninput\", EventCallback.Factory.CreateBinder<string>(this, CreateHandler(), _boundValue)); builder.CloseElement();",
                "Invocation target: RazorVue.FailureMatrix.DirectRenderFailure")
            {
                Members = "private string _boundValue = " + Literal(marker) + "; private System.Action<string> CreateHandler() => value => _boundValue = value.Trim();"
            },
            _ => new(
                "builder.OpenElement(0, \"input\"); builder.AddAttribute(1, \"oninput\", EventCallback.Factory.CreateBinder<string>(this, default(System.Action<string>), _boundValue)); builder.CloseElement();",
                "Handler operation kind: DefaultValue.")
            {
                Members = "private string _boundValue = " + Literal(marker) + ";"
            }
        };

    private delegate BoundaryCaseSpec BoundaryCaseFactory(int variant, string marker);

    private sealed record BoundaryCaseSpec(string Body, string ExpectedFailureFragment)
    {
        public string Members { get; init; } = "";
    }
}
