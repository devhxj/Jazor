using System.Reflection;

using ECMAScript;
using ECMAScript.Contract;

#pragma warning disable CA1416

namespace ECMAScriptWangEditorTest;

/// <summary>
/// Reflection checks for the WangEditor host surface: adapter entry, editor/toolbar component
/// proxies, typed config, and the no-<c>object</c>-downgrade rule.
/// 宿主表面反射检查：适配器入口、编辑器/工具栏组件代理、类型化配置与 object 降级规则。
/// </summary>
[TestClass]
public sealed class WangEditorProxyTests
{
    [TestMethod]
    public void WangEditor_Entry_UsesTheVueAdapterImport()
    {
        var runtime = typeof(WangEditor).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(runtime);
        Assert.AreEqual("@wangeditor/editor-for-vue", runtime!.Import);
    }

    [TestMethod]
    public void WangEditor_ComponentProxies_BindEditorAndToolbar()
    {
        var editor = typeof(WangEditorComponent).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(editor);
        Assert.AreEqual("@wangeditor/editor-for-vue", editor!.Import);
        Assert.AreEqual("Editor", typeof(WangEditorComponent).GetCustomAttribute<ECMAScriptNameAttribute>()?.Name);

        var toolbar = typeof(WangEditorToolbar).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(toolbar);
        Assert.AreEqual("Toolbar", typeof(WangEditorToolbar).GetCustomAttribute<ECMAScriptNameAttribute>()?.Name);

        Assert.IsTrue(typeof(Microsoft.AspNetCore.Components.ComponentBase).IsAssignableFrom(typeof(WangEditorComponent)));
        Assert.IsTrue(typeof(Microsoft.AspNetCore.Components.ComponentBase).IsAssignableFrom(typeof(WangEditorToolbar)));
    }

    [TestMethod]
    public void WangEditor_EditorProxy_ExposesTwoWayHtmlAndTypedConfig()
    {
        var editor = typeof(WangEditorComponent);
        Assert.AreEqual(typeof(string), editor.GetProperty(nameof(WangEditorComponent.ModelValue))!.PropertyType);
        Assert.AreEqual(
            typeof(Microsoft.AspNetCore.Components.EventCallback<string>),
            editor.GetProperty(nameof(WangEditorComponent.ModelValueChanged))!.PropertyType);
        Assert.AreEqual(typeof(WangEditorMode?), editor.GetProperty(nameof(WangEditorComponent.Mode))!.PropertyType);
        // WangEditorConfig 是引用类型，可空性用 NullabilityInfoContext 断言而非 typeof(T?)（后者对引用类型非法）。
        Assert.AreEqual(typeof(WangEditorConfig), editor.GetProperty(nameof(WangEditorComponent.DefaultConfig))!.PropertyType);
        Assert.AreEqual(
            NullabilityState.Nullable,
            new NullabilityInfoContext().Create(editor.GetProperty(nameof(WangEditorComponent.DefaultConfig))!).ReadState);
        Assert.IsTrue(editor.GetProperty(nameof(WangEditorComponent.AdditionalAttributes))!
            .GetCustomAttribute<Microsoft.AspNetCore.Components.ParameterAttribute>()!.CaptureUnmatchedValues);

        // 工具栏代理需关联编辑器实例。
        Assert.AreEqual(
            typeof(WangEditorInstance),
            typeof(WangEditorToolbar).GetProperty(nameof(WangEditorToolbar.Editor))!.PropertyType);
    }

    [TestMethod]
    public void WangEditor_ModeEnum_MirrorsUpstreamValues()
    {
        Assert.IsNotNull(typeof(WangEditorMode).GetCustomAttribute<ECMAScript.StringAttribute>());
        foreach (var (field, expected) in new[]
                 {
                     (nameof(WangEditorMode.Default), "@#default"),
                     (nameof(WangEditorMode.Simple), "@#simple")
                 })
        {
            Assert.AreEqual(expected, typeof(WangEditorMode).GetField(field)!
                .GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!.Description, field);
        }
    }

    [TestMethod]
    public void WangEditor_ConfigShapes_DoNotDowngradeToObject()
    {
        foreach (var type in new[]
                 {
                     typeof(WangEditor), typeof(WangEditorComponent), typeof(WangEditorToolbar),
                     typeof(WangEditorConfig), typeof(WangEditorToolbarConfig),
                     typeof(WangEditorInstance), typeof(WangEditorToolbarInstance)
                 })
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                // CaptureUnmatchedValues 透传字典的值承载任意 HTML 属性，值域本身开放，属正当例外。
                // IUnion.Value 与 CaptureUnmatchedValues 透传字典的值按契约就是 object；
                // 二者值域本身开放，C# 无法给出更精确类型。
                if (property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value) ||
                    property.Name == nameof(WangEditorComponent.AdditionalAttributes) ||
                    property.Name == nameof(WangEditorToolbar.AdditionalAttributes))
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

    private static void AssertNoObjectDowngrade(Type type, string location)
    {
        Assert.AreNotEqual(typeof(object), Nullable.GetUnderlyingType(type) ?? type, $"{location} must not downgrade to object.");
        if (!type.IsGenericType)
            return;
        foreach (var argument in type.GetGenericArguments())
            AssertNoObjectDowngrade(argument, location);
    }
}
