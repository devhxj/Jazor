using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNativeAdminLayoutTests
{
    [TestMethod]
    public void Vben_AdminLayout_TopMode_DoesNotRenderSidebarRegionOrDefaultSidebarMenu()
    {
        var frames = RenderLayout(
            new VbenAdminLayout
            {
                Mode = VbenLayoutMode.Top,
                NavItems =
                [
                    new VbenNavItem
                    {
                        Key = "dashboard",
                        Title = "Dashboard"
                    }
                ]
            });

        Assert.IsFalse(ContainsElementWithClassToken(frames, "aside", "vben-shell__sidebar"));
        Assert.IsFalse(ContainsComponent<VbenSidebarMenu>(frames));
        Assert.IsTrue(ContainsElementWithClassToken(frames, "div", "vben-shell__main"));
        Assert.IsTrue(ContainsElementWithClassToken(frames, "header", "vben-shell__header"));
        Assert.IsTrue(ContainsElementWithClassToken(frames, "main", "vben-shell__content"));
    }

    [TestMethod]
    public void Vben_AdminLayout_SidebarMode_RendersDefaultSidebarMenuWhenNavItemsExist()
    {
        var frames = RenderLayout(
            new VbenAdminLayout
            {
                Mode = VbenLayoutMode.Sidebar,
                NavItems =
                [
                    new VbenNavItem
                    {
                        Key = "dashboard",
                        Title = "Dashboard"
                    }
                ]
            });

        Assert.IsTrue(ContainsElementWithClassToken(frames, "aside", "vben-shell__sidebar"));
        Assert.IsTrue(ContainsComponent<VbenSidebarMenu>(frames));
    }

    [TestMethod]
    public void Vben_AdminLayout_MixedMode_PreservesSidebarRegion()
    {
        var frames = RenderLayout(
            new VbenAdminLayout
            {
                Mode = VbenLayoutMode.Mixed,
                Sidebar = builder =>
                {
                    builder.OpenElement(0, "section");
                    builder.AddAttribute(1, "class", "custom-sidebar");
                    builder.AddContent(2, "Custom sidebar");
                    builder.CloseElement();
                }
            });

        Assert.IsTrue(ContainsElementWithClassToken(frames, "aside", "vben-shell__sidebar"));
        Assert.IsTrue(ContainsElementWithClassToken(frames, "section", "custom-sidebar"));
    }

    #pragma warning disable BL0006
    private static ArrayRange<RenderTreeFrame> RenderLayout(VbenAdminLayout component)
    {
        var builder = new RenderTreeBuilder();
        typeof(VbenAdminLayout)
            .GetMethod("BuildRenderTree", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { builder });
        return builder.GetFrames();
    }

    private static bool ContainsComponent<TComponent>(ArrayRange<RenderTreeFrame> frames)
    {
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType == RenderTreeFrameType.Component
                && frame.ComponentType == typeof(TComponent))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsElementWithClassToken(
        ArrayRange<RenderTreeFrame> frames,
        string elementName,
        string expectedClassToken)
    {
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType != RenderTreeFrameType.Element
                || !string.Equals(frame.ElementName, elementName, StringComparison.Ordinal))
            {
                continue;
            }

            for (var attributeIndex = index + 1; attributeIndex < frames.Count; attributeIndex++)
            {
                var attributeFrame = frames.Array[attributeIndex];
                if (attributeFrame.FrameType == RenderTreeFrameType.Element
                    || attributeFrame.FrameType == RenderTreeFrameType.Component
                    || attributeFrame.FrameType == RenderTreeFrameType.Region)
                {
                    break;
                }

                if (attributeFrame.FrameType != RenderTreeFrameType.Attribute
                    || !string.Equals(attributeFrame.AttributeName, "class", StringComparison.Ordinal))
                {
                    continue;
                }

                var classNames = attributeFrame.AttributeValue?.ToString();
                if (classNames is null)
                {
                    break;
                }

                foreach (var token in classNames.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (string.Equals(token, expectedClassToken, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                break;
            }
        }

        return false;
    }
    #pragma warning restore BL0006
}
