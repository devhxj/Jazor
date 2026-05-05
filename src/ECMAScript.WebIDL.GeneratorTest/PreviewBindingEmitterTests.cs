using System.Text.Json;
using ECMAScript.WebIDL.Generator;

namespace ECMAScript.WebIDL.GeneratorTest;

[TestClass]
public sealed class PreviewBindingEmitterTests
{
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

        Assert.IsFalse(files["GlobalUsings.cs"].Contains("Either<", StringComparison.Ordinal));
        StringAssert.Contains(files["Unions.cs"], "public readonly struct ConstrainDOMString : IEither, IEnumerable<string>");
        StringAssert.Contains(files["Unions.cs"], "public static implicit operator ConstrainDOMString(string[] value)");
        StringAssert.Contains(files["Unions.cs"], "public static class ConstrainDOMStringCollectionBuilder");
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
        StringAssert.Contains(files["Unions.cs"], "public readonly struct ClipboardItemDataValue : IEither");
        Assert.IsFalse(files["GlobalUsings.cs"].Contains("Either<", StringComparison.Ordinal));
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
        Assert.IsFalse(files["Dictionaries.cs"].Contains("Either<", StringComparison.Ordinal));
        StringAssert.Contains(files["Unions.cs"], "public readonly struct IntersectionObserverInitThreshold : IEither, IEnumerable<double>");
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
        Assert.IsFalse(files["Interfaces.cs"].Contains("Either<", StringComparison.Ordinal));
        StringAssert.Contains(files["Unions.cs"], "public readonly struct URLSearchParamsInit : IEither, IEnumerable<string[]>");
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
        StringAssert.Contains(files["Unions.cs"], "public readonly struct EventTargetAddEventListenerOptionsValue : IEither");
        Assert.IsFalse(files["Unions.cs"].Contains("public readonly struct EventTargetAddEventListenerOptions : IEither", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task EmitAsync_WrapperIncompatibleTypedefUnions_FallBackToEitherAliases()
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

        StringAssert.Contains(files["GlobalUsings.cs"], "global using BufferSource = ECMAScript.Either<ECMAScript.IArrayBufferView, ECMAScript.ArrayBuffer>;");
        StringAssert.Contains(files["GlobalUsings.cs"], "global using AlgorithmIdentifier = ECMAScript.Either<object, string>;");
        Assert.IsFalse(files.ContainsKey("Unions.cs"));
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
        Assert.IsFalse(files["Unions.cs"].Contains("public readonly struct RoundRectRadiiValueValue2 : IEither", StringComparison.Ordinal));
        Assert.IsFalse(files["Unions.cs"].Contains("public readonly struct RoundRectRadiiValueValue3 : IEither", StringComparison.Ordinal));
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
        StringAssert.Contains(files["Interfaces.cs"], "public extern void SetFormValue(ElementInternalsSetFormValue value, File state);");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void SetFormValue(ElementInternalsSetFormValue value, string state);");
        StringAssert.Contains(files["Interfaces.cs"], "public extern void SetFormValue(ElementInternalsSetFormValue value, FormData state);");
        StringAssert.Contains(files["Unions.cs"], "public readonly struct ElementInternalsSetFormValue : IEither");
        StringAssert.Contains(files["Unions.cs"], "public readonly struct ElementInternalsSetFormValueState : IEither");
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
		Assert.Contains("public class AudioRenderCapacityEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)", output);
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
    public async Task EmitAsync_InterfaceMemberDocumentation_IsIndentedConsistently()
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
                """));

        StringAssert.Contains(output, $"    /// <summary>{Environment.NewLine}    /// Constructor {Environment.NewLine}    /// </summary>{Environment.NewLine}    public extern AbortController();");
        StringAssert.Contains(output, $"    /// <summary>{Environment.NewLine}    /// signal{Environment.NewLine}    /// </summary>{Environment.NewLine}    [Description(\"@#signal\")]");
        StringAssert.Contains(output, $"    /// <summary>{Environment.NewLine}    /// abort{Environment.NewLine}    /// </summary>{Environment.NewLine}    /// <param name=\"reason\">reason</param>{Environment.NewLine}    [Description(\"@#abort\")]");
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

    private static WebIdlDeclarationInventory Interface(string name, string membersJson, string? inheritance = null, bool partial = false)
    {
        var payload = ParseObject($$"""
            {
              "members": {{membersJson}}
            }
        """);
        return new WebIdlDeclarationInventory("interface", name, partial ? true : null, inheritance, null, null, payload.GetArray("members").Count, payload);
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
