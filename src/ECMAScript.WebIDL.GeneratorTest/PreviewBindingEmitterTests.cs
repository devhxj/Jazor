using System.Text.Json;
using Basic.Reference.Assemblies;
using ECMAScript.WebIDL.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ECMAScript.WebIDL.GeneratorTest;

[TestClass]
public sealed class PreviewBindingEmitterTests
{
    [TestMethod]
    public async Task EmitAsync_EnumValues_PreservesWebIdlWireTokens()
    {
        var files = await EmitGeneratedFilesAsync(
            Enum("RequestCredentials", """
                [
                  { "type": "enum-value", "value": "same-origin" },
                  { "type": "enum-value", "value": "no-store" },
                  { "type": "enum-value", "value": "include" }
                ]
                """));

        var enums = files["Enums.cs"].Replace("\r\n", "\n", StringComparison.Ordinal);
        StringAssert.Contains(enums, "[Description(\"@#same-origin\")]\n    SameOrigin = 0");
        StringAssert.Contains(enums, "[Description(\"@#no-store\")]\n    NoStore = 1");
        StringAssert.Contains(enums, "[Description(\"@#include\")]\n    Include = 2");
    }

    [TestMethod]
    public async Task EmitAsync_FileInterface_UsesJazorFileAuthoringTypeAndPreservesFileAbi()
    {
        var files = await EmitGeneratedFilesAsync(
            Interface("File", """
                [
                  {
                    "type": "constructor",
                    "arguments": [],
                    "special": ""
                  },
                  {
                    "type": "attribute",
                    "name": "file",
                    "idlType": { "idlType": "File" },
                    "readonly": true,
                    "special": ""
                  }
                ]
                """),
            Interface("FileChild", "[]", inheritance: "File"),
            Callback("FileConsumer", """
                {
                  "idlType": { "idlType": "undefined" },
                  "arguments": [
                    { "name": "file", "idlType": { "idlType": "File" }, "optional": false, "variadic": false }
                  ]
                }
                """),
            Dictionary("FileReference", """
                [
                  {
                    "type": "field",
                    "name": "value",
                    "idlType": {
                      "union": true,
                      "idlType": [
                        { "idlType": "File" },
                        { "idlType": "DOMString" }
                      ]
                    }
                  }
                ]
                """));

        StringAssert.Contains(files["Interfaces.cs"], "[Description(\"@#File\")]\r\npublic class JazorFile");
        StringAssert.Contains(files["Interfaces.cs"], "public extern JazorFile();");
        StringAssert.Contains(files["Interfaces.cs"], "public extern JazorFile File { get; }");
        StringAssert.Contains(files["Interfaces.cs"], "public class FileChild : JazorFile");
        StringAssert.Contains(files["Callbacks.cs"], "public delegate void FileConsumer(JazorFile file);");
        StringAssert.Contains(files["Dictionaries.cs"], "FileReferenceValue? Value = default");
        StringAssert.Contains(files["Unions.cs"], "public readonly union FileReferenceValue(JazorFile, string)");
        StringAssert.Contains(files["Unions.cs"], "public JazorFile? AsFile => Value is JazorFile value ? value : default(JazorFile?);");
        Assert.IsFalse(files.Values.Any(static file => file.Contains("public class File\r\n", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task EmitAsync_WebCryptoBigIntegerTypedef_DoesNotCapturePrimitiveBigInt()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("BigInteger", """
                { "idlType": "Uint8Array" }
                """),
            Typedef("CryptoKeyID", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "unsigned long long" },
                    { "idlType": "bigint" }
                  ]
                }
                """));

        StringAssert.Contains(files["GlobalUsings.cs"], "global using BigInteger = ECMAScript.Uint8Array;");
        StringAssert.Contains(files["Unions.cs"], "public readonly union CryptoKeyID(Number, BigInt)");
        StringAssert.Contains(files["Unions.cs"], "AsBigInt => Value is BigInt value");
        Assert.IsFalse(files["Unions.cs"].Contains("System.Numerics.BigInteger", StringComparison.Ordinal));

        var diagnostics = CompileGeneratedFiles(
            files,
            """
            namespace ECMAScript;

            public class Uint8Array;
            public readonly struct Number;
            public abstract class BigInt;
            """);

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
    }

    [TestMethod]
    public async Task EmitAsync_WebIdlIntegerPrimitives_UseWidthContractsAndRuntimeHostTypes()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("Signed64", """
                { "idlType": "long long" }
                """),
            Typedef("ArbitraryInteger", """
                { "idlType": "bigint" }
                """),
            Interface("WebIdlIntegerWidths", """
                [
                  {
                    "type": "attribute",
                    "name": "longValue",
                    "idlType": { "idlType": "long" },
                    "readonly": true,
                    "special": ""
                  },
                  {
                    "type": "attribute",
                    "name": "unsignedLongValue",
                    "idlType": { "idlType": "unsigned long" },
                    "readonly": true,
                    "special": ""
                  },
                  {
                    "type": "attribute",
                    "name": "longLongValue",
                    "idlType": { "idlType": "long long" },
                    "readonly": true,
                    "special": ""
                  },
                  {
                    "type": "attribute",
                    "name": "unsignedLongLongValue",
                    "idlType": { "idlType": "unsigned long long" },
                    "readonly": true,
                    "special": ""
                  },
                  {
                    "type": "attribute",
                    "name": "bigIntValue",
                    "idlType": { "idlType": "bigint" },
                    "readonly": true,
                    "special": ""
                  },
                  {
                    "type": "const",
                    "name": "signedLimit",
                    "idlType": { "idlType": "long long" },
                    "value": { "type": "number", "value": "-1" }
                  },
                  {
                    "type": "const",
                    "name": "signedLimitAlias",
                    "idlType": { "idlType": "Signed64" },
                    "value": { "type": "number", "value": "-1" }
                  },
                  {
                    "type": "operation",
                    "name": "setMinimum",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      {
                        "name": "value",
                        "idlType": { "idlType": "long long" },
                        "default": { "type": "number", "value": "1" },
                        "optional": true,
                        "variadic": false
                      }
                    ],
                    "special": ""
                  }
                ]
                """),
            Dictionary("WebIdlIntegerDefaults", """
                [
                  {
                    "type": "field",
                    "name": "minimum",
                    "idlType": { "idlType": "long long" },
                    "default": { "type": "number", "value": "1" }
                  }
                ]
                """));

        var interfaces = files["Interfaces.cs"];
        StringAssert.Contains(interfaces, "public extern int LongValue { get; }");
        StringAssert.Contains(interfaces, "public extern uint UnsignedLongValue { get; }");
        StringAssert.Contains(interfaces, "public extern Number LongLongValue { get; }");
        StringAssert.Contains(interfaces, "public extern Number UnsignedLongLongValue { get; }");
        StringAssert.Contains(interfaces, "public extern BigInt BigIntValue { get; }");
        StringAssert.Contains(interfaces, "public static extern Number SignedLimit { get; }");
        StringAssert.Contains(interfaces, "public static extern Signed64 SignedLimitAlias { get; }");
        Assert.IsFalse(interfaces.Contains("public const Number", StringComparison.Ordinal));
        StringAssert.Contains(interfaces, "public extern void SetMinimum(Number? value = default);");
        StringAssert.Contains(files["Dictionaries.cs"], "Number? Minimum = default");
        StringAssert.Contains(files["GlobalUsings.cs"], "global using Signed64 = ECMAScript.Number;");
        StringAssert.Contains(files["GlobalUsings.cs"], "global using ArbitraryInteger = ECMAScript.BigInt;");
    }

    [TestMethod]
    public async Task EmitAsync_HttpByteStrings_EmitStringContracts()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("HeadersInit", """
                {
                  "union": true,
                  "idlType": [
                    {
                      "generic": "sequence",
                      "idlType": [
                        {
                          "generic": "sequence",
                          "idlType": [ { "idlType": "ByteString" } ]
                        }
                      ]
                    },
                    {
                      "generic": "record",
                      "idlType": [
                        { "idlType": "ByteString" },
                        { "idlType": "ByteString" }
                      ]
                    }
                  ]
                }
                """),
            Dictionary("RequestInit", """
                [
                  { "type": "field", "name": "method", "idlType": { "idlType": "ByteString" } },
                  { "type": "field", "name": "headers", "idlType": { "idlType": "HeadersInit" } }
                ]
                """),
            Interface("Headers", """
                [
                  {
                    "type": "operation",
                    "name": "append",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      { "name": "name", "idlType": { "idlType": "ByteString" }, "optional": false, "variadic": false },
                      { "name": "value", "idlType": { "idlType": "ByteString" }, "optional": false, "variadic": false }
                    ],
                    "special": ""
                  }
                ]
                """));

        StringAssert.Contains(files["Dictionaries.cs"], "[property: Description(\"@#method\")]string? Method = default");
        StringAssert.Contains(files["Unions.cs"], "public readonly union HeadersInit(string[][], Dictionary<string, string>) : IEnumerable<string[]>");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void Append(string name, string value);");
    }

    [TestMethod]
    public async Task EmitAsync_EnumDefaultRemovedFromLatestSpec_UsesCSharpDefault()
    {
        var files = await EmitGeneratedFilesAsync(
            Enum("LanguageModelSamplingMode", """
                [
                  { "type": "enum-value", "value": "balanced" }
                ]
                """),
            Dictionary("LanguageModelCreateCoreOptions", """
                [
                  {
                    "type": "field",
                    "name": "samplingMode",
                    "idlType": { "idlType": "LanguageModelSamplingMode" },
                    "default": { "type": "string", "value": "default" }
                  }
                ]
                """));

        StringAssert.Contains(
            files["Dictionaries.cs"],
            "LanguageModelSamplingMode SamplingMode = default");
        Assert.IsFalse(
            files["Dictionaries.cs"].Contains("LanguageModelSamplingMode.Default", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_TypedefUnionWithSequenceBranch_EmitsNamedWrapper()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("ConstrainDOMString", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "DOMString" },
                    { "generic": "sequence", "idlType": [ { "idlType": "DOMString" } ] },
                    { "idlType": "ConstrainDOMStringParameters" }
                  ]
                }
                """));

        StringAssert.Contains(files["Unions.cs"], "public readonly union ConstrainDOMString(string, string[], ConstrainDOMStringParameters) : IEnumerable<string>");
        StringAssert.Contains(files["Unions.cs"], "[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringCollectionBuilder), nameof(ConstrainDOMStringCollectionBuilder.Create))]");
        StringAssert.Contains(files["Unions.cs"], "public string? AsString => Value is string value ? value : default(string?);");
        StringAssert.Contains(files["Unions.cs"], "public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);");
        StringAssert.Contains(files["Unions.cs"], "public static implicit operator ConstrainDOMString(string[] value)");
        StringAssert.Contains(files["Unions.cs"], "public static class ConstrainDOMStringCollectionBuilder");
    }

    [TestMethod]
    public async Task EmitAsync_TypedefUnionWithSequenceBranch_AllowsCollectionExpressionAssignment()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("ConstrainDOMString", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "DOMString" },
                    { "generic": "sequence", "idlType": [ { "idlType": "DOMString" } ] },
                    { "idlType": "ConstrainDOMStringParameters" }
                  ]
                }
                """));

        var diagnostics = CompileGeneratedFiles(
            files,
            """
            namespace ECMAScript;

            public record ConstrainDOMStringParameters;
            """,
            """
            using ECMAScript;

            public static class CollectionExpressionConsumer
            {
                public static ConstrainDOMString Build()
                {
                    ConstrainDOMString value = ["a", "b"];
                    return value;
                }
            }
            """);

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
    }

    [TestMethod]
    public async Task EmitAsync_TypedefPromiseOfUnion_EmitsNamedNestedWrapper()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("ClipboardItemData", """
                {
                  "generic": "Promise",
                  "idlType": [
                    {
                      "union": true,
                      "idlType": [
                        { "idlType": "DOMString" },
                        { "idlType": "Blob" }
                      ]
                    }
                  ]
                }
                """));

        StringAssert.Contains(files["GlobalUsings.cs"], "global using ClipboardItemData = ECMAScript.PromiseResult<ECMAScript.ClipboardItemDataValue>;");
        StringAssert.Contains(files["Unions.cs"], "public readonly union ClipboardItemDataValue(string, Blob)");
    }

    [TestMethod]
    public async Task EmitAsync_DictionaryUnionProperty_UsesNamedWrapperType()
    {
        var files = await EmitGeneratedFilesAsync(
            Dictionary("IntersectionObserverInit", """
                [
                  {
                    "type": "field",
                    "name": "root",
                    "idlType": {
                      "union": true,
                      "idlType": [
                        { "idlType": "Element" },
                        { "idlType": "Document" }
                      ]
                    }
                  },
                  {
                    "type": "field",
                    "name": "threshold",
                    "idlType": {
                      "union": true,
                      "idlType": [
                        { "idlType": "double" },
                        { "generic": "sequence", "idlType": [ { "idlType": "double" } ] }
                      ]
                    }
                  }
                ]
                """));

        StringAssert.Contains(files["Dictionaries.cs"], "[property: Description(\"@#threshold\")]IntersectionObserverInitThreshold? Threshold = default");
        StringAssert.Contains(files["Unions.cs"], "public readonly union IntersectionObserverInitThreshold(double, double[]) : IEnumerable<double>");
    }

    [TestMethod]
    public async Task EmitAsync_ConstructorUnionParameter_UsesNamedWrapperType()
    {
        var files = await EmitGeneratedFilesAsync(
            Interface("URLSearchParams", """
                [
                  {
                    "type": "constructor",
                    "arguments": [
                      {
                        "name": "init",
                        "idlType": {
                          "union": true,
                          "idlType": [
                            {
                              "generic": "sequence",
                              "idlType": [
                                {
                                  "generic": "sequence",
                                  "idlType": [ { "idlType": "DOMString" } ]
                                }
                              ]
                            },
                            {
                              "generic": "record",
                              "idlType": [
                                { "idlType": "DOMString" },
                                { "idlType": "DOMString" }
                              ]
                            },
                            { "idlType": "DOMString" }
                          ]
                        }
                      }
                    ]
                  }
                ]
                """));

        StringAssert.Contains(files["Interfaces.cs"], "public extern URLSearchParams(URLSearchParamsInit init);");
        StringAssert.Contains(files["Unions.cs"], "public readonly union URLSearchParamsInit(string[][], Dictionary<string, string>, string) : IEnumerable<string[]>");
        StringAssert.Contains(files["Unions.cs"], "public static implicit operator URLSearchParamsInit(string[][] value)");
    }

    [TestMethod]
    public async Task EmitAsync_OperationUnionParameterNameConflict_UsesValueSuffix()
    {
        var files = await EmitGeneratedFilesAsync(
            Dictionary("EventTargetAddEventListenerOptions", "[]"),
            Interface("EventTarget", """
                [
                  {
                    "type": "operation",
                    "name": "addEventListener",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      {
                        "name": "type",
                        "idlType": { "idlType": "DOMString" },
                        "optional": false,
                        "variadic": false
                      },
                      {
                        "name": "options",
                        "idlType": {
                          "union": true,
                          "idlType": [
                            { "idlType": "EventTargetAddEventListenerOptions" },
                            { "idlType": "boolean" }
                          ]
                        },
                        "optional": true,
                        "variadic": false
                      }
                    ],
                    "special": ""
                  }
                ]
                """));

        StringAssert.Contains(files["Interfaces.cs"], "public extern void AddEventListener(string type, EventTargetAddEventListenerOptionsValue? options = default);");
        StringAssert.Contains(files["Unions.cs"], "public readonly union EventTargetAddEventListenerOptionsValue(EventTargetAddEventListenerOptions, bool)");
    }

    [TestMethod]
    public async Task EmitAsync_InterfaceAndObjectTypedefUnions_EmitNamedWrappers()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("BufferSource", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "ArrayBufferView" },
                    { "idlType": "ArrayBuffer" }
                  ]
                }
                """),
            Typedef("AlgorithmIdentifier", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "object" },
                    { "idlType": "DOMString" }
                  ]
                }
                """));

        StringAssert.Contains(files["Unions.cs"], "public readonly struct BufferSource");
        StringAssert.Contains(files["Unions.cs"], "public readonly struct AlgorithmIdentifier");
        Assert.IsFalse(files["Unions.cs"].Contains("[System.Runtime.CompilerServices.Union]", StringComparison.Ordinal));
        Assert.IsFalse(files["Unions.cs"].Contains(": System.Runtime.CompilerServices.IUnion", StringComparison.Ordinal));
        StringAssert.Contains(files["Unions.cs"], "private BufferSource(IArrayBufferView value)");
        StringAssert.Contains(files["Unions.cs"], "private AlgorithmIdentifier(object value)");
        StringAssert.Contains(files["Unions.cs"], "public static BufferSource FromIArrayBufferView(IArrayBufferView value)");
        Assert.IsFalse(files["Unions.cs"].Contains("public static implicit operator BufferSource(IArrayBufferView value)", StringComparison.Ordinal));
        StringAssert.Contains(files["Unions.cs"], "public static implicit operator BufferSource(Uint8Array value)");
        StringAssert.Contains(files["Unions.cs"], "public static AlgorithmIdentifier FromObject(object value)");
        Assert.IsFalse(files["Unions.cs"].Contains("public static implicit operator AlgorithmIdentifier(object value)", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_DerivedAndBaseUnionBranches_UseTaggedFallback()
    {
        var files = await EmitGeneratedFilesAsync(
            Interface("CSSStyleValue", "[]"),
            Interface("CSSColorValue", "[]", inheritance: "CSSStyleValue"),
            Interface("AnimationEffect", "[]"),
            Typedef("CSSColorValueParseResult", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "CSSColorValue" },
                    { "idlType": "CSSStyleValue" }
                  ]
                }
                """),
            Typedef("AnimationEffects", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "AnimationEffect" },
                    {
                      "generic": "sequence",
                      "idlType": [ { "idlType": "AnimationEffect" } ]
                    }
                  ]
                }
                """));

        StringAssert.Contains(files["Interfaces.cs"], "public class CSSColorValue : CSSStyleValue");
        StringAssert.Contains(files["Unions.cs"], "public readonly struct CSSColorValueParseResult : System.Runtime.CompilerServices.IUnion");
        StringAssert.Contains(files["Unions.cs"], "public CSSColorValue? AsCSSColorValue => _kind == 1 ? _value1 : default;");
        Assert.IsFalse(files["Unions.cs"].Contains("public readonly union CSSColorValueParseResult(", StringComparison.Ordinal));
        StringAssert.Contains(files["Unions.cs"], "public readonly union AnimationEffects(AnimationEffect, AnimationEffect[])");
    }

    [TestMethod]
    public async Task EmitAsync_InterfaceUnionBranch_EmitsForwardingImplicitOperatorsForConcreteBufferTypes()
    {
        var files = await EmitGeneratedFilesAsync(
            Dictionary("PushSubscriptionOptionsInit", """
                [
                  {
                    "type": "field",
                    "name": "applicationServerKey",
                    "idlType": {
                      "union": true,
                      "idlType": [
                        { "idlType": "IBufferSource" },
                        { "idlType": "DOMString" }
                      ]
                    }
                  }
                ]
                """));

        StringAssert.Contains(files["Dictionaries.cs"], "[property: Description(\"@#applicationServerKey\")]PushSubscriptionOptionsInitApplicationServerKey? ApplicationServerKey = default");
        StringAssert.Contains(files["Unions.cs"], "public static PushSubscriptionOptionsInitApplicationServerKey FromIBufferSource(IBufferSource value)");
        Assert.IsFalse(files["Unions.cs"].Contains("public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(IBufferSource value)", StringComparison.Ordinal));
        StringAssert.Contains(files["Unions.cs"], "public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint8Array value)");
        StringAssert.Contains(files["Unions.cs"], "public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(ArrayBuffer value)");
    }

    [TestMethod]
    public async Task EmitAsync_CallbackInterfaceUnion_UsesLiteralBranchInsteadOfSelfAlias()
    {
        var files = await EmitGeneratedFilesAsync(
            CallbackInterface("EventListener", """
                [
                  {
                    "type": "operation",
                    "name": "handleEvent",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      { "name": "event", "idlType": { "idlType": "Event" } }
                    ]
                  }
                ]
                """));

        StringAssert.Contains(files["GlobalUsings.cs"], "global using EventListener = ECMAScript.EventListenerValue;");
        StringAssert.Contains(files["Callbacks.cs"], "public sealed class EventListenerLiteral");
        StringAssert.Contains(files["Unions.cs"], "public readonly union EventListenerValue(EventListenerLiteral, HandleEventCallback)");
        StringAssert.Contains(files["Unions.cs"], "public EventListenerLiteral? AsEventListenerLiteral => Value is EventListenerLiteral value ? value : default(EventListenerLiteral?);");
        Assert.IsFalse(files["Unions.cs"].Contains("private readonly EventListener? _value1;", StringComparison.Ordinal));
        Assert.IsFalse(files["Unions.cs"].Contains("public static implicit operator EventListenerValue(EventListener value)", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_UnionTailParameter_EmitsNamedWrapperAndBranchOverloads()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("BufferSource", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "ArrayBufferView" },
                    { "idlType": "ArrayBuffer" }
                  ]
                }
                """),
            Interface("WebSocket", """
                [
                  {
                    "type": "operation",
                    "name": "send",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      {
                        "name": "data",
                        "idlType": {
                          "union": true,
                          "idlType": [
                            { "idlType": "IBufferSource" },
                            { "idlType": "Blob" },
                            { "idlType": "DOMString" }
                          ]
                        },
                        "optional": false,
                        "variadic": false
                      }
                    ],
                    "special": ""
                  }
                ]
                """));

        StringAssert.Contains(files["Interfaces.cs"], "public extern void Send(WebSocketSendData data);");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void Send(IBufferSource data);");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void Send(Blob data);");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void Send(string data);");
        StringAssert.Contains(files["Unions.cs"], "public readonly struct WebSocketSendData");
    }

    [TestMethod]
    public async Task EmitAsync_RepeatedNestedUnionShape_ReusesCanonicalInnerUnionNameInsteadOfGeneratingValueValue2()
    {
        var files = await EmitGeneratedFilesAsync(
            Typedef("RoundRectRadiiValue", """
                {
                  "union": true,
                  "idlType": [
                    { "idlType": "double" },
                    { "idlType": "DOMPointInit" },
                    {
                      "generic": "sequence",
                      "idlType": [
                        {
                          "union": true,
                          "idlType": [
                            { "idlType": "double" },
                            { "idlType": "DOMPointInit" }
                          ]
                        }
                      ]
                    }
                  ]
                }
                """),
            Interface("CanvasRenderingContext2D", """
                [
                  {
                    "type": "operation",
                    "name": "roundRect",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      { "name": "x", "idlType": { "idlType": "double" }, "optional": false, "variadic": false },
                      { "name": "y", "idlType": { "idlType": "double" }, "optional": false, "variadic": false },
                      { "name": "w", "idlType": { "idlType": "double" }, "optional": false, "variadic": false },
                      { "name": "h", "idlType": { "idlType": "double" }, "optional": false, "variadic": false },
                      {
                        "name": "radii",
                        "idlType": {
                          "union": true,
                          "idlType": [
                            { "idlType": "double" },
                            { "idlType": "DOMPointInit" },
                            {
                              "generic": "sequence",
                              "idlType": [
                                {
                                  "union": true,
                                  "idlType": [
                                    { "idlType": "double" },
                                    { "idlType": "DOMPointInit" }
                                  ]
                                }
                              ]
                            }
                          ]
                        },
                        "optional": true,
                        "variadic": false
                      }
                    ],
                    "special": ""
                  }
                ]
                """));

        StringAssert.Contains(files["Interfaces.cs"], "public extern void RoundRect(double x, double y, double w, double h, RoundRectRadii[] radii);");
        Assert.IsFalse(files["Interfaces.cs"].Contains("RoundRectRadiiValueValue2", StringComparison.Ordinal));
        Assert.IsFalse(files["Interfaces.cs"].Contains("RoundRectRadiiValueValue3", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_SameShapeDifferentRequestedNames_DoesNotReuseNonValueAlias()
    {
        var files = await EmitGeneratedFilesAsync(
            Interface("ElementInternals", """
                [
                  {
                    "type": "operation",
                    "name": "setFormValue",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      {
                        "name": "value",
                        "idlType": {
                          "union": true,
                          "idlType": [
                            { "idlType": "File" },
                            { "idlType": "USVString" },
                            { "idlType": "FormData" }
                          ]
                        },
                        "optional": false,
                        "variadic": false
                      },
                      {
                        "name": "state",
                        "idlType": {
                          "union": true,
                          "idlType": [
                            { "idlType": "File" },
                            { "idlType": "USVString" },
                            { "idlType": "FormData" }
                          ]
                        },
                        "optional": true,
                        "variadic": false
                      }
                    ],
                    "special": ""
                  }
                ]
                """));

        StringAssert.Contains(files["Interfaces.cs"], "public extern void SetFormValue(ElementInternalsSetFormValue value, ElementInternalsSetFormValueState? state = default);");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void SetFormValue(ElementInternalsSetFormValue value, JazorFile state);");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void SetFormValue(ElementInternalsSetFormValue value, string state);");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void SetFormValue(ElementInternalsSetFormValue value, FormData state);");
        StringAssert.Contains(files["Unions.cs"], "public readonly union ElementInternalsSetFormValue(JazorFile, string, FormData)");
        StringAssert.Contains(files["Unions.cs"], "public readonly union ElementInternalsSetFormValueState(JazorFile, string, FormData)");
        Assert.IsFalse(files["Interfaces.cs"].Contains("ElementInternalsSetFormValue? state", StringComparison.Ordinal));
        Assert.IsFalse(files["Interfaces.cs"].Contains("public extern void SetFormValue(SetFormValue", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_FormDataOptionalConstructor_EmitsDefaultableSignature()
    {
        var files = await EmitGeneratedFilesAsync(
            Interface("FormData", """
                [
                  {
                    "type": "constructor",
                    "arguments": [
                      {
                        "name": "form",
                        "idlType": { "idlType": "HTMLFormElement" },
                        "optional": true,
                        "variadic": false
                      },
                      {
                        "name": "submitter",
                        "idlType": { "idlType": "HTMLElement", "nullable": true },
                        "default": { "type": "null" },
                        "optional": true,
                        "variadic": false
                      }
                    ]
                  }
                ]
                """));

        StringAssert.Contains(files["Interfaces.cs"], "public extern FormData(HTMLFormElement? form = default, HTMLElement? submitter = default);");
    }

    [TestMethod]
    public async Task EmitAsync_InheritedMethodWithDifferentReturnType_UsesNewModifier()
    {
        var output = await EmitInterfacesAsync(
            Interface("MediaStreamTrack", """
                [
                  { "type": "operation", "name": "clone", "arguments": [], "idlType": { "idlType": "MediaStreamTrack" } }
                ]
                """),
            Interface("BrowserCaptureMediaStreamTrack", """
                [
                  { "type": "operation", "name": "clone", "arguments": [], "idlType": { "idlType": "BrowserCaptureMediaStreamTrack" } }
                ]
                """, inheritance: "MediaStreamTrack"));
		Assert.Contains("public new extern BrowserCaptureMediaStreamTrack Clone();", output);
    }

    [TestMethod]
    public async Task EmitAsync_InheritedMethodWithSameSignature_SkipsDuplicate()
    {
        var output = await EmitInterfacesAsync(
            Interface("ParentType", """
                [
                  { "type": "operation", "name": "close", "arguments": [], "idlType": { "idlType": "void" } }
                ]
                """),
            Interface("ChildType", """
                [
                  { "type": "operation", "name": "close", "arguments": [], "idlType": { "idlType": "void" } }
                ]
                """, inheritance: "ParentType"));

        Assert.AreEqual(1, CountOccurrences(output, "public extern void Close();"));
    }

    [TestMethod]
    public async Task EmitAsync_ParentWithoutParameterlessConstructor_UsesPrimaryConstructorBaseForwarding()
    {
        var output = await EmitInterfacesAsync(
            Interface("Event", """
                [
                  {
                    "type": "constructor",
                    "arguments": [
                      { "name": "type", "idlType": { "idlType": "DOMString" } },
                      { "name": "eventInitDict", "idlType": { "idlType": "EventInit" } }
                    ]
                  }
                ]
                """),
            Interface("AudioRenderCapacityEvent", """
                [
                  {
                    "type": "constructor",
                    "arguments": [
                      { "name": "type", "idlType": { "idlType": "DOMString" } },
                      { "name": "eventInitDict", "idlType": { "idlType": "AudioRenderCapacityEventInit" } }
                    ]
                  }
                ]
                """, inheritance: "Event"));
		Assert.Contains("public class AudioRenderCapacityEvent(string type, EventInit eventInitDict) : JazorEvent(type, eventInitDict)", output);
    }

    [TestMethod]
    public async Task EmitAsync_MixinSetterCompletesGetterIndexer()
    {
        var output = await EmitInterfacesAsync(
            Interface("NamedCollection", """
                [
                  {
                    "type": "operation",
                    "special": "getter",
                    "arguments": [
                      { "name": "name", "idlType": { "idlType": "DOMString" } }
                    ],
                    "idlType": { "idlType": "DOMString" }
                  }
                ]
                """),
            InterfaceMixin("NamedCollectionWritable", """
                [
                  {
                    "type": "operation",
                    "special": "setter",
                    "arguments": [
                      { "name": "name", "idlType": { "idlType": "DOMString" } },
                      { "name": "value", "idlType": { "idlType": "DOMString" } }
                    ]
                  }
                ]
                """),
            Includes("NamedCollection", "NamedCollectionWritable"));

        StringAssert.Contains(output, "public extern string this[string name] { get; set; }");
    }

    [TestMethod]
    public async Task EmitAsync_W3cDocumentation_UsesSourceAuthoredProseExamplesAndSpecificationAnchors()
    {
        var output = await EmitInterfacesAsync(
            Interface("AbortController", """
                [
                  {
                    "type": "constructor",
                    "arguments": []
                  },
                  {
                    "type": "attribute",
                    "name": "signal",
                    "idlType": { "idlType": "AbortSignal" },
                    "readonly": true,
                    "special": ""
                  },
                  {
                    "type": "operation",
                    "name": "abort",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      { "name": "reason", "idlType": { "idlType": "object" }, "default": null, "optional": true, "variadic": false }
                    ],
                    "special": ""
                  }
                ]
                """,
                documentation: Documentation(
                    "https://dom.spec.whatwg.org/#abortcontroller",
                    "DOM Standard",
                    "3.1 Interface AbortController",
                    "A controller that can stop a request when its lifetime ends.",
                    "controller = new AbortController()"),
                memberDocumentation:
                [
                    new WebIdlMemberDocumentation(
                        0,
                        Documentation(
                            "https://dom.spec.whatwg.org/#dom-abortcontroller-abortcontroller",
                            "DOM Standard",
                            "3.1 Interface AbortController")),
                    new WebIdlMemberDocumentation(
                        1,
                        Documentation(
                            "https://dom.spec.whatwg.org/#dom-abortcontroller-signal",
                            "DOM Standard",
                            "3.1 Interface AbortController")),
                    new WebIdlMemberDocumentation(
                        2,
                        Documentation(
                            "https://dom.spec.whatwg.org/#dom-abortcontroller-abort",
                            "DOM Standard",
                            "3.1 Interface AbortController"),
                        [
                            new WebIdlArgumentDocumentation(
                                0,
                                Documentation(
                                    "https://dom.spec.whatwg.org/#dom-abortcontroller-abort-reason-reason",
                                    "DOM Standard",
                                    "3.1 Interface AbortController"))
                        ])
                ]));

        StringAssert.Contains(output, $"/// <summary>{Environment.NewLine}/// A controller that can stop a request when its lifetime ends.{Environment.NewLine}/// </summary>{Environment.NewLine}/// <remarks>{Environment.NewLine}/// <see href=\"https://dom.spec.whatwg.org/#abortcontroller\">DOM Standard: 3.1 Interface AbortController</see>{Environment.NewLine}/// </remarks>");
        StringAssert.Contains(output, $"/// <example>{Environment.NewLine}/// <code>controller = new AbortController()</code>{Environment.NewLine}/// </example>{Environment.NewLine}[ECMAScript]");
        StringAssert.Contains(output, $"    /// <summary>{Environment.NewLine}    /// <see href=\"https://dom.spec.whatwg.org/#dom-abortcontroller-signal\">DOM Standard: 3.1 Interface AbortController</see>{Environment.NewLine}    /// </summary>{Environment.NewLine}    [Description(\"@#signal\")]");
        StringAssert.Contains(output, $"    /// <param name=\"reason\"><see href=\"https://dom.spec.whatwg.org/#dom-abortcontroller-abort-reason-reason\">DOM Standard: 3.1 Interface AbortController</see></param>{Environment.NewLine}    [Description(\"@#abort\")]");
        Assert.IsFalse(output.Contains("Represents the", StringComparison.Ordinal));
        Assert.IsFalse(output.Contains("Invokes the", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_UnmatchedDeclarations_DoNotInventDocumentation()
    {
        var output = await EmitInterfacesAsync(
            Interface("UnmatchedPlatformType", """
                [
                  {
                    "type": "operation",
                    "name": "run",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [],
                    "special": ""
                  }
                ]
                """));

        Assert.IsFalse(output.Contains("/// <summary>", StringComparison.Ordinal));
        Assert.IsFalse(output.Contains("/// <param", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_NamespaceMergesPartialsIntoStaticContainer()
    {
        var output = await EmitNamespacesAsync(
            Namespace("CSS", """
                [
                  { "type": "attribute", "name": "animationWorklet", "idlType": { "idlType": "Worklet" }, "readonly": true, "special": "" }
                ]
                """, namespaceName: "CSS", partial: true),
            Namespace("CSS", """
                [
                  {
                    "type": "operation",
                    "name": "supports",
                    "idlType": { "idlType": "boolean" },
                    "arguments": [
                      { "name": "property", "idlType": { "idlType": "CSSOMString" }, "optional": false, "variadic": false },
                      { "name": "value", "idlType": { "idlType": "CSSOMString" }, "optional": false, "variadic": false }
                    ],
                    "special": ""
                  }
                ]
                """, namespaceName: "CSS", partial: true));
		Assert.Contains("public static partial class CSS", output);
        StringAssert.Contains(output, "public static extern Worklet AnimationWorklet { get; }");
        StringAssert.Contains(output, "public static extern bool Supports(string property, string value);");
    }

    [TestMethod]
    public async Task EmitAsync_NamespaceVariadicAndExplicitDefaults_ArePreserved()
    {
        var output = await EmitNamespacesAsync(
            Namespace("testUtils", """
                [
                  {
                    "type": "operation",
                    "name": "assert",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      { "name": "condition", "idlType": { "idlType": "boolean" }, "default": { "type": "boolean", "value": false }, "optional": true, "variadic": false },
                      { "name": "data", "idlType": { "idlType": "any" }, "optional": false, "variadic": true }
                    ],
                    "special": ""
                  },
                  {
                    "type": "operation",
                    "name": "count",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      { "name": "label", "idlType": { "idlType": "DOMString" }, "default": { "type": "string", "value": "default" }, "optional": true, "variadic": false }
                    ],
                    "special": ""
                  },
                  {
                    "type": "operation",
                    "name": "timeLog",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      { "name": "label", "idlType": { "idlType": "DOMString" }, "default": { "type": "string", "value": "default" }, "optional": true, "variadic": false },
                      { "name": "data", "idlType": { "idlType": "any" }, "optional": false, "variadic": true }
                    ],
                    "special": ""
                  }
                ]
                """, namespaceName: "TestUtils"));

        StringAssert.Contains(output, "public static extern void Assert(bool condition = false, params object[] data);");
        StringAssert.Contains(output, "public static extern void Count(string label = \"default\");");
        StringAssert.Contains(output, "public static extern void TimeLog(string label = \"default\", params object[] data);");
    }

    [TestMethod]
    public async Task EmitAsync_OptionalArgumentWithoutExplicitDefault_UsesOptionalNullableDefault()
    {
        var output = await EmitNamespacesAsync(
            Namespace("WebAssembly", """
                [
                  {
                    "type": "operation",
                    "name": "instantiate",
                    "idlType": { "generic": "Promise", "idlType": [ { "idlType": "Instance" } ] },
                    "arguments": [
                      { "name": "moduleObject", "idlType": { "idlType": "Module" }, "optional": false, "variadic": false },
                      { "name": "importObject", "idlType": { "idlType": "object" }, "default": null, "optional": true, "variadic": false }
                    ],
                    "special": ""
                  }
                ]
                """, namespaceName: "WebAssembly"));

        StringAssert.Contains(output, "public static extern PromiseResult<Instance> Instantiate(Module moduleObject, object? importObject = default);");
    }

    [TestMethod]
    public async Task EmitAsync_NonConstantExplicitDefaults_DegradeToOptionalDefaultLiteral()
    {
        var output = await EmitNamespacesAsync(
            Namespace("testUtils", """
                [
                  {
                    "type": "operation",
                    "name": "fromRect",
                    "idlType": { "idlType": "DOMRectReadOnly" },
                    "arguments": [
                      {
                        "name": "other",
                        "idlType": { "idlType": "DOMRectInit" },
                        "default": { "type": "dictionary" },
                        "optional": true,
                        "variadic": false
                      }
                    ],
                    "special": ""
                  },
                  {
                    "type": "operation",
                    "name": "initMessageEvent",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      {
                        "name": "ports",
                        "idlType": { "generic": "sequence", "idlType": [ { "idlType": "MessagePort" } ] },
                        "default": { "type": "sequence" },
                        "optional": true,
                        "variadic": false
                      }
                    ],
                    "special": ""
                  },
                  {
                    "type": "operation",
                    "name": "setPoint",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      {
                        "name": "point",
                        "idlType": {
                          "union": true,
                          "idlType": [
                            { "idlType": "double" },
                            { "idlType": "DOMPointInit" },
                            { "generic": "sequence", "idlType": [ { "idlType": "DOMPointInit" } ] }
                          ]
                        },
                        "default": { "type": "number", "value": "0" },
                        "optional": true,
                        "variadic": false
                      }
                    ],
                    "special": ""
                  }
                ]
                """, namespaceName: "TestUtils"));

        StringAssert.Contains(output, "public static extern DOMRectReadOnly FromRect(DOMRectInit? other = default);");
        StringAssert.Contains(output, "public static extern void InitMessageEvent(MessagePort[]? ports = default);");
        StringAssert.Contains(output, "point = default");
        Assert.IsFalse(output.Contains("= new()", StringComparison.Ordinal));
        Assert.IsFalse(output.Contains("= new MessagePort[]()", StringComparison.Ordinal));
        Assert.IsFalse(output.Contains("= 0)", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_PromiseVoidReturn_UsesNonGenericPromiseResult()
    {
        var output = await EmitNamespacesAsync(
            Namespace("testUtils", """
                [
                  {
                    "type": "operation",
                    "name": "flush",
                    "idlType": { "generic": "Promise", "idlType": [ { "idlType": "void" } ] },
                    "arguments": [],
                    "special": ""
                  }
                ]
                """, namespaceName: "TestUtils"));

        StringAssert.Contains(output, "public static extern PromiseResult Flush();");
        Assert.IsFalse(output.Contains("PromiseResult<void>", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_UnionVoidAndConcreteReturn_UsesNullableConcreteType()
    {
        var output = await EmitNamespacesAsync(
            Namespace("testUtils", """
                [
                  {
                    "type": "operation",
                    "name": "parse",
                    "idlType": {
                      "union": true,
                      "idlType": [
                        { "idlType": "void" },
                        { "idlType": "CSSStyleValue" }
                      ]
                    },
                    "arguments": [],
                    "special": ""
                  }
                ]
                """, namespaceName: "TestUtils"));

        StringAssert.Contains(output, "public static extern CSSStyleValue? Parse();");
        Assert.IsFalse(output.Contains("Either<void, CSSStyleValue>", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_AnonymousDeleter_UsesInlineDeleteTemplate()
    {
        var output = await EmitInterfacesAsync(
            Interface("DOMStringMap", """
                [
                  {
                    "type": "operation",
                    "idlType": { "idlType": "undefined" },
                    "arguments": [
                      { "name": "name", "idlType": { "idlType": "DOMString" }, "optional": false, "variadic": false }
                    ],
                    "special": "deleter"
                  }
                ]
                """));

        StringAssert.Contains(output, "[Description(\"@#\")]");
        StringAssert.Contains(output, "[Jazor(\"delete (__arg1)[__arg2]\")]");
        StringAssert.Contains(output, "public extern void Delete(string name);");
        Assert.IsFalse(output.Contains("[Category(\"deleter\")]", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_ExcludedNamespaceDoesNotAddGlobalAliasOrPreviewFile()
    {
        var tempDirectory = Directory.CreateTempSubdirectory("webidl-preview-test-");
        try
        {
            var options = new GeneratorOptions(
                RepositoryRoot: tempDirectory.FullName,
                WorkerPath: Path.Combine(tempDirectory.FullName, "worker.ts"),
                DenoConfigPath: Path.Combine(tempDirectory.FullName, "deno.json"),
                OutputDirectory: tempDirectory.FullName,
                InventoryFileName: "inventory.json");
            var emitter = new PreviewBindingEmitter(options);
            await emitter.EmitAsync(CreateInventory(
                Namespace("console", """
                    [
                      {
                        "type": "operation",
                        "name": "log",
                        "idlType": { "idlType": "undefined" },
                        "arguments": [
                          { "name": "data", "idlType": { "idlType": "any" }, "optional": false, "variadic": true }
                        ],
                        "special": ""
                      }
                    ]
                    """, namespaceName: "console")), CancellationToken.None);

            var previewRoot = Path.Combine(tempDirectory.FullName, "generate");
            var globalUsings = await File.ReadAllTextAsync(Path.Combine(previewRoot, "GlobalUsings.cs"));
            Assert.IsFalse(globalUsings.Contains("global using console =", StringComparison.Ordinal));
            Assert.IsFalse(File.Exists(Path.Combine(previewRoot, "Console", "Namespaces.cs")));
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    private static async Task<string> EmitInterfacesAsync(params WebIdlDeclarationInventory[] declarations)
    {
        var tempDirectory = Directory.CreateTempSubdirectory("webidl-preview-test-");
        try
        {
            var options = new GeneratorOptions(
                RepositoryRoot: tempDirectory.FullName,
                WorkerPath: Path.Combine(tempDirectory.FullName, "worker.ts"),
                DenoConfigPath: Path.Combine(tempDirectory.FullName, "deno.json"),
                OutputDirectory: tempDirectory.FullName,
                InventoryFileName: "inventory.json");
            var emitter = new PreviewBindingEmitter(options);
            await emitter.EmitAsync(CreateInventory(declarations), CancellationToken.None);
            return await File.ReadAllTextAsync(Path.Combine(tempDirectory.FullName, "generate", "Interfaces.cs"));
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    private static async Task<IReadOnlyDictionary<string, string>> EmitGeneratedFilesAsync(params WebIdlDeclarationInventory[] declarations)
    {
        var tempDirectory = Directory.CreateTempSubdirectory("webidl-preview-test-");
        try
        {
            var options = new GeneratorOptions(
                RepositoryRoot: tempDirectory.FullName,
                WorkerPath: Path.Combine(tempDirectory.FullName, "worker.ts"),
                DenoConfigPath: Path.Combine(tempDirectory.FullName, "deno.json"),
                OutputDirectory: tempDirectory.FullName,
                InventoryFileName: "inventory.json");
            var emitter = new PreviewBindingEmitter(options);
            await emitter.EmitAsync(CreateInventory(declarations), CancellationToken.None);

            var previewRoot = Path.Combine(tempDirectory.FullName, "generate");
            var files = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var file in Directory.GetFiles(previewRoot, "*.cs", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(previewRoot, file).Replace('\\', '/');
                files[relativePath] = await File.ReadAllTextAsync(file);
            }

            AssertNoLegacyUnionArtifacts(files);
            return files;
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    private static async Task<string> EmitNamespacesAsync(params WebIdlDeclarationInventory[] declarations)
    {
        var tempDirectory = Directory.CreateTempSubdirectory("webidl-preview-test-");
        try
        {
            var options = new GeneratorOptions(
                RepositoryRoot: tempDirectory.FullName,
                WorkerPath: Path.Combine(tempDirectory.FullName, "worker.ts"),
                DenoConfigPath: Path.Combine(tempDirectory.FullName, "deno.json"),
                OutputDirectory: tempDirectory.FullName,
                InventoryFileName: "inventory.json");
            var emitter = new PreviewBindingEmitter(options);
            await emitter.EmitAsync(CreateInventory(declarations), CancellationToken.None);
            var fileNamespace = declarations[0].Payload.GetStringOrNull("__fileNamespace");
            var filePath = string.IsNullOrWhiteSpace(fileNamespace)
                ? Path.Combine(tempDirectory.FullName, "generate", "Namespaces.cs")
                : Path.Combine(tempDirectory.FullName, "generate", fileNamespace, "Namespaces.cs");
            return await File.ReadAllTextAsync(filePath);
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    private static void AssertNoLegacyUnionArtifacts(IReadOnlyDictionary<string, string> files)
    {
        foreach (var (path, content) in files)
        {
            Assert.IsFalse(content.Contains("Either<", StringComparison.Ordinal), path);
            Assert.IsFalse(content.Contains(": IEither", StringComparison.Ordinal), path);
            Assert.IsFalse(content.Contains("[ECMAScriptUnion]", StringComparison.Ordinal), path);
        }
    }

    private static Diagnostic[] CompileGeneratedFiles(IReadOnlyDictionary<string, string> files, params string[] additionalSources)
    {
        const string infrastructureSource = """
            namespace ECMAScript
            {
                [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
                public sealed class ECMAScriptAttribute : System.Attribute;

                public class DOMPoint;

                public class DOMRect;

                public class DOMMatrix;
            }

            namespace ECMAScript.Contract
            {
                public interface IUIComponent;
            }

            namespace ECMAScript.CSS
            {
                public static class CssNamespaceStub;
            }

            namespace ECMAScript.GPUBufferUsage
            {
                public static class GpuBufferUsageNamespaceStub;
            }

            namespace ECMAScript.WebAssembly
            {
                public static class WebAssemblyNamespaceStub;
            }

            namespace System.Runtime.CompilerServices
            {
                [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false)]
                public sealed class UnionAttribute : System.Attribute;

                public interface IUnion
                {
                    object? Value { get; }
                }
            }
            """;
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var syntaxTrees = files
            .Where(static pair => pair.Key is "GlobalUsings.cs" or "Unions.cs")
            .Select(pair => CSharpSyntaxTree.ParseText(pair.Value, parseOptions, pair.Key))
            .Concat(additionalSources.Select((source, index) => CSharpSyntaxTree.ParseText(source, parseOptions, $"Additional{index}.cs")))
            .Append(CSharpSyntaxTree.ParseText(infrastructureSource, parseOptions, "Infrastructure.cs"))
            .ToArray();
        var compilation = CSharpCompilation.Create(
            "ECMAScript.WebIDL.GeneratorTest.GeneratedPreview",
            syntaxTrees,
            Net110.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
    }

    private static WebIdlInventory CreateInventory(params WebIdlDeclarationInventory[] declarations)
    {
        var files = declarations
            .Select((declaration, index) => new WebIdlFileInventory(
                FileName: $"test-{index}.idl",
                Namespace: declaration.Payload.GetStringOrNull("__fileNamespace"),
                Declarations: [declaration]))
            .ToArray();
        return new WebIdlInventory(
            SchemaVersion: 1,
            GeneratedAt: DateTimeOffset.UtcNow,
            Source: new WebIdlSourceInfo("test", "test", "test", "test"),
            Files: files,
            InterfaceEvents: [],
            Stats: new WebIdlStats(files.Length, declarations.Length, 0, new Dictionary<string, int>(StringComparer.Ordinal)));
    }

    private static WebIdlDeclarationInventory Interface(
        string name,
        string membersJson,
        string? inheritance = null,
        bool partial = false,
        WebIdlDocumentation? documentation = null,
        IReadOnlyList<WebIdlMemberDocumentation>? memberDocumentation = null)
    {
        var payload = ParseObject($$"""
            {
              "members": {{membersJson}}
            }
        """);
        return new WebIdlDeclarationInventory(
            "interface",
            name,
            partial ? true : null,
            inheritance,
            null,
            null,
            payload.GetArray("members").Count,
            payload,
            documentation,
            memberDocumentation);
    }

    private static WebIdlDeclarationInventory Callback(string name, string signatureJson)
    {
        var payload = ParseObject(signatureJson);
        return new WebIdlDeclarationInventory(
            "callback",
            name,
            null,
            null,
            null,
            null,
            payload.GetArray("arguments").Count,
            payload);
    }

    private static WebIdlDocumentation Documentation(
        string href,
        string specificationTitle,
        string heading,
        string? prose = null,
        string? usage = null)
        => new(href, specificationTitle, heading, href, prose, usage);

    private static WebIdlDeclarationInventory Enum(string name, string valuesJson)
    {
        var payload = ParseObject($$"""
            {
              "values": {{valuesJson}}
            }
            """);
        return new WebIdlDeclarationInventory("enum", name, null, null, null, null, payload.GetArray("values").Count, payload);
    }

    private static WebIdlDeclarationInventory CallbackInterface(string name, string membersJson, bool partial = false)
    {
        var payload = ParseObject($$"""
            {
              "members": {{membersJson}}
            }
        """);
        return new WebIdlDeclarationInventory("callback interface", name, partial ? true : null, null, null, null, payload.GetArray("members").Count, payload);
    }

    private static WebIdlDeclarationInventory Dictionary(string name, string membersJson, string? inheritance = null)
    {
        var payload = ParseObject($$"""
            {
              "members": {{membersJson}}
            }
            """);
        return new WebIdlDeclarationInventory("dictionary", name, null, inheritance, null, null, payload.GetArray("members").Count, payload);
    }

    private static WebIdlDeclarationInventory Typedef(string name, string idlTypeJson)
    {
        var payload = ParseObject($$"""
            {
              "idlType": {{idlTypeJson}}
            }
            """);
        return new WebIdlDeclarationInventory("typedef", name, null, null, null, null, null, payload);
    }

    private static WebIdlDeclarationInventory InterfaceMixin(string name, string membersJson)
    {
        var payload = ParseObject($$"""
            {
              "members": {{membersJson}}
            }
            """);
        return new WebIdlDeclarationInventory("interface mixin", name, null, null, null, null, payload.GetArray("members").Count, payload);
    }

    private static WebIdlDeclarationInventory Includes(string target, string mixin)
    {
        return new WebIdlDeclarationInventory("includes", null, null, null, target, mixin, null, ParseObject("{}"));
    }

    private static WebIdlDeclarationInventory Namespace(string name, string membersJson, string? namespaceName = null, bool partial = false)
    {
        var payload = ParseObject($$"""
            {
              "type": "namespace",
              "name": "{{name}}",
              "__fileNamespace": {{(namespaceName is null ? "null" : $"\"{namespaceName}\"")}},
              "members": {{membersJson}}
            }
            """);
        return new WebIdlDeclarationInventory("namespace", name, partial ? true : null, null, null, null, payload.GetArray("members").Count, payload);
    }

    private static JsonElement ParseObject(string json)
    {
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
