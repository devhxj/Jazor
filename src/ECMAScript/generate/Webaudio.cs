namespace ECMAScript;

/// <summary>
/// AudioContextOptions
/// </summary>
[ECMAScript]
[Description("@#AudioContextOptions")]
public record AudioContextOptions(
    [property: Description("@#latencyHint")]Either<AudioContextLatencyCategory, double>? LatencyHint = default,
	[property: Description("@#sampleRate")]float SampleRate = default,
	[property: Description("@#sinkId")]Either<string, AudioSinkOptions>? SinkId = default,
	[property: Description("@#renderSizeHint")]Either<AudioContextRenderSizeCategory, uint>? RenderSizeHint = default);

/// <summary>
/// AudioSinkOptions
/// </summary>
[ECMAScript]
[Description("@#AudioSinkOptions")]
public record AudioSinkOptions(
    [property: Description("@#type")]AudioSinkType? Type = default);

/// <summary>
/// AudioTimestamp
/// </summary>
[ECMAScript]
[Description("@#AudioTimestamp")]
public record AudioTimestamp(
    [property: Description("@#contextTime")]double ContextTime = default,
	[property: Description("@#performanceTime")]double PerformanceTime = default);

/// <summary>
/// OfflineAudioContextOptions
/// </summary>
[ECMAScript]
[Description("@#OfflineAudioContextOptions")]
public record OfflineAudioContextOptions(
    [property: Description("@#numberOfChannels")]uint NumberOfChannels = 1,
	[property: Description("@#length")]uint Length = default,
	[property: Description("@#sampleRate")]float SampleRate = default,
	[property: Description("@#renderSizeHint")]Either<AudioContextRenderSizeCategory, uint>? RenderSizeHint = default);

/// <summary>
/// OfflineAudioCompletionEventInit
/// </summary>
[ECMAScript]
[Description("@#OfflineAudioCompletionEventInit")]
public record OfflineAudioCompletionEventInit(
    [property: Description("@#renderedBuffer")]AudioBuffer? RenderedBuffer = default): EventInit;

/// <summary>
/// AudioBufferOptions
/// </summary>
[ECMAScript]
[Description("@#AudioBufferOptions")]
public record AudioBufferOptions(
    [property: Description("@#numberOfChannels")]uint NumberOfChannels = 1,
	[property: Description("@#length")]uint Length = default,
	[property: Description("@#sampleRate")]float SampleRate = default);

/// <summary>
/// AudioNodeOptions
/// </summary>
[ECMAScript]
[Description("@#AudioNodeOptions")]
public record AudioNodeOptions(
    [property: Description("@#channelCount")]uint ChannelCount = default,
	[property: Description("@#channelCountMode")]ChannelCountMode? ChannelCountMode = default,
	[property: Description("@#channelInterpretation")]ChannelInterpretation? ChannelInterpretation = default);

/// <summary>
/// AnalyserOptions
/// </summary>
[ECMAScript]
[Description("@#AnalyserOptions")]
public record AnalyserOptions(
    [property: Description("@#fftSize")]uint FftSize = 2048,
	[property: Description("@#maxDecibels")]double MaxDecibels = -30d,
	[property: Description("@#minDecibels")]double MinDecibels = -100d,
	[property: Description("@#smoothingTimeConstant")]double SmoothingTimeConstant = 0.8d): AudioNodeOptions;

/// <summary>
/// AudioBufferSourceOptions
/// </summary>
[ECMAScript]
[Description("@#AudioBufferSourceOptions")]
public record AudioBufferSourceOptions(
    [property: Description("@#buffer")]AudioBuffer? Buffer = default,
	[property: Description("@#detune")]float Detune = 0f,
	[property: Description("@#loop")]bool Loop = false,
	[property: Description("@#loopEnd")]double LoopEnd = 0d,
	[property: Description("@#loopStart")]double LoopStart = 0d,
	[property: Description("@#playbackRate")]float PlaybackRate = 1f);

/// <summary>
/// AudioProcessingEventInit
/// </summary>
[ECMAScript]
[Description("@#AudioProcessingEventInit")]
public record AudioProcessingEventInit(
    [property: Description("@#playbackTime")]double PlaybackTime = default,
	[property: Description("@#inputBuffer")]AudioBuffer? InputBuffer = default,
	[property: Description("@#outputBuffer")]AudioBuffer? OutputBuffer = default): EventInit;

/// <summary>
/// BiquadFilterOptions
/// </summary>
[ECMAScript]
[Description("@#BiquadFilterOptions")]
public record BiquadFilterOptions(
    [property: Description("@#type")]BiquadFilterType Type = BiquadFilterType.Lowpass,
	[property: Description("@#Q")]float Q = 1f,
	[property: Description("@#detune")]float Detune = 0f,
	[property: Description("@#frequency")]float Frequency = 350f,
	[property: Description("@#gain")]float Gain = 0f): AudioNodeOptions;

/// <summary>
/// ChannelMergerOptions
/// </summary>
[ECMAScript]
[Description("@#ChannelMergerOptions")]
public record ChannelMergerOptions(
    [property: Description("@#numberOfInputs")]uint NumberOfInputs = 6): AudioNodeOptions;

/// <summary>
/// ChannelSplitterOptions
/// </summary>
[ECMAScript]
[Description("@#ChannelSplitterOptions")]
public record ChannelSplitterOptions(
    [property: Description("@#numberOfOutputs")]uint NumberOfOutputs = 6): AudioNodeOptions;

/// <summary>
/// ConstantSourceOptions
/// </summary>
[ECMAScript]
[Description("@#ConstantSourceOptions")]
public record ConstantSourceOptions(
    [property: Description("@#offset")]float Offset = 1f);

/// <summary>
/// ConvolverOptions
/// </summary>
[ECMAScript]
[Description("@#ConvolverOptions")]
public record ConvolverOptions(
    [property: Description("@#buffer")]AudioBuffer? Buffer = default,
	[property: Description("@#disableNormalization")]bool DisableNormalization = false): AudioNodeOptions;

/// <summary>
/// DelayOptions
/// </summary>
[ECMAScript]
[Description("@#DelayOptions")]
public record DelayOptions(
    [property: Description("@#maxDelayTime")]double MaxDelayTime = 1d,
	[property: Description("@#delayTime")]double DelayTime = 0d): AudioNodeOptions;

