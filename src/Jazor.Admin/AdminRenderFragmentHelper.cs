using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Rendering;

namespace Jazor.Admin;

#pragma warning disable BL0006
#pragma warning disable ASP0006

internal static class AdminRenderFragmentHelper
{
    public static RenderFragment? Normalize(RenderFragment? fragment)
    {
        var capture = Capture(fragment);
        return capture.HasContent ? capture.Fragment : null;
    }

    private static CapturedRenderFragment Capture(RenderFragment? fragment)
    {
        if (fragment is null)
        {
            return default;
        }

        var scratchBuilder = new RenderTreeBuilder();
        fragment(scratchBuilder);

        var frames = scratchBuilder.GetFrames();
        if (!HasRenderableFrames(frames.Array, 0, frames.Count))
        {
            return default;
        }

        var copiedFrames = new RenderTreeFrame[frames.Count];
        Array.Copy(frames.Array, copiedFrames, frames.Count);

        return new CapturedRenderFragment(
            copiedFrames,
            static capturedFrames => replayBuilder =>
            {
                ReplayFrames(replayBuilder, capturedFrames, 0, capturedFrames.Length);
            });
    }

    // This helper intentionally works at render-tree frame level so the original slot
    // executes only once. Using the original fragment after a probe render would
    // evaluate user content twice per render pass and reintroduce duplicated side effects.
    private static bool HasRenderableFrames(
        RenderTreeFrame[] frames,
        int startIndex,
        int endIndexExclusive)
    {
        for (var index = startIndex; index < endIndexExclusive; index++)
        {
            var frame = frames[index];
            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Element:
                case RenderTreeFrameType.Component:
                    return true;
                case RenderTreeFrameType.Text:
                    if (!string.IsNullOrWhiteSpace(frame.TextContent))
                    {
                        return true;
                    }

                    break;
                case RenderTreeFrameType.Markup:
                    if (HasVisibleMarkup(frame.MarkupContent))
                    {
                        return true;
                    }

                    break;
                case RenderTreeFrameType.Region:
                {
                    var childStartIndex = index + 1;
                    var childEndIndex = index + frame.RegionSubtreeLength;
                    if (HasRenderableFrames(frames, childStartIndex, childEndIndex))
                    {
                        return true;
                    }

                    index = childEndIndex - 1;
                    break;
                }
            }
        }

        return false;
    }

    private static bool HasVisibleMarkup(string? markup)
    {
        if (string.IsNullOrWhiteSpace(markup))
        {
            return false;
        }

        ReadOnlySpan<char> remaining = markup.AsSpan();
        while (!remaining.IsEmpty)
        {
            remaining = remaining.TrimStart();
            if (remaining.IsEmpty)
            {
                return false;
            }

            if (!remaining.StartsWith("<!--", StringComparison.Ordinal))
            {
                return true;
            }

            var endIndex = remaining.IndexOf("-->", StringComparison.Ordinal);
            if (endIndex < 0)
            {
                return true;
            }

            remaining = remaining[(endIndex + 3)..];
        }

        return false;
    }

    private static void ReplayFrames(
        RenderTreeBuilder builder,
        RenderTreeFrame[] frames,
        int startIndex,
        int endIndexExclusive)
    {
        for (var index = startIndex; index < endIndexExclusive; index++)
        {
            var frame = frames[index];
            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Element:
                {
                    builder.OpenElement(frame.Sequence, frame.ElementName);
                    if (frame.ElementKey is not null)
                    {
                        builder.SetKey(frame.ElementKey);
                    }

                    var childStartIndex = index + 1;
                    var childEndIndex = index + frame.ElementSubtreeLength;
                    ReplayFrames(builder, frames, childStartIndex, childEndIndex);
                    builder.CloseElement();
                    index = childEndIndex - 1;
                    break;
                }
                case RenderTreeFrameType.Text:
                    builder.AddContent(frame.Sequence, frame.TextContent);
                    break;
                case RenderTreeFrameType.Markup:
                    builder.AddMarkupContent(frame.Sequence, frame.MarkupContent);
                    break;
                case RenderTreeFrameType.Attribute:
                    builder.AddAttribute(frame.Sequence, frame);
                    break;
                case RenderTreeFrameType.Component:
                {
                    builder.OpenComponent(frame.Sequence, frame.ComponentType);
                    if (frame.ComponentKey is not null)
                    {
                        builder.SetKey(frame.ComponentKey);
                    }

                    var childStartIndex = index + 1;
                    var childEndIndex = index + frame.ComponentSubtreeLength;
                    ReplayFrames(builder, frames, childStartIndex, childEndIndex);
                    builder.CloseComponent();
                    index = childEndIndex - 1;
                    break;
                }
                case RenderTreeFrameType.Region:
                {
                    builder.OpenRegion(frame.Sequence);
                    var childStartIndex = index + 1;
                    var childEndIndex = index + frame.RegionSubtreeLength;
                    ReplayFrames(builder, frames, childStartIndex, childEndIndex);
                    builder.CloseRegion();
                    index = childEndIndex - 1;
                    break;
                }
                case RenderTreeFrameType.ElementReferenceCapture:
                    builder.AddElementReferenceCapture(frame.Sequence, frame.ElementReferenceCaptureAction);
                    break;
                case RenderTreeFrameType.ComponentReferenceCapture:
                    builder.AddComponentReferenceCapture(frame.Sequence, frame.ComponentReferenceCaptureAction);
                    break;
                case RenderTreeFrameType.ComponentRenderMode:
                    builder.AddComponentRenderMode(frame.ComponentRenderMode);
                    break;
                case RenderTreeFrameType.NamedEvent:
                    builder.AddNamedEvent(frame.NamedEventType, frame.NamedEventAssignedName);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported render-tree frame type '{frame.FrameType}'.");
            }
        }
    }

    private readonly record struct CapturedRenderFragment(
        RenderTreeFrame[] Frames,
        Func<RenderTreeFrame[], RenderFragment> Factory)
    {
        public bool HasContent
            => Frames is { Length: > 0 };

        public RenderFragment Fragment
            => Factory(Frames);
    }
}

#pragma warning restore ASP0006
#pragma warning restore BL0006
