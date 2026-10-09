namespace ECMAScript.ElementPlus;

/// <summary>Typed toast service entry point; use <c>ElMessage.Service.Success("Saved")</c>.</summary>
[ECMAScript("element-plus/es/index.mjs")]
[Style("element-plus/es/components/message/style/css.mjs")]
public static class ElMessage
{
    /// <summary>The upstream ElMessage named export.</summary>
    [ECMAScriptName("ElMessage")]
    public extern static ElMessageService Service { get; }
}

/// <summary>Typed dialog service entry point. Confirmation rejects when cancelled.</summary>
[ECMAScript("element-plus/es/index.mjs")]
[Style("element-plus/es/components/message-box/style/css.mjs")]
public static class ElMessageBox
{
    /// <summary>The upstream ElMessageBox named export.</summary>
    [ECMAScriptName("ElMessageBox")]
    public extern static ElMessageBoxService Service { get; }
}

/// <summary>Typed notification service entry point.</summary>
[ECMAScript("element-plus/es/index.mjs")]
[Style("element-plus/es/components/notification/style/css.mjs")]
public static class ElNotification
{
    /// <summary>The upstream ElNotification named export.</summary>
    [ECMAScriptName("ElNotification")]
    public extern static ElNotificationService Service { get; }
}

/// <summary>Closable toast handle returned by Element Plus.</summary>
[ECMAScript]
public sealed record ElMessageHandle
{
    /// <summary>Close this toast.</summary>
    [ECMAScriptName("close")]
    public extern void Close();
}

/// <summary>Closable notification handle returned by Element Plus.</summary>
[ECMAScript]
public sealed record ElNotificationHandle
{
    /// <summary>Close this notification.</summary>
    [ECMAScriptName("close")]
    public extern void Close();
}

/// <summary>The message/notification visual intent.</summary>
[String]
public enum ElFeedbackType
{
    /// <summary>Informational feedback.</summary>
    [Description("@#info")]
    Info,
    /// <summary>Successful operation.</summary>
    [Description("@#success")]
    Success,
    /// <summary>Warning feedback.</summary>
    [Description("@#warning")]
    Warning,
    /// <summary>Error feedback.</summary>
    [Description("@#error")]
    Error
}

/// <summary>Typed options for toast messages.</summary>
[ECMAScript]
public sealed record ElMessageOptions : VueProps
{
    /// <summary>The text displayed in the toast.</summary>
    [ECMAScriptName("message")]
    public string? Message { get; init; }
    /// <summary>The visual intent.</summary>
    [ECMAScriptName("type")]
    public ElFeedbackType? Type { get; init; }
    /// <summary>Time in milliseconds; zero keeps the toast open.</summary>
    [ECMAScriptName("duration")]
    public Number? Duration { get; init; }
    /// <summary>Whether to show a close button.</summary>
    [ECMAScriptName("showClose")]
    public bool? ShowClose { get; init; }
    /// <summary>Group identical messages.</summary>
    [ECMAScriptName("grouping")]
    public bool? Grouping { get; init; }
    /// <summary>Called when this toast closes.</summary>
    [ECMAScriptName("onClose")]
    public Action? OnClose { get; init; }
}

/// <summary>Typed toast operations on the upstream service.</summary>
[ECMAScript]
public sealed record ElMessageService
{
    /// <summary>Show an informational toast.</summary>
    [ECMAScriptName("info")]
    public extern ElMessageHandle Info(string message);
    /// <summary>Show an informational toast with options.</summary>
    [ECMAScriptName("info")]
    public extern ElMessageHandle Info(ElMessageOptions options);
    /// <summary>Show a successful-operation toast.</summary>
    [ECMAScriptName("success")]
    public extern ElMessageHandle Success(string message);
    /// <summary>Show a successful-operation toast with options.</summary>
    [ECMAScriptName("success")]
    public extern ElMessageHandle Success(ElMessageOptions options);
    /// <summary>Show a warning toast.</summary>
    [ECMAScriptName("warning")]
    public extern ElMessageHandle Warning(string message);
    /// <summary>Show a warning toast with options.</summary>
    [ECMAScriptName("warning")]
    public extern ElMessageHandle Warning(ElMessageOptions options);
    /// <summary>Show an error toast.</summary>
    [ECMAScriptName("error")]
    public extern ElMessageHandle Error(string message);
    /// <summary>Show an error toast with options.</summary>
    [ECMAScriptName("error")]
    public extern ElMessageHandle Error(ElMessageOptions options);
    /// <summary>Close every toast.</summary>
    [ECMAScriptName("closeAll")]
    public extern void CloseAll();
}

/// <summary>The successful action returned by a message box.</summary>
[String]
public enum ElMessageBoxAction
{
    /// <summary>The user confirmed the dialog.</summary>
    [Description("@#confirm")]
    Confirm
}

/// <summary>Successful prompt result, containing the authored text.</summary>
[ECMAScript]
public sealed record ElMessageBoxPromptResult : VueProps
{
    /// <summary>The user's input.</summary>
    [ECMAScriptName("value")]
    public string Value { get; init; } = default!;
    /// <summary>The successful dialog action.</summary>
    [ECMAScriptName("action")]
    public ElMessageBoxAction Action { get; init; }
}

