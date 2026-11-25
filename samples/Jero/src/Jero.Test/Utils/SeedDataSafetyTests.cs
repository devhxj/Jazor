using Jero.Core;
using Xunit;

namespace Jero.Test.Utils;

public class SeedDataSafetyTests
{
    [Fact]
    public void SysTenantSeedData_IgnoresUpdatesToPreserveRuntimeInfoSettings()
    {
        var ignoresSeedUpdates = Attribute.IsDefined(
            typeof(SysTenantSeedData),
            typeof(IgnoreUpdateSeedAttribute));

        Assert.True(ignoresSeedUpdates);
    }
}
