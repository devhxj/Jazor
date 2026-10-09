using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace ECMAScript;

/// <summary>Typed native DOM properties absent from Blazor's C# event argument contracts.</summary>
/// <remarks>These projections apply to DOM-origin callbacks. WebIDL remains the owner of native interfaces.</remarks>
[ECMAScript]
public static class NativeDomEventExtensions
{
    // Do not extend System.EventArgs: routing and other synthetic CLR events share that
    // base but have no native Event carrier. Derived mouse families reuse these properties.
    // Read CurrentTarget and transient clipboard/drag payloads in the callback before awaiting.
    extension(ChangeEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern EventRef NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }

        /// <summary>Gets the native type property without copying the browser object.</summary>
        [Description("@#type")]
        public extern string Type { get; }
    }

    extension(InputFileChangeEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern EventRef NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }

        /// <summary>Gets the native type property without copying the browser object.</summary>
        [Description("@#type")]
        public extern string Type { get; }
    }

    extension(MouseEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern MouseEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }

        /// <summary>Gets the native view property without copying the browser object.</summary>
        [Description("@#view")]
        public extern WindowRef? View { get; }

        /// <summary>Gets the native sourceCapabilities property without copying the browser object.</summary>
        [Description("@#sourceCapabilities")]
        public extern InputDeviceCapabilities? SourceCapabilities { get; }

        /// <summary>Gets the native x property without copying the browser object.</summary>
        [Description("@#x")]
        public extern double X { get; }

        /// <summary>Gets the native y property without copying the browser object.</summary>
        [Description("@#y")]
        public extern double Y { get; }

        /// <summary>Gets the native relatedTarget property without copying the browser object.</summary>
        [Description("@#relatedTarget")]
        public extern EventTarget? RelatedTarget { get; }
    }

    extension(KeyboardEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern KeyboardEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }

        /// <summary>Gets the native view property without copying the browser object.</summary>
        [Description("@#view")]
        public extern WindowRef? View { get; }

        /// <summary>Gets the native sourceCapabilities property without copying the browser object.</summary>
        [Description("@#sourceCapabilities")]
        public extern InputDeviceCapabilities? SourceCapabilities { get; }

        /// <summary>Gets the native detail property without copying the browser object.</summary>
        [Description("@#detail")]
        public extern int Detail { get; }
    }

    extension(FocusEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern FocusEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }

        /// <summary>Gets the native view property without copying the browser object.</summary>
        [Description("@#view")]
        public extern WindowRef? View { get; }

        /// <summary>Gets the native sourceCapabilities property without copying the browser object.</summary>
        [Description("@#sourceCapabilities")]
        public extern InputDeviceCapabilities? SourceCapabilities { get; }

        /// <summary>Gets the native detail property without copying the browser object.</summary>
        [Description("@#detail")]
        public extern int Detail { get; }

        /// <summary>Gets the native relatedTarget property without copying the browser object.</summary>
        [Description("@#relatedTarget")]
        public extern EventTarget? RelatedTarget { get; }
    }

    extension(TouchEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern TouchEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }

        /// <summary>Gets the native view property without copying the browser object.</summary>
        [Description("@#view")]
        public extern WindowRef? View { get; }

        /// <summary>Gets the native sourceCapabilities property without copying the browser object.</summary>
        [Description("@#sourceCapabilities")]
        public extern InputDeviceCapabilities? SourceCapabilities { get; }

        /// <summary>Gets the native touches property without copying the browser object.</summary>
        [Description("@#touches")]
        public extern TouchList NativeTouches { get; }

        /// <summary>Gets the native targetTouches property without copying the browser object.</summary>
        [Description("@#targetTouches")]
        public extern TouchList NativeTargetTouches { get; }

        /// <summary>Gets the native changedTouches property without copying the browser object.</summary>
        [Description("@#changedTouches")]
        public extern TouchList NativeChangedTouches { get; }
    }

    extension(ClipboardEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern ClipboardEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }

        /// <summary>Gets the nullable native payload; read it during the clipboard or drag callback.</summary>
        [Description("@#clipboardData")]
        public extern DataTransfer? ClipboardData { get; }
    }

    extension(ErrorEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern ErrorEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }
    }

    extension(ProgressEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern ProgressEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget? Target { get; }

        /// <summary>Gets the current listener target; capture it before awaiting in the callback.</summary>
        [Description("@#currentTarget")]
        public extern EventTarget? CurrentTarget { get; }

        /// <summary>Gets the native eventPhase property without copying the browser object.</summary>
        [Description("@#eventPhase")]
        public extern ushort EventPhase { get; }

        /// <summary>Gets the native bubbles property without copying the browser object.</summary>
        [Description("@#bubbles")]
        public extern bool Bubbles { get; }

        /// <summary>Gets the native cancelable property without copying the browser object.</summary>
        [Description("@#cancelable")]
        public extern bool Cancelable { get; }

        /// <summary>Gets the native defaultPrevented property without copying the browser object.</summary>
        [Description("@#defaultPrevented")]
        public extern bool DefaultPrevented { get; }

        /// <summary>Gets the native composed property without copying the browser object.</summary>
        [Description("@#composed")]
        public extern bool Composed { get; }

        /// <summary>Gets the native isTrusted property without copying the browser object.</summary>
        [Description("@#isTrusted")]
        public extern bool IsTrusted { get; }

        /// <summary>Gets the native timeStamp property without copying the browser object.</summary>
        [Description("@#timeStamp")]
        public extern double TimeStamp { get; }
    }

    extension(PointerEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern PointerEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native tangentialPressure property without copying the browser object.</summary>
        [Description("@#tangentialPressure")]
        public extern float TangentialPressure { get; }

        /// <summary>Gets the native twist property without copying the browser object.</summary>
        [Description("@#twist")]
        public extern int Twist { get; }

        /// <summary>Gets the native altitudeAngle property without copying the browser object.</summary>
        [Description("@#altitudeAngle")]
        public extern double AltitudeAngle { get; }

        /// <summary>Gets the native azimuthAngle property without copying the browser object.</summary>
        [Description("@#azimuthAngle")]
        public extern double AzimuthAngle { get; }

        /// <summary>Gets the native persistentDeviceId property without copying the browser object.</summary>
        [Description("@#persistentDeviceId")]
        public extern int PersistentDeviceId { get; }
    }

    extension(WheelEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern WheelEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }
    }

    extension(DragEventArgs args)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern DragEvent NativeEvent
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the nullable native payload; read it during the clipboard or drag callback.</summary>
        [Description("@#dataTransfer")]
        public extern DataTransfer? NativeDataTransfer { get; }
    }
}
