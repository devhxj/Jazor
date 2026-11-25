const fs = require('node:fs/promises');
const path = require('node:path');
const crypto = require('node:crypto');
const { createRequire } = require('node:module');
const { pathToFileURL } = require('node:url');
const { inspect } = require('node:util');

const exists = (file) => fs.access(file).then(() => true, () => false);
const hash = (value) => crypto.createHash('sha256').update(value).digest('hex');

exports.executeModule = async (requestJson) => {
    const { projectDirectory, modulePath, arguments: args, imports, embeddedModules } = JSON.parse(requestJson);
    if (embeddedModules) {
        // Keep file URLs for native ESM package resolution, but serve our own checker modules from the assembly.
        const modules = new Map(Object.entries(embeddedModules).map(([file, source]) =>
            [pathToFileURL(path.join(path.dirname(modulePath), file)).href, source]));
        require('node:module').registerHooks({
            resolve(specifier, context, nextResolve) {
                if (specifier.startsWith('./') || specifier.startsWith('file:')) {
                    const url = new URL(specifier, context.parentURL).href;
                    if (modules.has(url)) return { url, shortCircuit: true };
                }
                return nextResolve(specifier, context);
            },
            load(url, context, nextLoad) {
                return modules.has(url) ? { format: 'module', source: modules.get(url), shortCircuit: true } : nextLoad(url, context);
            },
        });
    }
    if (imports) {
        // Only test fixtures need an import map for their local host doubles.
        // Register an environment-owned ESM hook; production modules retain
        // standard package resolution and are never rewritten by this runner.
        const loader = `
            let imports, baseURL;
            export function initialize(data) { ({ imports, baseURL } = data); }
            export async function resolve(specifier, context, nextResolve) {
                const key = Object.keys(imports).sort((a, b) => b.length - a.length)
                    .find(key => key === specifier || key.endsWith('/') && specifier.startsWith(key));
                if (!key) return nextResolve(specifier, context);
                const target = imports[key] + specifier.slice(key.length);
                return nextResolve(target, { ...context, parentURL: baseURL });
            }
        `;
        const { register } = require('node:module');
        register('data:text/javascript,' + encodeURIComponent(loader), {
            data: { imports, baseURL: pathToFileURL(path.join(projectDirectory, 'package.json')).href }
        });
    }
    const tests = [];
    const cleanups = [];
    const output = [];
    const { format } = require('node:util');
    console.log = (...values) => output.push(format(...values));
    globalThis.__jazorArguments = args;
    // Runtime fixtures register their existing assertions here. This is a
    // test-owned function, not a Deno compatibility object or a production API.
    globalThis.__jazorTest = (name, body) => tests.push({ name, body });
    // Embedded environments have no browser unload event. Release fixture-owned
    // DOM windows after assertions, including import and async test failures.
    globalThis.__jazorAfterAll = (cleanup) => cleanups.push(cleanup);
    try {
        await import(pathToFileURL(modulePath).href);
        for (const test of tests) {
            try { await test.body(); }
            catch (error) { throw new Error(test.name + ': ' + (error.stack ?? error)); }
        }
    } finally {
        for (const cleanup of cleanups.reverse()) await cleanup();
    }
    return JSON.stringify({ tests: tests.length, pid: process.pid, output: output.join('\n') });
};

