namespace ECMAScript;

/// <summary>
/// SummarizerCreateCoreOptions
/// </summary>
[ECMAScript]
[Description("@#SummarizerCreateCoreOptions")]
public record SummarizerCreateCoreOptions(
    [property: Description("@#type")]SummarizerType Type = SummarizerType.KeyPoints,
	[property: Description("@#format")]SummarizerFormat Format = SummarizerFormat.Markdown,
	[property: Description("@#length")]SummarizerLength Length = SummarizerLength.Short,
	[property: Description("@#expectedInputLanguages")]string[]? ExpectedInputLanguages = default,
	[property: Description("@#expectedContextLanguages")]string[]? ExpectedContextLanguages = default,
	[property: Description("@#outputLanguage")]string? OutputLanguage = default);

/// <summary>
/// SummarizerCreateOptions
/// </summary>
[ECMAScript]
[Description("@#SummarizerCreateOptions")]
public record SummarizerCreateOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#monitor")]CreateMonitorCallback? Monitor = default,
	[property: Description("@#sharedContext")]string? SharedContext = default): SummarizerCreateCoreOptions;

/// <summary>
/// SummarizerSummarizeOptions
/// </summary>
[ECMAScript]
[Description("@#SummarizerSummarizeOptions")]
public record SummarizerSummarizeOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#context")]string? Context = default);

/// <summary>
/// WriterCreateCoreOptions
/// </summary>
[ECMAScript]
[Description("@#WriterCreateCoreOptions")]
public record WriterCreateCoreOptions(
    [property: Description("@#tone")]WriterTone Tone = WriterTone.Neutral,
	[property: Description("@#format")]WriterFormat Format = WriterFormat.Markdown,
	[property: Description("@#length")]WriterLength Length = WriterLength.Short,
	[property: Description("@#expectedInputLanguages")]string[]? ExpectedInputLanguages = default,
	[property: Description("@#expectedContextLanguages")]string[]? ExpectedContextLanguages = default,
	[property: Description("@#outputLanguage")]string? OutputLanguage = default);

/// <summary>
/// WriterCreateOptions
/// </summary>
[ECMAScript]
[Description("@#WriterCreateOptions")]
public record WriterCreateOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#monitor")]CreateMonitorCallback? Monitor = default,
	[property: Description("@#sharedContext")]string? SharedContext = default): WriterCreateCoreOptions;

