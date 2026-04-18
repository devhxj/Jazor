using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.DevServer;
using Jazor.VueHost.Extensions;
using Jazor.VueHost.Extensions.Builtin;
using Jazor.VueHost.Jazor.Projection;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.VirtualDocuments.Registry;
using Jazor.VueHost.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostPhase7ExtensionSecurityAndBuiltinTests
{
    [TestMethod]
    public void ExtensionHostOptionsResolver_Resolve_MergesSecurityConfigAndCliOverrides()
    {
        var options = ExtensionHostOptionsResolver.Resolve(
            [
                "--extensions-trusted=trusted.cli.a,trusted.cli.b",
                "--extensions-require-hash=true",
                "--extensions-enforce-provider-permissions=false"
            ],
            rootDirectory: @"D:\repo\phase7",
            config: new JazorConfig
            {
                Extensions = new JazorExtensionsConfig
                {
                    Trusted = ["trusted.config"],
                    RequireAssemblyHash = false,
                    EnforceProviderPermissions = true
                }
            });

        CollectionAssert.AreEquivalent(
            new[] { "trusted.cli.a", "trusted.cli.b" },
            options.TrustedExtensionIds.ToArray());
        Assert.IsTrue(options.RequireAssemblyHash);
        Assert.IsFalse(options.EnforceProviderPermissions);
    }

    [TestMethod]
    public void ExtensionHostOptionsResolver_Resolve_WithInvalidIoCapability_Throws()
    {
        var exception = ExpectInvalidOperationException(
            () => ExtensionHostOptionsResolver.Resolve(
                ["--extensions-max-io-capability=read-only"],
                rootDirectory: @"D:\repo\phase7",
                config: null));

        StringAssert.Contains(
            exception.Message,
            "--extensions-max-io-capability",
            StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void ExtensionHostOptionsResolver_Resolve_WithInvalidNetworkCapabilityInConfig_Throws()
    {
        var exception = ExpectInvalidOperationException(
            () => ExtensionHostOptionsResolver.Resolve(
                args: [],
                rootDirectory: @"D:\repo\phase7",
                config: new JazorConfig
                {
                    Extensions = new JazorExtensionsConfig
                    {
                        MaxNetworkCapability = "public"
                    }
                }));

        StringAssert.Contains(
            exception.Message,
            "extensions.maxNetworkCapability",
            StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void ExtensionHostOptionsResolver_Resolve_WithInvalidBooleanOption_Throws()
    {
        var exception = ExpectInvalidOperationException(
            () => ExtensionHostOptionsResolver.Resolve(
                ["--extensions-require-process-isolation=maybe"],
                rootDirectory: @"D:\repo\phase7",
                config: null));

        StringAssert.Contains(
            exception.Message,
            "--extensions-require-process-isolation",
            StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void ExtensionHostOptionsResolver_Resolve_WithInvalidRetentionOption_Throws()
    {
        var exception = ExpectInvalidOperationException(
            () => ExtensionHostOptionsResolver.Resolve(
                ["--extensions-load-event-retention=abc"],
                rootDirectory: @"D:\repo\phase7",
                config: null));

        StringAssert.Contains(
            exception.Message,
            "--extensions-load-event-retention",
            StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void ExtensionSecurityPolicy_IsAssemblyHashSatisfied_AcceptsNormalizedSha256()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"phase7-hash-{Guid.NewGuid():N}.bin");
        File.WriteAllText(tempFile, "phase7-hash");
        try
        {
            var expectedHash = ComputeSha256Hex(tempFile);
            var prefixedHash = "sha256:" + expectedHash.ToLowerInvariant();

            Assert.IsTrue(ExtensionSecurityPolicy.IsAssemblyHashSatisfied(tempFile, prefixedHash));
            Assert.IsFalse(ExtensionSecurityPolicy.IsAssemblyHashSatisfied(tempFile, "sha256:deadbeef"));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public void ExtensionSecurityPolicy_IsProviderPermissionSatisfied_RejectsMissingCapabilities()
    {
        var deniedManifest = new ExtensionManifest
        {
            Permissions = new ExtensionPermissionManifest
            {
                Providers = ["hover"]
            }
        };
        var allowedManifest = new ExtensionManifest
        {
            Permissions = new ExtensionPermissionManifest
            {
                Providers = ["hover", "completion", "unknown-capability"]
            }
        };

        var denied = ExtensionSecurityPolicy.IsProviderPermissionSatisfied(
            typeof(ManifestLoadableTestExtension),
            deniedManifest,
            out var deniedReason);
        var allowed = ExtensionSecurityPolicy.IsProviderPermissionSatisfied(
            typeof(ManifestLoadableTestExtension),
            allowedManifest,
            out var allowedReason);

        Assert.IsFalse(denied);
        Assert.IsNotNull(deniedReason);
        StringAssert.Contains(deniedReason, "completion", StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(allowed);
        Assert.IsNull(allowedReason);

        var normalized = ExtensionSecurityPolicy.NormalizeAllowedCapabilities(allowedManifest);
        CollectionAssert.AreEquivalent(
            new[] { "hover", "completion" },
            normalized.ToArray());
    }

    [TestMethod]
    public void ExtensionSecurityPolicy_IsSandboxPermissionSatisfied_WithInvalidIoLevel_Rejects()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"phase7-sandbox-root-{Guid.NewGuid():N}"));
        var extensionDirectory = Path.Combine(rootDirectory, ".jazor", "extensions", "invalid-io");
        Directory.CreateDirectory(extensionDirectory);
        try
        {
            var satisfied = ExtensionSecurityPolicy.IsSandboxPermissionSatisfied(
                manifest: new ExtensionManifest
                {
                    Permissions = new ExtensionPermissionManifest
                    {
                        Io = new ExtensionIoPermissionManifest
                        {
                            Level = "read-only"
                        }
                    }
                },
                options: CreateHostOptions(rootDirectory, Path.Combine(rootDirectory, ".jazor", "extensions")),
                rootDirectory: rootDirectory,
                extensionDirectory: extensionDirectory,
                reason: out var reason);

            Assert.IsFalse(satisfied);
            StringAssert.Contains(reason ?? string.Empty, "unsupported io capability", StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public void ExtensionSecurityPolicy_IsSandboxPermissionSatisfied_WithInvalidNetworkLevel_Rejects()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"phase7-sandbox-root-{Guid.NewGuid():N}"));
        var extensionDirectory = Path.Combine(rootDirectory, ".jazor", "extensions", "invalid-network");
        Directory.CreateDirectory(extensionDirectory);
        try
        {
            var satisfied = ExtensionSecurityPolicy.IsSandboxPermissionSatisfied(
                manifest: new ExtensionManifest
                {
                    Permissions = new ExtensionPermissionManifest
                    {
                        Network = new ExtensionNetworkPermissionManifest
                        {
                            Level = "lan"
                        }
                    }
                },
                options: CreateHostOptions(rootDirectory, Path.Combine(rootDirectory, ".jazor", "extensions")),
                rootDirectory: rootDirectory,
                extensionDirectory: extensionDirectory,
                reason: out var reason);

            Assert.IsFalse(satisfied);
            StringAssert.Contains(reason ?? string.Empty, "unsupported network capability", StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithValidManifest_LoadsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-signing-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys),
                CancellationToken.None);

            Assert.AreEqual(1, registry.GetExtensions().Count);
            Assert.IsTrue(registry.GetExtensions().ContainsKey(ManifestLoadableTestExtension.ExtensionId));
            Assert.AreEqual(1, registry.GetLspHoverProviders().Count);
            Assert.AreEqual(1, registry.GetLspCompletionProviders().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProviderPermissionMismatch_SkipsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-provider-permission-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover"],
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys,
                    enforceProviderPermissions: true),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            Assert.AreEqual(0, registry.GetLspHoverProviders().Count);
            Assert.AreEqual(0, registry.GetLspCompletionProviders().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithMissingAssemblyHash_WhenRequired_SkipsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-hash-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: null,
                providers: ["hover", "completion"],
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys,
                    requireAssemblyHash: true),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithManifestIdMismatch_SkipsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-id-mismatch-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: "phase7.manifest.id-mismatch",
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithTrustedAllowList_SkipsUntrustedExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-trusted-list-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedExtensionIds: new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "phase7.other-extension"
                    },
                    trustedPublicKeys: signer.TrustedPublicKeys),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithInvalidManifestSignature_RejectsExtensionAndReportsHealth()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-invalid-signature-key");
            var invalidSignature = new ExtensionSignatureManifest
            {
                KeyId = signer.KeyId,
                Algorithm = "RS256",
                Value = Convert.ToBase64String("invalid-signature"u8.ToArray())
            };
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                explicitSignature: invalidSignature);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = registry.GetExtensionLoadHealth()
                .Single(static item =>
                    string.Equals(item.ExtensionId, ManifestLoadableTestExtension.ExtensionId, StringComparison.Ordinal)
                    && string.Equals(item.Source, "user", StringComparison.Ordinal));
            Assert.AreEqual(0, loadHealth.LoadedCount);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            Assert.AreEqual(0, loadHealth.FailedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "signature",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithUnknownManifestSignatureKey_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            var unknownKeySignature = new ExtensionSignatureManifest
            {
                KeyId = "phase7.unknown-signing-key",
                Algorithm = "RS256",
                Value = "AA=="
            };
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                explicitSignature: unknownKeySignature);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = registry.GetExtensionLoadHealth()
                .Single(static item => string.Equals(item.ExtensionId, ManifestLoadableTestExtension.ExtensionId, StringComparison.Ordinal));
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "not configured",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithSignatureRequirementDisabled_LoadsUnsignedManifest()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"]);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    requireManifestSignature: false),
                CancellationToken.None);

            Assert.AreEqual(1, registry.GetExtensions().Count);
            Assert.IsTrue(registry.GetExtensions().ContainsKey(ManifestLoadableTestExtension.ExtensionId));
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithUnsupportedManifestSignatureAlgorithm_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-unsupported-alg-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                explicitSignature: new ExtensionSignatureManifest
                {
                    KeyId = signer.KeyId,
                    Algorithm = "ES256",
                    Value = "AA=="
                });

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = GetSingleUserLoadHealth(registry);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "algorithm",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithMalformedManifestSignatureEncoding_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-malformed-signature-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                explicitSignature: new ExtensionSignatureManifest
                {
                    KeyId = signer.KeyId,
                    Algorithm = "RS256",
                    Value = "%%%bad-base64%%%"
                });

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = GetSingleUserLoadHealth(registry);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "base64",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public void ExtensionSecurityPolicy_IsManifestSignatureSatisfied_WithMalformedTrustedKeyData_ReturnsFalse()
    {
        var manifest = new ExtensionManifest
        {
            Id = "phase7.invalid-public-key",
            Assembly = "sample.dll",
            AssemblySha256 = "sha256:deadbeef",
            Type = "Sample.Extension",
            Permissions = new ExtensionPermissionManifest
            {
                Providers = ["hover"],
                ProcessIsolation = false
            },
            Signature = new ExtensionSignatureManifest
            {
                KeyId = "phase7.invalid-public-key",
                Algorithm = "RS256",
                Value = "AA=="
            }
        };

        var satisfied = ExtensionSecurityPolicy.IsManifestSignatureSatisfied(
            manifest,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["phase7.invalid-public-key"] = "this-is-not-a-valid-pem-key"
            },
            out var reason);

        Assert.IsFalse(satisfied);
        StringAssert.Contains(
            reason ?? string.Empty,
            "public key",
            StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithPermissionTamperingAfterSigning_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-permission-tamper-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                signer: signer);

            var manifestPath = Path.Combine(sandbox.ExtensionDirectory, "extension.json");
            var signedManifest = JsonSerializer.Deserialize<ExtensionManifest>(
                File.ReadAllText(manifestPath));
            Assert.IsNotNull(signedManifest);

            var tamperedManifest = new ExtensionManifest
            {
                Id = signedManifest!.Id,
                Assembly = signedManifest.Assembly,
                AssemblySha256 = signedManifest.AssemblySha256,
                Type = signedManifest.Type,
                Permissions = new ExtensionPermissionManifest
                {
                    Providers = signedManifest.Permissions?.Providers ?? Array.Empty<string>(),
                    ProcessIsolation = true,
                    Io = signedManifest.Permissions?.Io,
                    Network = signedManifest.Permissions?.Network
                },
                Signature = signedManifest.Signature,
                Settings = signedManifest.Settings
            };
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(tamperedManifest));

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = GetSingleUserLoadHealth(registry);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "signature",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithRequiredProcessIsolationAndMissingManifestFlag_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-process-isolation-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                processIsolation: false,
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys,
                    requireProcessIsolation: true),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = GetSingleUserLoadHealth(registry);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "process-level isolation",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithIoCapabilityExceedingHostPolicy_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-io-capability-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityReadWrite,
                    ReadRoots = ["./data"],
                    WriteRoots = ["./data"]
                },
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys,
                    maxIoCapability: ExtensionHostOptions.IoCapabilityRead),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = GetSingleUserLoadHealth(registry);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "io capability",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithLoopbackNetworkPolicyAndPublicHost_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-loopback-network-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                networkPermission: new ExtensionNetworkPermissionManifest
                {
                    Level = ExtensionHostOptions.NetworkCapabilityLoopback,
                    AllowedHosts = ["example.com"]
                },
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys,
                    maxNetworkCapability: ExtensionHostOptions.NetworkCapabilityLoopback),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = GetSingleUserLoadHealth(registry);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "loopback",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithIoPermissionPathEscapingBoundary_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-io-boundary-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityRead,
                    ReadRoots = ["../../../../outside-root"]
                },
                signer: signer);

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys,
                    maxIoCapability: ExtensionHostOptions.IoCapabilityRead),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = GetSingleUserLoadHealth(registry);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "escapes extension/root boundary",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_DisposeAsync_AfterUserLoad_AllowsAssemblyReplacement()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-unload-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                signer: signer);

            var registry = new ExtensionRegistry();
            await using (var loader = new ExtensionLoader(registry))
            {
                await loader.LoadUserExtensionsAsync(
                    CreateHostOptions(
                        sandbox.RootDirectory,
                        sandbox.ExtensionsDirectory,
                        trustedPublicKeys: signer.TrustedPublicKeys),
                    CancellationToken.None);
            }

            Assert.AreEqual(0, registry.GetExtensions().Count);
            File.Copy(typeof(ManifestLoadableTestExtension).Assembly.Location, sandbox.AssemblyPath, overwrite: true);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task BuiltinExtensionCatalog_LoadBuiltinExtensionsAsync_RegistersProductionProviders()
    {
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        CollectionAssert.Contains(
            registry.GetLspDiagnosticProviders().Select(static provider => provider.Name).ToArray(),
            "BuiltinStructureDiagnosticProvider");
        CollectionAssert.Contains(
            registry.GetLspCompletionProviders().Select(static provider => provider.Name).ToArray(),
            "BuiltinDirectiveCompletionProvider");
        CollectionAssert.Contains(
            registry.GetLspCodeActionProviders().Select(static provider => provider.Name).ToArray(),
            "BuiltinComponentCodeActionProvider");
        CollectionAssert.Contains(
            registry.GetLspWorkspaceSymbolProviders().Select(static provider => provider.Name).ToArray(),
            "BuiltinWorkspaceSymbolProvider");
    }

    [TestMethod]
    public async Task BuiltinStructureDiagnosticProvider_ReportsTemplateAndCodeShapeIssues()
    {
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        var provider = registry.GetLspDiagnosticProviders()
            .Single(static item => string.Equals(item.Name, "BuiltinStructureDiagnosticProvider", StringComparison.Ordinal));
        var document = new DocumentSnapshot(
            documentPath: Path.Combine(Path.GetTempPath(), $"phase7-structure-{Guid.NewGuid():N}.jazor"),
            documentKind: DocumentKind.Jazor,
            text: """
                  <div>hello</div>
                  @code {
                    private int count = 0;
                  """,
            version: "1");
        var diagnostics = await provider.ProvideDiagnosticsAsync(
            new LspDiagnosticProviderContext(document, Array.Empty<LspDiagnostic>()),
            CancellationToken.None);
        var diagnosticCodes = diagnostics.Select(static item => item.Code).ToArray();

        CollectionAssert.Contains(diagnosticCodes, "JAZORVUEEXTSTR004");
        CollectionAssert.Contains(diagnosticCodes, "JAZORVUEEXTSTR005");
    }

    [TestMethod]
    public async Task BuiltinDirectiveCompletionProvider_ServesDirectiveCompletionsThroughLspSession()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-completion-{Guid.NewGuid():N}.jazor");
        await workspaceStore.UpsertDocumentAsync(
            new DocumentSnapshot(documentPath, DocumentKind.Jazor, "@c", version: "1"),
            CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            registry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 3101,
                Method = "textDocument/completion",
                Params = new LspCompletionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 0, Character = 2 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var items = response.Result as IReadOnlyList<LspCompletionItem>;
        Assert.IsNotNull(items);
        Assert.IsTrue(items.Any(static item => string.Equals(item.Label, "@code", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task BuiltinComponentCodeActionProvider_OffersVueImportQuickFixThroughLspSession()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-code-action-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var vuePath = Path.Combine(rootDirectory, "CounterWidget.vue");
            await File.WriteAllTextAsync(vuePath, "<template><div /></template>");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(
                    documentPath,
                    DocumentKind.Jazor,
                    """
                    <template>
                      <CounterWidget />
                    </template>
                    """,
                    version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var response = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3102,
                    Method = "textDocument/codeAction",
                    Params = new LspCodeActionParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Range = new LspRange
                        {
                            Start = new LspPosition { Line = 1, Character = 3 },
                            End = new LspPosition { Line = 1, Character = 18 }
                        },
                        Context = new LspCodeActionContext
                        {
                            Diagnostics =
                            [
                                new LspDiagnostic
                                {
                                    Range = new LspRange
                                    {
                                        Start = new LspPosition { Line = 1, Character = 3 },
                                        End = new LspPosition { Line = 1, Character = 18 }
                                    },
                                    Severity = 1,
                                    Code = "JAZORVUEFRONTEND001",
                                    Source = "Jazor.VueHost.Frontend",
                                    Message = "Unable to resolve component CounterWidget."
                                }
                            ]
                        }
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(response);
            Assert.IsNull(response!.Error);
            var actions = response.Result as IReadOnlyList<LspCodeAction>;
            Assert.IsNotNull(actions);

            var importAction = actions.FirstOrDefault(static action =>
                string.Equals(action.Title, "Add @vueimport for CounterWidget", StringComparison.Ordinal));
            Assert.IsNotNull(importAction);
            Assert.IsNotNull(importAction!.Edit);

            var uri = LspProtocolHelpers.ToDocumentUri(documentPath);
            Assert.IsTrue(importAction.Edit.Changes.ContainsKey(uri));
            var inserted = importAction.Edit.Changes[uri].Single();
            StringAssert.Contains(inserted.NewText, "@vueimport CounterWidget from \"./CounterWidget.vue\"", StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task BuiltinWorkspaceSymbolProvider_IndexesOpenDocumentsWithStableOrdering()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-workspace-symbol-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            var jazorPath = Path.Combine(rootDirectory, "Counter.jazor");
            var csharpPath = Path.Combine(rootDirectory, "DataService.cs");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(
                    jazorPath,
                    DocumentKind.Jazor,
                    """
                    <template>
                      <TodoItem />
                    </template>
                    @code {
                        public void LoadData() { }
                    }
                    """,
                    version: "1"),
                CancellationToken.None);
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(
                    csharpPath,
                    DocumentKind.CSharp,
                    "public class DataService { public void SaveRecord() { } }",
                    version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var filteredResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3103,
                    Method = "workspace/symbol",
                    Params = new LspWorkspaceSymbolParams
                    {
                        Query = "Load"
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(filteredResponse);
            Assert.IsNull(filteredResponse!.Error);
            var filteredSymbols = filteredResponse.Result as IReadOnlyList<LspWorkspaceSymbol>;
            Assert.IsNotNull(filteredSymbols);
            Assert.IsTrue(filteredSymbols.Any(static symbol => string.Equals(symbol.Name, "LoadData", StringComparison.Ordinal)));

            var allResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3104,
                    Method = "workspace/symbol",
                    Params = new LspWorkspaceSymbolParams
                    {
                        Query = string.Empty
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(allResponse);
            Assert.IsNull(allResponse!.Error);
            var allSymbols = allResponse.Result as IReadOnlyList<LspWorkspaceSymbol>;
            Assert.IsNotNull(allSymbols);
            Assert.IsTrue(allSymbols.Count >= 2);

            var actualOrder = allSymbols.Select(static symbol => symbol.Name).ToArray();
            var expectedOrder = actualOrder
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            CollectionAssert.AreEqual(expectedOrder, actualOrder);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task LspSession_ExtensionLoadHealth_Request_ExposesLoadHealthSnapshot()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var registry = new ExtensionRegistry();
        registry.ReportExtensionLoad(new ExtensionLoadInvocation(
            ExtensionId: ManifestLoadableTestExtension.ExtensionId,
            Source: "user",
            ExtensionDirectory: Path.GetTempPath(),
            ManifestPath: null,
            AssemblyPath: null,
            Status: ExtensionLoadStatus.Rejected,
            Reason: "manifest signature verification failed",
            Timestamp: DateTimeOffset.UtcNow));

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            registry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 3105,
                Method = "jazor/extensionLoadHealth"
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var health = response.Result as IReadOnlyList<ExtensionLoadHealth>;
        Assert.IsNotNull(health);
        var item = health.Single(static entry =>
            string.Equals(entry.ExtensionId, ManifestLoadableTestExtension.ExtensionId, StringComparison.Ordinal)
            && string.Equals(entry.Source, "user", StringComparison.Ordinal));
        Assert.AreEqual(0, item.LoadedCount);
        Assert.AreEqual(1, item.RejectedCount);
    }

    [TestMethod]
    public async Task LspSession_ExtensionObservabilityDashboard_Request_ExposesDashboardSnapshot()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var registry = new ExtensionRegistry();
        registry.ReportExtensionLoad(new ExtensionLoadInvocation(
            ExtensionId: ManifestLoadableTestExtension.ExtensionId,
            Source: "user",
            ExtensionDirectory: Path.GetTempPath(),
            ManifestPath: null,
            AssemblyPath: null,
            Status: ExtensionLoadStatus.Loaded,
            Reason: "extension loaded",
            Timestamp: DateTimeOffset.UtcNow));
        registry.ReportProviderInvocation(new ExtensionProviderInvocation(
            ProviderName: "ManifestLoadableHoverProvider",
            Capability: "hover",
            Duration: TimeSpan.FromMilliseconds(5),
            Succeeded: true,
            TimedOut: false,
            Skipped: false,
            ErrorMessage: null));

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            registry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 3106,
                Method = "jazor/extensionObservabilityDashboard"
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var dashboard = response.Result as ExtensionObservabilityDashboard;
        Assert.IsNotNull(dashboard);
        Assert.AreEqual(1, dashboard.LoadHealth.Count);
        Assert.AreEqual(1, dashboard.ProviderHealth.Count);
        Assert.AreEqual(1, dashboard.RecentLoadEvents.Count);
    }

    private static InvalidOperationException ExpectInvalidOperationException(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }

        throw new AssertFailedException("Expected InvalidOperationException was not thrown.");
    }

    private static ExtensionHostOptions CreateHostOptions(
        string rootDirectory,
        string extensionsDirectory,
        IReadOnlySet<string>? trustedExtensionIds = null,
        IReadOnlyDictionary<string, string>? trustedPublicKeys = null,
        bool requireAssemblyHash = true,
        bool enforceProviderPermissions = true,
        bool requireManifestSignature = true,
        bool requireProcessIsolation = false,
        string maxIoCapability = ExtensionHostOptions.IoCapabilityRead,
        string maxNetworkCapability = ExtensionHostOptions.NetworkCapabilityLoopback)
    {
        return new ExtensionHostOptions
        {
            RootDirectory = rootDirectory,
            Enabled = true,
            ExtensionsDirectory = extensionsDirectory,
            AllowExternalDirectory = false,
            TrustedExtensionIds = trustedExtensionIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            TrustedPublicKeys = trustedPublicKeys ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            RequireAssemblyHash = requireAssemblyHash,
            EnforceProviderPermissions = enforceProviderPermissions,
            RequireManifestSignature = requireManifestSignature,
            RequireProcessIsolation = requireProcessIsolation,
            MaxIoCapability = maxIoCapability,
            MaxNetworkCapability = maxNetworkCapability
        };
    }

    private static void WriteManifest(
        string extensionDirectory,
        string id,
        string assembly,
        string type,
        string? assemblySha256,
        string[] providers,
        ExtensionIoPermissionManifest? ioPermission = null,
        ExtensionNetworkPermissionManifest? networkPermission = null,
        bool? processIsolation = null,
        ManifestSigner? signer = null,
        ExtensionSignatureManifest? explicitSignature = null)
    {
        var permissions = CreatePermissionsManifest(
            providers,
            ioPermission,
            networkPermission,
            processIsolation);
        var unsignedManifest = new ExtensionManifest
        {
            Id = id,
            Assembly = assembly,
            AssemblySha256 = assemblySha256,
            Type = type,
            Permissions = permissions
        };

        var finalSignature = explicitSignature;
        if (finalSignature is null && signer is not null)
        {
            finalSignature = signer.CreateManifestSignature(unsignedManifest);
        }

        var manifest = new ExtensionManifest
        {
            Id = id,
            Assembly = assembly,
            AssemblySha256 = assemblySha256,
            Type = type,
            Permissions = CreatePermissionsManifest(
                providers,
                ioPermission,
                networkPermission,
                processIsolation),
            Signature = finalSignature
        };

        var manifestPath = Path.Combine(extensionDirectory, "extension.json");
        File.WriteAllText(
            manifestPath,
            JsonSerializer.Serialize(manifest));
    }

    private static ExtensionPermissionManifest CreatePermissionsManifest(
        string[] providers,
        ExtensionIoPermissionManifest? ioPermission,
        ExtensionNetworkPermissionManifest? networkPermission,
        bool? processIsolation)
    {
        return new ExtensionPermissionManifest
        {
            Providers = providers,
            Io = ioPermission,
            Network = networkPermission,
            ProcessIsolation = processIsolation
        };
    }

    private static ExtensionLoadHealth GetSingleUserLoadHealth(ExtensionRegistry registry)
    {
        return registry.GetExtensionLoadHealth()
            .Single(static item =>
                string.Equals(item.ExtensionId, ManifestLoadableTestExtension.ExtensionId, StringComparison.Ordinal)
                && string.Equals(item.Source, "user", StringComparison.Ordinal));
    }

    private static ExtensionSandbox CreateExtensionSandbox()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-extension-sandbox-{Guid.NewGuid():N}");
        var extensionsDirectory = Path.Combine(rootDirectory, ".jazor", "extensions");
        var extensionDirectory = Path.Combine(extensionsDirectory, "manifest-loadable");
        Directory.CreateDirectory(extensionDirectory);

        var sourceAssemblyPath = typeof(ManifestLoadableTestExtension).Assembly.Location;
        var assemblyFileName = "manifest-loadable.dll";
        var copiedAssemblyPath = Path.Combine(extensionDirectory, assemblyFileName);
        File.Copy(sourceAssemblyPath, copiedAssemblyPath, overwrite: true);

        return new ExtensionSandbox(
            rootDirectory,
            extensionsDirectory,
            extensionDirectory,
            assemblyFileName,
            copiedAssemblyPath,
            ComputeSha256Hex(copiedAssemblyPath));
    }

    private static string ComputeSha256Hex(string filePath)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(filePath)));

    private static LspSession CreateSession(
        IVueHostWorkspaceStore workspaceStore,
        IVirtualDocumentRegistry virtualDocumentRegistry,
        ILspLane[] lanes,
        Stream outputStream,
        IExtensionRegistry extensionRegistry)
    {
        var laneRouter = new LspLaneRouter();
        var projectionResolver = new DocumentProjectionResolver(
            new DocumentRegionClassifier(),
            virtualDocumentRegistry);
        var projectionService = new JazorProjectionService();
        var resultAggregator = new LspResultAggregator();
        var markupBridgeService = new MarkupComponentBridgeService(workspaceStore);
        var markupBridgeFanout = new MarkupBridgeFanoutCoordinator(markupBridgeService, resultAggregator);
        var laneMap = lanes.ToDictionary(static lane => lane.LaneKind);

        return new LspSession(
            workspaceStore,
            lanes,
            laneRouter,
            new LspMessageWriter(outputStream),
            projectionService,
            virtualDocumentRegistry,
            projectionResolver,
            resultAggregator,
            markupBridgeFanout,
            new ReferenceCoordinator(laneMap, laneRouter, markupBridgeFanout),
            new RenameCoordinator(laneMap, laneRouter, resultAggregator, markupBridgeFanout),
            new CodeActionCoordinator(laneMap, laneRouter, resultAggregator),
            workspaceDocumentChangeSink: null,
            extensionRegistry: extensionRegistry);
    }

    private sealed class EmptyJazorLane : ILspLane
    {
        public LaneKind LaneKind => LaneKind.Jazor;

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

        public ValueTask<LspHoverResult?> GetHoverAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspHoverResult?>(null);

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

        public ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());

        public ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspSignatureHelp?>(null);

        public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(DocumentSnapshot document, LspPosition position, bool includeDeclaration, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<LspWorkspaceEdit?> GetRenameAsync(DocumentSnapshot document, LspPosition position, string newName, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspWorkspaceEdit?>(null);

        public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(DocumentSnapshot document, LspRange range, IReadOnlyList<LspDiagnostic> diagnostics, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
    }

    private sealed class ExtensionSandbox(
        string rootDirectory,
        string extensionsDirectory,
        string extensionDirectory,
        string assemblyFileName,
        string assemblyPath,
        string assemblySha256) : IDisposable
    {
        public string RootDirectory { get; } = rootDirectory;

        public string ExtensionsDirectory { get; } = extensionsDirectory;

        public string ExtensionDirectory { get; } = extensionDirectory;

        public string AssemblyFileName { get; } = assemblyFileName;

        public string AssemblyPath { get; } = assemblyPath;

        public string AssemblySha256 { get; } = assemblySha256;

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }

    private sealed class ManifestSigner : IDisposable
    {
        private readonly RSA _rsa;

        public ManifestSigner(string keyId)
        {
            _rsa = RSA.Create(2048);
            KeyId = keyId;
            TrustedPublicKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [keyId] = _rsa.ExportSubjectPublicKeyInfoPem()
            };
        }

        public string KeyId { get; }

        public IReadOnlyDictionary<string, string> TrustedPublicKeys { get; }

        public ExtensionSignatureManifest CreateManifestSignature(ExtensionManifest unsignedManifest)
        {
            var payload = ExtensionSecurityPolicy.BuildManifestSignaturePayload(unsignedManifest);
            var signatureBytes = _rsa.SignData(
                Encoding.UTF8.GetBytes(payload),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            return new ExtensionSignatureManifest
            {
                KeyId = KeyId,
                Algorithm = "RS256",
                Value = Convert.ToBase64String(signatureBytes)
            };
        }

        public void Dispose()
        {
            _rsa.Dispose();
        }
    }
}

public sealed class ManifestLoadableTestExtension : IExtension, ILspHoverProvider, ILspCompletionProvider
{
    public const string ExtensionId = "phase7.manifest-loadable";

    private static readonly ExtensionMetadata MetadataValue = new(
        Id: ExtensionId,
        Name: "Manifest Loadable Test Extension",
        Version: "1.0.0");

    ExtensionMetadata IExtension.Metadata => MetadataValue;

    string ILspHoverProvider.Name => "ManifestLoadableHoverProvider";

    int ILspHoverProvider.Priority => 10;

    string ILspCompletionProvider.Name => "ManifestLoadableCompletionProvider";

    int ILspCompletionProvider.Priority => 10;

    ValueTask IExtension.InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask<LspHoverResult?> ILspHoverProvider.ProvideHoverAsync(
        LspHoverProviderContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<LspHoverResult?>(null);
    }

    ValueTask<IReadOnlyList<LspCompletionItem>> ILspCompletionProvider.ProvideCompletionItemsAsync(
        LspCompletionProviderContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());
    }
}
