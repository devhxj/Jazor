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
