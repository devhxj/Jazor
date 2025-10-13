namespace ECMAScript;

/// <summary>
/// DOMException
/// </summary>
[ECMAScript]
[Description("@#DOMException")]
public class DOMException
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="name">name</param>
    public extern DOMException(string message, string name);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }

    /// <summary>
    /// code
    /// </summary>
    [Description("@#code")]
    public extern ushort Code { get; }

    /// <summary>
    /// INDEX_SIZE_ERR
    /// </summary>
    [Description("@#INDEX_SIZE_ERR")]
    public const ushort INDEX_SIZE_ERR = 1;

    /// <summary>
    /// DOMSTRING_SIZE_ERR
    /// </summary>
    [Description("@#DOMSTRING_SIZE_ERR")]
    public const ushort DOMSTRING_SIZE_ERR = 2;

    /// <summary>
    /// HIERARCHY_REQUEST_ERR
    /// </summary>
    [Description("@#HIERARCHY_REQUEST_ERR")]
    public const ushort HIERARCHY_REQUEST_ERR = 3;

    /// <summary>
    /// WRONG_DOCUMENT_ERR
    /// </summary>
    [Description("@#WRONG_DOCUMENT_ERR")]
    public const ushort WRONG_DOCUMENT_ERR = 4;

    /// <summary>
    /// INVALID_CHARACTER_ERR
    /// </summary>
    [Description("@#INVALID_CHARACTER_ERR")]
    public const ushort INVALID_CHARACTER_ERR = 5;

    /// <summary>
    /// NO_DATA_ALLOWED_ERR
    /// </summary>
    [Description("@#NO_DATA_ALLOWED_ERR")]
    public const ushort NO_DATA_ALLOWED_ERR = 6;

    /// <summary>
    /// NO_MODIFICATION_ALLOWED_ERR
    /// </summary>
    [Description("@#NO_MODIFICATION_ALLOWED_ERR")]
    public const ushort NO_MODIFICATION_ALLOWED_ERR = 7;

    /// <summary>
    /// NOT_FOUND_ERR
    /// </summary>
    [Description("@#NOT_FOUND_ERR")]
    public const ushort NOT_FOUND_ERR = 8;

    /// <summary>
    /// NOT_SUPPORTED_ERR
    /// </summary>
    [Description("@#NOT_SUPPORTED_ERR")]
    public const ushort NOT_SUPPORTED_ERR = 9;

    /// <summary>
    /// INUSE_ATTRIBUTE_ERR
    /// </summary>
    [Description("@#INUSE_ATTRIBUTE_ERR")]
    public const ushort INUSE_ATTRIBUTE_ERR = 10;

    /// <summary>
    /// INVALID_STATE_ERR
    /// </summary>
    [Description("@#INVALID_STATE_ERR")]
    public const ushort INVALID_STATE_ERR = 11;

    /// <summary>
    /// SYNTAX_ERR
    /// </summary>
    [Description("@#SYNTAX_ERR")]
    public const ushort SYNTAX_ERR = 12;

    /// <summary>
    /// INVALID_MODIFICATION_ERR
    /// </summary>
    [Description("@#INVALID_MODIFICATION_ERR")]
    public const ushort INVALID_MODIFICATION_ERR = 13;

    /// <summary>
    /// NAMESPACE_ERR
    /// </summary>
    [Description("@#NAMESPACE_ERR")]
    public const ushort NAMESPACE_ERR = 14;

    /// <summary>
    /// INVALID_ACCESS_ERR
    /// </summary>
    [Description("@#INVALID_ACCESS_ERR")]
    public const ushort INVALID_ACCESS_ERR = 15;

    /// <summary>
    /// VALIDATION_ERR
    /// </summary>
    [Description("@#VALIDATION_ERR")]
    public const ushort VALIDATION_ERR = 16;

    /// <summary>
    /// TYPE_MISMATCH_ERR
    /// </summary>
    [Description("@#TYPE_MISMATCH_ERR")]
    public const ushort TYPE_MISMATCH_ERR = 17;

    /// <summary>
    /// SECURITY_ERR
    /// </summary>
    [Description("@#SECURITY_ERR")]
    public const ushort SECURITY_ERR = 18;

    /// <summary>
    /// NETWORK_ERR
    /// </summary>
    [Description("@#NETWORK_ERR")]
    public const ushort NETWORK_ERR = 19;

    /// <summary>
    /// ABORT_ERR
    /// </summary>
    [Description("@#ABORT_ERR")]
    public const ushort ABORT_ERR = 20;

    /// <summary>
    /// URL_MISMATCH_ERR
    /// </summary>
    [Description("@#URL_MISMATCH_ERR")]
    public const ushort URL_MISMATCH_ERR = 21;

    /// <summary>
    /// QUOTA_EXCEEDED_ERR
    /// </summary>
    [Description("@#QUOTA_EXCEEDED_ERR")]
    public const ushort QUOTA_EXCEEDED_ERR = 22;

    /// <summary>
    /// TIMEOUT_ERR
    /// </summary>
    [Description("@#TIMEOUT_ERR")]
    public const ushort TIMEOUT_ERR = 23;

    /// <summary>
    /// INVALID_NODE_TYPE_ERR
    /// </summary>
    [Description("@#INVALID_NODE_TYPE_ERR")]
    public const ushort INVALID_NODE_TYPE_ERR = 24;

    /// <summary>
    /// DATA_CLONE_ERR
    /// </summary>
    [Description("@#DATA_CLONE_ERR")]
    public const ushort DATA_CLONE_ERR = 25;
}