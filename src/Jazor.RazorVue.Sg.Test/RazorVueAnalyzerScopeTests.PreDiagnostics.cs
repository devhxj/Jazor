namespace Jazor.RazorVue.Sg.Test;

public sealed partial class RazorVueAnalyzerScopeTests
{
    [TestMethod]
    [DataRow("e.ClientX = 1;")]
    [DataRow("e.ClientX += 1;")]
    [DataRow("e.ClientX++;")]
    [DataRow("(e.ClientX, e.ClientY) = (1, 2);")]
    public async Task GetterOnlyMapping_RejectsWrites(string statement)
    {
        var diagnostics = await AnalyzeAsync($$"""
            using ECMAScript;
            using Microsoft.AspNetCore.Components.Web;
            [ECMAScriptModule("./events")]
            public static class Events
            {
                public static void Update(MouseEventArgs e) { {{statement}} }
            }
            """);

        Assert.IsTrue(diagnostics.Any(d => d.Id == "JAZOR001" && d.GetMessage().Contains("ClientX")),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task SupportedPropertyAccesses_AllowGetterAndMappedReadWriteAccessors()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            using Microsoft.AspNetCore.Components.Web;
            using System.Collections.Generic;
            [ECMAScriptModule("./events")]
            public static class Events
            {
                public static double Read(MouseEventArgs e) => e.ClientX;
                public static void Update(List<int> values)
                {
                    values.Capacity = 10;
                    values.Capacity += 1;
                    values.Capacity++;
                }
            }
            """);
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    [DataRow("_ = value + value;")]
    [DataRow("_ = -value;")]
    [DataRow("value++;")]
    [DataRow("value += value;")]
    [DataRow("_ = (int)value;")]
    public async Task UnmappedOperators_ReportAtTheUsageSite(string statement)
    {
        var diagnostics = await AnalyzeAsync($$"""
            using ECMAScript;
            public abstract class Number
            {
                public static Number operator +(Number a, Number b) => a;
                public static Number operator -(Number a) => a;
                public static Number operator ++(Number a) => a;
                public static explicit operator int(Number a) => 1;
            }
            [ECMAScriptModule("./operators")]
            public static class Operators
            {
                public static void Run(Number value) { {{statement}} }
            }
            """);
        Assert.IsTrue(diagnostics.Any(d => d.Id == "JAZOR001" && d.GetMessage().Contains("operator")),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task SupportedOperators_AllowNumericAndHostMappings()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            using System;
            public abstract class HostNumber
            {
                [ECMAScript]
                public static HostNumber operator +(HostNumber left, HostNumber right) => left;
            }
            [ECMAScriptModule("./operators")]
            public static class Operators
            {
                public static decimal Run(decimal value)
                {
                    value += 1;
                    value++;
                    return -value + (decimal)2.5;
                }
                public static DateTime Add(DateTime value, TimeSpan span) => value + span;
                public static HostNumber Add(HostNumber left, HostNumber right) => left + right;
            }
            """);
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task NativeUnionAndIndexConversions_RemainAccepted()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            using ECMAScript.VueDataUi;
            [ECMAScriptModule("./unions")]
            public static class Unions
            {
                public static VdCellValue Cell() => 42;
                public static VdXySeriesValues Series() => new double?[] { 1, null };
                public static System.Range Slice() => 1..^1;
            }
            """);
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    [DataRow("ExternalHost.Create()")]
    [DataRow("ExternalHost.Field")]
    [DataRow("ExternalHost.Property")]
    public async Task MetadataHostResult_RejectsUnsupportedTypeEvenWhenDiscarded(string expression)
    {
        // The host declaration lives in metadata: only the consumer's usage can produce this diagnostic.
        var diagnostics = await AnalyzeAsync($$"""
            using ECMAScript;
            [ECMAScriptModule("./metadata-results")]
            public static class Results
            {
                public static void Run() { _ = {{expression}}; }
            }
            """, referencedSource: """
            using ECMAScript;
            [ECMAScript]
            public static class ExternalHost
            {
                public static System.IO.FileInfo Create() => null!;
                public static System.IO.FileInfo Field;
                public static System.IO.FileInfo Property => null!;
            }
            """);
        AssertReportsUnsupportedFileInfo(diagnostics);
        Assert.IsTrue(diagnostics.Any(d => d.Id == "JAZOR001" &&
            d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan) == expression),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task GetterOnlyMapping_RejectsCoalescingAssignment()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            [ECMAScriptModule("./events")]
            public static class Events
            {
                public static void Update(ChangeEventArgs e) { e.Value ??= 1; }
            }
            """);
        Assert.IsTrue(diagnostics.Any(d => d.Id == "JAZOR001" && d.GetMessage().Contains("Value")),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task ClosedGenericMethodGroup_RejectsErasedUnsupportedTypeArgument()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            using System;
            [ECMAScriptModule("./method-groups")]
            public static class Groups
            {
                public static void Ignore<T>() { }
                public static Action Bind() => Ignore<System.IO.FileInfo>;
            }
            """);
        AssertReportsUnsupportedFileInfo(diagnostics);
    }

    [TestMethod]
    public async Task NestedGenericType_RejectsUnsupportedContainingTypeArgument()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            [ECMAScript]
            public class Box<T> { public class Item { } }
            [ECMAScriptModule("./nested-types")]
            public static class NestedTypes
            {
                public static Box<System.IO.FileInfo>.Item Value;
            }
            """);
        AssertReportsUnsupportedFileInfo(diagnostics);
    }

    [TestMethod]
    [DataRow("Box<System.IO.FileInfo>.Ignore();")]
    [DataRow("System.Action action = Box<System.IO.FileInfo>.Ignore;")]
    [DataRow("_ = Box<System.IO.FileInfo>.Field;")]
    [DataRow("_ = Box<System.IO.FileInfo>.Property;")]
    public async Task StaticGenericMemberUse_RejectsUnsupportedContainingTypeArgument(string statement)
    {
        var diagnostics = await AnalyzeAsync($$"""
            using ECMAScript;
            [ECMAScript]
            public static class Box<T>
            {
                public static void Ignore() { }
                public static int Field;
                public static int Property { get; set; }
            }
            [ECMAScriptModule("./generic-members")]
            public static class Members
            {
                public static void Run() { {{statement}} }
            }
            """);
        AssertReportsUnsupportedFileInfo(diagnostics);
    }

    [TestMethod]
    public async Task RecordStructSignature_RejectsUnsupportedConcreteType()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            [ECMAScript]
            public readonly record struct Payload(System.IO.FileInfo File);
            """);
        AssertReportsUnsupportedFileInfo(diagnostics);
    }

    [TestMethod]
    [DataRow("Read<System.IO.FileInfo>(out var file);")]
    [DataRow("var (file, count) = Pair<System.IO.FileInfo>();")]
    [DataRow("foreach (var file in values) { }")]
    public async Task InferredLocalForms_DiagnoseTheDeclaredVariable(string statement)
    {
        var diagnostics = await AnalyzeAsync($$"""
            using ECMAScript;
            using System.Collections.Generic;
            [ECMAScriptModule("./locals")]
            public static class Locals
            {
                public static void Read<T>(out T value) { value = default!; }
                public static (T, int) Pair<T>() => (default!, 0);
                public static void Run(IEnumerable<System.IO.FileInfo> values)
                {
                    {{statement}}
                }
            }
            """);
        Assert.IsTrue(diagnostics.Any(d => d.Id == "JAZOR001" &&
            d.GetMessage().Contains("System.IO.FileInfo") &&
            d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan) == "file"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task OpenGenericForms_AndCodeOutsideModules_RemainAccepted()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            using System;
            using System.Collections.Generic;
            [ECMAScript]
            public class Box<T> { public class Item { } }
            [ECMAScriptModule("./locals")]
            public static class Locals
            {
                public static void Read<T>(out T value) { value = default!; }
                public static (T, int) Pair<T>() => (default!, 0);
                public static void Ignore<T>() { }
                public static Box<T>.Item Keep<T>(Box<T>.Item item) => item;
                public static void Run<T>(IEnumerable<T> values)
                {
                    Read<T>(out var file);
                    var (value, count) = Pair<T>();
                    foreach (var current in values) { }
                    Action action = Ignore<T>;
                }
            }
            public static class ServerOnly
            {
                public static System.IO.FileInfo Read() => new("test.txt");
            }
            """);
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task AsCast_ReportsSharedRuntimeAliasAmbiguity()
    {
        var diagnostics = await AnalyzeAsync("""
            using ECMAScript;
            [ECMAScriptModule("./casts")]
            public static class Casts
            {
                public static object? Cast(object value) => value as System.Threading.Tasks.Task<int>;
            }
            """);
        Assert.IsTrue(diagnostics.Any(d => d.Id == "JAZOR002"), string.Join(Environment.NewLine, diagnostics));
    }
}
