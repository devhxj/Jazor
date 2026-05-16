using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

namespace ECMAScript.Vben.Test;

#pragma warning disable BL0006
internal static class VbenNativeRenderTreeTestHelper
{
    public static NativeRenderTreeSnapshot RenderComponent(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var builder = new RenderTreeBuilder();
        component.GetType()
            .GetMethod("BuildRenderTree", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(component, new object[] { builder });
        return new(builder.GetFrames());
    }

    public static NativeRenderTreeSnapshot RenderFragment(RenderFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        var builder = new RenderTreeBuilder();
        fragment(builder);
        return new(builder.GetFrames());
    }

    public static NativeRenderTreeSnapshot RenderFragmentFromInstanceMethod(
        object component,
        string methodName,
        params object?[] arguments)
        => RenderFragment(InvokeInstanceMethod<RenderFragment>(component, methodName, arguments));

    public static TResult InvokeInstanceMethod<TResult>(
        object target,
        string methodName,
        params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        return (TResult)target.GetType()
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(target, arguments)!;
    }

    public static TResult InvokeStaticMethod<TResult>(
        Type type,
        string methodName,
        params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        return (TResult)type
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, arguments)!;
    }

    public static void SetParameter<TComponent, TValue>(
        TComponent component,
        string parameterName,
        TValue value)
        where TComponent : class
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        component.GetType()
            .GetProperty(parameterName, BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(component, value);
    }
}

internal readonly struct NativeRenderTreeSnapshot(ArrayRange<RenderTreeFrame> frames)
{
    public bool ContainsComponent<TComponent>()
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

    public bool ContainsElement(string elementName)
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

    public bool ContainsElementWithClassToken(
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

    public bool ContainsAttribute(
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

    public bool ContainsClassToken(string expectedToken)
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
}
#pragma warning restore BL0006
