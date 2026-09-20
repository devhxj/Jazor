using System.Reflection;

using ECMAScript;
using ECMAScript.Contract;

#pragma warning disable CA1416

namespace ECMAScriptVueDraggableTest;

/// <summary>
/// Reflection checks for the vue-draggable-plus host surface: import host, component proxy,
/// strongly typed shapes, and the no-<c>object</c>-downgrade rule.
/// 宿主表面反射检查：import 宿主、组件代理、强类型形状与 object 降级规则。
/// </summary>
[TestClass]
public sealed class VueDraggableProxyTests
{
    [TestMethod]
    public void VueDraggable_Entry_UsesTheAuthorImport()
    {
        var runtime = typeof(VueDraggable).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(runtime);
        Assert.AreEqual("vue-draggable-plus", runtime!.Import);
    }

    [TestMethod]
    public void VueDraggable_ComponentProxy_BindsTheUpstreamComponent()
    {
        var componentType = typeof(VueDraggableList<string>);
        Assert.IsTrue(typeof(Microsoft.AspNetCore.Components.ComponentBase).IsAssignableFrom(componentType));

        var component = componentType.GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(component);
        Assert.AreEqual("vue-draggable-plus", component!.Import);
        Assert.AreEqual("VueDraggable", componentType.GetCustomAttribute<ECMAScriptNameAttribute>()?.Name);

        var modelValue = componentType.GetProperty(nameof(VueDraggableList<string>.ModelValue))!;
        Assert.IsNotNull(modelValue.GetCustomAttribute<Microsoft.AspNetCore.Components.ParameterAttribute>());
        Assert.IsNotNull(modelValue.GetCustomAttribute<Microsoft.AspNetCore.Components.EditorRequiredAttribute>());

        var additionalAttributes = componentType.GetProperty(nameof(VueDraggableList<string>.AdditionalAttributes))!;
        Assert.IsTrue(additionalAttributes.GetCustomAttribute<Microsoft.AspNetCore.Components.ParameterAttribute>()!.CaptureUnmatchedValues);

        // 插槽名按运行时属性名书写（不带 @# 前缀，前缀只用于 [Description] 导出映射）。
        Assert.AreEqual(
            "default",
            componentType.GetProperty(nameof(VueDraggableList<string>.ChildContent))!
                .GetCustomAttribute<ECMAScriptNameAttribute>()!.Name);
    }

    [TestMethod]
    public void VueDraggable_Composable_ReturnsStronglyTypedControl()
    {
        var method = typeof(VueDraggable).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(static candidate => candidate.Name == nameof(VueDraggable.UseDraggable));
        Assert.IsTrue(method.IsGenericMethodDefinition);
        Assert.AreEqual(typeof(VueDraggableReturn), method.ReturnType);

        var parameters = method.GetParameters();
        Assert.AreEqual(3, parameters.Length);
        Assert.AreEqual(typeof(Element), parameters[0].ParameterType);
        Assert.AreEqual(typeof(VueDraggableOptions), parameters[2].ParameterType);

        var control = typeof(VueDraggableReturn);
        foreach (var name in new[] { "Start", "Pause", "Resume", "Destroy" })
            Assert.IsNotNull(control.GetMethod(name), $"useDraggable must expose {name}.");
    }

    [TestMethod]
    public void VueDraggable_RuntimeShapes_DoNotDowngradeToObject()
    {
        foreach (var type in new[]
                 {
                     typeof(VueDraggable), typeof(VueDraggableList<string>), typeof(VueDraggableOptions),
                     typeof(VueDraggableGroupOptions), typeof(VueDraggableOffset), typeof(VueDraggableReturn),
                     typeof(VueDraggableEvent), typeof(VueDraggablePullValue)
                 })
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                // IUnion.Value 按契约就是 object（强类型分支承担契约）；
                // CaptureUnmatchedValues 透传字典的值承载任意 HTML 属性。二者是正当例外，
                // 因为此处值域本身开放，C# 无法给出更精确类型。
                if (property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value) ||
                    property.Name == nameof(VueDraggableList<string>.AdditionalAttributes))
                    continue;
                AssertNoObjectDowngrade(property.PropertyType, $"{type.Name}.{property.Name}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(static m => !m.IsSpecialName)
                         .Where(static m => m.Name is not ("Equals" or "GetHashCode" or "ToString" or "PrintMembers" or "<Clone>$")))
            {
                AssertNoObjectDowngrade(method.ReturnType, $"{type.Name}.{method.Name} return");
                foreach (var parameter in method.GetParameters())
                    AssertNoObjectDowngrade(parameter.ParameterType, $"{type.Name}.{method.Name}({parameter.Name})");
            }
        }
    }

    [TestMethod]
    public void VueDraggable_Options_MirrorUpstreamFieldNames()
    {
        foreach (var (name, expected) in new[]
                 {
                     (nameof(VueDraggableOptions.Animation), "@#animation"),
                     (nameof(VueDraggableOptions.ChosenClass), "@#chosenClass"),
                     (nameof(VueDraggableOptions.GhostClass), "@#ghostClass"),
                     (nameof(VueDraggableOptions.Handle), "@#handle"),
                     (nameof(VueDraggableOptions.Filter), "@#filter"),
                     (nameof(VueDraggableOptions.Direction), "@#direction"),
                     (nameof(VueDraggableOptions.Sort), "@#sort"),
                     (nameof(VueDraggableOptions.OnUpdate), "@#onUpdate"),
                     (nameof(VueDraggableOptions.CustomUpdate), "@#customUpdate")
                 })
        {
            var property = typeof(VueDraggableOptions).GetProperty(name)!;
            Assert.AreEqual(expected, property.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!.Description, name);
        }

        Assert.AreEqual("@#horizontal", typeof(VueDraggableDirection).GetField(nameof(VueDraggableDirection.Horizontal))!
            .GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!.Description);
    }

    private static void AssertNoObjectDowngrade(Type type, string location)
    {
        Assert.AreNotEqual(typeof(object), Nullable.GetUnderlyingType(type) ?? type, $"{location} must not downgrade to object.");
        if (!type.IsGenericType)
            return;
        foreach (var argument in type.GetGenericArguments())
            AssertNoObjectDowngrade(argument, location);
    }
}
