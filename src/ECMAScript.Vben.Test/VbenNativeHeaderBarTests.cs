using System.Reflection;
using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNativeHeaderBarTests
{
    [TestMethod]
    public void Vben_HeaderBar_WithoutTitles_DoesNotRenderEmptyTitlesContainer()
    {
        var frames = RenderHeaderBar(new VbenHeaderBar());

        Assert.IsFalse(ContainsElementWithClassToken(frames, "div", "vben-header__titles"));
        Assert.IsFalse(ContainsElementWithClassToken(frames, "div", "vben-header__title"));
        Assert.IsFalse(ContainsElementWithClassToken(frames, "div", "vben-header__subtitle"));
    }

    [TestMethod]
    public void Vben_HeaderBar_WithoutActionsOrUserRegion_DoesNotRenderEmptyRightRegion()
    {
        var frames = RenderHeaderBar(
            new VbenHeaderBar
            {
                Title = "Workbench"
            });

        Assert.IsFalse(ContainsElementWithClassToken(frames, "div", "vben-header__actions"));
        Assert.IsFalse(ContainsElementWithClassToken(frames, "div", "vben-header__toolbar"));
        Assert.IsFalse(ContainsElementWithClassToken(frames, "div", "vben-header__user-region"));
    }

    [TestMethod]
    public void Vben_HeaderBar_WithActionsAndUserRegion_RendersDedicatedSemanticWrappers()
    {
        var frames = RenderHeaderBar(
            new VbenHeaderBar
            {
                Actions = builder =>
                {
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "class", "header-action");
                    builder.AddContent(2, "Refresh");
                    builder.CloseElement();
                },
                UserRegion = builder =>
                {
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "class", "current-user");
                    builder.AddContent(2, "Alice");
                    builder.CloseElement();
                }
            });

        Assert.IsTrue(ContainsElementWithClassToken(frames, "div", "vben-header__actions"));
        Assert.IsTrue(ContainsElementWithClassToken(frames, "div", "vben-header__toolbar"));
        Assert.IsTrue(ContainsElementWithClassToken(frames, "div", "vben-header__user-region"));
        Assert.IsTrue(ContainsElementWithClassToken(frames, "button", "header-action"));
        Assert.IsTrue(ContainsElementWithClassToken(frames, "span", "current-user"));
    }

    #pragma warning disable BL0006
    private static ArrayRange<RenderTreeFrame> RenderHeaderBar(VbenHeaderBar component)
    {
        var builder = new RenderTreeBuilder();
        typeof(VbenHeaderBar)
            .GetMethod("BuildRenderTree", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { builder });
        return builder.GetFrames();
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
