using System.Reflection;
using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNativePageContainerTests
{
    [TestMethod]
    public void Vben_PageContainer_BreadcrumbHref_RendersNavigableAnchor()
    {
        var breadcrumb = new VbenBreadcrumbItem
        {
            Key = "dashboard",
            Title = "Dashboard",
            Target = "/dashboard"
        };

        var frames = RenderBreadcrumbItem(breadcrumb);

        Assert.IsTrue(ContainsElement(frames, "a"));
        Assert.IsTrue(ContainsAttribute(frames, "href", "/dashboard"));
        Assert.IsFalse(ContainsAttribute(frames, "aria-disabled"));
    }

    [TestMethod]
    public void Vben_PageContainer_BreadcrumbRouteHash_RendersResolvedHref()
    {
        var breadcrumb = new VbenBreadcrumbItem
        {
            Key = "logs",
            Title = "Logs",
            Target = new VbenRouteLocation
            {
                Path = "/ops",
                Hash = "logs"
            }
        };

        var frames = RenderBreadcrumbItem(breadcrumb);

        Assert.IsTrue(ContainsElement(frames, "a"));
        Assert.IsTrue(ContainsAttribute(frames, "href", "/ops#logs"));
    }

    [TestMethod]
    public void Vben_PageContainer_DisabledBreadcrumb_DoesNotRenderNavigableAnchor()
    {
        var breadcrumb = new VbenBreadcrumbItem
        {
            Key = "reports",
            Title = "Reports",
            Disabled = true,
            Target = "/reports"
        };

        var frames = RenderBreadcrumbItem(breadcrumb);

        Assert.IsFalse(ContainsElement(frames, "a"));
        Assert.IsTrue(ContainsElement(frames, "span"));
        Assert.IsTrue(ContainsAttribute(frames, "aria-disabled"));
        Assert.IsFalse(ContainsAttribute(frames, "href", "/reports"));
    }

    [TestMethod]
    public void Vben_PageContainer_ActionHref_RendersNavigableAnchor()
    {
        var action = new VbenPageAction
        {
            Key = "create",
            Text = "Create",
            Kind = VbenPageActionKind.Primary,
            Target = "/create"
        };

        var frames = RenderAction(action);

        Assert.IsTrue(ContainsElement(frames, "a"));
        Assert.IsTrue(ContainsAttribute(frames, "href", "/create"));
        Assert.IsTrue(ContainsClassToken(frames, "vben-page__action"));
        Assert.IsTrue(ContainsClassToken(frames, "vben-page__action--primary"));
    }

    [TestMethod]
    public void Vben_PageContainer_ActionRouteHash_RendersResolvedHref()
    {
        var action = new VbenPageAction
        {
            Key = "preview",
            Text = "Preview",
            Kind = VbenPageActionKind.Link,
            Target = new VbenRouteLocation
            {
                Hash = "#preview"
            }
        };

        var frames = RenderAction(action);

        Assert.IsTrue(ContainsElement(frames, "a"));
        Assert.IsTrue(ContainsAttribute(frames, "href", "#preview"));
        Assert.IsTrue(ContainsClassToken(frames, "vben-page__action"));
        Assert.IsTrue(ContainsClassToken(frames, "vben-page__action--link"));
    }

    [TestMethod]
    public void Vben_PageContainer_DisabledActionWithTarget_DoesNotRenderNavigableAnchor()
    {
        var action = new VbenPageAction
        {
            Key = "danger",
            Text = "Delete",
            Kind = VbenPageActionKind.Danger,
            Disabled = true,
            Target = "/danger"
        };

        var frames = RenderAction(action);

        Assert.IsFalse(ContainsElement(frames, "a"));
        Assert.IsTrue(ContainsElement(frames, "button"));
        Assert.IsTrue(ContainsAttribute(frames, "disabled"));
        Assert.IsTrue(ContainsAttribute(frames, "aria-disabled"));
        Assert.IsFalse(ContainsAttribute(frames, "href", "/danger"));
        Assert.IsTrue(ContainsClassToken(frames, "vben-page__action"));
        Assert.IsTrue(ContainsClassToken(frames, "vben-page__action--danger"));
        Assert.IsTrue(ContainsClassToken(frames, "is-disabled"));
    }

    private static ArrayRange<RenderTreeFrame> RenderBreadcrumbItem(VbenBreadcrumbItem item)
    {
        var component = new VbenPageContainer();
        var fragment = (RenderFragment)typeof(VbenPageContainer)
            .GetMethod("RenderBreadcrumbItem", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { item })!;

        return RenderFragment(fragment);
    }

    private static ArrayRange<RenderTreeFrame> RenderAction(VbenPageAction action)
    {
        var component = new VbenPageContainer();
        var fragment = (RenderFragment)typeof(VbenPageContainer)
            .GetMethod("RenderAction", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { action })!;

        return RenderFragment(fragment);
    }

    #pragma warning disable BL0006
    private static ArrayRange<RenderTreeFrame> RenderFragment(RenderFragment fragment)
    {
        var builder = new RenderTreeBuilder();
        fragment(builder);
        return builder.GetFrames();
    }

    private static bool ContainsElement(ArrayRange<RenderTreeFrame> frames, string elementName)
    {
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType == RenderTreeFrameType.Element
                && string.Equals(frame.ElementName, elementName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAttribute(
        ArrayRange<RenderTreeFrame> frames,
        string attributeName,
        string? expectedValue = null)
    {
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType != RenderTreeFrameType.Attribute
                || !string.Equals(frame.AttributeName, attributeName, StringComparison.Ordinal))
            {
                continue;
            }

            if (expectedValue is null)
            {
                return true;
            }

            if (string.Equals(frame.AttributeValue?.ToString(), expectedValue, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsClassToken(ArrayRange<RenderTreeFrame> frames, string expectedToken)
    {
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType != RenderTreeFrameType.Attribute
                || !string.Equals(frame.AttributeName, "class", StringComparison.Ordinal))
            {
                continue;
            }

            var classNames = frame.AttributeValue?.ToString();
            if (classNames is null)
            {
                continue;
            }

            foreach (var token in classNames.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.Equals(token, expectedToken, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #pragma warning restore BL0006
}