/// <summary>
/// DynamicsCompressorOptions
/// </summary>
[ECMAScript]
[Description("@#DynamicsCompressorOptions")]
public record DynamicsCompressorOptions(
    [property: Description("@#attack")]float Attack = 0.003f,
	[property: Description("@#knee")]float Knee = 30f,
	[property: Description("@#ratio")]float Ratio = 12f,
	[property: Description("@#release")]float Release = 0.25f,
	[property: Description("@#threshold")]float Threshold = -24f): AudioNodeOptions;

/// <summary>
/// GainOptions
/// </summary>
[ECMAScript]
[Description("@#GainOptions")]
public record GainOptions(
    [property: Description("@#gain")]float Gain = 1.0f): AudioNodeOptions;

/// <summary>
/// IIRFilterOptions
/// </summary>
[ECMAScript]
[Description("@#IIRFilterOptions")]
public record IIRFilterOptions(
    [property: Description("@#feedforward")]double[]? Feedforward = default,
	[property: Description("@#feedback")]double[]? Feedback = default): AudioNodeOptions;

/// <summary>
/// MediaElementAudioSourceOptions
/// </summary>
[ECMAScript]
[Description("@#MediaElementAudioSourceOptions")]
public record MediaElementAudioSourceOptions(
    [property: Description("@#mediaElement")]HTMLMediaElement? MediaElement = default);

/// <summary>
/// MediaStreamAudioSourceOptions
/// </summary>
[ECMAScript]
[Description("@#MediaStreamAudioSourceOptions")]
public record MediaStreamAudioSourceOptions(
    [property: Description("@#mediaStream")]MediaStream? MediaStream = default);

/// <summary>
/// MediaStreamTrackAudioSourceOptions
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackAudioSourceOptions")]
public record MediaStreamTrackAudioSourceOptions(
    [property: Description("@#mediaStreamTrack")]MediaStreamTrack? MediaStreamTrack = default);

/// <summary>
/// OscillatorOptions
/// </summary>
[ECMAScript]
[Description("@#OscillatorOptions")]
public record OscillatorOptions(
    [property: Description("@#type")]OscillatorType Type = OscillatorType.Sine,
	[property: Description("@#frequency")]float Frequency = 440f,
	[property: Description("@#detune")]float Detune = 0f,
	[property: Description("@#periodicWave")]PeriodicWave? PeriodicWave = default): AudioNodeOptions;

/// <summary>
/// PannerOptions
/// </summary>
[ECMAScript]
[Description("@#PannerOptions")]
public record PannerOptions(
    [property: Description("@#panningModel")]PanningModelType PanningModel = PanningModelType.Equalpower,
	[property: Description("@#distanceModel")]DistanceModelType DistanceModel = DistanceModelType.Inverse,
	[property: Description("@#positionX")]float PositionX = 0f,
	[property: Description("@#positionY")]float PositionY = 0f,
	[property: Description("@#positionZ")]float PositionZ = 0f,
	[property: Description("@#orientationX")]float OrientationX = 1f,
	[property: Description("@#orientationY")]float OrientationY = 0f,
	[property: Description("@#orientationZ")]float OrientationZ = 0f,
	[property: Description("@#refDistance")]double RefDistance = 1d,
	[property: Description("@#maxDistance")]double MaxDistance = 10000d,
	[property: Description("@#rolloffFactor")]double RolloffFactor = 1d,
	[property: Description("@#coneInnerAngle")]double ConeInnerAngle = 360d,
	[property: Description("@#coneOuterAngle")]double ConeOuterAngle = 360d,
	[property: Description("@#coneOuterGain")]double ConeOuterGain = 0d): AudioNodeOptions;

/// <summary>
/// PeriodicWaveConstraints
/// </summary>
[ECMAScript]
[Description("@#PeriodicWaveConstraints")]
public record PeriodicWaveConstraints(
    [property: Description("@#disableNormalization")]bool DisableNormalization = false);

/// <summary>
/// PeriodicWaveOptions
/// </summary>
[ECMAScript]
[Description("@#PeriodicWaveOptions")]
public record PeriodicWaveOptions(
    [property: Description("@#real")]float[]? Real = default,
	[property: Description("@#imag")]float[]? Imag = default): PeriodicWaveConstraints;

/// <summary>
/// StereoPannerOptions
/// </summary>
[ECMAScript]
[Description("@#StereoPannerOptions")]
public record StereoPannerOptions(
    [property: Description("@#pan")]float Pan = 0f): AudioNodeOptions;

/// <summary>
/// WaveShaperOptions
/// </summary>
[ECMAScript]
[Description("@#WaveShaperOptions")]
public record WaveShaperOptions(
    [property: Description("@#curve")]float[]? Curve = default,
	[property: Description("@#oversample")]OverSampleType Oversample = OverSampleType.None): AudioNodeOptions;

/// <summary>
/// AudioWorkletNodeOptions
/// </summary>
[ECMAScript]
[Description("@#AudioWorkletNodeOptions")]
public record AudioWorkletNodeOptions(
    [property: Description("@#numberOfInputs")]uint NumberOfInputs = 1,
	[property: Description("@#numberOfOutputs")]uint NumberOfOutputs = 1,
	[property: Description("@#outputChannelCount")]uint[]? OutputChannelCount = default,
	[property: Description("@#parameterData")]Dictionary<string, double>? ParameterData = default,
	[property: Description("@#processorOptions")]object? ProcessorOptions = default): AudioNodeOptions;

/// <summary>
/// AudioParamDescriptor
/// </summary>
[ECMAScript]
[Description("@#AudioParamDescriptor")]
public record AudioParamDescriptor(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#defaultValue")]float DefaultValue = 0f,
	[property: Description("@#minValue")]float MinValue = -3.4028235e38f,
	[property: Description("@#maxValue")]float MaxValue = 3.4028235e38f,
	[property: Description("@#automationRate")]AutomationRate AutomationRate = AutomationRate.ARate);

/// <summary>
/// BaseAudioContext
/// </summary>
[ECMAScript]
[Description("@#BaseAudioContext")]
public class BaseAudioContext : EventTarget
{
    /// <summary>
    /// destination
    /// </summary>
    [Description("@#destination")]
    public extern AudioDestinationNode Destination { get; }

    /// <summary>
    /// sampleRate
    /// </summary>
    [Description("@#sampleRate")]
    public extern float SampleRate { get; }

    /// <summary>
    /// currentTime
    /// </summary>
    [Description("@#currentTime")]
    public extern double CurrentTime { get; }

