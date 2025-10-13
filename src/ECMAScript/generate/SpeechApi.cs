namespace ECMAScript;

/// <summary>
/// SpeechRecognitionOptions
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionOptions")]
public record SpeechRecognitionOptions(
    [property: Description("@#langs")]string[]? Langs = default,
	[property: Description("@#processLocally")]bool ProcessLocally = false);

/// <summary>
/// SpeechRecognitionErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionErrorEventInit")]
public record SpeechRecognitionErrorEventInit(
    [property: Description("@#error")]SpeechRecognitionErrorCode? Error = default,
	[property: Description("@#message")]string? Message = default): EventInit;

/// <summary>
/// SpeechRecognitionEventInit
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionEventInit")]
public record SpeechRecognitionEventInit(
    [property: Description("@#resultIndex")]uint ResultIndex = 0,
	[property: Description("@#results")]SpeechRecognitionResultList? Results = default): EventInit;

/// <summary>
/// SpeechSynthesisEventInit
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesisEventInit")]
public record SpeechSynthesisEventInit(
    [property: Description("@#utterance")]SpeechSynthesisUtterance? Utterance = default,
	[property: Description("@#charIndex")]uint CharIndex = 0,
	[property: Description("@#charLength")]uint CharLength = 0,
	[property: Description("@#elapsedTime")]float ElapsedTime = 0f,
	[property: Description("@#name")]string? Name = default): EventInit;

/// <summary>
/// SpeechSynthesisErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesisErrorEventInit")]
public record SpeechSynthesisErrorEventInit(
    [property: Description("@#error")]SpeechSynthesisErrorCode? Error = default): SpeechSynthesisEventInit;

/// <summary>
/// SpeechRecognition
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognition")]
public class SpeechRecognition : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern SpeechRecognition();

    /// <summary>
    /// grammars
    /// </summary>
    [Description("@#grammars")]
    public extern SpeechGrammarList Grammars { get; set; }

    /// <summary>
    /// lang
    /// </summary>
    [Description("@#lang")]
    public extern string Lang { get; set; }

    /// <summary>
    /// continuous
    /// </summary>
    [Description("@#continuous")]
    public extern bool Continuous { get; set; }

    /// <summary>
    /// interimResults
    /// </summary>
    [Description("@#interimResults")]
    public extern bool InterimResults { get; set; }

    /// <summary>
    /// maxAlternatives
    /// </summary>
    [Description("@#maxAlternatives")]
    public extern uint MaxAlternatives { get; set; }

    /// <summary>
    /// processLocally
    /// </summary>
    [Description("@#processLocally")]
    public extern bool ProcessLocally { get; set; }

    /// <summary>
    /// phrases
    /// </summary>
    [Description("@#phrases")]
    public extern SpeechRecognitionPhraseList Phrases { get; set; }

    /// <summary>
    /// start
    /// </summary>
    [Description("@#start")]
    public extern void Start();

    /// <summary>
    /// start
    /// </summary>
    /// <param name="audioTrack">audioTrack</param>
    [Description("@#start")]
    public extern void Start(MediaStreamTrack audioTrack);

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();

    /// <summary>
    /// abort
    /// </summary>
    [Description("@#abort")]
    public extern void Abort();

    /// <summary>
    /// available
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#available")]
    public extern static PromiseResult<AvailabilityStatus> Available(SpeechRecognitionOptions options);

    /// <summary>
    /// install
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#install")]
    public extern static PromiseResult<bool> Install(SpeechRecognitionOptions options);

    /// <summary>
    /// onaudiostart
    /// </summary>
    [Description("@#onaudiostart")]
    public extern EventHandler Onaudiostart { get; set; }

    /// <summary>
    /// onsoundstart
    /// </summary>
    [Description("@#onsoundstart")]
    public extern EventHandler Onsoundstart { get; set; }

    /// <summary>
    /// onspeechstart
    /// </summary>
    [Description("@#onspeechstart")]
    public extern EventHandler Onspeechstart { get; set; }

    /// <summary>
    /// onspeechend
    /// </summary>
    [Description("@#onspeechend")]
    public extern EventHandler Onspeechend { get; set; }

    /// <summary>
    /// onsoundend
    /// </summary>
    [Description("@#onsoundend")]
    public extern EventHandler Onsoundend { get; set; }

    /// <summary>
    /// onaudioend
    /// </summary>
    [Description("@#onaudioend")]
    public extern EventHandler Onaudioend { get; set; }

    /// <summary>
    /// onresult
    /// </summary>
    [Description("@#onresult")]
    public extern EventHandler Onresult { get; set; }

    /// <summary>
    /// onnomatch
    /// </summary>
    [Description("@#onnomatch")]
    public extern EventHandler Onnomatch { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onstart
    /// </summary>
    [Description("@#onstart")]
    public extern EventHandler Onstart { get; set; }

    /// <summary>
    /// onend
    /// </summary>
    [Description("@#onend")]
    public extern EventHandler Onend { get; set; }
}

