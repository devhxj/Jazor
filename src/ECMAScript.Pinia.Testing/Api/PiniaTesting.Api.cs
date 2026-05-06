using System.ComponentModel;

namespace ECMAScript;

public static partial class PiniaTesting
{
	/// <summary>
	/// Creates a Pinia root instance configured for component/unit testing.
	/// This mirrors <c>createTestingPinia(options?)</c> from <c>@pinia/testing</c>.
	/// </summary>
	/// <param name="options">Optional testing configuration.</param>
	/// <returns>A Pinia instance suitable for test-time store resolution.</returns>
	[Description("@#createTestingPinia")]
	public extern static Pinia.PiniaInstance CreateTestingPinia();

	/// <summary>
	/// Creates a Pinia root instance configured for component/unit testing.
	/// This mirrors <c>createTestingPinia(options?)</c> from <c>@pinia/testing</c>.
	/// </summary>
	/// <param name="options">Optional testing configuration.</param>
	/// <returns>A Pinia instance suitable for test-time store resolution.</returns>
	[Description("@#createTestingPinia")]
	public extern static Pinia.PiniaInstance CreateTestingPinia(TestingOptions options);
}
