namespace ECMAScript;

/// <summary>
/// NotificationOptions
/// </summary>
[ECMAScript]
[Description("@#NotificationOptions")]
public record NotificationOptions(
    [property: Description("@#dir")]NotificationDirection Dir = NotificationDirection.Auto,
	[property: Description("@#lang")]string? Lang = default,
	[property: Description("@#body")]string? Body = default,
	[property: Description("@#tag")]string? Tag = default,
	[property: Description("@#image")]string? Image = default,
	[property: Description("@#icon")]string? Icon = default,
	[property: Description("@#badge")]string? Badge = default,
	[property: Description("@#vibrate")]VibratePattern? Vibrate = default,
	[property: Description("@#timestamp")]EpochTimeStamp? Timestamp = default,
	[property: Description("@#renotify")]bool Renotify = false,
	[property: Description("@#silent")]bool? Silent = null,
	[property: Description("@#requireInteraction")]bool RequireInteraction = false,
	[property: Description("@#data")]object? Data = default,
	[property: Description("@#actions")]NotificationAction[]? Actions = default);

/// <summary>
/// NotificationAction
/// </summary>
[ECMAScript]
[Description("@#NotificationAction")]
public record NotificationAction(
    [property: Description("@#action")]string? Action = default,
	[property: Description("@#title")]string? Title = default,
	[property: Description("@#icon")]string? Icon = default);

/// <summary>
/// GetNotificationOptions
/// </summary>
[ECMAScript]
[Description("@#GetNotificationOptions")]
public record GetNotificationOptions(
    [property: Description("@#tag")]string? Tag = default);

/// <summary>
/// NotificationEventInit
/// </summary>
[ECMAScript]
[Description("@#NotificationEventInit")]
public record NotificationEventInit(
    [property: Description("@#notification")]Notification? Notification = default,
	[property: Description("@#action")]string? Action = default): ExtendableEventInit;

/// <summary>
/// Notification
/// </summary>
[ECMAScript]
[Description("@#Notification")]
public class Notification : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="title">title</param>
    /// <param name="options">options</param>
    public extern Notification(string title, NotificationOptions options);

    /// <summary>
    /// permission
    /// </summary>
    [Description("@#permission")]
    public extern static NotificationPermission Permission { get; }

    /// <summary>
    /// requestPermission
    /// </summary>
    /// <param name="deprecatedCallback">deprecatedCallback</param>
    [Description("@#requestPermission")]
    public extern static PromiseResult<NotificationPermission> RequestPermission(NotificationPermissionCallback deprecatedCallback);

    /// <summary>
    /// maxActions
    /// </summary>
    [Description("@#maxActions")]
    public extern static uint MaxActions { get; }

    /// <summary>
    /// onclick
    /// </summary>
    [Description("@#onclick")]
    public extern EventHandler Onclick { get; set; }

    /// <summary>
    /// onshow
    /// </summary>
    [Description("@#onshow")]
    public extern EventHandler Onshow { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }

    /// <summary>
    /// title
    /// </summary>
    [Description("@#title")]
    public extern string Title { get; }

    /// <summary>
    /// dir
    /// </summary>
    [Description("@#dir")]
    public extern NotificationDirection Dir { get; }

    /// <summary>
    /// lang
    /// </summary>
    [Description("@#lang")]
    public extern string Lang { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern string Body { get; }

    /// <summary>
    /// tag
    /// </summary>
    [Description("@#tag")]
    public extern string Tag { get; }

    /// <summary>
    /// image
    /// </summary>
    [Description("@#image")]
    public extern string Image { get; }

    /// <summary>
    /// icon
    /// </summary>
    [Description("@#icon")]
    public extern string Icon { get; }

    /// <summary>
    /// badge
    /// </summary>
    [Description("@#badge")]
    public extern string Badge { get; }

    /// <summary>
    /// vibrate
    /// </summary>
    [Description("@#vibrate")]
    public extern FrozenSet<uint> Vibrate { get; }

    /// <summary>
    /// timestamp
    /// </summary>
    [Description("@#timestamp")]
    public extern EpochTimeStamp Timestamp { get; }

    /// <summary>
    /// renotify
    /// </summary>
    [Description("@#renotify")]
    public extern bool Renotify { get; }

    /// <summary>
    /// silent
    /// </summary>
    [Description("@#silent")]
    public extern bool? Silent { get; }

    /// <summary>
    /// requireInteraction
    /// </summary>
    [Description("@#requireInteraction")]
    public extern bool RequireInteraction { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }

    /// <summary>
    /// actions
    /// </summary>
    [Description("@#actions")]
    public extern FrozenSet<NotificationAction> Actions { get; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();
}

/// <summary>
/// NotificationEvent
/// </summary>
[ECMAScript]
[Description("@#NotificationEvent")]
public class NotificationEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern NotificationEvent(string type, NotificationEventInit eventInitDict);

    /// <summary>
    /// notification
    /// </summary>
    [Description("@#notification")]
    public extern Notification Notification { get; }

    /// <summary>
    /// action
    /// </summary>
    [Description("@#action")]
    public extern string Action { get; }
}