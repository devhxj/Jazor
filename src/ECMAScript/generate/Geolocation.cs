namespace ECMAScript;

/// <summary>
/// PositionOptions
/// </summary>
[ECMAScript]
[Description("@#PositionOptions")]
public record PositionOptions(
    [property: Description("@#enableHighAccuracy")]bool EnableHighAccuracy = false,
	[property: Description("@#timeout")]uint Timeout = 0xFFFFFFFF,
	[property: Description("@#maximumAge")]uint MaximumAge = 0);

/// <summary>
/// Geolocation
/// </summary>
[ECMAScript]
[Description("@#Geolocation")]
public class Geolocation
{
    /// <summary>
    /// getCurrentPosition
    /// </summary>
    /// <param name="successCallback">successCallback</param>
    /// <param name="errorCallback">errorCallback</param>
    /// <param name="options">options</param>
    [Description("@#getCurrentPosition")]
    public extern void GetCurrentPosition(PositionCallback successCallback, PositionErrorCallback? errorCallback = null, PositionOptions? options = default);

    /// <summary>
    /// watchPosition
    /// </summary>
    /// <param name="successCallback">successCallback</param>
    /// <param name="errorCallback">errorCallback</param>
    /// <param name="options">options</param>
    [Description("@#watchPosition")]
    public extern int WatchPosition(PositionCallback successCallback, PositionErrorCallback? errorCallback = null, PositionOptions? options = default);

    /// <summary>
    /// clearWatch
    /// </summary>
    /// <param name="watchId">watchId</param>
    [Description("@#clearWatch")]
    public extern void ClearWatch(int watchId);
}

/// <summary>
/// GeolocationPosition
/// </summary>
[ECMAScript]
[Description("@#GeolocationPosition")]
public class GeolocationPosition
{
    /// <summary>
    /// coords
    /// </summary>
    [Description("@#coords")]
    public extern GeolocationCoordinates Coords { get; }

    /// <summary>
    /// timestamp
    /// </summary>
    [Description("@#timestamp")]
    public extern EpochTimeStamp Timestamp { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// GeolocationCoordinates
/// </summary>
[ECMAScript]
[Description("@#GeolocationCoordinates")]
public class GeolocationCoordinates
{
    /// <summary>
    /// accuracy
    /// </summary>
    [Description("@#accuracy")]
    public extern double Accuracy { get; }

    /// <summary>
    /// latitude
    /// </summary>
    [Description("@#latitude")]
    public extern double Latitude { get; }

    /// <summary>
    /// longitude
    /// </summary>
    [Description("@#longitude")]
    public extern double Longitude { get; }

    /// <summary>
    /// altitude
    /// </summary>
    [Description("@#altitude")]
    public extern double? Altitude { get; }

    /// <summary>
    /// altitudeAccuracy
    /// </summary>
    [Description("@#altitudeAccuracy")]
    public extern double? AltitudeAccuracy { get; }

    /// <summary>
    /// heading
    /// </summary>
    [Description("@#heading")]
    public extern double? Heading { get; }

    /// <summary>
    /// speed
    /// </summary>
    [Description("@#speed")]
    public extern double? Speed { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// GeolocationPositionError
/// </summary>
[ECMAScript]
[Description("@#GeolocationPositionError")]
public class GeolocationPositionError
{
    /// <summary>
    /// PERMISSION_DENIED
    /// </summary>
    [Description("@#PERMISSION_DENIED")]
    public const ushort PERMISSION_DENIED = 1;

    /// <summary>
    /// POSITION_UNAVAILABLE
    /// </summary>
    [Description("@#POSITION_UNAVAILABLE")]
    public const ushort POSITION_UNAVAILABLE = 2;

    /// <summary>
    /// TIMEOUT
    /// </summary>
    [Description("@#TIMEOUT")]
    public const ushort TIMEOUT = 3;

    /// <summary>
    /// code
    /// </summary>
    [Description("@#code")]
    public extern ushort Code { get; }

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }
}