/// <summary>Typed confirmation, alert and prompt options.</summary>
[ECMAScript]
public sealed record ElMessageBoxOptions : VueProps
{
    /// <summary>The visual intent.</summary>
    [ECMAScriptName("type")]
    public ElFeedbackType? Type { get; init; }
    /// <summary>Text on the confirmation button.</summary>
    [ECMAScriptName("confirmButtonText")]
    public string? ConfirmButtonText { get; init; }
    /// <summary>Text on the cancellation button.</summary>
    [ECMAScriptName("cancelButtonText")]
    public string? CancelButtonText { get; init; }
    /// <summary>Whether cancellation is offered.</summary>
    [ECMAScriptName("showCancelButton")]
    public bool? ShowCancelButton { get; init; }
    /// <summary>Initial prompt text.</summary>
    [ECMAScriptName("inputValue")]
    public string? InputValue { get; init; }
    /// <summary>Prompt placeholder.</summary>
    [ECMAScriptName("inputPlaceholder")]
    public string? InputPlaceholder { get; init; }
    /// <summary>Whether backdrop clicks close the dialog.</summary>
    [ECMAScriptName("closeOnClickModal")]
    public bool? CloseOnClickModal { get; init; }
    /// <summary>Whether Escape closes the dialog.</summary>
    [ECMAScriptName("closeOnPressEscape")]
    public bool? CloseOnPressEscape { get; init; }
}

/// <summary>Typed dialog operations. Cancel/close propagate through promise rejection.</summary>
[ECMAScript]
public sealed record ElMessageBoxService
{
    /// <summary>Show an alert.</summary>
    [ECMAScriptName("alert")]
    public extern IPromise<ElMessageBoxAction> Alert(string message);
    /// <summary>Show a titled alert with options.</summary>
    [ECMAScriptName("alert")]
    public extern IPromise<ElMessageBoxAction> Alert(string message, string title, ElMessageBoxOptions? options = null);
    /// <summary>Ask for confirmation; cancellation rejects the promise.</summary>
    [ECMAScriptName("confirm")]
    public extern IPromise<ElMessageBoxAction> Confirm(string message);
    /// <summary>Ask for titled confirmation; cancellation rejects the promise.</summary>
    [ECMAScriptName("confirm")]
    public extern IPromise<ElMessageBoxAction> Confirm(string message, string title, ElMessageBoxOptions? options = null);
    /// <summary>Ask for text; cancellation rejects the promise.</summary>
    [ECMAScriptName("prompt")]
    public extern IPromise<ElMessageBoxPromptResult> Prompt(string message);
    /// <summary>Ask for text with a title and options.</summary>
    [ECMAScriptName("prompt")]
    public extern IPromise<ElMessageBoxPromptResult> Prompt(string message, string title, ElMessageBoxOptions? options = null);
    /// <summary>Close the currently displayed dialog.</summary>
    [ECMAScriptName("close")]
    public extern void Close();
}

/// <summary>Typed notification options.</summary>
[ECMAScript]
public sealed record ElNotificationOptions : VueProps
{
    /// <summary>The notification title.</summary>
    [ECMAScriptName("title")]
    public string? Title { get; init; }
    /// <summary>The notification message.</summary>
    [ECMAScriptName("message")]
    public string? Message { get; init; }
    /// <summary>The visual intent.</summary>
    [ECMAScriptName("type")]
    public ElFeedbackType? Type { get; init; }
    /// <summary>Time in milliseconds; zero keeps the notification open.</summary>
    [ECMAScriptName("duration")]
    public Number? Duration { get; init; }
    /// <summary>Whether to show a close button.</summary>
    [ECMAScriptName("showClose")]
    public bool? ShowClose { get; init; }
    /// <summary>Called when the notification is clicked.</summary>
    [ECMAScriptName("onClick")]
    public Action? OnClick { get; init; }
    /// <summary>Called when the notification closes.</summary>
    [ECMAScriptName("onClose")]
    public Action? OnClose { get; init; }
}

/// <summary>Typed notification operations on the upstream service.</summary>
[ECMAScript]
public sealed record ElNotificationService
{
    /// <summary>Show an informational notification.</summary>
    [ECMAScriptName("info")]
    public extern ElNotificationHandle Info(string message);
    /// <summary>Show an informational notification with options.</summary>
    [ECMAScriptName("info")]
    public extern ElNotificationHandle Info(ElNotificationOptions options);
    /// <summary>Show a successful-operation notification.</summary>
    [ECMAScriptName("success")]
    public extern ElNotificationHandle Success(string message);
    /// <summary>Show a successful-operation notification with options.</summary>
    [ECMAScriptName("success")]
    public extern ElNotificationHandle Success(ElNotificationOptions options);
    /// <summary>Show a warning notification.</summary>
    [ECMAScriptName("warning")]
    public extern ElNotificationHandle Warning(string message);
    /// <summary>Show a warning notification with options.</summary>
    [ECMAScriptName("warning")]
    public extern ElNotificationHandle Warning(ElNotificationOptions options);
    /// <summary>Show an error notification.</summary>
    [ECMAScriptName("error")]
    public extern ElNotificationHandle Error(string message);
    /// <summary>Show an error notification with options.</summary>
    [ECMAScriptName("error")]
    public extern ElNotificationHandle Error(ElNotificationOptions options);
    /// <summary>Close every notification.</summary>
    [ECMAScriptName("closeAll")]
    public extern void CloseAll();
}
