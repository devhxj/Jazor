using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Jolt.Debug;
using Jazor.SourceMaps;
using Jolt.SourceMap;
using static Jolt.Test.SourceMapTestHelpers;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDebugProtocolTests
{
    private const string RealCdpHmrStressEnvironmentVariable = "JOLT_RUN_REAL_CDP_HMR_STRESS";
    private const string RealCdpSourceMapMatrixEnvironmentVariable = "JOLT_RUN_REAL_CDP_SOURCE_MAP_MATRIX";
    private const string RealCdpBrowserPathEnvironmentVariable = "JOLT_REAL_BROWSER_PATH";

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
    public async Task DapRequestHandler_HandleAsync_SetBreakpoints_UsesColumnAwareSourceMapSegment()
    {
        var sourceText = """
            <button>Count</button>
            @code {
                private int Count = 1;
            }
            """;
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceColumnMap(
                "Counter.jazor",
                sourceText,
                (0, 0, 0, 0),
                (0, 8, 1, 8)));
        var cdpClient = new FakeCdpClient();
        cdpClient.SetBreakpointResolution(
            "/Counter.jazor",
            0,
            8,
            new CdpBreakpointResolution(
                "bp-cdp-column",
                new CdpLocation("/virtual/Counter.generated.js", 4, 11)));
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
                        new { line = 2, column = 9 }
                    }
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        var breakpoints = body.RootElement.GetProperty("breakpoints").EnumerateArray().ToArray();
        Assert.AreEqual(1, breakpoints.Length);
        Assert.IsTrue(breakpoints[0].GetProperty("verified").GetBoolean());
        Assert.AreEqual(9, breakpoints[0].GetProperty("column").GetInt32());

        var bindings = session.GetBreakpoints(@"D:\repo\Counter.jazor");
        Assert.AreEqual(1, bindings.Count);
        Assert.IsNotNull(bindings[0].GeneratedBreakpoint);
        Assert.AreEqual("/virtual/Counter.generated.js", bindings[0].GeneratedBreakpoint!.GeneratedPath);
        Assert.AreEqual(4, bindings[0].GeneratedBreakpoint!.GeneratedLine);
        Assert.AreEqual(11, bindings[0].GeneratedBreakpoint!.GeneratedColumn);

        Assert.AreEqual(1, cdpClient.BreakpointRequests.Count);
        Assert.AreEqual("/Counter.jazor", cdpClient.BreakpointRequests[0].GeneratedUrl);
        Assert.AreEqual(0, cdpClient.BreakpointRequests[0].GeneratedLine);
        Assert.AreEqual(8, cdpClient.BreakpointRequests[0].GeneratedColumn);
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
    public async Task DapRequestHandler_HandleAsync_SetBreakpoints_WithNonScalarSourcePath_ReturnsEmptyBreakpoints()
    {
        var handler = CreateHandler(new InMemorySourceMapService(), out _);

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 102,
                Command = "setBreakpoints",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    source = new
                    {
                        path = new
                        {
                            unexpected = true
                        }
                    },
                    breakpoints = new[]
                    {
                        new { line = 1 }
                    }
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        var breakpoints = body.RootElement.GetProperty("breakpoints");
        Assert.AreEqual(JsonValueKind.Array, breakpoints.ValueKind);
        Assert.AreEqual(0, breakpoints.GetArrayLength());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_SetBreakpoints_ConcurrentRequests_ProduceUniqueBreakpointIds()
    {
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceLineMap(
                "Counter.jazor",
                "line0\nline1\nline2",
                [1]));
        var handler = CreateHandler(sourceMapService, out _);

        const int requestCount = 48;
        var startGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = Enumerable.Range(0, requestCount)
            .Select(index => CreateSetBreakpointsRequest(seq: 500 + index))
            .ToArray();
        var tasks = requests
            .Select(async request =>
            {
                await startGate.Task;
                return await handler.HandleAsync(request, CancellationToken.None);
            })
            .ToArray();

        startGate.SetResult();
        var results = await Task.WhenAll(tasks);

        var breakpointIds = new List<int>(requestCount);
        foreach (var result in results)
        {
            Assert.IsTrue(result.Response.Success);
            using var body = GetResponseBody(result.Response);
            var breakpoints = body.RootElement.GetProperty("breakpoints").EnumerateArray().ToArray();
            Assert.AreEqual(1, breakpoints.Length);
            Assert.IsTrue(breakpoints[0].GetProperty("verified").GetBoolean());
            breakpointIds.Add(breakpoints[0].GetProperty("id").GetInt32());
        }

        Assert.AreEqual(requestCount, breakpointIds.Count);
        Assert.AreEqual(requestCount, breakpointIds.Distinct().Count());
        CollectionAssert.AreEqual(
            Enumerable.Range(1, requestCount).ToArray(),
            breakpointIds.OrderBy(static id => id).ToArray());
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
    public async Task DapRequestHandler_HandleAsync_StackTrace_MapsSessionFrameColumns()
    {
        var sourceText = """
            <button>Count</button>

            @code {
                private int Count = 1;
                public void Increment()
                {
                    Count++;
                }
            }
            """;
        var sourceLine = GetLineIndexContaining(sourceText, "Count++;");
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceColumnMap(
                "Counter.jazor",
                sourceText,
                (0, 0, sourceLine - 1, 4),
                (1, 4, sourceLine, 8)));
        var handler = CreateHandler(sourceMapService, out var session);
        session.CurrentCallFrames =
        [
            new CdpCallFrame(
                "frame-1",
                "increment",
                new CdpLocation("/Counter.jazor", 1, 4))
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
        Assert.AreEqual(GetLineIndexContaining(sourceText, "Count++;") + 1, frames[0].GetProperty("line").GetInt32());
        Assert.AreEqual(9, frames[0].GetProperty("column").GetInt32());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_StackTrace_ExceptionStyleFrames_KeepMappedAndUnmappedOrder()
    {
        var sourceText = """
            <button>Count</button>

            @code {
                private int Count = 1;
                public void Increment()
                {
                    throw new InvalidOperationException();
                }
            }
            """;
        var throwLine = GetLineIndexContaining(sourceText, "throw new InvalidOperationException();");
        var incrementLine = GetLineIndexContaining(sourceText, "public void Increment()");
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceColumnMap(
                "Counter.jazor",
                sourceText,
                (0, 0, incrementLine, 16),
                (1, 4, throwLine, 8)));
        var handler = CreateHandler(sourceMapService, out var session);
        session.CurrentCallFrames =
        [
            new CdpCallFrame(
                "frame-throw",
                "Increment",
                new CdpLocation("/Counter.jazor", 1, 4)),
            new CdpCallFrame(
                "frame-anonymous",
                string.Empty,
                new CdpLocation("/Counter.jazor", 0, 0)),
            new CdpCallFrame(
                "frame-runtime",
                "Promise.then",
                new CdpLocation("/runtime/internal.js", 20, 2))
        ];

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 6,
                Command = "stackTrace",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    threadId = 1
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        var frames = body.RootElement.GetProperty("stackFrames").EnumerateArray().ToArray();
        Assert.AreEqual(3, frames.Length);

        Assert.AreEqual("Increment", frames[0].GetProperty("name").GetString());
        Assert.AreEqual("Counter.jazor", frames[0].GetProperty("source").GetProperty("path").GetString());
        Assert.AreEqual(throwLine + 1, frames[0].GetProperty("line").GetInt32());

        Assert.AreEqual("(anonymous)", frames[1].GetProperty("name").GetString());
        Assert.AreEqual("Counter.jazor", frames[1].GetProperty("source").GetProperty("path").GetString());
        Assert.AreEqual(incrementLine + 1, frames[1].GetProperty("line").GetInt32());

        Assert.AreEqual("Promise.then", frames[2].GetProperty("name").GetString());
        Assert.AreEqual("/runtime/internal.js", frames[2].GetProperty("source").GetProperty("path").GetString());
        Assert.AreEqual(21, frames[2].GetProperty("line").GetInt32());
        Assert.AreEqual(3, frames[2].GetProperty("column").GetInt32());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_StackTrace_PaginatesMixedFrames_WithStableTotalFrames()
    {
        var sourceText = """
            <button>Count</button>

            @code {
                private int Count = 1;
                public void Increment()
                {
                    Count++;
                    Count += 2;
                }
            }
            """;
        var incrementLine = GetLineIndexContaining(sourceText, "Count++;");
        var incrementByTwoLine = GetLineIndexContaining(sourceText, "Count += 2;");
        var sourceMapService = new InMemorySourceMapService();
        sourceMapService.Register(
            "/Counter.jazor",
            CreateSingleSourceColumnMap(
                "Counter.jazor",
                sourceText,
                (0, 0, incrementLine, 8),
                (1, 8, incrementByTwoLine, 12)));
        var handler = CreateHandler(sourceMapService, out var session);
        session.CurrentCallFrames =
        [
            new CdpCallFrame(
                "frame-mapped-head",
                "Increment",
                new CdpLocation("/Counter.jazor", 0, 0)),
            new CdpCallFrame(
                "frame-runtime",
                "Promise.then",
                new CdpLocation("/runtime/internal.js", 20, 2)),
            new CdpCallFrame(
                "frame-mapped-tail",
                "IncrementTail",
                new CdpLocation("/Counter.jazor", 1, 8)),
            new CdpCallFrame(
                "frame-vendor",
                "vendorTick",
                new CdpLocation("/vendor/chunk.js", 8, 0))
        ];

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 7,
                Command = "stackTrace",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    threadId = 1,
                    startFrame = 1,
                    levels = 2
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        Assert.AreEqual(4, body.RootElement.GetProperty("totalFrames").GetInt32());

        var frames = body.RootElement.GetProperty("stackFrames").EnumerateArray().ToArray();
        Assert.AreEqual(2, frames.Length);

        Assert.AreEqual("Promise.then", frames[0].GetProperty("name").GetString());
        Assert.AreEqual("/runtime/internal.js", frames[0].GetProperty("source").GetProperty("path").GetString());
        Assert.AreEqual(21, frames[0].GetProperty("line").GetInt32());
        Assert.AreEqual(3, frames[0].GetProperty("column").GetInt32());

        Assert.AreEqual("IncrementTail", frames[1].GetProperty("name").GetString());
        Assert.AreEqual("Counter.jazor", frames[1].GetProperty("source").GetProperty("path").GetString());
        Assert.AreEqual(incrementByTwoLine + 1, frames[1].GetProperty("line").GetInt32());
        Assert.AreEqual(13, frames[1].GetProperty("column").GetInt32());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_StackTrace_OutOfRangeStartFrame_ReturnsEmptyFramesAndStableTotalFrames()
    {
        var handler = CreateHandler(new InMemorySourceMapService(), out var session);
        session.CurrentCallFrames =
        [
            new CdpCallFrame("frame-1", "render", new CdpLocation("/Counter.jazor", 1, 0)),
            new CdpCallFrame("frame-2", "tick", new CdpLocation("/runtime/internal.js", 8, 2)),
            new CdpCallFrame("frame-3", "flush", new CdpLocation("/runtime/internal.js", 10, 1))
        ];

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 8,
                Command = "stackTrace",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    threadId = 1,
                    startFrame = 99,
                    levels = 5
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        Assert.AreEqual(3, body.RootElement.GetProperty("totalFrames").GetInt32());
        Assert.AreEqual(0, body.RootElement.GetProperty("stackFrames").GetArrayLength());
    }

    [TestMethod]
    public async Task DapRequestHandler_HandleAsync_StackTrace_ClampsNegativeStartAndTreatsNonPositiveLevelsAsAllRemaining()
    {
        var handler = CreateHandler(new InMemorySourceMapService(), out var session);
        session.CurrentCallFrames =
        [
            new CdpCallFrame("frame-1", "render", new CdpLocation("/Counter.jazor", 1, 0)),
            new CdpCallFrame("frame-2", "tick", new CdpLocation("/runtime/internal.js", 8, 2))
        ];

        var result = await handler.HandleAsync(
            new DapRequest
            {
                Seq = 9,
                Command = "stackTrace",
                Arguments = JsonSerializer.SerializeToElement(new
                {
                    threadId = 1,
                    startFrame = -12,
                    levels = -1
                })
            },
            CancellationToken.None);

        Assert.IsTrue(result.Response.Success);
        using var body = GetResponseBody(result.Response);
        Assert.AreEqual(2, body.RootElement.GetProperty("totalFrames").GetInt32());

        var frames = body.RootElement.GetProperty("stackFrames").EnumerateArray().ToArray();
        Assert.AreEqual(2, frames.Length);
        Assert.AreEqual("render", frames[0].GetProperty("name").GetString());
        Assert.AreEqual("tick", frames[1].GetProperty("name").GetString());
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
    public async Task Jolt_DapProcess_InitializeAndDisconnect_ReturnsCapabilities()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        using var process = CreateJoltDapProcess();
        Assert.IsTrue(process.Start(), "Expected Jolt DAP process to start.");

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 1,
                type = "request",
                command = "initialize",
                arguments = new
                {
                    adapterID = "jolt"
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
    public async Task Jolt_DapProcess_EvaluateAndContinue_ReturnsFallbackResponses()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        using var process = CreateJoltDapProcess();
        Assert.IsTrue(process.Start(), "Expected Jolt DAP process to start.");

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 1,
                type = "request",
                command = "initialize",
                arguments = new
                {
                    adapterID = "jolt"
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
    public async Task Jolt_DapProcess_ScopesVariablesEvaluateAndContinue_FormMinimalLoop()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        using var process = CreateJoltDapProcess();
        Assert.IsTrue(process.Start(), "Expected Jolt DAP process to start.");

        await WriteDapMessageAsync(
            process.StandardInput.BaseStream,
            new
            {
                seq = 1,
                type = "request",
                command = "initialize",
                arguments = new
                {
                    adapterID = "jolt"
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

    [TestMethod]
    public async Task Jolt_DapProcess_RealBrowserCdpAndHmrStress_PreservesMappedBreakpointAcrossHotUpdates()
    {
        if (!ReadBooleanEnvironmentVariable(RealCdpHmrStressEnvironmentVariable))
        {
            return;
        }

        var browserExecutablePath = ResolveRealBrowserExecutablePath();
        if (string.IsNullOrWhiteSpace(browserExecutablePath))
        {
            Assert.Inconclusive(
                $"Unable to locate browser executable. Set {RealCdpBrowserPathEnvironmentVariable} to Chrome/Edge path.");
        }

        var devRoot = CreateTemporaryDirectory();
        var browserProfilePath = Path.Combine(devRoot, ".browser-profile");
        Directory.CreateDirectory(browserProfilePath);

        var indexPath = Path.Combine(devRoot, "index.html");
        var modulePath = Path.Combine(devRoot, "main.ts");
        var moduleVersion1 = CreateRealCdpStressMainModule(1);
        await File.WriteAllTextAsync(
            indexPath,
            """
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <title>Jolt CDP Stress</title>
            </head>
            <body>
              <div id="app">ready</div>
              <script type="module" src="/main.ts"></script>
            </body>
            </html>
            """,
            CancellationToken.None);
        await File.WriteAllTextAsync(modulePath, moduleVersion1, CancellationToken.None);

        var breakpointNeedle = "const marker = buildVersion + MarkerState.Active;";
        var expectedBreakpointLine = GetLineIndexContaining(moduleVersion1, breakpointNeedle) + 1;
        var expectedBreakpointColumn = GetSourceColumn(moduleVersion1, breakpointNeedle);
        var devPort = AllocateLoopbackTcpPort();
        var cdpPort = AllocateLoopbackTcpPort();
        const int iterations = 8;

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(4));
        Process? browserProcess = null;
        Process? hostProcess = null;

        try
        {
            browserProcess = CreateHeadlessBrowserProcess(
                browserExecutablePath,
                cdpPort,
                browserProfilePath);
            Assert.IsTrue(browserProcess.Start(), "Expected headless browser process to start.");

            var cdpWebSocket = await WaitForBrowserCdpPageEndpointAsync(cdpPort, cancellationSource.Token);
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(cdpWebSocket),
                "Expected browser to expose a page-level CDP WebSocket endpoint.");

            hostProcess = CreateJoltDapDevProcess(
                devRoot,
                devPort,
                cdpWebSocket!);
            Assert.IsTrue(hostProcess.Start(), "Expected Jolt DAP+Dev process to start.");

            await WaitForHttpReadyAsync(
                new Uri($"http://127.0.0.1:{devPort}/index.html"),
                cancellationSource.Token);

            var sequence = new DapSequenceCounter();

            using var initializeResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "initialize",
                new
                {
                    adapterID = "jolt"
                },
                cancellationSource.Token);
            Assert.AreEqual("initialize", initializeResponse.RootElement.GetProperty("command").GetString());
            Assert.IsTrue(
                initializeResponse.RootElement.GetProperty("body").GetProperty("supportsConfigurationDoneRequest").GetBoolean());

            using var initializedEvent = await ReadDapMessageAsync(hostProcess, cancellationSource.Token);
            Assert.AreEqual("initialized", initializedEvent.RootElement.GetProperty("event").GetString());

            using var configurationDoneResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "configurationDone",
                null,
                cancellationSource.Token);
            Assert.IsTrue(configurationDoneResponse.RootElement.GetProperty("success").GetBoolean());

            var navigateExpression = "location.href = "
                + JsonSerializer.Serialize($"http://127.0.0.1:{devPort}/index.html")
                + "; 'navigating';";
            using var navigateResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "evaluate",
                new
                {
                    expression = navigateExpression,
                    context = "repl"
                },
                cancellationSource.Token);
            Assert.IsTrue(navigateResponse.RootElement.GetProperty("success").GetBoolean());

            await WaitForIterationReadyAsync(
                hostProcess,
                sequence,
                expectedVersion: 1,
                cancellationSource.Token);

            using var setBreakpointsResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "setBreakpoints",
                new
                {
                    source = new
                    {
                        path = modulePath
                    },
                    breakpoints = new object[]
                    {
                        new
                        {
                            line = expectedBreakpointLine,
                            column = expectedBreakpointColumn
                        }
                    }
                },
                cancellationSource.Token);
            Assert.IsTrue(setBreakpointsResponse.RootElement.GetProperty("success").GetBoolean());
            var verifiedBreakpoints = setBreakpointsResponse.RootElement
                .GetProperty("body")
                .GetProperty("breakpoints")
                .EnumerateArray()
                .ToArray();
            Assert.AreEqual(1, verifiedBreakpoints.Length);
            Assert.IsTrue(
                verifiedBreakpoints[0].GetProperty("verified").GetBoolean(),
                "Breakpoint binding failed: " + verifiedBreakpoints[0].GetRawText());

            for (var iteration = 1; iteration <= iterations; iteration++)
            {
                if (iteration > 1)
                {
                    var updatedSource = CreateRealCdpStressMainModule(iteration);
                    await File.WriteAllTextAsync(modulePath, updatedSource, cancellationSource.Token);
                    await WaitForIterationReadyAsync(
                        hostProcess,
                        sequence,
                        expectedVersion: iteration,
                        cancellationSource.Token);
                }

                using var triggerResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "evaluate",
                    new
                    {
                        expression = "setTimeout(() => globalThis.__jazorRunIteration?.(), 0); 'scheduled';",
                        context = "repl"
                    },
                    cancellationSource.Token);
                Assert.IsTrue(triggerResponse.RootElement.GetProperty("success").GetBoolean());

                var pausedFrame = await WaitForPausedFrameAsync(hostProcess, sequence, cancellationSource.Token);
                var sourcePath = pausedFrame.GetProperty("source").GetProperty("path").GetString() ?? string.Empty;
                Assert.AreEqual("main.ts", Path.GetFileName(sourcePath));
                Assert.AreEqual(expectedBreakpointLine, pausedFrame.GetProperty("line").GetInt32());
                var pausedColumn = pausedFrame.GetProperty("column").GetInt32();
                Assert.IsTrue(
                    pausedColumn >= expectedBreakpointColumn,
                    $"Expected mapped column >= {expectedBreakpointColumn}, actual {pausedColumn}.");

                var frameId = pausedFrame.GetProperty("id").GetInt32();
                using var scopesResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "scopes",
                    new
                    {
                        frameId
                    },
                    cancellationSource.Token);
                var scopes = scopesResponse.RootElement
                    .GetProperty("body")
                    .GetProperty("scopes")
                    .EnumerateArray()
                    .ToArray();
                Assert.IsTrue(scopes.Length >= 2, "Expected at least one runtime scope plus the session scope.");

                var runtimeScopeReference = scopes
                    .Where(scope =>
                        scope.TryGetProperty("variablesReference", out var referenceElement)
                        && referenceElement.GetInt32() > 0
                        && !string.Equals(scope.GetProperty("name").GetString(), "Session", StringComparison.OrdinalIgnoreCase))
                    .Select(scope => scope.GetProperty("variablesReference").GetInt32())
                    .DefaultIfEmpty(0)
                    .First();
                Assert.IsTrue(runtimeScopeReference > 0, "Expected runtime scope to expose variablesReference.");

                using var variablesResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "variables",
                    new
                    {
                        variablesReference = runtimeScopeReference
                    },
                    cancellationSource.Token);
                var runtimeVariablesElement = variablesResponse.RootElement
                    .GetProperty("body")
                    .GetProperty("variables");
                Assert.AreEqual(JsonValueKind.Array, runtimeVariablesElement.ValueKind);

                using var evaluateBuildVersionResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "evaluate",
                    new
                    {
                        expression = "buildVersion",
                        frameId,
                        context = "watch"
                    },
                    cancellationSource.Token);
                var evaluatedBuildVersion = evaluateBuildVersionResponse.RootElement
                    .GetProperty("body")
                    .GetProperty("result")
                    .GetString();
                Assert.AreEqual(
                    iteration.ToString(),
                    evaluatedBuildVersion,
                    $"Expected paused-frame evaluate(buildVersion) to stay aligned with HMR iteration {iteration}.");

                using var continueResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "continue",
                    new
                    {
                        threadId = 1
                    },
                    cancellationSource.Token);
                Assert.IsTrue(continueResponse.RootElement.GetProperty("success").GetBoolean());

                using var continuedEvent = await ReadDapMessageAsync(hostProcess, cancellationSource.Token);
                Assert.AreEqual("continued", continuedEvent.RootElement.GetProperty("event").GetString());

                await WaitForStackToClearAsync(hostProcess, sequence, cancellationSource.Token);
            }

            using var disconnectResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "disconnect",
                null,
                cancellationSource.Token);
            Assert.AreEqual("disconnect", disconnectResponse.RootElement.GetProperty("command").GetString());

            await hostProcess.WaitForExitAsync(cancellationSource.Token);
            var hostStandardError = await hostProcess.StandardError.ReadToEndAsync(cancellationSource.Token);
            Assert.AreEqual(0, hostProcess.ExitCode, hostStandardError);
        }
        finally
        {
            await EnsureProcessTerminatedAsync(hostProcess, cancellationSource.Token);
            await EnsureProcessTerminatedAsync(browserProcess, cancellationSource.Token);
            TryDeleteDirectoryWithRetry(devRoot);
        }
    }

    [TestMethod]
    public async Task Jolt_DapProcess_RealBrowserCdpSourceMapMatrix_ValidatesStackTracePaginationAcrossHmrIterations()
    {
        if (!ReadBooleanEnvironmentVariable(RealCdpSourceMapMatrixEnvironmentVariable))
        {
            return;
        }

        var browserExecutablePath = ResolveRealBrowserExecutablePath();
        if (string.IsNullOrWhiteSpace(browserExecutablePath))
        {
            Assert.Inconclusive(
                $"Unable to locate browser executable. Set {RealCdpBrowserPathEnvironmentVariable} to Chrome/Edge path.");
        }

        var devRoot = CreateTemporaryDirectory();
        var browserProfilePath = Path.Combine(devRoot, ".browser-profile");
        Directory.CreateDirectory(browserProfilePath);

        var indexPath = Path.Combine(devRoot, "index.html");
        var modulePath = Path.Combine(devRoot, "main.ts");
        var moduleVersion1 = CreateRealCdpSourceMapMatrixMainModule(1);
        await File.WriteAllTextAsync(
            indexPath,
            """
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <title>Jolt CDP SourceMap Matrix</title>
            </head>
            <body>
              <div id="app">ready</div>
              <script type="module" src="/main.ts"></script>
            </body>
            </html>
            """,
            CancellationToken.None);
        await File.WriteAllTextAsync(modulePath, moduleVersion1, CancellationToken.None);

        var breakpointNeedle = "const marker = buildVersion + depth + MarkerState.Active;";
        var expectedBreakpointLine = GetLineIndexContaining(moduleVersion1, breakpointNeedle) + 1;
        var expectedBreakpointColumn = GetSourceColumn(moduleVersion1, breakpointNeedle);
        var devPort = AllocateLoopbackTcpPort();
        var cdpPort = AllocateLoopbackTcpPort();
        const int iterations = 4;
        const int recursionDepth = 6;

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(4));
        Process? browserProcess = null;
        Process? hostProcess = null;

        try
        {
            browserProcess = CreateHeadlessBrowserProcess(
                browserExecutablePath,
                cdpPort,
                browserProfilePath);
            Assert.IsTrue(browserProcess.Start(), "Expected headless browser process to start.");

            var cdpWebSocket = await WaitForBrowserCdpPageEndpointAsync(cdpPort, cancellationSource.Token);
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(cdpWebSocket),
                "Expected browser to expose a page-level CDP WebSocket endpoint.");

            hostProcess = CreateJoltDapDevProcess(
                devRoot,
                devPort,
                cdpWebSocket!);
            Assert.IsTrue(hostProcess.Start(), "Expected Jolt DAP+Dev process to start.");

            await WaitForHttpReadyAsync(
                new Uri($"http://127.0.0.1:{devPort}/index.html"),
                cancellationSource.Token);

            var sequence = new DapSequenceCounter();

            using var initializeResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "initialize",
                new
                {
                    adapterID = "jolt"
                },
                cancellationSource.Token);
            Assert.AreEqual("initialize", initializeResponse.RootElement.GetProperty("command").GetString());
            Assert.IsTrue(
                initializeResponse.RootElement.GetProperty("body").GetProperty("supportsConfigurationDoneRequest").GetBoolean());

            using var initializedEvent = await ReadDapMessageAsync(hostProcess, cancellationSource.Token);
            Assert.AreEqual("initialized", initializedEvent.RootElement.GetProperty("event").GetString());

            using var configurationDoneResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "configurationDone",
                null,
                cancellationSource.Token);
            Assert.IsTrue(configurationDoneResponse.RootElement.GetProperty("success").GetBoolean());

            var navigateExpression = "location.href = "
                + JsonSerializer.Serialize($"http://127.0.0.1:{devPort}/index.html")
                + "; 'navigating';";
            using var navigateResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "evaluate",
                new
                {
                    expression = navigateExpression,
                    context = "repl"
                },
                cancellationSource.Token);
            Assert.IsTrue(navigateResponse.RootElement.GetProperty("success").GetBoolean());

            await WaitForIterationReadyAsync(
                hostProcess,
                sequence,
                expectedVersion: 1,
                cancellationSource.Token);

            using var setBreakpointsResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "setBreakpoints",
                new
                {
                    source = new
                    {
                        path = modulePath
                    },
                    breakpoints = new object[]
                    {
                        new
                        {
                            line = expectedBreakpointLine,
                            column = expectedBreakpointColumn
                        }
                    }
                },
                cancellationSource.Token);
            Assert.IsTrue(setBreakpointsResponse.RootElement.GetProperty("success").GetBoolean());
            var verifiedBreakpoints = setBreakpointsResponse.RootElement
                .GetProperty("body")
                .GetProperty("breakpoints")
                .EnumerateArray()
                .ToArray();
            Assert.AreEqual(1, verifiedBreakpoints.Length);
            Assert.IsTrue(
                verifiedBreakpoints[0].GetProperty("verified").GetBoolean(),
                "Breakpoint binding failed: " + verifiedBreakpoints[0].GetRawText());

            for (var iteration = 1; iteration <= iterations; iteration++)
            {
                if (iteration > 1)
                {
                    var updatedSource = CreateRealCdpSourceMapMatrixMainModule(iteration);
                    await File.WriteAllTextAsync(modulePath, updatedSource, cancellationSource.Token);
                    await WaitForIterationReadyAsync(
                        hostProcess,
                        sequence,
                        expectedVersion: iteration,
                        cancellationSource.Token);
                }

                using var triggerResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "evaluate",
                    new
                    {
                        expression = $"setTimeout(() => globalThis.__jazorRunIteration?.({recursionDepth}), 0); 'scheduled';",
                        context = "repl"
                    },
                    cancellationSource.Token);
                Assert.IsTrue(triggerResponse.RootElement.GetProperty("success").GetBoolean());

                var pausedFrame = await WaitForPausedFrameAsync(hostProcess, sequence, cancellationSource.Token);
                var sourcePath = pausedFrame.GetProperty("source").GetProperty("path").GetString() ?? string.Empty;
                Assert.AreEqual("main.ts", Path.GetFileName(sourcePath));
                Assert.AreEqual(expectedBreakpointLine, pausedFrame.GetProperty("line").GetInt32());
                var pausedColumn = pausedFrame.GetProperty("column").GetInt32();
                Assert.IsTrue(
                    pausedColumn >= expectedBreakpointColumn,
                    $"Expected mapped column >= {expectedBreakpointColumn}, actual {pausedColumn}.");

                using var fullStackTraceResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "stackTrace",
                    new
                    {
                        threadId = 1
                    },
                    cancellationSource.Token);
                var totalFrames = fullStackTraceResponse.RootElement
                    .GetProperty("body")
                    .GetProperty("totalFrames")
                    .GetInt32();
                Assert.IsTrue(
                    totalFrames >= 3,
                    $"Expected recursive pause stack to contain at least 3 frames, actual {totalFrames}.");

                using var firstPageResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "stackTrace",
                    new
                    {
                        threadId = 1,
                        startFrame = 0,
                        levels = 2
                    },
                    cancellationSource.Token);
                var firstPageBody = firstPageResponse.RootElement.GetProperty("body");
                Assert.AreEqual(totalFrames, firstPageBody.GetProperty("totalFrames").GetInt32());
                var firstPageFrames = firstPageBody.GetProperty("stackFrames").EnumerateArray().ToArray();
                Assert.AreEqual(Math.Min(totalFrames, 2), firstPageFrames.Length);
                Assert.AreEqual("main.ts", Path.GetFileName(firstPageFrames[0].GetProperty("source").GetProperty("path").GetString()));
                Assert.AreEqual(expectedBreakpointLine, firstPageFrames[0].GetProperty("line").GetInt32());

                using var secondPageResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "stackTrace",
                    new
                    {
                        threadId = 1,
                        startFrame = 2,
                        levels = 2
                    },
                    cancellationSource.Token);
                var secondPageBody = secondPageResponse.RootElement.GetProperty("body");
                Assert.AreEqual(totalFrames, secondPageBody.GetProperty("totalFrames").GetInt32());
                var secondPageFrames = secondPageBody.GetProperty("stackFrames").EnumerateArray().ToArray();
                Assert.IsTrue(
                    secondPageFrames.Length > 0,
                    "Expected second pagination window to contain additional frames.");

                using var outOfRangeResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "stackTrace",
                    new
                    {
                        threadId = 1,
                        startFrame = totalFrames + 5,
                        levels = 2
                    },
                    cancellationSource.Token);
                var outOfRangeBody = outOfRangeResponse.RootElement.GetProperty("body");
                Assert.AreEqual(totalFrames, outOfRangeBody.GetProperty("totalFrames").GetInt32());
                Assert.AreEqual(0, outOfRangeBody.GetProperty("stackFrames").GetArrayLength());

                var frameId = pausedFrame.GetProperty("id").GetInt32();
                using var evaluateBuildVersionResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "evaluate",
                    new
                    {
                        expression = "buildVersion",
                        frameId,
                        context = "watch"
                    },
                    cancellationSource.Token);
                var evaluatedBuildVersion = evaluateBuildVersionResponse.RootElement
                    .GetProperty("body")
                    .GetProperty("result")
                    .GetString();
                Assert.AreEqual(
                    iteration.ToString(),
                    evaluatedBuildVersion,
                    $"Expected paused-frame evaluate(buildVersion) to stay aligned with HMR iteration {iteration}.");

                using var continueResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "continue",
                    new
                    {
                        threadId = 1
                    },
                    cancellationSource.Token);
                Assert.IsTrue(continueResponse.RootElement.GetProperty("success").GetBoolean());

                using var continuedEvent = await ReadDapMessageAsync(hostProcess, cancellationSource.Token);
                Assert.AreEqual("continued", continuedEvent.RootElement.GetProperty("event").GetString());

                await WaitForStackToClearAsync(hostProcess, sequence, cancellationSource.Token);
            }

            using var disconnectResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "disconnect",
                null,
                cancellationSource.Token);
            Assert.AreEqual("disconnect", disconnectResponse.RootElement.GetProperty("command").GetString());

            await hostProcess.WaitForExitAsync(cancellationSource.Token);
            var hostStandardError = await hostProcess.StandardError.ReadToEndAsync(cancellationSource.Token);
            Assert.AreEqual(0, hostProcess.ExitCode, hostStandardError);
        }
        finally
        {
            await EnsureProcessTerminatedAsync(hostProcess, cancellationSource.Token);
            await EnsureProcessTerminatedAsync(browserProcess, cancellationSource.Token);
            TryDeleteDirectoryWithRetry(devRoot);
        }
    }

    [TestMethod]
    public async Task Jolt_DapProcess_RealBrowserCdpSourceMapExceptionMatrix_ValidatesMixedMappedAndUnmappedFramesAcrossHmrIterations()
    {
        if (!ReadBooleanEnvironmentVariable(RealCdpSourceMapMatrixEnvironmentVariable))
        {
            return;
        }

        var browserExecutablePath = ResolveRealBrowserExecutablePath();
        if (string.IsNullOrWhiteSpace(browserExecutablePath))
        {
            Assert.Inconclusive(
                $"Unable to locate browser executable. Set {RealCdpBrowserPathEnvironmentVariable} to Chrome/Edge path.");
        }

        var devRoot = CreateTemporaryDirectory();
        var browserProfilePath = Path.Combine(devRoot, ".browser-profile");
        Directory.CreateDirectory(browserProfilePath);

        var indexPath = Path.Combine(devRoot, "index.html");
        var modulePath = Path.Combine(devRoot, "main.ts");
        var bridgePath = Path.Combine(devRoot, "bridge.js");
        var moduleVersion1 = CreateRealCdpSourceMapExceptionMatrixMainModule(1);
        await File.WriteAllTextAsync(
            indexPath,
            """
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <title>Jolt CDP SourceMap Exception Matrix</title>
            </head>
            <body>
              <div id="app">ready</div>
              <script type="module" src="/main.ts"></script>
            </body>
            </html>
            """,
            CancellationToken.None);
        await File.WriteAllTextAsync(bridgePath, CreateRealCdpSourceMapMatrixBridgeModule(), CancellationToken.None);
        await File.WriteAllTextAsync(modulePath, moduleVersion1, CancellationToken.None);

        var breakpointNeedle = "throw new Error(`matrix-iteration-${buildVersion}`);";
        var expectedBreakpointLine = GetLineIndexContaining(moduleVersion1, breakpointNeedle) + 1;
        var expectedBreakpointColumn = GetSourceColumn(moduleVersion1, breakpointNeedle);
        var devPort = AllocateLoopbackTcpPort();
        var cdpPort = AllocateLoopbackTcpPort();
        const int iterations = 4;
        const int recursionDepth = 6;

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(4));
        Process? browserProcess = null;
        Process? hostProcess = null;

        try
        {
            browserProcess = CreateHeadlessBrowserProcess(
                browserExecutablePath,
                cdpPort,
                browserProfilePath);
            Assert.IsTrue(browserProcess.Start(), "Expected headless browser process to start.");

            var cdpWebSocket = await WaitForBrowserCdpPageEndpointAsync(cdpPort, cancellationSource.Token);
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(cdpWebSocket),
                "Expected browser to expose a page-level CDP WebSocket endpoint.");

            hostProcess = CreateJoltDapDevProcess(
                devRoot,
                devPort,
                cdpWebSocket!);
            Assert.IsTrue(hostProcess.Start(), "Expected Jolt DAP+Dev process to start.");

            await WaitForHttpReadyAsync(
                new Uri($"http://127.0.0.1:{devPort}/index.html"),
                cancellationSource.Token);

            var sequence = new DapSequenceCounter();

            using var initializeResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "initialize",
                new
                {
                    adapterID = "jolt"
                },
                cancellationSource.Token);
            Assert.AreEqual("initialize", initializeResponse.RootElement.GetProperty("command").GetString());
            Assert.IsTrue(
                initializeResponse.RootElement.GetProperty("body").GetProperty("supportsConfigurationDoneRequest").GetBoolean());

            using var initializedEvent = await ReadDapMessageAsync(hostProcess, cancellationSource.Token);
            Assert.AreEqual("initialized", initializedEvent.RootElement.GetProperty("event").GetString());

            using var configurationDoneResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "configurationDone",
                null,
                cancellationSource.Token);
            Assert.IsTrue(configurationDoneResponse.RootElement.GetProperty("success").GetBoolean());

            var navigateExpression = "location.href = "
                + JsonSerializer.Serialize($"http://127.0.0.1:{devPort}/index.html")
                + "; 'navigating';";
            using var navigateResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "evaluate",
                new
                {
                    expression = navigateExpression,
                    context = "repl"
                },
                cancellationSource.Token);
            Assert.IsTrue(navigateResponse.RootElement.GetProperty("success").GetBoolean());

            await WaitForIterationReadyAsync(
                hostProcess,
                sequence,
                expectedVersion: 1,
                cancellationSource.Token);

            using var setBreakpointsResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "setBreakpoints",
                new
                {
                    source = new
                    {
                        path = modulePath
                    },
                    breakpoints = new object[]
                    {
                        new
                        {
                            line = expectedBreakpointLine,
                            column = expectedBreakpointColumn
                        }
                    }
                },
                cancellationSource.Token);
            Assert.IsTrue(setBreakpointsResponse.RootElement.GetProperty("success").GetBoolean());
            var verifiedBreakpoints = setBreakpointsResponse.RootElement
                .GetProperty("body")
                .GetProperty("breakpoints")
                .EnumerateArray()
                .ToArray();
            Assert.AreEqual(1, verifiedBreakpoints.Length);
            Assert.IsTrue(
                verifiedBreakpoints[0].GetProperty("verified").GetBoolean(),
                "Breakpoint binding failed: " + verifiedBreakpoints[0].GetRawText());

            for (var iteration = 1; iteration <= iterations; iteration++)
            {
                if (iteration > 1)
                {
                    var updatedSource = CreateRealCdpSourceMapExceptionMatrixMainModule(iteration);
                    await File.WriteAllTextAsync(modulePath, updatedSource, cancellationSource.Token);
                    await WaitForIterationReadyAsync(
                        hostProcess,
                        sequence,
                        expectedVersion: iteration,
                        cancellationSource.Token);
                }

                using var triggerResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "evaluate",
                    new
                    {
                        expression = $"setTimeout(() => {{ try {{ globalThis.__jazorRunIteration?.({recursionDepth}); }} catch {{ }} }}, 0); 'scheduled';",
                        context = "repl"
                    },
                    cancellationSource.Token);
                Assert.IsTrue(triggerResponse.RootElement.GetProperty("success").GetBoolean());

                var pausedFrame = await WaitForPausedFrameAsync(hostProcess, sequence, cancellationSource.Token);
                var sourcePath = pausedFrame.GetProperty("source").GetProperty("path").GetString() ?? string.Empty;
                Assert.AreEqual("main.ts", Path.GetFileName(sourcePath));
                Assert.AreEqual(expectedBreakpointLine, pausedFrame.GetProperty("line").GetInt32());
                var pausedColumn = pausedFrame.GetProperty("column").GetInt32();
                Assert.IsTrue(
                    pausedColumn >= expectedBreakpointColumn,
                    $"Expected mapped column >= {expectedBreakpointColumn}, actual {pausedColumn}.");

                using var fullStackTraceResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "stackTrace",
                    new
                    {
                        threadId = 1
                    },
                    cancellationSource.Token);
                var fullStackBody = fullStackTraceResponse.RootElement.GetProperty("body");
                var totalFrames = fullStackBody.GetProperty("totalFrames").GetInt32();
                var fullStackFrames = fullStackBody
                    .GetProperty("stackFrames")
                    .EnumerateArray()
                    .ToArray();
                Assert.IsTrue(
                    totalFrames >= 4,
                    $"Expected exception matrix stack to contain at least 4 frames, actual {totalFrames}.");
                Assert.AreEqual(totalFrames, fullStackFrames.Length);
                Assert.IsTrue(
                    fullStackFrames.Any(frame => string.Equals(
                        Path.GetFileName(frame.GetProperty("source").GetProperty("path").GetString()),
                        "main.ts",
                        StringComparison.OrdinalIgnoreCase)),
                    "Expected full stack to include mapped main.ts frames.");
                var bridgeFrameIndex = Array.FindIndex(
                    fullStackFrames,
                    frame => string.Equals(
                        Path.GetFileName(frame.GetProperty("source").GetProperty("path").GetString()),
                        "bridge.js",
                        StringComparison.OrdinalIgnoreCase));
                Assert.IsTrue(
                    bridgeFrameIndex >= 0,
                    "Expected full stack to include unmapped bridge.js frame.");

                var bridgeWindowStart = Math.Max(bridgeFrameIndex - 1, 0);
                using var bridgeWindowResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "stackTrace",
                    new
                    {
                        threadId = 1,
                        startFrame = bridgeWindowStart,
                        levels = 3
                    },
                    cancellationSource.Token);
                var bridgeWindowBody = bridgeWindowResponse.RootElement.GetProperty("body");
                Assert.AreEqual(totalFrames, bridgeWindowBody.GetProperty("totalFrames").GetInt32());
                var bridgeWindowFrames = bridgeWindowBody.GetProperty("stackFrames").EnumerateArray().ToArray();
                Assert.IsTrue(
                    bridgeWindowFrames.Any(frame => string.Equals(
                        Path.GetFileName(frame.GetProperty("source").GetProperty("path").GetString()),
                        "bridge.js",
                        StringComparison.OrdinalIgnoreCase)),
                    "Expected bridge pagination window to retain unmapped frame.");

                using var firstPageResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "stackTrace",
                    new
                    {
                        threadId = 1,
                        startFrame = 0,
                        levels = 2
                    },
                    cancellationSource.Token);
                var firstPageBody = firstPageResponse.RootElement.GetProperty("body");
                var firstPageFrames = firstPageBody.GetProperty("stackFrames").EnumerateArray().ToArray();
                Assert.AreEqual(totalFrames, firstPageBody.GetProperty("totalFrames").GetInt32());
                Assert.AreEqual(2, firstPageFrames.Length);

                using var secondPageResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "stackTrace",
                    new
                    {
                        threadId = 1,
                        startFrame = 2,
                        levels = 2
                    },
                    cancellationSource.Token);
                var secondPageBody = secondPageResponse.RootElement.GetProperty("body");
                var secondPageFrames = secondPageBody.GetProperty("stackFrames").EnumerateArray().ToArray();
                Assert.AreEqual(totalFrames, secondPageBody.GetProperty("totalFrames").GetInt32());
                Assert.IsTrue(secondPageFrames.Length > 0);
                Assert.AreNotEqual(
                    firstPageFrames[^1].GetProperty("id").GetInt32(),
                    secondPageFrames[0].GetProperty("id").GetInt32(),
                    "Expected adjacent pagination windows to advance stack frame identity.");

                var frameId = pausedFrame.GetProperty("id").GetInt32();
                using var evaluateBuildVersionResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "evaluate",
                    new
                    {
                        expression = "buildVersion",
                        frameId,
                        context = "watch"
                    },
                    cancellationSource.Token);
                var evaluatedBuildVersion = evaluateBuildVersionResponse.RootElement
                    .GetProperty("body")
                    .GetProperty("result")
                    .GetString();
                Assert.AreEqual(
                    iteration.ToString(),
                    evaluatedBuildVersion,
                    $"Expected paused-frame evaluate(buildVersion) to stay aligned with HMR iteration {iteration}.");

                using var continueResponse = await SendDapRequestAsync(
                    hostProcess,
                    sequence,
                    "continue",
                    new
                    {
                        threadId = 1
                    },
                    cancellationSource.Token);
                Assert.IsTrue(continueResponse.RootElement.GetProperty("success").GetBoolean());

                using var continuedEvent = await ReadDapMessageAsync(hostProcess, cancellationSource.Token);
                Assert.AreEqual("continued", continuedEvent.RootElement.GetProperty("event").GetString());

                await WaitForStackToClearAsync(hostProcess, sequence, cancellationSource.Token);
            }

            using var disconnectResponse = await SendDapRequestAsync(
                hostProcess,
                sequence,
                "disconnect",
                null,
                cancellationSource.Token);
            Assert.AreEqual("disconnect", disconnectResponse.RootElement.GetProperty("command").GetString());

            await hostProcess.WaitForExitAsync(cancellationSource.Token);
            var hostStandardError = await hostProcess.StandardError.ReadToEndAsync(cancellationSource.Token);
            Assert.AreEqual(0, hostProcess.ExitCode, hostStandardError);
        }
        finally
        {
            await EnsureProcessTerminatedAsync(hostProcess, cancellationSource.Token);
            await EnsureProcessTerminatedAsync(browserProcess, cancellationSource.Token);
            TryDeleteDirectoryWithRetry(devRoot);
        }
    }

    private static async Task<JsonDocument> SendDapRequestAsync(
        Process process,
        DapSequenceCounter sequence,
        string command,
        object? arguments,
        CancellationToken cancellationToken)
    {
        if (arguments is null)
        {
            await WriteDapMessageAsync(
                process.StandardInput.BaseStream,
                new
                {
                    seq = sequence.Next(),
                    type = "request",
                    command
                },
                cancellationToken);
        }
        else
        {
            await WriteDapMessageAsync(
                process.StandardInput.BaseStream,
                new
                {
                    seq = sequence.Next(),
                    type = "request",
                    command,
                    arguments
                },
                cancellationToken);
        }

        var response = await ReadDapMessageAsync(process, cancellationToken);
        Assert.AreEqual("response", response.RootElement.GetProperty("type").GetString());
        Assert.AreEqual(command, response.RootElement.GetProperty("command").GetString());
        Assert.IsTrue(
            response.RootElement.GetProperty("success").GetBoolean(),
            response.RootElement.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : "DAP request failed.");
        return response;
    }

    private static async Task WaitForIterationReadyAsync(
        Process process,
        DapSequenceCounter sequence,
        int expectedVersion,
        CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow + TimeSpan.FromSeconds(20);
        while (DateTime.UtcNow < timeoutAt)
        {
            using var evaluateResponse = await SendDapRequestAsync(
                process,
                sequence,
                "evaluate",
                new
                {
                    expression = "(() => { const g = globalThis; return String(g.__jazorBuildVersion ?? 'null') + ':' + String(g.__jazorRuntimeCounter ?? 'null'); })()",
                    context = "repl"
                },
                cancellationToken);

            var state = evaluateResponse.RootElement
                .GetProperty("body")
                .GetProperty("result")
                .GetString() ?? string.Empty;
            var parts = state.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2
                && int.TryParse(parts[0], out var version)
                && int.TryParse(parts[1], out var runtimeCounter)
                && version == expectedVersion
                && runtimeCounter >= expectedVersion)
            {
                return;
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException($"Timed out waiting for HMR iteration {expectedVersion} readiness.");
    }

    private static async Task<JsonElement> WaitForPausedFrameAsync(
        Process process,
        DapSequenceCounter sequence,
        CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow + TimeSpan.FromSeconds(20);
        while (DateTime.UtcNow < timeoutAt)
        {
            using var stackTraceResponse = await SendDapRequestAsync(
                process,
                sequence,
                "stackTrace",
                new
                {
                    threadId = 1
                },
                cancellationToken);
            var stackFrames = stackTraceResponse.RootElement
                .GetProperty("body")
                .GetProperty("stackFrames")
                .EnumerateArray()
                .ToArray();
            if (stackFrames.Length > 0)
            {
                return stackFrames[0].Clone();
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException("Timed out waiting for CDP pause frame.");
    }

    private static async Task WaitForStackToClearAsync(
        Process process,
        DapSequenceCounter sequence,
        CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < timeoutAt)
        {
            using var stackTraceResponse = await SendDapRequestAsync(
                process,
                sequence,
                "stackTrace",
                new
                {
                    threadId = 1
                },
                cancellationToken);
            var totalFrames = stackTraceResponse.RootElement
                .GetProperty("body")
                .GetProperty("totalFrames")
                .GetInt32();
            if (totalFrames == 0)
            {
                return;
            }

            await Task.Delay(50, cancellationToken);
        }

        throw new TimeoutException("Timed out waiting for cleared call stack after continue.");
    }

    private static Process CreateHeadlessBrowserProcess(
        string browserExecutablePath,
        int cdpPort,
        string profilePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = browserExecutablePath,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add($"--remote-debugging-port={cdpPort}");
        startInfo.ArgumentList.Add("--headless=new");
        startInfo.ArgumentList.Add("--disable-gpu");
        startInfo.ArgumentList.Add("--no-first-run");
        startInfo.ArgumentList.Add("--no-default-browser-check");
        startInfo.ArgumentList.Add("--disable-background-networking");
        startInfo.ArgumentList.Add("--disable-background-timer-throttling");
        startInfo.ArgumentList.Add("--disable-renderer-backgrounding");
        startInfo.ArgumentList.Add($"--user-data-dir={profilePath}");
        startInfo.ArgumentList.Add("about:blank");

        return new Process
        {
            StartInfo = startInfo
        };
    }

    private static Process CreateJoltDapDevProcess(
        string devRoot,
        int devPort,
        string cdpWebSocketUrl)
    {
        var hostAssemblyPath = ResolveDapHostAssemblyPath();
        Assert.IsTrue(File.Exists(hostAssemblyPath), $"Expected Jolt assembly to exist at '{hostAssemblyPath}'.");

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
        startInfo.ArgumentList.Add("--dev");
        startInfo.ArgumentList.Add($"--dev-root={devRoot}");
        startInfo.ArgumentList.Add("--dev-host=127.0.0.1");
        startInfo.ArgumentList.Add($"--dev-port={devPort}");
        startInfo.ArgumentList.Add($"--dap-cdp-ws={cdpWebSocketUrl}");

        return new Process
        {
            StartInfo = startInfo
        };
    }

    private static async Task<string> WaitForBrowserCdpPageEndpointAsync(
        int cdpPort,
        CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow + TimeSpan.FromSeconds(20);
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                using var response = await httpClient.GetAsync(
                    new Uri($"http://127.0.0.1:{cdpPort}/json/list"),
                    cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var payload = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var document = JsonDocument.Parse(payload);
                    if (document.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var entry in document.RootElement.EnumerateArray())
                        {
                            if (entry.TryGetProperty("type", out var typeElement)
                                && typeElement.ValueKind == JsonValueKind.String
                                && !string.Equals(typeElement.GetString(), "page", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            if (entry.TryGetProperty("webSocketDebuggerUrl", out var urlElement)
                                && urlElement.ValueKind == JsonValueKind.String
                                && !string.IsNullOrWhiteSpace(urlElement.GetString()))
                            {
                                return urlElement.GetString()!;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException($"Timed out waiting for browser CDP endpoint on port {cdpPort}.");
    }

    private static async Task WaitForHttpReadyAsync(
        Uri url,
        CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow + TimeSpan.FromSeconds(20);
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                using var response = await httpClient.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException($"Timed out waiting for HTTP endpoint '{url}'.");
    }

    private static int AllocateLoopbackTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "JoltDebugProtocolTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void TryDeleteDirectoryWithRetry(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }

                return;
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            Thread.Sleep(100);
        }
    }

    private static async Task EnsureProcessTerminatedAsync(Process? process, CancellationToken cancellationToken)
    {
        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        finally
        {
            process.Dispose();
        }
    }

    private static string CreateRealCdpStressMainModule(int version)
        => $$"""
        export const buildVersion = {{version}};

        enum MarkerState {
          Active = 1
        }

        export function runIteration() {
          const marker = buildVersion + MarkerState.Active;
          return marker;
        }

        const __jazorGlobal = globalThis;
        __jazorGlobal.__jazorRuntimeCounter = (__jazorGlobal.__jazorRuntimeCounter ?? 0) + 1;
        const __jazorHot = import.meta.hot ?? __jazorGlobal.__JAZOR_HMR__?.createHotContext(import.meta.url);
        if (__jazorHot) {
          import.meta.hot = __jazorHot;
          __jazorHot.accept((updatedModule) => {
            if (typeof updatedModule?.runIteration === "function") {
              __jazorGlobal.__jazorRunIteration = updatedModule.runIteration;
            }
            __jazorGlobal.__jazorBuildVersion = updatedModule?.buildVersion ?? -1;
          });
        }

        __jazorGlobal.__jazorRunIteration = runIteration;
        __jazorGlobal.__jazorBuildVersion = buildVersion;
        __jazorGlobal.__jazorEditToken = "v{{version}}";
        """;

    private static string CreateRealCdpSourceMapMatrixMainModule(int version)
        => $$"""
        export const buildVersion = {{version}};

        enum MarkerState {
          Active = 1
        }

        function leaf(depth: number): number {
          const marker = buildVersion + depth + MarkerState.Active;
          return marker;
        }

        function descend(depth: number): number {
          if (depth <= 0) {
            return leaf(depth);
          }

          return descend(depth - 1) + 1;
        }

        export function runIteration(depth: number = 6): number {
          return descend(depth);
        }

        const __jazorGlobal = globalThis;
        __jazorGlobal.__jazorRuntimeCounter = (__jazorGlobal.__jazorRuntimeCounter ?? 0) + 1;
        const __jazorHot = import.meta.hot ?? __jazorGlobal.__JAZOR_HMR__?.createHotContext(import.meta.url);
        if (__jazorHot) {
          import.meta.hot = __jazorHot;
          __jazorHot.accept((updatedModule) => {
            if (typeof updatedModule?.runIteration === "function") {
              __jazorGlobal.__jazorRunIteration = updatedModule.runIteration;
            }
            __jazorGlobal.__jazorBuildVersion = updatedModule?.buildVersion ?? -1;
          });
        }

        __jazorGlobal.__jazorRunIteration = runIteration;
        __jazorGlobal.__jazorBuildVersion = buildVersion;
        __jazorGlobal.__jazorEditToken = "matrix-v{{version}}";
        """;

    private static string CreateRealCdpSourceMapExceptionMatrixMainModule(int version)
        => $$"""
        import { invokeWithBridge } from "./bridge.js";

        export const buildVersion = {{version}};

        enum MarkerState {
          Active = 1
        }

        function throwLeaf(depth: number): number {
          const marker = buildVersion + depth + MarkerState.Active;
          if (depth <= 0) {
            throw new Error(`matrix-iteration-${buildVersion}`);
          }

          return marker;
        }

        function descend(depth: number): number {
          if (depth <= 0) {
            return throwLeaf(depth);
          }

          return descend(depth - 1) + 1;
        }

        export function runIteration(depth: number = 6): number {
          return invokeWithBridge(depth, (value: number) => descend(value));
        }

        const __jazorGlobal = globalThis;
        __jazorGlobal.__jazorRuntimeCounter = (__jazorGlobal.__jazorRuntimeCounter ?? 0) + 1;
        const __jazorHot = import.meta.hot ?? __jazorGlobal.__JAZOR_HMR__?.createHotContext(import.meta.url);
        if (__jazorHot) {
          import.meta.hot = __jazorHot;
          __jazorHot.accept((updatedModule) => {
            if (typeof updatedModule?.runIteration === "function") {
              __jazorGlobal.__jazorRunIteration = updatedModule.runIteration;
            }
            __jazorGlobal.__jazorBuildVersion = updatedModule?.buildVersion ?? -1;
          });
        }

        __jazorGlobal.__jazorRunIteration = runIteration;
        __jazorGlobal.__jazorBuildVersion = buildVersion;
        __jazorGlobal.__jazorEditToken = "matrix-ex-v{{version}}";
        """;

    private static string CreateRealCdpSourceMapMatrixBridgeModule()
        => """
        export function invokeWithBridge(depth, callback) {
          return bridgeLevelOne(depth, callback);
        }

        function bridgeLevelOne(depth, callback) {
          return bridgeLevelTwo(depth, callback);
        }

        function bridgeLevelTwo(depth, callback) {
          return callback(depth);
        }
        """;

    private static int GetSourceColumn(string sourceText, string needle)
    {
        var lines = sourceText
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n');
        var lineIndex = GetLineIndexContaining(sourceText, needle);
        var lineText = lines[lineIndex];
        var columnIndex = lineText.IndexOf(needle, StringComparison.Ordinal);
        return columnIndex < 0
            ? 1
            : columnIndex + 1;
    }

    private static bool ReadBooleanEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.Equals(value, "1", StringComparison.Ordinal)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveRealBrowserExecutablePath()
    {
        var configured = Environment.GetEnvironmentVariable(RealCdpBrowserPathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var candidates = new[]
        {
            Path.Combine(programFiles, "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(programFilesX86, "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(programFiles, "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(programFilesX86, "Google", "Chrome", "Application", "chrome.exe")
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private sealed class DapSequenceCounter
    {
        private int _nextValue = 1;

        public int Next() => _nextValue++;
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

    private static Process CreateJoltDapProcess()
    {
        var hostAssemblyPath = ResolveDapHostAssemblyPath();

        Assert.IsTrue(File.Exists(hostAssemblyPath), $"Expected Jolt assembly to exist at '{hostAssemblyPath}'.");

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
        var configuredPath = Environment.GetEnvironmentVariable("JOLT_DAP_HOST_DLL");
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
                    "Jolt",
                    "bin",
                    currentConfiguration,
                    "net10.0",
                    "Jolt.dll");
                var depsPathForCurrentConfiguration = Path.Combine(
                    repositoryRootForConfiguration,
                    "src",
                    "Jolt",
                    "bin",
                    currentConfiguration,
                    "net10.0",
                    "Jolt.deps.json");
                if (File.Exists(hostPathForCurrentConfiguration) && File.Exists(depsPathForCurrentConfiguration))
                {
                    return hostPathForCurrentConfiguration;
                }
            }
        }

        var siblingHostPath = Path.Combine(AppContext.BaseDirectory, "Jolt.dll");
        var siblingDepsPath = Path.Combine(AppContext.BaseDirectory, "Jolt.deps.json");
        if (File.Exists(siblingHostPath) && File.Exists(siblingDepsPath))
        {
            return siblingHostPath;
        }

        if (TryGetRepositoryRoot(out var repositoryRoot))
        {
            var defaultHostPath = Path.Combine(
                repositoryRoot,
                "src",
                "Jolt",
                "bin",
                "Debug",
                "net10.0",
                "Jolt.dll");
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

    // Deterministic fake-CDP tests cover sourcemap rebinding semantics here.
    // Real browser/CDP target reload timing remains better suited to env-gated integration runs.
    private static DapRequest CreateSetBreakpointsRequest(int seq)
        => new()
        {
            Seq = seq,
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
        };

    private static string CreateSingleSourceColumnMap(
        string fileName,
        string sourceText,
        params (int GeneratedLine, int GeneratedColumn, int SourceLine, int SourceColumn)[] segments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);
        ArgumentNullException.ThrowIfNull(segments);

        var builder = new StringBuilder();
        var previousGeneratedLine = 0;
        var previousGeneratedColumn = 0;
        var previousSourceLine = 0;
        var previousSourceColumn = 0;

        foreach (var segment in segments
            .OrderBy(static item => item.GeneratedLine)
            .ThenBy(static item => item.GeneratedColumn))
        {
            while (previousGeneratedLine < segment.GeneratedLine)
            {
                builder.Append(';');
                previousGeneratedLine++;
                previousGeneratedColumn = 0;
            }

            if (builder.Length > 0 && builder[^1] != ';')
            {
                builder.Append(',');
            }

            builder.Append(EncodeVlq(segment.GeneratedColumn - previousGeneratedColumn));
            builder.Append(EncodeVlq(0));
            builder.Append(EncodeVlq(segment.SourceLine - previousSourceLine));
            builder.Append(EncodeVlq(segment.SourceColumn - previousSourceColumn));

            previousGeneratedColumn = segment.GeneratedColumn;
            previousSourceLine = segment.SourceLine;
            previousSourceColumn = segment.SourceColumn;
        }

        return JsonSerializer.Serialize(new
        {
            version = 3,
            sources = new[] { fileName },
            sourcesContent = new[] { sourceText },
            names = Array.Empty<string>(),
            mappings = builder.ToString(),
            file = Path.ChangeExtension(fileName, ".js")
        });
    }

    private static string EncodeVlq(int value)
    {
        const string base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        var vlq = value < 0 ? ((-value) << 1) + 1 : value << 1;
        var builder = new StringBuilder();
        do
        {
            var digit = vlq & 31;
            vlq >>= 5;
            if (vlq > 0)
            {
                digit |= 32;
            }

            builder.Append(base64Digits[digit]);
        }
        while (vlq > 0);

        return builder.ToString();
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
