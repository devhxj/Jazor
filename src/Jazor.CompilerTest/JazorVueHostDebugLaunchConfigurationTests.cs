using Jazor.VueHost.Debug;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostDebugLaunchConfigurationTests
{
    [TestMethod]
    public void LaunchConfiguration_TryParse_PrefersJazorVueHostEntry()
    {
        const string launchJson = """
            {
              "version": "0.2.0",
              "configurations": [
                {
                  "name": "Chrome Attach",
                  "type": "pwa-chrome",
                  "request": "attach",
                  "cdpWebSocketUrl": "ws://127.0.0.1:9222/devtools/page/legacy"
                },
                {
                  "name": "Jazor VueHost DAP",
                  "type": "jazor-vuehost",
                  "request": "launch",
                  "cdpWebSocketUrl": "ws://127.0.0.1:9222/devtools/page/primary"
                }
              ]
            }
            """;

        var configuration = LaunchConfiguration.TryParse(launchJson);

        Assert.IsNotNull(configuration);
        Assert.AreEqual("Jazor VueHost DAP", configuration.Name);
        Assert.AreEqual("jazor-vuehost", configuration.Type);
        Assert.AreEqual("launch", configuration.Request);
        Assert.AreEqual("ws://127.0.0.1:9222/devtools/page/primary", configuration.CdpWebSocketUrl);
    }

    [TestMethod]
    public void LaunchConfiguration_ResolveFromArgs_ReadsDapCdpWsOption()
    {
        var configuration = LaunchConfiguration.ResolveFromArgs(
            [
                "--dap-cdp-ws=ws://127.0.0.1:9222/devtools/page/xyz"
            ],
            workingDirectory: @"D:\repo\jazor");

        Assert.IsNotNull(configuration);
        Assert.AreEqual("jazor-vuehost", configuration.Type);
        Assert.AreEqual("launch", configuration.Request);
        Assert.AreEqual("ws://127.0.0.1:9222/devtools/page/xyz", configuration.CdpWebSocketUrl);
    }
}
