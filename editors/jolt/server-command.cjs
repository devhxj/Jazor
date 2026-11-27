const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { execFile } = require('node:child_process');
const { promisify } = require('node:util');

const execute = promisify(execFile);

async function serverCommand(directory, override) {
    if (override)
        return override.endsWith('.dll')
            ? { command: 'dotnet', args: [override], options: { cwd: directory } }
            : { command: override, args: ['lsp'], options: { cwd: directory } };

    const project = findProject(directory);
    if (project) {
        const cwd = path.dirname(project);
        let properties;
        try {
            const result = await execute('dotnet', ['msbuild', project, '-nologo', '-v:q',
                '-t:_ResolveJoltTool', '-getProperty:_JoltHost,_JoltPackageSpec,_JoltRoot'], { cwd });
            properties = JSON.parse(result.stdout).Properties;
        } catch (error) {
            // Plain C# projects can still opt into Jolt through a local tool manifest.
            if (!error.stdout?.includes('MSB4057') && !error.stderr?.includes('MSB4057'))
                throw error;
        }
        const options = { cwd };
        if (properties?._JoltRoot)
            options.env = { ...process.env, JAZOR_GENERATED_ROOT: properties._JoltRoot };
        if (properties?._JoltHost)
            return { command: properties._JoltHost, args: ['lsp'], options };
        if (properties?._JoltPackageSpec) {
            const apphost = restoredApphost(properties._JoltPackageSpec);
            if (apphost)
                return { command: apphost, args: ['lsp'], options };
            return {
                command: 'dotnet',
                args: ['tool', 'exec', properties._JoltPackageSpec, '-v', 'quiet', '--', 'lsp'],
                options,
            };
        }
    }
    if (hasLocalTool(directory))
        return { command: 'dotnet', args: ['tool', 'run', 'jolt', '--', 'lsp'], options: { cwd: directory } };

    const tools = path.join(process.env.DOTNET_CLI_HOME || os.homedir(), '.dotnet', 'tools');
    if (process.platform === 'win32') {
        const shim = path.join(tools, 'jolt.cmd');
        return {
            command: process.env.ComSpec || 'cmd.exe',
            args: ['/d', '/v:off', '/c', '%JOLT_EDITOR_SHIM%', 'lsp'],
            options: { cwd: directory, env: { ...process.env, JOLT_EDITOR_SHIM: fs.existsSync(shim) ? '"' + shim + '"' : 'jolt.cmd' } },
        };
    }
    const executable = path.join(tools, 'jolt');
    return { command: fs.existsSync(executable) ? executable : 'jolt', args: ['lsp'], options: { cwd: directory } };
}

function restoredApphost(spec) {
    const match = /^Jazor\.Jolt@(\d[\da-z.+-]*)$/i.exec(spec);
    if (!match) return;
    const platform = { win32: 'win', linux: 'linux' }[process.platform];
    if (!platform || !['x64', 'arm64'].includes(process.arch)) return;
    const rid = platform + '-' + process.arch;
    const cache = process.env.NUGET_PACKAGES || path.join(process.env.DOTNET_CLI_HOME || os.homedir(), '.nuget', 'packages');
    if (!path.isAbsolute(cache)) return;
    const version = match[1].toLowerCase();
    // The tool package dispatches to a RID-specific package with a CET-disabled apphost.
    if (!fs.existsSync(path.join(cache, 'jazor.jolt', version, '.nupkg.metadata'))) return;
    const restored = path.join(cache, 'jazor.jolt.' + rid, version);
    if (!fs.existsSync(path.join(restored, '.nupkg.metadata'))) return;
    const executable = path.join(restored, 'tools', 'net11.0', rid, platform === 'win' ? 'Jolt.Cli.exe' : 'Jolt.Cli');
    if (fs.existsSync(executable)) return executable;
}

function* ancestors(directory) {
    for (;;) {
        yield directory;
        const parent = path.dirname(directory);
        if (parent === directory) return;
        directory = parent;
    }
}

function findProject(directory) {
    for (const current of ancestors(directory)) {
        const projects = fs.readdirSync(current).filter(name => name.endsWith('.csproj'));
        if (projects.length === 1)
            return path.join(current, projects[0]);
        if (projects.length > 1)
            return;
    }
}

function projectDirectory(directory, workspace) {
    const project = findProject(directory);
    if (project) return path.dirname(project);
    for (const current of ancestors(directory)) {
        if (fs.existsSync(path.join(current, 'package.json'))) return current;
        if (current === workspace) break;
    }
    return workspace ?? directory;
}

function hasLocalTool(directory) {
    const allowRoot = /^(1|true)$/i.test(process.env.DOTNET_TOOLS_ALLOW_MANIFEST_IN_ROOT ?? '');
    for (const current of ancestors(directory)) {
        if (!allowRoot && path.dirname(current) === current) break;
        for (const name of ['.config/dotnet-tools.json', 'dotnet-tools.json']) {
            const manifest = path.join(current, name);
            if (!fs.existsSync(manifest)) continue;
            const data = JSON.parse(fs.readFileSync(manifest, 'utf8'));
            if (Object.values(data.tools ?? {}).some(tool => tool.commands?.includes('jolt'))) return true;
            if (data.isRoot) return false;
        }
    }
    return false;
}

module.exports = { serverCommand, projectDirectory };