    /// <summary>
    /// listener
    /// </summary>
    [Description("@#listener")]
    public extern AudioListener Listener { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern AudioContextState State { get; }

    /// <summary>
    /// renderQuantumSize
    /// </summary>
    [Description("@#renderQuantumSize")]
    public extern uint RenderQuantumSize { get; }

    /// <summary>
    /// audioWorklet
    /// </summary>
    [Description("@#audioWorklet")]
    public extern AudioWorklet AudioWorklet { get; }

    /// <summary>
    /// onstatechange
    /// </summary>
    [Description("@#onstatechange")]
    public extern EventHandler Onstatechange { get; set; }

    /// <summary>
    /// createAnalyser
    /// </summary>
    [Description("@#createAnalyser")]
    public extern AnalyserNode CreateAnalyser();

    /// <summary>
    /// createBiquadFilter
    /// </summary>
    [Description("@#createBiquadFilter")]
    public extern BiquadFilterNode CreateBiquadFilter();

    /// <summary>
    /// createBuffer
    /// </summary>
    /// <param name="numberOfChannels">numberOfChannels</param>
    /// <param name="length">length</param>
    /// <param name="sampleRate">sampleRate</param>
    [Description("@#createBuffer")]
    public extern AudioBuffer CreateBuffer(uint numberOfChannels, uint length, float sampleRate);

    /// <summary>
    /// createBufferSource
    /// </summary>
    [Description("@#createBufferSource")]
    public extern AudioBufferSourceNode CreateBufferSource();

    /// <summary>
    /// createChannelMerger
    /// </summary>
    /// <param name="numberOfInputs">numberOfInputs</param>
    [Description("@#createChannelMerger")]
    public extern ChannelMergerNode CreateChannelMerger(uint numberOfInputs = 6);

    /// <summary>
    /// createChannelSplitter
    /// </summary>
    /// <param name="numberOfOutputs">numberOfOutputs</param>
    [Description("@#createChannelSplitter")]
    public extern ChannelSplitterNode CreateChannelSplitter(uint numberOfOutputs = 6);

    /// <summary>
    /// createConstantSource
    /// </summary>
    [Description("@#createConstantSource")]
    public extern ConstantSourceNode CreateConstantSource();

    /// <summary>
    /// createConvolver
    /// </summary>
    [Description("@#createConvolver")]
    public extern ConvolverNode CreateConvolver();

    /// <summary>
    /// createDelay
    /// </summary>
    /// <param name="maxDelayTime">maxDelayTime</param>
    [Description("@#createDelay")]
    public extern DelayNode CreateDelay(double maxDelayTime = 1.0d);

    /// <summary>
    /// createDynamicsCompressor
    /// </summary>
    [Description("@#createDynamicsCompressor")]
    public extern DynamicsCompressorNode CreateDynamicsCompressor();

    /// <summary>
    /// createGain
    /// </summary>
    [Description("@#createGain")]
    public extern GainNode CreateGain();

    /// <summary>
    /// createIIRFilter
    /// </summary>
    /// <param name="feedforward">feedforward</param>
    /// <param name="feedback">feedback</param>
    [Description("@#createIIRFilter")]
    public extern IIRFilterNode CreateIIRFilter(double[] feedforward, double[] feedback);

    /// <summary>
    /// createOscillator
    /// </summary>
    [Description("@#createOscillator")]
    public extern OscillatorNode CreateOscillator();

    /// <summary>
    /// createPanner
    /// </summary>
    [Description("@#createPanner")]
    public extern PannerNode CreatePanner();

    /// <summary>
    /// createPeriodicWave
    /// </summary>
    /// <param name="real">real</param>
    /// <param name="imag">imag</param>
    /// <param name="constraints">constraints</param>
    [Description("@#createPeriodicWave")]
    public extern PeriodicWave CreatePeriodicWave(float[] real, float[] imag, PeriodicWaveConstraints? constraints = default);

    /// <summary>
    /// createScriptProcessor
    /// </summary>
    /// <param name="bufferSize">bufferSize</param>
    /// <param name="numberOfInputChannels">numberOfInputChannels</param>
    /// <param name="numberOfOutputChannels">numberOfOutputChannels</param>
    [Description("@#createScriptProcessor")]
    public extern ScriptProcessorNode CreateScriptProcessor(uint bufferSize = 0, uint numberOfInputChannels = 2, uint numberOfOutputChannels = 2);

    /// <summary>
    /// createStereoPanner
    /// </summary>
    [Description("@#createStereoPanner")]
    public extern StereoPannerNode CreateStereoPanner();

    /// <summary>
    /// createWaveShaper
    /// </summary>
    [Description("@#createWaveShaper")]
    public extern WaveShaperNode CreateWaveShaper();

    /// <summary>
    /// decodeAudioData
    /// </summary>
    /// <param name="audioData">audioData</param>
    /// <param name="successCallback">successCallback</param>
    /// <param name="errorCallback">errorCallback</param>
    [Description("@#decodeAudioData")]
    public extern PromiseResult<AudioBuffer> DecodeAudioData(ArrayBuffer audioData, DecodeSuccessCallback? successCallback, DecodeErrorCallback? errorCallback);
}

/// <summary>
/// AudioContext
/// </summary>
[ECMAScript]
[Description("@#AudioContext")]
public class AudioContext : BaseAudioContext
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="contextOptions">contextOptions</param>
    public extern AudioContext(AudioContextOptions contextOptions);

    /// <summary>
    /// baseLatency
    /// </summary>
    [Description("@#baseLatency")]
    public extern double BaseLatency { get; }

    /// <summary>
    /// outputLatency
    /// </summary>
    [Description("@#outputLatency")]
    public extern double OutputLatency { get; }

    /// <summary>
    /// sinkId
    /// </summary>
    [Description("@#sinkId")]
    public extern Either<string, AudioSinkInfo> SinkId { get; }

