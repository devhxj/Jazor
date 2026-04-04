using System.Text.Json;
using ECMAScript.WebIDL.Generator;

namespace ECMAScript.WebIDL.GeneratorTest;

[TestClass]
public sealed class PreviewBindingEmitterTests
{
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
            Namespace("console", """
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
                """, namespaceName: "console"));

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
    public async Task EmitAsync_NamespaceAddsGlobalAliasForDirectUsage()
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

            var globalUsings = await File.ReadAllTextAsync(Path.Combine(tempDirectory.FullName, "csharp-preview", "GlobalUsings.cs"));
			Assert.Contains("global using console = ECMAScript.Console.Console;", globalUsings);
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
            return await File.ReadAllTextAsync(Path.Combine(tempDirectory.FullName, "csharp-preview", "Interfaces.cs"));
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
                ? Path.Combine(tempDirectory.FullName, "csharp-preview", "Namespaces.cs")
                : Path.Combine(tempDirectory.FullName, "csharp-preview", fileNamespace, "Namespaces.cs");
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
