package org.jazor.jolt.rider;

import com.google.gson.JsonParser;
import com.intellij.execution.ExecutionException;
import com.intellij.execution.configurations.GeneralCommandLine;
import com.intellij.execution.configurations.PathEnvironmentVariableUtil;
import com.intellij.openapi.project.Project;
import com.intellij.openapi.util.SystemInfo;
import com.intellij.openapi.vfs.LocalFileSystem;
import com.intellij.openapi.vfs.VfsUtilCore;
import com.intellij.openapi.vfs.VirtualFile;
import com.intellij.platform.lsp.api.LspClientDescriptor;
import com.intellij.platform.lsp.api.LspIntegrationProvider;

import java.nio.file.Files;
import java.nio.file.Path;

public final class JoltLspProvider implements LspIntegrationProvider {
    @Override
    public void fileOpened(Project project, VirtualFile file, LspClientStarter starter) {
        if (!isJoltSource(file)) return;
        var workspace = workspaceFor(project, file);
        if (workspace != null) starter.ensureClientStarted(new Descriptor(project, workspace));
    }

    private static boolean isJoltSource(VirtualFile file) {
        return file.isInLocalFileSystem() && !file.isDirectory() && "jazor".equalsIgnoreCase(file.getExtension());
    }

    private static VirtualFile workspaceFor(Project project, VirtualFile file) {
        for (var directory = file.getParent(); directory != null; directory = directory.getParent()) {
            if (directory.findChild("package.json") != null)
                return directory;
        }
        var basePath = project.getBasePath();
        var base = basePath == null ? null : LocalFileSystem.getInstance().findFileByPath(basePath);
        return base != null && VfsUtilCore.isAncestor(base, file, true) ? base : file.getParent();
    }

    private static boolean hasLocalTool(Path workspace) {
        var allowRoot = System.getenv("DOTNET_TOOLS_ALLOW_MANIFEST_IN_ROOT");
        for (var directory = workspace; directory != null && (directory.getParent() != null ||
                "1".equals(allowRoot) || "true".equalsIgnoreCase(allowRoot)); directory = directory.getParent()) {
            for (var manifest : new Path[]{directory.resolve(".config/dotnet-tools.json"), directory.resolve("dotnet-tools.json")}) {
                if (!Files.isRegularFile(manifest)) continue;
                try (var reader = Files.newBufferedReader(manifest)) {
                    var json = JsonParser.parseReader(reader).getAsJsonObject();
                    var tools = json.getAsJsonObject("tools");
                    if (tools != null)
                        for (var tool : tools.entrySet()) {
                            var commands = tool.getValue().getAsJsonObject().getAsJsonArray("commands");
                            if (commands != null)
                                for (var command : commands)
                                    if ("jolt".equals(command.getAsString())) return true;
                        }
                    if (json.has("isRoot") && json.get("isRoot").getAsBoolean()) return false;
                } catch (java.io.IOException | com.google.gson.JsonParseException | IllegalStateException ignored) {
                    // Let dotnet report the manifest error using its own parser.
                    return true;
                }
            }
        }
        return false;
    }

    private static GeneralCommandLine command(Path workspace) throws ExecutionException {
        if (hasLocalTool(workspace))
            return new GeneralCommandLine("dotnet", "tool", "run", "jolt", "--", "lsp").withWorkDirectory(workspace.toFile());
        var dotnetHome = System.getenv("DOTNET_CLI_HOME");
        var tools = Path.of(dotnetHome == null || dotnetHome.isBlank() ? System.getProperty("user.home") : dotnetHome,
            ".dotnet", "tools");
        if (!SystemInfo.isWindows) {
            var global = tools.resolve("jolt");
            return new GeneralCommandLine(Files.isExecutable(global) ? global.toString() : "jolt", "lsp").withWorkDirectory(workspace.toFile());
        }
        var shim = tools.resolve("jolt.cmd");
        if (!Files.isRegularFile(shim))
            shim = PathEnvironmentVariableUtil.findFirst("jolt.cmd");
        if (shim == null)
            throw new ExecutionException("Jolt CLI not found. Install the Jazor.Jolt .NET tool, or restore it in this project.");
        // Resolve before launching: cmd searches the workspace before PATH. Expanding
        // a quoted absolute path inside cmd also preserves spaces and metacharacters.
        return new GeneralCommandLine("cmd.exe", "/d", "/v:off", "/c", "%JOLT_RIDER_SHIM%", "lsp")
            .withEnvironment("JOLT_RIDER_SHIM", "\"" + shim + "\"")
            .withWorkDirectory(workspace.toFile());
    }

    private static final class Descriptor extends LspClientDescriptor {
        private final VirtualFile workspace;

        private Descriptor(Project project, VirtualFile workspace) {
            super(project, "Jolt", workspace);
            this.workspace = workspace;
        }

        @Override
        public boolean isSupportedFile(VirtualFile file) {
            return isJoltSource(file) && workspace.equals(workspaceFor(getProject(), file));
        }

        @Override
        public String getLanguageId(VirtualFile file) {
            return "jazor";
        }

        @Override
        public GeneralCommandLine createCommandLine() throws ExecutionException {
            return command(workspace.toNioPath());
        }
    }
}