exports.restore = async (requestJson) => {
    const request = JSON.parse(requestJson);
    const project = request.projectDirectory;
    let root = project;
    let workspaceConfig;
    for (let directory = project; !request.standaloneTool; directory = path.dirname(directory)) {
        if (await exists(path.join(directory, 'pnpm-workspace.yaml'))) {
            root = directory;
            workspaceConfig = path.join(directory, 'pnpm-workspace.yaml');
            break;
        }
        if (directory === path.dirname(directory)) break;
    }

    const lockPath = path.join(root, 'pnpm-lock.yaml');
    const lock = await exists(lockPath) ? await fs.readFile(lockPath, 'utf8') : '';
    let pnpm;
    let parsedLock;
    if (workspaceConfig && lock) {
        pnpm = loadPnpm(request.bootstrap);
        parsedLock = await pnpm.readLockfile({ dir: root });
    }
    const importerFor = directory => path.relative(root, directory).split(path.sep).join('/') || '.';
    // The engine discovers the workspace from an importer when no complete
    // importer list is available yet. Root installs with only a child entry fail.
    const discover = workspaceConfig && !Object.hasOwn(parsedLock?.importers ?? {}, importerFor(project));
    const readProjects = lock => Promise.all([...new Set(['.', ...Object.keys(workspaceConfig ? lock?.importers ?? {} : {})])]
        .sort().map(async id => {
            const rootDir = path.resolve(root, id);
            const manifestPath = path.join(rootDir, 'package.json');
            const manifest = id === '.' && root !== project && !await exists(manifestPath)
                ? {} : JSON.parse(await fs.readFile(manifestPath, 'utf8'));
            return { rootDir, manifest };
        }));
    const projects = await readProjects(parsedLock);
    const manifest = projects[0].manifest;
    const importer = discover && project !== root
        ? { rootDir: project, manifest: JSON.parse(await fs.readFile(path.join(project, 'package.json'), 'utf8')) }
        : projects[0];
    if ([...projects, importer].some(({ manifest }) => manifest.dependencies?.vite || manifest.devDependencies?.vite)) {
        // The embedded host cannot load Lightning CSS's Windows native addon.
        // A workspace shares its root override even if Vite is used by a child.
        const selector = 'vite>lightningcss';
        const cssPackage = 'npm:lightningcss-wasm@' + request.lightningCssWasmVersion;
        const overrides = (manifest.pnpm ??= {}).overrides ??= {};
        if (overrides[selector] && overrides[selector] !== cssPackage)
            throw new Error('Embedded Vite requires pnpm.overrides["' + selector + '"] = "' + cssPackage + '".');
        if (overrides[selector] !== cssPackage) {
            overrides[selector] = cssPackage;
            await fs.writeFile(path.join(root, 'package.json'), JSON.stringify(manifest, null, 2) + '\n');
        }
    }
    const workspace = workspaceConfig ? await fs.readFile(workspaceConfig, 'utf8') : '';
    const statePath = path.join(root, 'node_modules', '.jazor-restore-state');
    // An ordinary cache hit needs only the dependency fields and lock bytes.
    const getIdentity = projects => {
        const graph = workspaceConfig ? projects.map(({ rootDir, manifest }) => ({ importer: importerFor(rootDir), manifest }))
        : JSON.stringify({ dependencies: manifest.dependencies, devDependencies: manifest.devDependencies,
            optionalDependencies: manifest.optionalDependencies, overrides: manifest.pnpm?.overrides });
        return hash(JSON.stringify({ graph, workspace: workspaceConfig ? workspace : undefined, engine: request.engineVersion, node: process.version,
        platform: process.platform, arch: process.arch, linker: 'hoisted' }));
    };
    const expected = getIdentity(projects) + '\n' + hash(lock);
    const hasLocal = projects => projects.some(({ manifest }) =>
        Object.values({ ...manifest.dependencies, ...manifest.devDependencies, ...manifest.optionalDependencies })
            .some(value => value.startsWith('file:')));
    if (!discover && !hasLocal(projects) && await exists(statePath) && await fs.readFile(statePath, 'utf8') === expected)
        return JSON.stringify({ operation: 'restore', status: 'skipped', pid: process.pid });

    pnpm ??= loadPnpm(request.bootstrap);
    if (!workspaceConfig && lock) parsedLock = await pnpm.readLockfile({ dir: root });
    const frozen = !discover && parsedLock != null && matchesLock(parsedLock, projects, importerFor, manifest.pnpm?.overrides);
    await fs.rm(statePath, { force: true });
    const config = pnpm.readConfig({ dir: root });
    await fs.mkdir(request.storeDirectory, { recursive: true });
    const result = await pnpm.install({
        dir: discover ? project : root,
        projects: discover ? [importer] : projects,
        registries: Object.fromEntries(config.registries.map(registry => [registry.name, registry.url])),
        authHeaderByUri: config.authHeaderByUri,
        proxyConfig: { httpProxy: config.httpProxy, httpsProxy: config.httpsProxy, noProxy: config.noProxy },
        networkConfig: { ca: config.ca, cert: config.cert, key: config.key, strictSsl: config.strictSsl },
        storeDir: request.storeDirectory,
        cacheDir: path.join(request.storeDirectory, 'cache'),
        nodeLinker: 'hoisted',
        overrides: manifest.pnpm?.overrides,
        nodeVersion: process.versions.node,
        autoInstallPeers: true,
        frozenLockfile: frozen,
        ignoreScripts: true,
        reporter: { appendOnly: true },
    }, undefined, undefined, chunk => process.stderr.write(chunk));
    const installedProjects = discover ? await readProjects(await pnpm.readLockfile({ dir: root })) : projects;
    if (!hasLocal(installedProjects))
        await fs.writeFile(statePath, getIdentity(installedProjects) + '\n' + hash(await fs.readFile(lockPath, 'utf8')));
    return JSON.stringify({ operation: 'restore', status: 'completed', pid: process.pid, frozen, ...result });
};

function loadPnpm(bootstrap) {
    try {
        return createRequire(path.join(bootstrap, 'package.json'))('@pnpm/napi');
    } catch (error) {
        // NodeApi's managed exception does not carry Error.cause.
        throw new Error(inspect(error, { depth: 5 }));
    }
}

