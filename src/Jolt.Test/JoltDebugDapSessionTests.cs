using Jolt.Debug;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDebugDapSessionTests
{
    [TestMethod]
    public async Task DapSession_VariableReferences_AreBoundedAndEvictOldestEntries()
    {
        var session = new DapSession();
        session.IsInitialized = true;
        session.IsStarted = true;
        session.CurrentCallFrames =
        [
            new CdpCallFrame(
                "frame-1",
                "increment",
                new CdpLocation("/Counter.jazor", 1, 0))
        ];

        var allocatedReferences = new List<int>(DapSession.MaxTrackedVariablesReferences + 32);
        for (var index = 0; index < DapSession.MaxTrackedVariablesReferences + 32; index++)
        {
            var evaluation = await session.EvaluateAsync("location", frameId: 1, context: "watch", CancellationToken.None);
            Assert.IsTrue(evaluation.VariablesReference > 0);
            allocatedReferences.Add(evaluation.VariablesReference);
        }

        Assert.IsTrue(session.TrackedVariablesReferenceCount <= DapSession.MaxTrackedVariablesReferences);

        var firstVariables = await session.GetVariablesAsync(allocatedReferences[0], CancellationToken.None);
        var latestVariables = await session.GetVariablesAsync(allocatedReferences[^1], CancellationToken.None);

        Assert.AreEqual(0, firstVariables.Count, "Old variable references should be evicted once the per-session cap is reached.");
        Assert.AreEqual(3, latestVariables.Count);
        Assert.AreEqual("url", latestVariables[0].Name);
    }

    [TestMethod]
    public async Task DapSession_ContinueExecution_ClearsPreviousVariableReferences()
    {
        var session = new DapSession();
        session.IsInitialized = true;
        session.IsStarted = true;
        session.CurrentCallFrames =
        [
            new CdpCallFrame(
                "frame-1",
                "increment",
                new CdpLocation("/Counter.jazor", 1, 0))
        ];

        var scopes = session.CreateScopes(frameId: 1);
        var localsReference = scopes[0].VariablesReference;
        var locals = await session.GetVariablesAsync(localsReference, CancellationToken.None);
        Assert.AreEqual(5, locals.Count);

        await session.ContinueExecutionAsync(CancellationToken.None);

        Assert.IsFalse(session.IsPaused);
        Assert.AreEqual(0, session.CurrentCallFrames.Count);
        Assert.AreEqual(0, session.TrackedVariablesReferenceCount);

        var staleVariables = await session.GetVariablesAsync(localsReference, CancellationToken.None);
        Assert.AreEqual(0, staleVariables.Count);
    }
}
