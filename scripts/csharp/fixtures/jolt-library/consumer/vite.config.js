import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueJsx from '@vitejs/plugin-vue-jsx';

export default defineConfig({
    plugins: [vue(), vueJsx(), {
        name: 'verify-single-vue',
        generateBundle() {
            const runtimeRoots = new Set([...this.getModuleIds()]
                .map(id => /^(.*\/node_modules\/@vue\/runtime-core)\/dist\/runtime-core\.esm-bundler\.js$/.exec(id.replaceAll('\\', '/'))?.[1])
                .filter(Boolean));
            if (runtimeRoots.size !== 1) {
                this.error(`Expected one Vue runtime, found ${JSON.stringify([...runtimeRoots])}`);
            }
        }
    }]
});