    /// <summary>
    /// onsinkchange
    /// </summary>
    [Description("@#onsinkchange")]
    public extern EventHandler Onsinkchange { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// getOutputTimestamp
    /// </summary>
    [Description("@#getOutputTimestamp")]
    public extern AudioTimestamp GetOutputTimestamp();

    /// <summary>
    /// resume
    /// </summary>
    [Description("@#resume")]
    public extern PromiseResult<object> Resume();

    /// <summary>
    /// suspend
    /// </summary>
    [Description("@#suspend")]
    public extern PromiseResult<object> Suspend();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern PromiseResult<object> Close();

    /// <summary>
    /// setSinkId
    /// </summary>
    /// <param name="sinkId">sinkId</param>
    [Description("@#setSinkId")]
    public extern PromiseResult<object> SetSinkId(Either<string, AudioSinkOptions> sinkId);

    /// <summary>
    /// setSinkId
    /// </summary>
    /// <param name="sinkId">sinkId</param>
    [Description("@#setSinkId")]
    public extern PromiseResult<object> SetSinkId(string sinkId);

    /// <summary>
    /// setSinkId
    /// </summary>
    /// <param name="sinkId">sinkId</param>
    [Description("@#setSinkId")]
    public extern PromiseResult<object> SetSinkId(AudioSinkOptions? sinkId = default);

    /// <summary>
    /// createMediaElementSource
    /// </summary>
    /// <param name="mediaElement">mediaElement</param>
    [Description("@#createMediaElementSource")]
    public extern MediaElementAudioSourceNode CreateMediaElementSource(HTMLMediaElement mediaElement);

    /// <summary>
    /// createMediaStreamSource
    /// </summary>
    /// <param name="mediaStream">mediaStream</param>
    [Description("@#createMediaStreamSource")]
    public extern MediaStreamAudioSourceNode CreateMediaStreamSource(MediaStream mediaStream);

    /// <summary>
    /// createMediaStreamTrackSource
    /// </summary>
    /// <param name="mediaStreamTrack">mediaStreamTrack</param>
    [Description("@#createMediaStreamTrackSource")]
    public extern MediaStreamTrackAudioSourceNode CreateMediaStreamTrackSource(MediaStreamTrack mediaStreamTrack);

    /// <summary>
    /// createMediaStreamDestination
    /// </summary>
    [Description("@#createMediaStreamDestination")]
    public extern MediaStreamAudioDestinationNode CreateMediaStreamDestination();
}

/// <summary>
/// AudioSinkInfo
/// </summary>
[ECMAScript]
[Description("@#AudioSinkInfo")]
public class AudioSinkInfo
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern AudioSinkType Type { get; }
}

/// <summary>
/// OfflineAudioContext
/// </summary>
[ECMAScript]
[Description("@#OfflineAudioContext")]
public class OfflineAudioContext : BaseAudioContext
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="contextOptions">contextOptions</param>
    public extern OfflineAudioContext(OfflineAudioContextOptions contextOptions);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="numberOfChannels">numberOfChannels</param>
    /// <param name="length">length</param>
    /// <param name="sampleRate">sampleRate</param>
    public extern OfflineAudioContext(uint numberOfChannels, uint length, float sampleRate);

    /// <summary>
    /// startRendering
    /// </summary>
    [Description("@#startRendering")]
    public extern PromiseResult<AudioBuffer> StartRendering();

    /// <summary>
    /// resume
    /// </summary>
    [Description("@#resume")]
    public extern PromiseResult<object> Resume();

    /// <summary>
    /// suspend
    /// </summary>
    /// <param name="suspendTime">suspendTime</param>
    [Description("@#suspend")]
    public extern PromiseResult<object> Suspend(double suspendTime);

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// oncomplete
    /// </summary>
    [Description("@#oncomplete")]
    public extern EventHandler Oncomplete { get; set; }
}

/// <summary>
/// OfflineAudioCompletionEvent
/// </summary>
[ECMAScript]
[Description("@#OfflineAudioCompletionEvent")]
public class OfflineAudioCompletionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern OfflineAudioCompletionEvent(string type, OfflineAudioCompletionEventInit eventInitDict);

    /// <summary>
    /// renderedBuffer
    /// </summary>
    [Description("@#renderedBuffer")]
    public extern AudioBuffer RenderedBuffer { get; }
}

/// <summary>
/// AudioBuffer
/// </summary>
[ECMAScript]
[Description("@#AudioBuffer")]
public class AudioBuffer
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern AudioBuffer(AudioBufferOptions options);

    /// <summary>
    /// sampleRate
    /// </summary>
    [Description("@#sampleRate")]
    public extern float SampleRate { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern double Duration { get; }

    /// <summary>
    /// numberOfChannels
    /// </summary>
    [Description("@#numberOfChannels")]
    public extern uint NumberOfChannels { get; }

    /// <summary>
    /// getChannelData
    /// </summary>
    /// <param name="channel">channel</param>
    [Description("@#getChannelData")]
    public extern Float32Array GetChannelData(uint channel);

    /// <summary>
    /// copyFromChannel
    /// </summary>
    /// <param name="destination">destination</param>
    /// <param name="channelNumber">channelNumber</param>
    /// <param name="bufferOffset">bufferOffset</param>
    [Description("@#copyFromChannel")]
    public extern void CopyFromChannel(Float32Array destination, uint channelNumber, uint bufferOffset = 0);

    /// <summary>
    /// copyToChannel
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="channelNumber">channelNumber</param>
    /// <param name="bufferOffset">bufferOffset</param>
    [Description("@#copyToChannel")]
    public extern void CopyToChannel(Float32Array source, uint channelNumber, uint bufferOffset = 0);
}

/// <summary>
/// AudioNode
/// </summary>
[ECMAScript]
[Description("@#AudioNode")]
public class AudioNode : EventTarget
{
    /// <summary>
    /// connect
    /// </summary>
    /// <param name="destinationNode">destinationNode</param>
    /// <param name="output">output</param>
    /// <param name="input">input</param>
    [Description("@#connect")]
    public extern AudioNode Connect(AudioNode destinationNode, uint output = 0, uint input = 0);

    /// <summary>
    /// connect
    /// </summary>
    /// <param name="destinationParam">destinationParam</param>
    /// <param name="output">output</param>
    [Description("@#connect")]
    public extern void Connect(AudioParam destinationParam, uint output = 0);

    /// <summary>
    /// disconnect
    /// </summary>
    [Description("@#disconnect")]
    public extern void Disconnect();

    /// <summary>
    /// disconnect
    /// </summary>
    /// <param name="output">output</param>
    [Description("@#disconnect")]
    public extern void Disconnect(uint output);

    /// <summary>
    /// disconnect
    /// </summary>
    /// <param name="destinationNode">destinationNode</param>
    [Description("@#disconnect")]
    public extern void Disconnect(AudioNode destinationNode);

    /// <summary>
    /// disconnect
    /// </summary>
    /// <param name="destinationNode">destinationNode</param>
    /// <param name="output">output</param>
    [Description("@#disconnect")]
    public extern void Disconnect(AudioNode destinationNode, uint output);

