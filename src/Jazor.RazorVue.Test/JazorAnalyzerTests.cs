using System.Collections.Immutable;
using Jazor.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class JazorAnalyzerTests
{
	[TestMethod]
	public async Task Jazor_GenericConcreteTypeArgumentInMemberSignature_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using System.Collections.Generic;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public List<Random> Items { get; set; } = new();
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_LocalInferredUnsupportedArrayElementType_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public void Test()
			    {
			        var items = Array.Empty<Random>();
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_CollectionExpressionWithUnsupportedConcreteElementType_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using System.Collections.Generic;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public void Test()
			    {
			        List<Random> list = [];
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnsupportedGenericHostInSignature_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public void Test(Lazy<int> value)
			    {
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_GenericTypeParameterSurface_WithUnsupportedConcreteSignatureType_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using System.Collections.Generic;
			using System.Threading;
			using System.Threading.Tasks;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class ValidModule<T>
			{
			    public List<T> Items { get; set; } = [];

			    public Task<T> Test(Task<T> task, CancellationToken cancellationToken)
			    {
			        return task.WaitAsync(cancellationToken);
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_GenericTypeParameterSurface_WithoutConcreteUnsupportedType_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System.Collections.Generic;
			using System.Threading.Tasks;

			namespace ECMAScript
			{
			    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : System.Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class ValidModule<T>
			{
			    public List<T> Items { get; set; } = [];

			    public Task<T> Test(Task<T> task)
			    {
			        return task;
			    }
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_DefaultUnsupportedConcreteTypeInBody_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public void Test()
			    {
			        var value = default(Random);
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_FakeSupportMarkerOnExternalHost_DoesNotSuppressJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			namespace Fake
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			    }

			    [ECMAScriptModule]
			    public static class Helper
			    {
			        public static int DoWork() => 1;
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public int Test()
			    {
			        return Fake.Helper.DoWork();
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_TypedCatchWithSharedRuntimeAlias_ReportsJAZOR002()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public void Test()
			    {
			        try
			        {
			            throw new InvalidOperationException();
			        }
			        catch (InvalidOperationException)
			        {
			        }
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR002");
	}

	[TestMethod]
	public async Task Jazor_IsTypeWithSharedRuntimeAlias_ReportsJAZOR002()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public bool Test(object value)
			    {
			        return value is InvalidOperationException;
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR002");
	}

	[TestMethod]
	public async Task Jazor_DeclarationPatternWithSharedRuntimeAlias_ReportsJAZOR002()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public bool Test(object value)
			    {
			        return value is InvalidOperationException ex && ex.Message.Length >= 0;
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR002");
	}

	[TestMethod]
	public async Task Jazor_SwitchExpressionPatternWithSharedRuntimeAlias_ReportsJAZOR002()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public int Test(object value)
			    {
			        return value switch
			        {
			            InvalidOperationException => 1,
			            _ => 0
			        };
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR002");
	}

	[TestMethod]
	public async Task Jazor_IsTypeWithGenericErasureFamilyAlias_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System.Threading.Tasks;

			namespace ECMAScript
			{
			    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : System.Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class ValidModule
			{
			    public bool Test(Task<int> task)
			    {
			        return task is Task<int>;
			    }
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001", "JAZOR002");
	}

	[TestMethod]
	public async Task Jazor_SpreadOnNonRecordProperty_ReportsJAZOR003()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public sealed class ChildProps
			    {
			        public string? Name { get; set; }
			    }

			    public sealed class Wrapper
			    {
			        [ECMAScript.Spread]
			        public ChildProps? Child { get; set; }
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR003");
	}

	[TestMethod]
	public async Task Jazor_SpreadCombinedWithExplicitPropertyName_ReportsJAZOR004()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using System.ComponentModel;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public sealed record ChildProps
			    {
			        public string? Name { get; init; }
			    }

			    public sealed record Wrapper
			    {
			        [ECMAScript.Spread]
			        [Description("@#child")]
			        public ChildProps? Child { get; init; }
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR004");
	}

	[TestMethod]
	public async Task Jazor_SpreadOnRecordPropertyWithoutExplicitName_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class ValidModule
			{
			    public sealed record ChildProps
			    {
			        public string? Name { get; init; }
			    }

			    public sealed record Wrapper
			    {
			        [ECMAScript.Spread]
			        public ChildProps? Child { get; init; }
			    }
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR003", "JAZOR004");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordStructuralUsage_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			public sealed record PersonProps(string Name, int Age);

			[ECMAScript.ECMAScriptModule]
			public class ValidModule
			{
			    public PersonProps Create()
			    {
			        var person = new PersonProps("Ada", 37);
			        return person with { Age = person.Age + 1 };
			    }
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordAutoPropertyAccess_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			public sealed record PersonProps
			{
			    public string Name { get; init; } = string.Empty;
			}

			[ECMAScript.ECMAScriptModule]
			public class ValidModule
			{
			    public string Test(PersonProps person)
			    {
			        return person.Name;
			    }
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordComputedPropertyAccess_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			public sealed record PersonProps(string Name)
			{
			    public string UpperName => Name.ToUpperInvariant();
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public string Test(PersonProps person)
			    {
			        return person.UpperName;
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordCustomMethodInvocation_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			public sealed record PersonProps(string Name)
			{
			    public string Format() => Name.ToUpperInvariant();
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public string Test(PersonProps person)
			    {
			        return person.Format();
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_RecordNestedInECMAScriptModuleCustomMethodInvocation_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public sealed record PersonProps(string Name)
			    {
			        public string Format() => Name.ToUpperInvariant();
			    }

			    public string Test(PersonProps person)
			    {
			        return person.Format();
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_RecordNestedInECMAScriptModuleStaticPropertyAccess_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public sealed record PersonProps(string Name)
			    {
			        public static int Version => 1;
			    }

			    public int Test()
			    {
			        return PersonProps.Version;
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordObjectToStringInvocation_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			public sealed record PersonProps(string Name);

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public string Test(PersonProps person)
			    {
			        return person.ToString();
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordObjectEqualsInvocation_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			public sealed record PersonProps(string Name);

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public bool Test(PersonProps left, PersonProps right)
			    {
			        return object.Equals(left, right);
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordEqualityComparerGetHashCodeInvocation_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using System.Collections.Generic;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			public sealed record PersonProps(string Name);

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public int Test(PersonProps person)
			    {
			        return EqualityComparer<PersonProps>.Default.GetHashCode(person);
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_RecordNestedInECMAScriptModuleEqualityOperator_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using ECMAScript;

			namespace ECMAScript
			{
			    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
			    public sealed class ECMAScriptModuleAttribute : Attribute
			    {
			        public ECMAScriptModuleAttribute() { }
			        public ECMAScriptModuleAttribute(string import) { }
			    }
			}

			[ECMAScript.ECMAScriptModule]
			public class InvalidModule
			{
			    public sealed record PersonProps(string Name);

			    public bool Test(PersonProps left, PersonProps right)
			    {
			        return left == right;
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_VueDictionaryIndexerInitializer_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using ECMAScript;
			using static ECMAScript.Vue3;

			[ECMAScriptModule]
			public class ValidModule
			{
			    public VueDictionary Create()
			        => new()
			        {
			            ["aria-live"] = "polite",
			            ["aria-atomic"] = "true"
			        };
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_VueEventHandlersIndexerInitializer_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using ECMAScript;
			using static ECMAScript.Vue3;

			[ECMAScriptModule]
			public class ValidModule
			{
			    public VueEventHandlers<Event> Create()
			        => new()
			        {
			            ["onInput"] = OnInput
			        };

			    private static void OnInput(Event value)
			    {
			    }
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_ECMAScriptRecordProxyIndexerRead_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using System.ComponentModel;
			using ECMAScript;

			[ECMAScript]
			[Description("@#")]
			public record ListenerBag
			{
			    public extern Action? this[string key] { get; set; }
			}

			[ECMAScriptModule]
			public class ValidModule
			{
			    public Action? Get(ListenerBag listeners)
			        => listeners["on:update"];
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_ECMAScriptRecordProxyIndexerAssignment_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using System;
			using System.ComponentModel;
			using ECMAScript;

			[ECMAScript]
			[Description("@#")]
			public record ListenerBag
			{
			    public extern Action? this[string key] { get; set; }
			}

			[ECMAScriptModule]
			public class ValidModule
			{
			    public void Set(ListenerBag listeners, Action value)
			    {
			        listeners["on:update"] = value;
			    }
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_ECMAScriptRecordProxyExternProperty_IsAccepted()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using ECMAScript;
			using static ECMAScript.VueRoute;

			[ECMAScriptModule]
			public class ValidModule
			{
			    public string GetCurrentPath(Router router)
			        => router.CurrentRoute.Value.Path;
			}
			""");

		AssertNoDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordIndexerRead_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using ECMAScript;

			public record PlainBag
			{
			    public string this[string key]
			    {
			        get => key;
			        set { }
			    }
			}

			[ECMAScriptModule]
			public class InvalidModule
			{
			    public string Get(PlainBag bag)
			        => bag["name"];
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	[TestMethod]
	public async Task Jazor_UnmarkedRecordIndexerAssignment_ReportsJAZOR001()
	{
		var diagnostics = await GetAnalyzerDiagnosticsAsync(
			"""
			using ECMAScript;

			public record PlainBag
			{
			    public string this[string key]
			    {
			        get => key;
			        set { }
			    }
			}

			[ECMAScriptModule]
			public class InvalidModule
			{
			    public void Set(PlainBag bag, string value)
			    {
			        bag["name"] = value;
			    }
			}
			""");

		AssertHasDiagnostic(diagnostics, "JAZOR001");
	}

	private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source)
	{
		var compilation = CreateCompilation(source);
		var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new Jazor.Analyzer.Analyzer());

		var compileErrors = compilation.GetDiagnostics()
			.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();
		Assert.AreEqual(0, compileErrors.Length, string.Join(Environment.NewLine, compileErrors.Select(static x => x.ToString())));

		return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
	}

	private static CSharpCompilation CreateCompilation(string source)
	{
		var references = RazorVueMetadataReferences.Create();

		return CSharpCompilation.Create(
			assemblyName: "Jazor.Analyzer.Tests",
			syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
			references: references,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}

	private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string id)
		=> Assert.IsTrue(
			diagnostics.Any(diagnostic => diagnostic.Id == id),
			$"Expected diagnostic {id}, actual: {string.Join(Environment.NewLine, diagnostics.Select(static x => x.ToString()))}");

	private static void AssertNoDiagnostic(IEnumerable<Diagnostic> diagnostics, params string[] ids)
	{
		var unexpected = diagnostics
			.Where(diagnostic => ids.Contains(diagnostic.Id, StringComparer.Ordinal))
			.ToArray();

		Assert.AreEqual(0, unexpected.Length, string.Join(Environment.NewLine, unexpected.Select(static x => x.ToString())));
	}
}
