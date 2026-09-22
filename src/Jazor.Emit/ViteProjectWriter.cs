namespace Jazor.Emit;

/// <summary>Writes ordinary Vite configuration for the emitted JavaScript project.</summary>
internal static class ViteProjectWriter
{
    public const string Version = "8.3.0";
    public const string ConfigFileName = "vite.config.js";

    public static void Write(string projectRoot)
    {
        // A consumer may replace Vite with another standard tool. Emit supplies a usable
        // default once and leaves an authored config untouched on subsequent builds.
        if (File.Exists(Path.Combine(projectRoot, ConfigFileName)))
            return;

        // Entry exports remain callable by host HTML. Shared and dynamically imported modules
        // are evaluated by the bundler's native ESM graph, with no Jazor module registry.
        ProjectFileWriter.Write(Path.Combine(projectRoot, ConfigFileName), """
            import { existsSync } from 'node:fs';
            import { defineConfig } from 'vite';

            export default defineConfig({
              base: process.env.JAZOR_VITE_BASE || '/jazor/',
              server: { host: '127.0.0.1' },
              build: {
                target: 'esnext',
                outDir: 'dist',
                sourcemap: true,
                rolldownOptions: {
                  input: {
                    bundle: 'entry.js',
                    ...(existsSync('hydration.js') ? { hydration: 'hydration.js' } : {})
                  },
                  preserveEntrySignatures: 'strict',
                  output: { entryFileNames: '[name].js' }
                }
              }
            });
            """ + "\n");
    }
}
