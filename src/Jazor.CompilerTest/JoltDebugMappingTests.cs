using Jolt.Debug;
using Jazor.SourceMaps;
using Jolt.SourceMap;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JoltDebugMappingTests
{
    [TestMethod]
    public void BreakpointManager_MapBreakpoint_ReturnsRegisteredResolvedUrlAndLocation()
    {
        var service = new InMemorySourceMapService();
        const string sourceText = "line0\nline1\nline2";
        service.Register(
            "/Counter.jazor",
            CreateSingleSourceLineMap(
                "Counter.jazor",
                sourceText,
                [
                    1,
                    2
                ]));

        var manager = new BreakpointManager(service);

        var mapped = manager.MapBreakpoint(@"D:\repo\Counter.jazor", 2, 0);

        Assert.IsNotNull(mapped);
        Assert.AreEqual("/Counter.jazor", mapped.GeneratedPath);
        Assert.AreEqual(1, mapped.GeneratedLine);
        Assert.AreEqual(0, mapped.GeneratedColumn);
    }

    [TestMethod]
    public void BreakpointManager_MapBreakpoint_WhenMapMissing_ReturnsNull()
    {
        var manager = new BreakpointManager(new InMemorySourceMapService());

        var mapped = manager.MapBreakpoint(@"D:\repo\Counter.jazor", 0, 0);

        Assert.IsNull(mapped);
    }

    [TestMethod]
    [DoNotParallelize]
    public void BreakpointManager_MapBreakpoint_WhenRootedSourcePathMissing_ReportsWarning()
    {
        var manager = new BreakpointManager(new InMemorySourceMapService());
        var originalError = Console.Error;
        using var errorWriter = new StringWriter();
        Console.SetError(errorWriter);

        try
        {
            var mapped = manager.MapBreakpoint(@"Z:\missing\Counter.jazor", 0, 0);

            Assert.IsNull(mapped);
            StringAssert.Contains(errorWriter.ToString(), "dapBreakpointSourcePathUnavailable");
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    [TestMethod]
    public void CallStackMapper_MapCallStack_MapsFramesBackToOriginalSource()
    {
        var service = new InMemorySourceMapService();
        const string sourceText = """
            <button>@Count</button>

            @code {
                private int Count = 1;

                public void Increment()
                {
                    Count++;
                }
            }
            """;
        service.Register(
            "/Counter.jazor",
            CreateSingleSourceLineMap(
                "Counter.jazor",
                sourceText,
                [
                    GetLineIndexContaining(sourceText, "private int Count = 1;"),
                    GetLineIndexContaining(sourceText, "Count++;")
                ]));

        var mapper = new CallStackMapper(service);

        var frames = mapper.MapCallStack(
        [
            new CdpCallFrame(
                "frame-1",
                "increment",
                new CdpLocation("/Counter.jazor", 1, 0))
        ]);

        Assert.AreEqual(1, frames.Count);
        Assert.AreEqual(1, frames[0].Id);
        Assert.AreEqual("increment", frames[0].Name);
        Assert.AreEqual("Counter.jazor", frames[0].Source.Name);
        Assert.AreEqual("Counter.jazor", frames[0].Source.Path);
        Assert.AreEqual(GetLineIndexContaining(sourceText, "Count++;") + 1, frames[0].Line);
        Assert.AreEqual(1, frames[0].Column);
    }

    [TestMethod]
    public void CallStackMapper_MapCallStack_WhenMappingMissing_FallsBackToGeneratedLocation()
    {
        var mapper = new CallStackMapper(new InMemorySourceMapService());

        var frames = mapper.MapCallStack(
        [
            new CdpCallFrame(
                "frame-1",
                "",
                new CdpLocation("/generated/chunk.js", 5, 2))
        ]);

        Assert.AreEqual(1, frames.Count);
        Assert.AreEqual("(anonymous)", frames[0].Name);
        Assert.AreEqual("chunk.js", frames[0].Source.Name);
        Assert.AreEqual("/generated/chunk.js", frames[0].Source.Path);
        Assert.AreEqual(6, frames[0].Line);
        Assert.AreEqual(3, frames[0].Column);
    }

    [TestMethod]
    public void CallStackMapper_MapCallStack_TrimsNullPaddedAnonymousFunctionNames()
    {
        var mapper = new CallStackMapper(new InMemorySourceMapService());

        var frames = mapper.MapCallStack(
        [
            new CdpCallFrame(
                "frame-1",
                "\0 \t\r\n",
                new CdpLocation("/generated/chunk.js", 5, 2))
        ]);

        Assert.AreEqual("(anonymous)", frames[0].Name);
    }
}
