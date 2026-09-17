using System.Reflection;

using ECMAScript;
using ECMAScript.Contract;

#pragma warning disable CA1416

namespace ECMAScriptFilePondTest;

/// <summary>
/// Reflection checks for the FilePond host surface: core entry, Vue component proxy, instance
/// API, and the no-<c>object</c>-downgrade rule.
/// 宿主表面反射检查：核心入口、Vue 组件代理、实例 API 与 object 降级规则。
/// </summary>
[TestClass]
public sealed class FilePondProxyTests
{
    [TestMethod]
    public void FilePond_CoreEntry_UsesTheCoreImport()
    {
        var runtime = typeof(FilePond).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(runtime);
        Assert.AreEqual("filepond", runtime!.Import);
        Assert.AreEqual(Transform.Import, runtime.Transform);
    }

    [TestMethod]
    public void FilePond_ComponentProxy_BindsTheDefaultExportFactory()
    {
        var componentType = typeof(VueFilePond);
        Assert.IsTrue(typeof(Microsoft.AspNetCore.Components.ComponentBase).IsAssignableFrom(componentType));

        var component = componentType.GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(component);
        Assert.AreEqual("vue-filepond", component!.Import);
        Assert.AreEqual(Transform.Component, component.Transform);
        // 上游适配器是默认导出的工厂，因此绑定 default 导出。
        Assert.AreEqual("default", component.ExportName);

        var additionalAttributes = componentType.GetProperty(nameof(VueFilePond.AdditionalAttributes))!;
        Assert.IsTrue(additionalAttributes.GetCustomAttribute<Microsoft.AspNetCore.Components.ParameterAttribute>()!.CaptureUnmatchedValues);
        Assert.IsNotNull(componentType.GetProperty(nameof(VueFilePond.Options)));
    }

    [TestMethod]
    public void FilePond_CoreApi_ExposesCreateFindAndDestroy()
    {
        var methods = typeof(FilePond).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        var create = methods.Single(static m => m.Name == nameof(FilePond.Create));
        Assert.AreEqual(typeof(FilePondInstance), create.ReturnType);
        Assert.AreEqual(2, create.GetParameters().Length);

        Assert.AreEqual(typeof(void), methods.Single(static m => m.Name == nameof(FilePond.Destroy)).ReturnType);
        Assert.AreEqual(typeof(FilePondInstance), methods.Single(static m => m.Name == nameof(FilePond.Find)).ReturnType);
        Assert.AreEqual(typeof(bool), methods.Single(static m => m.Name == nameof(FilePond.Supported)).ReturnType);

        // registerPlugin 用不透明插件宿主类型表达，而非 object 兜底。
        var register = methods.Single(static m => m.Name == nameof(FilePond.RegisterPlugin));
        Assert.AreEqual(typeof(FilePondPlugin[]), register.GetParameters().Single().ParameterType);
    }

    [TestMethod]
    public void FilePond_InstanceApi_CoversUploadLifecycle()
    {
        var instance = typeof(FilePondInstance);
        Assert.AreEqual(typeof(FilePondFile[]), instance.GetMethod(nameof(FilePondInstance.GetFiles))!.ReturnType);
        Assert.AreEqual(typeof(IPromise<FilePondFile>), instance.GetMethod(nameof(FilePondInstance.ProcessFile), [typeof(string)])!.ReturnType);
        Assert.AreEqual(typeof(IPromise<FilePondFile[]>), instance.GetMethod(nameof(FilePondInstance.ProcessFiles))!.ReturnType);
        Assert.AreEqual(typeof(void), instance.GetMethod(nameof(FilePondInstance.Destroy))!.ReturnType);
    }

    [TestMethod]
    public void FilePond_RuntimeShapes_DoNotDowngradeToObject()
    {
        foreach (var type in new[]
                 {
                     typeof(FilePond), typeof(VueFilePond), typeof(FilePondInstance), typeof(FilePondOptions),
                     typeof(FilePondServerOptions), typeof(FilePondServer), typeof(FilePondFileSource),
                     typeof(FilePondFile), typeof(FilePondError), typeof(FilePondPlugin)
                 })
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                // CaptureUnmatchedValues 透传字典的值承载任意 HTML 属性，值域本身开放，属正当例外。
                // IUnion.Value 按契约就是 object（强类型分支承担契约）；
                // CaptureUnmatchedValues 透传字典的值承载任意 HTML 属性。二者值域本身开放。
                if (property.Name == nameof(System.Runtime.CompilerServices.IUnion.Value) ||
                    property.Name == nameof(VueFilePond.AdditionalAttributes))
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
    public void FilePond_Options_MirrorUpstreamFieldNames()
    {
        foreach (var (name, expected) in new[]
                 {
                     (nameof(FilePondOptions.Server), "@#server"),
                     (nameof(FilePondOptions.AllowMultiple), "@#allowMultiple"),
                     (nameof(FilePondOptions.MaxFiles), "@#maxFiles"),
                     (nameof(FilePondOptions.AllowRevert), "@#allowRevert"),
                     (nameof(FilePondOptions.OnAddFile), "@#onaddfile"),
                     (nameof(FilePondOptions.OnProcessFile), "@#onprocessfile"),
                     (nameof(FilePondOptions.OnUpdateFiles), "@#onupdatefiles"),
                     (nameof(FilePondOptions.BeforeAddFile), "@#beforeAddFile")
                 })
        {
            var property = typeof(FilePondOptions).GetProperty(name)!;
            Assert.AreEqual(expected, property.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()!.Description, name);
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