/// <summary>
/// WriterWriteOptions
/// </summary>
[ECMAScript]
[Description("@#WriterWriteOptions")]
public record WriterWriteOptions(
    [property: Description("@#context")]string? Context = default,
	[property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// RewriterCreateCoreOptions
/// </summary>
[ECMAScript]
[Description("@#RewriterCreateCoreOptions")]
public record RewriterCreateCoreOptions(
    [property: Description("@#tone")]RewriterTone Tone = RewriterTone.AsIs,
	[property: Description("@#format")]RewriterFormat Format = RewriterFormat.AsIs,
	[property: Description("@#length")]RewriterLength Length = RewriterLength.AsIs,
	[property: Description("@#expectedInputLanguages")]string[]? ExpectedInputLanguages = default,
	[property: Description("@#expectedContextLanguages")]string[]? ExpectedContextLanguages = default,
	[property: Description("@#outputLanguage")]string? OutputLanguage = default);

/// <summary>
/// RewriterCreateOptions
/// </summary>
[ECMAScript]
[Description("@#RewriterCreateOptions")]
public record RewriterCreateOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#monitor")]CreateMonitorCallback? Monitor = default,
	[property: Description("@#sharedContext")]string? SharedContext = default): RewriterCreateCoreOptions;

/// <summary>
/// RewriterRewriteOptions
/// </summary>
[ECMAScript]
[Description("@#RewriterRewriteOptions")]
public record RewriterRewriteOptions(
    [property: Description("@#context")]string? Context = default,
	[property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// Summarizer
/// </summary>
[ECMAScript]
[Description("@#Summarizer")]
public class Summarizer
{
    /// <summary>
    /// create
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#create")]
    public extern static PromiseResult<Summarizer> Create(SummarizerCreateOptions? options = default);

    /// <summary>
    /// availability
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#availability")]
    public extern static PromiseResult<Availability> Availability(SummarizerCreateCoreOptions? options = default);

    /// <summary>
    /// summarize
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#summarize")]
    public extern PromiseResult<string> Summarize(string input, SummarizerSummarizeOptions? options = default);

    /// <summary>
    /// summarizeStreaming
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#summarizeStreaming")]
    public extern ReadableStream SummarizeStreaming(string input, SummarizerSummarizeOptions? options = default);

    /// <summary>
    /// sharedContext
    /// </summary>
    [Description("@#sharedContext")]
    public extern string SharedContext { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern SummarizerType Type { get; }

    /// <summary>
    /// format
    /// </summary>
    [Description("@#format")]
    public extern SummarizerFormat Format { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern SummarizerLength Length { get; }

    /// <summary>
    /// expectedInputLanguages
    /// </summary>
    [Description("@#expectedInputLanguages")]
    public extern FrozenSet<string>? ExpectedInputLanguages { get; }

    /// <summary>
    /// expectedContextLanguages
    /// </summary>
    [Description("@#expectedContextLanguages")]
    public extern FrozenSet<string>? ExpectedContextLanguages { get; }

    /// <summary>
    /// outputLanguage
    /// </summary>
    [Description("@#outputLanguage")]
    public extern string? OutputLanguage { get; }

    /// <summary>
    /// measureInputUsage
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#measureInputUsage")]
    public extern PromiseResult<double> MeasureInputUsage(string input, SummarizerSummarizeOptions? options = default);

    /// <summary>
    /// inputQuota
    /// </summary>
    [Description("@#inputQuota")]
    public extern double InputQuota { get; }

    #region mixin DestroyableModel
    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();
    #endregion
}

/// <summary>
/// Writer
/// </summary>
[ECMAScript]
[Description("@#Writer")]
public class Writer
{
    /// <summary>
    /// create
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#create")]
    public extern static PromiseResult<Writer> Create(WriterCreateOptions? options = default);

    /// <summary>
    /// availability
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#availability")]
    public extern static PromiseResult<Availability> Availability(WriterCreateCoreOptions? options = default);

    /// <summary>
    /// write
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#write")]
    public extern PromiseResult<string> Write(string input, WriterWriteOptions? options = default);

    /// <summary>
    /// writeStreaming
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#writeStreaming")]
    public extern ReadableStream WriteStreaming(string input, WriterWriteOptions? options = default);

    /// <summary>
    /// sharedContext
    /// </summary>
    [Description("@#sharedContext")]
    public extern string SharedContext { get; }

    /// <summary>
    /// tone
    /// </summary>
    [Description("@#tone")]
    public extern WriterTone Tone { get; }

    /// <summary>
    /// format
    /// </summary>
    [Description("@#format")]
    public extern WriterFormat Format { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern WriterLength Length { get; }

    /// <summary>
    /// expectedInputLanguages
    /// </summary>
    [Description("@#expectedInputLanguages")]
    public extern FrozenSet<string>? ExpectedInputLanguages { get; }

    /// <summary>
    /// expectedContextLanguages
    /// </summary>
    [Description("@#expectedContextLanguages")]
    public extern FrozenSet<string>? ExpectedContextLanguages { get; }

    /// <summary>
    /// outputLanguage
    /// </summary>
    [Description("@#outputLanguage")]
    public extern string? OutputLanguage { get; }

    /// <summary>
    /// measureInputUsage
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#measureInputUsage")]
    public extern PromiseResult<double> MeasureInputUsage(string input, WriterWriteOptions? options = default);

    /// <summary>
    /// inputQuota
    /// </summary>
    [Description("@#inputQuota")]
    public extern double InputQuota { get; }

    #region mixin DestroyableModel
    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();
    #endregion
}

/// <summary>
/// Rewriter
/// </summary>
[ECMAScript]
[Description("@#Rewriter")]
public class Rewriter
{
    /// <summary>
    /// create
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#create")]
    public extern static PromiseResult<Rewriter> Create(RewriterCreateOptions? options = default);

    /// <summary>
    /// availability
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#availability")]
    public extern static PromiseResult<Availability> Availability(RewriterCreateCoreOptions? options = default);

    /// <summary>
    /// rewrite
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#rewrite")]
    public extern PromiseResult<string> Rewrite(string input, RewriterRewriteOptions? options = default);

    /// <summary>
    /// rewriteStreaming
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#rewriteStreaming")]
    public extern ReadableStream RewriteStreaming(string input, RewriterRewriteOptions? options = default);

    /// <summary>
    /// sharedContext
    /// </summary>
    [Description("@#sharedContext")]
    public extern string SharedContext { get; }

    /// <summary>
    /// tone
    /// </summary>
    [Description("@#tone")]
    public extern RewriterTone Tone { get; }

    /// <summary>
    /// format
    /// </summary>
    [Description("@#format")]
    public extern RewriterFormat Format { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern RewriterLength Length { get; }

    /// <summary>
    /// expectedInputLanguages
    /// </summary>
    [Description("@#expectedInputLanguages")]
    public extern FrozenSet<string>? ExpectedInputLanguages { get; }

    /// <summary>
    /// expectedContextLanguages
    /// </summary>
    [Description("@#expectedContextLanguages")]
    public extern FrozenSet<string>? ExpectedContextLanguages { get; }

    /// <summary>
    /// outputLanguage
    /// </summary>
    [Description("@#outputLanguage")]
    public extern string? OutputLanguage { get; }

    /// <summary>
    /// measureInputUsage
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#measureInputUsage")]
    public extern PromiseResult<double> MeasureInputUsage(string input, RewriterRewriteOptions? options = default);

    /// <summary>
    /// inputQuota
    /// </summary>
    [Description("@#inputQuota")]
    public extern double InputQuota { get; }

    #region mixin DestroyableModel
    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();
    #endregion
}

/// <summary>
/// CreateMonitor
/// </summary>
[ECMAScript]
[Description("@#CreateMonitor")]
public class CreateMonitor : EventTarget
{
    /// <summary>
    /// ondownloadprogress
    /// </summary>
    [Description("@#ondownloadprogress")]
    public extern EventHandler Ondownloadprogress { get; set; }
}