/// <summary>
/// SpeechRecognitionErrorEvent
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionErrorEvent")]
public class SpeechRecognitionErrorEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern SpeechRecognitionErrorEvent(string type, SpeechRecognitionErrorEventInit eventInitDict);

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern SpeechRecognitionErrorCode Error { get; }

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// SpeechRecognitionAlternative
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionAlternative")]
public class SpeechRecognitionAlternative
{
    /// <summary>
    /// transcript
    /// </summary>
    [Description("@#transcript")]
    public extern string Transcript { get; }

    /// <summary>
    /// confidence
    /// </summary>
    [Description("@#confidence")]
    public extern float Confidence { get; }
}

/// <summary>
/// SpeechRecognitionResult
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionResult")]
public class SpeechRecognitionResult
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern SpeechRecognitionAlternative GetItem(uint index);

    /// <summary>
    /// isFinal
    /// </summary>
    [Description("@#isFinal")]
    public extern bool IsFinal { get; }
}

/// <summary>
/// SpeechRecognitionResultList
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionResultList")]
public class SpeechRecognitionResultList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern SpeechRecognitionResult GetItem(uint index);
}

/// <summary>
/// SpeechRecognitionEvent
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionEvent")]
public class SpeechRecognitionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern SpeechRecognitionEvent(string type, SpeechRecognitionEventInit eventInitDict);

    /// <summary>
    /// resultIndex
    /// </summary>
    [Description("@#resultIndex")]
    public extern uint ResultIndex { get; }

    /// <summary>
    /// results
    /// </summary>
    [Description("@#results")]
    public extern SpeechRecognitionResultList Results { get; }
}

/// <summary>
/// SpeechGrammar
/// </summary>
[ECMAScript]
[Description("@#SpeechGrammar")]
public class SpeechGrammar
{
    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// weight
    /// </summary>
    [Description("@#weight")]
    public extern float Weight { get; set; }
}

/// <summary>
/// SpeechGrammarList
/// </summary>
[ECMAScript]
[Description("@#SpeechGrammarList")]
public class SpeechGrammarList
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern SpeechGrammarList();

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern SpeechGrammar GetItem(uint index);

    /// <summary>
    /// addFromURI
    /// </summary>
    /// <param name="src">src</param>
    /// <param name="weight">weight</param>
    [Description("@#addFromURI")]
    public extern void AddFromURI(string src, float weight = 1.0f);

    /// <summary>
    /// addFromString
    /// </summary>
    /// <param name="string">string</param>
    /// <param name="weight">weight</param>
    [Description("@#addFromString")]
    public extern void AddFromString(string @string, float weight = 1.0f);
}

/// <summary>
/// SpeechRecognitionPhrase
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionPhrase")]
public class SpeechRecognitionPhrase
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="phrase">phrase</param>
    /// <param name="boost">boost</param>
    public extern SpeechRecognitionPhrase(string phrase, float boost);

    /// <summary>
    /// phrase
    /// </summary>
    [Description("@#phrase")]
    public extern string Phrase { get; }

    /// <summary>
    /// boost
    /// </summary>
    [Description("@#boost")]
    public extern float Boost { get; }
}

/// <summary>
/// SpeechRecognitionPhraseList
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionPhraseList")]
public class SpeechRecognitionPhraseList
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="phrases">phrases</param>
    public extern SpeechRecognitionPhraseList(SpeechRecognitionPhrase[] phrases);

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern SpeechRecognitionPhrase GetItem(uint index);

    /// <summary>
    /// addItem
    /// </summary>
    /// <param name="item">item</param>
    [Description("@#addItem")]
    public extern void AddItem(SpeechRecognitionPhrase item);

    /// <summary>
    /// removeItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#removeItem")]
    public extern void RemoveItem(uint index);
}

/// <summary>
/// SpeechSynthesis
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesis")]
public class SpeechSynthesis : EventTarget
{
    /// <summary>
    /// pending
    /// </summary>
    [Description("@#pending")]
    public extern bool Pending { get; }

