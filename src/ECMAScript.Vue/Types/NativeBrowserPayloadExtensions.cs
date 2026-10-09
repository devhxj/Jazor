using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using BlazorDataTransfer = Microsoft.AspNetCore.Components.Web.DataTransfer;
using BlazorDataTransferItem = Microsoft.AspNetCore.Components.Web.DataTransferItem;
using BlazorTouchPoint = Microsoft.AspNetCore.Components.Web.TouchPoint;

namespace ECMAScript;

/// <summary>Native WebIDL payloads projected from existing Blazor C# carrier types.</summary>
[ECMAScript]
public static class NativeBrowserPayloadExtensions
{
    extension(BlazorDataTransfer transfer)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern DataTransfer NativeDataTransfer
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        // CLR Files is string[] and Items is a DTO array. C# instance members win over
        // extensions, so keep distinct names for the browser-owned native collections.
        /// <summary>Gets the native files property without copying the browser object.</summary>
        [Description("@#files")]
        public extern FileList NativeFiles { get; }

        /// <summary>Gets the native items property without copying the browser object.</summary>
        [Description("@#items")]
        public extern DataTransferItemList NativeItems { get; }
    }

    extension(BlazorDataTransferItem item)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern DataTransferItem NativeItem
        {
            [ECMAScriptInline("__arg1")]
            get;
        }
    }

    extension(BlazorTouchPoint point)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern Touch NativeTouch
        {
            [ECMAScriptInline("__arg1")]
            get;
        }

        /// <summary>Gets the native target property without copying the browser object.</summary>
        [Description("@#target")]
        public extern EventTarget Target { get; }

        /// <summary>Gets the native radiusX property without copying the browser object.</summary>
        [Description("@#radiusX")]
        public extern float RadiusX { get; }

        /// <summary>Gets the native radiusY property without copying the browser object.</summary>
        [Description("@#radiusY")]
        public extern float RadiusY { get; }

        /// <summary>Gets the native rotationAngle property without copying the browser object.</summary>
        [Description("@#rotationAngle")]
        public extern float RotationAngle { get; }

        /// <summary>Gets the native force property without copying the browser object.</summary>
        [Description("@#force")]
        public extern float Force { get; }

        /// <summary>Gets the native altitudeAngle property without copying the browser object.</summary>
        [Description("@#altitudeAngle")]
        public extern float AltitudeAngle { get; }

        /// <summary>Gets the native azimuthAngle property without copying the browser object.</summary>
        [Description("@#azimuthAngle")]
        public extern float AzimuthAngle { get; }

        /// <summary>Gets the native touchType property without copying the browser object.</summary>
        [Description("@#touchType")]
        public extern TouchType TouchType { get; }
    }

    extension(ElementReference element)
    {
        /// <summary>Gets the same native browser object with its complete WebIDL contract.</summary>
        public extern HTMLElement NativeElement
        {
            [ECMAScriptInline("__arg1")]
            get;
        }
    }
}
