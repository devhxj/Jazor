using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Jazor.VueHost.Debug;
using Jazor.VueHost.SourceMap;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostDebugProtocolTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_Initialize_ReturnsCapabilitiesAndInitializedEvent()
    {
        var handler = CreateHandler(new InMemorySourceMapService(), out _);

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 1,
                Command = "initialize"
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        Assert.AreEqual("initialize", result.Response.Command);
        Assert.AreEqual(1, result.Events.Count);
        Assert.AreEqual("initialized", result.Events[0].Event);

        using var body = GetResponseBody(result.Response);
        Assert.IsTrue(body.RootElement.GetProperty("supportsConfigurationDoneRequest").GetBoolean());
        Assert.IsFalse(body.RootElement.GetProperty("supportsConditionalBreakpoints").GetBoolean());
        Assert.IsTrue(body.RootElement.GetProperty("supportsEvaluateForHovers").GetBoolean());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_SetBreakpoints_VerifiesMappedAndUnmappedBreakpoints()
    {
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceLineMap(
                "Counter.jazor",
                "line0\nline1\nline2",
                [
                    1
                ]));
        var handler = CreateHandler(sourceMapService, out var session);

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 2,
                Command = "setBreakpoints",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    source = new
                    {
                        path = @"D:\repo\Counter.jazor"
                    },
                    breakpoints = new object[]
                    {
                        new { line = 2, column = 1 }
                    }
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        var breakpoints = body.RootElement.GetProperty("breakpoints").EnumerateArray().ToArray();
        Assert.AreEqual(1, breakpoints.Length);
        Assert.IsTrue(breakpoints[0].GetProperty("verified").GetBoolean());

        var bindings = session.GetBreakpoints(@"D:\repo\Counter.jazor");
        Assert.AreEqual(1, bindings.Count);
        Assert.IsNotNull(bindings[0].GeneratedBreakpoint);
        Assert.AreEqual("/Counter.jazor", bindings[0].GeneratedBreakpoint!.GeneratedPath);

        var missingResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 3,
                Command = "setBreakpoints",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    source = new
                    {
                        path = @"D:\repo\Missing.jazor"
                    },
                    breakpoints = new object[]
                    {
                        new { line = 2, column = 1 }
                    }
                })
            },
            CancellationToken.None);

        Assert.IsTrue(missingResult.Response.Success);
        using var missingBody = GetResponseBody(missingResult.Response);
        var missingBreakpoints = missingBody.RootElement.GetProperty("breakpoints").EnumerateArray().ToArray();
        Assert.AreEqual(1, missingBreakpoints.Length);
        Assert.IsFalse(missingBreakpoints[0].GetProperty("verified").GetBoolean());
        StringAssert.Contains(
            missingBreakpoints[0].GetProperty("message").GetString() ?? string.Empty,
            "could not be mapped");
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_SetBreakpoints_WithCdpBackend_UsesResolvedGeneratedLocation()
    {
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceLineMap(
                "Counter.jazor",
                "line0\nline1\nline2",
                [
                    1
                ]));
        var cdpClient = new FakeCdpClient();
        cdpClient.SetBreakpointResolution(
            "/Counter.jazor",
            0,
            0,
            new CdpBreakpointResolution(
                "bp-cdp-1",
                new CdpLocation("/virtual/Counter.generated.js", 3, 2)));
        var handler = CreateHandler(sourceMapService, out var session, cdpClient);

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 100,
                Command = "setBreakpoints",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    source = new
                    {
                        path = @"D:\repo\Counter.jazor"
                    },
                    breakpoints = new object[]
                    {
                        new { line = 2, column = 1 }
                    }
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        var breakpoints = body.RootElement.GetProperty("breakpoints").EnumerateArray().ToArray();
        Assert.AreEqual(1, breakpoints.Length);
        Assert.IsTrue(breakpoints[0].GetProperty("verified").GetBoolean());
        Assert.IsFalse(breakpoints[0].TryGetProperty("message", out _));

        var bindings = session.GetBreakpoints(@"D:\repo\Counter.jazor");
        Assert.AreEqual(1, bindings.Count);
        Assert.IsNotNull(bindings[0].GeneratedBreakpoint);
        Assert.AreEqual("/virtual/Counter.generated.js", bindings[0].GeneratedBreakpoint!.GeneratedPath);
        Assert.AreEqual(3, bindings[0].GeneratedBreakpoint!.GeneratedLine);
        Assert.AreEqual(2, bindings[0].GeneratedBreakpoint!.GeneratedColumn);

        Assert.AreEqual(1, cdpClient.BreakpointRequests.Count);
        Assert.AreEqual("/Counter.jazor", cdpClient.BreakpointRequests[0].GeneratedUrl);
        Assert.AreEqual(0, cdpClient.BreakpointRequests[0].GeneratedLine);
        Assert.AreEqual(0, cdpClient.BreakpointRequests[0].GeneratedColumn);
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_SetBreakpoints_WithCdpBackend_ReturnsUnverifiedWhenTargetBindingFails()
    {
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceLineMap(
                "Counter.jazor",
                "line0\nline1\nline2",
                [
                    1
                ]));
        var cdpClient = new FakeCdpClient();
        var handler = CreateHandler(sourceMapService, out var session, cdpClient);

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 101,
                Command = "setBreakpoints",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    source = new
                    {
                        path = @"D:\repo\Counter.jazor"
                    },
                    breakpoints = new object[]
                    {
                        new { line = 2, column = 1 }
                    }
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        var breakpoints = body.RootElement.GetProperty("breakpoints").EnumerateArray().ToArray();
        Assert.AreEqual(1, breakpoints.Length);
        Assert.IsFalse(breakpoints[0].GetProperty("verified").GetBoolean());
        StringAssert.Contains(
            breakpoints[0].GetProperty("message").GetString() ?? string.Empty,
            "CDP target");

        var bindings = session.GetBreakpoints(@"D:\repo\Counter.jazor");
        Assert.AreEqual(1, bindings.Count);
        Assert.IsNull(bindings[0].GeneratedBreakpoint);

        Assert.AreEqual(1, cdpClient.BreakpointRequests.Count);
        Assert.AreEqual("/Counter.jazor", cdpClient.BreakpointRequests[0].GeneratedUrl);
        Assert.AreEqual(0, cdpClient.BreakpointRequests[0].GeneratedLine);
        Assert.AreEqual(0, cdpClient.BreakpointRequests[0].GeneratedColumn);
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_StackTrace_MapsSessionFrames()
    {
        var sourceText = """
            <button>@Count</button>

            @code {
                private int Count = 1;

                public void Increment()
                {
                    Count++;
                }
            }
            """;
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceLineMap(
                "Counter.jazor",
                sourceText,
                [
                    GetLineIndexContaining(sourceText, "private int Count = 1;"),
                    GetLineIndexContaining(sourceText, "Count++;")
                ]));
        var handler = CreateHandler(sourceMapService, out var session);
        session.CurrentCallFrames =
        [
            new CdpCallFrame(
                "frame-1",
                "increment",
                new CdpLocation("/Counter.jazor", 1, 0))
        ];

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 3,
                Command = "stackTrace",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    threadId = 1
                })
            },
            CancellationToken.None);

        using var body = GetResponseBody(result.Response);
        var frames = body.RootElement.GetProperty("stackFrames").EnumerateArray().ToArray();
        Assert.AreEqual(1, frames.Length);
        Assert.AreEqual("increment", frames[0].GetProperty("name").GetString());
        Assert.AreEqual("Counter.jazor", frames[0].GetProperty("source").GetProperty("path").GetString());
        Assert.AreEqual(GetLineIndexContaining(sourceText, "Count++;") + 1, frames[0].GetProperty("line").GetInt32());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_ScopesAndVariables_ReturnDeterministicFallbackState()
    {
        var handler = CreateHandler(new InMemorySourceMapService(), out var session);
        session.IsInitialized = true;
        session.IsStarted = true;
        session.CurrentCallFrames =
        [
            new CdpCallFrame(
                "frame-1",
                "increment",
                new CdpLocation("/Counter.jazor", 1, 0))
        ];

        var scopesResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 4,
                Command = "scopes",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    frameId = 1
                })
            },
            CancellationToken.None);

        using var scopesBody = GetResponseBody(scopesResult.Response);
        var scopes = scopesBody.RootElement.GetProperty("scopes").EnumerateArray().ToArray();
        Assert.AreEqual(2, scopes.Length);
        Assert.AreEqual("Locals", scopes[0].GetProperty("name").GetString());
        Assert.AreEqual("Session", scopes[1].GetProperty("name").GetString());

        var localsReference = scopes[0].GetProperty("variablesReference").GetInt32();
        var sessionReference = scopes[1].GetProperty("variablesReference").GetInt32();
        Assert.IsTrue(localsReference > 0);
        Assert.IsTrue(sessionReference > 0);

        var localsResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 5,
                Command = "variables",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    variablesReference = localsReference
                })
            },
            CancellationToken.None);

        using var localsBody = GetResponseBody(localsResult.Response);
        var locals = localsBody.RootElement.GetProperty("variables").EnumerateArray().ToArray();
        Assert.AreEqual(5, locals.Length);
        Assert.AreEqual("callFrameId", locals[0].GetProperty("name").GetString());
        Assert.AreEqual("frame-1", locals[0].GetProperty("value").GetString());
        Assert.AreEqual("functionName", locals[1].GetProperty("name").GetString());
        Assert.AreEqual("increment", locals[1].GetProperty("value").GetString());
        Assert.AreEqual("source", locals[2].GetProperty("name").GetString());
        Assert.IsTrue(locals[2].GetProperty("variablesReference").GetInt32() > 0);
        Assert.AreEqual("location", locals[3].GetProperty("name").GetString());
        Assert.IsTrue(locals[3].GetProperty("variablesReference").GetInt32() > 0);
        Assert.AreEqual("backend", locals[4].GetProperty("name").GetString());
        Assert.AreEqual("fallback", locals[4].GetProperty("value").GetString());

        var sourceResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 6,
                Command = "variables",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    variablesReference = locals[2].GetProperty("variablesReference").GetInt32()
                })
            },
            CancellationToken.None);

        using var sourceBody = GetResponseBody(sourceResult.Response);
        var sourceVariables = sourceBody.RootElement.GetProperty("variables").EnumerateArray().ToArray();
        Assert.AreEqual(2, sourceVariables.Length);
        Assert.AreEqual("name", sourceVariables[0].GetProperty("name").GetString());
        Assert.AreEqual("Counter.jazor", sourceVariables[0].GetProperty("value").GetString());
        Assert.AreEqual("path", sourceVariables[1].GetProperty("name").GetString());
        Assert.AreEqual("/Counter.jazor", sourceVariables[1].GetProperty("value").GetString());

        var sessionResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 7,
                Command = "variables",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    variablesReference = sessionReference
                })
            },
            CancellationToken.None);

        using var sessionBody = GetResponseBody(sessionResult.Response);
        var sessionVariables = sessionBody.RootElement.GetProperty("variables").EnumerateArray().ToArray();
        Assert.AreEqual(6, sessionVariables.Length);
        Assert.AreEqual("initialized", sessionVariables[0].GetProperty("name").GetString());
        Assert.AreEqual("true", sessionVariables[0].GetProperty("value").GetString());
        Assert.AreEqual("paused", sessionVariables[2].GetProperty("name").GetString());
        Assert.AreEqual("true", sessionVariables[2].GetProperty("value").GetString());
        Assert.AreEqual("selectedFrameId", sessionVariables[5].GetProperty("name").GetString());
        Assert.AreEqual("1", sessionVariables[5].GetProperty("value").GetString());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_ContinueAndEvaluate_UseFallbackSessionState()
    {
        var handler = CreateHandler(new InMemorySourceMapService(), out var session);
        session.IsInitialized = true;
        session.IsStarted = true;
        session.CurrentCallFrames =
        [
            new CdpCallFrame(
                "frame-1",
                "increment",
                new CdpLocation("/Counter.jazor", 1, 0))
        ];

        var evaluateResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 8,
                Command = "evaluate",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    expression = "source.path",
                    frameId = 1,
                    context = "repl"
                })
            },
            CancellationToken.None);

        using var evaluateBody = GetResponseBody(evaluateResult.Response);
        Assert.AreEqual("/Counter.jazor", evaluateBody.RootElement.GetProperty("result").GetString());
        Assert.AreEqual("string", evaluateBody.RootElement.GetProperty("type").GetString());
        Assert.AreEqual(0, evaluateBody.RootElement.GetProperty("variablesReference").GetInt32());

        var objectEvaluateResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 9,
                Command = "evaluate",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    expression = "location",
                    frameId = 1,
                    context = "watch"
                })
            },
            CancellationToken.None);

        using var objectEvaluateBody = GetResponseBody(objectEvaluateResult.Response);
        var locationReference = objectEvaluateBody.RootElement.GetProperty("variablesReference").GetInt32();
        Assert.IsTrue(locationReference > 0);
        Assert.AreEqual("object", objectEvaluateBody.RootElement.GetProperty("type").GetString());

        var missingEvaluateResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 10,
                Command = "evaluate",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    expression = "Count + 1",
                    frameId = 1,
                    context = "repl"
                })
            },
            CancellationToken.None);

        using var missingEvaluateBody = GetResponseBody(missingEvaluateResult.Response);
        Assert.AreEqual(
            "[repl] Count + 1",
            missingEvaluateBody.RootElement.GetProperty("result").GetString());

        var continueResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 11,
                Command = "continue",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    threadId = 1
                })
            },
            CancellationToken.None);

        Assert.IsTrue(continueResult.Response.Success);
        Assert.AreEqual(1, continueResult.Events.Count);
        Assert.AreEqual("continued", continueResult.Events[0].Event);
        Assert.IsFalse(session.IsPaused);
        Assert.AreEqual(0, session.CurrentCallFrames.Count);

        using var continueBody = GetResponseBody(continueResult.Response);
        Assert.IsTrue(continueBody.RootElement.GetProperty("allThreadsContinued").GetBoolean());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_CdpScopesVariablesAndEvaluate_UseScopeChainAndRemoteExpansion()
    {
        var sourceMapService = new InMemorySourceMapService();
        var cdpClient = new FakeCdpClient();
        var handler = CreateHandler(sourceMapService, out _, cdpClient);

        cdpClient.SetProperties(
            "scope-local-1",
            new CdpPropertyDescriptor(
                "count",
                new CdpRemoteObject(
                    Type: "number",
                    SubType: null,
                    Description: "3",
                    Value: "3",
                    UnserializableValue: null,
                    ObjectId: null)),
            new CdpPropertyDescriptor(
                "model",
                new CdpRemoteObject(
                    Type: "object",
                    SubType: null,
                    Description: "Object",
                    Value: null,
                    UnserializableValue: null,
                    ObjectId: "remote-model-1")));
        cdpClient.SetProperties(
            "scope-closure-1",
            new CdpPropertyDescriptor(
                "captured",
                new CdpRemoteObject(
                    Type: "string",
                    SubType: null,
                    Description: "\"from closure\"",
                    Value: "from closure",
                    UnserializableValue: null,
                    ObjectId: null)));
        cdpClient.SetProperties(
            "remote-model-1",
            new CdpPropertyDescriptor(
                "label",
                new CdpRemoteObject(
                    Type: "string",
                    SubType: null,
                    Description: "\"counter\"",
                    Value: "counter",
                    UnserializableValue: null,
                    ObjectId: null)));
        cdpClient.SetEvaluationResult(
            "frame-1",
            "model",
            new CdpRemoteObject(
                Type: "object",
                SubType: null,
                Description: "Object",
                Value: null,
                UnserializableValue: null,
                ObjectId: "remote-model-1"));

        cdpClient.EmitPaused(
        [
            new CdpCallFrame(
                "frame-1",
                "increment",
                new CdpLocation("/Counter.jazor", 1, 0),
                [
                    new CdpScope(
                        "local",
                        null,
                        new CdpRemoteObject(
                            Type: "object",
                            SubType: null,
                            Description: "Local",
                            Value: null,
                            UnserializableValue: null,
                            ObjectId: "scope-local-1")),
                    new CdpScope(
                        "closure",
                        "setup",
                        new CdpRemoteObject(
                            Type: "object",
                            SubType: null,
                            Description: "Closure",
                            Value: null,
                            UnserializableValue: null,
                            ObjectId: "scope-closure-1"))
                ])
        ]);

        var scopesResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 12,
                Command = "scopes",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    frameId = 1
                })
            },
            CancellationToken.None);

        using var scopesBody = GetResponseBody(scopesResult.Response);
        var scopes = scopesBody.RootElement.GetProperty("scopes").EnumerateArray().ToArray();
        Assert.AreEqual(3, scopes.Length);
        Assert.AreEqual("Local", scopes[0].GetProperty("name").GetString());
        Assert.AreEqual("Closure (setup)", scopes[1].GetProperty("name").GetString());
        Assert.AreEqual("Session", scopes[2].GetProperty("name").GetString());

        var localScopeReference = scopes[0].GetProperty("variablesReference").GetInt32();
        var closureScopeReference = scopes[1].GetProperty("variablesReference").GetInt32();

        var localVariablesResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 13,
                Command = "variables",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    variablesReference = localScopeReference
                })
            },
            CancellationToken.None);

        using var localVariablesBody = GetResponseBody(localVariablesResult.Response);
        var localVariables = localVariablesBody.RootElement.GetProperty("variables").EnumerateArray().ToArray();
        Assert.AreEqual(2, localVariables.Length);
        Assert.AreEqual("count", localVariables[0].GetProperty("name").GetString());
        Assert.AreEqual("3", localVariables[0].GetProperty("value").GetString());
        Assert.AreEqual(0, localVariables[0].GetProperty("variablesReference").GetInt32());
        Assert.AreEqual("model", localVariables[1].GetProperty("name").GetString());
        Assert.IsTrue(localVariables[1].GetProperty("variablesReference").GetInt32() > 0);

        var closureVariablesResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 14,
                Command = "variables",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    variablesReference = closureScopeReference
                })
            },
            CancellationToken.None);

        using var closureVariablesBody = GetResponseBody(closureVariablesResult.Response);
        var closureVariables = closureVariablesBody.RootElement.GetProperty("variables").EnumerateArray().ToArray();
        Assert.AreEqual(1, closureVariables.Length);
        Assert.AreEqual("captured", closureVariables[0].GetProperty("name").GetString());
        Assert.AreEqual("from closure", closureVariables[0].GetProperty("value").GetString());

        var evaluateResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 15,
                Command = "evaluate",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    expression = "model",
                    frameId = 1,
                    context = "watch"
                })
            },
            CancellationToken.None);

        using var evaluateBody = GetResponseBody(evaluateResult.Response);
        Assert.AreEqual("Object", evaluateBody.RootElement.GetProperty("result").GetString());
        Assert.AreEqual("object", evaluateBody.RootElement.GetProperty("type").GetString());

        var evaluationReference = evaluateBody.RootElement.GetProperty("variablesReference").GetInt32();
        Assert.IsTrue(evaluationReference > 0);

        var evaluatedObjectVariablesResult = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 16,
                Command = "variables",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    variablesReference = evaluationReference
                })
            },
            CancellationToken.None);

        using var evaluatedObjectVariablesBody = GetResponseBody(evaluatedObjectVariablesResult.Response);
        var evaluatedObjectVariables = evaluatedObjectVariablesBody.RootElement.GetProperty("variables").EnumerateArray().ToArray();
        Assert.AreEqual(1, evaluatedObjectVariables.Length);
        Assert.AreEqual("label", evaluatedObjectVariables[0].GetProperty("name").GetString());
        Assert.AreEqual("counter", evaluatedObjectVariables[0].GetProperty("value").GetString());
    }

    [TestMethod]
    public async Task JazorVueHost_DapProcess_InitializeAndDisconnect_ReturnsCapabilities()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        using var process = CreateVueHostDapProcess();
        Assert.IsTrue(process.Start(), "Expected Jazor.VueHost DAP process to start.");

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 1,
                type = "request",
                command = "initialize",
                arguments = new
                {
                    adapterID = "jazor-vuehost"
                }
            },
            cancellationSource.Token);

        using var initializeResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("response", initializeResponse.RootElement.GetProperty("type").GetString());
        Assert.AreEqual("initialize", initializeResponse.RootElement.GetProperty("command").GetString());
        Assert.IsTrue(initializeResponse.RootElement.GetProperty("success").GetBoolean());
        Assert.IsTrue(
            initializeResponse.RootElement.GetProperty("body").GetProperty("supportsConfigurationDoneRequest").GetBoolean());

        using var initializedEvent = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("event", initializedEvent.RootElement.GetProperty("type").GetString());
        Assert.AreEqual("initialized", initializedEvent.RootElement.GetProperty("event").GetString());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 2,
                type = "request",
                command = "disconnect"
            },
            cancellationSource.Token);

        using var disconnectResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("disconnect", disconnectResponse.RootElement.GetProperty("command").GetString());
        Assert.IsTrue(disconnectResponse.RootElement.GetProperty("success").GetBoolean());

        await process.WaitForExitAsync(cancellationSource.Token);
        var errorOutput = await process.StandardError.ReadToEndAsync(cancellationSource.Token);
        Assert.AreEqual(0, process.ExitCode, errorOutput);
    }

    [TestMethod]
    public async Task JazorVueHost_DapProcess_EvaluateAndContinue_ReturnsFallbackResponses()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        using var process = CreateVueHostDapProcess();
        Assert.IsTrue(process.Start(), "Expected Jazor.VueHost DAP process to start.");

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 1,
                type = "request",
                command = "initialize",
                arguments = new
                {
                    adapterID = "jazor-vuehost"
                }
            },
            cancellationSource.Token);

        using var initializeResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("initialize", initializeResponse.RootElement.GetProperty("command").GetString());
        using var initializedEvent = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("initialized", initializedEvent.RootElement.GetProperty("event").GetString());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 2,
                type = "request",
                command = "evaluate",
                arguments = new
                {
                    expression = "initialized",
                    context = "repl"
                }
            },
            cancellationSource.Token);

        using var evaluateResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("evaluate", evaluateResponse.RootElement.GetProperty("command").GetString());
        Assert.IsTrue(evaluateResponse.RootElement.GetProperty("success").GetBoolean());
        Assert.AreEqual(
            "true",
            evaluateResponse.RootElement.GetProperty("body").GetProperty("result").GetString());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 3,
                type = "request",
                command = "continue",
                arguments = new
                {
                    threadId = 1
                }
            },
            cancellationSource.Token);

        using var continueResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("continue", continueResponse.RootElement.GetProperty("command").GetString());
        Assert.IsTrue(continueResponse.RootElement.GetProperty("success").GetBoolean());
        Assert.IsTrue(
            continueResponse.RootElement.GetProperty("body").GetProperty("allThreadsContinued").GetBoolean());

        using var continuedEvent = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("event", continuedEvent.RootElement.GetProperty("type").GetString());
        Assert.AreEqual("continued", continuedEvent.RootElement.GetProperty("event").GetString());
        Assert.AreEqual(1, continuedEvent.RootElement.GetProperty("body").GetProperty("threadId").GetInt32());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 4,
                type = "request",
                command = "disconnect"
            },
            cancellationSource.Token);

        using var disconnectResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("disconnect", disconnectResponse.RootElement.GetProperty("command").GetString());
        Assert.IsTrue(disconnectResponse.RootElement.GetProperty("success").GetBoolean());

        await process.WaitForExitAsync(cancellationSource.Token);
        var errorOutput = await process.StandardError.ReadToEndAsync(cancellationSource.Token);
        Assert.AreEqual(0, process.ExitCode, errorOutput);
    }

    [TestMethod]
    public async Task JazorVueHost_DapProcess_ScopesVariablesEvaluateAndContinue_FormMinimalLoop()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        using var process = CreateVueHostDapProcess();
        Assert.IsTrue(process.Start(), "Expected Jazor.VueHost DAP process to start.");

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 1,
                type = "request",
                command = "initialize",
                arguments = new
                {
                    adapterID = "jazor-vuehost"
                }
            },
            cancellationSource.Token);

        using var initializeResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("initialize", initializeResponse.RootElement.GetProperty("command").GetString());
        using var initializedEvent = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("initialized", initializedEvent.RootElement.GetProperty("event").GetString());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 2,
                type = "request",
                command = "stackTrace",
                arguments = new
                {
                    threadId = 1
                }
            },
            cancellationSource.Token);

        using var stackTraceResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        var stackFrames = stackTraceResponse.RootElement.GetProperty("body").GetProperty("stackFrames").EnumerateArray().ToArray();
        Assert.AreEqual(1, stackFrames.Length);
        Assert.AreEqual("render", stackFrames[0].GetProperty("name").GetString());

        var frameId = stackFrames[0].GetProperty("id").GetInt32();

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 3,
                type = "request",
                command = "scopes",
                arguments = new
                {
                    frameId
                }
            },
            cancellationSource.Token);

        using var scopesResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        var scopes = scopesResponse.RootElement.GetProperty("body").GetProperty("scopes").EnumerateArray().ToArray();
        Assert.AreEqual(2, scopes.Length);
        var localsReference = scopes[0].GetProperty("variablesReference").GetInt32();

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 4,
                type = "request",
                command = "variables",
                arguments = new
                {
                    variablesReference = localsReference
                }
            },
            cancellationSource.Token);

        using var localsResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        var locals = localsResponse.RootElement.GetProperty("body").GetProperty("variables").EnumerateArray().ToArray();
        Assert.AreEqual(5, locals.Length);
        Assert.AreEqual("functionName", locals[1].GetProperty("name").GetString());
        Assert.AreEqual("render", locals[1].GetProperty("value").GetString());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 5,
                type = "request",
                command = "evaluate",
                arguments = new
                {
                    expression = "location.line",
                    frameId,
                    context = "watch"
                }
            },
            cancellationSource.Token);

        using var evaluateResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("evaluate", evaluateResponse.RootElement.GetProperty("command").GetString());
        Assert.AreEqual(
            "1",
            evaluateResponse.RootElement.GetProperty("body").GetProperty("result").GetString());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 6,
                type = "request",
                command = "continue",
                arguments = new
                {
                    threadId = 1
                }
            },
            cancellationSource.Token);

        using var continueResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("continue", continueResponse.RootElement.GetProperty("command").GetString());
        Assert.IsTrue(
            continueResponse.RootElement.GetProperty("body").GetProperty("allThreadsContinued").GetBoolean());

        using var continuedEvent = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("continued", continuedEvent.RootElement.GetProperty("event").GetString());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 7,
                type = "request",
                command = "stackTrace",
                arguments = new
                {
                    threadId = 1
                }
            },
            cancellationSource.Token);

        using var postContinueStackTraceResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual(
            0,
            postContinueStackTraceResponse.RootElement.GetProperty("body").GetProperty("totalFrames").GetInt32());

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 8,
                type = "request",
                command = "disconnect"
            },
            cancellationSource.Token);

        using var disconnectResponse = await ReadDapMessageAsync(process, cancellationSource.Token);
        Assert.AreEqual("disconnect", disconnectResponse.RootElement.GetProperty("command").GetString());

        await process.WaitForExitAsync(cancellationSource.Token);
        var errorOutput = await process.StandardError.ReadToEndAsync(cancellationSource.Token);
        Assert.AreEqual(0, process.ExitCode, errorOutput);
    }

    private static DapRequestHandler CreateHandler(
        ISourceMapService sourceMapService,
        out DapSession session,
        ICdpClient? cdpClient = null)
    {
        session = new DapSession(cdpClient);
        return new DapRequestHandler(
            session,
            new BreakpointManager(sourceMapService),
            new CallStackMapper(sourceMapService));
    }

    private static JsonDocument GetResponseBody(DapResponse response)
    {
        var responseJson = DapProtocolSerializer.Serialize(response);
        using var responseDocument = JsonDocument.Parse(responseJson);
        return JsonDocument.Parse(responseDocument.RootElement.GetProperty("body").GetRawText());
    }

    private static Process CreateVueHostDapProcess()
    {
        var hostAssemblyPath = ResolveDapHostAssemblyPath();

        Assert.IsTrue(File.Exists(hostAssemblyPath), $"Expected Jazor.VueHost assembly to exist at '{hostAssemblyPath}'.");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(hostAssemblyPath);
        startInfo.ArgumentList.Add("--dap");
        startInfo.ArgumentList.Add("--stdio");

        return new Process
        {
            StartInfo = startInfo
        };
    }

    private static string ResolveDapHostAssemblyPath()
    {
        var configuredPath = Environment.GetEnvironmentVariable("JAZOR_VUEHOST_DAP_HOST_DLL");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        if (TryGetRepositoryRoot(out var repositoryRootForConfiguration))
        {
            var currentConfiguration = TryGetCurrentBuildConfiguration();
            if (!string.IsNullOrWhiteSpace(currentConfiguration))
            {
                var hostPathForCurrentConfiguration = Path.Combine(
                    repositoryRootForConfiguration,
                    "src",
                    "Jazor.VueHost",
                    "bin",
                    currentConfiguration,
                    "net10.0",
                    "Jazor.VueHost.dll");
                var depsPathForCurrentConfiguration = Path.Combine(
                    repositoryRootForConfiguration,
                    "src",
                    "Jazor.VueHost",
                    "bin",
                    currentConfiguration,
                    "net10.0",
                    "Jazor.VueHost.deps.json");
                if (File.Exists(hostPathForCurrentConfiguration) && File.Exists(depsPathForCurrentConfiguration))
                {
                    return hostPathForCurrentConfiguration;
                }
            }
        }

        var siblingHostPath = Path.Combine(AppContext.BaseDirectory, "Jazor.VueHost.dll");
        var siblingDepsPath = Path.Combine(AppContext.BaseDirectory, "Jazor.VueHost.deps.json");
        if (File.Exists(siblingHostPath) && File.Exists(siblingDepsPath))
        {
            return siblingHostPath;
        }

        if (TryGetRepositoryRoot(out var repositoryRoot))
        {
            var defaultHostPath = Path.Combine(
                repositoryRoot,
                "src",
                "Jazor.VueHost",
                "bin",
                "Debug",
                "net10.0",
                "Jazor.VueHost.dll");
            if (File.Exists(defaultHostPath))
            {
                return defaultHostPath;
            }
        }

        return typeof(DapSession).Assembly.Location;
    }

    private static string? TryGetCurrentBuildConfiguration()
    {
        var runtimeDirectory = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var configurationDirectory = runtimeDirectory.Parent;
        if (configurationDirectory is null || string.IsNullOrWhiteSpace(configurationDirectory.Name))
        {
            return null;
        }

        return configurationDirectory.Name;
    }

    private static bool TryGetRepositoryRoot(out string repositoryRoot)
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
            {
                repositoryRoot = current.FullName;
                return true;
            }
        }

        repositoryRoot = string.Empty;
        return false;
    }

    private static async Task WriteDapMessageAsync(
        Stream output,
        object payload,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var body = Encoding.UTF8.GetBytes(json);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        await output.WriteAsync(header, cancellationToken);
        await output.WriteAsync(body, cancellationToken);
        await output.FlushAsync(cancellationToken);
    }

    private static async Task<JsonDocument> ReadDapMessageAsync(
        Process process,
        CancellationToken cancellationToken)
    {
        var contentLength = await ReadContentLengthAsync(process, cancellationToken);
        var buffer = new byte[contentLength];
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await process.StandardOutput.BaseStream.ReadAsync(
                buffer.AsMemory(offset, buffer.Length - offset),
                cancellationToken);
            if (read == 0)
            {
                var stderr = process.HasExited
                    ? await process.StandardError.ReadToEndAsync(cancellationToken)
                    : string.Empty;
                throw new EndOfStreamException("Unexpected end of stream while reading DAP body. stderr: " + stderr);
            }

            offset += read;
        }

        return JsonDocument.Parse(buffer);
    }

    private static async Task<int> ReadContentLengthAsync(
        Process process,
        CancellationToken cancellationToken)
    {
        var headerBytes = new List<byte>();
        var buffer = new byte[1];
        while (true)
        {
            var read = await process.StandardOutput.BaseStream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
            if (read == 0)
            {
                var stderr = process.HasExited
                    ? await process.StandardError.ReadToEndAsync(cancellationToken)
                    : string.Empty;
                throw new EndOfStreamException("Unexpected end of stream while reading DAP headers. stderr: " + stderr);
            }

            headerBytes.Add(buffer[0]);
            var count = headerBytes.Count;
            if (count >= 4
                && headerBytes[count - 4] == '\r'
                && headerBytes[count - 3] == '\n'
                && headerBytes[count - 2] == '\r'
                && headerBytes[count - 1] == '\n')
            {
                break;
            }
        }

        var headerText = Encoding.ASCII.GetString(headerBytes.ToArray());
        foreach (var line in headerText.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return int.Parse(
                line["Content-Length:".Length..].Trim(),
                System.Globalization.CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException("Expected Content-Length header in DAP response.");
    }

    private sealed class FakeCdpClient : ICdpClient
    {
        private readonly Dictionary<(string Url, int Line, int Column), CdpBreakpointResolution?> _breakpointResolutions = [];
        private readonly Dictionary<(string? CallFrameId, string Expression), CdpRemoteObject> _evaluations = [];
        private readonly Dictionary<string, IReadOnlyList<CdpPropertyDescriptor>> _propertiesByObjectId = new(StringComparer.Ordinal);

        public IReadOnlyList<CdpCallFrame> LatestCallFrames { get; private set; } = [];

        public event Action<IReadOnlyList<CdpCallFrame>>? Paused;

        public event Action? Resumed;

        public List<BreakpointRequest> BreakpointRequests { get; } = [];

        public void SetEvaluationResult(string? callFrameId, string expression, CdpRemoteObject remoteObject)
            => _evaluations[(callFrameId, expression)] = remoteObject;

        public void SetProperties(string objectId, params CdpPropertyDescriptor[] properties)
            => _propertiesByObjectId[objectId] = properties;

        public void SetBreakpointResolution(
            string generatedUrl,
            int generatedLine,
            int generatedColumn,
            CdpBreakpointResolution? resolution)
            => _breakpointResolutions[(generatedUrl, generatedLine, generatedColumn)] = resolution;

        public void EmitPaused(IReadOnlyList<CdpCallFrame> callFrames)
        {
            LatestCallFrames = callFrames;
            Paused?.Invoke(callFrames);
        }

        public Task ContinueAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LatestCallFrames = [];
            Resumed?.Invoke();
            return Task.CompletedTask;
        }

        public Task<CdpBreakpointResolution?> SetBreakpointByUrlAsync(
            string generatedUrl,
            int generatedLine,
            int generatedColumn,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            BreakpointRequests.Add(new BreakpointRequest(generatedUrl, generatedLine, generatedColumn));

            if (_breakpointResolutions.TryGetValue((generatedUrl, generatedLine, generatedColumn), out var resolution))
            {
                return Task.FromResult(resolution);
            }

            return Task.FromResult<CdpBreakpointResolution?>(null);
        }

        public Task<CdpRemoteObject?> EvaluateAsync(
            string expression,
            string? callFrameId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_evaluations.TryGetValue((callFrameId, expression), out var remoteObject))
            {
                return Task.FromResult<CdpRemoteObject?>(remoteObject);
            }

            if (_evaluations.TryGetValue((null, expression), out remoteObject))
            {
                return Task.FromResult<CdpRemoteObject?>(remoteObject);
            }

            return Task.FromResult<CdpRemoteObject?>(null);
        }

        public Task<IReadOnlyList<CdpPropertyDescriptor>> GetPropertiesAsync(
            string objectId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                _propertiesByObjectId.TryGetValue(objectId, out var properties)
                    ? properties
                    : (IReadOnlyList<CdpPropertyDescriptor>)[]);
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;

        public readonly record struct BreakpointRequest(
            string GeneratedUrl,
            int GeneratedLine,
            int GeneratedColumn);
    }
}