    /// <summary>
    /// speaking
    /// </summary>
    [Description("@#speaking")]
    public extern bool Speaking { get; }

    /// <summary>
    /// paused
    /// </summary>
    [Description("@#paused")]
    public extern bool Paused { get; }

    /// <summary>
    /// onvoiceschanged
    /// </summary>
    [Description("@#onvoiceschanged")]
    public extern EventHandler Onvoiceschanged { get; set; }

    /// <summary>
    /// speak
    /// </summary>
    /// <param name="utterance">utterance</param>
    [Description("@#speak")]
    public extern void Speak(SpeechSynthesisUtterance utterance);

    /// <summary>
    /// cancel
    /// </summary>
    [Description("@#cancel")]
    public extern void Cancel();

    /// <summary>
    /// pause
    /// </summary>
    [Description("@#pause")]
    public extern void Pause();

    /// <summary>
    /// resume
    /// </summary>
    [Description("@#resume")]
    public extern void Resume();

    /// <summary>
    /// getVoices
    /// </summary>
    [Description("@#getVoices")]
    public extern SpeechSynthesisVoice[] GetVoices();
}

/// <summary>
/// SpeechSynthesisUtterance
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesisUtterance")]
public class SpeechSynthesisUtterance : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="text">text</param>
    public extern SpeechSynthesisUtterance(string text);

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; set; }

    /// <summary>
    /// lang
    /// </summary>
    [Description("@#lang")]
    public extern string Lang { get; set; }

    /// <summary>
    /// voice
    /// </summary>
    [Description("@#voice")]
    public extern SpeechSynthesisVoice? Voice { get; set; }

    /// <summary>
    /// volume
    /// </summary>
    [Description("@#volume")]
    public extern float Volume { get; set; }

    /// <summary>
    /// rate
    /// </summary>
    [Description("@#rate")]
    public extern float Rate { get; set; }

    /// <summary>
    /// pitch
    /// </summary>
    [Description("@#pitch")]
    public extern float Pitch { get; set; }

    /// <summary>
    /// onstart
    /// </summary>
    [Description("@#onstart")]
    public extern EventHandler Onstart { get; set; }

    /// <summary>
    /// onend
    /// </summary>
    [Description("@#onend")]
    public extern EventHandler Onend { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onpause
    /// </summary>
    [Description("@#onpause")]
    public extern EventHandler Onpause { get; set; }

    /// <summary>
    /// onresume
    /// </summary>
    [Description("@#onresume")]
    public extern EventHandler Onresume { get; set; }

    /// <summary>
    /// onmark
    /// </summary>
    [Description("@#onmark")]
    public extern EventHandler Onmark { get; set; }

    /// <summary>
    /// onboundary
    /// </summary>
    [Description("@#onboundary")]
    public extern EventHandler Onboundary { get; set; }
}

/// <summary>
/// SpeechSynthesisEvent
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesisEvent")]
public class SpeechSynthesisEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern SpeechSynthesisEvent(string type, SpeechSynthesisEventInit eventInitDict);

    /// <summary>
    /// utterance
    /// </summary>
    [Description("@#utterance")]
    public extern SpeechSynthesisUtterance Utterance { get; }

    /// <summary>
    /// charIndex
    /// </summary>
    [Description("@#charIndex")]
    public extern uint CharIndex { get; }

    /// <summary>
    /// charLength
    /// </summary>
    [Description("@#charLength")]
    public extern uint CharLength { get; }

    /// <summary>
    /// elapsedTime
    /// </summary>
    [Description("@#elapsedTime")]
    public extern float ElapsedTime { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }
}

/// <summary>
/// SpeechSynthesisErrorEvent
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesisErrorEvent")]
public class SpeechSynthesisErrorEvent(string type, SpeechSynthesisEventInit eventInitDict) : SpeechSynthesisEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern SpeechSynthesisErrorEvent(string type, SpeechSynthesisErrorEventInit eventInitDict);

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern SpeechSynthesisErrorCode Error { get; }
}

/// <summary>
/// SpeechSynthesisVoice
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesisVoice")]
public class SpeechSynthesisVoice
{
    /// <summary>
    /// voiceURI
    /// </summary>
    [Description("@#voiceURI")]
    public extern string VoiceURI { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// lang
    /// </summary>
    [Description("@#lang")]
    public extern string Lang { get; }

    /// <summary>
    /// localService
    /// </summary>
    [Description("@#localService")]
    public extern bool LocalService { get; }

    /// <summary>
    /// default
    /// </summary>
    [Description("@#default")]
    public extern bool Default { get; }
}