    /// <summary>
    /// disconnect
    /// </summary>
    /// <param name="destinationNode">destinationNode</param>
    /// <param name="output">output</param>
    /// <param name="input">input</param>
    [Description("@#disconnect")]
    public extern void Disconnect(AudioNode destinationNode, uint output, uint input);

    /// <summary>
    /// disconnect
    /// </summary>
    /// <param name="destinationParam">destinationParam</param>
    [Description("@#disconnect")]
    public extern void Disconnect(AudioParam destinationParam);

    /// <summary>
    /// disconnect
    /// </summary>
    /// <param name="destinationParam">destinationParam</param>
    /// <param name="output">output</param>
    [Description("@#disconnect")]
    public extern void Disconnect(AudioParam destinationParam, uint output);

    /// <summary>
    /// context
    /// </summary>
    [Description("@#context")]
    public extern BaseAudioContext Context { get; }

    /// <summary>
    /// numberOfInputs
    /// </summary>
    [Description("@#numberOfInputs")]
    public extern uint NumberOfInputs { get; }

    /// <summary>
    /// numberOfOutputs
    /// </summary>
    [Description("@#numberOfOutputs")]
    public extern uint NumberOfOutputs { get; }

    /// <summary>
    /// channelCount
    /// </summary>
    [Description("@#channelCount")]
    public extern uint ChannelCount { get; set; }

    /// <summary>
    /// channelCountMode
    /// </summary>
    [Description("@#channelCountMode")]
    public extern ChannelCountMode ChannelCountMode { get; set; }

    /// <summary>
    /// channelInterpretation
    /// </summary>
    [Description("@#channelInterpretation")]
    public extern ChannelInterpretation ChannelInterpretation { get; set; }
}

/// <summary>
/// AudioParam
/// </summary>
[ECMAScript]
[Description("@#AudioParam")]
public class AudioParam
{
    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern float Value { get; set; }

    /// <summary>
    /// automationRate
    /// </summary>
    [Description("@#automationRate")]
    public extern AutomationRate AutomationRate { get; set; }

    /// <summary>
    /// defaultValue
    /// </summary>
    [Description("@#defaultValue")]
    public extern float DefaultValue { get; }

    /// <summary>
    /// minValue
    /// </summary>
    [Description("@#minValue")]
    public extern float MinValue { get; }

    /// <summary>
    /// maxValue
    /// </summary>
    [Description("@#maxValue")]
    public extern float MaxValue { get; }

    /// <summary>
    /// setValueAtTime
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="startTime">startTime</param>
    [Description("@#setValueAtTime")]
    public extern AudioParam SetValueAtTime(float value, double startTime);

    /// <summary>
    /// linearRampToValueAtTime
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="endTime">endTime</param>
    [Description("@#linearRampToValueAtTime")]
    public extern AudioParam LinearRampToValueAtTime(float value, double endTime);

    /// <summary>
    /// exponentialRampToValueAtTime
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="endTime">endTime</param>
    [Description("@#exponentialRampToValueAtTime")]
    public extern AudioParam ExponentialRampToValueAtTime(float value, double endTime);

    /// <summary>
    /// setTargetAtTime
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="startTime">startTime</param>
    /// <param name="timeConstant">timeConstant</param>
    [Description("@#setTargetAtTime")]
    public extern AudioParam SetTargetAtTime(float target, double startTime, float timeConstant);

    /// <summary>
    /// setValueCurveAtTime
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="startTime">startTime</param>
    /// <param name="duration">duration</param>
    [Description("@#setValueCurveAtTime")]
    public extern AudioParam SetValueCurveAtTime(float[] values, double startTime, double duration);

    /// <summary>
    /// cancelScheduledValues
    /// </summary>
    /// <param name="cancelTime">cancelTime</param>
    [Description("@#cancelScheduledValues")]
    public extern AudioParam CancelScheduledValues(double cancelTime);

    /// <summary>
    /// cancelAndHoldAtTime
    /// </summary>
    /// <param name="cancelTime">cancelTime</param>
    [Description("@#cancelAndHoldAtTime")]
    public extern AudioParam CancelAndHoldAtTime(double cancelTime);
}

/// <summary>
/// AudioScheduledSourceNode
/// </summary>
[ECMAScript]
[Description("@#AudioScheduledSourceNode")]
public class AudioScheduledSourceNode : AudioNode
{
    /// <summary>
    /// onended
    /// </summary>
    [Description("@#onended")]
    public extern EventHandler Onended { get; set; }

    /// <summary>
    /// start
    /// </summary>
    /// <param name="when">when</param>
    [Description("@#start")]
    public extern void Start(double when = 0d);

    /// <summary>
    /// stop
    /// </summary>
    /// <param name="when">when</param>
    [Description("@#stop")]
    public extern void Stop(double when = 0d);
}

/// <summary>
/// AnalyserNode
/// </summary>
[ECMAScript]
[Description("@#AnalyserNode")]
public class AnalyserNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern AnalyserNode(BaseAudioContext context, AnalyserOptions options);

    /// <summary>
    /// getFloatFrequencyData
    /// </summary>
    /// <param name="array">array</param>
    [Description("@#getFloatFrequencyData")]
    public extern void GetFloatFrequencyData(Float32Array array);

    /// <summary>
    /// getByteFrequencyData
    /// </summary>
    /// <param name="array">array</param>
    [Description("@#getByteFrequencyData")]
    public extern void GetByteFrequencyData(Uint8Array array);

    /// <summary>
    /// getFloatTimeDomainData
    /// </summary>
    /// <param name="array">array</param>
    [Description("@#getFloatTimeDomainData")]
    public extern void GetFloatTimeDomainData(Float32Array array);

    /// <summary>
    /// getByteTimeDomainData
    /// </summary>
    /// <param name="array">array</param>
    [Description("@#getByteTimeDomainData")]
    public extern void GetByteTimeDomainData(Uint8Array array);

    /// <summary>
    /// fftSize
    /// </summary>
    [Description("@#fftSize")]
    public extern uint FftSize { get; set; }

    /// <summary>
    /// frequencyBinCount
    /// </summary>
    [Description("@#frequencyBinCount")]
    public extern uint FrequencyBinCount { get; }

    /// <summary>
    /// minDecibels
    /// </summary>
    [Description("@#minDecibels")]
    public extern double MinDecibels { get; set; }

    /// <summary>
    /// maxDecibels
    /// </summary>
    [Description("@#maxDecibels")]
    public extern double MaxDecibels { get; set; }

    /// <summary>
    /// smoothingTimeConstant
    /// </summary>
    [Description("@#smoothingTimeConstant")]
    public extern double SmoothingTimeConstant { get; set; }
}

