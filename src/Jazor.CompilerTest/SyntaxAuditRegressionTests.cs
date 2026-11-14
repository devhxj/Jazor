using Acornima;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Jazor.NodeHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SyntaxAuditRegressionTests
{
    [TestMethod]
    public void IntegralBoundaries_MatchClrResultsAndExceptions()
        => NumericAuditOracle.AssertBoundaries();

    public static IEnumerable<TestDataRow<AuditCase>> ProtocolCases
    {
        get
        {
            foreach (var (name, body, expected, failure) in new (string, string, string, string?)[]
            {
                ("distinct-out-locals", "int a; int b; Set(out a,out b); return a+b;", "3", null),
                ("declared-out-locals", "Set(out var a,out var b); return a+b;", "3", null),
                ("local-and-field", "int a=0; var holder=new H(); Set(out a,out holder.A); return a+holder.A;", "3", null),
                ("local-and-element", "int a=0; var items=new int[1]; Set(out a,out items[0]); return a+items[0];", "3", null),
                ("distinct-fields", "var holder=new H(); Set(out holder.A,out holder.B); return holder.A+holder.B;", "3", null),
                ("distinct-elements", "var items=new int[2]; Set(out items[0],out items[1]); return items[0]+items[1];", "3", null),
                ("distinct-rectangular-elements", "var items=new int[1,2]; Set(out items[0,0],out items[0,1]); return items[0,0]+items[0,1];", "3", null),
                ("different-array-ranks", "var a=new int[1];var b=new int[1,1];Set(out a[0],out b[0,0]);return a[0]+b[0,0];", "3", null),
                ("field-and-element", "var holder=new H(); var items=new int[1]; Set(out holder.A,out items[0]); return holder.A+items[0];", "3", null),
                ("element-and-field", "var holder=new H(); var items=new int[1]; Set(out items[0],out holder.A); return holder.A+items[0];", "3", null),
                ("element-and-local", "var items=new int[1]; int a=0; Set(out items[0],out a); return items[0]+a;", "3", null),
                ("discard-out", "Set(out _,out var a); return a;", "2", null),
                ("repeated-fields", "var holder=new H(); Set(out holder.A,out holder.A); return holder.A;", "0", "aliased ref/out"),
                ("overlapping-elements", "var items=new int[2]; int index=0; Set(out items[0],out items[index]); return items[0];", "0", "aliased ref/out"),
                ("unknown-first-element", "var items=new int[2]; int index=0; Set(out items[index],out items[0]); return items[0];", "0", "aliased ref/out"),
                ("identical-elements", "var items=new int[2]; Set(out items[0],out items[0]); return items[0];", "0", "aliased ref/out"),
                ("possibly-aliased-fields", "var a=new H(); var b=a; Set(out a.A,out b.A); return a.A;", "0", "aliased ref/out"),
            })
                yield return new(new AuditCase(42, $$"""
                    public static class M
                    {
                        public class H { public int A; public int B; }
                        private static void Set(out int a,out int b) { a=1; b=2; }
                        public static int Run() { {{body}} }
                    }
                    """, failure, Expected: expected)) { DisplayName = "Ref/out boundary: " + name };

            foreach (var (name, source, failure, arguments, expected) in new (string, string, string?, string, string)[]
            {
                ("value-parameters", "private static int Call(int a,int b){Set(out a,out b);return a+b;} private static void Set(out int a,out int b){a=1;b=2;} public static int Run()=>Call(0,0);", null,"[]","3"),
                ("parameter-and-local", "private static int Call(int a){int b;Set(out a,out b);return a+b;} private static void Set(out int a,out int b){a=1;b=2;} public static int Run()=>Call(0);", null,"[]","3"),
                ("parameter-and-field", "public class H{public int A;}private static int Call(int a){var h=new H();Set(out a,out h.A);return a+h.A;} private static void Set(out int a,out int b){a=1;b=2;} public static int Run()=>Call(0);", null,"[]","3"),
                ("value-and-reference-parameter", "private static int Call(int a,ref int b){Set(out a,out b);return a+b;} private static void Set(out int a,out int b){a=1;b=2;} public static int Run(){int a=0;return Call(0,ref a);}", null,"[]","3"),
                ("reference-and-value-parameter", "private static int Call(ref int a,int b){Set(out a,out b);return a+b;} private static void Set(out int a,out int b){a=1;b=2;} public static int Run(){int a=0;return Call(ref a,0);}", null,"[]","3"),
                ("element-and-reference-parameter", "private static void Call(int[]items,ref int a){Set(out items[0],out a);}private static void Set(out int a,out int b){a=1;b=2;}public static int Run(){var a=new int[1];Call(a,ref a[0]);return a[0];}", "aliased ref/out","[]","0"),
                ("field-and-reference-parameter", "public class H{public int A;}private static H holder=new H();private static void Call(ref int a){Set(out holder.A,out a);} private static void Set(out int a,out int b){a=1;b=2;}public static int Run(){Call(ref holder.A);return holder.A;}", "aliased ref/out","[]","0"),
                ("repeated-value-parameter", "private static int Call(int a){Set(out a,out a);return a;} private static void Set(out int a,out int b){a=1;b=2;} public static int Run()=>Call(0);", "aliased ref/out","[]","0"),
                ("byref-parameter-and-local", "private static int Call(ref int a){int b;Set(out a,out b);return a+b;} private static void Set(out int a,out int b){a=1;b=2;} public static int Run(){int a=0;return Call(ref a);}", null,"[]","3"),
                ("captured-local-and-reference", "private static void Set(out int a,out int b){a=1;b=2;} public static int Run(){int a=0;void Call(ref int b){Set(out a,out b);}Call(ref a);return a;}", "aliased ref/out","[]","0"),
                ("byref-parameters", "private static int Call(ref int a,ref int b){Set(out a,out b);return a+b;} private static void Set(out int a,out int b){a=1;b=2;} public static int Run(){int a=0,b=0;return Call(ref a,ref b);}", "aliased ref/out","[]","0"),
                ("dynamic-right-operand", "public static object Run(dynamic a)=>1+a;", "runtime binder","[]","0"),
                ("dynamic-null-operand", "public static bool Run(dynamic a)=>null==a;", "runtime binder","[]","0"),
                ("dynamic-negation", "public static object Run(dynamic a)=>-a;", "runtime binder","[]","0"),
                ("dynamic-compound", "public static object Run(dynamic a){a+=1;return a;}", "runtime binder","[]","0"),
                ("dynamic-right-compound", "public static int Run(dynamic a){int b=1;b+=a;return b;}", "runtime binder","[]","0"),
                ("dynamic-pass-through", "public static object Run(dynamic a)=>a;", null,"[7]","7"),
                ("delegate-compound", "public static System.Action Run(System.Action a,System.Action b){a+=b;return a;}", "Delegate combination","[]","0"),
                ("generic-auto-property", "public class G<T>{public static int Value{get;set;}} public static int Run()=>G<int>.Value;", "Static storage","[]","0"),
                ("generic-constant", "public class G<T>{public const int Value=7;} public static int Run()=>G<int>.Value;", null,"[]","7"),
                ("generic-computed-property", "public class G<T>{public static int Value=>7;} public static int Run()=>G<int>.Value;", null,"[]","7"),
                ("generic-accessor-expression", "public class G<T>{public static int Value{get=>7;}} public static int Run()=>G<int>.Value;", null,"[]","7"),
                ("generic-accessor-body", "public class G<T>{public static int Value{get{return 7;}}} public static int Run()=>G<int>.Value;", null,"[]","7"),
                ("ref-local-function", "private static int field;public static int Run(){ref int Ref()=>ref field;return Ref();}", "ref returns","[]","0"),
                ("readonly-ref-return", "private static int field;public static ref readonly int Read()=>ref field;public static int Run()=>Read();", "ref returns","[]","0"),
                ("ref-parameter-reassignment", "private static void Set(ref int a,ref int b){a=ref b;} public static int Run(){int a=0,b=1;Set(ref a,ref b);return a;}", "Reference aliases","[]","0"),
                ("conditional-ref-target", "public static int Run(bool flag){int a=0,b=1;(flag?ref a:ref b)=3;return a;}", "Reference aliases","[]","0"),
                ("static-interface-property", "public interface I{static abstract int X{get;}}private static int Read<T>()where T:I=>T.X;public static int Run()=>0;", "Static abstract","[]","0"),
                ("default-interface-property", "public interface I{int X=>1;}public class C:I{}public static int Run()=>((I)new C()).X;", "Default interface","[]","0"),
                ("boxed-interface-numeric", "public static bool Run(){System.IComparable number=1;return number is int;}", null,"[]","true"),
                ("boxed-dynamic-numeric", "public static bool Run(){dynamic number=1;return number is double;}", null,"[]","false"),
                ("boxed-enum-numeric", "public enum E{A=1}public static bool Run(){object number=E.A;return number is int;}", null,"[]","false"),
                ("boxed-string-numeric", "public static bool Run(){object value=\"text\";return value is int;}", null,"[]","false"),
                ("boxed-null-numeric", "public static bool Run(){object value=null;return value is long;}", null,"[]","false"),
                ("boxed-nullable-numeric", "public static bool Run(int? value){object boxed=value;return boxed is int;}", "CLR boxing identities","[]","false"),
                ("boxed-known-nullable", "public static bool Run(){int? number=1;object boxed=number;return boxed is int;}", null,"[]","true"),
                ("boxed-loop-write", "public static bool Run(){object value=1;bool result=false;for(int i=0;i<2;i++){result=value is int;value=\"text\";}return result;}", "CLR boxing identities","[]","false"),
                ("boxed-tuple-write", "public static bool Run(){object value=1;int other=0;(other,(value,_))=(0,(2.0,0));return value is int;}", "CLR boxing identities","[]","false"),
                ("boxed-loop-tuple-write", "public static bool Run(){object value=1;int other=0;bool result=false;for(int i=0;i<2;i++){result=value is int;(other,(value,_))=(0,(2.0,0));}return result;}", "CLR boxing identities","[]","false"),
                ("boxed-unrelated-tuple-declaration", "public static bool Run(){object value=1;var (first,second)=(1,2);return value is int;}", null,"[]","true"),
                ("boxed-type-parameter", "public static bool Run<T>(T number){object value=number;return value is int;}", "CLR boxing identities","[]","false"),
                ("nested-boxed-numeric", "public class C{public object Value{get;set;}}public static bool Run(C value)=>value is {Value:int number};", "CLR boxing identities","[]","false"),
                ("nested-boxed-recursive-numeric", "public class C{public object Value{get;set;}}public static bool Run(C value)=>value is {Value:int { }};", "CLR boxing identities","[]","false"),
                ("record-expression-constructor", "public record R{public int X;public R(int x)=>X=x;}public static int Run()=>new R(1).X;", "constructor body","[]","0"),
                ("record-initializer-constructor", "public record R(int X){public R():this(7){}}public static int Run()=>new R().X;", "constructor body","[]","0"),
                ("record-empty-constructor", "public record R{public R(){}}public static int Run(){new R();return 7;}", null,"[]","7"),
                ("explicit-interface-property", "public interface I{int X{get;}}public class C:I{int I.X=>1;}public static int Run()=>((I)new C()).X;", "Explicit interface","[]","0"),
            })
                yield return new(new AuditCase(0, "public static class M {" + source + "}", failure, arguments, expected)) { DisplayName = "Protocol boundary: " + name };
        }
    }

    [TestMethod]
    [DynamicData(nameof(ProtocolCases))]
    public Task ProtocolBoundaries_KeepSafeCallsAndRejectUnimplementedSemantics(AuditCase test)
        => Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(test);

    [TestMethod]
    public Task ArrayDefaults_KeepIndependentElementsAndDimensionOrder()
        => Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(new AuditCase(102, """
            public static class M
            {
                private static int trace;
                private static int Dimension(int value) { trace=trace*10+value; return value; }
                public static int Run()
                {
                    var matrix=new int[Dimension(2),Dimension(3)];
                    matrix[0,0]=1;
                    var tuples=new (int Count,string Name)[2];
                    tuples[0].Count=7;
                    var strings=new string[1];
                    var booleans=new bool[1];
                    var longs=new long[1];
                    return strings[0]==null && !booleans[0] && longs[0]==0 && tuples[1].Name==null
                        ? trace*100+matrix[0,0]*10+matrix[1,0]+tuples[1].Count : -1;
                }
            }
            """, null, Expected: "2310"));

    [TestMethod]
    public Task ModuleDefaults_AreVisibleBeforeAuthoredInitializers()
        => Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(new AuditCase(103, """
            public static class M
            {
                public static int Observed=Read();
                public static int Value=7;
                public static bool Flag;
                public static string Text;
                public static int? Optional;
                private static int Read()=>!Flag && Text==null && Optional==null ? Value : -1;
                public static int Run()=>Observed*10+Value;
            }
            """, null, Expected: "7"));

    [TestMethod]
    public Task BoxedNumericIdentity_PreservesConversionsAndSideEffects()
        => Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(new AuditCase(107, """
            public static class M
            {
                private static int reads;
                private static double Read() { reads++; return 1; }
                public static int Run()
                {
                    double number = 1;
                    object boxed = number;
                    bool exact = boxed is double;
                    bool wrong = ((object)(double)1) is int;
                    bool sideEffect = ((object)Read()) is int;
                    object empty = null;
                    bool nullMatch = empty is int;
                    return (exact && !wrong && !sideEffect && !nullMatch) ? reads : -1;
                }
            }
            """, null, Expected: "1"));

    [TestMethod]
    public Task StructuredPatternSwitch_PreservesBindingsContinueAndSingleEvaluation()
        => Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(new AuditCase(136, """
            public static class M
            {
                private static int reads;
                private static int Read() { reads++; return 3; }
                public static int Run()
                {
                    int result = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        switch (Read())
                        {
                            case int number when number > 0:
                                if (i == 0) continue;
                                result += number;
                                break;
                            default: return -1;
                        }
                    }
                    return result * 10 + reads;
                }
            }
            """, null, Expected: "63"));

    [TestMethod]
    public Task StructuredPatternSwitch_PreservesReturnThroughFinallyAndAwaitedGuard()
        => Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(new AuditCase(137, """
            using System.Threading.Tasks;
            public static class M
            {
                private static int trace;
                private static async Task<bool> Ready(int value) { await Task.CompletedTask; trace += value; return true; }
                private static async Task<int> Select()
                {
                    try
                    {
                        switch (4)
                        {
                            case int number when await Ready(number): return number;
                            default: return 0;
                        }
                    }
                    finally { trace += 10; }
                }
                public static async Task<int> Run() { var value = await Select(); return value * 100 + trace; }
            }
            """, null, Expected: "414"));

    [TestMethod]
    public Task StructuredPatternSwitch_PreservesYieldInTheGenerator()
        => Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(new AuditCase(136, """
            using System.Collections.Generic;
            public static class M
            {
                private static IEnumerable<int> Values()
                {
                    switch (2)
                    {
                        case 0: yield break;
                        case > 0: yield return 3; break;
                        default: yield return 4; break;
                    }
                }
                public static int Run() { foreach(var value in Values()) return value; return 0; }
            }
            """, null, Expected: "3"));

    [TestMethod]
    public Task StructuredPatternSwitch_GroupedCasesReachTheSharedBody()
        => Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(new AuditCase(136,
            "public static class M{public static int Run(int value){switch(value){case 0:case >0:return 3;default:return 4;}}}",
            null, "[0]", "3"));

    public sealed record AuditCase(int Issue, string Source, string? Failure, string Arguments = "[]", string Expected = "null", string? Error = null);

    // The independently authored reproducers preserve the issue's observed behavior. Runtime
    // cases assert values/errors; unsupported protocols assert a source-aware deliberate failure.
    public static IEnumerable<TestDataRow<AuditCase>> Cases =>
    [
        new(new(9, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static object Run(dynamic x) => x + 1; } ", "runtime binder")) { DisplayName = "Issue 9: dynamic-binary" },
        new(new(10, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(dynamic x) => (int)x; } ", "runtime binder")) { DisplayName = "Issue 10: dynamic-conversion" },
        new(new(13, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static void Run(){System.Linq.Expressions.Expression<Func<int,int>> e=x=>x+1;} } ", "Expression tree")) { DisplayName = "Issue 13: expression-trees-local" },
        new(new(17, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static unsafe int Run(int* p) => *p; } ", "Pointer")) { DisplayName = "Issue 17: pointer-dereference" },
        new(new(18, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static unsafe int Run(int* p) => p[1]; } ", "Pointer")) { DisplayName = "Issue 18: pointer-index" },
        new(new(20, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static unsafe int Run(int[] x) { fixed(int* p = x) { return 1; } } } ", "Fixed statements")) { DisplayName = "Issue 20: fixed-statement" },
        new(new(28, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run() { G<int>.Value = 1; G<string>.Value = 2; return G<int>.Value; } } public class G<T> { public static int Value; }", "separate state")) { DisplayName = "Issue 28: generic-static-state" },
        new(new(29, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public interface I<T> where T:I<T>{static abstract T Create();} public static T Run<T>() where T:I<T> => T.Create(); } ", "Static abstract")) { DisplayName = "Issue 29: generic-interface-static" },
        new(new(32, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(int x) {return checked(x+1);} } ", null, "[2147483647]", "null", "OverflowException")) { DisplayName = "Issue 32: checked-integer-add" },
        new(new(33, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static byte Run(int x) {return checked((byte)x);} } ", null, "[256]", "null", "OverflowException")) { DisplayName = "Issue 33: checked-integer-cast" },
        new(new(34, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(int x){return unchecked(x+1);} } ", null, "[2147483647]", "-2147483648", null)) { DisplayName = "Issue 34: unchecked-arithmetic" },
        new(new(35, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(int x,int y)=>x/y; } ", null, "[3,2]", "1", null)) { DisplayName = "Issue 35: integer-division" },
        new(new(36, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(double x)=>(int)x; } ", null, "[1.7]", "1", null)) { DisplayName = "Issue 36: integer-cast" },
        new(new(37, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run() { int x=1; ref int r = ref x; r=2; return x; } } ", "Reference aliases")) { DisplayName = "Issue 37: ref-local" },
        new(new(38, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run() { int x=1,y=2; ref int r=ref x; r=ref y; r=3; return y; } } ", "Reference aliases")) { DisplayName = "Issue 38: ref-reassignment" },
        new(new(39, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(bool flag){int x=1,y=2;ref int r=ref(flag?ref x:ref y);r=3;return x;} } ", "Reference aliases")) { DisplayName = "Issue 39: ref-conditional" },
        new(new(40, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { private static int _value; public static ref int Get() {return ref _value;} public static int Run() { Get() = 2; return _value; } } ", "ref returns")) { DisplayName = "Issue 40: ref-return" },
        new(new(42, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Change(ref int a,ref int b){a=1;return b;}public static int Run(){int x=0;return Change(ref x,ref x);} } ", "aliased ref/out")) { DisplayName = "Issue 42: ref-parameter-aliasing-observable" },
        new(new(74, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static Action Run(Action a,Action b)=>a+b; } ", "Delegate combination")) { DisplayName = "Issue 74: delegate-combination" },
        new(new(76, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public interface I { int Read(); } public class C:I { int I.Read()=>1; } public static int Run()=>((I)new C()).Read(); } ", "Explicit interface")) { DisplayName = "Issue 76: explicit-interface" },
        new(new(77, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public interface I{int Read()=>1;} public class C:I{} public static int Run()=>((I)new C()).Read(); } ", "Default interface")) { DisplayName = "Issue 77: default-interface-method" },
        new(new(98, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public record R { public int Value {get;init;} public R(int value) {Value=value+1;} } public static int Run()=>new R(2).Value; } ", "constructor body")) { DisplayName = "Issue 98: record-custom-constructor" },
        new(new(99, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public record struct R(int X);public static int Run(){var a=new R(1);var b=a;b.X=2;return a.X;} } ", "Mutable record structs")) { DisplayName = "Issue 99: record-struct-copy" },
        new(new(102, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(){var values=new int[2];return values[1];} } ", null, "[]", "0", null)) { DisplayName = "Issue 102: array-default" },
        new(new(103, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Value;public static int Run()=>Value; } ", null, "[]", "0", null)) { DisplayName = "Issue 103: module-field-default" },
        new(new(104, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public sealed class C{public E GetEnumerator()=>new E();} public sealed class E{public int Current=>1;public bool MoveNext()=>false;} public static int Run(){foreach(var x in new C())return x;return 0;} } ", "enumerator patterns")) { DisplayName = "Issue 104: foreach-enumerator" },
        new(new(105, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public class C{public E GetAsyncEnumerator()=>new E();}public class E{public int Current=>1;public async Task<bool> MoveNextAsync(){await Task.CompletedTask;return false;}}public static async Task<int> Run(){await foreach(var x in new C())return x;return 0;} } ", "enumerator patterns")) { DisplayName = "Issue 105: async-foreach-custom-enumerator" },
        new(new(106, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static async Task<int> Run(){return await new C();} } public class C {public A GetAwaiter()=>new A();}public class A:System.Runtime.CompilerServices.INotifyCompletion{public bool IsCompleted=>true;public int GetResult()=>7;public void OnCompleted(Action callback)=>callback();}", "Custom C# awaiters")) { DisplayName = "Issue 106: custom-await-block" },
        new(new(107, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static bool Run(){object x=1;return x is double;} } ", null, "[]", "false", null)) { DisplayName = "Issue 107: box-unbox-type-test" },
        new(new(108, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static string Run(string? x)=>x!; } ", null, "[\"value\"]", "\"value\"", null)) { DisplayName = "Issue 108: nullable-suppression-expression-body" },
        new(new(130, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static object Run(dynamic x)=>!x; } ", "runtime binder")) { DisplayName = "Issue 130: dynamic-unary" },
        new(new(131, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static object Run(dynamic x)=>++x; } ", "runtime binder")) { DisplayName = "Issue 131: dynamic-update" },
        new(new(132, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(dynamic x){if(x)return 1;return 0;} } ", "runtime binder")) { DisplayName = "Issue 132: dynamic-condition" },
        new(new(136, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static int Run(int x){switch(x){case >0:return 1;default:return 2;}} } ", null, "[1]", "1", null)) { DisplayName = "Issue 136: return-pattern-switch" },
        new(new(137, "using System; using System.Collections.Generic; using System.Threading.Tasks; using ECMAScript; public static class M { public static async Task<int> Run(int x){var result=0;switch(x){case >0:await Task.CompletedTask;result=1;break;default:break;}return result;} } ", null, "[1]", "1", null)) { DisplayName = "Issue 137: await-pattern-switch" },
    ];

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public async Task Convert_AuditedBug_PreservesBehaviorOrRejectsUnsupportedProtocol(AuditCase test)
    {
        if (test.Failure is not null)
        {
            Exception? failure = null;
            try { await Compile(test.Source); }
            catch (Exception exception) { failure = exception; }
            Assert.IsNotNull(failure, $"Issue {test.Issue}: unsafe output was accepted.");
            StringAssert.Contains(failure.Message, test.Failure, StringComparison.OrdinalIgnoreCase);
            var location = failure switch
            {
                OperationTransformationException operation => operation.SourceLocation,
                SyntaxNodeTransformationException syntax => syntax.SourceLocation,
                _ => null
            };
            Assert.IsNotNull(location, failure.ToString());
            Assert.IsTrue(location.IsInSource, failure.ToString());
            return;
        }

        var script = await Compile(test.Source);
        Assert.AreEqual(script, await Compile(test.Source), "Emission must be deterministic.");
        _ = new Parser().ParseModule(script);
        var root = Path.Combine(RepositoryTemp.Root, "syntax-audit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "app.mjs"), script);
            var testPath = Path.Combine(root, "test.mjs");
            await File.WriteAllTextAsync(testPath, $$"""
                import { Run } from './app.mjs';
                globalThis.__jazorTest('issue {{test.Issue}}', async () => {
                  let actual, error;
                  try { actual = await Run(...{{test.Arguments}}); }
                  catch (caught) { error = caught; }
                  const expectedError = {{System.Text.Json.JsonSerializer.Serialize(test.Error)}};
                  if (expectedError !== null) {
                    if (!error || !error.message.includes(expectedError)) throw new Error(`expected ${expectedError}, got ${error}`);
                  } else {
                    if (error) throw error;
                    if (actual !== {{test.Expected}}) throw new Error(`unexpected value: ${actual}`);
                  }
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await NodeModuleRunner.RunAsync(root, testPath, cancellationToken: timeout.Token);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static async Task<string> Compile(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, "syntax-audit.cs");
        var compilation = CSharpCompilation.Create("SyntaxAudit", [tree], TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
        var converter = new AstConverter(compilation.GetTypeByMetadataName("M")!, compilation.GetSemanticModel(tree));
        return (await converter.Convert())!.ToKnRECMAScript();
    }
}
