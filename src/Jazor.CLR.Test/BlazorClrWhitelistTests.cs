using System.Reflection;
using ECMAScript;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class BlazorClrWhitelistTests
{
    [TestMethod]
    public void ComponentBaseProductHookSurface_IsAllowedAndHasNoRuntimeModule()
    {
        AssertTypeMapping(typeof(ComponentBaseModule), Op.Allowed, "Microsoft.AspNetCore.Components.ComponentBase");
        AssertNoRuntimeModule(typeof(ComponentBaseModule));
        AssertAllowedMembers(
            typeof(ComponentBaseModule),
            "Microsoft.AspNetCore.Components.ComponentBase.StateHasChanged()",
            "Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Action)",
            "Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Func<System.Threading.Tasks.Task>)",
            "virtual Microsoft.AspNetCore.Components.ComponentBase.SetParametersAsync(Microsoft.AspNetCore.Components.ParameterView)");
    }

    [TestMethod]
    public void EventCallbackProductHookSurface_IsAllowedAndHasNoRuntimeModule()
    {
        AssertTypeMapping(typeof(EventCallbackModule), Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback");
        AssertTypeMapping(typeof(EventCallbackT1Module<>), Op.Allowed, "Microsoft.AspNetCore.Components.EventCallback<TValue>");
        AssertTypeMapping(typeof(EventCallbackFactoryModule), Op.Allowed, "Microsoft.AspNetCore.Components.EventCallbackFactory");
        AssertNoRuntimeModule(typeof(EventCallbackModule));
        AssertNoRuntimeModule(typeof(EventCallbackT1Module<>));
        AssertNoRuntimeModule(typeof(EventCallbackFactoryModule));

        AssertAllowedMembers(
            typeof(EventCallbackModule),
            "static readonly Microsoft.AspNetCore.Components.EventCallback.Factory",
            "Microsoft.AspNetCore.Components.EventCallback.InvokeAsync()",
            "Microsoft.AspNetCore.Components.EventCallback.InvokeAsync(object)");
        AssertAllowedMembers(
            typeof(EventCallbackT1Module<>),
            "Microsoft.AspNetCore.Components.EventCallback<TValue>.InvokeAsync()",
            "Microsoft.AspNetCore.Components.EventCallback<TValue>.InvokeAsync(TValue)");
        AssertAllowedMembers(
            typeof(EventCallbackFactoryModule),
            "Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Action)",
            "Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Action<TValue>)",
            "Microsoft.AspNetCore.Components.EventCallbackFactory.Create(object, System.Func<System.Threading.Tasks.Task>)",
            "Microsoft.AspNetCore.Components.EventCallbackFactory.Create<TValue>(object, System.Func<TValue, System.Threading.Tasks.Task>)");
    }

    [TestMethod]
    public void RenderTreeBuilderProductHookSurface_TracksTheReferencePublicSurface()
    {
        AssertTypeMapping(typeof(RenderTreeBuilderModule), Op.Allowed, "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder");
        AssertNoRuntimeModule(typeof(RenderTreeBuilderModule));

        var mappings = GetMappings(typeof(RenderTreeBuilderModule));
        var actual = mappings.Keys.Order(StringComparer.Ordinal).ToArray();
        var expected = typeof(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(GetRenderTreeBuilderMemberKey)
            .Concat(
                typeof(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(static _ => "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.RenderTreeBuilder()"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected, actual, string.Join(Environment.NewLine, actual));
        Assert.IsTrue(mappings.Values.All(static mapping => mapping.Op == Op.Allowed));
    }

    [TestMethod]
    public void WebRenderTreeBuilderExtensionsProductHookSurface_TracksTheReferencePublicSurface()
    {
        AssertTypeMapping(
            typeof(WebRenderTreeBuilderExtensionsModule),
            Op.Allowed,
            "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions");
        AssertNoRuntimeModule(typeof(WebRenderTreeBuilderExtensionsModule));

        var mappings = GetMappings(typeof(WebRenderTreeBuilderExtensionsModule));
        var actual = mappings.Keys.Order(StringComparer.Ordinal).ToArray();
        var expected = typeof(Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(static method =>
                "static Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions." + GetMethodSurfaceSignature(method))
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected, actual, string.Join(Environment.NewLine, actual));
        Assert.IsTrue(mappings.Values.All(static mapping => mapping.Op == Op.Allowed));
    }

    [TestMethod]
    public void ExtendedDomEventModules_ExposeNativeReadSurfaceAndRejectMutation()
    {
        AssertTypeMapping(
            typeof(PointerEventArgsModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs");
        AssertTypeMapping(
            typeof(WheelEventArgsModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs");
        AssertModulePath(
            typeof(PointerEventArgsModule),
            "Microsoft/AspNetCore/Components/Web/PointerEventArgsModule.js");
        AssertModulePath(
            typeof(WheelEventArgsModule),
            "Microsoft/AspNetCore/Components/Web/WheelEventArgsModule.js");

        AssertMember(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerId.get",
            Op.Inline,
            "__arg1.pointerId");
        AssertMember(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Width.get",
            Op.Inline,
            "__arg1.width");
        AssertMember(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Height.get",
            Op.Inline,
            "__arg1.height");
        AssertMember(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Pressure.get",
            Op.Inline,
            "__arg1.pressure");
        AssertMember(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltX.get",
            Op.Inline,
            "__arg1.tiltX");
        AssertMember(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltY.get",
            Op.Inline,
            "__arg1.tiltY");
        AssertMember(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerType.get",
            Op.Inline,
            "__arg1.pointerType");
        AssertMember(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.IsPrimary.get",
            Op.Inline,
            "__arg1.isPrimary");
        AssertMember(
            typeof(WheelEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaX.get",
            Op.Inline,
            "__arg1.deltaX");
        AssertMember(
            typeof(WheelEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaY.get",
            Op.Inline,
            "__arg1.deltaY");
        AssertMember(
            typeof(WheelEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaZ.get",
            Op.Inline,
            "__arg1.deltaZ");
        AssertMember(
            typeof(WheelEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaMode.get",
            Op.Inline,
            "__arg1.deltaMode");

        foreach (var member in new[]
        {
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerId.set",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Width.set",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Height.set",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Pressure.set",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltX.set",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltY.set",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerType.set",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.IsPrimary.set",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerEventArgs()"
        })
        {
            AssertMember(typeof(PointerEventArgsModule), member, Op.Discard);
        }

        foreach (var member in new[]
        {
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaX.set",
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaY.set",
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaZ.set",
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaMode.set",
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.WheelEventArgs()"
        })
        {
            AssertMember(typeof(WheelEventArgsModule), member, Op.Discard);
        }
    }

    [TestMethod]
    public void DragAndClipboardEventModules_ExposeNativeReadSurfaceAndRejectMutation()
    {
        AssertTypeMapping(
            typeof(DragEventArgsModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.DragEventArgs");
        AssertTypeMapping(
            typeof(DataTransferModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.DataTransfer");
        AssertTypeMapping(
            typeof(ClipboardEventArgsModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs");
        AssertModulePath(
            typeof(DragEventArgsModule),
            "Microsoft/AspNetCore/Components/Web/DragEventArgsModule.js");
        AssertModulePath(
            typeof(DataTransferModule),
            "Microsoft/AspNetCore/Components/Web/DataTransferModule.js");
        AssertModulePath(
            typeof(ClipboardEventArgsModule),
            "Microsoft/AspNetCore/Components/Web/ClipboardEventArgsModule.js");

        AssertMember(
            typeof(DragEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.DragEventArgs.DataTransfer.get",
            Op.Inline,
            "__arg1.dataTransfer");
        AssertMember(
            typeof(DataTransferModule),
            "Microsoft.AspNetCore.Components.Web.DataTransfer.DropEffect.get",
            Op.Inline,
            "__arg1.dropEffect");
        AssertMember(
            typeof(DataTransferModule),
            "Microsoft.AspNetCore.Components.Web.DataTransfer.EffectAllowed.get",
            Op.Inline,
            "__arg1.effectAllowed");
        AssertMember(
            typeof(DataTransferModule),
            "Microsoft.AspNetCore.Components.Web.DataTransfer.Types.get",
            Op.Inline,
            "__arg1.types");
        AssertMember(
            typeof(ClipboardEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.Type.get",
            Op.Inline,
            "__arg1.type");

        foreach (var (module, members) in new[]
        {
            (
                typeof(DragEventArgsModule),
                new[]
                {
                    "Microsoft.AspNetCore.Components.Web.DragEventArgs.DataTransfer.set",
                    "Microsoft.AspNetCore.Components.Web.DragEventArgs.DragEventArgs()"
                }),
            (
                typeof(DataTransferModule),
                new[]
                {
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.DropEffect.set",
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.EffectAllowed.set",
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.Types.set",
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.Files.get",
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.Files.set",
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.Items.get",
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.Items.set",
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.DataTransfer()"
                }),
            (
                typeof(ClipboardEventArgsModule),
                new[]
                {
                    "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.Type.set",
                    "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.ClipboardEventArgs()"
                })
        })
        {
            foreach (var member in members)
                AssertMember(module, member, Op.Discard);
        }
    }

    [TestMethod]
    public void TouchErrorAndProgressEventModules_ExposeNativeReadSurfaceAndRejectMutation()
    {
        AssertTypeMapping(
            typeof(TouchEventArgsModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.TouchEventArgs");
        AssertTypeMapping(
            typeof(TouchPointModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.TouchPoint");
        AssertTypeMapping(
            typeof(ErrorEventArgsModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.ErrorEventArgs");
        AssertTypeMapping(
            typeof(ProgressEventArgsModule),
            Op.Alias,
            "Microsoft.AspNetCore.Components.Web.ProgressEventArgs");

        AssertModulePath(
            typeof(TouchEventArgsModule),
            "Microsoft/AspNetCore/Components/Web/TouchEventArgsModule.js");
        AssertModulePath(
            typeof(TouchPointModule),
            "Microsoft/AspNetCore/Components/Web/TouchPointModule.js");
        AssertModulePath(
            typeof(ErrorEventArgsModule),
            "Microsoft/AspNetCore/Components/Web/ErrorEventArgsModule.js");
        AssertModulePath(
            typeof(ProgressEventArgsModule),
            "Microsoft/AspNetCore/Components/Web/ProgressEventArgsModule.js");

        foreach (var (module, member, value) in new[]
        {
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Detail.get", "__arg1.detail"),
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Touches.get", "Array.from(__arg1.touches)"),
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.TargetTouches.get", "Array.from(__arg1.targetTouches)"),
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.ChangedTouches.get", "Array.from(__arg1.changedTouches)"),
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.CtrlKey.get", "__arg1.ctrlKey"),
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.ShiftKey.get", "__arg1.shiftKey"),
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.AltKey.get", "__arg1.altKey"),
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.MetaKey.get", "__arg1.metaKey"),
            (typeof(TouchEventArgsModule), "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Type.get", "__arg1.type"),
            (typeof(TouchPointModule), "Microsoft.AspNetCore.Components.Web.TouchPoint.Identifier.get", "__arg1.identifier"),
            (typeof(TouchPointModule), "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenX.get", "__arg1.screenX"),
            (typeof(TouchPointModule), "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenY.get", "__arg1.screenY"),
            (typeof(TouchPointModule), "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientX.get", "__arg1.clientX"),
            (typeof(TouchPointModule), "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientY.get", "__arg1.clientY"),
            (typeof(TouchPointModule), "Microsoft.AspNetCore.Components.Web.TouchPoint.PageX.get", "__arg1.pageX"),
            (typeof(TouchPointModule), "Microsoft.AspNetCore.Components.Web.TouchPoint.PageY.get", "__arg1.pageY"),
            (typeof(ErrorEventArgsModule), "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Message.get", "__arg1.message"),
            (typeof(ErrorEventArgsModule), "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Filename.get", "__arg1.filename"),
            (typeof(ErrorEventArgsModule), "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Lineno.get", "__arg1.lineno"),
            (typeof(ErrorEventArgsModule), "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Colno.get", "__arg1.colno"),
            (typeof(ErrorEventArgsModule), "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Type.get", "__arg1.type"),
            (typeof(ProgressEventArgsModule), "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.LengthComputable.get", "__arg1.lengthComputable"),
            (typeof(ProgressEventArgsModule), "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Loaded.get", "__arg1.loaded"),
            (typeof(ProgressEventArgsModule), "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Total.get", "__arg1.total"),
            (typeof(ProgressEventArgsModule), "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Type.get", "__arg1.type")
        })
        {
            AssertMember(module, member, Op.Inline, value);
        }

        foreach (var (module, members) in new[]
        {
            (
                typeof(TouchEventArgsModule),
                new[]
                {
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Detail.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Touches.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.TargetTouches.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.ChangedTouches.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.CtrlKey.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.ShiftKey.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.AltKey.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.MetaKey.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Type.set",
                    "Microsoft.AspNetCore.Components.Web.TouchEventArgs.TouchEventArgs()"
                }),
            (
                typeof(TouchPointModule),
                new[]
                {
                    "Microsoft.AspNetCore.Components.Web.TouchPoint.Identifier.set",
                    "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenX.set",
                    "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenY.set",
                    "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientX.set",
                    "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientY.set",
                    "Microsoft.AspNetCore.Components.Web.TouchPoint.PageX.set",
                    "Microsoft.AspNetCore.Components.Web.TouchPoint.PageY.set",
                    "Microsoft.AspNetCore.Components.Web.TouchPoint.TouchPoint()"
                }),
            (
                typeof(ErrorEventArgsModule),
                new[]
                {
                    "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Message.set",
                    "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Filename.set",
                    "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Lineno.set",
                    "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Colno.set",
                    "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Type.set",
                    "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.ErrorEventArgs()"
                }),
            (
                typeof(ProgressEventArgsModule),
                new[]
                {
                    "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.LengthComputable.set",
                    "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Loaded.set",
                    "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Total.set",
                    "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Type.set",
                    "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.ProgressEventArgs()"
                })
        })
        {
            foreach (var member in members)
                AssertMember(module, member, Op.Discard);
        }
    }

    private static IReadOnlyDictionary<string, JazorAttribute> GetMappings(Type module)
        => module
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(static mapping => mapping.Member, StringComparer.Ordinal);

    private static void AssertTypeMapping(Type module, Op op, string member)
    {
        var mapping = module.GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(mapping, module.Name);
        Assert.AreEqual(op, mapping.Op, module.Name);
        Assert.AreEqual(member, mapping.Member, module.Name);
    }

    private static void AssertAllowedMembers(Type module, params string[] expected)
    {
        var actual = GetMappings(module)
            .Where(static entry => entry.Value.Op == Op.Allowed)
            .Select(static entry => entry.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected.Order(StringComparer.Ordinal).ToArray(), actual, module.Name);
    }

    private static void AssertMember(Type module, string member, Op op, string? value = null)
    {
        var mappings = GetMappings(module);
        Assert.IsTrue(mappings.TryGetValue(member, out var mapping), $"Missing mapping: {member}");
        Assert.AreEqual(op, mapping.Op, member);
        if (value is not null)
            Assert.AreEqual(value, mapping.Value, member);
    }

    private static void AssertModulePath(Type module, string expected)
    {
#pragma warning disable CA1416
        var attribute = module.GetCustomAttribute<ECMAScriptModuleAttribute>();
        Assert.IsNotNull(attribute, module.Name);
        Assert.AreEqual(expected, attribute.Export, module.Name);
#pragma warning restore CA1416
    }

    private static void AssertNoRuntimeModule(Type module)
        => Assert.IsFalse(
            module.CustomAttributes.Any(static attribute =>
                string.Equals(attribute.AttributeType.FullName, "ECMAScript.ECMAScriptModuleAttribute", StringComparison.Ordinal)),
            module.Name);

    private static string GetRenderTreeBuilderMemberKey(MethodInfo method)
        => "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder." + GetMethodSurfaceSignature(method);

    private static string GetMethodSurfaceSignature(MethodInfo method)
    {
        var genericArguments = method.IsGenericMethodDefinition
            ? "<" + string.Join(", ", method.GetGenericArguments().Select(static argument => argument.Name)) + ">"
            : string.Empty;
        var parameters = string.Join(
            ", ",
            method.GetParameters().Select(static parameter => GetTypeSurfaceName(parameter.ParameterType)));
        return method.Name + genericArguments + "(" + parameters + ")";
    }

    private static string GetTypeSurfaceName(Type type)
    {
        if (type.IsGenericParameter)
            return type.Name;

        if (!type.IsGenericType)
            return GetNonGenericTypeSurfaceName(type);

        var definition = type.GetGenericTypeDefinition();
        var definitionName = definition.FullName ?? definition.Name;
        var tickIndex = definitionName.IndexOf('`');
        if (tickIndex >= 0)
            definitionName = definitionName[..tickIndex];

        var arguments = string.Join(
            ", ",
            type.GetGenericArguments().Select(static argument => GetTypeSurfaceName(argument)));
        return definitionName.Replace('+', '.') + "<" + arguments + ">";
    }

    private static string GetNonGenericTypeSurfaceName(Type type)
        => type == typeof(int)
            ? "int"
            : type == typeof(string)
                ? "string"
                : type == typeof(bool)
                    ? "bool"
                    : type == typeof(object)
                        ? "object"
                        : (type.FullName ?? type.Name).Replace('+', '.');
}
