using System.Reflection;

using ECMAScript;
using ECMAScript.Contract;

#pragma warning disable CA1416

namespace ECMAScriptFloatingUiTest;

/// <summary>
/// Reflection checks for the Floating UI host surface: import host, strongly typed shapes, and
/// the repository rule that public parameters and returns never degrade to object.
/// 宿主表面反射检查：import 宿主、强类型形状与禁用 object 万能参数的仓库规则。
/// </summary>
[TestClass]
public sealed class FloatingUiProxyTests
{
    [TestMethod]
    public void FloatingUi_ImportHost_UsesTheVueAuthorEntry()
    {
        var runtime = typeof(FloatingUi).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(runtime);
        Assert.AreEqual("@floating-ui/vue", runtime!.Import);
    }

    [TestMethod]
    public void FloatingUi_RuntimeShapes_DoNotExposeObject()
    {
        var runtimeTypes = new[]
        {
            typeof(FloatingUi), typeof(FloatingMiddleware), typeof(FloatingLimiter), typeof(FloatingMaybeElement),
            typeof(FloatingCoords), typeof(FloatingDimensions), typeof(FloatingRect), typeof(FloatingSideObject),
            typeof(FloatingElementRects), typeof(FloatingElements), typeof(FloatingMiddlewareData),
            typeof(FloatingArrowData), typeof(FloatingPlacementData), typeof(FloatingHideData), typeof(FloatingStyles),
            typeof(FloatingComputePositionReturn), typeof(FloatingComputePositionConfig), typeof(FloatingOffsetValue),
            typeof(FloatingDetectOverflowOptions), typeof(FloatingBoundary), typeof(FloatingShiftOptions),
            typeof(FloatingFlipOptions), typeof(FloatingSizeOptions), typeof(FloatingAutoPlacementOptions),
            typeof(FloatingHideOptions), typeof(FloatingArrowOptions), typeof(FloatingInlineOptions),
            typeof(FloatingAutoUpdateOptions), typeof(FloatingUseOptions), typeof(FloatingUseReturn)
        };

        foreach (var type in runtimeTypes)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                // An erased-value union's IUnion.Value is intentionally object-typed; the typed
                // branch surface (AsX or branch projection) carries the contract.
                // union 的 IUnion.Value 按契约就是 object；强类型分支由 AsX/分支投影承担。
                if (property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value))
                    continue;
                AssertNotObject(property.PropertyType, $"{type.Name}.{property.Name}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(static method => !method.IsSpecialName)
                         .Where(static method => method.Name is not ("Equals" or "GetHashCode" or "ToString" or "PrintMembers" or "<Clone>$")))
            {
                AssertNotObject(method.ReturnType, $"{type.Name}.{method.Name} return");
                foreach (var parameter in method.GetParameters())
                    AssertNotObject(parameter.ParameterType, $"{type.Name}.{method.Name}({parameter.Name})");
            }
        }
    }

    [TestMethod]
    public void FloatingUi_StaticApi_ExposesTheFirstSliceSurface()
    {
        var methods = typeof(FloatingUi)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .ToArray();

        RequiredStatic(methods, nameof(FloatingUi.UseFloating), static method =>
            method.ReturnType == typeof(FloatingUseReturn) && method.IsGenericMethodDefinition &&
            method.GetParameters().Length == 3);

        RequiredStatic(methods, nameof(FloatingUi.Arrow), static method =>
            method.ReturnType == typeof(FloatingMiddleware) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(FloatingArrowOptions) }));
        RequiredStatic(methods, nameof(FloatingUi.Offset), static method =>
            method.ReturnType == typeof(FloatingMiddleware) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(Number) }));
        RequiredStatic(methods, nameof(FloatingUi.Offset), static method =>
            method.ReturnType == typeof(FloatingMiddleware) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(new[] { typeof(FloatingOffsetValue) }));
        foreach (var name in new[] { nameof(FloatingUi.Shift), nameof(FloatingUi.Flip), nameof(FloatingUi.Size), nameof(FloatingUi.AutoPlacement), nameof(FloatingUi.Hide), nameof(FloatingUi.Inline) })
            RequiredStatic(methods, name, static method => method.ReturnType == typeof(FloatingMiddleware) && method.GetParameters().Length == 1);

        RequiredStatic(methods, nameof(FloatingUi.LimitShift), static method =>
            method.ReturnType == typeof(FloatingLimiter) && method.GetParameters().Length == 0);
        RequiredStatic(methods, nameof(FloatingUi.AutoUpdate), static method =>
            method.ReturnType == typeof(Action) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Element), typeof(Element), typeof(Action), typeof(FloatingAutoUpdateOptions) }));
        RequiredStatic(methods, nameof(FloatingUi.ComputePosition), static method =>
            method.ReturnType == typeof(IPromise<FloatingComputePositionReturn>) &&
            method.GetParameters().Select(static parameter => parameter.ParameterType)
                .SequenceEqual(new[] { typeof(Element), typeof(Element), typeof(FloatingComputePositionConfig) }));
    }

    [TestMethod]
    public void FloatingUi_StringEnums_AndUnions_MirrorUpstreamValues()
    {
        AssertStringEnumMember<FloatingPlacement>("bottom-start", nameof(FloatingPlacement.BottomStart));
        AssertStringEnumMember<FloatingPlacement>("left-end", nameof(FloatingPlacement.LeftEnd));
        AssertStringEnumMember<FloatingSide>("bottom", nameof(FloatingSide.Bottom));
        AssertStringEnumMember<FloatingAlignment>("start", nameof(FloatingAlignment.Start));
        AssertStringEnumMember<FloatingStrategy>("absolute", nameof(FloatingStrategy.Absolute));
        AssertStringEnumMember<FloatingRootBoundary>("viewport", nameof(FloatingRootBoundary.Viewport));
        AssertStringEnumMember<FloatingFallbackStrategy>("bestFit", nameof(FloatingFallbackStrategy.BestFit));

        AssertNet11UnionContract(typeof(FloatingMaybeElement), typeof(Element), typeof(Vue.VueComponentPublicInstance));
        AssertNet11UnionContract(typeof(FloatingBoundary), typeof(string), typeof(Element), typeof(Element[]));
    }

    [TestMethod]
    public void FloatingUseReturn_ExposesReactiveRefsAndUpdateEntry()
    {
        var update = typeof(FloatingUseReturn).GetMethod(nameof(FloatingUseReturn.Update));
        Assert.IsNotNull(update);
        Assert.AreEqual(typeof(void), update!.ReturnType);

        Assert.AreEqual(typeof(Vue.VueReadonlyRef<Number>), typeof(FloatingUseReturn).GetProperty(nameof(FloatingUseReturn.X))!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<FloatingStyles>), typeof(FloatingUseReturn).GetProperty(nameof(FloatingUseReturn.FloatingStyles))!.PropertyType);
        Assert.AreEqual(typeof(Vue.VueReadonlyRef<bool>), typeof(FloatingUseReturn).GetProperty(nameof(FloatingUseReturn.IsPositioned))!.PropertyType);
    }

    private static void AssertStringEnumMember<TEnum>(string javaScriptValue, string memberName)
        where TEnum : struct, Enum
    {
        Assert.IsNotNull(typeof(TEnum).GetCustomAttribute<ECMAScript.StringAttribute>(), typeof(TEnum).FullName);
        var description = typeof(TEnum).GetField(memberName)!
            .GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!.Description;
        Assert.AreEqual("@#" + javaScriptValue, description, $"{typeof(TEnum).Name}.{memberName}");
    }

    private static void AssertNet11UnionContract(Type unionType, params Type[] branchTypes)
    {
        Assert.IsNotNull(unionType.GetCustomAttribute<System.Runtime.CompilerServices.UnionAttribute>(), unionType.FullName);
        Assert.IsTrue(typeof(System.Runtime.CompilerServices.IUnion).IsAssignableFrom(unionType), unionType.FullName);
    }

    private static void RequiredStatic(MethodInfo[] methods, string name, Func<MethodInfo, bool> predicate)
    {
        var matches = methods.Where(method => method.Name == name && predicate(method)).ToArray();
        Assert.IsTrue(matches.Length == 1, $"Expected exactly one strongly typed '{name}' overload, found {matches.Length}.");
    }

    private static void AssertNotObject(Type type, string location)
        => Assert.IsFalse(type == typeof(object), $"{location} must stay strongly typed; object is not permitted on this surface.");
}
