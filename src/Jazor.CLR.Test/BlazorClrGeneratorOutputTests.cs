using System.Reflection;
using ECMAScript;
using ECMAScript.Contract;
using Jazor.CLR.Generator;
using Microsoft.AspNetCore.Components;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class BlazorClrGeneratorOutputTests
{
    [TestMethod]
    public void ModuleOutputNaming_PreservesDistinctGenericAndNonGenericBlazorArtifacts()
    {
        var types = new[]
        {
            typeof(EventCallback),
            typeof(EventCallback<>),
            typeof(RenderFragment),
            typeof(RenderFragment<>)
        };
        var names = types
            .Select(ModuleOutputNaming.GetModuleName)
            .ToArray();

        CollectionAssert.AreEqual(
            new[]
            {
                "EventCallbackModule",
                "EventCallbackT1Module",
                "RenderFragmentModule",
                "RenderFragmentT1Module"
            },
            names);
        Assert.HasCount(types.Length, names.Distinct(StringComparer.Ordinal));
        CollectionAssert.AreEqual(
            new[]
            {
                "Microsoft/AspNetCore/Components/EventCallbackModule.js",
                "Microsoft/AspNetCore/Components/EventCallbackT1Module.js",
                "Microsoft/AspNetCore/Components/RenderFragmentModule.js",
                "Microsoft/AspNetCore/Components/RenderFragmentT1Module.js"
            },
            types.Select(ModuleOutputNaming.GetModulePath).ToArray());
    }

    [TestMethod]
    public void MouseEventArgsModule_WebIdlIntegerGettersUseNumberCarriers()
    {
        var expectedMembers = new[]
        {
            "Microsoft.AspNetCore.Components.Web.MouseEventArgs.Detail.get",
            "Microsoft.AspNetCore.Components.Web.MouseEventArgs.Button.get",
            "Microsoft.AspNetCore.Components.Web.MouseEventArgs.Buttons.get"
        };

        var methods = typeof(MouseEventArgsModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => (Method: method, Mapping: method.GetCustomAttribute<JazorAttribute>()))
            .Where(static entry => entry.Mapping is not null)
            .ToDictionary(static entry => entry.Mapping!.Member, static entry => entry.Method, StringComparer.Ordinal);

        foreach (var member in expectedMembers)
        {
            Assert.IsTrue(methods.TryGetValue(member, out var method), $"Missing mapping: {member}");
            Assert.AreEqual(typeof(Number), method.ReturnType, member);
        }
    }

    [TestMethod]
    public void PointerAndWheelEventArgsModules_WebIdlLongGettersUseNumberCarriers()
    {
        AssertNumberGetters(
            typeof(PointerEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerId.get",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Width.get",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Height.get",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.Pressure.get",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltX.get",
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltY.get");
        AssertNumberGetters(
            typeof(WheelEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaX.get",
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaY.get",
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaZ.get",
            "Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaMode.get");
    }

    [TestMethod]
    public void DragAndClipboardModules_UseNativeEventAndTransferCarriers()
    {
        Assert.AreEqual(
            typeof(DataTransfer),
            typeof(DragEventArgsModule)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(method => method.GetCustomAttribute<JazorAttribute>()?.Member ==
                    "Microsoft.AspNetCore.Components.Web.DragEventArgs.DataTransfer.get")
                .ReturnType);
        Assert.AreEqual(
            typeof(Array<string>),
            typeof(DataTransferModule)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(method => method.GetCustomAttribute<JazorAttribute>()?.Member ==
                    "Microsoft.AspNetCore.Components.Web.DataTransfer.Types.get")
                .ReturnType);
        Assert.AreEqual(
            typeof(string),
            typeof(ClipboardEventArgsModule)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(method => method.GetCustomAttribute<JazorAttribute>()?.Member ==
                    "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.Type.get")
                .ReturnType);
    }

    [TestMethod]
    public void TouchErrorAndProgressModules_UseWebIdlCarriers()
    {
        AssertNumberGetters(
            typeof(TouchEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.TouchEventArgs.Detail.get");
        AssertNumberGetters(
            typeof(TouchPointModule),
            "Microsoft.AspNetCore.Components.Web.TouchPoint.Identifier.get",
            "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenX.get",
            "Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenY.get",
            "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientX.get",
            "Microsoft.AspNetCore.Components.Web.TouchPoint.ClientY.get",
            "Microsoft.AspNetCore.Components.Web.TouchPoint.PageX.get",
            "Microsoft.AspNetCore.Components.Web.TouchPoint.PageY.get");
        AssertNumberGetters(
            typeof(ErrorEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Lineno.get",
            "Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Colno.get");
        AssertBigIntGetters(
            typeof(ProgressEventArgsModule),
            "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Loaded.get",
            "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Total.get");
    }

    private static void AssertBigIntGetters(Type module, params string[] expectedMembers)
    {
        var methods = module
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => (Method: method, Mapping: method.GetCustomAttribute<JazorAttribute>()))
            .Where(static entry => entry.Mapping is not null)
            .ToDictionary(static entry => entry.Mapping!.Member, static entry => entry.Method, StringComparer.Ordinal);

        foreach (var member in expectedMembers)
        {
            Assert.IsTrue(methods.TryGetValue(member, out var method), $"Missing mapping: {member}");
            Assert.AreEqual(typeof(BigInt), method.ReturnType, member);
        }
    }

    private static void AssertNumberGetters(Type module, params string[] expectedMembers)
    {
        var methods = module
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => (Method: method, Mapping: method.GetCustomAttribute<JazorAttribute>()))
            .Where(static entry => entry.Mapping is not null)
            .ToDictionary(static entry => entry.Mapping!.Member, static entry => entry.Method, StringComparer.Ordinal);

        foreach (var member in expectedMembers)
        {
            Assert.IsTrue(methods.TryGetValue(member, out var method), $"Missing mapping: {member}");
            Assert.AreEqual(typeof(Number), method.ReturnType, member);
        }
    }
}
