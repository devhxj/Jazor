using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jolt.DevServer;
using Jolt.Extensions;
using Jolt.Extensions.Builtin;
using Jolt.Jazor.Projection;
using Jolt.Lsp;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jazor.RazorVue.Protocol;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltPhase7ExtensionSecurityAndBuiltinTests
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
    public void ExtensionHostOptionsResolver_Resolve_MergesProviderLogAndRetentionOverrides()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-provider-options-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            var options = ExtensionHostOptionsResolver.Resolve(
                [
                    "--extensions-provider-log-file=logs/providers-cli.jsonl",
                    "--extensions-provider-event-retention=321"
                ],
                rootDirectory: rootDirectory,
                config: new JazorConfig
                {
                    Extensions = new JazorExtensionsConfig
                    {
                        ProviderLogFile = "logs/providers-config.jsonl",
                        ProviderEventRetention = 123
                    }
                });

            Assert.AreEqual(
                Path.GetFullPath(Path.Combine(rootDirectory, "logs", "providers-cli.jsonl")),
                options.ProviderLogFilePath);
            Assert.AreEqual(321, options.ProviderEventRetention);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void ExtensionHostOptionsResolver_Resolve_WithInvalidProviderRetentionOption_Throws()
    {
        var exception = ExpectInvalidOperationException(
            () => ExtensionHostOptionsResolver.Resolve(
                ["--extensions-provider-event-retention=not-number"],
                rootDirectory: @"D:\repo\phase7",
                config: null));

        StringAssert.Contains(
            exception.Message,
            "--extensions-provider-event-retention",
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
    public void ExtensionSecurityPolicy_CreateRuntimeSandboxProfile_AppliesDefaultRootsAndLoopbackHosts()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"phase7-runtime-profile-root-{Guid.NewGuid():N}"));
        var extensionDirectory = Path.Combine(rootDirectory, ".jazor", "extensions", "runtime-profile");
        Directory.CreateDirectory(extensionDirectory);
        try
        {
            var profile = ExtensionSecurityPolicy.CreateRuntimeSandboxProfile(
                manifest: new ExtensionManifest
                {
                    Permissions = new ExtensionPermissionManifest
                    {
                        Io = new ExtensionIoPermissionManifest
                        {
                            Level = ExtensionHostOptions.IoCapabilityReadWrite
                        },
                        Network = new ExtensionNetworkPermissionManifest
                        {
                            Level = ExtensionHostOptions.NetworkCapabilityLoopback
                        }
                    }
                },
                rootDirectory: rootDirectory,
                extensionDirectory: extensionDirectory);

            Assert.AreEqual(ExtensionHostOptions.IoCapabilityReadWrite, profile.IoCapability);
            CollectionAssert.Contains(profile.ReadRoots, Path.GetFullPath(rootDirectory));
            CollectionAssert.Contains(profile.ReadRoots, Path.GetFullPath(extensionDirectory));
            CollectionAssert.AreEquivalent(
                new[] { Path.GetFullPath(extensionDirectory) },
                profile.WriteRoots);

            CollectionAssert.Contains(profile.AllowedHosts, "localhost");
            CollectionAssert.Contains(profile.AllowedHosts, "127.0.0.1");
            CollectionAssert.Contains(profile.AllowedHosts, "::1");

            Assert.IsTrue(profile.IsReadPathAllowed(Path.Combine(rootDirectory, "runtime-profile.jazor")));
            Assert.IsTrue(profile.IsWritePathAllowed(Path.Combine(extensionDirectory, "cache", "state.json")));
            Assert.IsFalse(profile.IsWritePathAllowed(Path.Combine(rootDirectory, "blocked-output.txt")));
            Assert.IsTrue(profile.IsNetworkHostAllowed("localhost"));
            Assert.IsFalse(profile.IsNetworkHostAllowed("example.com"));
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
    public void ExtensionSecurityPolicy_CreateRuntimeSandboxProfile_WithIoNoneAndNetworkNone_DeniesRuntimeAccess()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"phase7-runtime-none-root-{Guid.NewGuid():N}"));
        var extensionDirectory = Path.Combine(rootDirectory, ".jazor", "extensions", "runtime-none");
        Directory.CreateDirectory(extensionDirectory);
        try
        {
            var profile = ExtensionSecurityPolicy.CreateRuntimeSandboxProfile(
                manifest: new ExtensionManifest
                {
                    Permissions = new ExtensionPermissionManifest
                    {
                        Io = new ExtensionIoPermissionManifest
                        {
                            Level = ExtensionHostOptions.IoCapabilityNone
                        },
                        Network = new ExtensionNetworkPermissionManifest
                        {
                            Level = ExtensionHostOptions.NetworkCapabilityNone
                        }
                    }
                },
                rootDirectory: rootDirectory,
                extensionDirectory: extensionDirectory);

            Assert.IsFalse(profile.IsReadPathAllowed(Path.Combine(extensionDirectory, "input.txt")));
            Assert.IsFalse(profile.IsWritePathAllowed(Path.Combine(extensionDirectory, "output.txt")));
            Assert.IsFalse(profile.IsNetworkHostAllowed("localhost"));
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
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WhenRootEnumerationFails_ReportsWarningAndContinues()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "JoltExtensionLoaderTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootDirectory);
        try
        {
            var extensionRoot = Path.Combine(rootDirectory, ".jazor", "extensions");
            Directory.CreateDirectory(extensionRoot);
            var loadEvents = new List<ExtensionLoadInvocation>();
            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(
                registry,
                loadEvents.Add,
                _ => throw new IOException("extensions root disappeared"));

            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(rootDirectory, extensionRoot),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            Assert.AreEqual(1, loadEvents.Count);
            Assert.AreEqual(ExtensionLoadStatus.Failed, loadEvents[0].Status);
            StringAssert.Contains(loadEvents[0].Reason, "extensions root enumeration failed");
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
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithLegacyManifestVersion0_MigratesAndLoadsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            var manifestPath = Path.Combine(sandbox.ExtensionDirectory, "extension.json");
            var legacyManifest = new
            {
                manifestVersion = 0,
                id = ManifestLoadableTestExtension.ExtensionId,
                main = sandbox.AssemblyFileName,
                entryType = typeof(ManifestLoadableTestExtension).FullName,
                assemblySha256 = sandbox.AssemblySha256,
                capabilities = new[] { "hover", "completion" },
                processIsolation = false
            };
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(legacyManifest));

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
            Assert.AreEqual(1, registry.GetLspHoverProviders().Count);
            Assert.AreEqual(1, registry.GetLspCompletionProviders().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithLegacySchemaVersionStringAndDelimitedCapabilities_MigratesAndLoadsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            var manifestPath = Path.Combine(sandbox.ExtensionDirectory, "extension.json");
            var legacyManifest = new
            {
                schemaVersion = "0",
                id = ManifestLoadableTestExtension.ExtensionId,
                assemblyPath = sandbox.AssemblyFileName,
                typeName = typeof(ManifestLoadableTestExtension).FullName,
                assemblyHash = sandbox.AssemblySha256,
                capabilities = "hover, completion",
                processIsolation = "false"
            };
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(legacyManifest));

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
            Assert.AreEqual(1, registry.GetLspHoverProviders().Count);
            Assert.AreEqual(1, registry.GetLspCompletionProviders().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithLegacyFieldsWithoutVersion_MigratesAndLoadsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            var manifestPath = Path.Combine(sandbox.ExtensionDirectory, "extension.json");
            var legacyManifest = new
            {
                id = ManifestLoadableTestExtension.ExtensionId,
                main = sandbox.AssemblyFileName,
                entryType = typeof(ManifestLoadableTestExtension).FullName,
                sha256 = sandbox.AssemblySha256,
                providers = new[] { "hover", "completion" },
                processIsolation = false
            };
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(legacyManifest));

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
            Assert.AreEqual(1, registry.GetLspHoverProviders().Count);
            Assert.AreEqual(1, registry.GetLspCompletionProviders().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithUnsupportedFutureManifestVersion_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            var manifestPath = Path.Combine(sandbox.ExtensionDirectory, "extension.json");
            var futureManifest = new
            {
                manifestVersion = 99,
                id = ManifestLoadableTestExtension.ExtensionId,
                assembly = sandbox.AssemblyFileName,
                assemblySha256 = sandbox.AssemblySha256,
                type = typeof(ManifestLoadableTestExtension).FullName,
                permissions = new
                {
                    providers = new[] { "hover", "completion" }
                }
            };
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(futureManifest));

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    requireManifestSignature: false),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            var loadHealth = registry.GetExtensionLoadHealth()
                .Single(static item => string.Equals(item.Source, "user", StringComparison.Ordinal));
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "unsupported manifest version",
                StringComparison.OrdinalIgnoreCase);
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
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithIoOrNetworkCapabilityAndMissingProcessIsolation_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-capability-process-isolation-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityRead
                },
                processIsolation: false,
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
            var loadHealth = GetSingleUserLoadHealth(registry);
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "required when io/network capabilities are declared",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedManifest_LoadsViaWorkerProxy()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-process-worker-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityRead
                },
                processIsolation: true,
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

            var loadReason = string.Join(
                " | ",
                registry.GetExtensionLoadHealth()
                    .Where(static item => string.Equals(item.Source, "user", StringComparison.Ordinal))
                    .Select(static item => $"{item.ExtensionId}:{item.LastReason}"));
            Assert.AreEqual(1, registry.GetExtensions().Count, loadReason);
            var extension = registry.GetExtensions()[ManifestLoadableTestExtension.ExtensionId];
            Assert.IsTrue(
                string.Equals(extension.GetType().Name, "OutOfProcessExtensionProxy", StringComparison.Ordinal),
                $"expected worker proxy extension type but got '{extension.GetType().FullName}'.");

            var workspaceStore = new InMemoryWorkspaceStore();
            var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
            var documentPath = Path.Combine(sandbox.RootDirectory, "ProcessIsolated.jazor");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, "@", version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var hoverResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3110,
                    Method = "textDocument/hover",
                    Params = new LspHoverParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Position = new LspPosition { Line = 0, Character = 0 }
                    }
                },
                CancellationToken.None);
            Assert.IsNotNull(hoverResponse);
            Assert.IsNull(hoverResponse!.Error);
            var hoverResult = hoverResponse.Result as LspHoverResult;
            Assert.IsNotNull(hoverResult);
            StringAssert.Contains(
                hoverResult!.Contents.Value,
                "manifest-loadable-hover",
                StringComparison.Ordinal);

            var completionResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3111,
                    Method = "textDocument/completion",
                    Params = new LspCompletionParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Position = new LspPosition { Line = 0, Character = 1 }
                    }
                },
                CancellationToken.None);
            Assert.IsNotNull(completionResponse);
            Assert.IsNull(completionResponse!.Error);
            var completionItems = completionResponse.Result as IReadOnlyList<LspCompletionItem>;
            Assert.IsNotNull(completionItems);
            Assert.IsTrue(completionItems.Any(static item => string.Equals(item.Label, "manifest-loadable-item", StringComparison.Ordinal)));
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedWorkerUnexpectedExit_RestartsWorkerAndServesRequest()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-process-worker-restart-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityRead
                },
                processIsolation: true,
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

            Assert.AreEqual(1, registry.GetExtensions().Count);
            var extension = registry.GetExtensions()[ManifestLoadableTestExtension.ExtensionId];
            TerminateOutOfProcessWorkerProcess(extension);

            var workspaceStore = new InMemoryWorkspaceStore();
            var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
            var documentPath = Path.Combine(sandbox.RootDirectory, "ProcessIsolatedRestart.jazor");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, "@", version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var completionResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3116,
                    Method = "textDocument/completion",
                    Params = new LspCompletionParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Position = new LspPosition { Line = 0, Character = 1 }
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(completionResponse);
            Assert.IsNull(completionResponse!.Error);
            var completionItems = completionResponse.Result as IReadOnlyList<LspCompletionItem>;
            Assert.IsNotNull(completionItems);
            Assert.IsTrue(completionItems.Any(static item => string.Equals(item.Label, "manifest-loadable-item", StringComparison.Ordinal)));

            var providerHealth = registry.GetProviderHealth()
                .Single(static item =>
                    string.Equals(item.ProviderName, "ManifestLoadableCompletionProvider", StringComparison.Ordinal)
                    && string.Equals(item.Capability, "completion", StringComparison.Ordinal));
            Assert.IsTrue(providerHealth.SuccessCount >= 1);
            Assert.AreEqual(0, providerHealth.FailureCount);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedWorkerRepeatedExit_TripsRestartCircuit()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-process-worker-circuit-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityRead
                },
                processIsolation: true,
                signer: signer,
                settings: new Dictionary<string, string>
                {
                    ["completionExitMode"] = "always",
                    [ExtensionWorkerHostSettingNames.WorkerMaxRestarts] = "1",
                    [ExtensionWorkerHostSettingNames.WorkerRestartWindowMilliseconds] = "1000",
                    [ExtensionWorkerHostSettingNames.WorkerRestartBaseDelayMilliseconds] = "1"
                });

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys,
                    enforceProviderPermissions: true),
                CancellationToken.None);

            Assert.AreEqual(1, registry.GetExtensions().Count);

            var workspaceStore = new InMemoryWorkspaceStore();
            var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
            var documentPath = Path.Combine(sandbox.RootDirectory, "ProcessIsolatedRestartCircuit.jazor");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, "@", version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var completionResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3117,
                    Method = "textDocument/completion",
                    Params = new LspCompletionParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Position = new LspPosition { Line = 0, Character = 1 }
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(completionResponse);
            Assert.IsNull(completionResponse!.Error);
            var completionItems = completionResponse.Result as IReadOnlyList<LspCompletionItem>;
            Assert.IsNotNull(completionItems);
            Assert.IsFalse(completionItems.Any(static item => string.Equals(item.Label, "manifest-loadable-item", StringComparison.Ordinal)));

            var providerHealth = registry.GetProviderHealth()
                .Single(static item =>
                    string.Equals(item.ProviderName, "ManifestLoadableCompletionProvider", StringComparison.Ordinal)
                    && string.Equals(item.Capability, "completion", StringComparison.Ordinal));
            Assert.IsTrue(providerHealth.FailureCount >= 1);
            StringAssert.Contains(
                providerHealth.LastErrorMessage ?? string.Empty,
                "restart circuit",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedIoNone_DeniesProviderInvocationAtRuntime()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-process-worker-io-none-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityNone
                },
                processIsolation: true,
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

            Assert.AreEqual(1, registry.GetExtensions().Count);

            var workspaceStore = new InMemoryWorkspaceStore();
            var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
            var documentPath = Path.Combine(sandbox.RootDirectory, "ProcessIsolatedIoNone.jazor");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, "@", version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var hoverResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3210,
                    Method = "textDocument/hover",
                    Params = new LspHoverParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Position = new LspPosition { Line = 0, Character = 0 }
                    }
                },
                CancellationToken.None);
            Assert.IsNotNull(hoverResponse);
            Assert.IsNull(hoverResponse!.Error);
            Assert.IsNull(hoverResponse.Result as LspHoverResult);

            var completionResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3211,
                    Method = "textDocument/completion",
                    Params = new LspCompletionParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Position = new LspPosition { Line = 0, Character = 1 }
                    }
                },
                CancellationToken.None);
            Assert.IsNotNull(completionResponse);
            Assert.IsNull(completionResponse!.Error);
            var completionItems = completionResponse.Result as IReadOnlyList<LspCompletionItem>;
            Assert.IsNotNull(completionItems);
            Assert.IsFalse(completionItems.Any(static item => string.Equals(item.Label, "manifest-loadable-item", StringComparison.Ordinal)));

            var hoverHealth = registry.GetProviderHealth()
                .Single(static item =>
                    string.Equals(item.ProviderName, "ManifestLoadableHoverProvider", StringComparison.Ordinal)
                    && string.Equals(item.Capability, "hover", StringComparison.Ordinal));
            Assert.IsTrue(hoverHealth.FailureCount >= 1);
            StringAssert.Contains(
                hoverHealth.LastErrorMessage ?? string.Empty,
                "sandbox_violation",
                StringComparison.OrdinalIgnoreCase);

            var completionHealth = registry.GetProviderHealth()
                .Single(static item =>
                    string.Equals(item.ProviderName, "ManifestLoadableCompletionProvider", StringComparison.Ordinal)
                    && string.Equals(item.Capability, "completion", StringComparison.Ordinal));
            Assert.IsTrue(completionHealth.FailureCount >= 1);
            StringAssert.Contains(
                completionHealth.LastErrorMessage ?? string.Empty,
                "sandbox_violation",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedReadOnlyIo_DeniesCodeActionAndRenameWorkspaceEdits()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-process-worker-write-denied-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ProcessIsolatedMutableEditTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ProcessIsolatedMutableEditTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["codeAction", "rename"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityRead
                },
                processIsolation: true,
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

            Assert.AreEqual(1, registry.GetExtensions().Count);

            var workspaceStore = new InMemoryWorkspaceStore();
            var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
            var documentPath = Path.Combine(sandbox.RootDirectory, "ProcessIsolatedReadOnlyIo.jazor");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(
                    documentPath,
                    DocumentKind.Jazor,
                    "<template><Counter /></template>",
                    version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var codeActionResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3212,
                    Method = "textDocument/codeAction",
                    Params = new LspCodeActionParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Range = new LspRange
                        {
                            Start = new LspPosition { Line = 0, Character = 0 },
                            End = new LspPosition { Line = 0, Character = 1 }
                        },
                        Context = new LspCodeActionContext
                        {
                            Diagnostics = []
                        }
                    }
                },
                CancellationToken.None);
            Assert.IsNotNull(codeActionResponse);
            Assert.IsNull(codeActionResponse!.Error);
            var codeActions = codeActionResponse.Result as IReadOnlyList<LspCodeAction>;
            Assert.IsNotNull(codeActions);
            Assert.AreEqual(0, codeActions.Count);

            var renameResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3213,
                    Method = "textDocument/rename",
                    Params = new LspRenameParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Position = new LspPosition { Line = 0, Character = 1 },
                        NewName = "RenamedCounter"
                    }
                },
                CancellationToken.None);
            Assert.IsNotNull(renameResponse);
            Assert.IsNull(renameResponse!.Error);
            Assert.IsNull(renameResponse.Result as LspWorkspaceEdit);

            var codeActionHealth = registry.GetProviderHealth()
                .Single(static item =>
                    string.Equals(item.ProviderName, "ProcessIsolatedMutableCodeActionProvider", StringComparison.Ordinal)
                    && string.Equals(item.Capability, "codeAction", StringComparison.Ordinal));
            Assert.IsTrue(codeActionHealth.FailureCount >= 1);
            StringAssert.Contains(
                codeActionHealth.LastErrorMessage ?? string.Empty,
                "sandbox_violation",
                StringComparison.OrdinalIgnoreCase);

            var renameHealth = registry.GetProviderHealth()
                .Single(static item =>
                    string.Equals(item.ProviderName, "ProcessIsolatedMutableRenameProvider", StringComparison.Ordinal)
                    && string.Equals(item.Capability, "rename", StringComparison.Ordinal));
            Assert.IsTrue(renameHealth.FailureCount >= 1);
            StringAssert.Contains(
                renameHealth.LastErrorMessage ?? string.Empty,
                "sandbox_violation",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedLoopbackNetwork_DeniesDisallowedResultHostAtRuntime()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-process-worker-network-result-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ProcessIsolatedNetworkWorkspaceSymbolTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ProcessIsolatedNetworkWorkspaceSymbolTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["workspaceSymbol"],
                networkPermission: new ExtensionNetworkPermissionManifest
                {
                    Level = ExtensionHostOptions.NetworkCapabilityLoopback
                },
                processIsolation: true,
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

            var workspaceStore = new InMemoryWorkspaceStore();
            var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
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
                    Id = 3214,
                    Method = "workspace/symbol",
                    Params = new LspWorkspaceSymbolParams
                    {
                        Query = "runtime"
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(response);
            Assert.IsNull(response!.Error);
            var symbols = response.Result as IReadOnlyList<LspWorkspaceSymbol>;
            Assert.IsNotNull(symbols);
            Assert.AreEqual(0, symbols.Count);

            var providerHealth = registry.GetProviderHealth()
                .Single(static item =>
                    string.Equals(item.ProviderName, "ProcessIsolatedNetworkWorkspaceSymbolProvider", StringComparison.Ordinal)
                    && string.Equals(item.Capability, "workspaceSymbol", StringComparison.Ordinal));
            Assert.IsTrue(providerHealth.FailureCount >= 1);
            StringAssert.Contains(
                providerHealth.LastErrorMessage ?? string.Empty,
                "sandbox_violation",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedLoopbackNetwork_DeniesDisallowedContextHostAtRuntime()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-process-worker-network-context-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ProcessIsolatedNetworkWorkspaceSymbolTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ProcessIsolatedNetworkWorkspaceSymbolTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["workspaceSymbol"],
                networkPermission: new ExtensionNetworkPermissionManifest
                {
                    Level = ExtensionHostOptions.NetworkCapabilityLoopback
                },
                processIsolation: true,
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

            registry.RegisterLspWorkspaceSymbolProvider(new ContextSeedWorkspaceSymbolProvider());

            var workspaceStore = new InMemoryWorkspaceStore();
            var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
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
                    Id = 3215,
                    Method = "workspace/symbol",
                    Params = new LspWorkspaceSymbolParams
                    {
                        Query = "context"
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(response);
            Assert.IsNull(response!.Error);
            var symbols = response.Result as IReadOnlyList<LspWorkspaceSymbol>;
            Assert.IsNotNull(symbols);
            Assert.IsTrue(symbols.Any(static item => string.Equals(item.Name, "seed-context-symbol", StringComparison.Ordinal)));

            var providerHealth = registry.GetProviderHealth()
                .Single(static item =>
                    string.Equals(item.ProviderName, "ProcessIsolatedNetworkWorkspaceSymbolProvider", StringComparison.Ordinal)
                    && string.Equals(item.Capability, "workspaceSymbol", StringComparison.Ordinal));
            Assert.IsTrue(providerHealth.FailureCount >= 1);
            StringAssert.Contains(
                providerHealth.LastErrorMessage ?? string.Empty,
                "sandbox_violation",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedWorkerBootstrapFailure_RejectsWithoutFallback()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-worker-bootstrap-failure-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: WorkerBootstrapSensitiveTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(WorkerBootstrapSensitiveTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover"],
                processIsolation: true,
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
            Assert.AreEqual(0, registry.GetLspHoverProviders().Count);

            var loadHealth = registry.GetExtensionLoadHealth()
                .Single(static item =>
                    string.Equals(item.ExtensionId, WorkerBootstrapSensitiveTestExtension.ExtensionId, StringComparison.Ordinal)
                    && string.Equals(item.Source, "user", StringComparison.Ordinal));
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "process-isolated worker bootstrap failed",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProcessIsolatedWorkerBootstrapTimeout_RejectsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-worker-bootstrap-timeout-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: SlowProcessIsolatedHoverTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(SlowProcessIsolatedHoverTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover"],
                processIsolation: true,
                signer: signer,
                settings: new Dictionary<string, string>
                {
                    ["bootstrapDelayMs"] = "250",
                    [ExtensionWorkerHostSettingNames.BootstrapTimeoutMilliseconds] = "100"
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
            Assert.AreEqual(0, registry.GetLspHoverProviders().Count);

            var loadHealth = registry.GetExtensionLoadHealth()
                .Single(static item =>
                    string.Equals(item.ExtensionId, SlowProcessIsolatedHoverTestExtension.ExtensionId, StringComparison.Ordinal)
                    && string.Equals(item.Source, "user", StringComparison.Ordinal));
            Assert.AreEqual(1, loadHealth.RejectedCount);
            StringAssert.Contains(
                loadHealth.LastReason ?? string.Empty,
                "timed out",
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionWorkerServer_Invoke_WithSlowProvider_ReturnsTimeoutError()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            using var signer = new ManifestSigner("phase7-worker-invoke-timeout-key");
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: SlowProcessIsolatedHoverTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(SlowProcessIsolatedHoverTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover"],
                ioPermission: new ExtensionIoPermissionManifest
                {
                    Level = ExtensionHostOptions.IoCapabilityRead
                },
                processIsolation: true,
                signer: signer,
                settings: new Dictionary<string, string>
                {
                    ["hoverDelayMs"] = "250",
                    [ExtensionWorkerHostSettingNames.InvokeTimeoutMilliseconds] = "100"
                });

            var registry = new ExtensionRegistry();
            await using var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedPublicKeys: signer.TrustedPublicKeys),
                CancellationToken.None);

            var provider = registry.GetLspHoverProviders().Single();
            var documentPath = Path.Combine(sandbox.RootDirectory, "Timeout.jazor");
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await provider.ProvideHoverAsync(
                    new LspHoverProviderContext(
                        new DocumentSnapshot(
                            documentPath,
                            DocumentKind.Jazor,
                            "<template><div /></template>",
                            "1"),
                        new LspPosition { Line = 0, Character = 1 },
                        new ProjectionTarget(
                            LaneKind.Jazor,
                            DocumentRegionKind.Template,
                            documentPath,
                            documentPath,
                            IsProjected: false),
                        ExistingHover: null),
                    CancellationToken.None));

            StringAssert.Contains(exception.Message, "timed out", StringComparison.OrdinalIgnoreCase);
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
                processIsolation: true,
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
                processIsolation: true,
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
    public void ExtensionSandboxProfile_IsNetworkHostAllowed_WithLoopbackWildcard_ReturnsFalse()
    {
        var profile = new ExtensionSandboxProfile
        {
            IoCapability = ExtensionHostOptions.IoCapabilityNone,
            NetworkCapability = ExtensionHostOptions.NetworkCapabilityLoopback,
            ReadRoots = [],
            WriteRoots = [],
            AllowedHosts = ["*"]
        };

        Assert.IsFalse(profile.IsNetworkHostAllowed("127.0.0.1"));
        Assert.IsFalse(profile.IsNetworkHostAllowed("localhost"));
        Assert.IsFalse(profile.IsNetworkHostAllowed("::1"));
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
                processIsolation: true,
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
    public async Task BuiltinStructureDiagnosticProvider_IgnoresCommentedFakeCodeDirectiveMarkersWithoutBlockBody()
    {
        var provider = new StructureDiagnosticExtension();

        foreach (var (name, text) in new (string Name, string Text)[]
                 {
                     (
                         "line-comment",
                         """
                         <div>hello</div>
                         // @code
                         """),
                     (
                         "razor-comment",
                         """
                         <div>hello</div>
                         @*
                         @code
                         *@
                         """)
                 })
        {
            var document = new DocumentSnapshot(
                documentPath: Path.Combine(Path.GetTempPath(), $"phase7-structure-comment-no-block-{name}-{Guid.NewGuid():N}.jazor"),
                documentKind: DocumentKind.Jazor,
                text: text,
                version: "1");
            var diagnostics = await provider.ProvideDiagnosticsAsync(
                new LspDiagnosticProviderContext(document, Array.Empty<LspDiagnostic>()),
                CancellationToken.None);
            var diagnosticCodes = diagnostics.Select(static item => item.Code).ToArray();

            CollectionAssert.DoesNotContain(diagnosticCodes, "JAZORVUEEXTSTR003", $"{name}: fake @code should not trigger missing block-body diagnostics.");
            CollectionAssert.DoesNotContain(diagnosticCodes, "JAZORVUEEXTSTR005", $"{name}: fake @code should not trigger missing template wrapper diagnostics.");
        }
    }

    [TestMethod]
    public async Task BuiltinStructureDiagnosticProvider_IgnoresCommentedFakeCodeDirectiveMarkersWithOpenBrace()
    {
        var provider = new StructureDiagnosticExtension();

        foreach (var (name, text) in new (string Name, string Text)[]
                 {
                     (
                         "line-comment",
                         """
                         <div>hello</div>
                         // @code {
                         """),
                     (
                         "block-comment",
                         """
                         <div>hello</div>
                         /*
                         @code {
                         */
                         """),
                     (
                         "razor-comment",
                         """
                         <div>hello</div>
                         @*
                         @code {
                         *@
                         """)
                 })
        {
            var document = new DocumentSnapshot(
                documentPath: Path.Combine(Path.GetTempPath(), $"phase7-structure-comment-open-brace-{name}-{Guid.NewGuid():N}.jazor"),
                documentKind: DocumentKind.Jazor,
                text: text,
                version: "1");
            var diagnostics = await provider.ProvideDiagnosticsAsync(
                new LspDiagnosticProviderContext(document, Array.Empty<LspDiagnostic>()),
                CancellationToken.None);
            var diagnosticCodes = diagnostics.Select(static item => item.Code).ToArray();

            CollectionAssert.DoesNotContain(diagnosticCodes, "JAZORVUEEXTSTR004", $"{name}: fake @code should not trigger unbalanced brace diagnostics.");
            CollectionAssert.DoesNotContain(diagnosticCodes, "JAZORVUEEXTSTR005", $"{name}: fake @code should not trigger missing template wrapper diagnostics.");
        }
    }

    [TestMethod]
    public async Task BuiltinStructureDiagnosticProvider_ReportsMultipleRealCodeBlocks()
    {
        var provider = new StructureDiagnosticExtension();
        var document = new DocumentSnapshot(
            documentPath: Path.Combine(Path.GetTempPath(), $"phase7-structure-multi-code-{Guid.NewGuid():N}.jazor"),
            documentKind: DocumentKind.Jazor,
            text: """
                  @code {
                    private int first = 1;
                  }

                  @code {
                    private int second = 2;
                  }
                  """,
            version: "1");
        var diagnostics = await provider.ProvideDiagnosticsAsync(
            new LspDiagnosticProviderContext(document, Array.Empty<LspDiagnostic>()),
            CancellationToken.None);
        var duplicateCodeDiagnostic = diagnostics.Single(static item =>
            string.Equals(item.Code, "JAZORVUEEXTSTR006", StringComparison.Ordinal));

        Assert.AreEqual(4, duplicateCodeDiagnostic.Range.Start.Line);
        Assert.AreEqual(0, duplicateCodeDiagnostic.Range.Start.Character);
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
            new DocumentSnapshot(documentPath, DocumentKind.Jazor, "@m", version: "1"),
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
        Assert.IsTrue(items.Any(static item => string.Equals(item.Label, "@module", StringComparison.Ordinal)));
        Assert.IsFalse(items.Any(static item => string.Equals(item.Label, "@code", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task BuiltinDirectiveCompletionProvider_DoesNotServeStandardRazorDirectivesThroughLspSession()
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
                Id = 3102,
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
        Assert.AreEqual(0, items.Count);
    }

    [TestMethod]
    public async Task BuiltinDirectiveCompletionProvider_DoesNotServeModuleDirectiveInsideTemplateExpression()
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
            new DocumentSnapshot(
                documentPath,
                DocumentKind.Jazor,
                """
                <template>
                  @mod
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
                Id = 3103,
                Method = "textDocument/completion",
                Params = new LspCompletionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 1, Character = 6 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var items = response.Result as IReadOnlyList<LspCompletionItem>;
        Assert.IsNotNull(items);
        Assert.IsFalse(items.Any(static item => string.Equals(item.Label, "@module", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task BuiltinDirectiveCompletionProvider_ServesDirectiveCompletionsAfterCommentedCodeDirectiveMarker()
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
            new DocumentSnapshot(
                documentPath,
                DocumentKind.Jazor,
                """
                // @code {
                @m
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
                Id = 3104,
                Method = "textDocument/completion",
                Params = new LspCompletionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 1, Character = 2 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var items = response.Result as IReadOnlyList<LspCompletionItem>;
        Assert.IsNotNull(items);
        Assert.IsTrue(items.Any(static item => string.Equals(item.Label, "@module", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task BuiltinDirectiveCompletionProvider_ServesDirectiveCompletionsAfterBlockCommentedCodeDirectiveMarker()
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
            new DocumentSnapshot(
                documentPath,
                DocumentKind.Jazor,
                """
                /*
                @code {
                */
                @m
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
                Id = 3105,
                Method = "textDocument/completion",
                Params = new LspCompletionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 3, Character = 2 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var items = response.Result as IReadOnlyList<LspCompletionItem>;
        Assert.IsNotNull(items);
        Assert.IsTrue(items.Any(static item => string.Equals(item.Label, "@module", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task BuiltinDirectiveCompletionProvider_ServesDirectiveCompletionsAfterRazorCommentedCodeDirectiveMarker()
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
            new DocumentSnapshot(
                documentPath,
                DocumentKind.Jazor,
                """
                @*
                @code {
                *@
                @m
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
                Id = 3106,
                Method = "textDocument/completion",
                Params = new LspCompletionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 3, Character = 2 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var items = response.Result as IReadOnlyList<LspCompletionItem>;
        Assert.IsNotNull(items);
        Assert.IsTrue(items.Any(static item => string.Equals(item.Label, "@module", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task BuiltinComponentCodeActionProvider_OffersImportQuickFixThroughLspSession()
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
                                    Source = "Jolt.Frontend",
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
                string.Equals(action.Title, "Add @module for CounterWidget", StringComparison.Ordinal));
            Assert.IsNotNull(importAction);
            Assert.IsNotNull(importAction!.Edit);

            var uri = LspProtocolHelpers.ToDocumentUri(documentPath);
            Assert.IsTrue(importAction.Edit.Changes.ContainsKey(uri));
            var inserted = importAction.Edit.Changes[uri].Single();
            StringAssert.Contains(inserted.NewText, "@module CounterWidget from \"./CounterWidget.vue\"", StringComparison.Ordinal);
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
    public async Task BuiltinComponentCodeActionProvider_OffersImportQuickFix_WhenExistingImportOnlyMatchesSourcePath()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-code-action-source-path-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(rootDirectory, "CounterWidget.vue"),
                "<template><div /></template>");

            var actions = await GetBuiltinComponentCodeActionsAsync(
                rootDirectory,
                """
                @module OtherWidget from "./CounterWidget.vue"
                <template>
                  <CounterWidget />
                </template>
                """,
                "CounterWidget");

            Assert.IsTrue(actions.Any(static action =>
                string.Equals(action.Title, "Add @module for CounterWidget", StringComparison.Ordinal)));
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
    public async Task BuiltinComponentCodeActionProvider_OffersImportQuickFix_WhenExistingImportOnlyMatchesImportedNameBeforeAlias()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-code-action-imported-alias-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(rootDirectory, "CounterWidget.vue"),
                "<template><div /></template>");

            var actions = await GetBuiltinComponentCodeActionsAsync(
                rootDirectory,
                """
                @module { CounterWidget as RenamedWidget } from "./components"
                <template>
                  <CounterWidget />
                </template>
                """,
                "CounterWidget");

            Assert.IsTrue(actions.Any(static action =>
                string.Equals(action.Title, "Add @module for CounterWidget", StringComparison.Ordinal)));
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
    public async Task BuiltinComponentCodeActionProvider_DoesNotOfferImportQuickFix_WhenComponentAlreadyImportedByLocalBinding()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-code-action-local-binding-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(rootDirectory, "CounterWidget.vue"),
                "<template><div /></template>");

            var actions = await GetBuiltinComponentCodeActionsAsync(
                rootDirectory,
                """
                @module CounterWidget from "./shared/widgets"
                <template>
                  <CounterWidget />
                </template>
                """,
                "CounterWidget");

            Assert.IsFalse(actions.Any(static action =>
                string.Equals(action.Title, "Add @module for CounterWidget", StringComparison.Ordinal)));
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
    public async Task BuiltinComponentCodeActionProvider_DoesNotFallbackToFirstTagForUnresolvedDiagnostic()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-code-action-no-fallback-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(rootDirectory, "FirstWidget.vue"),
                "<template><div /></template>");
            const string text =
                """
                <template>
                  <FirstWidget />
                </template>
                """;
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var document = new DocumentSnapshot(documentPath, DocumentKind.Jazor, text, version: "1");
            var provider = new ComponentCodeActionExtension();

            var actions = await provider.ProvideCodeActionsAsync(
                new LspCodeActionProviderContext(
                    document,
                    LspProtocolHelpers.ToRange(text, 0, 0),
                    [
                        new LspDiagnostic
                        {
                            Range = LspProtocolHelpers.ToRange(text, 0, 0),
                            Severity = 1,
                            Code = "JAZORVUEFRONTEND001",
                            Source = "Jolt.Frontend",
                            Message = "Unable to resolve component MissingWidget."
                        }
                    ],
                    new ProjectionTarget(
                        LaneKind.Jazor,
                        DocumentRegionKind.Template,
                        documentPath,
                        documentPath,
                        IsProjected: false),
                    Array.Empty<LspCodeAction>()),
                CancellationToken.None);

            Assert.AreEqual(0, actions.Count);
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
    public void ExtensionProviderLogPersistence_AppendAndReplay_RehydratesRecentEventsWithoutProviderHealth()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-provider-log-{Guid.NewGuid():N}");
        var logFilePath = Path.Combine(rootDirectory, "logs", "provider-events.jsonl");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            ExtensionProviderLogPersistence.Append(
                new ExtensionProviderInvocation(
                    ProviderName: "ReplayHoverProvider",
                    Capability: "hover",
                    Duration: TimeSpan.FromMilliseconds(12),
                    Succeeded: true,
                    TimedOut: false,
                    Skipped: false,
                    ErrorMessage: null),
                logFilePath);
            ExtensionProviderLogPersistence.Append(
                new ExtensionProviderInvocation(
                    ProviderName: "ReplayHoverProvider",
                    Capability: "hover",
                    Duration: TimeSpan.FromMilliseconds(8),
                    Succeeded: false,
                    TimedOut: false,
                    Skipped: false,
                    ErrorMessage: "sandbox_violation"),
                logFilePath);

            var registry = new ExtensionRegistry(
                loadEventRetention: 0,
                providerEventRetention: 10);
            ExtensionProviderLogPersistence.Replay(registry, logFilePath);

            Assert.AreEqual(0, registry.GetProviderHealth().Count);

            var recent = registry.GetRecentProviderInvocations(maxCount: 10);
            Assert.AreEqual(2, recent.Count);
            Assert.IsTrue(recent.Any(static item => item.Succeeded));
            Assert.IsTrue(recent.Any(static item => !item.Succeeded && string.Equals(item.ErrorMessage, "sandbox_violation", StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
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
        Assert.AreEqual(1, dashboard.RecentProviderEvents.Count);
        Assert.AreEqual("ManifestLoadableHoverProvider", dashboard.RecentProviderEvents[0].ProviderName);
        Assert.AreEqual("hover", dashboard.RecentProviderEvents[0].Capability);
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

    private static void TerminateOutOfProcessWorkerProcess(IExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        var workerClientField = extension.GetType().GetField(
            "_workerClient",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(workerClientField);
        var workerClient = workerClientField.GetValue(extension);
        Assert.IsNotNull(workerClient);

        var processField = workerClient.GetType().GetField(
            "_process",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(processField);
        var process = processField.GetValue(workerClient) as System.Diagnostics.Process;
        Assert.IsNotNull(process);

        if (process.HasExited)
        {
            return;
        }

        process.Kill(entireProcessTree: true);
        Assert.IsTrue(process.WaitForExit(5_000), "process-isolated extension worker did not exit within timeout.");
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
        ExtensionSignatureManifest? explicitSignature = null,
        IReadOnlyDictionary<string, string>? settings = null)
    {
        var permissions = CreatePermissionsManifest(
            providers,
            ioPermission,
            networkPermission,
            processIsolation);
        var manifestSettings = CreateSettingsManifest(settings);
        var unsignedManifest = new ExtensionManifest
        {
            Id = id,
            Assembly = assembly,
            AssemblySha256 = assemblySha256,
            Type = type,
            Permissions = permissions,
            Settings = manifestSettings
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
            Signature = finalSignature,
            Settings = manifestSettings
        };

        var manifestPath = Path.Combine(extensionDirectory, "extension.json");
        File.WriteAllText(
            manifestPath,
            JsonSerializer.Serialize(manifest));
    }

    private static Dictionary<string, JsonElement>? CreateSettingsManifest(
        IReadOnlyDictionary<string, string>? settings)
    {
        if (settings is null || settings.Count == 0)
        {
            return null;
        }

        var manifestSettings = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var item in settings)
        {
            manifestSettings[item.Key] = JsonSerializer.SerializeToElement(item.Value);
        }

        return manifestSettings;
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

    private static async Task<IReadOnlyList<LspCodeAction>> GetBuiltinComponentCodeActionsAsync(
        string rootDirectory,
        string documentText,
        string componentName)
    {
        var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
        var document = new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            documentText,
            version: "1");
        var diagnostic = CreateMissingComponentDiagnostic(documentText, componentName);

        var provider = new ComponentCodeActionExtension();
        return await provider.ProvideCodeActionsAsync(
            new LspCodeActionProviderContext(
                document,
                diagnostic.Range,
                [diagnostic],
                new ProjectionTarget(
                    LaneKind.Jazor,
                    DocumentRegionKind.Template,
                    documentPath,
                    documentPath,
                    IsProjected: false),
                Array.Empty<LspCodeAction>()),
            CancellationToken.None);
    }

    private static LspDiagnostic CreateMissingComponentDiagnostic(string text, string componentName)
    {
        var componentOffset = text.IndexOf($"<{componentName}", StringComparison.Ordinal);
        if (componentOffset >= 0)
        {
            componentOffset++;
        }
        else
        {
            componentOffset = text.IndexOf(componentName, StringComparison.Ordinal);
        }

        if (componentOffset < 0)
        {
            throw new InvalidOperationException($"Unable to locate component '{componentName}' in test document.");
        }

        return new LspDiagnostic
        {
            Range = LspProtocolHelpers.ToRange(text, componentOffset, componentName.Length),
            Severity = 1,
            Code = "JAZORVUEFRONTEND001",
            Source = "Jolt.Frontend",
            Message = $"Unable to resolve component {componentName}."
        };
    }

    private static LspSession CreateSession(
        IJoltWorkspaceStore workspaceStore,
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

        public ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentHighlight>>(Array.Empty<LspDocumentHighlight>());

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
    private bool _exitOnCompletion;

    ExtensionMetadata IExtension.Metadata => MetadataValue;

    string ILspHoverProvider.Name => "ManifestLoadableHoverProvider";

    int ILspHoverProvider.Priority => 10;

    string ILspCompletionProvider.Name => "ManifestLoadableCompletionProvider";

    int ILspCompletionProvider.Priority => 10;

    ValueTask IExtension.InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
    {
        _exitOnCompletion = context.Settings.TryGetValue("completionExitMode", out var exitMode)
            && string.Equals(exitMode, "always", StringComparison.OrdinalIgnoreCase);
        return ValueTask.CompletedTask;
    }

    ValueTask IExtension.ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask<LspHoverResult?> ILspHoverProvider.ProvideHoverAsync(
        LspHoverProviderContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<LspHoverResult?>(new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "plaintext",
                Value = "manifest-loadable-hover"
            },
            Range = null
        });
    }

    ValueTask<IReadOnlyList<LspCompletionItem>> ILspCompletionProvider.ProvideCompletionItemsAsync(
        LspCompletionProviderContext context,
        CancellationToken cancellationToken)
    {
        if (_exitOnCompletion)
        {
            Process.GetCurrentProcess().Kill();
            return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());
        }

        return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(
        [
            new LspCompletionItem
            {
                Label = "manifest-loadable-item",
                Kind = 3,
                Detail = "phase7 process-isolated completion item",
                Documentation = "generated from process-isolated extension worker"
            }
        ]);
    }
}

public sealed class ProcessIsolatedMutableEditTestExtension : IExtension, ILspCodeActionProvider, ILspRenameProvider
{
    public const string ExtensionId = "phase7.process-isolated-mutable-edit";

    private static readonly ExtensionMetadata MetadataValue = new(
        Id: ExtensionId,
        Name: "Process Isolated Mutable Edit Test Extension",
        Version: "1.0.0");

    ExtensionMetadata IExtension.Metadata => MetadataValue;

    string ILspCodeActionProvider.Name => "ProcessIsolatedMutableCodeActionProvider";

    int ILspCodeActionProvider.Priority => 10;

    string ILspRenameProvider.Name => "ProcessIsolatedMutableRenameProvider";

    int ILspRenameProvider.Priority => 10;

    ValueTask IExtension.InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask<IReadOnlyList<LspCodeAction>> ILspCodeActionProvider.ProvideCodeActionsAsync(
        LspCodeActionProviderContext context,
        CancellationToken cancellationToken)
    {
        var documentUri = LspProtocolHelpers.ToDocumentUri(context.Document.DocumentPath);
        return ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(
        [
            new LspCodeAction
            {
                Title = "process-isolated mutable code action",
                Kind = "quickfix",
                Edit = new LspWorkspaceEdit
                {
                    Changes = new Dictionary<string, LspTextEdit[]>
                    {
                        [documentUri] =
                        [
                            new LspTextEdit
                            {
                                Range = new LspRange
                                {
                                    Start = new LspPosition { Line = 0, Character = 0 },
                                    End = new LspPosition { Line = 0, Character = 0 }
                                },
                                NewText = "<!-- code-action-edit -->"
                            }
                        ]
                    }
                }
            }
        ]);
    }

    ValueTask<LspWorkspaceEdit?> ILspRenameProvider.ProvideRenameAsync(
        LspRenameProviderContext context,
        CancellationToken cancellationToken)
    {
        var documentUri = LspProtocolHelpers.ToDocumentUri(context.Document.DocumentPath);
        return ValueTask.FromResult<LspWorkspaceEdit?>(new LspWorkspaceEdit
        {
            Changes = new Dictionary<string, LspTextEdit[]>
            {
                [documentUri] =
                [
                    new LspTextEdit
                    {
                        Range = new LspRange
                        {
                            Start = new LspPosition { Line = 0, Character = 0 },
                            End = new LspPosition { Line = 0, Character = 1 }
                        },
                        NewText = context.NewName
                    }
                ]
            }
        });
    }
}

public sealed class ProcessIsolatedNetworkWorkspaceSymbolTestExtension : IExtension, ILspWorkspaceSymbolProvider
{
    public const string ExtensionId = "phase7.process-isolated-network-workspace-symbol";

    private static readonly ExtensionMetadata MetadataValue = new(
        Id: ExtensionId,
        Name: "Process Isolated Network Workspace Symbol Test Extension",
        Version: "1.0.0");

    ExtensionMetadata IExtension.Metadata => MetadataValue;

    string ILspWorkspaceSymbolProvider.Name => "ProcessIsolatedNetworkWorkspaceSymbolProvider";

    int ILspWorkspaceSymbolProvider.Priority => 10;

    ValueTask IExtension.InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ILspWorkspaceSymbolProvider.ProvideWorkspaceSymbolsAsync(
        LspWorkspaceSymbolProviderContext context,
        CancellationToken cancellationToken)
    {
        var symbolUri = context.ExistingSymbols.Count > 0
            ? "https://localhost/context-allowed"
            : "https://example.com/runtime-forbidden";

        return ValueTask.FromResult<IReadOnlyList<LspWorkspaceSymbol>>(
        [
            new LspWorkspaceSymbol
            {
                Name = "process-isolated-network-symbol",
                Kind = 5,
                Location = new LspLocation
                {
                    Uri = symbolUri,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 1 }
                    }
                },
                ContainerName = "phase7"
            }
        ]);
    }
}

public sealed class ContextSeedWorkspaceSymbolProvider : ILspWorkspaceSymbolProvider
{
    string ILspWorkspaceSymbolProvider.Name => "ContextSeedWorkspaceSymbolProvider";

    int ILspWorkspaceSymbolProvider.Priority => 100;

    ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ILspWorkspaceSymbolProvider.ProvideWorkspaceSymbolsAsync(
        LspWorkspaceSymbolProviderContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<IReadOnlyList<LspWorkspaceSymbol>>(
        [
            new LspWorkspaceSymbol
            {
                Name = "seed-context-symbol",
                Kind = 5,
                Location = new LspLocation
                {
                    Uri = "https://example.com/context-disallowed",
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 1 }
                    }
                },
                ContainerName = "phase7-seed"
            }
        ]);
    }
}

public sealed class WorkerBootstrapSensitiveTestExtension : IExtension, ILspHoverProvider
{
    public const string ExtensionId = "phase7.worker-bootstrap-sensitive";

    private static readonly ExtensionMetadata MetadataValue = new(
        Id: ExtensionId,
        Name: "Worker Bootstrap Sensitive Test Extension",
        Version: "1.0.0");

    ExtensionMetadata IExtension.Metadata => MetadataValue;

    string ILspHoverProvider.Name => "WorkerBootstrapSensitiveHoverProvider";

    int ILspHoverProvider.Priority => 10;

    ValueTask IExtension.InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
    {
        if (Environment.GetCommandLineArgs()
            .Any(static arg => string.Equals(arg, "--extension-worker", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("worker bootstrap intentionally failed for no-fallback test.");
        }

        return ValueTask.CompletedTask;
    }

    ValueTask IExtension.ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask<LspHoverResult?> ILspHoverProvider.ProvideHoverAsync(
        LspHoverProviderContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<LspHoverResult?>(new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "plaintext",
                Value = "worker-bootstrap-sensitive-hover"
            },
            Range = null
        });
    }
}

public sealed class SlowProcessIsolatedHoverTestExtension : IExtension, ILspHoverProvider
{
    public const string ExtensionId = "phase7.slow-process-isolated-hover";

    private static readonly ExtensionMetadata MetadataValue = new(
        Id: ExtensionId,
        Name: "Slow Process Isolated Hover Test Extension",
        Version: "1.0.0");

    private int _hoverDelayMs;

    ExtensionMetadata IExtension.Metadata => MetadataValue;

    string ILspHoverProvider.Name => "SlowProcessIsolatedHoverProvider";

    int ILspHoverProvider.Priority => 10;

    async ValueTask IExtension.InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
    {
        _hoverDelayMs = GetDelay(context.Settings, "hoverDelayMs");
        var bootstrapDelayMs = GetDelay(context.Settings, "bootstrapDelayMs");
        if (bootstrapDelayMs > 0)
        {
            await Task.Delay(bootstrapDelayMs, cancellationToken);
        }
    }

    ValueTask IExtension.ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    async ValueTask<LspHoverResult?> ILspHoverProvider.ProvideHoverAsync(
        LspHoverProviderContext context,
        CancellationToken cancellationToken)
    {
        if (_hoverDelayMs > 0)
        {
            await Task.Delay(_hoverDelayMs, cancellationToken);
        }

        return new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "plaintext",
                Value = "slow-process-isolated-hover"
            },
            Range = null
        };
    }

    private static int GetDelay(IReadOnlyDictionary<string, string> settings, string key)
        => settings.TryGetValue(key, out var value) && int.TryParse(value, out var delayMs)
            ? delayMs
            : 0;
}