/// <summary>
/// AudioBufferSourceNode
/// </summary>
[ECMAScript]
[Description("@#AudioBufferSourceNode")]
public class AudioBufferSourceNode : AudioScheduledSourceNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern AudioBufferSourceNode(BaseAudioContext context, AudioBufferSourceOptions options);

    /// <summary>
    /// buffer
    /// </summary>
    [Description("@#buffer")]
    public extern AudioBuffer? Buffer { get; set; }

    /// <summary>
    /// playbackRate
    /// </summary>
    [Description("@#playbackRate")]
    public extern AudioParam PlaybackRate { get; }

    /// <summary>
    /// detune
    /// </summary>
    [Description("@#detune")]
    public extern AudioParam Detune { get; }

    /// <summary>
    /// loop
    /// </summary>
    [Description("@#loop")]
    public extern bool Loop { get; set; }

    /// <summary>
    /// loopStart
    /// </summary>
    [Description("@#loopStart")]
    public extern double LoopStart { get; set; }

    /// <summary>
    /// loopEnd
    /// </summary>
    [Description("@#loopEnd")]
    public extern double LoopEnd { get; set; }

    /// <summary>
    /// start
    /// </summary>
    /// <param name="when">when</param>
    /// <param name="offset">offset</param>
    /// <param name="duration">duration</param>
    [Description("@#start")]
    public extern void Start(double when = 0d, double? offset = default, double? duration = default);
}

/// <summary>
/// AudioDestinationNode
/// </summary>
[ECMAScript]
[Description("@#AudioDestinationNode")]
public class AudioDestinationNode : AudioNode
{
    /// <summary>
    /// maxChannelCount
    /// </summary>
    [Description("@#maxChannelCount")]
    public extern uint MaxChannelCount { get; }
}

/// <summary>
/// AudioListener
/// </summary>
[ECMAScript]
[Description("@#AudioListener")]
public class AudioListener
{
    /// <summary>
    /// positionX
    /// </summary>
    [Description("@#positionX")]
    public extern AudioParam PositionX { get; }

    /// <summary>
    /// positionY
    /// </summary>
    [Description("@#positionY")]
    public extern AudioParam PositionY { get; }

    /// <summary>
    /// positionZ
    /// </summary>
    [Description("@#positionZ")]
    public extern AudioParam PositionZ { get; }

    /// <summary>
    /// forwardX
    /// </summary>
    [Description("@#forwardX")]
    public extern AudioParam ForwardX { get; }

    /// <summary>
    /// forwardY
    /// </summary>
    [Description("@#forwardY")]
    public extern AudioParam ForwardY { get; }

    /// <summary>
    /// forwardZ
    /// </summary>
    [Description("@#forwardZ")]
    public extern AudioParam ForwardZ { get; }

    /// <summary>
    /// upX
    /// </summary>
    [Description("@#upX")]
    public extern AudioParam UpX { get; }

    /// <summary>
    /// upY
    /// </summary>
    [Description("@#upY")]
    public extern AudioParam UpY { get; }

    /// <summary>
    /// upZ
    /// </summary>
    [Description("@#upZ")]
    public extern AudioParam UpZ { get; }

    /// <summary>
    /// setPosition
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    [Description("@#setPosition")]
    public extern void SetPosition(float x, float y, float z);

    /// <summary>
    /// setOrientation
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="xUp">xUp</param>
    /// <param name="yUp">yUp</param>
    /// <param name="zUp">zUp</param>
    [Description("@#setOrientation")]
    public extern void SetOrientation(float x, float y, float z, float xUp, float yUp, float zUp);
}

/// <summary>
/// AudioProcessingEvent
/// </summary>
[ECMAScript]
[Description("@#AudioProcessingEvent")]
public class AudioProcessingEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern AudioProcessingEvent(string type, AudioProcessingEventInit eventInitDict);

    /// <summary>
    /// playbackTime
    /// </summary>
    [Description("@#playbackTime")]
    public extern double PlaybackTime { get; }

    /// <summary>
    /// inputBuffer
    /// </summary>
    [Description("@#inputBuffer")]
    public extern AudioBuffer InputBuffer { get; }

    /// <summary>
    /// outputBuffer
    /// </summary>
    [Description("@#outputBuffer")]
    public extern AudioBuffer OutputBuffer { get; }
}

/// <summary>
/// BiquadFilterNode
/// </summary>
[ECMAScript]
[Description("@#BiquadFilterNode")]
public class BiquadFilterNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern BiquadFilterNode(BaseAudioContext context, BiquadFilterOptions options);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern BiquadFilterType Type { get; set; }

    /// <summary>
    /// frequency
    /// </summary>
    [Description("@#frequency")]
    public extern AudioParam Frequency { get; }

    /// <summary>
    /// detune
    /// </summary>
    [Description("@#detune")]
    public extern AudioParam Detune { get; }

    /// <summary>
    /// Q
    /// </summary>
    [Description("@#Q")]
    public extern AudioParam Q { get; }

    /// <summary>
    /// gain
    /// </summary>
    [Description("@#gain")]
    public extern AudioParam Gain { get; }

    /// <summary>
    /// getFrequencyResponse
    /// </summary>
    /// <param name="frequencyHz">frequencyHz</param>
    /// <param name="magResponse">magResponse</param>
    /// <param name="phaseResponse">phaseResponse</param>
    [Description("@#getFrequencyResponse")]
    public extern void GetFrequencyResponse(Float32Array frequencyHz, Float32Array magResponse, Float32Array phaseResponse);
}

/// <summary>
/// ChannelMergerNode
/// </summary>
[ECMAScript]
[Description("@#ChannelMergerNode")]
public class ChannelMergerNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern ChannelMergerNode(BaseAudioContext context, ChannelMergerOptions options);
}

/// <summary>
/// ChannelSplitterNode
/// </summary>
[ECMAScript]
[Description("@#ChannelSplitterNode")]
public class ChannelSplitterNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern ChannelSplitterNode(BaseAudioContext context, ChannelSplitterOptions options);
}

/// <summary>
/// ConstantSourceNode
/// </summary>
[ECMAScript]
[Description("@#ConstantSourceNode")]
public class ConstantSourceNode : AudioScheduledSourceNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern ConstantSourceNode(BaseAudioContext context, ConstantSourceOptions options);

    /// <summary>
    /// offset
    /// </summary>
    [Description("@#offset")]
    public extern AudioParam Offset { get; }
}

