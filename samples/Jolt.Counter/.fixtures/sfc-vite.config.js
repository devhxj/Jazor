import { defineConfig } from 'vite';
import { compileScript, parse } from 'vue/compiler-sfc';
import config from './vite.shared.config.js';

export default defineConfig({
    ...config,
    plugins: [{
        name: 'editable-list-sfc',
        transform(source, id) {
            if (!id.endsWith('/EditableList.vue'))
                return;
            const { descriptor, errors } = parse(source, { filename: id });
            if (errors.length)
                throw new Error(errors.join('\n'));
            // This fixture only uses script setup and an inline template.
            const compiled = compileScript(descriptor, { id: 'editable-list', inlineTemplate: true, isProd: true });
            return { code: compiled.content, map: compiled.map };
        },
    }],
});
