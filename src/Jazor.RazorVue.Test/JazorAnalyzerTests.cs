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