/// <summary>
/// ConvolverNode
/// </summary>
[ECMAScript]
[Description("@#ConvolverNode")]
public class ConvolverNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern ConvolverNode(BaseAudioContext context, ConvolverOptions options);

    /// <summary>
    /// buffer
    /// </summary>
    [Description("@#buffer")]
    public extern AudioBuffer? Buffer { get; set; }

    /// <summary>
    /// normalize
    /// </summary>
    [Description("@#normalize")]
    public extern bool Normalize { get; set; }
}

/// <summary>
/// DelayNode
/// </summary>
[ECMAScript]
[Description("@#DelayNode")]
public class DelayNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern DelayNode(BaseAudioContext context, DelayOptions options);

    /// <summary>
    /// delayTime
    /// </summary>
    [Description("@#delayTime")]
    public extern AudioParam DelayTime { get; }
}

/// <summary>
/// DynamicsCompressorNode
/// </summary>
[ECMAScript]
[Description("@#DynamicsCompressorNode")]
public class DynamicsCompressorNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern DynamicsCompressorNode(BaseAudioContext context, DynamicsCompressorOptions options);

    /// <summary>
    /// threshold
    /// </summary>
    [Description("@#threshold")]
    public extern AudioParam Threshold { get; }

    /// <summary>
    /// knee
    /// </summary>
    [Description("@#knee")]
    public extern AudioParam Knee { get; }

    /// <summary>
    /// ratio
    /// </summary>
    [Description("@#ratio")]
    public extern AudioParam Ratio { get; }

    /// <summary>
    /// reduction
    /// </summary>
    [Description("@#reduction")]
    public extern float Reduction { get; }

    /// <summary>
    /// attack
    /// </summary>
    [Description("@#attack")]
    public extern AudioParam Attack { get; }

    /// <summary>
    /// release
    /// </summary>
    [Description("@#release")]
    public extern AudioParam Release { get; }
}

/// <summary>
/// GainNode
/// </summary>
[ECMAScript]
[Description("@#GainNode")]
public class GainNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern GainNode(BaseAudioContext context, GainOptions options);

    /// <summary>
    /// gain
    /// </summary>
    [Description("@#gain")]
    public extern AudioParam Gain { get; }
}

/// <summary>
/// IIRFilterNode
/// </summary>
[ECMAScript]
[Description("@#IIRFilterNode")]
public class IIRFilterNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern IIRFilterNode(BaseAudioContext context, IIRFilterOptions options);

    /// <summary>
    /// getFrequencyResponse
    /// </summary>
    /// <param name="frequencyHz">frequencyHz</param>
    /// <param name="magResponse">magResponse</param>
    /// <param name="phaseResponse">phaseResponse</param>
    [Description("@#getFrequencyResponse")]
    public extern void GetFrequencyResponse(Float32Array frequencyHz, Float32Array magResponse, Float32Array phaseResponse);
}

/// <summary>
/// MediaElementAudioSourceNode
/// </summary>
[ECMAScript]
[Description("@#MediaElementAudioSourceNode")]
public class MediaElementAudioSourceNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern MediaElementAudioSourceNode(AudioContext context, MediaElementAudioSourceOptions options);

    /// <summary>
    /// mediaElement
    /// </summary>
    [Description("@#mediaElement")]
    public extern HTMLMediaElement MediaElement { get; }
}

/// <summary>
/// MediaStreamAudioDestinationNode
/// </summary>
[ECMAScript]
[Description("@#MediaStreamAudioDestinationNode")]
public class MediaStreamAudioDestinationNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern MediaStreamAudioDestinationNode(AudioContext context, AudioNodeOptions options);

    /// <summary>
    /// stream
    /// </summary>
    [Description("@#stream")]
    public extern MediaStream Stream { get; }
}

/// <summary>
/// MediaStreamAudioSourceNode
/// </summary>
[ECMAScript]
[Description("@#MediaStreamAudioSourceNode")]
public class MediaStreamAudioSourceNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern MediaStreamAudioSourceNode(AudioContext context, MediaStreamAudioSourceOptions options);

    /// <summary>
    /// mediaStream
    /// </summary>
    [Description("@#mediaStream")]
    public extern MediaStream MediaStream { get; }
}

/// <summary>
/// MediaStreamTrackAudioSourceNode
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackAudioSourceNode")]
public class MediaStreamTrackAudioSourceNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern MediaStreamTrackAudioSourceNode(AudioContext context, MediaStreamTrackAudioSourceOptions options);
}

/// <summary>
/// OscillatorNode
/// </summary>
[ECMAScript]
[Description("@#OscillatorNode")]
public class OscillatorNode : AudioScheduledSourceNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern OscillatorNode(BaseAudioContext context, OscillatorOptions options);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern OscillatorType Type { get; set; }

    /// <summary>
    /// frequency
    /// </summary>
    [Description("@#frequency")]
    public extern AudioParam Frequency { get; }

    /// <summary>
    /// detune
    /// </summary>
    [Description("@#detune")]
    public extern AudioParam Detune { get; }

    /// <summary>
    /// setPeriodicWave
    /// </summary>
    /// <param name="periodicWave">periodicWave</param>
    [Description("@#setPeriodicWave")]
    public extern void SetPeriodicWave(PeriodicWave periodicWave);
}

