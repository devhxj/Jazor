namespace ECMAScript;

/// <summary>
/// TranslatorCreateCoreOptions
/// </summary>
[ECMAScript]
[Description("@#TranslatorCreateCoreOptions")]
public record TranslatorCreateCoreOptions(
    [property: Description("@#sourceLanguage")]string? SourceLanguage = default,
	[property: Description("@#targetLanguage")]string? TargetLanguage = default);

/// <summary>
/// TranslatorCreateOptions
/// </summary>
[ECMAScript]
[Description("@#TranslatorCreateOptions")]
public record TranslatorCreateOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#monitor")]CreateMonitorCallback? Monitor = default): TranslatorCreateCoreOptions;

/// <summary>
/// TranslatorTranslateOptions
/// </summary>
[ECMAScript]
[Description("@#TranslatorTranslateOptions")]
public record TranslatorTranslateOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// LanguageDetectorCreateCoreOptions
/// </summary>
[ECMAScript]
[Description("@#LanguageDetectorCreateCoreOptions")]
public record LanguageDetectorCreateCoreOptions(
    [property: Description("@#expectedInputLanguages")]string[]? ExpectedInputLanguages = default);

/// <summary>
/// LanguageDetectorCreateOptions
/// </summary>
[ECMAScript]
[Description("@#LanguageDetectorCreateOptions")]
public record LanguageDetectorCreateOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#monitor")]CreateMonitorCallback? Monitor = default): LanguageDetectorCreateCoreOptions;

/// <summary>
/// LanguageDetectorDetectOptions
/// </summary>
[ECMAScript]
[Description("@#LanguageDetectorDetectOptions")]
public record LanguageDetectorDetectOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// LanguageDetectionResult
/// </summary>
[ECMAScript]
[Description("@#LanguageDetectionResult")]
public record LanguageDetectionResult(
    [property: Description("@#detectedLanguage")]string? DetectedLanguage = default,
	[property: Description("@#confidence")]double Confidence = default);

/// <summary>
/// Translator
/// </summary>
[ECMAScript]
[Description("@#Translator")]
public class Translator
{
    /// <summary>
    /// create
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#create")]
    public extern static PromiseResult<Translator> Create(TranslatorCreateOptions options);

    /// <summary>
    /// availability
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#availability")]
    public extern static PromiseResult<Availability> Availability(TranslatorCreateCoreOptions options);

    /// <summary>
    /// translate
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#translate")]
    public extern PromiseResult<string> Translate(string input, TranslatorTranslateOptions? options = default);

    /// <summary>
    /// translateStreaming
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#translateStreaming")]
    public extern ReadableStream TranslateStreaming(string input, TranslatorTranslateOptions? options = default);

    /// <summary>
    /// sourceLanguage
    /// </summary>
    [Description("@#sourceLanguage")]
    public extern string SourceLanguage { get; }

    /// <summary>
    /// targetLanguage
    /// </summary>
    [Description("@#targetLanguage")]
    public extern string TargetLanguage { get; }

    /// <summary>
    /// measureInputUsage
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#measureInputUsage")]
    public extern PromiseResult<double> MeasureInputUsage(string input, TranslatorTranslateOptions? options = default);

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
/// LanguageDetector
/// </summary>
[ECMAScript]
[Description("@#LanguageDetector")]
public class LanguageDetector
{
    /// <summary>
    /// create
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#create")]
    public extern static PromiseResult<LanguageDetector> Create(LanguageDetectorCreateOptions? options = default);

    /// <summary>
    /// availability
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#availability")]
    public extern static PromiseResult<Availability> Availability(LanguageDetectorCreateCoreOptions? options = default);

    /// <summary>
    /// detect
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#detect")]
    public extern PromiseResult<LanguageDetectionResult[]> Detect(string input, LanguageDetectorDetectOptions? options = default);

    /// <summary>
    /// expectedInputLanguages
    /// </summary>
    [Description("@#expectedInputLanguages")]
    public extern FrozenSet<string>? ExpectedInputLanguages { get; }

    /// <summary>
    /// measureInputUsage
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#measureInputUsage")]
    public extern PromiseResult<double> MeasureInputUsage(string input, LanguageDetectorDetectOptions? options = default);

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