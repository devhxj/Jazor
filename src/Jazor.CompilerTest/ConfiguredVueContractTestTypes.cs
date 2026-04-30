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
	public string[]? PropNames { get; init; }

	[ComponentDescription("@#emits")]
	[Emits(SourceMemberName = "Bootstrap")]
	public string[]? EmitNames { get; init; }

	[ComponentDescription("@#setup")]
	public VueTypedSetupCallback<TProps>? Bootstrap { get; init; }
}

public abstract record TestInheritedContractComponentOptionsBase<TProps> : VueComponentDefinition
	where TProps : VueProps
{
	[ComponentDescription("@#props")]
	[Props]
	public string[]? PropNames { get; init; }

	[ComponentDescription("@#emits")]
	[Emits]
	public string[]? EmitNames { get; init; }
}

public sealed record TestInheritedContractComponentOptions<TProps> : TestInheritedContractComponentOptionsBase<TProps>
	where TProps : VueProps
{
	[ComponentDescription("@#name")]
	public string? Name { get; init; }

	[ComponentDescription("@#setup")]
	public VueTypedSetupCallback<TProps>? Setup { get; init; }
}