function matchesLock(lock, projects, importerFor, overrides) {
    const orderedOverrides = value => JSON.stringify(Object.entries(value ?? {}).sort(([a], [b]) => a < b ? -1 : a > b ? 1 : 0));
    if (orderedOverrides(lock.overrides) !== orderedOverrides(overrides)) return false;
    return projects.every(({ rootDir, manifest }) => {
        const importer = lock.importers?.[importerFor(rootDir)];
        if (!importer) return false;
        for (const group of ['dependencies', 'devDependencies', 'optionalDependencies']) {
            const expected = manifest[group] ?? {};
            const actual = importer[group] ?? {};
            if (Object.keys(expected).length !== Object.keys(actual).length) return false;
            for (const [name, specifier] of Object.entries(expected)) if (actual[name]?.specifier !== specifier) return false;
        }
        return true;
    });
}

async function loadVite(projectDirectory) {
        const requireFromProject = createRequire(path.join(projectDirectory, 'package.json'));
        if (process.platform === 'win32' && process.arch === 'x64' && process.config.variables.node_target_type === 'shared_library') {
            // Rolldown's x64 loader mistakes the MSVC libnode shared_library for MinGW.
            // Use its explicit native-binding contract only while loading rolldown.
            // Replacing this environment's env object avoids writing the OS-wide
            // environment through Node's special process.env setter properties.
            const inheritedEnvironment = process.env;
            process.env = { ...inheritedEnvironment, NAPI_RS_NATIVE_LIBRARY_PATH: requireFromProject.resolve('@rolldown/binding-win32-x64-msvc') };
            try {
                await import(pathToFileURL(requireFromProject.resolve('rolldown')).href);
            } finally {
                process.env = inheritedEnvironment;
            }
        }
        return import(pathToFileURL(requireFromProject.resolve('vite')).href);
}

exports.loadVite = loadVite;

exports.build = async (requestJson) => {
    const { projectDirectory, ssr } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    await vite.build({ root: projectDirectory, configFile: path.join(projectDirectory, ssr ? 'ssr.vite.config.js' : 'vite.config.js') });
    return JSON.stringify({ operation: 'build', ssr, status: 'completed', pid: process.pid });
};

exports.check = async (requestJson) => {
    const { projectDirectory, entryPaths } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    const server = await vite.createServer({
        root: projectDirectory,
        appType: 'custom',
        server: { middlewareMode: true, watch: null, hmr: false, ws: false },
        optimizeDeps: { noDiscovery: true, include: [] }
    });
    try {
        const visited = new Set();
        async function visit(url) {
            if (visited.has(url)) return;
            visited.add(url);
            await server.transformRequest(url);
            const module = await server.moduleGraph.getModuleByUrl(url);
            for (const dependency of module?.importedModules ?? []) {
                // Transform authored local modules without evaluating their
                // initializers. Package exports resolve through Vite; package
                // implementations are checked by the eventual build.
                if (dependency.file && !dependency.file.includes('/node_modules/'))
                    await visit(dependency.url);
            }
        }
        for (const entry of entryPaths)
            await visit('/' + path.relative(projectDirectory, entry).split(path.sep).join('/'));
        return JSON.stringify({ operation: 'check', modules: visited.size, pid: process.pid });
    } finally {
        await server.close();
    }
};

let ownedViteServer;
exports.startVite = async (requestJson) => {
    const { projectDirectory, host, port, base, pluginsModule } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    ownedViteServer = await vite.createServer({ root: projectDirectory, ...(base == null ? {} : { base }),
        server: { host, port, strictPort: true, open: false },
        plugins: pluginsModule ? await require(pluginsModule).plugins(projectDirectory, true) : [] });
    try {
        await ownedViteServer.listen();
        return JSON.stringify({ operation: 'vite', status: 'listening', pid: process.pid, origin: ownedViteServer.resolvedUrls.local[0] });
    } catch (error) {
        await ownedViteServer.close();
        ownedViteServer = undefined;
        throw error;
    }
};

exports.closeVite = async () => {
    if (ownedViteServer) await ownedViteServer.close();
    ownedViteServer = undefined;
    return JSON.stringify({ operation: 'vite', status: 'closed' });
};

exports.verifyVite = async (projectDirectory) => {
    try {
        const vite = await loadVite(projectDirectory);
        const server = await vite.createServer({ root: projectDirectory, configFile: false, server: { port: 0, host: '127.0.0.1' } });
        try {
            await server.listen();
            const response = await fetch(server.resolvedUrls.local[0] + '@vite/client');
            if (!response.ok) throw new Error('Vite client failed: ' + response.status);
        } finally {
            await server.close();
        }
        await vite.build({ root: projectDirectory, configFile: false, build: { outDir: path.join(projectDirectory, 'dist'), rolldownOptions: { input: path.join(projectDirectory, 'entry.js') } } });
        return JSON.stringify({ operation: 'vite', status: 'passed', pid: process.pid, node: process.version });
    } catch (error) {
        throw new Error(inspect(error, { depth: 8 }));
    }
};
