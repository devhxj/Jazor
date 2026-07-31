using ECMAScript;
using ECMAScript.Contract;
using static ECMAScript.Vue3;
using ComponentDescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Jazor.ComplierTest;

public sealed record TestShiftedContractComponentOptions<TMarker, TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#name")]
	public string? Name { get; init; }

	[ComponentDescription("@#props")]
	[Props(TypeArgumentIndex = 1)]
	public string[]? Props { get; init; }

	[ComponentDescription("@#emits")]
	[Emits(SourceMemberName = "Bootstrap")]
	public string[]? Emits { get; init; }

	[ComponentDescription("@#setup")]
	public VueTypedSetupCallback<TProps>? Bootstrap { get; init; }
}

public abstract record TestInheritedContractComponentOptionsBase<TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#props")]
	[Props]
	public string[]? Props { get; init; }

	[ComponentDescription("@#emits")]
	[Emits]
	public string[]? Emits { get; init; }
}

public sealed record TestInheritedContractComponentOptions<TProps> : TestInheritedContractComponentOptionsBase<TProps>
	where TProps : VueProps
{
	[ComponentDescription("@#name")]
	public string? Name { get; init; }

	[ComponentDescription("@#setup")]
	public VueTypedSetupCallback<TProps>? Setup { get; init; }
}

public delegate VueRenderCallback TestProbeSetupCallback(TestProbeEmitContext context);

[ECMAScript]
public sealed class TestProbeEmitContext
{
	public static void Emit(int code) { }
	public void Emit() { }
	public void Emit(string name) { }

	public string State = string.Empty;
}

public sealed record TestProbeNestedOptions
{
	[ComponentDescription("@#enabled")]
	public bool Enabled { get; set; }
}

public sealed record TestProbeContractOptions
{
	[ComponentDescription("@#emits")]
	[Emits]
	public string[]? Emits { get; init; }

	[ComponentDescription("@#setup")]
	public TestProbeSetupCallback? Setup { get; init; }

	[ComponentDescription("@#nested")]
	public TestProbeNestedOptions Nested { get; init; } = new();

	public static string StaticMetadata => "ignored";
	public string this[int index] => index.ToString();
}

public sealed record TestStaticEmitSourceOptions
{
	[ComponentDescription("@#emits")]
	[Emits]
	public string[]? Emits { get; init; }

	public static TestProbeSetupCallback? Setup { get; }
}

public sealed record TestInvalidPropsTypeComponentOptions<TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#props")]
	[Props]
	public string? Props { get; init; }
}

public sealed record TestNegativePropsIndexComponentOptions<TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#props")]
	[Props(TypeArgumentIndex = -1)]
	public string[]? Props { get; init; }
}

public sealed record TestMissingPropsTypeArgumentComponentOptions<TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#props")]
	[Props(TypeArgumentIndex = 1)]
	public string[]? Props { get; init; }
}

public sealed record TestInvalidEmitsTypeComponentOptions<TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#emits")]
	[Emits]
	public string? Emits { get; init; }

	[ComponentDescription("@#setup")]
	public VueTypedSetupCallback<TProps>? Setup { get; init; }
}

public sealed record TestMissingEmitsSourceComponentOptions<TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#emits")]
	[Emits(SourceMemberName = "Missing")]
	public string[]? Emits { get; init; }
}

public sealed record TestWhitespaceEmitsSourceComponentOptions<TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#emits")]
	[Emits(SourceMemberName = " ")]
	public string[]? Emits { get; init; }
}

[ECMAScript]
public static class TestExternalSetupCallbacks
{
	public static VueRenderCallback Setup<TProps>(TProps props, VueSetupContext context)
		where TProps : VueProps
		=> static () => null!;
}
