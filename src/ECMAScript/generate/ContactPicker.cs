namespace ECMAScript;

/// <summary>
/// ContactInfo
/// </summary>
[ECMAScript]
[Description("@#ContactInfo")]
public record ContactInfo(
    [property: Description("@#address")]ContactAddress[]? Address = default,
	[property: Description("@#email")]string[]? Email = default,
	[property: Description("@#icon")]Blob[]? Icon = default,
	[property: Description("@#name")]string[]? Name = default,
	[property: Description("@#tel")]string[]? Tel = default);

/// <summary>
/// ContactsSelectOptions
/// </summary>
[ECMAScript]
[Description("@#ContactsSelectOptions")]
public record ContactsSelectOptions(
    [property: Description("@#multiple")]bool Multiple = false);

/// <summary>
/// ContactAddress
/// </summary>
[ECMAScript]
[Description("@#ContactAddress")]
public class ContactAddress
{
    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();

    /// <summary>
    /// city
    /// </summary>
    [Description("@#city")]
    public extern string City { get; }

    /// <summary>
    /// country
    /// </summary>
    [Description("@#country")]
    public extern string Country { get; }

    /// <summary>
    /// dependentLocality
    /// </summary>
    [Description("@#dependentLocality")]
    public extern string DependentLocality { get; }

    /// <summary>
    /// organization
    /// </summary>
    [Description("@#organization")]
    public extern string Organization { get; }

    /// <summary>
    /// phone
    /// </summary>
    [Description("@#phone")]
    public extern string Phone { get; }

    /// <summary>
    /// postalCode
    /// </summary>
    [Description("@#postalCode")]
    public extern string PostalCode { get; }

    /// <summary>
    /// recipient
    /// </summary>
    [Description("@#recipient")]
    public extern string Recipient { get; }

    /// <summary>
    /// region
    /// </summary>
    [Description("@#region")]
    public extern string Region { get; }

    /// <summary>
    /// sortingCode
    /// </summary>
    [Description("@#sortingCode")]
    public extern string SortingCode { get; }

    /// <summary>
    /// addressLine
    /// </summary>
    [Description("@#addressLine")]
    public extern FrozenSet<string> AddressLine { get; }
}

/// <summary>
/// ContactsManager
/// </summary>
[ECMAScript]
[Description("@#ContactsManager")]
public class ContactsManager
{
    /// <summary>
    /// getProperties
    /// </summary>
    [Description("@#getProperties")]
    public extern PromiseResult<ContactProperty[]> GetProperties();

    /// <summary>
    /// select
    /// </summary>
    /// <param name="properties">properties</param>
    /// <param name="options">options</param>
    [Description("@#select")]
    public extern PromiseResult<ContactInfo[]> Select(ContactProperty[] properties, ContactsSelectOptions? options = default);
}