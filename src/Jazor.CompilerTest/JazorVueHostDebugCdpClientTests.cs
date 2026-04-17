using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.VueHost.Debug;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostDebugCdpClientTests
{
    [TestMethod]
    public void CdpClient_ParsePausedCallFrames_ReadsFrameUrlAndLocation()
    {
        using var parameters = JsonDocument.Parse(
            """
            {
              "callFrames": [
                {
                  "callFrameId": "frame-1",
                  "functionName": "render",
                  "url": "http://localhost:5173/src/App.vue",
                  "location": {
                    "lineNumber": 12,
                    "columnNumber": 4
                  }
                }
              ]
            }
            """);

        var frames = CdpClient.ParsePausedCallFrames(parameters.RootElement);

        Assert.AreEqual(1, frames.Count);
        Assert.AreEqual("frame-1", frames[0].CallFrameId);
        Assert.AreEqual("render", frames[0].FunctionName);
        Assert.AreEqual("http://localhost:5173/src/App.vue", frames[0].Location.Url);
        Assert.AreEqual(12, frames[0].Location.LineNumber);
        Assert.AreEqual(4, frames[0].Location.ColumnNumber);
    }

    [TestMethod]
    public void CdpClient_ParsePausedCallFrames_RetainsScopeChainEntries()
    {
        using var parameters = JsonDocument.Parse(
            """
            {
              "callFrames": [
                {
                  "callFrameId": "frame-1",
                  "functionName": "render",
                  "url": "http://localhost:5173/src/App.vue",
                  "location": {
                    "lineNumber": 12,
                    "columnNumber": 4
                  },
                  "scopeChain": [
                    {
                      "type": "local",
                      "object": {
                        "type": "object",
                        "description": "Local",
                        "objectId": "scope-local-1"
                      }
                    },
                    {
                      "type": "closure",
                      "name": "setup",
                      "object": {
                        "type": "object",
                        "description": "Closure",
                        "objectId": "scope-closure-1"
                      }
                    }
                  ]
                }
              ]
            }
            """);

        var frames = CdpClient.ParsePausedCallFrames(parameters.RootElement);

        Assert.AreEqual(1, frames.Count);
        var scopeChain = frames[0].ScopeChain;
        Assert.IsNotNull(scopeChain);
        Assert.AreEqual(2, scopeChain.Count);
        Assert.AreEqual("local", scopeChain[0].Type);
        Assert.AreEqual("scope-local-1", scopeChain[0].Object.ObjectId);
        Assert.AreEqual("closure", scopeChain[1].Type);
        Assert.AreEqual("setup", scopeChain[1].Name);
        Assert.AreEqual("scope-closure-1", scopeChain[1].Object.ObjectId);
    }

    [TestMethod]
    public void CdpClient_ParseRemoteObject_PrefersTypedValue()
    {
        using var remoteObject = JsonDocument.Parse(
            """
            {
              "type": "number",
              "description": "42",
              "value": 42
            }
            """);

        var parsed = CdpClient.ParseRemoteObject(remoteObject.RootElement);

        Assert.AreEqual("number", parsed.Type);
        Assert.AreEqual("42", parsed.Value);
        Assert.AreEqual("42", parsed.Description);
    }

    [TestMethod]
    public void CdpClient_ParsePropertyDescriptors_ReadsExpandableObjectProperties()
    {
        using var properties = JsonDocument.Parse(
            """
            [
              {
                "name": "count",
                "value": {
                  "type": "number",
                  "description": "3",
                  "value": 3
                }
              },
              {
                "name": "model",
                "value": {
                  "type": "object",
                  "description": "Object",
                  "objectId": "remote-model-1"
                }
              }
            ]
            """);

        var parsed = CdpClient.ParsePropertyDescriptors(properties.RootElement);

        Assert.AreEqual(2, parsed.Count);
        Assert.AreEqual("count", parsed[0].Name);
        Assert.AreEqual("3", parsed[0].Value.Value);
        Assert.AreEqual("model", parsed[1].Name);
        Assert.AreEqual("remote-model-1", parsed[1].Value.ObjectId);
    }

    [TestMethod]
    public void CdpClient_BuildBreakpointUrlRegex_MatchesRelativePathSuffixWithQuery()
    {
        var regex = CdpClient.BuildBreakpointUrlRegex("/main.ts");

        Assert.IsNotNull(regex);
        Assert.IsTrue(Regex.IsMatch("http://localhost:5173/main.ts?t=1", regex));
        Assert.IsTrue(Regex.IsMatch("http://localhost:5173/main.ts#hash", regex));
        Assert.IsFalse(Regex.IsMatch("http://localhost:5173/main.tsx?t=1", regex));
    }

    [TestMethod]
    public void CdpClient_BuildBreakpointUrlRegex_LeavesAbsoluteHttpUrlOnExactMatchPath()
    {
        var regex = CdpClient.BuildBreakpointUrlRegex("http://localhost:5173/main.ts");

        Assert.IsNull(regex);
    }

    [TestMethod]
    public void CdpClient_BuildBreakpointUrlRegex_NormalizesWindowsSeparators()
    {
        var regex = CdpClient.BuildBreakpointUrlRegex(@"src\pages\main.ts");

        Assert.IsNotNull(regex);
        StringAssert.Contains(regex, "src/pages/main\\.ts");
        Assert.IsTrue(Regex.IsMatch("http://localhost:5173/src/pages/main.ts?import", regex));
    }

    [TestMethod]
    public void CdpClient_TryParseScriptParsed_ParsesScriptIdAndUrl()
    {
        using var message = JsonDocument.Parse(
            """
            {
              "method": "Debugger.scriptParsed",
              "params": {
                "scriptId": "16",
                "url": "http://127.0.0.1:5173/main.ts?t=1710000000"
              }
            }
            """);

        var parsed = CdpClient.TryParseScriptParsed(
            message.RootElement,
            out var scriptId,
            out var scriptUrl);

        Assert.IsTrue(parsed);
        Assert.AreEqual("16", scriptId);
        Assert.AreEqual("http://127.0.0.1:5173/main.ts?t=1710000000", scriptUrl);
    }

    [TestMethod]
    public void CdpClient_TryParseScriptParsed_ReturnsFalseWhenUrlMissing()
    {
        using var message = JsonDocument.Parse(
            """
            {
              "method": "Debugger.scriptParsed",
              "params": {
                "scriptId": "16"
              }
            }
            """);

        var parsed = CdpClient.TryParseScriptParsed(
            message.RootElement,
            out var scriptId,
            out var scriptUrl);

        Assert.IsFalse(parsed);
        Assert.AreEqual(string.Empty, scriptId);
        Assert.AreEqual(string.Empty, scriptUrl);
    }

    [TestMethod]
    public void CdpClient_ResolveScriptUrls_RewritesScriptIdLocationWithKnownScriptUrl()
    {
        var frames = new[]
        {
            new CdpCallFrame(
                "frame-1",
                "runIteration",
                new CdpLocation("16", 8, 18))
        };
        var mappings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["16"] = "http://127.0.0.1:5173/main.ts?t=1710000000"
        };

        var rewrittenFrames = CdpClient.ResolveScriptUrls(frames, mappings);

        Assert.AreEqual(1, rewrittenFrames.Count);
        Assert.AreEqual("http://127.0.0.1:5173/main.ts?t=1710000000", rewrittenFrames[0].Location.Url);
        Assert.AreEqual(8, rewrittenFrames[0].Location.LineNumber);
        Assert.AreEqual(18, rewrittenFrames[0].Location.ColumnNumber);
    }
}