/// <summary>
/// PannerNode
/// </summary>
[ECMAScript]
[Description("@#PannerNode")]
public class PannerNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern PannerNode(BaseAudioContext context, PannerOptions options);

    /// <summary>
    /// panningModel
    /// </summary>
    [Description("@#panningModel")]
    public extern PanningModelType PanningModel { get; set; }

    /// <summary>
    /// positionX
    /// </summary>
    [Description("@#positionX")]
    public extern AudioParam PositionX { get; }

    /// <summary>
    /// positionY
    /// </summary>
    [Description("@#positionY")]
    public extern AudioParam PositionY { get; }

    /// <summary>
    /// positionZ
    /// </summary>
    [Description("@#positionZ")]
    public extern AudioParam PositionZ { get; }

    /// <summary>
    /// orientationX
    /// </summary>
    [Description("@#orientationX")]
    public extern AudioParam OrientationX { get; }

    /// <summary>
    /// orientationY
    /// </summary>
    [Description("@#orientationY")]
    public extern AudioParam OrientationY { get; }

    /// <summary>
    /// orientationZ
    /// </summary>
    [Description("@#orientationZ")]
    public extern AudioParam OrientationZ { get; }

    /// <summary>
    /// distanceModel
    /// </summary>
    [Description("@#distanceModel")]
    public extern DistanceModelType DistanceModel { get; set; }

    /// <summary>
    /// refDistance
    /// </summary>
    [Description("@#refDistance")]
    public extern double RefDistance { get; set; }

    /// <summary>
    /// maxDistance
    /// </summary>
    [Description("@#maxDistance")]
    public extern double MaxDistance { get; set; }

    /// <summary>
    /// rolloffFactor
    /// </summary>
    [Description("@#rolloffFactor")]
    public extern double RolloffFactor { get; set; }

    /// <summary>
    /// coneInnerAngle
    /// </summary>
    [Description("@#coneInnerAngle")]
    public extern double ConeInnerAngle { get; set; }

    /// <summary>
    /// coneOuterAngle
    /// </summary>
    [Description("@#coneOuterAngle")]
    public extern double ConeOuterAngle { get; set; }

    /// <summary>
    /// coneOuterGain
    /// </summary>
    [Description("@#coneOuterGain")]
    public extern double ConeOuterGain { get; set; }

    /// <summary>
    /// setPosition
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    [Description("@#setPosition")]
    public extern void SetPosition(float x, float y, float z);

    /// <summary>
    /// setOrientation
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    [Description("@#setOrientation")]
    public extern void SetOrientation(float x, float y, float z);
}

/// <summary>
/// PeriodicWave
/// </summary>
[ECMAScript]
[Description("@#PeriodicWave")]
public class PeriodicWave
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern PeriodicWave(BaseAudioContext context, PeriodicWaveOptions options);
}

/// <summary>
/// ScriptProcessorNode
/// </summary>
[ECMAScript]
[Description("@#ScriptProcessorNode")]
public class ScriptProcessorNode : AudioNode
{
    /// <summary>
    /// onaudioprocess
    /// </summary>
    [Description("@#onaudioprocess")]
    public extern EventHandler Onaudioprocess { get; set; }

    /// <summary>
    /// bufferSize
    /// </summary>
    [Description("@#bufferSize")]
    public extern int BufferSize { get; }
}

/// <summary>
/// StereoPannerNode
/// </summary>
[ECMAScript]
[Description("@#StereoPannerNode")]
public class StereoPannerNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern StereoPannerNode(BaseAudioContext context, StereoPannerOptions options);

    /// <summary>
    /// pan
    /// </summary>
    [Description("@#pan")]
    public extern AudioParam Pan { get; }
}

/// <summary>
/// WaveShaperNode
/// </summary>
[ECMAScript]
[Description("@#WaveShaperNode")]
public class WaveShaperNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="options">options</param>
    public extern WaveShaperNode(BaseAudioContext context, WaveShaperOptions options);

    /// <summary>
    /// curve
    /// </summary>
    [Description("@#curve")]
    public extern Float32Array? Curve { get; set; }

    /// <summary>
    /// oversample
    /// </summary>
    [Description("@#oversample")]
    public extern OverSampleType Oversample { get; set; }
}

/// <summary>
/// AudioWorklet
/// </summary>
[ECMAScript]
[Description("@#AudioWorklet")]
public class AudioWorklet : Worklet
{
    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern MessagePort Port { get; }
}

/// <summary>
/// AudioWorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#AudioWorkletGlobalScope")]
public class AudioWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// registerProcessor
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="processorCtor">processorCtor</param>
    [Description("@#registerProcessor")]
    public extern void RegisterProcessor(string name, AudioWorkletProcessorConstructor processorCtor);

    /// <summary>
    /// currentFrame
    /// </summary>
    [Description("@#currentFrame")]
    public extern ulong CurrentFrame { get; }

    /// <summary>
    /// currentTime
    /// </summary>
    [Description("@#currentTime")]
    public extern double CurrentTime { get; }

    /// <summary>
    /// sampleRate
    /// </summary>
    [Description("@#sampleRate")]
    public extern float SampleRate { get; }

    /// <summary>
    /// renderQuantumSize
    /// </summary>
    [Description("@#renderQuantumSize")]
    public extern uint RenderQuantumSize { get; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern MessagePort Port { get; }
}

/// <summary>
/// AudioParamMap
/// </summary>
[ECMAScript]
[Description("@#AudioParamMap")]
public class AudioParamMap : IDictionary<string, AudioParam>
{
    #region Dictionary
    extern AudioParam IDictionary<string, AudioParam>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, AudioParam>.Keys { get; }
    extern ICollection<AudioParam> IDictionary<string, AudioParam>.Values { get; }
    extern int ICollection<KeyValuePair<string, AudioParam>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, AudioParam>>.IsReadOnly { get; }
    extern void IDictionary<string, AudioParam>.Add(string key, AudioParam value);
    extern void ICollection<KeyValuePair<string, AudioParam>>.Add(KeyValuePair<string, AudioParam> item);
    extern void ICollection<KeyValuePair<string, AudioParam>>.Clear();
    extern bool ICollection<KeyValuePair<string, AudioParam>>.Contains(KeyValuePair<string, AudioParam> item);
    extern bool IDictionary<string, AudioParam>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, AudioParam>>.CopyTo(KeyValuePair<string, AudioParam>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, AudioParam>> IEnumerable<KeyValuePair<string, AudioParam>>.GetEnumerator();
    extern bool IDictionary<string, AudioParam>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, AudioParam>>.Remove(KeyValuePair<string, AudioParam> item);
    extern bool IDictionary<string, AudioParam>.TryGetValue(string key, [MaybeNullWhen(false)] out AudioParam value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// AudioWorkletNode
/// </summary>
[ECMAScript]
[Description("@#AudioWorkletNode")]
public class AudioWorkletNode : AudioNode
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="context">context</param>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    public extern AudioWorkletNode(BaseAudioContext context, string name, AudioWorkletNodeOptions options);

    /// <summary>
    /// parameters
    /// </summary>
    [Description("@#parameters")]
    public extern AudioParamMap Parameters { get; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern MessagePort Port { get; }

    /// <summary>
    /// onprocessorerror
    /// </summary>
    [Description("@#onprocessorerror")]
    public extern EventHandler Onprocessorerror { get; set; }
}

/// <summary>
/// AudioWorkletProcessor
/// </summary>
[ECMAScript]
[Description("@#AudioWorkletProcessor")]
public class AudioWorkletProcessor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern AudioWorkletProcessor();

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern MessagePort Port { get; }
}